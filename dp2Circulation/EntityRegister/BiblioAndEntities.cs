#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Diagnostics;
using System.Drawing.Drawing2D;

using DigitalPlatform;
using DigitalPlatform.EasyMarc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using System.Threading;

namespace dp2Circulation
{
    /// <summary>
    /// 处理一条书目记录和下属册记录的界面事务的包装类
    /// </summary>
    public class BiblioAndEntities
    {
        public MainForm MainForm = null;

        public Form Owner = null;
        public EasyMarcControl easyMarcControl1 = null;     // MarcControl
        public FlowLayoutPanel flowLayoutPanel1 = null;     // ItemsContainer

        string OldMARC = "";
        public byte [] Timestamp = null;

        public string ServerName = "";
        public string BiblioRecPath = "";

        public string MarcSyntax = "";

        // 存储书目和<dprms:file>以外的其它XML片断
        XmlDocument domXmlFragment = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        public event DeleteItemEventHandler DeleteEntity = null;

        public event EventHandler EntitySelectionChanged = null;

        /// <summary>
        /// 获得册记录缺省值
        /// </summary>
        public event GetDefaultItemEventHandler GetEntityDefault = null;

        /// <summary>
        /// 通知需要装载下属的册记录
        /// 触发前，要求 BiblioRecPath 有当前书目记录的路径
        /// </summary>
        public event EventHandler LoadEntities = null;

        // Ctrl+A自动创建数据
        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        /// <summary>
        /// 校验条码号
        /// </summary>
        public event VerifyBarcodeHandler VerifyBarcode = null;

        public BiblioAndEntities(
            Form owner,
            EasyMarcControl easyMarcControl1,
            FlowLayoutPanel flowLayoutPanel1)
        {
            this.Owner = owner;
            this.easyMarcControl1 = easyMarcControl1;
            this.flowLayoutPanel1 = flowLayoutPanel1;
        }

        /// <summary>
        /// 书目记录内容是否发生过修改
        /// </summary>
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

        /// <summary>
        /// 清除当前的书目和册信息
        /// </summary>
        public void Clear()
        {
            this.easyMarcControl1.Clear();
            this.ClearEntityEditControls();
            this.BiblioChanged = false;
            this.EntitiesChanged = false;
        }

        public string GetChangedWarningText()
        {
            if (this.BiblioChanged == true
    || this.EntitiesChanged == true)
            {
                string strParts = "";
                if (this.BiblioChanged)
                    strParts += "书目记录";
                if (this.EntitiesChanged)
                {
                    if (string.IsNullOrEmpty(strParts) == false)
                        strParts += "和";
                    strParts += "册记录";
                }
                return "当前" + strParts + "修改后尚未保存";
            }
            return null;
        }

        // 获得书目记录的XML格式
        // parameters:
        //      strBiblioDbName 书目库名。用来辅助决定要创建的XML记录的marcsyntax。如果此参数==null，表示会从this.BiblioRecPath中去取书目库名
        //      bIncludeFileID  是否要根据当前rescontrol内容合成<dprms:file>元素?
        public int GetBiblioXml(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strMarcSyntax = this.MarcSyntax;

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            string strMARC = this.GetMarc();
            XmlDocument domMarc = null;
            int nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 因为domMarc是根据MARC记录合成的，所以里面没有残留的<dprms:file>元素，也就没有(创建新的id前)清除的需要

            Debug.Assert(domMarc != null, "");

            // 合成其它XML片断
            if (domXmlFragment != null
                && string.IsNullOrEmpty(domXmlFragment.DocumentElement.InnerXml) == false)
            {
                XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = domXmlFragment.DocumentElement.InnerXml;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                domMarc.DocumentElement.AppendChild(fragment);
            }

            strXml = domMarc.OuterXml;
            return 0;
        }

        // 装载书目以外的其它XML片断
        int LoadXmlFragment(string strXml,
            out string strError)
        {
            strError = "";

            this.domXmlFragment = null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "LoadXmlFragment() XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            // XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield | //dprms:file", nsmgr);
            // 留下 dprms:file
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield", nsmgr);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            this.domXmlFragment = new XmlDocument();
            this.domXmlFragment.LoadXml("<root />");
            this.domXmlFragment.DocumentElement.InnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }


