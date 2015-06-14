using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient.localhost;
using System.Collections;
using System.Xml;
using DigitalPlatform.Script;
using System.IO;
using DigitalPlatform;

#pragma warning disable 1591

namespace dp2Circulation
{
    /// <summary>
    /// 负责书目信息输入的控件
    /// 被册登记控件内嵌使用
    /// </summary>
    public partial class BiblioRegisterControl : UserControl
    {
        public event DeleteItemEventHandler DeleteItem = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        // 长操作时的动画
        public Image LoaderImage = null;

        public string BarColor = "";   // 需要在色条上显示的颜色

        public string ImageUrl = "";    // 图像 URL
        public string ImageFileName = "";   // 图像临时文件 TODO: 注意释放的时候删除这个临时文件
        bool CoverImageRequested = false; // 如果为 true ,表示已经请求了异步获取图像，不要重复请求

        public string ServerName = "";   // 服务器名
        // 当前书目记录信息
        public byte[] Timestamp = null;
        public string BiblioRecPath = "";

        public string BiblioBarcode
        {
            get
            {
                return GetBiblioBarcode();
            }
            set
            {
                SetBiblioBarcode(value);
            }
        }

        string GetBiblioBarcode()
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new Func<string>(GetBiblioBarcode));
            }

            return this.textBox_biblioBarcode.Text;
        }

        void SetBiblioBarcode(string strRecPath)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetBiblioBarcode), strRecPath);
                return;
            }

            this.textBox_biblioBarcode.Text = strRecPath;
        }

        public string MarcSyntax = "";
        public string OldMARC = ""; // 修改前的书目记录

        public event EnsureVisibleEventHandler EnsureVisible = null;

#if NO
        public event GetServerTypeEventHandler GetServerType = null;
#endif

        /// <summary>
        /// 通知需要装载下属的册记录
        /// 触发前，要求 BiblioRecPath 有当前书目记录的路径
        /// </summary>
        public event EventHandler LoadEntities = null;

        /// <summary>
        /// 获得配置文件的 XmlDocument 对象
        /// </summary>
        public event GetConfigDomEventHandle GetConfigDom = null;

        public event AsyncGetImageEventHandler AsyncGetImage = null;

        string _displayMode = "summary";   // select/summary/detail。 意思是 选择命中结果 / 书目摘要(错误信息) / 详细书目模板
        
        /// <summary>
        /// 显示模式
        /// </summary>
        public string DisplayMode
        {
            get
            {
                return this._displayMode;
            }
            set
            {
                this._displayMode = value;
                SetDisplayMode(value);
            }
        }

        /// <summary>
        /// 书目记录内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("BiblioChanged")]
        [DefaultValue(false)]
        public bool BiblioChanged
        {
            get
            {
                return this.easyMarcControl1.Changed;
            }
            set
            {
                if (this.easyMarcControl1.Changed != value)
                {
                    this.easyMarcControl1.Changed = value;
                }
            }
        }

        bool m_bEntitiesChanged = false;

        /// <summary>
        /// 册记录内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("EntitiesChanged")]
        [DefaultValue(false)]
        public bool EntitiesChanged
        {
            get
            {
                return this.m_bEntitiesChanged;
            }
            set
            {
                if (this.m_bEntitiesChanged != value)
                {
                    this.m_bEntitiesChanged = value;
                }
            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public bool Changed
        {
            get
            {
                return this.BiblioChanged || this.m_bEntitiesChanged;
            }
#if NOOOOOOOOOOO
            set
            {
                if (this.m_bChanged != value)
                {
                    this.m_bChanged = value;

#if NO
                    if (value == false)
                        ResetLineState();
#endif
                }
            }
#endif
        }

#if NO
        public string SelectBiblioText
        {
            get
            {
                return this.toolStripButton_selectBiblio.Text;
            }
            set
            {
                this.toolStripButton_selectBiblio.Text = value;
            }
        }
#endif
        string _searchState = "";   // "" / searching / error / complete

        public string GetSearchState()
        {
            return _searchState;
        }

        /// <summary>
        /// 设置 “选择书目” 按钮的状态
        /// </summary>
        /// <param name="strState">状态字符串。为 searching / error / 其他</param>
        public void SetSearchState(string strState)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string>(SetSearchState), strState);
                return;
            }

            if (strState == "searching")
            {
                // this.toolStripButton_selectBiblio.DisplayStyle = ToolStripItemDisplayStyle.Image;
                this.label_summary.Image = this.LoaderImage;
                this._searchState = strState;
                return;
            }
            if (strState == "error")
            {
                this.toolStripButton_selectBiblio.DisplayStyle = ToolStripItemDisplayStyle.Image;
                // 把图像设置为 error
                this._searchState = strState;
                return;
            }

            this.toolStripButton_selectBiblio.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectBiblio.Text = strState;
            this.label_summary.Image = null;
            this._searchState = "complete";

            if (strState == "0")
                this.label_summary.Text = "没有命中书目记录，可创建一条新的书目记录";
            else
                this.label_summary.Text = "命中 "+strState+" 条书目记录，请选择";
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BiblioRegisterControl()
        {
            InitializeComponent();

            this.AutoScroll = false;
            this.tableLayoutPanel1.AutoScroll = false;
            this.tableLayoutPanel1.AutoSize = true;

            this.tableLayoutPanel1.SetColumnSpan(this.textBox_biblioBarcode, this.tableLayoutPanel1.ColumnStyles.Count);
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, this.tableLayoutPanel1.ColumnStyles.Count);
            this.tableLayoutPanel1.SetColumnSpan(this.splitter_line, this.tableLayoutPanel1.ColumnStyles.Count);

            SetDisplayMode(this._displayMode);

            CreateBrowseColumns();

            this.dpTable_browseLines.ImageList = this.imageList_progress;
        }

        // 创建浏览栏目标题
        void CreateBrowseColumns()
        {
            if (this.dpTable_browseLines.Columns.Count > 2)
                return;

            List<string> columns = new List<string>() {"书名", "作者", "出版者", "出版日期" };
            foreach (string s in columns)
            {
                DpColumn column = new DpColumn();
                column.Text = s;
                column.Width = 120;
                this.dpTable_browseLines.Columns.Add(column);
            }
        }

        public void SetMarc(string strMarc)
        {
            this.easyMarcControl1.SetMarc(strMarc);
        }

        public string GetMarc()
        {
            return this.easyMarcControl1.GetMarc();
        }

        static void EnableButton(ToolStripButton button, bool bEnable)
        {
            button.Enabled = bEnable;
            button.Checked = !bEnable;
        }

        void SetColumnStyle(int nColumn, ColumnStyle style)
        {
            // 确保足够
            while (this.tableLayoutPanel1.ColumnStyles.Count <= nColumn)
            {
                this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            }

            Debug.Assert(nColumn < this.tableLayoutPanel1.ColumnStyles.Count, "");
            this.tableLayoutPanel1.ColumnStyles[nColumn] = style;
        }

        /// <summary>
        /// 设置显示模式
        /// </summary>
        /// <param name="strMode">显示模式。select/summary/detail</param>
        public void SetDisplayMode(string strMode)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string>(SetDisplayMode), strMode);
                return;
            }

            if (strMode == "detail")
            {
                this.flowLayoutPanel1.Visible = true;

                this.easyMarcControl1.Visible = true;
                this.label_summary.Visible = false;
                // this.pictureBox1.Visible = false;
                this.pictureBox1.Visible = true;
                this.dpTable_browseLines.Visible = false;

                // this.tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 0);
                SetColumnStyle(0, new ColumnStyle(SizeType.AutoSize));

                SetColumnStyle(2, new ColumnStyle(SizeType.Percent, 100));
                SetColumnStyle(3, new ColumnStyle(SizeType.Absolute, 0));
                SetColumnStyle(4, new ColumnStyle(SizeType.Absolute, 0));

                // 复原图像
                if (string.IsNullOrEmpty(this.ImageUrl) == false)
                {
                    // this.pictureBox1.LoadAsync(this.ImageUrl);

                    // DpRow row = this.dpTable_browseLines.SelectedRows[0];

                    // 设置封面图像
                    // 按照这样的次序：1) 如果是 http: 直接设置; 2) 如果有本地文件，直接设置; 3) 从服务器获取，然后异步设置
                    SetCoverImage(this.ImageUrl,
                        this.ImageFileName,
                        this.BiblioRecPath,
                        null);
                }
                else
                    this.pictureBox1.Image = null;

                EnableButton(this.toolStripButton_selectBiblio , true);
                EnableButton(this.toolStripButton_viewDetail , false);
                EnableButton(this.toolStripButton_viewSummary , true);
            }
            else if (strMode == "summary")
            {
                this.flowLayoutPanel1.Visible = true;

                this.easyMarcControl1.Visible = false;
                this.label_summary.Visible = true;
                this.pictureBox1.Visible = true;
                this.dpTable_browseLines.Visible = false;

                // column 4 max
                SetColumnStyle(0,new ColumnStyle(SizeType.AutoSize));

                SetColumnStyle(2, new ColumnStyle(SizeType.Absolute, 0));
                SetColumnStyle(3, new ColumnStyle(SizeType.Percent, 100));
                SetColumnStyle(4, new ColumnStyle(SizeType.Absolute, 0));

                EnableButton(this.toolStripButton_selectBiblio , true);
                EnableButton(this.toolStripButton_viewDetail , true);
                EnableButton(this.toolStripButton_viewSummary , false);

            }
            else
            {
                Debug.Assert(strMode == "select", "");

                this.flowLayoutPanel1.Visible = false;

                this.easyMarcControl1.Visible = false;
                this.label_summary.Visible = false;
                // this.pictureBox1.Visible = false;
                this.pictureBox1.Visible = true;
                this.dpTable_browseLines.Visible = true;

                // column 4 max
                // this.tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 0);
                SetColumnStyle(0, new ColumnStyle(SizeType.AutoSize));

                SetColumnStyle(2, new ColumnStyle(SizeType.Absolute, 0));
                SetColumnStyle(3, new ColumnStyle(SizeType.Absolute, 0));
                SetColumnStyle(4, new ColumnStyle(SizeType.Percent, 100));

                // this.dpTable_browseLines.Focus();

                // 模拟一次 selection changed
                dpTable_browseLines_SelectionChanged(this, new EventArgs());

                EnableButton(this.toolStripButton_selectBiblio , false);
                EnableButton(this.toolStripButton_viewDetail , true);
                EnableButton(this.toolStripButton_viewSummary, true);
            }
        }

