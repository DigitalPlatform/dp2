
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Drawing;
using DigitalPlatform.EasyMarc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 接受键盘输入，引导操作流程的浮动小窗口
    /// </summary>
    public partial class KeyboardForm : Form
    {
        public EntityRegisterWizard BaseForm = null;

        LineLayerForm _layer = null;

        public KeyboardForm()
        {
            InitializeComponent();

            this.webBrowser_info.Size = new System.Drawing.Size(200, this.webBrowser_info.Size.Height);
            this.panel_barcode.Size = new System.Drawing.Size(200, this.panel_barcode.Size.Height);

            OpenLayerForm();

            this.MinimizeBox = this.MaximizeBox = false;
#if NO
            this.TransparencyKey = this.BackColor;  // = Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
#endif

            // http://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
            this.toolStrip1.Renderer = new TransparentToolStripRenderer(this.toolStrip1);
        }

        private void KeyboardForm_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            UpdateRectSource();

            this.textBox_input.BackColor = this.BackColor;
            this.textBox_input.ForeColor = this.ForeColor;

            {
                this._layer.Show(this.BaseForm.TopLevelControl);    // Show() 的时候，先前设好的 Location 可能会变动。所以 Show() 以后设置较好
                // Rectangle rect = Screen.PrimaryScreen.Bounds;
                Rectangle rect = Screen.GetWorkingArea(this);
                this._layer.Size = rect.Size;
                this._layer.Location = rect.Location;
            }

            this.BeginInvoke(new Func<Step, string, bool>(SetStep), Step.PrepareSearchBiblio, "");
        }

        // 更新连线的源控件矩形
        void UpdateRectSource()
        {
            // this._layer.SourceRect = this.RectangleToScreen(this.DesktopBounds);
            if (this._layer != null)
            {
                if (this.Docked == false)
                    this._layer.SourceRect = this.DesktopBounds;
                else
                    this._layer.SourceRect = this.tableLayoutPanel1.RectangleToScreen(new Rectangle(new Point(0, 0), this.tableLayoutPanel1.Size));
            }
        }

        string _targetName = "";    // 当前连线的目标控件名字

        // 更新连线的目标控件矩形
        public void UpdateRectTarget()
        {
            if (this._layer != null && this.BaseForm != null)
                this._layer.TargetRect = this.BaseForm.GetRect(_targetName);
        }

        // 设置连线的目标
        void SetTarget(string strTargetName)
        {
            if (strTargetName == "None")
                this._layer.TargetRect = new Rectangle(0, 0, 0, 0);
            else
            {
                _targetName = strTargetName;
                this._layer.TargetRect = this.BaseForm.GetRect(_targetName);
            }
        }

        Color ErrorBackColor = Color.DarkRed;

        public void SetColorStyle(string strStyle)
        {
            if (strStyle == "dark")
            {
                this.BackColor = Color.DimGray;
                this.ForeColor = Color.White;
                this.ErrorBackColor = Color.DarkRed;

                this.toolStrip1.BackColor = Color.FromArgb(70, 70, 70); // 50

                this.textBox_input.BackColor = Color.Black;
            }

            if (strStyle == "light")
            {
                this.BackColor = SystemColors.Window;
                this.ForeColor = SystemColors.WindowText;
                this.ErrorBackColor = Color.Red;

                this.toolStrip1.BackColor = this.BackColor;

                this.textBox_input.BackColor = this.BackColor;
            }

            this.tableLayoutPanel1.BackColor = this.BackColor;
            this.tableLayoutPanel1.ForeColor = this.ForeColor;

            this.toolStrip1.ForeColor = this.ForeColor;
            foreach (ToolStripItem item in this.toolStrip1.Items)
            {
                item.BackColor = this.toolStrip1.BackColor;
                item.ForeColor = this.ForeColor;
            }

            this.textBox_input.ForeColor = this.ForeColor;

            RefreshHtmlDisplay();
        }

        private void KeyboardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseLayerForm();
        }

        void OpenLayerForm()
        {
            if (this._layer == null)
            {
                this._layer = new LineLayerForm();
                // this._layer.WindowState = FormWindowState.Maximized;
                // Rectangle rect = Screen.GetWorkingArea(this);

                this._layer.TopMost = true;
            }
        }

        void CloseLayerForm()
        {
            if (this._layer != null)
            {
                this._layer.Close();
                this._layer = null;
            }
        }
#if NO
        /// <summary>
        /// 返回结果: System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                cp.ExStyle |= 0x80000;  // WS_EX_LAYERED
                return cp;
            }
        }
#endif

