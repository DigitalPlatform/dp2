using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform.GUI;
// using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Marc
{
    internal class MyEdit : TextBox
    {
#if BIDI_SUPPORT
        const int WM_ADJUST_CARET = API.WM_USER + 200;
#endif
        public bool ContentIsNull = false;

        public int DisableFlush = 0;    // 临时禁止flush
        MarcEditor m_marcEditor = null;

        //是否是替换状态
        public bool Overwrite = false;

        public int nInMenu = 0;

        public bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        /*
        protected override void CreateHandle()
        {
            this.TextChanged -= new EventHandler(MyEdit_TextChanged);
            this.TextChanged += new EventHandler(MyEdit_TextChanged);
        }*/

        void MyEdit_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }
            set
            {
                this.DisableFlush++;
                base.Multiline = value;
                this.DisableFlush--;
            }
        }

        MarcEditor MarcEditor
        {
            get
            {
                if (this.m_marcEditor == null)
                    this.m_marcEditor = (MarcEditor)this.Parent;

                return this.m_marcEditor;
            }
        }

        // 获得有关插入符所在当前子字段的信息
        // return:
        //      0   不在子字段上
        //      1   在子字段上
        public static int GetCurrentSubfieldCaretInfo(
            string strFieldValue,
            int nCaretPos,
            out string strSubfieldName,
            out string strSufieldContent,
            out int nStart,
            out int nContentStart,
            out int nContentLength)
        {
            nStart = 0;
            nContentStart = 0;
            nContentLength = 0;
            strSubfieldName = "";
            strSufieldContent = "";

            strFieldValue = strFieldValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);

            bool bFoundPrevDollar = false;

            int nEnd = strFieldValue.Length;

            // 向左找到$符号
            for (int i = nCaretPos; i >= 0; i--)
            {
                if (nCaretPos > strFieldValue.Length - 1)
                    continue;

                char ch = strFieldValue[i];

                // 是子字段符号
                if (ch == Record.SUBFLD)
                {
                    bFoundPrevDollar = true;
                    nStart = i;
                    break;
                }
            }

            // 向右找到$符号
            for (int i = nCaretPos + 1; i < strFieldValue.Length; i++)
            {
                char ch = strFieldValue[i];

                // 是子字段符号
                if (ch == Record.SUBFLD)
                {
                    nEnd = i;
                    break;
                }
            }

            if (bFoundPrevDollar == true)
            {
                nContentStart = nStart + 2;
                if (nContentStart > nEnd)
                    nContentStart = nEnd;
                strSubfieldName = strFieldValue.Substring(nStart, 1);
            }

            nContentLength = nEnd - nContentStart;
            strSufieldContent = strFieldValue.Substring(nContentStart, nContentLength);

            return 1;
        }

        // return:
        //      false   需要执行缺省窗口过程
        //      true    不要执行缺省窗口过程。即消息接管了。
        bool DoDoubleClick()
        {
            // OnMouseDoubleClick那里接管不行。因为那里edit原有动作已经执行,this.SelectionStart值已经被破坏

            if (this.MarcEditor.m_nFocusCol != 3)
                return false;

            string strSubfieldName = "";
            string strSufieldContent = "";
            int nStart = 0;
            int nContentStart = 0;
            int nContentLength = 0;

            // 获得有关插入符所在当前子字段的信息
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            int nRet = GetCurrentSubfieldCaretInfo(
                this.Text,
                this.SelectionStart,
                out strSubfieldName,
                out strSufieldContent,
                out nStart,
                out nContentStart,
                out nContentLength);
            if (nRet == 0)
                return false;

            this.SelectionStart = nContentStart;
            this.SelectionLength = nContentLength;
            return true;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {

            switch (m.Msg)
            {
#if BIDI_SUPPORT
                case WM_ADJUST_CARET:
                    // 插入符避免处在方向符号和子字段符号(31)之间
                    if (this.SelectionLength == 0)
                    {
                        if (IsInForbiddenPos(this.SelectionStart) == true
                            && this.SelectionStart > 0)
                            this.SelectionStart--;
                    }
                    return;
#endif

                    // 禁止edit本身的菜单
                case API.WM_RBUTTONDOWN:
                case API.WM_RBUTTONUP:
                    {
                        if (nInMenu > 0)
                            return;
                    }
                    break;
                case API.WM_LBUTTONDOWN:
                    {
                        uint nCurClickTime = API.GetMessageTime();
                        uint nDelta = nCurClickTime - this.MarcEditor.m_nLastClickTime;

                        

                        int x = (int)((uint)m.LParam & 0x0000ffff);
                        int y = (int)(((uint)m.LParam >> 16) & 0x0000ffff);

                        bool bIn = true;
                        try
                        {
                            if (this.MarcEditor.m_rectLastClick.Contains(x + this.Location.X, y + this.Location.Y) == false)
                                bIn = false;
                        }
                        catch   // 2006/11/6 add
                        {
                        }



                        this.MarcEditor.m_nLastClickTime = nCurClickTime;
                        this.MarcEditor.m_rectLastClick = new Rectangle((int)x + this.Location.X, (int)y + this.Location.Y, 0, 0);
                        try
                        {
                            this.MarcEditor.m_rectLastClick.Inflate(
                                SystemInformation.DoubleClickSize.Width / 2,
                                SystemInformation.DoubleClickSize.Height / 2);
                        }
                        catch  // 2006/11/6 add
                        { }

                        if (bIn == false)
                            break;


                        if (nDelta < API.GetDoubleClickTime())
                        {
                            // 等于双击
                            if (DoDoubleClick() == true)
                                return;
                        }
                    }
                    break;
                case API.WM_LBUTTONDBLCLK:
                    {
                        if (DoDoubleClick() == true)
                            return;
                    }
                    break;
            }

            /*
            try
            {
             * */

            base.DefWndProc(ref m);
            /*
            }
            catch { }
             * */
        }

        /*
        // 接管回车键，变成给后面新增字段
        // 可以改写到OnKeyPress中?
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case API.WM_CHAR:
                    {
                        int nKey =API.LoWord(m.WParam.ToInt32());
                        switch (nKey)
                        {
                            case (int)Keys.Enter:
                                {
                                    // 后插字段
                                    this.MarcEditor.InsertAfterFieldNoDlg();
                                    return;
                                }
                            case (int)Keys.Back:
                                {
                                    if (this.Overwrite == true)
                                    {
                                        int nOldSelectionStart = this.SelectionStart;
                                        if (nOldSelectionStart > 0)
                                        {
                                            this.Text = this.Text.Remove(nOldSelectionStart - 1, 1);

                                            this.Text = this.Text.Insert(nOldSelectionStart - 1, " ");
                                            this.SelectionStart = nOldSelectionStart - 1;
                                            return;
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    if (this.Overwrite == true)
                                    {
                                        if ((Control.ModifierKeys == Keys.Control)
                                            || Control.ModifierKeys == Keys.Shift
                                            || Control.ModifierKeys == Keys.Alt)
                                        {
                                            break;
                                        }
                                        int nOldSelectionStart = this.SelectionStart;
                                        if (nOldSelectionStart < this.Text.Length)
                                        {
                                            this.Text = this.Text.Remove(this.SelectionStart, 1);
                                            this.SelectionStart = nOldSelectionStart;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
            base.DefWndProc(ref m);
        }
         */


        // 接管Ctrl+各种键
        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 去掉Control/Shift/Alt 以后的纯净的键码
            // 2008/11/30 changed
            Keys pure_key = (keyData & (~(Keys.Control | Keys.Shift | Keys.Alt)));

            // Ctrl + M
            if (Control.ModifierKeys == Keys.Control
                && pure_key == Keys.Enter)
            {
                // 禁止插入回车换行
                return true;
            }

            // Ctrl + M
            if (Control.ModifierKeys == Keys.Control
                && pure_key == Keys.M)
            {
                MarcEditor.EditControlTextToItem();

                // 调模板
                this.MarcEditor.GetValueFromTemplate();
                return true;
            }

            // Ctrl + C
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.C)
            {
                this.Menu_Copy(null,null);
                return true;
            }

            // Ctrl + X
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.X)
            {
                this.Menu_Cut(null, null);
                return true;
            }

            // Ctrl + V
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.V)
            {
                this.Menu_Paste(null, null);
                return true;
            }

            /*
            // Ctrl + A 自动录入功能
            if ((keyData & Keys.Control) == Keys.Control
                && (keyData & Keys.A) == Keys.A)
            {
                if (this.m_marcEditor != null)
                {
                    GenerateDataEventArgs ea = new GenerateDataEventArgs();
                    this.m_marcEditor.OnGenerateData(ea);
                    return true;
                }
            }*/

            // Ctrl + A 自动录入功能
            // && (keyData & (~Keys.Control)) == Keys.A)   // 2007/5/15 修改，原来的行是CTRL+C和CTRL+A都起作用，CTRL+C是副作用。

            if (keyData == (Keys.A | Keys.Control) // 这也是一个办法
                || keyData == (Keys.A | Keys.Control | Keys.Alt))
            {
                if (this.m_marcEditor != null)
                {
                    GenerateDataEventArgs ea = new GenerateDataEventArgs();
                    this.m_marcEditor.OnGenerateData(ea);
                    return true;
                }
            }

            if (keyData == (Keys.Y | Keys.Control)) // 这也是一个办法
            {
                if (this.m_marcEditor != null)
                {
                    GenerateDataEventArgs ea = new GenerateDataEventArgs();
                    this.m_marcEditor.OnVerifyData(ea);
                    return true;
                }
            }

            /*
            // Ctrl + T 测试
            if ((keyData & Keys.Control) == Keys.Control
                && (keyData & (~Keys.Control)) == Keys.T)   // 2008/11/30 修改，原来的行是CTRL+C和CTRL+A都起作用，CTRL+C是副作用。
                // && (keyData & Keys.T) == Keys.T)
            {
                if (this.m_marcEditor != null)
                {
                    this.m_marcEditor.Test();

                    return true;
                }
            }*/

            // 其余未处理的键
            if ((keyData & Keys.Control) == Keys.Control)
            {
                bool bRet = this.MarcEditor.OnControlLetterKeyPress(pure_key);
                if (bRet == true)
                    return true;
            }

            // 2008/11/30 changed
            return base.ProcessDialogKey(keyData);  // return false
        }

        // 失去焦点时，应把内容还回去
        protected override void OnLostFocus(EventArgs e)
        {
            // 2008/6/4
            this.MarcEditor.OldImeMode = this.ImeMode;

            if (this.DisableFlush > 0)
            {
                base.OnLostFocus(e);    //
                return;
            }

            this.MarcEditor.Flush();
            base.OnLostFocus(e);
        }

        // 获得焦点，禁止全选
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            //this.SelectionLength = 0;

            // 2008/6/4
            if (this.ImeMode != this.MarcEditor.OldImeMode)
            {
                if (this.MarcEditor.ReadOnly == true)
                    this.ImeMode = this.MarcEditor.OldImeMode;
                else
                {   // 2009/11/20
                    if (this.MarcEditor.OldImeMode == ImeMode.Disable)
                        this.ImeMode = ImeMode.NoControl;
                    else
                        this.ImeMode = this.MarcEditor.OldImeMode;
                }
                API.SetImeHalfShape(this);
            }
        }

        // 卷滚
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int numberOfPixelsToMove = numberOfTextLinesToMove * (int)this.Font/*.DefaultTextFont*/.GetHeight();
            this.MarcEditor.DocumentOrgY += numberOfPixelsToMove;
        }

        #region 右键菜单

        // parameters:
        //      nActiveCol  当前活动的列
        public void PopupMenu(Control control,
            Point p)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem;

            // 缺省值
            Cursor oldcursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;   // 出现沙漏

            List<string> macros = this.MarcEditor.SetDefaultValue(true, -1);

            this.Cursor = oldcursor;

            string strText = "";
            if (macros == null || macros.Count == 0)
                strText = "缺省值(无)";
            else if (macros.Count == 1)
            {
                Debug.Assert(macros != null, "");
                strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
            }
            else
                strText = "缺省值 " + macros.Count.ToString() + " 个";

            menuItem = new MenuItem(strText);
            // menuItem.Click += new System.EventHandler(this.MarcEditor.SetCurFirstDefaultValue);

            if (macros != null && macros.Count == 1)
            {
                menuItem.Click += new System.EventHandler(this.MarcEditor.SetCurrentDefaultValue);
                menuItem.Tag = 0;
            }
            else if (macros != null && macros.Count > 1)
            {
                // 子菜单
                for (int i = 0; i < macros.Count; i++)
                {
                    string strMenuText = macros[i];

                    MenuItem subMenuItem = new MenuItem(strMenuText);
                    subMenuItem.Click += new System.EventHandler(this.MarcEditor.SetCurrentDefaultValue);
                    subMenuItem.Tag = i;
                    menuItem.MenuItems.Add(subMenuItem);
                }
            }
            contextMenu.MenuItems.Add(menuItem);
            if (macros == null || macros.Count == 0)
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 撤消
            menuItem = new MenuItem("撤消(&U)");
            menuItem.Click += new System.EventHandler(this.Menu_Undo);
            contextMenu.MenuItems.Add(menuItem);
            if (this.CanUndo == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 剪切
            menuItem = new MenuItem("剪切(&I)");
            menuItem.Click += new System.EventHandler(this.Menu_Cut);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 复制
            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.Menu_Copy);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 粘贴
            menuItem = new MenuItem("粘贴(&P)");
            menuItem.Click += new System.EventHandler(this.Menu_Paste);
            contextMenu.MenuItems.Add(menuItem);
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 删除
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.Menu_Delete);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.Menu_SelectAll);
            contextMenu.MenuItems.Add(menuItem);
            if (this.Text != "")
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 定长模板
            string strCurName = "";
            bool bEnable = this.MarcEditor.HasTemplateOrValueListDef(
                "template",
                out strCurName);

            menuItem = new MenuItem("定长模板(Ctrl+M) " + strCurName);
            menuItem.Click += new System.EventHandler(this.MarcEditor.GetValueFromTemplate);
            contextMenu.MenuItems.Add(menuItem);
            if (this.MarcEditor.SelectedFieldIndices.Count == 1
                && bEnable == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 值列表
            bEnable = this.MarcEditor.HasTemplateOrValueListDef(
                "valuelist",
                out strCurName);

            menuItem = new MenuItem("值列表 " + strCurName);
            menuItem.Click += new System.EventHandler(this.MarcEditor.GetValueFromValueList);
            contextMenu.MenuItems.Add(menuItem);
            if (this.MarcEditor.SelectedFieldIndices.Count == 1
                && bEnable == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

#if NO
            // 测试
            menuItem = new MenuItem("IMEMODE");
            menuItem.Click += new System.EventHandler(this.ShowImeMode);
            contextMenu.MenuItems.Add(menuItem);
#endif

            /*
            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 删除字段
            menuItem = new MenuItem("删除字段");
            menuItem.Click += new System.EventHandler(this.MarcEditor.DeleteFieldWithDlg);
            contextMenu.MenuItems.Add(menuItem);
            if (this.MarcEditor.m_nFocusCol == 1 || this.MarcEditor.m_nFocusCol == 2)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
             */

            // 追加其他菜单项
            if (this.MarcEditor != null)
            {
                //--------------
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);

                this.MarcEditor.AppendMenu(contextMenu, false);
            }

            contextMenu.Show(control, p);
        }

        void ShowImeMode(System.Object sender, System.EventArgs e)
        {
            MessageBox.Show(this, this.ImeMode.ToString());
        }

        private void Menu_Copy(System.Object sender, System.EventArgs e)
        {
            if (this.SelectionLength > 0)
            {
                string strText = this.SelectedText;
                strText = strText.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);
                DigitalPlatform.Marc.MarcEditor.TextToClipboard(strText);

                //this.Copy();
            }
        }

        private void Menu_Cut(System.Object sender, System.EventArgs e)
        {
            if (this.SelectedText != "")
            {
                string strText = this.SelectedText;
                strText = strText.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);

                this.Cut();

                DigitalPlatform.Marc.MarcEditor.TextToClipboard(strText);

                // 字段名，确保3字符
                if (this.MarcEditor.m_nFocusCol == 1)
                {
                    if (this.Text.Length < 3)
                        this.Text = this.Text.PadRight(3, ' ');
                }
                // 字段指示符，确保2字符
                else if (this.MarcEditor.m_nFocusCol == 2)
                {
                    if (this.Text.Length < 2)
                        this.Text = this.Text.PadRight(2, ' ');
                }

                this.MarcEditor.Flush();
            }
        }

        private void Menu_Paste(System.Object sender, System.EventArgs e)
        {
            // Determine if there is any text in the Clipboard to paste into the text box.
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
            {
                // 把子字段符号换一下
                string strText = DigitalPlatform.Marc.MarcEditor.ClipboardToText();

                // 去掉回车换行符号
                strText = strText.Replace("\r\n", "\r");
                strText = strText.Replace("\r", "*");
                strText = strText.Replace("\n", "*");
                strText = strText.Replace("\t", "*");

                Debug.Assert(this.MarcEditor.SelectedFieldIndices.Count == 1, "Menu_Paste(),MarcEditor.SelectedFieldIndices必须为1。");

                string strFieldsMarc = strText;
                // 先找到有几个字段
                strFieldsMarc = strFieldsMarc.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
                List<string> fields = Record.GetFields(strFieldsMarc);
                if (fields == null || fields.Count == 0)
                    return;

                // 粘贴内容
                if (fields.Count == 1)
                {
                    string strThisText = fields[0];
                    int nOldSelectionStart = this.SelectionStart;
                    if (this.SelectedText == "")
                    {
                        this.Text = this.Text.Insert(this.SelectionStart, strThisText);
                    }
                    else
                    {
                        string strTempText = this.Text;
                        strTempText = strTempText.Remove(nOldSelectionStart, this.SelectedText.Length);
                        strTempText = strTempText.Insert(nOldSelectionStart, strThisText);

                        this.Text = strTempText;
                    }

                    this.SelectionStart = nOldSelectionStart + strThisText.Length;

                    // 字段名，确保3字符
                    if (this.MarcEditor.m_nFocusCol == 1)
                    {
                        if (this.Text.Length > 3)
                            this.Text = this.Text.Substring(0, 3);
                    }
                    // 字段指示符，确保2字符
                    else if (this.MarcEditor.m_nFocusCol == 2)
                    {
                        if (this.Text.Length > 2)
                            this.Text = this.Text.Substring(0, 2);
                    }

                    this.MarcEditor.Flush();    // 促使通知外界
                }
                else if (fields.Count > 1)
                {
                    List<string> addFields = new List<string>();
                    // 甩掉第一个 i = 1
                    for (int i = 0; i < fields.Count; i++)
                    {
                        addFields.Add(fields[i]);
                    }

                    int nIndex = this.MarcEditor.FocusedFieldIndex;
                    Debug.Assert(nIndex >= 0 && nIndex < this.MarcEditor.Record.Fields.Count, "Menu_Paste()，FocusFieldIndex越界。");
                    int nStartIndex = nIndex + 1;
                    int nNewFieldsCount = addFields.Count;

                    this.MarcEditor.Record.Fields.InsertInternal(nStartIndex,
                        addFields);

                    // 把焦点设为最后一项上
                    Debug.Assert(nStartIndex + nNewFieldsCount <= this.MarcEditor.Record.Fields.Count, "不可能的情况");

                    // 把新字段中的最后一个字段设为当前字段
                    this.MarcEditor.SetActiveField(nStartIndex + nNewFieldsCount - 1, 3);

                    InvalidateRect iRect = new InvalidateRect();
                    iRect.bAll = false;
                    iRect.rect = this.MarcEditor.GetItemBounds(nIndex, //nStartIndex,
                        -1,
                        BoundsPortion.FieldAndBottom);
                    this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                        iRect);
                }
            }
        }

        private void Menu_Delete(System.Object sender, System.EventArgs e)
        {
            if (this.SelectedText != "")
            {
                int nStart = this.SelectionStart;
                this.Text = this.Text.Remove(this.SelectionStart, this.SelectionLength);
                this.SelectionStart = nStart;

                // 字段名，确保3字符
                if (this.MarcEditor.m_nFocusCol == 1)
                {
                    if (this.Text.Length < 3)
                        this.Text = this.Text.PadRight(3, ' ');
                }
                // 字段指示符，确保2字符
                else if (this.MarcEditor.m_nFocusCol == 2)
                {
                    if (this.Text.Length < 2)
                        this.Text = this.Text.PadRight(2, ' ');
                }
            }
        }

        private void Menu_Undo(System.Object sender, System.EventArgs e)
        {
            // Determine if last operation can be undone in text box.   
            if (this.CanUndo == true)
            {
                // Undo the last operation.
                this.Undo();
                // Clear the undo buffer to prevent last action from being redone.
                this.ClearUndo();
            }
        }

        private void Menu_SelectAll(System.Object sender, System.EventArgs e)
        {
            this.SelectAll();
        }

        #endregion


        protected override void OnMouseUp(MouseEventArgs e)
        {

            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (nInMenu > 0)
                return;

            if (e.Button == MouseButtons.Right)
            {
                nInMenu++;
                PopupMenu(this, new Point(e.X, e.Y));
                nInMenu--;
                return;
            }

            API.PostMessage(this.MarcEditor.Handle,
MarcEditor.WM_LEFTRIGHT_MOVED,
0,
0);

            base.OnMouseDown(e);

            /*
             * 暂时注释掉 xietao

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            Rectangle rect = new Rectangle(point.x - 10,
                point.y,
                20,
                30);
            // parameter:
            //		nCol	列号 
            //				0 字段说明;
            //				1 字段名;
            //				2 字段指示符 
            //				3 字段内部
            this.MarcEditor.EnsureVisible(this.MarcEditor.FocusedFieldIndex,
                3,
                rect);
             */
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\\':
                    {
                        if (this.m_marcEditor.m_nFocusCol != 3)
                            break;

                        // 为何不起作用?
                        // Ctrl + \ 还是输入 \
                        if (Control.ModifierKeys == Keys.Control)
                            break;


                        int nOldStart = this.SelectionStart;

                        string strTemp = this.Text;
                        /*
                        if (nOldStart < strTemp.Length)
                            strTemp = strTemp.Remove(nOldStart, 1);
                         */
#if BIDI_SUPPORT

                        strTemp = strTemp.Insert(nOldStart, "\x200e" + new string((char)Record.KERNEL_SUBFLD, 1));
#else
                        strTemp = strTemp.Insert(nOldStart, new string((char)Record.KERNEL_SUBFLD, 1));
#endif

                        this.Text = strTemp;
                        this.SelectionStart = nOldStart;
                        e.Handled = true;

#if BIDI_SUPPORT
                        this.SelectionStart += 2;

#else
                        this.SelectionStart++;
#endif
                        return;
                    }
                    break;
                case (char)Keys.Enter:
                    {
                        if (this.MarcEditor.EnterAsAutoGenerate == true)
                        {
                            GenerateDataEventArgs ea = new GenerateDataEventArgs();
                            this.m_marcEditor.OnGenerateData(ea);
                        }
                        else
                        {
                            // 后插字段
                            // this.MarcEditor.InsertAfterFieldWithoutDlg();

                            // parameters:
                            //      nAutoComplate   0: false; 1: true; -1:保持当前记忆状态
                            this.MarcEditor.InsertField(this.MarcEditor.FocusedFieldIndex, 0, 1);   // false, true
                        }
                        e.Handled = true;
                        return;
                    }
                case (char)Keys.Back:
                    {
                        if (this.Overwrite == true)
                        {
                            e.Handled = true;
                            Console.Beep();
                            return;

                            /* 这个功能本来不错，但是被禁止了
                            int nOldSelectionStart = this.SelectionStart;
                            if (nOldSelectionStart > 0)
                            {
                                this.Text = this.Text.Remove(nOldSelectionStart - 1, 1);

                                this.Text = this.Text.Insert(nOldSelectionStart - 1, " ");
                                this.SelectionStart = nOldSelectionStart - 1;
                                return;
                            }
                             * */
                        }

#if BIDI_SUPPORT
                            int nStart = this.SelectionStart - 1;
                        if (this.SelectionLength == 0
                            && nStart > 0)
                        {
                            // 如果删除的正好是方向字符
                            if (this.Text[nStart] == 0x200e
                                && this.Text.Length >= nStart + 1 + 1)
                            {
                                // 一同删除
                                this.Text = this.Text.Remove(nStart-1, 2);
                                this.SelectionStart = nStart-1;
                                // 2011/12/5 上面两行曾经有BUG
                                e.Handled = true;
                            }
                            // 如果删除位置的左方正好是方向字符
                            else if (nStart > 0
                                && this.Text[nStart - 1] == 0x200e)
                            {
                                // 一同删除
                                this.Text = this.Text.Remove(nStart - 1, 2);
                                this.SelectionStart = nStart - 1;
                                e.Handled = true;
                            }
                        }
#endif

                    }
                    break;
                default:
                    {
                        if (this.Overwrite == true)
                        {
                            if ((Control.ModifierKeys == Keys.Control)
                                // || Control.ModifierKeys == Keys.Shift
                                || Control.ModifierKeys == Keys.Alt)
                            {
                                break;
                            }
                            int nOldSelectionStart = this.SelectionStart;
                            if (nOldSelectionStart < this.Text.Length)
                            {
                                /*
                                if (this.Text.Length >= this.MaxLength - 1) // 2008/12/18
                                {
                                    this.Text = this.Text.Remove(this.SelectionStart, 1);
                                    this.SelectionStart = nOldSelectionStart;
                                }
                                 * */
                                if (this.Text.Length >= this.MaxLength) // 2009/3/6 changed
                                {
                                    this.Text = this.Text.Remove(this.SelectionStart, 1 + (this.Text.Length - this.MaxLength));
                                    this.SelectionStart = nOldSelectionStart;
                                }

                            }
                            else
                            {
                                Console.Beep(); // 表示拒绝了输入的字符
                            }
                        }
                    }
                    break;

            }

            base.OnKeyPress(e);
        }