#if NO
        /// <summary>
        /// 设置内容宽度
        /// </summary>
        /// <param name="nWidth"></param>
        public void SetWidth(int nWidth)
        {
            this.Width = nWidth;
            this.tableLayoutPanel1.Width = nWidth;
            this.flowLayoutPanel1.Width = nWidth;

            // this.tableLayoutPanel1.PerformLayout();

        }
#endif

        public const int TYPE_ERROR = 2;
        public const int TYPE_INFO = 3;

#if NO
        // 加入一个浏览行
        public void AddBiblioBrowseLine(string strText,
            int nType)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string, int>(AddBiblioBrowseLine),
                    strText,
                    nType);
                return;
            }

            DpRow row = new DpRow();

            DpCell cell = new DpCell();
            cell.Text = (this.dpTable_browseLines.Rows.Count + 1).ToString();

            {
                cell.ImageIndex = nType;
                if (nType == TYPE_ERROR)
                    cell.BackColor = Color.Red;
                else if (nType == TYPE_INFO)
                    cell.BackColor = Color.Yellow;
            }
            row.Add(cell);

            cell = new DpCell();
            cell.Text = strText;
            row.Add(cell);

            row.Tag = null;
            this.dpTable_browseLines.Rows.Add(row);
        }
#endif

        // 加入一个浏览行
        public void AddBiblioBrowseLine(
            int nType,
            string strBiblioRecPath,
            string strBrowseText,
            RegisterBiblioInfo info)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<int, string, string, RegisterBiblioInfo>(AddBiblioBrowseLine),
                    nType,
                    strBiblioRecPath,
                    strBrowseText,
                    info);
                return;
            }

            List<string> columns = StringUtil.SplitList(strBrowseText, '\t');
            DpRow row = new DpRow();

            DpCell cell = new DpCell();
            cell.Text = (this.dpTable_browseLines.Rows.Count + 1).ToString();
            {
                cell.ImageIndex = nType;
                if (nType == TYPE_ERROR)
                    cell.BackColor = Color.Red;
                else if (nType == TYPE_INFO)
                    cell.BackColor = Color.Yellow;
            }
            row.Add(cell);

            cell = new DpCell();
            cell.Text = strBiblioRecPath;
            row.Add(cell);

            foreach (string s in columns)
            {
                cell = new DpCell();
                cell.Text = s;
                row.Add(cell);
            }

            row.Tag = info;
            this.dpTable_browseLines.Rows.Add(row);

            PrepareCoverImage(row);
        }

        public void ClearList()
        {
            if (this.dpTable_browseLines == null)
                return;

            foreach(DpRow row in this.dpTable_browseLines.Rows)
            {
                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info != null)
                {
                    if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                    {
                        try
                        {
                            File.Delete(info.CoverImageFileName);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        // 
        /// <summary>
        /// (如果必要，将)册信息部分显示为空
        /// </summary>
        /// <param name="strStyle">not_initial/none</param>
        public void TrySetBlank(string strStyle)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.Invoke(new Action<string>(TrySetBlank), strStyle);
                return;
            }

            Label label = null;
            bool bEntity = false;   // 具有至少一个册控件

            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is Label)
                    label = control as Label;
                else
                    bEntity = true;

                if (label != null && bEntity == true)
                    break;
            }

            // 如果有册控件了，就不要加入 label 了

            if (bEntity == false)
            {
                if (label == null)
                {
                    label = new Label();
                    string strFontName = "";
                    Font ref_font = GuiUtil.GetDefaultFont();
                    if (ref_font != null)
                        strFontName = ref_font.Name;
                    else
                        strFontName = this.Font.Name;

                    label.Font = new Font(strFontName, this.Font.Size * 2, FontStyle.Bold);
                    label.ForeColor = SystemColors.GrayText;


                    label.AutoSize = true;
                    label.Margin = new Padding(8, 8, 8, 8);
                    this.flowLayoutPanel1.Controls.Add(label);
                }

                if (strStyle == "not_initial")
                    label.Text = "册信息尚未初始化";
                else if (strStyle == "none")
                    label.Text = "无册信息";
                else
                {
                    Debug.Assert(false);
                    label.Text = "册信息尚未初始化";
                }

                this.AdjustFlowLayoutHeight();
            }
        }

        // 清除临时标签
        void ClearBlank()
        {
            if (this.flowLayoutPanel1.Controls.Count == 1
                && this.flowLayoutPanel1.Controls[0] is Label)
                this.flowLayoutPanel1.Controls.Clear();
        }

        delegate int Delegate_NewEntity(
            string strRecPath,
            byte[] timestamp,
            string strXml,
            bool ScrollIntoView,
            out string strError);

        // 添加一个新的册对象
        // parameters:
        //      strRecPath  记录路径
        public int NewEntity(string strRecPath,
            byte [] timestamp,
            string strXml,
            bool ScrollIntoView,
            out string strError)
        {
            strError = "";

            if (this.InvokeRequired)
            {
                Delegate_NewEntity d = new Delegate_NewEntity(NewEntity);
                object[] args = new object[5];
                args[0] = strRecPath;
                args[1] = timestamp;
                args[2] = strXml;
                args[3] = ScrollIntoView;
                args[4] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strError = (string)args[4];
                return result;
            }

            EntityEditControl control = new EntityEditControl();
            control.DisplayMode = "simple_register";
            control.Width = 120;
            control.AutoScroll = false;
            control.AutoSize = true;
            control.Font = this.Font;
            control.BackColor = Color.Transparent;

            // control.ErrorInfo = "测试文字 asdfasdf a asd fa daf a df af asdf asdf adf asdf asdf asf asdf asdf ---- ";

            if (string.IsNullOrEmpty(strXml) == false)
            {
                int nRet = control.SetData(strXml, strRecPath, timestamp, out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                control.Initializing = false;
                // control.Barcode = strItemBarcode;
                if (string.IsNullOrEmpty(control.RefID) == true)
                    control.RefID = Guid.NewGuid().ToString();
            }

            if (timestamp == null)
            {
                control.CreateState = ItemDisplayState.New;
                control.Changed = true;
                this.EntitiesChanged = true;    // 让外界能感知到含有新册事项
            }

            control.PaintContent += new PaintEventHandler(control_PaintContent);
            control.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(control_ContentChanged);
            control.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(control_GetValueTable);
            control.AppendMenu += new ApendMenuEventHandler(control_AppendMenu);

            ClearBlank();

            this.flowLayoutPanel1.Controls.Add(control);

            // this.flowLayoutPanel1.PerformLayout();
            // this.tableLayoutPanel1.PerformLayout();

            this.AdjustFlowLayoutHeight();

            if (ScrollIntoView)
            {
                this.flowLayoutPanel1.ScrollControlIntoView(control);
                if (this.EnsureVisible != null)
                {
                    EnsureVisibleEventArgs e1 = new EnsureVisibleEventArgs();
                    e1.Control = control;
                    e1.Rect = new Rectangle(control.Location, control.Size);
                    e1.Rect.X += this.flowLayoutPanel1.Location.X;
                    e1.Rect.Y += this.flowLayoutPanel1.Location.Y;
                    this.EnsureVisible(this, e1);
                }
            }

            // this.BeginInvoke(new Action<Control>(EnsureVisible), control);

            return 0;
        }

        void control_AppendMenu(object sender, AppendMenuEventArgs e)
        {

            MenuItem menuItem = null;

            menuItem = new MenuItem("删除册(&D)");
            menuItem.Tag = sender;
            menuItem.Click += new System.EventHandler(this.menu_deleteItem_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);
        }

        void menu_deleteItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            EntityEditControl control = menuItem.Tag as EntityEditControl;

            DialogResult result = MessageBox.Show(this,
"确实要删除册记录?",
"BiblioRegisterControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            if (string.IsNullOrEmpty(control.RecPath) == false)
            {
                if (this.DeleteItem != null)
                {
                    DeleteItemEventArgs e1 = new DeleteItemEventArgs();
                    e1.Control = control;
                    this.DeleteItem(this, e1);
                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        MessageBox.Show(this, e1.ErrorInfo);
                        return;
                    }
                }
            }

            this.flowLayoutPanel1.Controls.Remove(control);
        }

        // 删除一个册记录控件
        public void RemoveEditControl(EntityEditControl control)
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new Action<EntityEditControl>(RemoveEditControl), control);
                return;
            }

            this.flowLayoutPanel1.Controls.Remove(control);
        }

        void control_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(this, e);    // sender wei
        }

        void control_ContentChanged(object sender, DigitalPlatform.ContentChangedEventArgs e)
        {
            this.m_bEntitiesChanged = true;
        }

        // 获得各个部分的 Rectangle。假定控件左上角 0,0
        // all / biblio / items
        public Rectangle GetRect(string strPart = "all")
        {
            if (strPart == "all")
            {
                Rectangle rect = new Rectangle(0,
                    0,
                    this.tableLayoutPanel1.Width,
                    this.flowLayoutPanel1.Location.Y + this.flowLayoutPanel1.Size.Height);
                return rect;
            }
            if (strPart == "biblio")
            {
                Rectangle rect = new Rectangle(0, 
                    0,
                    this.tableLayoutPanel1.Width,
                    this.flowLayoutPanel1.Location.Y);
                return rect;
            }
            if (strPart == "items")
            {
                Rectangle rect = new Rectangle(0,
                    this.flowLayoutPanel1.Location.Y,
                    this.tableLayoutPanel1.Width,
                    this.flowLayoutPanel1.Size.Height);
                return rect;
            }
            return new Rectangle();
        }