        // 从列表中选择一条书目记录装入编辑模板
        // return:
        //      -1  出错
        //      0   放弃装入
        //      1   成功装入
        public int SetBiblio(
            RegisterBiblioInfo info,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";

            if (this.BiblioChanged == true
                || this.EntitiesChanged == true)
            {
                string strParts = "";
                if (this.BiblioChanged)
                    strParts += "书目记录";
                if (this.EntitiesChanged)
                {
                    if (string.IsNullOrEmpty(strParts) == false)
                        strParts += "和";
                    strParts += "册记录";
                }
                DialogResult result = MessageBox.Show(this.Owner,
"当前"+strParts+"修改后尚未保存。如果此时装入新记录内容，先前的修改将会丢失。\r\n\r\n是否装入新记录?",
"册登记",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

#if NO
            // 警告那些从原书目记录下属装入的册记录修改。但新增的册不会被清除
            if (HasEntitiesChanged("normal") == true)
            {
                DialogResult result = MessageBox.Show(this.Owner,
"当前有册记录修改后尚未保存。如果此时装入新记录内容，先前的修改将会丢失。\r\n\r\n是否装入新记录?",
"BiblioRegisterControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }
            
            this.ClearEntityEditControls("normal");

#endif

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(info.OldXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                goto ERROR1;
            }

            // 装载书目以外的其它XML片断
            nRet = LoadXmlFragment(info.OldXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.ClearEntityEditControls();
            this.EntitiesChanged = false;

            this.OldMARC = strMARC;
            this.Timestamp = info.Timestamp;

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(info.RecPath, "@", out strPath, out strServerName);
            this.ServerName = strServerName;
            this.BiblioRecPath = strPath;

            this.MarcSyntax = strMarcSyntax;    // info.MarcSyntax;

            this.SetMarc(strMARC); // info.OldXml
            Debug.Assert(this.BiblioChanged == false, "");

            // 设置封面图像
#if NO
            string strMARC = info.OldXml;
            if (string.IsNullOrEmpty(strMARC) == false)
            {
                this.ImageUrl = ScriptUtil.GetCoverImageUrl(strMARC);
                this.CoverImageRequested = false;
            }
#endif

            if (this.LoadEntities != null)
                this.LoadEntities(this, new EventArgs());

            return 1;
        ERROR1:
            // MessageBox.Show(this, strError);
            return -1;
        }

        // parameters:
        //      strStyle    清除哪些部分? all/normal
        //                  normal 表示只清除那些从数据库中调出的记录
        public void ClearEntityEditControls(string strStyle = "all")
        {
            if (this.easyMarcControl1.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.easyMarcControl1.Invoke(new Action<string>(ClearEntityEditControls), strStyle);
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

                    // AddEditEvents(edit, false);
                }
                else
                    controls.Add(control);
            }

            if (strStyle != "normal")
            {
                // 卸载事件
                foreach (Control control in this.flowLayoutPanel1.Controls)
                {
                    if (control is PlusButton)
                        AddPlusEvents(control as PlusButton, false);
                    else if (control is EntityEditControl)
                        AddEditEvents(control as EntityEditControl, false);
                }
                this.flowLayoutPanel1.Controls.Clear();
            }
            else
            {
                foreach (Control control in controls)
                {
                    this.flowLayoutPanel1.Controls.Remove(control);
                    // 卸载事件
                    if (control is PlusButton)
                        AddPlusEvents(control as PlusButton, false);
                    else if (control is EntityEditControl)
                        AddEditEvents(control as EntityEditControl, false);
                }
            }

            InvalidateEditControls(-1);

            this.AdjustFlowLayoutHeight();
        }

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

        bool _bEntitiesChanged = false;

        /// <summary>
        /// 册记录内容是否发生过修改
        /// </summary>
        public bool EntitiesChanged
        {
            get
            {
                return this._bEntitiesChanged;
            }
            set
            {
                if (this._bEntitiesChanged != value)
                {
                    this._bEntitiesChanged = value;
                }
            }
        }

        void SetControlColor(EntityEditControl control)
        {
            control.BackColor = _entityBackColor;
            control.MemberForeColor = _entityCaptionForeColor;
            control.SetAllEditColor(_entityEditBackColor, _entityEditForeColor);
            control.FocusedEditBackColor = _focusedEditBackColor;
        }

        void SetControlColor(PlusButton control)
        {
            control.BackColor = _entityBackColor;
            control.ForeColor = this.flowLayoutPanel1.ForeColor;  // SystemColors.GrayText;
        }

        // 添加一个新的册对象
        // parameters:
        //      strRecPath  记录路径
        public int NewEntity(string strRecPath,
            byte[] timestamp,
            string strXml,
            bool ScrollIntoView,
            bool bAutoSetFocus,
            out EntityEditControl control,
            out string strError)
        {
            strError = "";
#if NO
            if (this.easyMarcControl1.InvokeRequired)
            {
                Delegate_NewEntity d = new Delegate_NewEntity(NewEntity);
                object[] args = new object[5];
                args[0] = strRecPath;
                args[1] = timestamp;
                args[2] = strXml;
                args[3] = ScrollIntoView;
                args[4] = strError;
                int result = (int)this.easyMarcControl1.Invoke(d, args);

                // 取出out参数值
                strError = (string)args[4];
                return result;
            }
#endif

            control = new EntityEditControl();
            control.DisplayMode = "simple_register";
            control.HideSelection = false;
            control.Width = 120;
            control.AutoScroll = false;
            control.AutoSize = true;
            control.Font = this.Owner.Font;
            control.BackColor = Color.Transparent;
            control.Margin = new Padding(8, 8, 8, 8);
            // control.TableMargin = new Padding(100);
            control.TablePadding = new Padding(64,12,24,12);

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

            AddEditEvents(control, true);

            // ClearBlank();

            // control.BackColor = ControlPaint.Dark(this.flowLayoutPanel1.BackColor);
            SetControlColor(control);
#if NO
            control.BackColor = this.flowLayoutPanel1.BackColor;
            control.ForeColor = this.flowLayoutPanel1.ForeColor;
#endif
            
            // this.flowLayoutPanel1.Controls.Add(control);
            AddEditControl(control);

            // this.flowLayoutPanel1.PerformLayout();
            // this.tableLayoutPanel1.PerformLayout();

            this.AdjustFlowLayoutHeight();

            if (bAutoSetFocus == true)
                control.Focus();    // 这一句让 Edit Control 部分可见，但不是全部可见

            if (ScrollIntoView)
                this.flowLayoutPanel1.ScrollControlIntoView(control);

            return 0;
        }

        // 册记录编辑器 背景色
        Color _entityBackColor = SystemColors.Control;
        // 册记录编辑器 左边标题区的前景颜色
        Color _entityCaptionForeColor = SystemColors.ControlText;
        // 册记录编辑器 编辑控件的背景色
        Color _entityEditBackColor = SystemColors.Window;
        // 册记录编辑器 编辑控件的前景色
        Color _entityEditForeColor = SystemColors.WindowText;

        Color _focusedEditBackColor = Color.FromArgb(200, 200, 255);

        public void SetEntityColorStyle(string strStyle)
        {
            if (strStyle == "dark")
            {
                // 册记录编辑器 背景色
                _entityBackColor = Color.FromArgb(50, 50, 50); // ControlPaint.Light(this.flowLayoutPanel1.BackColor);
                // 册记录编辑器 左边标题区的前景颜色
                _entityCaptionForeColor = Color.FromArgb(200, 200, 180);
                // 册记录编辑器 编辑控件的背景色
                _entityEditBackColor = this.flowLayoutPanel1.BackColor;
                // 册记录编辑器 编辑控件的前景色
                _entityEditForeColor = this.flowLayoutPanel1.ForeColor;

                _focusedEditBackColor = ControlPaint.Dark(this.flowLayoutPanel1.BackColor);
            }
            else if (strStyle == "light")
            {
                // 册记录编辑器 背景色
                _entityBackColor = SystemColors.Control;
                // 册记录编辑器 左边标题区的前景颜色
                _entityCaptionForeColor = SystemColors.ControlText;
                // 册记录编辑器 编辑控件的背景色
                _entityEditBackColor = SystemColors.Window;
                // 册记录编辑器 编辑控件的前景色
                _entityEditForeColor = SystemColors.WindowText;

                _focusedEditBackColor = Color.FromArgb(200, 200, 255);
            }

            // 把当前已经创建的 EntityEditControl 和 PlusButton 重设颜色
            foreach(Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is EntityEditControl)
                {
                    SetControlColor(control as EntityEditControl);
                }

                if (control is PlusButton)
                {
                    SetControlColor(control as PlusButton);
                }
            }
        }

        void AddEditEvents(EntityEditControl edit, bool bAdd)
        {
            if (bAdd)
            {
                edit.PaintContent += new PaintEventHandler(edit_PaintContent);
                edit.ContentChanged += new DigitalPlatform.ContentChangedEventHandler(edit_ContentChanged);
                edit.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(edit_GetValueTable);
                edit.AppendMenu += new ApendMenuEventHandler(edit_AppendMenu);
                edit.MouseClick += edit_MouseClick;
                edit.GetAccessNoButton.Click += GetAccessNoButton_Click;
                edit.LocationStringChanged += edit_LocationStringChanged;
                edit.ControlKeyDown += edit_ControlKeyDown;
                edit.Enter += edit_Enter;
                edit.SelectionChanged += edit_SelectionChanged;
            }
            else
            {
                edit.PaintContent -= new PaintEventHandler(edit_PaintContent);
                edit.ContentChanged -= new DigitalPlatform.ContentChangedEventHandler(edit_ContentChanged);
                edit.GetValueTable -= new DigitalPlatform.GetValueTableEventHandler(edit_GetValueTable);
                edit.AppendMenu -= new ApendMenuEventHandler(edit_AppendMenu);
                edit.MouseClick -= edit_MouseClick;
                edit.GetAccessNoButton.Click -= GetAccessNoButton_Click;
                edit.LocationStringChanged -= edit_LocationStringChanged;
                edit.ControlKeyDown -= edit_ControlKeyDown;
                edit.Enter -= edit_Enter;
                edit.SelectionChanged -= edit_SelectionChanged;
            }
        }

        void edit_SelectionChanged(object sender, EventArgs e)
        {
            if (this.EntitySelectionChanged != null)
                this.EntitySelectionChanged(sender, e);
        }

        void edit_Enter(object sender, EventArgs e)
        {

        }

        void edit_ControlKeyDown(object sender, ControlKeyEventArgs e)
        {
            EntityEditControl edit = sender as EntityEditControl;
            Debug.Assert(edit != null, "");

            string strAction = "copy";

            bool bUp = false;

            Debug.WriteLine("keycode=" + e.e.KeyCode.ToString());

            if (e.e.KeyCode == Keys.A && e.e.Control == true)
            {
                if (this.GenerateData != null)
                {
                    // 如果遇到报错会弹出 MessageBox
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    if (e.Name == "AccessNo")
                        e1.ScriptEntry = "CreateCallNumber";
                    e1.FocusedControl = sender; // sender为 EntityEditControl
                    this.GenerateData(this, e1);
                }
                e.e.SuppressKeyPress = true;    // 避免 Ctrl+A 键引起 textbox 文本的无谓改变
                return;
            }
            else if (e.Name == "AccessNo"
                && e.e.KeyCode == Keys.Enter
                && (StringUtil.HasHead(edit.AccessNo, "@accessNo") == true || string.IsNullOrEmpty(edit.AccessNo) == true))
            {
                if (this.GenerateData != null)
                {
                    // MessageBox.Show(this.Owner, "create call number");
                    // edit.ErrorInfo = "";
                    // 如果遇到报错会弹出 MessageBox
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.ScriptEntry = "CreateCallNumber";
                    e1.FocusedControl = sender; // sender为 EntityEditControl
                    this.GenerateData(this, e1);
                }
                return;
            }
            else if (e.e.KeyCode == Keys.PageDown && e.e.Control == true)
            {
                return;
            }
            else if (e.e.KeyCode == Keys.PageUp && e.e.Control == true)
            {
                return;
            }
            else if (e.e.KeyCode == Keys.OemOpenBrackets && e.e.Control == true)
            {
                bUp = true; // 从上面拷贝
            }
            else if (e.e.KeyCode == Keys.OemCloseBrackets && e.e.Control == true)
            {
                bUp = false;    // 从下面拷贝
            }
            else if (e.e.KeyCode == Keys.OemMinus && e.e.Control == true)
            {
                bUp = true; // 从上面减量
                strAction = "minus";
            }
            else if (e.e.KeyCode == Keys.Oemplus && e.e.Control == true)
            {
                bUp = true;    // 从上面增量
                strAction = "plus";
            }
            else if (e.e.KeyCode == Keys.D0 && e.e.Control == true)
            {
                bUp = false; // 从下面减量
                strAction = "minus";
            }
            else if (e.e.KeyCode == Keys.D9 && e.e.Control == true)
            {
                bUp = false;    // 从下面增量
                strAction = "plus";
            }
            else
                return;

            EntityEditControl next = GetPrevOrNextEdit(edit, bUp);
            if (next == null)
                return; 
            switch (e.Name)
            {
                case "PublishTime":
                    edit.PublishTime =
                        DoAction(strAction, next.PublishTime);
                    break;
                case "Seller":
                    edit.Seller =
                        DoAction(strAction, next.Seller);
                    break;
                case "Source":
                    edit.Source =
                        DoAction(strAction, next.Source);
                    break;
                case "Intact":
                    edit.Intact =
                        DoAction(strAction, next.Intact);
                    break;
                case "Binding":
                    edit.Binding =
                        DoAction(strAction, next.Binding);
                    break;
                case "Operations":
                    edit.Operations =
                        DoAction(strAction, next.Operations);
                    break;
                case "Price":
                    edit.Price =
                        DoAction(strAction, next.Price);
                    break;
                case "Barcode":
                    edit.Barcode =
                        DoAction(strAction, next.Barcode);
                    break;
                case "State":
                    edit.State =
                        DoAction(strAction, next.State);
                    break;
                case "Location":
                    edit.LocationString =
                        DoAction(strAction, next.LocationString);
                    break;
                case "Comment":
                    edit.Comment =
                        DoAction(strAction, next.Comment);
                    break;
                case "Borrower":
                    Console.Beep();
                    break;
                case "BorrowDate":
                    Console.Beep();
                    break;
                case "BorrowPeriod":
                    Console.Beep();
                    break;
                case "RecPath":
                    Console.Beep();
                    break;
                case "BookType":
                    edit.BookType =
                        DoAction(strAction, next.BookType);
                    break;
                case "RegisterNo":
                    edit.RegisterNo =
                        DoAction(strAction, next.RegisterNo);
                    break;
                case "MergeComment":
                    edit.MergeComment =
                        DoAction(strAction, next.MergeComment);
                    break;
                case "BatchNo":
                    edit.BatchNo =
                        DoAction(strAction, next.BatchNo);
                    break;
                case "Volume":
                    edit.Volume =
                        DoAction(strAction, next.Volume);
                    break;
                case "AccessNo":
                    edit.AccessNo =
                        DoAction(strAction, next.AccessNo);
                    break;
                case "RefID":
                    Console.Beep();
                    break;
                default:
                    Debug.Assert(false, "未知的栏目名称 '" + e.Name + "'");
                    return;
            }
        }

        static string DoAction(
    string strAction,
    string strValue)
        {
            string strError = "";
            string strResult = "";
            int nNumber = 0;
            int nRet = 0;

            if (strAction == "minus")
            {
                nNumber = -1;

                // 给一个被字符引导的数字增加一个数量。
                // 例如 B019 + 1 变成 B020
                nRet = StringUtil.IncreaseLeadNumber(strValue,
                    nNumber,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    strResult = strError;
                return strResult;
            }
            else if (strAction == "plus")
            {
                nNumber = 1;

                // 给一个被字符引导的数字增加一个数量。
                // 例如 B019 + 1 变成 B020
                nRet = StringUtil.IncreaseLeadNumber(strValue,
                    nNumber,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    strResult = strError;
                return strResult;
            }
            else if (strAction == "copy")
                return strValue;
            else
                return "未知的strAction值 '" + strAction + "'";
        }

        EntityEditControl GetPrevOrNextEdit(
    EntityEditControl current,
    bool bPrev)
        {
            int nIndex = this.flowLayoutPanel1.Controls.IndexOf(current);
            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                Debug.Assert(false, "");
                return null;
            }

            if (bPrev == true)
                nIndex--;
            else
                nIndex++;

            if (nIndex <= -1)
                return null;

            if (nIndex >= this.flowLayoutPanel1.Controls.Count)
                return null;

            Control control = this.flowLayoutPanel1.Controls[nIndex];
            if (!(control is EntityEditControl))
                return null;

            return control as EntityEditControl;
        }

        public PlusButton GetPlusButton()
        {
            foreach(Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is PlusButton)
                    return control as PlusButton;
            }
            return null;
        }

        /// <summary>
        /// 构造索取号信息集合
        /// </summary>
        /// <returns>CallNumberItem事项集合</returns>
        public List<CallNumberItem> GetCallNumberItems()
        {
            List<CallNumberItem> results = new List<CallNumberItem>();
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (!(control is EntityEditControl))
                    continue;

                EntityEditControl edit = control as EntityEditControl;

                CallNumberItem item = new CallNumberItem();
                item.RecPath = edit.RecPath;
                item.CallNumber = edit.AccessNo;
                item.Location = edit.LocationString;
                item.Barcode = edit.Barcode;

                results.Add(item);
            }

            return results;
        }

        void edit_LocationStringChanged(object sender, TextChangeEventArgs e)
        {
            string strError = "";

            EntityEditControl edit = sender as EntityEditControl;

            if (edit.Initializing == false
                && string.IsNullOrEmpty(edit.AccessNo) == false)
            {
                ArrangementInfo old_info = null;
                string strOldName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                // <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
                int nRet = this.MainForm.GetArrangementInfo(e.OldText,
                    out old_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strOldName = old_info.ArrangeGroupName;

                ArrangementInfo new_info = null;
                string strNewName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = this.MainForm.GetArrangementInfo(e.NewText,
                   out new_info,
                   out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strNewName = new_info.ArrangeGroupName;

                if (strOldName != strNewName)
                {
                    DialogResult result = MessageBox.Show(this.Owner,
    "您修改了馆藏地点，因而变动了记录所从属的排架体系，现有的索取号已不再适合变动后的排架体系。\r\n\r\n是否重新创建索取号?",
    "册登记",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.No)
                        return;
                    edit.AccessNo = "";
                    edit.ErrorInfo = "";
                    if (this.GenerateData != null)
                    {
                        Cursor old_cursor = edit.Cursor;
                        edit.Cursor = Cursors.WaitCursor;
                        try
                        {
                            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                            e1.ScriptEntry = "CreateCallNumber";
                            e1.FocusedControl = edit; // sender为 EntityEditControl
                            this.GenerateData(this, e1);
                        }
                        finally
                        {
                            edit.Cursor = old_cursor;
                        }
                    }
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this.Owner, strError);
        }

        public EntityEditControl GetFocusedEditControl()
        {
            if (this.flowLayoutPanel1.ContainsFocus == false)
                return null;

            // 找到拥有输入焦点的那个 EntityEditControl
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control.ContainsFocus == true
                    && control is EntityEditControl)
                    return (control as EntityEditControl);
            }

            return null;
        }

        void GetAccessNoButton_Click(object sender, EventArgs e)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                if (Control.ModifierKeys == Keys.Control)
                    e1.ScriptEntry = "ManageCallNumber";
                else
                    e1.ScriptEntry = "CreateCallNumber";
                e1.FocusedControl = GetFocusedEditControl(); // sender为最原始的子控件
                this.GenerateData(this, e1);
            }
        }

        void edit_MouseDown(object sender, MouseEventArgs e)
        {

        }

        void edit_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            Control control = sender as Control;

            if (GuiUtil.PtInRect(e.X, e.Y, GetEditCloseButtonRect(control)) == true)
                menu_deleteItem_Click(sender, new EventArgs());
        }