#if NO
        // search_biblio edit_biblio input_entity
        string _step = "";
        string _subStep = "";
#endif
        // 流程步骤
        public enum Step
        {
            None = 0,
            PrepareSearchBiblio = 1,    // 从头开始，准备检索书目
            SearchingBiblio = 2,    // 正在检索书目。这是一个不停留的状态
            SearchBiblioCompleted = 3,  // 检索书目完成。这是一个不停留的状态
            SearchBiblioError = 4,  // 检索书目出错
            SelectSearchResult = 5, // 检索书目命中多条。若此时输入原 ISBN 则将当前已经选定行(通常是第一行)装入编辑
            SearchBiblioNotFound = 6,   // 检索书目后没有命中。若此时输入原 ISBN 则创建一条新的书目记录
            EditBiblio = 7,
            EditEntity = 8,
        }

        Step _step = Step.None;

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
            this.SetStep(Step.PrepareSearchBiblio);
        }

        EasyLine _currenBiblioLine = null;

        public Step GetStep()
        {
            return this._step;
        }

        // return:
        //      false   出错
        //      true    成功
        public bool SetStep(Step step, string strStyle = "")
        {
            int nRet = 0;

            Step save = this._step;

        REDO_SETSTEP:

            this._step = step;

            this.DisplayInfo(Step.None);

            if (step == Step.None)
            {
                SetTarget("None");
                this.EnableInput(false);
                return true;
            }

            // 准备检索书目
            if (step == Step.PrepareSearchBiblio)
            {
                this.label_name.Text = "请输入检索词";

                this._currenBiblioLine = null;
                this._currentEntityControl = null;
                this._currentEntityLine = null;

                this.EnableInput(true);

                if (StringUtil.IsInList("dont_clear", strStyle) == false)
                {
                    this.textBox_input.Text = "";

                    // 操作前要确保各种修改已经自动保存，以免出现提示保存的对话框导致破坏流畅性
                    // return:
                    //      -1      保存过程出错
                    //      0       成功
                    nRet = this.BaseForm.ReStart();
                    if (nRet == -1)
                    {
                        this.DisplayError("操作出错");
                        this._step = save;
                        return false;
                    }
                }

                if (StringUtil.IsInList("dont_hilight", strStyle) == false)
                {
                    this.EnableInput(true);

                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus(); // 将输入焦点切换回来
                }
                SetTarget("QueryWord");
                DisplayInfo(step);
                return true;
            }

            if (step == Step.SearchingBiblio)
            {
                this.SetTarget("ResultList");
                DisplayInfo(step);
                return true;
            }

            if (step == Step.SearchBiblioCompleted)
            {
                this.EnableInput(true);

                this.SetTarget("ResultList");
                DisplayInfo(step);
                return true;
            }

            if (step == Step.SearchBiblioError)
            {
                this.SetTarget("ResultList");
                this.DisplayInfo(Step.SearchBiblioError);
                return true;
            }

            if (step == Step.SearchBiblioNotFound)
            {
                this.EnableInput(true);

                this.SetTarget("ResultList");
                this.DisplayInfo(Step.SearchBiblioNotFound);
                return true;
            }

            // 在书目命中多条结果中选择
            if (step == Step.SelectSearchResult)
            {
                this.label_name.Text = "请输入命中事项的序号";

                this.EnableInput(true);

                this.textBox_input.SelectAll();
                this.SetTarget("ResultList");
                DisplayInfo(step);
                return true;
            }

            // 编辑书目记录
            if (step == Step.EditBiblio)
            {
                this.textBox_input.SelectAll();
                this.SetTarget("MarcEdit");

                if (StringUtil.IsInList("dont_hilight", strStyle) == false)
                {
#if NO
                EasyLine start_line = _currenBiblioLine;
            REDO:
                EasyLine line = this.BaseForm.GetNextBiblioLine(start_line);
                if (line != null && string.IsNullOrEmpty(line.Content) == false && line.Content.IndexOf("?") == -1)
                {
                    start_line = line;
                    goto REDO;  // 直到找到一个内容为空或者有问号的字段
                }
#endif
                    EasyLine line = GetNextBiblioLine(_currenBiblioLine);
                    if (line != null)
                    {
#if NO
                        string strCaption = line.Caption;
                        if (line is SubfieldLine)
                        {
                            strCaption = line.Caption + " - " + line.Container.GetFieldLine(line as SubfieldLine).Caption;
                        }
                        this.label_name.Text = strCaption;
                        this.textBox_input.Text = line.Content;
                        line.Container.SelectItem(line, true, false);
                        line.EnsureVisible();
#endif
                        LinkBiblioField(line);
                        this.EnableInput(true);
                    }
                    else
                    {
                        // 全部字段都已经有内容了，要转为编辑册记录
                        if (_currenBiblioLine != null)
                        {
                            this.BaseForm.DisableWizardControl();
                            _currenBiblioLine.Container.SelectItem(null, true, false);
                            this.BaseForm.EnableWizardControl();
                        }

                        _currenBiblioLine = null;
                        this.textBox_input.Text = "";
                        // this.BeginInvoke(new Action<Step, string>(SetStep), Step.EditEntity, "");
                        step = Step.EditEntity;
                        goto REDO_SETSTEP;
                    }
                    _currenBiblioLine = line;
                }
                else
                {
                    // 定位到当前字段上
                    if (this._currenBiblioLine != null)
                    {
                        LinkBiblioField(this._currenBiblioLine);
                        this.EnableInput(true);
                    }
                    else
                    {
                        this.textBox_input.Text = "";
                        this.EnableInput(false);
                    }
                }

                DisplayInfo(step);
                return true;
            }

            // 增加新的册记录
            if (step == Step.EditEntity)
            {
                this.label_name.Text = "册条码号";
                this._currenBiblioLine = null;

                this.EnableInput(true);

                if (this._currentEntityControl == null)
                {
                    this.textBox_input.Text = "";
                    PlusButton plus = this.BaseForm.GetEntityPlusButton();
                    if (plus != null)
                    {
                        this.BaseForm.EnsurePlusButtonVisible();

                        this.BaseForm.DisableWizardControl();
                        plus.Active = true;
                        this.BaseForm.EnableWizardControl();
                    }
                    else
                    {
                        ////
                        ////Debug.Assert(false, "");
                    }
                    DisplayInfo(step);

                    this.textBox_input.SelectAll();
                    this.SetTarget("Entities");

                    return true;
                }

                this.textBox_input.SelectAll();
                this.SetTarget("Entities");
                if (StringUtil.IsInList("dont_hilight", strStyle) == false)
                {

                    if (this._currentEntityControl != null)
                    {
#if NO
                    EditLine start_line = this._currentEntityLine;
                REDO:
                    EditLine line = this.BaseForm.GetNextEntityLine(this._currentEntityControl, start_line);
#endif
                        EditLine line = GetNextEntityLine(this._currentEntityLine);
                        if (line != null)
                        {
#if NO
                        this.label_name.Text = line.Caption;
                        this.textBox_input.Text = line.Content;

                        this._currentValueList = this._currentEntityControl.GetLineValueList(line);

                        line.ActiveState = true;
                        this.BaseForm.EnsureEntityVisible(line.EditControl);
                        // 把加号按钮修改为一般状态
                        PlusButton plus = this.BaseForm.GetEntityPlusButton();
                        if (plus != null)
                        {
                            plus.Active = false;
                        }
#endif
                            LinkEntityLine(line);
                            this.EnableInput(true);
                        }
                        else
                        {
                            if (this._currentEntityControl != null)
                            {
                                this.BaseForm.DisableWizardControl();
                                this._currentEntityControl.ClearActiveState();
                                this.BaseForm.EnableWizardControl();
                            }
                            this._currentEntityControl = null;
                            this._currentEntityLine = null;
                            this.textBox_input.Text = "";
                            // this.BeginInvoke(new Action<Step, string>(SetStep), Step.EditEntity, "");
                            step = Step.EditEntity;
                            goto REDO_SETSTEP;
                        }
                        _currentEntityLine = line;
                    }
                }
                else
                {
                    // 定位到当前字段上
                    if (this._currentEntityControl != null
                        && this._currentEntityLine != null)
                    {
                        LinkEntityLine(this._currentEntityLine);
                        this.EnableInput(true);
                    }
                    else
                    {
                        this.textBox_input.Text = "";
                        this.EnableInput(true); // 准备新增一条册记录
                    }
                }

                DisplayInfo(step);
                return true;
            }

            return true;
        }

        void LinkEntityLine(EditLine line)
        {
            if (line == null)
                return;

            this.BaseForm.DisableWizardControl();
            {
                this.label_name.Text = line.Caption;
                this.textBox_input.Text = line.Content;

                this._currentValueList = this._currentEntityControl.GetLineValueList(line);

                line.ActiveState = true;
                this.BaseForm.EnsureEntityVisible(line.EditControl);
                // 把加号按钮修改为一般状态
                PlusButton plus = this.BaseForm.GetEntityPlusButton();
                if (plus != null)
                {
                    plus.Active = false;
                }
            }
            this.BaseForm.EnableWizardControl();

        }

        void LinkBiblioField(EasyLine line)
        {
            if (line == null)
                return;

            string strCaption = line.Caption;
            if (line is SubfieldLine)
            {
                strCaption = line.Caption + " - " + line.Container.GetFieldLine(line as SubfieldLine).Caption;
            }
            this.label_name.Text = strCaption;
            this.textBox_input.Text = line.Content;

            this.BaseForm.DisableWizardControl();
            line.Container.SelectItem(line, true, false);
            this.BaseForm.EnableWizardControl();

            line.EnsureVisible();
        }

        void EnableInput(bool bEnable)
        {
            if (bEnable != this.panel_barcode.Visible)
            {
                this.panel_barcode.Visible = bEnable;
            }
        }

        public void SetCurrentEntityLine(EntityEditControl control, EditLine line)
        {
            this._currentEntityControl = control;
            this._currentEntityLine = line;
        }

        public void SetCurrentBiblioLine(EasyLine line)
        {
            this._currenBiblioLine = line;
        }

        EasyLine GetPrevBiblioLine(EasyLine start_line)
        {
            return this.BaseForm.GetPrevBiblioLine(start_line);
        }

        EasyLine GetNextBiblioLine(EasyLine start_line, bool bFindEmpty = true)
        {
        REDO:
            EasyLine line = this.BaseForm.GetNextBiblioLine(start_line);
            if (bFindEmpty == true)
            {
                if (line != null && string.IsNullOrEmpty(line.Content) == false && line.Content.IndexOf("?") == -1)
                {
                    start_line = line;
                    goto REDO;  // 直到找到一个内容为空或者有问号的字段
                }
            }
            return line;
        }

        EditLine GetNextEntityLine(EditLine start_line)
        {
            return EntityRegisterWizard.GetNextEntityLine(this._currentEntityControl, start_line);
        }

        string _lastHtml = "";  // 上一次显示过的 HTML

        void RefreshHtmlDisplay()
        {
            if (string.IsNullOrEmpty(_lastHtml) == false)
            {
                string strHtml = _lastHtml;
                ReplaceMacro(ref strHtml);
                Global.SetHtmlString(webBrowser_info,
                    strHtml,
                    this.BaseForm.MainForm.DataDir,
                    "kbd");
            }
        }

        void DisplayError(string strError)
        {
            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
+ "<style media='screen' type='text/css'>"
+ "body { font-family:Microsoft YaHei; background-color:%errorbackcolor%; color:%forecolor%; } "
+ "</style>"
+ "</head><body>";
            string strTail = "</body></html>";

            string strHtml = strHead
    + "<h2>" + strError + "</h2>"
    + strTail;

            if (strHtml == _lastHtml)
                return;
            _lastHtml = strHtml;

            ReplaceMacro(ref strHtml);
            Global.SetHtmlString(webBrowser_info,
                strHtml,
                this.BaseForm.MainForm.DataDir,
                "kbd");
        }

        void DisplayInfo(Step step)
        {
            string strHtml = "";

            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
+ "<style media='screen' type='text/css'>"
+ "body { font-family:Microsoft YaHei; background-color:%backcolor%; color:%forecolor%; } "
+ "</style>"
+ "</head><body>";
            string strTail = "</body></html>";

            if (step == Step.None)
            {
                // 清除显示
                strHtml = "";
            }
            else if (step == Step.PrepareSearchBiblio)
            {
                strHtml = strHead
                    + "<h1>开始：检索书目信息</h1>"
                    + "<li>输入 ISBN</li>"
                    + strTail;
            }
            else if (step == Step.SearchingBiblio)
            {
                string strGifFileName = Path.Combine(this.BaseForm.MainForm.DataDir, "ajax-loader3.gif");
                strHtml = strHead
                    + "<h2 align='center'><img src='" + strGifFileName + "' /></h2>"
                    + "<h2 align='center'>正在检索书目，请等待 ...</h2>"
                    + strTail;
            }
            else if (step == Step.SearchBiblioCompleted)
            {
                strHtml = strHead
                    + "<h2>检索完成</h2>"
                    + strTail;
            }
            else if (step == Step.SelectSearchResult)
            {
                strHtml = strHead
                    + "<h1>挑选书目</h1>"
                    + "<li>输入序号数字，挑选一个命中记录进行编辑</li>"
                    + "<li>若输入刚才用过的检索词，则可编辑已选定的一个命中记录</li>"
                    + strTail;
            }
            else if (step == Step.SearchBiblioError)
            {
                strHtml = strHead
                    + "<h2>检索过程出现错误</h2>"
                    + "<li>输入其它检索词，可重新检索书目</li>"
                    + strTail;
            }
            else if (step == Step.SearchBiblioNotFound)
            {
                strHtml = strHead
                    + "<h2>检索没有命中</h2>"
                    + "<li>输入其它检索词，可重新检索书目</li>"
                    + "<li>输入刚才用过的检索词，则创建新书目记录</li>"
                    + strTail;
            }
            else if (step == Step.EditEntity)
            {
                if (this._currentEntityControl == null)
                {
                    strHtml = strHead
                        + "<h1>增加新册</h1>"
                        + "<li>输入册条码号，则可新添一个册记录</li>"
                        + "<li>若输入刚才用过的检索词，则可新添一个没有册条码号的册记录</li>"
                        + strTail;
                }
                else
                {
                    strHtml = strHead
    + "<h1>为新册输入字段内容</h1>"
    + "<li>请按照提示输入</li>"
    + strTail;
                }
            }
            else if (step == Step.EditBiblio)
            {
                strHtml = strHead
                    + "<h1>编辑书目记录</h1>"
                    + "<li>请按照提示输入字段内容</li>"
                    + "<li>输入刚才用过的检索词，则可新添一个册记录</li>"
                    + strTail;
            }

            // webBrowser_info.DocumentText = strHtml;
            if (string.IsNullOrEmpty(strHtml) == true)
            {
                // Global.ClearHtmlPage(webBrowser_info, this.BaseForm.MainForm.DataDir);
                string strImageUrl = Path.Combine(this.BaseForm.MainForm.DataDir, "page_blank_128.png");
                strHtml = strHead
                    + "<img src='" + strImageUrl + "' width='64' height='64' alt='空'>"
                    + strTail;
            }

            if (strHtml == _lastHtml)
                return;

            _lastHtml = strHtml;

            ReplaceMacro(ref strHtml);
            Global.SetHtmlString(webBrowser_info,
                strHtml,
                this.BaseForm.MainForm.DataDir,
                "kbd");
        }

        void ReplaceMacro(ref string strHtml)
        {
            strHtml = strHtml.Replace("%backcolor%", ColorUtil.Color2String(this.BackColor))
                .Replace("%forecolor%", ColorUtil.Color2String(this.ForeColor))
                .Replace("%errorbackcolor%", ColorUtil.Color2String(this.ErrorBackColor));
        }

        // 包装函数，确保调用后焦点能回到本窗口
        void EditSelectedBrowseLine()
        {
            Control current_focused = (this.tableLayoutPanel1.TopLevelControl as Form).ActiveControl;

            this._layer.DisableUpdate();
            // TODO: 为了避免中途 Focus 被夺走导致的闪动，需要暂时禁止 layerForm 刷新
            this.BaseForm.EditSelectedBrowseLine(false);

            this._layer.EnableUpdate();

            if (current_focused != null)
                current_focused.Focus();
        }

        EntityEditControl _currentEntityControl = null;
        EditLine _currentEntityLine = null;
        List<string> _currentValueList = null;

        string _lastISBN = "";

        private async void button_scan_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.textBox_input.SelectAll();
            this.textBox_input.Focus();

            if (this._step == Step.PrepareSearchBiblio)
            {
                this.BaseForm.QueryWord = this.textBox_input.Text;
                this._lastISBN = this.textBox_input.Text;
                // 显示“正在检索”
                this.SetStep(Step.SearchingBiblio);
                await this.BaseForm.DoSearch(this.textBox_input.Text, "ISBN", false);
                this.SetStep(Step.SearchBiblioCompleted);
                // 如果检索命中，要选择后，切换到下一页。选择方面，可以提示输入 1 2 3 4 等用以选择
                // 如果检索没有命中，提示再次扫入相同的 ISBN，可以直接进入创建新记录流程
                if (this.BaseForm.SearchResultErrorCount > 0)
                {
                    this.SetStep(Step.SearchBiblioError);
                }
                else if (this.BaseForm.SearchResultCount > 0)
                {
                    if (this.BaseForm.SearchResultCount == 1)
                    {
                        // 命中唯一的一条
                        this.EditSelectedBrowseLine();
                        // 观察书目记录包含多少册记录。
                        this.SetStep(Step.EditEntity);
                        return;
                    }
                    this.textBox_input.Text = "1";
                    this.SetStep(Step.SelectSearchResult);
                }
                else
                {
                    Debug.Assert(this.BaseForm.SearchResultCount == 0, "");

                    // 没有命中。当再次输入同样的 ISBN, 表示创建新书目记录
                    this.SetStep(Step.SearchBiblioNotFound);
                }
                return;
            }

            if (this._step == Step.SelectSearchResult)
            {
                int index = 0;

                // 如果输入刚才用过的 ISBN ，表示选择第一个记录
                if (this.textBox_input.Text == this._lastISBN && string.IsNullOrEmpty(this._lastISBN) == false)
                {
                    index = 1;
                }
                else
                {
                    // 根据输入的数字挑选命中记录
                    if (StringUtil.IsPureNumber(this.textBox_input.Text) == false)
                    {
                        strError = "请输入一个数字";
                        goto ERROR1;
                    }

                    if (int.TryParse(this.textBox_input.Text, out index) == false)
                    {
                        strError = "输入的应该是一个数字";
                        goto ERROR1;
                    }

                    // TODO: 可以根据检索命中列表中事项的最大个数，探测 input 中输入达到这么多个字符后不需要按回车键就可以开始后续处理

                    if (index - 1 >= this.BaseForm.ResultListCount)
                    {
                        strError = "请输入一个 1-" + (this.BaseForm.ResultListCount + 1).ToString() + " 之间的数字";
                        goto ERROR1;
                    }
                }
                this.BaseForm.SelectBrowseLine(true, index - 1);
                this.EditSelectedBrowseLine();
                // 观察书目记录包含多少册记录。
                this.SetStep(Step.EditEntity);
                return;
            }

            if (this._step == Step.SearchBiblioNotFound)
            {
                // 如果输入刚才用过的 ISBN ，表示创建新书目记录
                if (this.textBox_input.Text == this._lastISBN && string.IsNullOrEmpty(this._lastISBN) == false)
                {
                    nRet = this.BaseForm.NewBiblio(false);
                    if (nRet == -1)
                    {
                        this.DisplayError("操作出错");
                        return;
                    }
                    this.SetStep(Step.EditBiblio);
                    return;
                }
                else
                {
                    string strText = this.textBox_input.Text;
                    this.SetStep(Step.PrepareSearchBiblio);
                    this.textBox_input.Text = strText;
                    this.BeginInvoke(new Action<object, EventArgs>(button_scan_Click), this, new EventArgs());
                    return;
                }
                //return;
            }

            if (this._step == Step.EditBiblio)
            {
                // 观察输入的字符串，如果和检索使用过的 ISBN 一致，则增加一个册(册条码号为空)；
                // 如果是其他 ISBN，则回到检索画面自动启动检索书目;
                // 如果不是 ISBN，则当作为当前书目字段输入内容

                string strText = this.textBox_input.Text;

                if (strText == this._lastISBN && string.IsNullOrEmpty(this._lastISBN) == false)
                {
                    // 将上一个 EntityEditControl 中的蓝色行标记清除
                    if (this._currentEntityControl != null)
                    {
                        this.BaseForm.DisableWizardControl();
                        this._currentEntityControl.ClearActiveState();
                        this.BaseForm.EnableWizardControl();
                    }
                    this._currentEntityControl = this.BaseForm.AddNewEntity("", false);
                    this._currentEntityLine = null;
#if NO
                    // 越过第一个字段
                    this._currentEntityLine = GetNextEntityLine(this._currentEntityLine);
#endif
                    return;
                }

                if (QuickChargingForm.IsISBN(ref strText) == true)
                {
                    this.SetStep(Step.PrepareSearchBiblio);
                    this.textBox_input.Text = strText;
                    this.BeginInvoke(new Action<object, EventArgs>(button_scan_Click), this, new EventArgs());
                    return;
                }
                else
                {
                    // 为当前子字段添加内容，顺次进入下一子字段。等所有字段都输入完以后，自动进入 EditEntity 状态
                    if (this._currenBiblioLine != null)
                    {
                        if (string.IsNullOrEmpty(this.textBox_input.Text) == true)
                        {
                            List<EasyLine> selected_lines = new List<EasyLine>();
                            selected_lines.Add(this._currenBiblioLine);
                            EasyMarcControl container = this._currenBiblioLine.Container;
                            // 删除前要先找到上一个行。否则删除后查找将无法继续
                            this._currenBiblioLine = GetPrevBiblioLine(this._currenBiblioLine);
                            // 删除
                            container.DeleteElements(selected_lines);
                        }
                        else
                        {
                            this._currenBiblioLine.Content = this.textBox_input.Text;
                            this.textBox_input.Text = "";
                        }
                        SetStep(Step.EditBiblio);
                    }
                    else
                    {
                        this.textBox_input.Text = "";
                        SetStep(Step.EditEntity);
                    }
                    return;
                }
            }

            if (this._step == Step.EditEntity)
            {
                // 观察输入的字符串，如果和检索使用过的 ISBN 一致，则增加一个册(册条码号为空)；
                // 如果是其他 ISBN，则回到检索画面自动启动检索书目;
                // 如果不是 ISBN，则当作册条码号增加一个册

                string strText = this.textBox_input.Text;

                if (strText == this._lastISBN && string.IsNullOrEmpty(this._lastISBN) == false)
                {
                    // 将上一个 EntityEditControl 中的蓝色行标记清除
                    if (this._currentEntityControl != null)
                    {
                        this.BaseForm.DisableWizardControl();
                        this._currentEntityControl.ClearActiveState();
                        this.BaseForm.EnableWizardControl();
                    }

                    this._currentEntityControl = this.BaseForm.AddNewEntity("", false);
                    this._currentEntityLine = null;
#if NO
                    // 越过第一个字段
                    this._currentEntityLine = GetNextEntityLine(this._currentEntityLine);
#endif
                    this.BeginInvoke(new Func<Step, string, bool>(SetStep), Step.EditEntity, "");
                    return;
                }

                if (QuickChargingForm.IsISBN(ref strText) == true)
                {
                    // TODO: 连带引起保存书目和册记录可能会失败哟
                    if (this.SetStep(Step.PrepareSearchBiblio) == false)
                        return;
                    this.textBox_input.Text = strText;
                    this.BeginInvoke(new Action<object, EventArgs>(button_scan_Click), this, new EventArgs());
                    return;
                }
                else
                {
                    // 首次输入册条码号。还没有关联的 EntityEditControl
                    if (this._currentEntityControl == null)
                    {
                        this._currentEntityControl = this.BaseForm.AddNewEntity(strText, false);
                        this._currentEntityLine = null;
                        // 越过第一个字段
                        this._currentEntityLine = GetNextEntityLine(this._currentEntityLine);

                        this.BeginInvoke(new Func<Step, string, bool>(SetStep), Step.EditEntity, "");
                        return;
                    }
                    // 为册编辑器当前行输入内容，顺次进入下一行。等所有字段都输入完以后，自动进入 EditEntity 等待输入册条码号的状态
                    if (this._currentEntityLine != null)
                    {
                        this._currentEntityLine.Content = this.textBox_input.Text;
                        this.textBox_input.Text = "";

                        SetStep(Step.EditEntity);
                    }
                    else
                    {
                        // 进入首次等待输入册条码号的状态
                        this._currentEntityControl = null;
                        this._currentEntityLine = null;
                        this.textBox_input.Text = "";
                        SetStep(Step.EditEntity);
                    }
                    return;
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void KeyboardForm_Activated(object sender, EventArgs e)
        {
            this.textBox_input.SelectAll();
            this.textBox_input.Focus();

            //this.Opacity = 1.0;

            if (this.Docked == false)
            {
#if NO
                if (this._layer != null)
                {
                    this._layer.PanelState = PanelState.HilightPanel;
                    this._layer.Transparent = false;
                }
#endif
                SetPanelState("panel");
                SetPanelState("display");
            }
        }

        private void KeyboardForm_Deactivate(object sender, EventArgs e)
        {
            //this.Opacity = 0.6; // 0.8

            if (this.Docked == false)
            {
#if NO
                if (this._layer != null)
                {
                    // this._layer.PanelState = PanelState.HilightForm;
                    this._layer.Transparent = true;
                }
#endif
                SetPanelState("form");
                SetPanelState("transparent");
            }
        }

        private void KeyboardForm_Paint(object sender, PaintEventArgs e)
        {
        }

        private void KeyboardForm_Move(object sender, EventArgs e)
        {
            UpdateRectSource();
        }

        private void KeyboardForm_Resize(object sender, EventArgs e)
        {
            ResetPanelBarcodeSize();
            UpdateRectSource();
        }

        void ResetPanelBarcodeSize()
        {
            int nWidth = this.tableLayoutPanel1.ClientSize.Width - this.tableLayoutPanel1.Padding.Horizontal - this.tableLayoutPanel1.Margin.Horizontal;
            this.webBrowser_info.Size = new System.Drawing.Size(nWidth, this.webBrowser_info.Size.Height);
            // this.panel_barcode.Size = new System.Drawing.Size(nWidth, this.panel_barcode.Size.Height);
            this.panel_barcode.Width = nWidth;
            this.button_scan.Location = new Point(this.textBox_input.Location.X + this.textBox_input.Width, this.textBox_input.Location.Y);
            // this.panel_barcode.BackColor = Color.Red;
            //this.tableLayoutPanel1.Width = this.ClientSize.Width;
        }

        // 从列表中选入上/下一个值
        void SelectFromList(List<string> list, bool bUp)
        {
            if (list == null)
                return;
            if (list.Count == 0)
                return;
            string strText = this.textBox_input.Text;
            // 在列表中定位
            int index = list.IndexOf(strText);
            if (index == -1)
                index = 0;
            else
            {
                if (bUp)
                    index--;
                else
                    index++;
            }
            if (index < 0 || index >= list.Count)
                return;
            this.textBox_input.Text = list[index];
        }

        private void textBox_input_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    button_scan_Click(sender, e);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Down:
                    SelectFromList(this._currentValueList, false);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    SelectFromList(this._currentValueList, true);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);
            UpdateRectSource();
        }

        #region 停靠相关

        bool _docked = false;

        public bool Docked
        {
            get
            {
                return _docked;
            }
            set
            {
                _docked = value;
                this.toolStripButton_dock.Checked = value;
            }
        }

        public TableLayoutPanel Table
        {
            get
            {
                return this.tableLayoutPanel1;
            }
        }

        /// <summary>
        /// 停靠
        /// </summary>
        public event DoDockEventHandler DoDockEvent = null;

        public void DoDock(bool bShowFixedPanel)
        {
            if (this.DoDockEvent != null)
            {
                DoDockEventArgs e = new DoDockEventArgs();
                e.ShowFixedPanel = bShowFixedPanel;
                this.DoDockEvent(this, e);

                this.tableLayoutPanel1.BackColor = Color.DimGray;
                this.tableLayoutPanel1.ForeColor = Color.White;
            }

            ResetPanelBarcodeSize();    // 2017/4/8
        }

        #endregion

        private void tableLayoutPanel1_Enter(object sender, EventArgs e)
        {
            if (this.Docked == true)
            {
#if NO
                if (this._layer != null)
                {
                    this._layer.PanelState = PanelState.HilightPanel;
                    this._layer.Transparent = false;
                }
#endif
                SetPanelState("panel");
                SetPanelState("display");
            }
        }

        private void tableLayoutPanel1_Leave(object sender, EventArgs e)
        {
            if (this.Docked == true)
            {
#if NO
                if (this._layer != null)
                {
                    // this._layer.PanelState = PanelState.HilightForm;
                    this._layer.Transparent = true;
                }
#endif
                SetPanelState("form");
                // SetPanelState("transparent");
            }
        }