#if NO
        void EnsureVisible(Control control)
        {
            this.flowLayoutPanel1.ScrollControlIntoView(control);
        }
#endif

        // 探测实体界面是否发生过修改?
        // parameters:
        //      strStyle    探测哪些部分? all/normal
        //                  normal 表示只清除那些从数据库中调出的记录
        bool HasEntitiesChanged(string strStyle = "all")
        {
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is EntityEditControl)
                {
                    EntityEditControl edit = control as EntityEditControl;

                    if (strStyle == "normal")
                    {
                        if (edit.CreateState != ItemDisplayState.Normal)
                            continue;
                    }

                    if (edit.Changed == true)
                        return true;
                }
            }

            return false;
        }

        // parameters:
        //      strStyle    清除哪些部分? all/normal
        //                  normal 表示只清除那些从数据库中调出的记录
        public void ClearEntityEditControls(string strStyle = "all")
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.Invoke(new Action<string>(ClearEntityEditControls), strStyle);
                return;
            }

            List<Control> controls = new List<Control>();
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is EntityEditControl)
                {
                    EntityEditControl edit = control as EntityEditControl;

                    if (strStyle == "normal")
                    {
                        if (edit.CreateState != ItemDisplayState.Normal)
                            continue;
                    }

                    controls.Add(edit);
                    edit.PaintContent -= new PaintEventHandler(control_PaintContent);
                    edit.ContentChanged -= new DigitalPlatform.ContentChangedEventHandler(control_ContentChanged);
                }
            }

            if (strStyle != "normal")
                this.flowLayoutPanel1.Controls.Clear();
            else
            {
                foreach (Control control in controls)
                {
                    this.flowLayoutPanel1.Controls.Remove(control);
                }
            }

            this.AdjustFlowLayoutHeight();
        }

        void control_PaintContent(object sender, PaintEventArgs e)
        {
            EntityEditControl control = sender as EntityEditControl;

            int index = this.flowLayoutPanel1.Controls.IndexOf(control);
            string strText = (index + 1).ToString();
            using (Brush brush = new SolidBrush(Color.FromArgb(220,220,220)))
            {
                if (control.CreateState == ItemDisplayState.New
                    || control.CreateState == ItemDisplayState.Deleted)
                {
                    Color state_color = Color.Transparent;
                    if (control.CreateState == ItemDisplayState.New)
                        state_color = Color.FromArgb(0, 200, 0);
                    else if (control.CreateState == ItemDisplayState.Deleted)
                        state_color = Color.FromArgb(200, 200, 200);

                    using (Brush brushState = new SolidBrush(state_color))
                    {
                        int nWidth = 80;
                        Point[] points = new Point[3];
                        points[0] = new Point(0, 0);
                        points[1] = new Point(0, nWidth);
                        points[2] = new Point(nWidth, 0);
                        e.Graphics.FillPolygon(brushState, points);
                    }
                }

                using (Font font = new Font(this.Font.Name, control.Height / 4, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    SizeF size = e.Graphics.MeasureString(strText, font);
                    // PointF start = new PointF(control.Width / 2, control.Height / 2 - size.Height / 2);
                    PointF start = new PointF(8, 16);

                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Near;

                    e.Graphics.DrawString(strText, font, brush, start, format);
                }


            }
        }

        public void AdjustFlowLayoutHeight()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(AdjustFlowLayoutHeight));
                return;
            }

            Size size = this.flowLayoutPanel1.GetPreferredSize(this.ClientSize);