#if BIDI_SUPPORT
        bool IsForbiddenPos()
        {
            if (this.Text.Length <= this.SelectionStart)
                return false;
            char current = this.Text[this.SelectionStart];
            if (current == 0x200e)
                return true;
            return false;
        }

        bool IsInForbiddenPos(int index)
        {
            if (index <= 0)
                return false;

            if (index >= this.Text.Length)
                return false;

            char left = this.Text[index - 1];
            char current = this.Text[index];

            // 插入符避免处在方向符号和子字段符号(31)
            if (left == 0x200e && current == Record.KERNEL_SUBFLD)
                return true;

            return false;

        }
#endif

        // 按下键
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        int x;
                        int y;
                        API.GetEditCurrentCaretPos(this,
                            out x,
                            out y);
                        if (y == 0 && this.MarcEditor.FocusedFieldIndex != 0)		// 虽然现在是0，但是马上会是-1
                        {
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex - 1,
                                this.MarcEditor.m_nFocusCol);

                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);
                            point.y = this.ClientSize.Height - 2;
                            API.SendMessage(this.Handle,
                                API.WM_LBUTTONDOWN,
                                new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                API.MakeLParam(point.x, point.y));

                            API.SendMessage(this.Handle,
                                API.WM_LBUTTONUP,
                                new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                API.MakeLParam(point.x, point.y));

                            e.Handled = true;
                        }