        // 将加号滚入视野可见范围
        public bool ScrollPlusIntoView()
        {
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is Button)
                {
                    this.flowLayoutPanel1.ScrollControlIntoView(control);
                    return true;
                }
            }

            return false;
        }

        // 将册记录编辑控件加入末尾。注意末尾可能有 Button 控件，要插入在它前面
        void AddEditControl(EntityEditControl edit)
        {
            List<Control> buttons = new List<Control>();
            // 先将 Button 标识出来
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control is Button)
                    buttons.Add(control);
            }

            // 移走 Button
            foreach(Control control in buttons)
            {
                this.flowLayoutPanel1.Controls.Remove(control);
            }

            // 追加 edit
            this.flowLayoutPanel1.Controls.Add(edit);

            // 将 Button 加到末尾
            foreach(Control control in buttons)
            {
                this.flowLayoutPanel1.Controls.Add(control);
            }

            // 注：edit 控件加入到末尾，不会改变前面已有的 edit 控件显示的序号

            // 重新设置 TabIndex
            int i = 0;
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                control.TabIndex = i++;
            }
        }

        void edit_PaintContent(object sender, PaintEventArgs e)
        {
            //e.Graphics.InterpolationMode = InterpolationMode.High;
            //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint =
        System.Drawing.Text.TextRenderingHint.AntiAlias;

            EntityEditControl control = sender as EntityEditControl;

            int index = this.flowLayoutPanel1.Controls.IndexOf(control);
            string strText = (index + 1).ToString();
            using (Brush brush = new SolidBrush(Color.DarkGreen))  // Color.FromArgb(220, 220, 220)
            {
                if (control.CreateState == ItemDisplayState.New
                    || control.CreateState == ItemDisplayState.Deleted)
                {
                    // 几种状态的颜色
                    Color state_color = Color.Transparent;
                    if (control.CreateState == ItemDisplayState.New)
                        state_color = Color.FromArgb(0, 200, 0);
                    else if (control.CreateState == ItemDisplayState.Deleted)
                        state_color = Color.FromArgb(200, 200, 200);

                    // 绘制左上角的三角形
                    using (Brush brushState = new SolidBrush(state_color))
                    {
                        int x0 = 0;
                        int y0 = 0;
                        int nWidth = 80;
                        Point[] points = new Point[3];
                        points[0] = new Point(x0, y0);
                        points[1] = new Point(x0, nWidth);
                        points[2] = new Point(nWidth, y0);
                        e.Graphics.FillPolygon(brushState, points);
                    }
                }

                // 绘制序号
                using (Font font = new Font(this.Owner.Font.Name, control.Height / 4, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    SizeF size = e.Graphics.MeasureString(strText, font);
                    // PointF start = new PointF(control.Width / 2, control.Height / 2 - size.Height / 2);
                    PointF start = new PointF(8, 16);

                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Near;

                    e.Graphics.DrawString(strText, font, brush, start, format);
                }
            }

            // 绘制关闭按钮
            {
                ControlPaint.DrawCaptionButton(e.Graphics, GetEditCloseButtonRect(control), CaptionButton.Close, ButtonState.Flat);
            }
        }

        static Rectangle GetEditCloseButtonRect(Control control)
        {
            Size size = new Size(24, 12); // SystemInformation.CaptionButtonSize;
            // return new Rectangle(control.ClientSize.Width - size.Width, 0, size.Width, size.Height);
            return new Rectangle(0, 0, size.Width, size.Height);    // 左上角
        }

        void edit_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(this, e);    // sender wei
        }

        void edit_ContentChanged(object sender, DigitalPlatform.ContentChangedEventArgs e)
        {
            this._bEntitiesChanged = true;
        }

        void edit_AppendMenu(object sender, AppendMenuEventArgs e)
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
            EntityEditControl control = null;

            if (sender is MenuItem)
            {
                MenuItem menuItem = sender as MenuItem;

                control = menuItem.Tag as EntityEditControl;
            }
            else if (sender is EntityEditControl)
                control = sender as EntityEditControl;
            else
                throw new ArgumentException("sender 必须为 MenuItem 或 EntityEditControl 类型", "sender");

            DialogResult result = MessageBox.Show(this.Owner,
"确实要删除册记录?",
"册登记",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            if (string.IsNullOrEmpty(control.RecPath) == false)
            {
                if (this.DeleteEntity != null)
                {
                    DeleteItemEventArgs e1 = new DeleteItemEventArgs();
                    e1.Control = control;
                    this.DeleteEntity(this, e1);
                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        MessageBox.Show(this.Owner, e1.ErrorInfo);
                        return;
                    }
                }
            }
            else
            {
                // this.flowLayoutPanel1.Controls.Remove(control);
                RemoveEditControl(control);
            }
        }

        // 删除一个册记录控件
        public void RemoveEditControl(EntityEditControl edit)
        {
            if (this.easyMarcControl1.InvokeRequired == true)
            {
                this.easyMarcControl1.Invoke(new Action<EntityEditControl>(RemoveEditControl), edit);
                return;
            }

            int index = this.flowLayoutPanel1.Controls.IndexOf(edit);
            if (index == -1)
            {
                Debug.Assert(false, "");
                return;
            }
            this.flowLayoutPanel1.Controls.Remove(edit);
            AddEditEvents(edit, false);

            // 把删除后顶替被删除对象位置的 Control，滚入可见范围
            if (index < this.flowLayoutPanel1.Controls.Count)
            {
                Control ref_control = this.flowLayoutPanel1.Controls[index];
                this.flowLayoutPanel1.ScrollControlIntoView(ref_control);
            }
            else if (this.flowLayoutPanel1.Controls.Count > 0)
            {
                Control ref_control = this.flowLayoutPanel1.Controls[this.flowLayoutPanel1.Controls.Count - 1];
                this.flowLayoutPanel1.ScrollControlIntoView(ref_control);
            }

            InvalidateEditControls(index);
        }

        // 要把指定的 Edit 控件后面的全部 Edit 控件 Invalidate 一遍，因为序号发生了变化
        // parameters:
        //      index    从哪个 edit 控件开始刷新。如果为 -1，表示全部刷新
        void InvalidateEditControls(int index)
        {
            if (index == -1)
                index = 0;
            for (int i = index; i < this.flowLayoutPanel1.Controls.Count; i++ )
            {
                Control control = this.flowLayoutPanel1.Controls[i];
                if (control is EntityEditControl)
                    control.Invalidate();
            }
        }