#if NO
            Control control = null;
            if (this.flowLayoutPanel1.Controls.Count > 0)
                control = this.flowLayoutPanel1.Controls[0];
#endif
            int nRow = this.tableLayoutPanel1.GetCellPosition(this.flowLayoutPanel1).Row;
            this.tableLayoutPanel1.RowStyles[nRow] = new RowStyle(SizeType.Absolute,
                size.Height + this.flowLayoutPanel1.Margin.Vertical
                // size.Height + (control != null ? control.Margin.Vertical : 0)
                );
        }

        /// <summary>
        /// 获取或设置书目摘要字符串
        /// </summary>
        public string Summary
        {
            get
            {
                return this.label_summary.Text;
            }
            set
            {
                // this.label_summary.Text = value;
                SetBiblioSummary(value);
            }
        }

        void SetBiblioSummary(string strText)
        {
            if (this.label_summary.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string>(SetBiblioSummary), strText);
                return;
            }

            this.label_summary.Text = strText;
        }

        private void BiblioRegisterControl_SizeChanged(object sender, EventArgs e)
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.tableLayoutPanel1.Width = this.ClientSize.Width;
            this.tableLayoutPanel1.PerformLayout();

            base.OnSizeChanged(e);
        }

#if NO
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size =  base.GetPreferredSize(proposedSize);
            size.Width = 100;
            return size;
        }