#if BIDI_SUPPORT
                        // 插入符避免处在方向符号和子字段符号(31)之间
                        API.PostMessage(this.Handle, WM_ADJUST_CARET, 0, 0);
#endif
                    }
                    break;
                case Keys.Down:
                    {
                        int x, y;
                        API.GetEditCurrentCaretPos(this,
                            out x,
                            out y);
                        int nTemp = API.GetEditLines(this);
                        if (y >= nTemp - 1
                            && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                        {
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, this.MarcEditor.m_nFocusCol);

                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);

                            {
                                point.y = 1;
                                API.SendMessage(this.Handle,
                                    API.WM_LBUTTONDOWN,
                                    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                    API.MakeLParam(point.x, point.y));

                                API.SendMessage(this.Handle,
                                    API.WM_LBUTTONUP,
                                    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                                    API.MakeLParam(point.x, point.y));
                            }
                            e.Handled = true;


                        }
#if BIDI_SUPPORT
                        // 插入符避免处在方向符号和子字段符号(31)之间
                        API.PostMessage(this.Handle, WM_ADJUST_CARET, 0, 0);
#endif
                    }
                    break;
                case Keys.Left:
                    {
                        API.PostMessage(this.MarcEditor.Handle,
    MarcEditor.WM_LEFTRIGHT_MOVED,
    0,
    0);


                        if (this.SelectionStart != 0)
                        {
#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间
                            if (this.SelectionLength == 0)
                            {
                                if (IsInForbiddenPos(this.SelectionStart - 1) == true
                                    || IsInForbiddenPos(this.SelectionStart) == true)
                                    this.SelectionStart--;
                            }
#endif
                            break;
                        }

                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                            if (this.MarcEditor.FocusedFieldIndex > 0)
                            {
                                // 到达上一行的末尾
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex - 1, 3);
                                this.SelectionStart = this.Text.Length;
                                e.Handled = true;
                                return;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 2)
                        {
                            // 从指示符到字段名
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                            this.SelectionStart = 2;
                            e.Handled = true;
                            return;
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            // 从内容到指示符

                            // 一般字段
                            if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == false)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);
                                this.SelectionStart = 1;
                                e.Handled = true;
                                return;
                            }
                            else
                            {
                                // 头标区
                                if (this.MarcEditor.FocusedField.Name != "###")
                                {
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                                    this.SelectionStart = 2;
                                    e.Handled = true;
                                    return;
                                }
                            }
                        }

                    }
                    break;
                case Keys.Right:    // 右方向键
                    {
                        API.PostMessage(this.MarcEditor.Handle,
MarcEditor.WM_LEFTRIGHT_MOVED,
0,
0);

                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                            // 从字段名到指示符
                            if (this.SelectionStart >= 2)
                            {
                                // 控制字段没有指示符, 直接到内容
                                if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == true)
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                else
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);

                                this.SelectionStart = 0;
                                e.Handled = true;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 2)
                        {
                            // 从指示符到内容
                            if (this.SelectionStart == 1 || this.SelectionStart == 2)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                this.SelectionStart = 0;
                                e.Handled = true;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            // 从内容末尾到下一行首部
                            if (this.SelectionStart == this.Text.Length
                                && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, 1);
                                this.SelectionStart = 0;
                                e.Handled = true;
                                break;
                            }