#if NO
        bool IsMainFormActive()
        {
            if (API.GetForegroundWindow() == this.BaseForm.MainForm.Handle)
                return true;
            return false;
        }
#endif

        public void SetPanelState(string strState)
        {
            if (this._layer == null)
                return;

            if (strState == "transparent")
            {
                // this._layer.PanelState = PanelState.HilightForm;
                this._layer.Transparent = true;
            }
            else if (strState == "display")
            {
                this._layer.Transparent = false;
            }
            else if (strState == "panel")
            {
                this._layer.PanelState = PanelState.HilightPanel;
                this._layer.Transparent = false;
            }
            else if (strState == "form")
            {
                this._layer.PanelState = PanelState.HilightForm;
                this._layer.Transparent = false;
            }
            else
                throw new ArgumentException("未知的 strState 值 '" + strState + "'", "strState");
        }

#if NO
        public bool HighlightPanel
        {
            get
            {
                if (this._layer == null)
                    return false;
                return this._layer.HighlightPanel;
            }
            set
            {
                if (this._layer == null)
                    return;
                this._layer.HighlightPanel = value;
            }
        }
#endif

        private void tableLayoutPanel1_Move(object sender, EventArgs e)
        {
            if (this.Docked == true)
                UpdateRectSource();
        }

        private void tableLayoutPanel1_Resize(object sender, EventArgs e)
        {
            if (this.Docked == true)
                UpdateRectSource();
        }

        private void tableLayoutPanel1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Docked == true)
                UpdateRectSource();
            if (this._layer != null)
            {
                if (this.tableLayoutPanel1.Visible == false)
                    this._layer.Transparent = true;
                else
                    this._layer.Transparent = false;
            }
        }
    }

#if NO
    class MyRenderer : ToolStripSystemRenderer
    {
        public ToolStrip ToolStrip = null;

        public MyRenderer(ToolStrip toolstrip)
        {
            this.ToolStrip = toolstrip;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(this.ToolStrip == null ? e.BackColor : this.ToolStrip.BackColor))  // SystemColors.ControlDark
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
            // base.OnRenderToolStripBackground(e);
        }

        // 去掉下面那根线
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
#endif
}