#endif

        int _nSplitterStart = 0;

        private void splitter_line_MouseUp(object sender, MouseEventArgs e)
        {
            int nDelta = e.Y - _nSplitterStart;
            this.ChangeMainRowHeight(nDelta);
        }

        private void splitter_line_MouseDown(object sender, MouseEventArgs e)
        {
            _nSplitterStart = e.Y;

        }

        void ChangeMainRowHeight(int nDelta)
        {
            int nRow = this.tableLayoutPanel1.GetCellPosition(this.pictureBox1).Row;

            int nOldHeight = (int)this.tableLayoutPanel1.RowStyles[nRow].Height;
            int nNewHeight = nOldHeight + nDelta;
            if (nNewHeight < 50)
                nNewHeight = 50;
            this.tableLayoutPanel1.RowStyles[nRow] = new RowStyle(SizeType.Absolute, nNewHeight);
        }

        private void dpTable_browseLines_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_browseLines.SelectedRows.Count == 0)
            {
                strError = "请选择要装入的一行";
                goto ERROR1;
            }

            int nRet = SelectBiblio(this.dpTable_browseLines.SelectedRowIndices[0], out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
#if NO
            if (this.BiblioChanged == true)
            {
                DialogResult result = MessageBox.Show(this,
"当前书目记录修改后尚未保存。如果此时装入新记录内容，先前的修改将会丢失。\r\n\r\n是否装入新记录?",
"BiblioRegisterControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            // TODO: 警告那些从原书目记录下属装入的册记录修改。但新增的册不会被清除

            // this.ClearEntityEditControls("normal");

            BiblioInfo info = this.dpTable_browseLines.SelectedRows[0].Tag as BiblioInfo;
            this.OldMARC = info.OldXml;
            this.Timestamp = info.Timestamp;

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(info.RecPath, "@", out strPath, out strServerName);
            this.ServerName = strServerName;
            this.BiblioRecPath = strPath;

            this.easyMarcControl1.SetMarc(info.OldXml);
            Debug.Assert(this.BiblioChanged == false, "");

            this.SetDisplayMode("detail");

            if (this.LoadEntities != null)
                this.LoadEntities(this, new EventArgs());

            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // 获得第一个真实记录行的 index
        public int GetFirstRecordIndex()
        {
            if (this.InvokeRequired)
            {
                return (int)this.Invoke(new Func<int>(GetFirstRecordIndex));
            }

            for (int i = 0; i < this.dpTable_browseLines.Rows.Count; i++ )
            {
                DpRow row = this.dpTable_browseLines.Rows[i];

                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info != null)
                    return i;
            }
            Debug.Assert(false, "");
            return -1;
        }

        delegate int Delegate_SelectBiblio(int index,
            out string strError);

        // 从列表中选择一条书目记录装入编辑模板
        public int SelectBiblio(int index, 
            out string strError)
        {
            strError = "";

            if (this.InvokeRequired)
            {
                Delegate_SelectBiblio d = new Delegate_SelectBiblio(SelectBiblio);
                object[] args = new object[2];
                args[0] = index;
                args[1] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strError = (string)args[1];
                return result;
            }

            if (index >= this.dpTable_browseLines.Rows.Count)
            {
                strError = "index 超过范围";
                goto ERROR1;
            }

            if (this.BiblioChanged == true)
            {
                DialogResult result = MessageBox.Show(this,
"当前书目记录修改后尚未保存。如果此时装入新记录内容，先前的修改将会丢失。\r\n\r\n是否装入新记录?",
"BiblioRegisterControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            // 警告那些从原书目记录下属装入的册记录修改。但新增的册不会被清除
            if (HasEntitiesChanged("normal") == true)
            {
                DialogResult result = MessageBox.Show(this,
"当前有册记录修改后尚未保存。如果此时装入新记录内容，先前的修改将会丢失。\r\n\r\n是否装入新记录?",
"BiblioRegisterControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            // this.ClearEntityEditControls("normal");

            DpRow row = this.dpTable_browseLines.Rows[index];
            RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;

            if (info == null)
            {
                strError = "这是提示信息行";
                return -1;
#if NO
                if (row[0].ImageIndex == TYPE_ERROR)
                {
                    strError = "这是提示信息行";
                    return -1;
                }
                // 装入一条空白书目记录
                info = new RegisterBiblioInfo();
                info.MarcSyntax = "unimarc";
                MarcRecord record = new MarcRecord();
                record.add(new MarcField('$', "010  $a" + this.BiblioBarcode));
                record.add(new MarcField('$', "2001 $a$f"));
                record.add(new MarcField('$', "210  $a$c$d"));
                record.add(new MarcField('$', "215  $a$d"));
                record.add(new MarcField('$', "690  $a"));
                record.add(new MarcField('$', "701  $a"));
                info.OldXml = record.Text;
#endif
            }

            this.OldMARC = info.OldXml;
            this.Timestamp = info.Timestamp;

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(info.RecPath, "@", out strPath, out strServerName);
            this.ServerName = strServerName;
            this.BiblioRecPath = strPath;

            this.MarcSyntax = info.MarcSyntax;

            this.easyMarcControl1.SetMarc(info.OldXml);
            Debug.Assert(this.BiblioChanged == false, "");

            // 设置封面图像
            // SetCoverImage(row);
            string strMARC = info.OldXml;
            if (string.IsNullOrEmpty(strMARC) == false)
            {
                this.ImageUrl = ScriptUtil.GetCoverImageUrl(strMARC);
                this.CoverImageRequested = false;
            }

            this.SetDisplayMode("detail");

            if (this.LoadEntities != null)
                this.LoadEntities(this, new EventArgs());

            return 1;
        ERROR1:
            // MessageBox.Show(this, strError);
            return -1;
        }

        private void toolStripButton_selectBiblio_Click(object sender, EventArgs e)
        {
            this.SetDisplayMode("select");
            this.Focus();
        }

        private void easyMarcControl1_GetConfigDom(object sender, 
            DigitalPlatform.Marc.GetConfigDomEventArgs e)
        {
            if (GetConfigDom != null)
            {
#if NO
                string strServerType = this.DoGetServerType(this.ServerName);
                if (strServerType == "dp2library")
                    e.Path = Global.GetDbName(this.BiblioRecPath) + "/cfgs/" + e.Path + "@" + this.ServerName;
                else if (strServerType == "amazon")
                    e.Path = this.BiblioRecPath + "@!" + strServerType;
#endif
                if (string.IsNullOrEmpty(this.BiblioRecPath) == true
                    && string.IsNullOrEmpty(this.ServerName) == true)
                    e.Path = e.Path + "@!unknown";
                else
                {
                    // e.Path = Global.GetDbName(this.BiblioRecPath) + "/cfgs/" + e.Path + "@" + this.ServerName;
                    e.Path = e.Path + "@" + this.ServerName;
                }

                GetConfigDom(this, e);
            }
        }

#if NO
        string DoGetServerType(string strServerName)
        {
            if (this.GetServerType == null)
                return "";
            GetServerTypeEventArgs e = new GetServerTypeEventArgs();
            e.ServerName = strServerName;
            this.GetServerType(this, e);
            return e.ServerType;
        }
#endif

        private void easyMarcControl1_TextChanged(object sender, EventArgs e)
        {
            // this.BiblioChanged = true;
        }

        public int ReplaceEntityMacro(ref string strXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            int nRet =  ReplaceEntityMacro(dom,
                out strError);
            if (nRet == -1)
                return -1;
            strXml = dom.DocumentElement.OuterXml;
            return nRet;
        }

        // 兑现实体记录中的宏
        // return:
        //      -1  出错
        //      0   没有发生修改
        //      1   发生过修改
        public int ReplaceEntityMacro(XmlDocument dom,
            out string strError)
        {
            strError = "";
            bool bChanged = false;

            // 遍历所有一级元素的内容
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = nodes[i].InnerText;
                if (strText.Length > 0 && strText[0] == '@')
                {
                    // 兑现宏
                    string strResult = DoGetMacroValue(strText);
                    if (strResult != strText)
                    {
                        nodes[i].InnerText = strResult;
                        bChanged = true;
                    }
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 兑现 @... 宏值。
        // 如果无法解释宏，则原样返回宏名
        string DoGetMacroValue(string strMacroName)
        {
            if (string.IsNullOrEmpty(this.MarcSyntax) == true)
                return strMacroName;

            if (this.MarcSyntax == "unimarc")
            {
                string strMARC = GetMarc();
                if (string.IsNullOrEmpty(strMARC) == true)
                    return strMacroName;

                MarcRecord record = new MarcRecord(strMARC);

                if (strMacroName == "@price")
                {
                    return record.select("field[@name='010']/subfield[@name='d']").FirstContent;
                }
            }

            if (this.MarcSyntax == "usmarc")
            {
            }

            return strMacroName;
        }

        delegate int Delegate_SetEditData(EntityEditControl edit,
            string strXml,
            string strRecPath,
            byte[] timestamp,
            out string strError);

        int SetEditData(EntityEditControl edit,
            string strXml,
            string strRecPath,
            byte[] timestamp,
            out string strError)
        {
            if (this.InvokeRequired)
            {
                Delegate_SetEditData d = new Delegate_SetEditData(SetEditData);
                object[] args = new object[5];
                args[0] = edit;
                args[1] = strXml;
                args[2] = strRecPath;
                args[3] = timestamp;
                args[4] = "";
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strError = (string)args[4];
                return result;
            }

            return edit.SetData(strXml, strRecPath, timestamp, out strError);
        }

        delegate int Delegate_GetEditData(EntityEditControl edit,
            string strBiblioRecPath,
            out string strXml,
            out string strError);

        int GetEditData(EntityEditControl edit, 
            string strBiblioRecPath,
            out string strXml,
            out string strError)
        {
            if (this.InvokeRequired)
            {
                Delegate_GetEditData d = new Delegate_GetEditData(GetEditData);
                object[] args = new object[4];
                args[0] = edit;
                args[1] = strBiblioRecPath;
                args[2] = "";
                args[3] = "";
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strXml = (string)args[2];
                strError = (string)args[3];
                return result;
            }

            string strParentID = Global.GetRecordID(strBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true
                || StringUtil.IsNumber(strParentID) == false)
            {
                strXml = "";
                strError = "书目记录路径 '"+strBiblioRecPath+"' 中的记录 ID 部分格式错误";
                return -1;
            }
            edit.ParentId = strParentID;
            return edit.GetData(true, out strXml, out strError);
        }

        // 构造用于保存的实体信息数组
        // parameters:
        //      strAction   change / delete。其中 change 表示新增和修改
        public int BuildSaveEntities(
            string strAction,
            List<EntityEditControl> controls,
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            if (controls == null || controls.Count == 0)
            {
                controls = new List<EntityEditControl>();

                foreach (Control control in this.flowLayoutPanel1.Controls)
                {
                    if (!(control is EntityEditControl))
                        continue;

                    EntityEditControl edit = control as EntityEditControl;
                    if (strAction == "change")
                    {
                        if (edit.Changed == false)
                            continue;
                    }

                    controls.Add(edit);
                }
            }

            List<EntityInfo> entityArray = new List<EntityInfo>();

            foreach (EntityEditControl edit in controls)
            {
                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(edit.RefID) == true)
                {
                        edit.RefID = BookItem.GenRefID();
                }

                info.RefID = edit.RefID;

                if (strAction == "change")
                {
                    string strXml = "";
                    // nRet = edit.GetData(true, out strXml, out strError);
                    nRet = GetEditData(edit, this.BiblioRecPath, out strXml, out strError);
                    if (nRet == -1)
                        return -1;

                    // 试探替换宏
                    // TODO: 如果还有宏没有替换完，应该警告提示
                    nRet = ReplaceEntityMacro(ref strXml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        nRet = SetEditData(edit, strXml, edit.RecPath, edit.Timestamp, out strError);
                        if (nRet == -1)
                        {
                            strError = "重新设置 Data 时出错: " + strError;
                            return -1;
                        }
                    }

                    if (string.IsNullOrEmpty(edit.RecPath) == true)
                    {
                        info.Action = "new";
                        info.NewRecPath = "";
                        info.NewRecord = strXml;
                        info.NewTimestamp = null;
                    }
                    else
                    {
                        info.Action = "change";
                        info.OldRecPath = edit.RecPath;
                        info.NewRecPath = edit.RecPath;

                        info.NewRecord = strXml;
                        info.NewTimestamp = null;

                        info.OldRecord = edit.OldRecord;
                        info.OldTimestamp = edit.Timestamp;
                    }
                }
                else if (strAction == "delete")
                {
                    if (string.IsNullOrEmpty(edit.RecPath) == true)
                    {
                        strError = "没有路径的记录无法删除";
                        return -1;
                    }

                    info.Action = "delete";
                    info.OldRecPath = edit.RecPath;
                    info.NewRecPath = edit.RecPath;

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = edit.OldRecord;
                    info.OldTimestamp = edit.Timestamp;
                }

                entityArray.Add(info);
            }

            // 复制到目标
            entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            return 0;
        }

        // 根据参考 ID 找到一个 EntityEditControl
        EntityEditControl GetEditControl(string strRefID)
        {
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (!(control is EntityEditControl))
                    continue;
                EntityEditControl edit = control as EntityEditControl;
                if (GetEditRefID(edit) == strRefID)
                    return edit;
            }

            return null;
        }

        // 构造事项称呼
        static string GetEntitySummary(EntityEditControl control)
        {
            if (control.InvokeRequired)
            {
                return (string)control.Invoke(new Func<EntityEditControl, string>(GetEntitySummary), control);
            }

            string strBarcode = control.Barcode;

            if (String.IsNullOrEmpty(strBarcode) == false)
                return "册条码号为 '" + strBarcode + "' 的事项";

            string strRegisterNo = control.RegisterNo;

            if (String.IsNullOrEmpty(strRegisterNo) == false)
                return "登录号为 '" + strRegisterNo + "' 的事项";

            string strRecPath = control.RecPath;

            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            string strRefID = control.RefID;
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }

        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”册事项（内存和视觉上）
        // return:
        //      false   没有警告
        //      true    出现警告
        public bool RefreshOperResult(EntityInfo[] errorinfos,
            out string strWarning)
        {
            int nRet = 0;

            strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            bool bHeightChanged = false;

            foreach (EntityInfo info in errorinfos)
            {

                string strError = "";

                if (String.IsNullOrEmpty(info.RefID) == true)
                {
                    strWarning += " 服务器返回的EntityInfo结构中RefID为空";
                    return true;
                }

                 EntityEditControl control = GetEditControl(info.RefID);
                 if (String.IsNullOrEmpty(info.RefID) == true)
                 {
                     // strWarning += " 定位错误信息 '" + errorinfos[i].ErrorInfo + "' 所在行的过程中发生错误:" + strError;
                     strWarning += " 服务器返回的EntityInfo结构中RefID '" + info.RefID + "' 找不到匹配的控件";
                     return true;
                 }

                string strLocationSummary = GetEntitySummary(control);

                // 正常信息处理
                if (info.ErrorCode == ErrorCodeValue.NoError)
                {
                    if (info.Action == "new"
                        || info.Action == "change"
                        || info.Action == "move")
                    {
                        control.OldRecord = info.NewRecord;
#if NO
                        control.SetData(info.NewRecord,
                            info.NewRecPath,
                            info.NewTimestamp,
                            out strError);
#endif
                        nRet = SetEditData(control,
                            info.NewRecord,
                            info.NewRecPath,
                            info.NewTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            // MessageBox.Show(ForegroundWindow.Instance, strError);
                            strWarning += " " + strError;
                        }

                        // bookitem.ItemDisplayState = ItemDisplayState.Normal;

                    }

#if NO
                    // 对于保存后变得不再属于本种的，要在listview中消除
                    if (String.IsNullOrEmpty(control.RecPath) == false)
                    {
                        string strTempItemDbName = Global.GetDbName(control.RecPath);
                        string strTempBiblioDbName = "";

                        strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName("item", strTempItemDbName);
                        if (string.IsNullOrEmpty(strTempBiblioDbName) == true)
                        {
                            strWarning += " " + this.ItemType + "类型的数据库名 '" + strTempItemDbName + "' 没有找到对应的书目库名";
                            //// MessageBox.Show(ForegroundWindow.Instance, this.ItemType + "类型的数据库名 '" + strTempItemDbName + "' 没有找到对应的书目库名");
                            return true;
                        }
                        string strTempBiblioRecPath = strTempBiblioDbName + "/" + bookitem.Parent;

                        if (strTempBiblioRecPath != this.BiblioRecPath)
                        {
                            this.Items.PhysicalDeleteItem(bookitem);
                            continue;
                        }
                    }
#endif

                    // control.ErrorInfo = "";
                    if (SetEditErrorInfo(control, "") == true)
                        bHeightChanged = true;

                    control.Changed = false;
                    control.CreateState = ItemDisplayState.Normal;

                    continue;
                }

                // 报错处理
                // control.ErrorInfo = info.ErrorInfo;
                if (SetEditErrorInfo(control, info.ErrorInfo) == true)
                    bHeightChanged = true;
                strWarning += strLocationSummary + "在提交保存过程中发生错误 -- " + info.ErrorInfo + "\r\n";
            }

#if NO
            // 最后把没有报错的，那些成功删除事项，都从内存和视觉上抹除
            for (int i = 0; i < this.Items.Count; i++)
            {
                BookItemBase bookitem = this.Items[i];
                if (bookitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    if (bookitem.ErrorInfo == "")
                    {
                        this.Items.PhysicalDeleteItem(bookitem);
                        i--;    // 2007/4/12 
                    }
                }
            }
#endif
            if (bHeightChanged == true)
                AdjustFlowLayoutHeight();

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改后重新提交保存";
                //// MessageBox.Show(ForegroundWindow.Instance, strWarning);
                return true;
            }

            return false;
        }
        
        // return:
        //      错误信息字符串是否确实被改变
        public static bool SetEditErrorInfo(EntityEditControl edit,
            string strErrorInfo)
        {
            if (edit.InvokeRequired)
            {
                return (bool)edit.Invoke(new Func<EntityEditControl, string, bool>(SetEditErrorInfo), edit, strErrorInfo);
            }
            if (edit.ErrorInfo != strErrorInfo)
            {
                edit.ErrorInfo = strErrorInfo;
                return true;
            }

            return false;
        }

        static string GetEditRefID(EntityEditControl edit)
        {
            if (edit.InvokeRequired)
            {
                return (string)edit.Invoke(new Func<EntityEditControl, string>(GetEditRefID), edit);
            }
            return edit.RefID;
        }

        private void dpTable_browseLines_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dpTable_browseLines.SelectedRows.Count == 1)
            {
                SetCoverImage(this.dpTable_browseLines.SelectedRows[0]);
            }
            else
                goto CLEAR;
            return;
        CLEAR:
            this.pictureBox1.Image = null;
        }

        // 准备特定浏览行的封面图像
        void PrepareCoverImage(DpRow row)
        {
            Debug.Assert(row != null, "");

            RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
            if (info == null)
                return;

            if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                return;

            string strMARC = info.OldXml;
            if (string.IsNullOrEmpty(strMARC) == true)
                return;

            string strUrl = ScriptUtil.GetCoverImageUrl(strMARC);
            if (string.IsNullOrEmpty(strUrl) == true)
                return;

            if (StringUtil.HasHead(strUrl, "http:") == true)
                return;

            if (info != null && info.CoverImageRquested == true)
                return;

            // 通过 dp2library 协议获得图像文件
            if (this.AsyncGetImage != null)
            {
                AsyncGetImageEventArgs e = new AsyncGetImageEventArgs();
                e.RecPath = row[1].Text;
                e.ObjectPath = strUrl;
                e.FileName = "";
                e.Row = row;
                this.AsyncGetImage(this, e);
                // 修改状态，表示已经发出请求
                if (row != null)
                {
                    if (info != null)
                        info.CoverImageRquested = true;
                }
                else
                {
                    this.CoverImageRequested = true;
                }
            }
        }

        // 设置封面图像
        // 按照这样的次序：1) 如果是 http: 直接设置; 2) 如果有本地文件，直接设置; 3) 从服务器获取，然后异步设置
        void SetCoverImage(string strUrl,
            string strLocalImageFileName,
            string strBiblioRecPath,
            DpRow row)
        {
            if (StringUtil.HasHead(strUrl, "http:") == true)
                this.pictureBox1.LoadAsync(strUrl);
            else
            {
                if (string.IsNullOrEmpty(strLocalImageFileName) == false)
                    this.pictureBox1.LoadAsync(strLocalImageFileName);
                else
                {
                    RegisterBiblioInfo info = null;
                    if (row != null)
                        info = row.Tag as RegisterBiblioInfo;

                    if (info != null && info.CoverImageRquested == true)
                        return;

                    // 通过 dp2library 协议获得图像文件
                    if (this.AsyncGetImage != null)
                    {
                        AsyncGetImageEventArgs e = new AsyncGetImageEventArgs();
                        e.RecPath = strBiblioRecPath;
                        e.ObjectPath = strUrl;
                        e.FileName = "";
                        e.Row = row;
                        this.AsyncGetImage(this, e);
                        // 修改状态，表示已经发出请求
                        if (row != null)
                        {
                            if (info != null)
                                info.CoverImageRquested = true;
                        }
                        else
                        {
                            this.CoverImageRequested = true;
                        }
                    }
                }
            }

        }

        // 设置特定浏览行的封面图像
        void SetCoverImage(DpRow row)
        {
            RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
            if (info == null)
                goto CLEAR;

            string strMARC = info.OldXml;
            if (string.IsNullOrEmpty(strMARC) == true)
                goto CLEAR;

            string strUrl = ScriptUtil.GetCoverImageUrl(strMARC);
            if (string.IsNullOrEmpty(strUrl) == true)
                goto CLEAR;

#if NO
            if (StringUtil.HasHead(strUrl, "http:") == true)
                this.pictureBox1.LoadAsync(strUrl);
            else
            {
                if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                    this.pictureBox1.LoadAsync(info.CoverImageFileName);
                else
                {
                    // 通过 dp2library 协议获得图像文件
                    if (this.AsyncGetImage != null)
                    {
                        string strBiblioRecPath = row[1].Text;
                        AsyncGetImageEventArgs e = new AsyncGetImageEventArgs();
                        e.RecPath = strBiblioRecPath;
                        e.ObjectPath = strUrl;
                        e.FileName = "";
                        e.Row = row;
                        this.AsyncGetImage(this, e);
                    }
                }
            }
#endif
            // 设置封面图像
            // 按照这样的次序：1) 如果是 http: 直接设置; 2) 如果有本地文件，直接设置; 3) 从服务器获取，然后异步设置
            SetCoverImage(strUrl,
                info.CoverImageFileName,
                row[1].Text,
                row);
            return;
        CLEAR:
            this.pictureBox1.Image = null;
        }

        // 根据记录路径定位浏览行
        DpRow FindRowByRecPath(string strRecPath)
        {
            foreach (DpRow row in this.dpTable_browseLines.Rows)
            {
                if (row[1].Text == strRecPath)
                    return row;
            }

            return null;
        }

        // 设置图像文件
        public void AsyncGetImageComplete(DpRow row,
            string strBiblioRecPath,
            string strFileName,
            string strErrorInfo)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                this.Invoke(new Action<DpRow, string, string, string>(AsyncGetImageComplete),
                    row, strBiblioRecPath, strFileName, strErrorInfo);
                return;
            }

            // 已经选定了书目记录的情况
            if (row == null)
            {
                if (string.IsNullOrEmpty(strErrorInfo) == false)
                {
                    // 如何报错?
                    // row 为空
                    // this.pictureBox1.Image = null;
                    return;
                }

                // 给相关浏览行设定
                row = FindRowByRecPath(this.BiblioRecPath + "@" + this.ServerName);
                if (row != null)
                {
                    RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                    if (info != null)
                        info.CoverImageFileName = strFileName;
                }

                if (strBiblioRecPath == this.BiblioRecPath
                    && this.DisplayMode == "detail")
                {
                    this.ImageFileName = strFileName;
                    this.pictureBox1.LoadAsync(strFileName);
                }
                return;
            }

            {
                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info != null)
                    info.CoverImageFileName = strFileName;

                if (this.DisplayMode == "select"
                    && this.dpTable_browseLines.SelectedRows.Count == 1
                    && this.dpTable_browseLines.SelectedRows[0] == row)
                    this.pictureBox1.LoadAsync(strFileName);
            }
        }