#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间
                            if (this.SelectionLength == 0)
                            {
                                if (IsInForbiddenPos(this.SelectionStart + 1) == true)
                                    this.SelectionStart++;
                            }
#endif
                        }

                    }
                    break;
                case Keys.End:
                    {
                        if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            // 目前正在内容上
                            break;
                        }

                        // 先到内容区
                        this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);

                        // 头标区
                        if (this.MarcEditor.FocusedField.Name == "###")
                            this.SelectionStart = this.Text.Length - 1;
                        else
                            this.SelectionStart = this.Text.Length;

                        this.SelectionLength = 0;

                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Home:
                    {
                        if (this.SelectionStart == 0)
                        {
                            if (this.MarcEditor.m_nFocusCol == 3)
                            {
                                // 目前正在内容上
                                if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == true)
                                {
                                    // 控制字段,到字段名上
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                                }
                                else
                                {
                                    // 一般字段,到指示符上
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);
                                }
                                this.SelectionStart = 0;
                                e.Handled = true;
                                return;
                            }
                            else if (this.MarcEditor.m_nFocusCol == 2)
                            {
                                // 目前正在指示符上, 那就到字段名上
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                                this.SelectionStart = 0;
                                e.Handled = true;
                                return;
                            }
                            else if (this.MarcEditor.m_nFocusCol == 1)
                            {
                                // 目前正在字段名上, 那就到内容上
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                this.SelectionStart = 0;
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                    break;
                case Keys.PageUp:
                    {
#if NO
                        if (this.MarcEditor.Record.Fields.Count > 0)
                        {
                            this.MarcEditor.SetActiveField(0, 3);
                            this.SelectionStart = 0;
                            e.Handled = true;
                            return;
                        }
#endif

                        this.MarcEditor.DocumentOrgY += this.MarcEditor.ClientRectangle.Height;

                        int x = this.MarcEditor.Record.NameCaptionTotalWidth + this.MarcEditor.Record.NameTotalWidth + this.MarcEditor.Record.IndicatorTotalWidth + 8 - this.MarcEditor.DocumentOrgX;

                        POINT point = new POINT();
                        point.x = 0;
                        point.y = 0;
                        bool bRet = API.GetCaretPos(ref point);


                        if (point.x + this.Location.X - this.MarcEditor.DocumentOrgX > x)
                            x = point.x + this.Location.X - this.MarcEditor.DocumentOrgX;

                        int y = this.MarcEditor.Font.Height / 2;

                        API.PostMessage(this.MarcEditor.Handle,
                            API.WM_LBUTTONDOWN,
                            new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                            API.MakeLParam(x, y));
                        API.PostMessage(this.MarcEditor.Handle,
API.WM_LBUTTONUP,
new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
API.MakeLParam(x, y));

                        API.PostMessage(this.MarcEditor.Handle,
    API.WM_LBUTTONDOWN,
    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
    API.MakeLParam(x, y));
                        API.PostMessage(this.MarcEditor.Handle,
API.WM_LBUTTONUP,
new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
API.MakeLParam(x, y));
                        e.Handled = true;
                    }
                    break;
                case Keys.PageDown:
                    {
                        this.MarcEditor.DocumentOrgY -= this.MarcEditor.ClientRectangle.Height;
                        /*
                        {
                            if (this.MarcEditor.Record.Fields.Count > 0)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.Record.Fields.Count - 1, 3);
                                this.SelectionStart = this.Text.Length;
                                e.Handled = true;
                                return;
                            }
                        }
                         * */
                        int x = this.MarcEditor.Record.NameCaptionTotalWidth + this.MarcEditor.Record.NameTotalWidth + this.MarcEditor.Record.IndicatorTotalWidth + 8 - this.MarcEditor.DocumentOrgX;

                        POINT point = new POINT();
                        point.x = 0;
                        point.y = 0;
                        bool bRet = API.GetCaretPos(ref point);


                        if (point.x + this.Location.X - this.MarcEditor.DocumentOrgX > x)
                            x = point.x + this.Location.X - this.MarcEditor.DocumentOrgX;

                        int y = this.MarcEditor.Font.Height / 2;


                        API.PostMessage(this.MarcEditor.Handle,
                            API.WM_LBUTTONDOWN,
                            new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                            API.MakeLParam(x, y));
                        API.PostMessage(this.MarcEditor.Handle,
API.WM_LBUTTONUP,
new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
API.MakeLParam(x, y));


                        API.PostMessage(this.MarcEditor.Handle,
    API.WM_LBUTTONDOWN,
    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
    API.MakeLParam(x, y));
                        API.PostMessage(this.MarcEditor.Handle,
API.WM_LBUTTONUP,
new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
API.MakeLParam(x, y));

                        e.Handled = true;
                    }
                    break;
                case Keys.Insert:
                    {
                        if (this.MarcEditor.m_nFocusCol == 1
                            || this.MarcEditor.m_nFocusCol == 2)
                        {
                            if (this.MarcEditor.ReadOnly == true)
                            {
                                Console.Beep();
                                break;
                            }
                            // parameters:
                            //      nAutoComplate   0: false; 1: true; -1:保持当前记忆状态
                            bool bDone = this.MarcEditor.InsertField(this.MarcEditor.FocusedFieldIndex, -1/*原来是1*/, -1);    // true, false
                            if (bDone == false)
                                break;
                        }
                    }
                    break;
                case Keys.Delete:
                    {
                        if (this.MarcEditor.m_nFocusCol == 1
                            || this.MarcEditor.m_nFocusCol == 2)
                        {
                            if (this.MarcEditor.ReadOnly == true)
                            {
                                Console.Beep();
                                break;
                            }
                            // 在 字段名 或 指示符 位置
                            int nStart = this.SelectionStart;
                            bool bRemoved = this.MarcEditor.DeleteFieldWithDlg();
                            if (bRemoved == false)
                            {
                                /*
                                if (nStart < this.Text.Length)
                                {
                                    string strOneChar = this.Text.Substring(nStart, 1);
                                    this.Text = this.Text.Insert(this.Text.Length, strOneChar);
                                }
                                this.SelectionStart = nStart;
                                 */
                                e.Handled = true;
                                return;
                            }
                            else
                            {
                                /*
                                if (nStart < this.Text.Length)
                                {
                                    string strOneChar = this.Text.Substring(nStart, 1);
                                    this.Text = this.Text.Insert(nStart, strOneChar);
                                }
                                 */
                                this.SelectionStart = nStart;
                                e.Handled = true;
                                return;
                            }
                        }
                        else
                        {
                            if (this.Overwrite == true)
                            {
                                int nStart = this.SelectionStart;
                                // this.Text = this.Text.Insert(this.Text.Length, " ");
                                if (nStart >= this.Text.Length)
                                {
                                    e.Handled = true;
                                    Console.Beep();
                                    return;
                                }


                                this.Text = this.Text.Insert(nStart, " ");
#if BIDI_SUPPORT
                                if (this.Text[nStart + 1] == 0x200e
                                    && this.Text.Length >= nStart + 2 + 1)
                                    this.Text = this.Text.Remove(nStart + 1, 2);
                                else
                                    this.Text = this.Text.Remove(nStart + 1, 1);
#else

                                this.Text = this.Text.Remove(nStart + 1, 1);
#endif

                                this.SelectionStart = nStart;
                                e.Handled = true;
                                return;
                            }

#if BIDI_SUPPORT
                            if (this.SelectionLength == 0
                                && this.SelectionStart < this.Text.Length)
                            {
                                // 如果删除的正好是方向字符
                                if (this.Text[this.SelectionStart] == 0x200e
                                    && this.Text.Length >= this.SelectionStart + 1 + 1)
                                {
                                    // 一同删除
                                    int nStart = this.SelectionStart;
                                    this.Text = this.Text.Remove(this.SelectionStart, 2);
                                    this.SelectionStart = nStart;
                                    e.Handled = true;
                                }
                                // 如果删除位置的左方正好是方向字符
                                else if (this.SelectionStart > 0
                                    && this.Text[this.SelectionStart - 1] == 0x200e)
                                {
                                    // 一同删除
                                    int nStart = this.SelectionStart;
                                    this.Text = this.Text.Remove(this.SelectionStart - 1, 2);
                                    this.SelectionStart = nStart - 1;
                                    e.Handled = true;
                                }
                            }
#endif

                        }
                    }
                    break;
                default:
                    break;
            }

            base.OnKeyDown(e);
        }

        void SendKeyUp()
        {
            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            API.GetCaretPos(ref point);
            API.SendMessage(this.Handle,
                API.WM_LBUTTONUP,
                new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                API.MakeLParam(point.x, point.y));
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.PageUp:
                case Keys.PageDown:
                    e.Handled = true;
                    return;
            }

            //键不带Shift时，把MarcEditor的Shift选中起始位置设为0
            if (e.Shift == false)
                this.MarcEditor.nStartFieldIndex = -1;

            base.OnKeyUp(e);
            this.MarcEditor.Flush();    // 2011/8/8

            switch (e.KeyCode)
            {
                case Keys.PageUp:
                case Keys.PageDown:
                    e.Handled = true;
                    return;
                case Keys.OemPipe:
                    {
                        /*
                        if (this.m_marcEditor.m_nFocusCol != 3)
                            break;

                        int nOldStart = this.SelectionStart;

                        string strTemp = this.Text;
                        strTemp = strTemp.Remove(nOldStart-1, 1);
                        strTemp = strTemp.Insert(nOldStart - 1, new string((char)Record.KERNEL_SUBFLD,1) );

                        this.Text = strTemp;
                        this.SelectionStart = nOldStart;
                         */

                    }
                    break;

                // 上下左右键时
                case Keys.Left:
                    {
                        if (this.SelectionStart != 0)
                            break;

                        /*
                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                        }
                        else if (this.MarcEditor.m_nFocusCol == 2)
                        {
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                            this.SelectionStart = 3;
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {

                            if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == false)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);
                                this.SelectionStart = 2;
                            }
                            else
                            {
                                if (this.MarcEditor.FocusedField.Name != "###")
                                {
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                                    this.SelectionStart = 3;
                                }
                            }
                        }
                         */
                    }
                    break;
                case Keys.Right:
                    {
                        /*
                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                            if (this.SelectionStart == 3)
                            {
                                if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == true)
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                else
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);
                                this.SelectionStart = 0;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 2)
                        {
                            if (this.SelectionStart == 2)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                this.SelectionStart = 0;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            if (this.SelectionStart == this.Text.Length
                                && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                            {
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, 1);
                                this.SelectionStart = 0;
                            }
                        }
                         */
                    }
                    break;
                case Keys.Delete:
                    {
                        /*
                        if (this.MarcEditor.m_nFocusCol == 1
                            || this.MarcEditor.m_nFocusCol == 2)
                        {
                            int nStart = this.SelectionStart;
                            bool bRemove = this.MarcEditor.DeleteFieldWithDlg();
                            if (bRemove == false)
                            {
                                if (nStart < this.Text.Length)
                                {
                                    string strOneChar = this.Text.Substring(nStart, 1);
                                    this.Text = this.Text.Insert(this.Text.Length, strOneChar);
                                }
                                this.SelectionStart = nStart;
                            }
                            else
                            {

                                this.SelectionStart = nStart;
                            }
                        }
                        else
                        {
                            if (this.Overwrite == true)
                            {
                                int nStart = this.SelectionStart;
                                this.Text = this.Text.Insert(this.Text.Length, " ");
                                this.SelectionStart = nStart;
                            }
                        }
                         */

                    }
                    break;
                case Keys.Up:
                case Keys.Down:
                    break;

                // 其它输入情况，当KeyUp时，如果是value框，则把输入框变大，
                // 如果是字段名，输完第3个，把位置定位到字段指示符
                // 如果是指示符，输完第2个，把位置定位到字段值上
                default:
                    {
                        this.MarcEditor.Flush();

                        if (this.m_marcEditor.m_nFocusCol == 1)
                        {
                            // 当目前输入框是字段名时
                            if (this.SelectionStart == 3)
                            {
                                if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == true)
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                else
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);

                                // this.SelectionStart = 0;
                                // return; 
                            }
                        }
                        if (this.m_marcEditor.m_nFocusCol == 2)
                        {
                            // 当目前输入框是字段指示符时

                            if (this.SelectionStart == 2)
                            {
                                Debug.Assert(Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == false, "列号为2时,不应为控制字段");
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                            }
                        }
                        else if (this.m_marcEditor.m_nFocusCol == 3)
                        {
                            // 当目前输入框是字段值时

                            bool bChangedHeight = false;
                            API.SendMessage(this.Handle,
                                API.EM_LINESCROLL,
                                0,
                                (int)1000);	// 0x7fffffff
                            while (true)
                            {
                                int nFirstLine = API.GetEditFirstVisibleLine(this);
                                if (nFirstLine != 0)
                                {
                                    bChangedHeight = true;
                                    this.Size = new Size(this.Size.Width, this.Size.Height + 10);
                                }
                                else
                                    break;
                            }
                            if (bChangedHeight)
                            {
                                this.MarcEditor.AfterItemHeightChanged(this.MarcEditor.FocusedFieldIndex,
                                    -1);
                            }
                        }
                    }
                    break;
            }

            // 让插入符的位置可见
            // this.Focus();
            int nHeight = 20;

            /*
            if (this.MarcEditor.curEdit != null)
                nHeight = Math.Max(20, this.MarcEditor.curEdit.Height + 8);
             * */
            nHeight = Math.Max(20, this.MarcEditor.Font.Height);

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            Rectangle rect = new Rectangle(point.x - 10,
                point.y,
                20,
                nHeight);
            // parameter:
            //		nCol	列号 
            //				0 字段说明;
            //				1 字段名;
            //				2 字段指示符
            //				3 字段内部
            this.MarcEditor.EnsureVisible(this.MarcEditor.FocusedFieldIndex,
                3,
                rect);
        }

#if NO
        public override void OnHeightChanged()
        {
            this.MarcEditor.AfterItemHeightChanged(this.MarcEditor.FocusedFieldIndex,
    -1);

            base.OnHeightChanged();
        }
#endif

        public void EnsureVisible()
        {
                        // 让插入符的位置可见
            int nHeight = 20;
            
            /*
            if (this.MarcEditor.curEdit != null)
                nHeight = Math.Max(20, this.MarcEditor.curEdit.Height + 8);
             * */
            nHeight = Math.Max(20, this.MarcEditor.Font.Height);

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            Rectangle rect = new Rectangle(point.x - 10,
                point.y,
                20,
                nHeight);
            // parameter:
            //		nCol	列号 
            //				0 字段说明;
            //				1 字段名;
            //				2 字段指示符
            //				3 字段内部
            this.MarcEditor.EnsureVisible(this.MarcEditor.FocusedFieldIndex,
                3,
                rect);
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            this.SelectionLength = 0;

            Debug.WriteLine("little edit Enter");
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            Debug.WriteLine("little edit leave");
        }
    }
}