#if NO
        // 要把指定的 Edit 控件后面的全部 Edit 控件 Invalidate 一遍，因为序号发生了变化
        // parameters:
        //      edit    从哪个 edit 控件开始刷新。如果为 null，表示全部刷新
        void InvalidateEditControls(EntityEditControl edit)
        {
            bool bBegin = false;
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (control == edit || edit == null)
                    bBegin = true;
                if (bBegin == true && control is EntityEditControl)
                    control.Invalidate();
            }
        }
#endif

#if NO
        // 清除临时标签
        void ClearBlank()
        {
            if (this.flowLayoutPanel1.Controls.Count == 1
                && this.flowLayoutPanel1.Controls[0] is Label)
                this.flowLayoutPanel1.Controls.Clear();
        }
#endif

        public void AdjustFlowLayoutHeight()
        {
#if NO
            if (this.easyMarcControl1.InvokeRequired)
            {
                this.easyMarcControl1.Invoke(new Action(AdjustFlowLayoutHeight));
                return;
            }

            Size size = this.flowLayoutPanel1.GetPreferredSize(this.ClientSize);
            int nRow = this.tableLayoutPanel1.GetCellPosition(this.flowLayoutPanel1).Row;
            this.tableLayoutPanel1.RowStyles[nRow] = new RowStyle(SizeType.Absolute,
                size.Height + this.flowLayoutPanel1.Margin.Vertical
                // size.Height + (control != null ? control.Margin.Vertical : 0)
                );
#endif
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

            int nRet = ReplaceEntityMacro(dom,
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

            // 第一遍宏兑现后，第二遍专门兑现 @accessNo 宏
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
            if (string.IsNullOrEmpty(strAccessNo) == false)
                strAccessNo = strAccessNo.Trim();
            if (strAccessNo == "@accessNo")
            {
                // 获得索取号
                if (this.GenerateData != null)
                {
                    GetCallNumberParameter parameter = new GetCallNumberParameter();
                    parameter.ExistingAccessNo = "";
                    parameter.Location = DomUtil.GetElementText(dom.DocumentElement, "location");
                    parameter.RecPath = ""; // TODO: 可以通过书目库名，获得对应的实体库名，从而模拟出记录路径
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.ScriptEntry = "CreateCallNumber";
                    e1.FocusedControl = null;
                    e1.Parameter = parameter;
                    this.GenerateData(this, e1);

                    if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        // TODO: edit 控件索取号右边要有个提示区就好了。或者 tips
                        strError = e1.ErrorInfo;
                        return -1;
                    }
                    else
                    {
                        parameter = e1.Parameter as GetCallNumberParameter;
                        DomUtil.SetElementText(dom.DocumentElement, "accessNo", parameter.ResultAccessNo);
                        bChanged = true;
                    }
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

#if NO
        // 从当前书目记录中获得价格字符串
        // TODO: 是否需要对来自 MARC21 的 $??? 字符串进行处理，变成 USD??? 另外 020$c 也不全是价格，可能还有单纯的注释
        string GetBiblioPriceString()
        {
            MarcRecord record = new MarcRecord(this.GetMarc());
            if (this.MarcSyntax == "unimarc")
                return record.select("field[@name='010']/subfield[@name='d']").FirstContent;
            else if (this.MarcSyntax == "usmarc")
                return record.select("field[@name='020']/subfield[@name='c']").FirstContent;
            else
                return "";  // 暂时无法处理其他 MARC 格式
        }
#endif

        // 兑现 @... 宏值。
        // 如果无法解释宏，则原样返回宏名
        string DoGetMacroValue(string strMacroName)
        {
            if (string.IsNullOrEmpty(this.MarcSyntax) == true)
                return strMacroName;

#if NO
            if (strMacroName == "@accessNo")
            {
                return strMacroName;    // 不处理
            }
#endif
            string strMARC = GetMarc();
            if (string.IsNullOrEmpty(strMARC) == false)
            {
                MarcRecord record = new MarcRecord(strMARC);

                string strValue = null;
                // UNIMARC 情形
                if (this.MarcSyntax == "unimarc")
                {
                    if (strMacroName == "@price")
                        strValue = record.select("field[@name='010']/subfield[@name='d']").FirstContent;
                }
                else if (this.MarcSyntax == "usmarc")
                {
                    if (strMacroName == "@price")
                        strValue = record.select("field[@name='020']/subfield[@name='c']").FirstContent;
                }

                if (string.IsNullOrEmpty(strValue) == false)
                    return strValue;
            }

            return strMacroName;
        }

        // 是否至少有一个内容非空?
        static bool AtLeastOneNotEmpty(MarcNodeList nodes)
        {
            foreach (MarcNode subfield in nodes)
            {
                if (string.IsNullOrEmpty(subfield.Content) == false)
                    return true;
            }

            return false;
        }

        // 检查书目记录的格式是否正确
        //      errors  返回册记录出错信息。每个元素返回一个错误信息，顺次对应于每个有错的字段或者子字段
        // return:
        //      -1  检查过程出错。错误信息在 strError 中。和返回 1 的区别是，这里是某些因素导致无法检查了，而不是因为册记录格式有错
        //      0   正确
        //      1   有错。错误信息在 errors 中
        public int VerifyBiblio(
            string strStyle,
            out List<BiblioError> errors,
            out string strError)
        {
            strError = "";
            errors = new List<BiblioError>();
            int nRet = 0;

            MarcRecord record = new MarcRecord(this.GetMarc());

            if (this.MarcSyntax == "unimarc")
            {
                string strTitle = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                if (string.IsNullOrEmpty(strTitle) == true)
                    errors.Add(new BiblioError("缺乏书名 (200$a)"));

                MarcNodeList nodes = record.select("field[starts-with(@name, '7')]/subfield[@name='a']");
                if (AtLeastOneNotEmpty(nodes) == false)
                    errors.Add(new BiblioError("缺乏作者 (7XX$a)"));

                nodes = record.select("field[starts-with(@name, '69') or @name='686']/subfield[@name='a']");
                if (AtLeastOneNotEmpty(nodes) == false)
                    errors.Add(new BiblioError("缺乏分类号 (69X$a 或 686$a)"));

                if (errors.Count > 0)
                    return 1;   // 先报错。等这里报错的问题都修正了，才会报空子字段的错
            }
            else if (this.MarcSyntax == "usmarc")
            {

                if (errors.Count > 0)
                    return 1;
            }
            else
            {
                // 暂时无法处理其他 MARC 格式
            }

            {
                MarcNodeList nodes = record.select("field/subfield");
                foreach (MarcSubfield subfield in nodes)
                {
                    if (string.IsNullOrEmpty(subfield.Content) == true)
                    {
                        string strFieldName = subfield.Parent.Name;
                        string strSubfieldName = subfield.Name;
                        errors.Add(new BiblioError(strFieldName, strSubfieldName,
                            "字段 '"
                            + this.easyMarcControl1.GetCaption(strFieldName, null, false)
                            + "' 中出现了空子字段 '" 
                            + this.easyMarcControl1.GetCaption(strFieldName, strSubfieldName, false)
                            + "'。需要把它删除"));
                    }
                }
            }

            if (errors.Count > 0)
                return 1; 
            return 0;
        }

        // 补全册记录字段
        public int CompleteEntities(out string strError)
        {
            strError = "";

            int i = 0;
            foreach (Control control in this.flowLayoutPanel1.Controls)
            {
                if (!(control is EntityEditControl))
                    continue;

                EntityEditControl edit = control as EntityEditControl;

                if (edit.Changed == false)
                {
                    i++;
                    continue;
                }
                string strName = "册记录 " + (i + 1);

                string strPrice = edit.Price.Trim();
                if (strPrice == "@price")
                {
                    // 兑现宏
                    string strResult = DoGetMacroValue(strPrice);
                    if (strResult != strPrice)
                        edit.Price = strResult;
                }

                string strAccessNo = edit.AccessNo.Trim();
                if (string.IsNullOrEmpty(strAccessNo) == true
                    || strAccessNo == "@accessNo")
                {
                    // 获得索取号
                    if (this.GenerateData != null)
                    {
                        GetCallNumberParameter parameter = new GetCallNumberParameter();
                        parameter.ExistingAccessNo = "";
                        parameter.Location = edit.LocationString;
                        parameter.RecPath = ""; // TODO: 可以通过书目库名，获得对应的实体库名，从而模拟出记录路径
                        GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                        e1.ScriptEntry = "CreateCallNumber";
                        e1.FocusedControl = null;
                        e1.Parameter = parameter;
                        this.GenerateData(this, e1);

                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            // TODO: edit 控件索取号右边要有个提示区就好了。或者 tips

                        }
                        else
                        {
                            parameter = e1.Parameter as GetCallNumberParameter;
                            edit.AccessNo = parameter.ResultAccessNo;
                        }
                    }
                }

                i++;
            }

            return 0;
        }

        // 检查册记录的格式是否正确
        // parameters:
        //      errors  返回册记录出错信息。每个元素返回一个错误信息，顺次对应于每个有错的册记录。文字中有说明，是那个册记录出错
        // return:
        //      -1  检查过程出错。错误信息在 strError 中。和返回 1 的区别是，这里是某些因素导致无法检查了，而不是因为册记录格式有错
        //      0   正确
        //      1   有错。错误信息在 errors 中
        public int VerifyEntities(
            string strStyle,
            out List<string> errors,
            out string strError)
        {
            strError = "";
            errors = new List<string>();
            int nRet = 0;

            bool bNeedBookType = (StringUtil.IsInList("need_booktype", strStyle) == true);
            bool bNeedLocation = (StringUtil.IsInList("need_location", strStyle) == true);
            bool bNeedAccessNo = (StringUtil.IsInList("need_accessno", strStyle) == true);
            bool bNeedPrice = (StringUtil.IsInList("need_price", strStyle) == true);
            bool bNeedBarcode = (StringUtil.IsInList("need_barcode", strStyle) == true);
            bool bNeedBatchNo = (StringUtil.IsInList("need_batchno", strStyle) == true);

            // 是否验证全部册记录？如果为 false，仅验证修改过的册记录
            bool bVerifyAll = (StringUtil.IsInList("verify_all", strStyle) == true);

            int i = 0;
            foreach(Control control in this.flowLayoutPanel1.Controls)
            {
                if (!(control is EntityEditControl))
                    continue;

                EntityEditControl edit = control as EntityEditControl;

                if (bVerifyAll == false && edit.Changed == false)
                {
                    i++;
                    continue;
                }
                string strName = "册记录 " + (i + 1);
                List<string> conditions = new List<string>();

                if (bNeedBookType == true)
                {
                    if (string.IsNullOrEmpty(edit.BookType) == true)
                        conditions.Add("尚未输入册类型");
                }

                if (bNeedLocation == true)
                {
                    if (string.IsNullOrEmpty(edit.LocationString) == true)
                        conditions.Add("尚未输入馆藏地");
                    else if (edit.LocationString.IndexOf("*") != -1)
                    {
                        conditions.Add("馆藏地点字符串中不允许出现字符 '*'");
                    }
                }

                if (bNeedAccessNo == true)
                {
                    if (string.IsNullOrEmpty(edit.AccessNo) == true)
                        conditions.Add("尚未创建索取号");
                    else if (edit.AccessNo == "@accessNo")
                        conditions.Add("尚未创建索取号 (宏 @accessNo 尚未兑现)");
                }


                if (bNeedPrice == true)
                {
                    if (string.IsNullOrEmpty(edit.Price) == true)
                        conditions.Add("尚未输入册价格");
                    else if (edit.AccessNo == "@price")
                        conditions.Add("尚未输入册价格 (宏 @price 尚未兑现)");
                    else if (edit.Price.IndexOf("@") != -1)
                    {
                        conditions.Add("价格字符串中不允许出现字符 '@'");
                    }
                    else
                    {
                        CurrencyItem item = null;
                        // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
                        nRet = PriceUtil.ParseSinglePrice(edit.Price,
                            out item,
                            out strError);
                        if (nRet == -1)
                        {
                            conditions.Add("册价格 '" + edit.Price + "' 格式不合法: " + strError);
                        }
                        else
                        {
                            // 进一步严格检查汉字。后缀
                            List<string> temp_errors = new List<string>();
                            if (StringUtil.ContainHanzi(item.Prefix) == true)
                                temp_errors.Add("前缀(货币类型)部分不应包含汉字");
                            if (string.IsNullOrEmpty(item.Postfix) == false)
                                temp_errors.Add("不允许使用后缀。价格字符串应为 CNY12.0 或 CNY6.0*2 或 CNY24.0/2 这样的形态");
#if NO
                            if (StringUtil.ContainHanzi(item.Postfix) == true)
                                temp_errors.Add("后缀部分不应包含汉字");
#endif
                            if (temp_errors.Count > 0)
                                conditions.Add("册价格 '" + edit.Price + "' 格式不合法: " + StringUtil.MakePathList(temp_errors, "; "));
                        }
                    }
                }

                if (bNeedBarcode == true)
                {
                    if (string.IsNullOrEmpty(edit.Barcode) == true)
                        conditions.Add("尚未输入册条码号");

                    // 检查册价格字符串格式是否正确
                    // 形式校验条码号
                    // return:
                    //      -2  服务器没有配置校验方法，无法校验
                    //      -1  error
                    //      0   不是合法的条码号
                    //      1   是合法的读者证条码号
                    //      2   是合法的册条码号
                    nRet = this.DoVerifyBarcode(
                        edit.Barcode,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "验证册条码号格式时出错: " + strError;
                        return -1;
                    }

                    // 输入的条码格式不合法
                    if (nRet == 0)
                    {
                        conditions.Add("条码号 " + edit.Barcode + " 格式不正确(" + strError + ")");
                    }

                    // 实际输入的是读者证条码号
                    if (nRet == 1)
                    {
                        conditions.Add("条码号 " + edit.Barcode + " 是读者证条码号。请输入册条码号");
                    }
                }

                if (bNeedBatchNo == true)
                {
                    if (string.IsNullOrEmpty(edit.BatchNo) == true)
                        conditions.Add("尚未输入批次号");
                }

                if (conditions.Count > 0)
                {
                    errors.Add(strName + ": " + StringUtil.MakePathList(conditions));
                    edit.ErrorInfo = StringUtil.MakePathList(conditions);
                }
                else
                {
                    edit.ErrorInfo = "";
                }

                i++;
            }

            if (errors.Count > 0)
                return 1;
            return 0;
        }

        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>      -2  服务器没有配置校验方法，无法校验</para>
        /// <para>      -1  出错</para>
        /// <para>      0   不是合法的条码号</para>
        /// <para>      1   是合法的读者证条码号</para>
        /// <para>      2   是合法的册条码号</para>
        /// </returns>
        public int DoVerifyBarcode(string strBarcode,
            out string strError)
        {
            if (this.VerifyBarcode == null)
            {
                strError = "尚未挂接 VerifyBarcode事件";
                return -1;
            }

            VerifyBarcodeEventArgs e = new VerifyBarcodeEventArgs();
            e.Barcode = strBarcode;
            this.VerifyBarcode(this, e);
            strError = e.ErrorInfo;
            return e.Result;
        }

        public List<string> HideFieldNames = new List<string>();

        public void SetMarc(string strMarc)
        {
            this.easyMarcControl1.SetMarc(strMarc);

            if (this.HideFieldNames.Count > 0)
            {
                // 将指定字段名的字段改变隐藏状态
                // parameters:
                //      field_names 要施加影响的字段名。如果为 null 表示全部
                this.easyMarcControl1.HideFields(this.HideFieldNames, true);
            }
        }

        public string GetMarc()
        {
            return this.easyMarcControl1.GetMarc();
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
            if (this.easyMarcControl1.InvokeRequired)
            {
                Delegate_SetEditData d = new Delegate_SetEditData(SetEditData);
                object[] args = new object[5];
                args[0] = edit;
                args[1] = strXml;
                args[2] = strRecPath;
                args[3] = timestamp;
                args[4] = "";
                int result = (int)this.easyMarcControl1.Invoke(d, args);

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
            if (this.easyMarcControl1.InvokeRequired)
            {
                Delegate_GetEditData d = new Delegate_GetEditData(GetEditData);
                object[] args = new object[4];
                args[0] = edit;
                args[1] = strBiblioRecPath;
                args[2] = "";
                args[3] = "";
                int result = (int)this.easyMarcControl1.Invoke(d, args);

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
                strError = "书目记录路径 '" + strBiblioRecPath + "' 中的记录 ID 部分格式错误";
                return -1;
            }
            edit.ParentId = strParentID;
            return edit.GetData(true, out strXml, out strError);
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

#if NO
        // 
        /// <summary>
        /// (如果必要，将)册信息部分显示为空
        /// </summary>
        /// <param name="strStyle">not_initial/none</param>
        public void TrySetBlank(string strStyle)
        {
            if (this.easyMarcControl1.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.easyMarcControl1.Invoke(new Action<string>(TrySetBlank), strStyle);
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
                        strFontName = this.Owner.Font.Name;

                    label.Font = new Font(strFontName, this.Owner.Font.Size * 2, FontStyle.Bold);
                    label.ForeColor = this.flowLayoutPanel1.ForeColor;  // SystemColors.GrayText;


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
#endif

#if NO
        public void AddPlus()
        {
            Label label_plus = new Label();
            string strFontName = "";
            Font ref_font = GuiUtil.GetDefaultFont();
            if (ref_font != null)
                strFontName = ref_font.Name;
            else
                strFontName = this.Owner.Font.Name;

            label_plus.Font = new Font(strFontName, this.Owner.Font.Size * 8, FontStyle.Bold);  // 12
            label_plus.ForeColor = this.flowLayoutPanel1.ForeColor;  // SystemColors.GrayText;

            // label_plus.AutoSize = true;
            // label_plus.Text = "+";
            label_plus.Size = new Size(100, 100);
            label_plus.AutoSize = false;
            label_plus.Margin = new Padding(8, 8, 8, 8);

            this.flowLayoutPanel1.Controls.Add(label_plus);

            label_plus.BackColor = ControlPaint.Dark(this.flowLayoutPanel1.BackColor);
            label_plus.TextAlign = ContentAlignment.MiddleCenter;

            AddPlusEvents(label_plus, true);
        }
#endif
        public void AddPlus()
        {
            PlusButton button_plus = new PlusButton();
#if NO
            string strFontName = "";
            Font ref_font = GuiUtil.GetDefaultFont();
            if (ref_font != null)
                strFontName = ref_font.Name;
            else
                strFontName = this.Owner.Font.Name;

            button_plus.Font = new Font(strFontName, this.Owner.Font.Size * 8, FontStyle.Bold);  // 12
#endif

#if NO
            button_plus.ForeColor = this.flowLayoutPanel1.ForeColor;  // SystemColors.GrayText;
            button_plus.BackColor = _entityBackColor;
#endif
            SetControlColor(button_plus);

            // label_plus.AutoSize = true;
            // label_plus.Text = "+";
            button_plus.Size = new Size(100, 100);
            button_plus.AutoSize = false;
            button_plus.Margin = new Padding(8, 8, 8, 8);
            button_plus.FlatStyle = FlatStyle.Flat;

            this.flowLayoutPanel1.Controls.Add(button_plus);

            // button_plus.TextAlign = ContentAlignment.MiddleCenter;

            AddPlusEvents(button_plus, true);
        }

        void AddPlusEvents(PlusButton button_plus, bool bAdd)
        {
            if (bAdd)
            {
                button_plus.Click += button_plus_Click;
                button_plus.MouseUp += button_plus_MouseUp;
                //button_plus.Paint += button_plus_Paint;
                button_plus.Enter += button_plus_Enter;
            }
            else
            {
                button_plus.Click -= button_plus_Click;
                button_plus.MouseUp -= button_plus_MouseUp;
                //button_plus.Paint += button_plus_Paint;
                button_plus.Enter -= button_plus_Enter;
            }
        }

        void button_plus_Enter(object sender, EventArgs e)
        {
            if (this.EntitySelectionChanged != null)
                this.EntitySelectionChanged(sender, new EventArgs());
        }

        void button_plus_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = AddNewEntity("", true, out strError);
            if (nRet == -1)
                MessageBox.Show(this.Owner, strError);

            // 把加号滚入视野可见范围，这样便于连续点击它
            this.ScrollPlusIntoView();
        }

#if NO
        void button_plus_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            // e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Button button = sender as Button;

            if (button.Focused == true)
            {
                using (Brush brush = new SolidBrush(SystemColors.Highlight))
                {
                    e.Graphics.FillRectangle(brush, button.ClientRectangle);
                }
            }

            int x_unit = button.Width / 3;
            int y_unit = button.Height / 3;

            Color darker_color = ControlPaint.Dark(button.ForeColor);
            // 绘制一个十字形状
            using (Brush brush = new SolidBrush(button.ForeColor)) 
            {
                Rectangle rect = new Rectangle(x_unit, y_unit + y_unit/2 - y_unit / 8, x_unit, y_unit / 4);
                e.Graphics.FillRectangle(brush, rect);
                rect = new Rectangle(x_unit + x_unit/2 - x_unit / 8, y_unit, x_unit / 4, y_unit);
                e.Graphics.FillRectangle(brush, rect);
            }

            // 绘制一个圆圈
            using (Pen pen = new Pen(darker_color, x_unit / 8))
            {
                Rectangle rect = new Rectangle(x_unit / 2, y_unit / 2, button.Width - x_unit, button.Height - y_unit);
                e.Graphics.DrawArc(pen, rect, 0, 360);
            }
        }
#endif
        void button_plus_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            for (int i = 1; i <= 10; i++)
            {
                menuItem = new MenuItem("新增 " + i.ToString() + " 个册");
                menuItem.Tag = i;
                menuItem.Click += new System.EventHandler(this.menu_newMultipleEntities_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.Show(sender as Control, new Point(e.X, e.Y));
        }

        void menu_newMultipleEntities_Click(object sender, EventArgs e)
        {
            string strError = "";
            MenuItem menu = sender as MenuItem;
            int n = (int)menu.Tag;
            for (int i = 0; i < n; i++)
            {
                int nRet = AddNewEntity("", 
                    i == n - 1 ? true : false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this.Owner, strError);
        }

        public int AddNewEntity(string strItemBarcode,
            bool bAutoSetFocus,
            out string strError)
        {
            EntityEditControl control = null;
            return AddNewEntity(strItemBarcode, bAutoSetFocus, out control, out strError);
        }

        // 新添加一个实体事项
        public int AddNewEntity(string strItemBarcode,
            bool bAutoSetFocus,
            out EntityEditControl control,
            out string strError)
        {
            strError = "";
            control = null;
            int nRet = 0;

            string strQuickDefault = "<root />";
            if (this.GetEntityDefault != null)
            {
                GetDefaultItemEventArgs e = new GetDefaultItemEventArgs();
                this.GetEntityDefault(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    return -1;
                }
                strQuickDefault = e.Xml;
            }
            // 根据缺省值，构造最初的 XML
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strQuickDefault);
            }
            catch (Exception ex)
            {
                strError = "缺省册记录装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement, "barcode", strItemBarcode);
            DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

            string strErrorInfo = "";
            // 兑现 @price 宏，如果有书目记录的话
            // TODO: 当书目记录切换的时候，是否还要重新兑现一次宏?

            // 记录路径可以保持为空，直到保存前才构造
            nRet = ReplaceEntityMacro(dom,
                out strError);
            if (nRet == -1)
            {
                // return -1;
                strErrorInfo = strError;
            }
            // 添加一个新的册对象
            nRet = NewEntity(dom.DocumentElement.OuterXml,
                bAutoSetFocus,
                out control,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strErrorInfo) == false)
                control.ErrorInfo = strErrorInfo;

            return 0;
        }

        // 添加一个新的册对象
        public int NewEntity(string strXml,
            bool bAutoSetFocus,
            out EntityEditControl control,
            out string strError)
        {
            return this.NewEntity("",
                null,
                strXml,
                true,
                bAutoSetFocus,
                out control,
                out strError);
        }

    }


    public class BiblioError
    {
        public string FieldName = "";
        public int FieldDupIndex = 0;   // (记录内)同名字段下标
        public string SubfieldName = "";
        public int SubfieldDupIndex = 0;    // (所在字段内)同名子字段下标
        public string ErrorInfo = "";

        public BiblioError(string strErrorInfo)
        {
            this.ErrorInfo = strErrorInfo;
        }

        public BiblioError(string strFieldName,
            string strSubfieldName,
            string strErrorInfo)
        {
            this.FieldName = strFieldName;
            this.SubfieldName = strSubfieldName;
            this.ErrorInfo = strErrorInfo;
        }

        public override string ToString()
        {
            return this.ErrorInfo;
        }

        public static string GetListString(List<BiblioError> list, string strSep)
        {
            StringBuilder result = new StringBuilder();
            foreach(BiblioError error in list)
            {
                result.Append(error.ToString() + strSep);
            }

            return result.ToString();
        }

        // 返回 errors 中的涉及到的字段超过 name_list 的范围的部分列表
        public static List<string> GetOutOfRangeFieldNames(List<BiblioError> errors, List<string> name_list)
        {
            List<string> results = new List<string>();
            foreach (BiblioError error in errors)
            {
                if (string.IsNullOrEmpty(error.FieldName) == true)
                    continue;

                // TODO: error.FieldName 内容，可以扩展为允许 xxx,xxx 这样的形态
                if (EasyMarcControl.MatchFieldName(error.FieldName, name_list) == false)
                    results.Add(error.FieldName);
            }
            StringUtil.RemoveDup(ref results);  // 对结果集合中的名字去重
            return results;
        }
    }

    /// <summary>
    /// 获得缺省册记录事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetDefaultItemEventHandler(object sender,
    GetDefaultItemEventArgs e);

    /// <summary>
    /// 获得缺省册记录事件的参数
    /// </summary>
    public class GetDefaultItemEventArgs : EventArgs
    {
        public string Xml = "";         // [out]
        public string ErrorInfo = "";   // [out]
    }
}
#pragma warning restore 1591