#if NO
        // TODO: 还可增加返回尺寸，以便 pictureBox 调整大小
        static string GetCoverImageUrl(string strMARC)
        {
            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList nodes = record.select("field[@name='856']");
            if (nodes.count == 0)
                return "";
            string strSmallUrl = "";
            string strLargeUrl = "";
            foreach (MarcField field in nodes)
            {
                string strX = field.select("subfield[@name='x']").FirstContent;
                if (string.IsNullOrEmpty(strX) == true)
                    continue;
                Hashtable table = StringUtil.ParseParameters(strX, ';', ':');
                string strType = (string)table["type"];
                // 优先返回中等尺寸
                if (strType == "FrontCover.MediumImage")
                {
                    return field.select("subfield[@name='u']").FirstContent;
                }
                if (strType == "FrontCover.SmallImage")
                    strSmallUrl = field.select("subfield[@name='u']").FirstContent;
                if (strType == "FrontCover.LargeImage")
                    strLargeUrl = field.select("subfield[@name='u']").FirstContent;
            }

            if (string.IsNullOrEmpty(strLargeUrl) == true)
                return strLargeUrl;
            if (string.IsNullOrEmpty(strSmallUrl) == true)
                return strSmallUrl;
            return "";
        }
#endif

        private void toolStripButton_viewDetail_Click(object sender, EventArgs e)
        {
            this.SetDisplayMode("detail");
            this.Focus();
        }

        private void toolStripButton_viewSummary_Click(object sender, EventArgs e)
        {
            this.SetDisplayMode("summary");
            this.Focus();
        }
    }

    /// <summary>
    /// 在内存中缓存一条书目信息。能够表示新旧记录的修改关系
    /// </summary>
    public class RegisterBiblioInfo
    {
        /// <summary>
        /// 记录路径
        /// </summary>
        public string RecPath = "";
        /// <summary>
        /// 旧的记录 XML
        /// </summary>
        public string OldXml = "";
        /// <summary>
        /// 新的记录 XML
        /// </summary>
        public string NewXml = "";
        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// MARC 格式类型
        /// </summary>
        public string MarcSyntax = "";

        /// <summary>
        /// 封面图像文件名。临时文件
        /// </summary>
        public string CoverImageFileName = "";

        public bool CoverImageRquested = false; // 如果为 true ,表示已经请求了异步获取图像，不要重复请求

        public RegisterBiblioInfo()
        {
        }

        public RegisterBiblioInfo(string strRecPath,
            string strOldXml,
            string strNewXml,
            byte[] timestamp,
            string strMarcSyntax)
        {
            this.RecPath = strRecPath;
            this.OldXml = strOldXml;
            this.NewXml = strNewXml;
            this.Timestamp = timestamp;
            this.MarcSyntax = strMarcSyntax;
        }

        // 拷贝构造
        public RegisterBiblioInfo(RegisterBiblioInfo ref_obj)
        {
            this.RecPath = ref_obj.RecPath;
            this.OldXml = ref_obj.OldXml;
            this.NewXml = ref_obj.NewXml;
            this.Timestamp = ref_obj.Timestamp;
            this.MarcSyntax = ref_obj.MarcSyntax;
        }

    }

    

        /// <summary>
    /// 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AsyncGetImageEventHandler(object sender,
    AsyncGetImageEventArgs e);

    /// <summary>
    /// 事件的参数
    /// </summary>
    public class AsyncGetImageEventArgs : EventArgs
    {
        public string RecPath = "";
        public string ObjectPath = "";  // 一般只是 ID 部分
        public string FileName = "";    // 预先给出的临时文件名
        public DpRow Row = null;
    }


    /// <summary>
    /// 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void DeleteItemEventHandler(object sender,
    DeleteItemEventArgs e);

    /// <summary>
    /// 事件的参数
    /// </summary>
    public class DeleteItemEventArgs : EventArgs
    {
        public EntityEditControl Control = null;
        public string ErrorInfo = "";   // [out]
    }
}

#pragma warning restore 1591