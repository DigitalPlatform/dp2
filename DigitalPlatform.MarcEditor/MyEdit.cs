#define CTRL_K

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;
using System.Xml;

using DigitalPlatform.Text;

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

#if REMOVED
        // 当前编辑器所编辑的内容，在整个 MARC 机内格式中的偏移
        public int Offs { get; set; }
#endif

        [DllImport("user32.dll")]
        static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        [DllImport("user32.dll")]
        static extern bool ShowCaret(IntPtr hWnd);

        protected override void CreateHandle()
        {
            base.CreateHandle();

            SetNewWordBreak();

            /*
            // https://stackoverflow.com/questions/609927/custom-caret-for-winforms-textbox
            CreateCaret(this.Handle, IntPtr.Zero, 10, this.Height);
            ShowCaret(this.Handle);
            */
            // BlinkingCaret(_cancel.Token);
        }

        /*
        void BlinkingCaret(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (token.IsCancellationRequested == false)
                {
                    await Task.Delay(500);
                    this.TryInvoke(() =>
                    {
                        CreateCaret(this.Handle, IntPtr.Zero, 10, this.Height);
                        ShowCaret(this.Handle);
                    });
                }
            });
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        protected override void DestroyHandle()
        {
            base.DestroyHandle();
            _cancel.Cancel();
        }
        */

#if REMOVED
        protected override void CreateHandle()
        {
            this.TextChanged -= new EventHandler(MyEdit_TextChanged);
            this.TextChanged += new EventHandler(MyEdit_TextChanged);
        }

        void MyEdit_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }
#endif

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

        // 2025/1/10
        // 获得当前插入符左侧和右侧的内容文字
        // 注：已经去掉
        public void GetLeftRight(out string left,
            out string right)
        {
            // 当前插入符所在位置
            int current = this.SelectionStart;
            // 为了让 Ctrl+B 顺利完成，探测点取中间位置
            if (this.SelectionLength > 0)
                current += this.SelectionLength / 2;

            left = this.Text.Substring(0, current); // .Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);
            right = this.Text.Substring(current);   // .Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);
        }

        // TODO: 需要补充获得子字段符号左边是否存在方向符号
        // 获得有关插入符所在当前子字段的信息
        // parameters:
        //      strFieldValue   整个字段内容
        //      nCaretPos       插入符所在位置
        //      strSubfieldName [out] 返回子字段名。一字符
        //      strSubfieldContent [out] 返回子字段内容字符串
        //      nStart          [out] 返回子字段名开始的偏移
        //      nContentStart   [out] 返回子字段内容开始的偏移
        //      nContentLength  [out] 返回子字段内容字符数
        //      forbidden       [out] nStart 偏移位置左边一个字符是否为方向字符?
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
            out int nContentLength,
            out bool forbidden)
        {
            nStart = 0;
            nContentStart = 0;
            nContentLength = 0;
            strSubfieldName = "";
            strSufieldContent = "";
            forbidden = false;

            strFieldValue = strFieldValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);

            bool bFoundPrevDollar = false;

            int nEnd = strFieldValue.Length;

            // 向左找到$符号
            for (int i = nCaretPos; i >= 0; i--)
            {
                if (i/*nCaretPos*/ > strFieldValue.Length - 1)
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

            // 2024/8/12
            if (bFoundPrevDollar == false)
            {
                return 0;
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
                /*
                 * 操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 ... 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentOutOfRangeException
Message: 索引和长度必须引用该字符串内的位置。
参数名: length
Stack:
在 System.String.Substring(Int32 startIndex, Int32 length)
在 DigitalPlatform.Marc.MyEdit.GetCurrentSubfieldCaretInfo(String strFieldValue, Int32 nCaretPos, String& strSubfieldName, String& strSufieldContent, Int32& nStart, Int32& nContentStart, Int32& nContentLength, Boolean& forbidden)
在 DigitalPlatform.Marc.MyEdit.DoCtrlH()
在 DigitalPlatform.Marc.MyEdit.Menu_pasteToCurrentSubfield(Object sender, EventArgs e)
在 DigitalPlatform.Marc.MyEdit.ProcessDialogKey(Keys keyData)
在 System.Windows.Forms.Control.PreProcessMessage(Message& msg)
在 System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message& msg)
在 System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG& msg)


dp2Circulation 版本: dp2Circulation, Version=3.94.9035.22741, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1
本机 MAC 地址: ...
操作时间 2024/10/9 15:23:52 (Wed, 09 Oct 2024 15:23:52 +0800) 
前端地址 ... 经由 http://dp2003.com/dp2library 

                 * */
                if (nStart + 1 < strFieldValue.Length)  // 2024/10/11 增加
                    strSubfieldName = strFieldValue.Substring(nStart + 1, 1);   // 2024/7/4 增加 +1
            }

            nContentLength = nEnd - nContentStart;
            strSufieldContent = strFieldValue.Substring(nContentStart, nContentLength);

#if BIDI_SUPPORT
            if (nStart > 0 && IsInForbiddenPos(strFieldValue, nStart))
                forbidden = true;
#endif
            return 1;
        }

        public void DupCurrentSubfield()
        {
            if (this.MarcEditor.m_nFocusCol != 3)
                return;

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
                out nContentLength,
                out _);
            if (nRet == 0)
                return;

            if (nContentStart >= 2)
            {
                nContentStart -= 2;
                nContentLength += 2;
            }

            string strText = this.Text.Substring(nContentStart, nContentLength);
            int nInsertPos = nContentStart + nContentLength;
            this.TextWithHeight = this.Text.Substring(0, nInsertPos) + strText + this.Text.Substring(nInsertPos);
            this.SelectionStart = nContentStart;
            this.SelectionLength = nContentLength;
        }

        // 插入一个子字段符号
        bool InsertSubfieldChar()
        {
            string text = this.Text;
            int pos = this.SelectionStart;
            int length = this.SelectionLength;
#if BIDI_SUPPORT

            string name = "\x200e" + new string((char)Record.KERNEL_SUBFLD, 1);
#else
            string name = new string((char)Record.KERNEL_SUBFLD, 1);
#endif
            this.TextWithHeight = text.Substring(0, pos) + name + text.Substring(pos + length);

            this.SelectionStart = pos + name.Length;
            this.SelectionLength = 0;

            // 关闭输入法，以便后面输入字母能顺利
            this.ImeMode = ImeMode.Off;
            return true;
        }


        // 插入一个子字段符号和子字段名
        bool InsertSubfieldName(Keys key)
        {
            char ch = (char)key;
            if (char.IsDigit(ch) == false && char.IsLetter(ch) == false)
                return false;
            //if (char.IsUpper(ch))
            //    ch = char.ToLower(ch);
            string text = this.Text;
            int pos = this.SelectionStart;
            int length = this.SelectionLength;
#if BIDI_SUPPORT

            string name = "\x200e" + new string((char)Record.KERNEL_SUBFLD, 1) + ch;
#else
            string name = new string((char)Record.KERNEL_SUBFLD, 1) + ch;
#endif
            this.TextWithHeight = text.Substring(0, pos) + name + text.Substring(pos + length);

            this.SelectionStart = pos + name.Length;
            this.SelectionLength = 0;
            return true;
        }

        // return:
        //      false   需要执行缺省窗口过程
        //      true    不要执行缺省窗口过程。即消息接管了。
        bool DoDeleteCurrentSubfield()
        {
            if (this.MarcEditor.m_nFocusCol != 3)
                return false;

            // 获得有关插入符所在当前子字段的信息
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            int nRet = GetCurrentSubfieldCaretInfo(
                this.Text,
                this.SelectionStart,
                out string strSubfieldName,
                out string strSufieldContent,
                out int nStart,
                out int nContentStart,
                out int nContentLength,
                out bool forbidden);
            if (nRet == 0)
                return false;

            Debug.Assert(nContentStart >= nStart);

            // 整个 $axxx 的字符数
            int length = nContentLength + (nContentStart - nStart);
            if (forbidden)
            {
                nStart--;
                length++;
            }
            var text = this.Text;
            this.TextWithHeight = text.Substring(0, nStart) + text.Substring(nStart + length);
            this.SelectionStart = nStart;
            this.SelectionLength = 0;
            return true;
        }

        // 记忆子字段上双击次数。奇偶
        class DoubleClickInfo
        {
            public string _double_string = null;
            public int _double_pos = -1;
            public int _double_count = 0;
        }

        DoubleClickInfo _double_info = new DoubleClickInfo();

        bool _inSimulateClicking = false;

        // TODO: 双击一次选中子字段内容，再双击一次选中整个子字段内容(即包括子字段符号、子字段名和内容)
        // return:
        //      false   需要执行缺省窗口过程
        //      true    不要执行缺省窗口过程。即消息接管了。
        bool DoDoubleClick()
        {
            if (_inSimulateClicking)
                return false;
            // OnMouseDoubleClick那里接管不行。因为那里edit原有动作已经执行,this.SelectionStart值已经被破坏

            if (this.MarcEditor.m_nFocusCol != 3)
                return false;

            // 当前插入符所在位置
            int current = this.SelectionStart;
            // 为了让 Ctrl+B 顺利完成，探测点取中间位置
            if (this.SelectionLength > 0)
                current += this.SelectionLength / 2;

            // 获得有关插入符所在当前子字段的信息
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            int nRet = GetCurrentSubfieldCaretInfo(
                this.Text,
                current,
                out string strSubfieldName,
                out string strSufieldContent,
                out int nStart,
                out int nContentStart,
                out int nContentLength,
                out bool forbidden);
            if (nRet == 0)
            {
                // return false;

                // 2025/5/15
                // 全选小 edit 内容
                this.SelectionStart = 0;
                this.SelectionLength = this.Text.Length;

                // 记忆
                this._double_info._double_string = this.Text;
                this._double_info._double_count = 0;
                this._double_info._double_pos = 0;
                return true;
            }

            if (this.Text == this._double_info._double_string
                && nStart == this._double_info._double_pos
                && (this._double_info._double_count % 2) == 1)  // 前面已经双击过一次了
            {
                this.SelectionStart = nContentStart;
                this.SelectionLength = nContentLength;
            }
            else
            {
                int start = nStart;
                int length = 2 + nContentLength;
                if (forbidden)
                {
                    start--;
                    length++;
                }
                this.SelectionStart = start;
                this.SelectionLength = length;
            }

            // 记忆
            if (this.Text != this._double_info._double_string)
            {
                this._double_info._double_string = this.Text;
                this._double_info._double_count = 0;
            }
            this._double_info._double_pos = nStart;
            this._double_info._double_count++;
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
                    /*
                case API.WM_LBUTTONUP:
                    if (_ensureFocus)
                    {
                        _ensureFocus = false;
                        Task.Run(async () =>
                        {
                            await Task.Delay(100);
                            this.TryInvoke(() =>
                            {
                                this.m_marcEditor.SetFocusEx();
                            });
                        });
                    }
                    break;
                    */
            }
            base.DefWndProc(ref m);
        }

        // 是否需要确认一次 Focused
        bool _ensureFocus = false;

        // 确保 WM_MOUSELBUTTONUP 时确认一次 Focused
        public void EnsureMarcEditFocued()
        {
            _ensureFocus = true;
        }

        #region 处理快捷键

        void PopupCtrlAMenu()
        {
            // 获得当前位置缺省值
            List<string> macros = this.MarcEditor.SetDefaultValue(true, -1);

            ContextMenu contextMenu = new ContextMenu();

            if (macros != null && macros.Count > 0)
            {
                string strText = "";
                if (macros.Count == 1)
                {
                    Debug.Assert(macros != null, "");
                    strText = "缺省值 '" + macros[0].Replace(" ", "_").Replace(Record.SUBFLD, Record.KERNEL_SUBFLD) + "'";
                }
                else
                    strText = "缺省值 " + macros.Count.ToString() + " 个";

                var menuItem = new MenuItem(strText);

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
            }

            // 获得当前位置可用的菜单
            GenerateDataEventArgs e = new GenerateDataEventArgs();
            e.ScriptEntry = "!getActiveMenu";
            this.MarcEditor.OnGenerateData(e);
            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                Console.Beep();
                var menuItem = new MenuItem("error: " + e.ErrorInfo);
                contextMenu.MenuItems.Add(menuItem);
            }
            else
            {
                // e.Parameter 中返回了 XML 格式的菜单项
                string xml = e.Parameter as string;
                if (string.IsNullOrEmpty(xml))
                {
                    Console.Beep();
                    var menuItem = new MenuItem($"error: e.Parameter 为空");
                    contextMenu.MenuItems.Add(menuItem);
                    goto END1;
                }
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                var actions = dom.DocumentElement.SelectNodes("action");

                if (actions.Count > 0
                    && macros != null && macros.Count > 0)
                {
                    var menuItem = new MenuItem("-");
                    contextMenu.MenuItems.Add(menuItem);
                }

                foreach (XmlElement action in actions)
                {
                    var scriptEntry = action.GetAttribute("scriptEntry");
                    var menuItem = new MenuItem(action.GetAttribute("name"));
                    menuItem.Tag = scriptEntry;
                    menuItem.Click += (o1, e1) =>
                    {
                        this.MarcEditor.OnGenerateData(new GenerateDataEventArgs
                        {
                            ScriptEntry = scriptEntry,
                        });
                        this.MarcEditor.Focus();
                    };
                    contextMenu.MenuItems.Add(menuItem);
                }
            }

            if (contextMenu.MenuItems.Count == 0)
            {
                Console.Beep();
                return;
            }

        END1:
            //if (contextMenu.MenuItems.Count > 0)
            //    contextMenu.MenuItems[0].PerformSelect();

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            contextMenu.Show(this, new Point(point.x, point.y + this.MarcEditor.CalcuTextLineHeight(null)));

            // SendKeys.Send("{DOWN}");
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            //Debug.WriteLine($"MyEdit ProcessCmdKey {keyData}");

            if (_k)
                return base.ProcessCmdKey(ref m, keyData);

            // TODO: 调用自动创建数据过程中，为了明显，
            // 可以用 FloatingMessage 显示。
            // 如果命令发现不适合完成，可以用 floatingMessage 报错。(例如，插入符在 701 上面点 Ctrl+A，因为 701 有内容了，没有执行功能，需要报错提示)
            if (keyData == (Keys.A | Keys.Control))
            {
                /*
                POINT point = new POINT();
                point.x = 0;
                point.y = 0;
                bool bRet = API.GetCaretPos(ref point);
                */

                this.MarcEditor.FireSelectedFieldChanged(); // 令数据加工菜单敏感
                PopupCtrlAMenu();

                //this.MarcEditor.OnGenerateData(new GenerateDataEventArgs());
                return true;
            }

            if (keyData == (Keys.Control | Keys.S))
            {
                // MessageBox.Show(this, "Call 加拼音");

                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.ScriptEntry = "AddPinyin";
                e1.FocusedControl = this.m_marcEditor;
                this.MarcEditor.OnGenerateData(e1);
                return true;
            }
            if (keyData == (Keys.Control | Keys.D))
            {
                // MessageBox.Show(this, "Call 删除拼音");

                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.ScriptEntry = "RemovePinyin";
                e1.FocusedControl = this.m_marcEditor;
                this.MarcEditor.OnGenerateData(e1);

                return true;
            }

            if (keyData == (Keys.Z | Keys.Control))
            {
                Menu_Undo(this, new EventArgs());
                return true;
            }

            if (keyData == (Keys.Y | Keys.Control))
            {
                Menu_Redo(this, new EventArgs());
                return true;
            }

            /*
            if (keyData == (Keys.PageUp | Keys.Control))
            {
                MessageBox.Show(this, "MyEdit Ctrl+PageUp");
                return true;
            }

            if (keyData == (Keys.PageDown | Keys.Control))
            {
                MessageBox.Show(this, "MyEdit Ctrl+PageDown");
                return true;
            }
            */
            if (keyData == (Keys.Home | Keys.Control))
            {
                this.MarcEditor.SetActiveField(0, 0);
                return true;
            }

            if (keyData == (Keys.End | Keys.Control))
            {
                var tail = this.MarcEditor.Record.Fields.Count - 1;
                if (tail >= 0)
                    this.MarcEditor.SetActiveField(tail, 0);
                return true;
            }

            return base.ProcessCmdKey(ref m, keyData);
        }

        public bool InCtrlK
        {
            get
            {
                return _k;
            }
        }


        internal bool _k = false;    // 是否在 Ctrl+K 状态

        // 接管Ctrl+各种键
        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            //Debug.WriteLine($"MyEdit ProcessDialogKey {keyData}");

            /*
            if (keyData == (Keys.PageUp | Keys.Control))
            {
                MessageBox.Show(this, "MyEdit 1 Ctrl+PageUp");
                return true;
            }

            if (keyData == (Keys.PageDown | Keys.Control))
            {
                MessageBox.Show(this, "MyEdit 1 Ctrl+PageDown");
                return true;
            }
            */

            // 去掉Control/Shift/Alt 以后的纯净的键码
            // 2008/11/30 changed
            Keys pure_key = (keyData & (~(Keys.Control | Keys.Shift | Keys.Alt)));

#if CTRL_K
            if (_k)
            {
                this.MarcEditor.DoCtrlK(pure_key);
                _k = false;
                return true;
            }
#endif

            if (Control.ModifierKeys == Keys.Control
                && (pure_key == Keys.Enter || pure_key == Keys.LineFeed))
            {
                // 插入一个新的字段
                this.MarcEditor.InsertField(this.MarcEditor.FocusedFieldIndex, 0, 1);
                return true;
            }

            // 2025/1/10
            if (Control.ModifierKeys == Keys.Shift
    && (pure_key == Keys.Enter || pure_key == Keys.LineFeed))
            {
                // 插入一个新的字段
                this.MarcEditor.SplitField(this.MarcEditor.FocusedFieldIndex);
                return true;
            }

            // Ctrl + M
            if (Control.ModifierKeys == Keys.Control
                && pure_key == Keys.M)
            {
                MarcEditor.EditControlTextToItem();

                // 调定长模板
                this.MarcEditor.GetValueFromTemplate();
                return true;
            }

            // 2024/7/4
            // Ctrl+B
            if ((keyData & Keys.Control) == Keys.Control
    && pure_key == Keys.B)
            {
                if (DoDoubleClick() == true)
                    return true;
            }

            // Ctrl + C
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.C)
            {
                this.Menu_Copy(null, null);
                return true;
            }

            // Ctrl + H
            // 粘贴剪贴板内容到当前子字段(内容部分)
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.H)
            {
                this.Menu_pasteToCurrentSubfield(null, null);
                return true;
            }

            // Ctrl + Q
            // 整理字段顺序
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.Q)
            {
                this.MarcEditor.menuItem_sortFields(this, new EventArgs());
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

#if CTRL_K
            // Ctrl + K
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.K)
            {
                _k = true;
                return true;
            }
#endif

            // 2024/7/4
            // Ctrl+I 输入一个子字段符号(并且自动把汉字输入法关闭)
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.I)
            {
                InsertSubfieldChar();
                return true;
            }

#if REMOVED
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
#endif

            // Ctrl+U 校验数据
            if (keyData == (Keys.U | Keys.Control))
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

            // Shift+Del 删除当前子字段
            if (keyData == (Keys.Delete | Keys.Shift))
            {
                if (DoDeleteCurrentSubfield() == true)
                    return true;
            }

            /*
            if ((keyData & (Keys.Control | Keys.Shift)) == (Keys.Control | Keys.Shift))
            {
                if (InsertSubfieldName(pure_key))
                    return true;
            }
            */

            // 其余未处理的键
            // 注: Ctrl+D 会经过这里到达 EntityForm 处理“删除拼音”
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key != Keys.ControlKey)
            {
                bool bRet = this.MarcEditor.OnControlLetterKeyPress(pure_key);
                if (bRet == true)
                    return true;
                else
                    return base.ProcessDialogKey(keyData);
            }


            return base.ProcessDialogKey(keyData);
        }

        #endregion

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
            menuItem = new MenuItem("撤消(&U)\tCtrl+Z");
            menuItem.Click += new System.EventHandler(this.Menu_Undo);
            contextMenu.MenuItems.Add(menuItem);
            // if (this.CanUndo == true)
            if (this.MarcEditor.CanUndo() == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 重做
            menuItem = new MenuItem("重做(&R)\tCtrl+Y");
            menuItem.Click += new System.EventHandler(this.Menu_Redo);
            menuItem.Enabled = this.MarcEditor.CanRedo();
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 剪切
            menuItem = new MenuItem("剪切(&T)\tCtrl+X");
            menuItem.Click += new System.EventHandler(this.Menu_Cut);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 复制
            menuItem = new MenuItem("复制(&C)\tCtrl+C");
            menuItem.Click += new System.EventHandler(this.Menu_Copy);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 粘贴
            menuItem = new MenuItem("粘贴(&P)\tCtrl+V");
            menuItem.Click += new System.EventHandler(this.Menu_Paste);
            contextMenu.MenuItems.Add(menuItem);

            StringUtil.RunClipboard(() =>
            {
                if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;
            });

            // 删除
            menuItem = new MenuItem("删除(&D)\tDel");
            menuItem.Click += new System.EventHandler(this.Menu_Delete);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectionLength > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 删除当前子字段
            menuItem = new MenuItem("删除当前子字段\tShift+Del");
            menuItem.Click += (o, e1) =>
            {
                DoDeleteCurrentSubfield();
            };
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*
            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.Menu_SelectAll);
            contextMenu.MenuItems.Add(menuItem);
            if (this.Text != "")
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            */

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 定长模板
            string strCurName = "";
            bool bEnable = this.MarcEditor.HasTemplateOrValueListDef(
                "template",
                out strCurName) == 1;

            menuItem = new MenuItem("定长模板 " + strCurName + "\tCtrl+M");
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
                out strCurName) == 1;

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
                DigitalPlatform.Marc.MarcEditor.TextToClipboardFormat(strText);

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

                DigitalPlatform.Marc.MarcEditor.TextToClipboardFormat(strText);

                // 字段名，确保3字符
                if (this.MarcEditor.m_nFocusCol == 1)
                {
                    if (this.Text.Length < 3)
                        this.TextWithHeight = this.Text.PadRight(3, ' ');
                }
                // 字段指示符，确保2字符
                else if (this.MarcEditor.m_nFocusCol == 2)
                {
                    if (this.Text.Length < 2)
                        this.TextWithHeight = this.Text.PadRight(2, ' ');
                }

                this.MarcEditor.Flush();
            }
        }

        public void PasteToCurrent(string strText)
        {
            if (string.IsNullOrEmpty(strText) == false)
            {
                int nPos = this.SelectionStart;
                if (this.SelectionLength != 0)
                    this.TextWithHeight = this.Text.Remove(nPos, this.SelectionLength);
                this.TextWithHeight = this.Text.Insert(nPos, strText);
                this.SelectionStart = nPos;
                this.SelectionLength = strText.Length;
            }
        }

        // 移走 $a 后面的一个空格
        static string RemoveBlankChar(string strText)
        {
            int step = -1;  // 表示当前 char 距离 $ 字符的步长。0 表示正好在 $a 的 $ 上，1 表示在 $a 的 a 上，2，表示在 a 后面一个字符上
            StringBuilder result = new StringBuilder();
            foreach (char ch in strText)
            {
                if (ch == Record.KERNEL_SUBFLD)
                    step = 0;
                else if (step != -1)
                    step++;

                if (step == 2 && ch == ' ')
                {
                    // 越过
                }
                else
                    result.Append(ch);
            }

            // 去除 $ 前面的一个空格
            for (int i = result.Length - 1; i >= 0; i--)
            {
                if (i - 1 >= 0
                    && result[i] == Record.KERNEL_SUBFLD
                    && result[i - 1] == ' ')
                {
                    result.Remove(i - 1, 1);
                    // i--;
                }
            }

            return result.ToString();
        }

        bool DoCtrlH()
        {
            if (this.MarcEditor.m_nFocusCol != 3)
            {
                Console.Beep();
                return false;
            }

            string strText = MarcEditor.ClipboardToTextFormat();
            if (string.IsNullOrEmpty(strText))
            {
                MessageBox.Show(this, "当前 Windows 剪贴板中没有任何内容可供粘贴");
                return false;
            }

            // 当前插入符所在位置
            int current = this.SelectionStart;
            // 为了让 Ctrl+B 顺利完成，探测点取中间位置
            if (this.SelectionLength > 0)
                current += this.SelectionLength / 2;

            // 获得有关插入符所在当前子字段的信息
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            int nRet = GetCurrentSubfieldCaretInfo(
                this.Text,
                current,
                out string strSubfieldName,
                out string strSufieldContent,
                out int nStart,
                out int nContentStart,
                out int nContentLength,
                out bool forbidden);
            if (nRet == 0)
                return false;

            {
                this.SelectionStart = nContentStart;
                this.SelectionLength = nContentLength;
            }

            DoPasteContinueText();
            return true;
        }

        void DoPasteContinueText()
        {
            string strText = MarcEditor.ClipboardToTextFormat();
            if (strText != null)
            {
                // 去掉回车换行符号
                strText = strText.Replace("\r\n", "\r");
                strText = strText.Replace("\r", "*");
                strText = strText.Replace("\n", "*");
                strText = strText.Replace("\t", "*");

                // 2017/2/20
                strText = strText.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

                Debug.Assert(this.MarcEditor.SelectedFieldIndices.Count == 1, "Menu_Paste(),MarcEditor.SelectedFieldIndices必须为1。");

                string strFieldsMarc = strText;

                // 先找到有几个字段

                List<string> fields = Record.GetFields(strFieldsMarc, out bool contains_field_end);
                if (fields == null || fields.Count == 0)
                    return;

                // 粘贴内容
                // 对三个不同区域的小 edit，分别限制粘贴后的字符数。
                // 比如，粘贴进入字段名 edit 的部分，不让超过 3 chars；
                // 粘贴进入指示符 edit 的部分，不让超过 2 chars；
                // 粘贴进入内容 edit 的部分，无 chars 限制。
                if (fields.Count == 1 && contains_field_end == false)
                {
                    string strThisText = fields[0];
                    int nOldSelectionStart = this.SelectionStart;
                    if (this.SelectedText == "")
                    {
                        this.TextWithHeight = this.Text.Insert(this.SelectionStart, strThisText);
                    }
                    else
                    {
                        string strTempText = this.Text;
                        strTempText = strTempText.Remove(nOldSelectionStart, this.SelectedText.Length);
                        strTempText = strTempText.Insert(nOldSelectionStart, strThisText);

                        this.TextWithHeight = strTempText;
                    }

                    this.SelectionStart = nOldSelectionStart + strThisText.Length;

                    // 字段名，确保3字符
                    if (this.MarcEditor.m_nFocusCol == 1)
                    {
                        if (this.Text.Length > 3)
                            this.TextWithHeight = this.Text.Substring(0, 3);
                    }
                    // 字段指示符，确保2字符
                    else if (this.MarcEditor.m_nFocusCol == 2)
                    {
                        if (this.Text.Length > 2)
                            this.TextWithHeight = this.Text.Substring(0, 2);
                    }

                    // TODO: 确保小 edit 的高度调整
                    this.MarcEditor.Flush();    // 促使通知外界
                }
                else if (fields.Count > 1)
                {
                    MessageBox.Show(this, "不允许粘贴多个字段");
                }
            }
        }



        void Menu_pasteToCurrentSubfield(System.Object sender, System.EventArgs e)
        {
            DoCtrlH();
        }

        private void Menu_Paste(System.Object sender, System.EventArgs e)
        {
            // bool bControl = Control.ModifierKeys == Keys.Control;

            string strText = MarcEditor.ClipboardToTextFormat();

            if (strText != null)
            {
                // 去掉回车换行符号
                strText = strText.Replace("\r\n", "\r");
                strText = strText.Replace("\r", "*");
                strText = strText.Replace("\n", "*");
                strText = strText.Replace("\t", "*");

                // 2017/2/20
                strText = strText.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

                /*
                if (bControl)
                {
                    strText = strText.Replace('|', Record.KERNEL_SUBFLD);
                    // strText = strText.Replace(" ", "");
                    strText = RemoveBlankChar(strText);
                }
                */

                Debug.Assert(this.MarcEditor.SelectedFieldIndices.Count == 1, "Menu_Paste(),MarcEditor.SelectedFieldIndices必须为1。");

                string strFieldsMarc = strText;

                // 先找到有几个字段

                List<string> fields = Record.GetFields(strFieldsMarc, out bool contains_field_end);
                if (fields == null || fields.Count == 0)
                    return;

                // 粘贴内容
                // 对三个不同区域的小 edit，分别限制粘贴后的字符数。
                // 比如，粘贴进入字段名 edit 的部分，不让超过 3 chars；
                // 粘贴进入指示符 edit 的部分，不让超过 2 chars；
                // 粘贴进入内容 edit 的部分，无 chars 限制。
                if (fields.Count == 1 && contains_field_end == false)
                {
                    string strThisText = fields[0];
                    int nOldSelectionStart = this.SelectionStart;
                    if (this.SelectedText == "")
                    {
                        this.TextWithHeight = this.Text.Insert(this.SelectionStart, strThisText);
                    }
                    else
                    {
                        string strTempText = this.Text;
                        strTempText = strTempText.Remove(nOldSelectionStart, this.SelectedText.Length);
                        strTempText = strTempText.Insert(nOldSelectionStart, strThisText);

                        this.TextWithHeight = strTempText;
                    }

                    // this.SelectionStart = nOldSelectionStart + strThisText.Length;
                    // 选中刚粘贴进入的部分
                    this.SelectionStart = nOldSelectionStart;
                    this.SelectionLength = strThisText.Length;

                    // 字段名，确保3字符
                    if (this.MarcEditor.m_nFocusCol == 1)
                    {
                        if (this.Text.Length > 3)
                            this.TextWithHeight = this.Text.Substring(0, 3);
                    }
                    // 字段指示符，确保2字符
                    else if (this.MarcEditor.m_nFocusCol == 2)
                    {
                        if (this.Text.Length > 2)
                            this.TextWithHeight = this.Text.Substring(0, 2);
                    }

                    this.MarcEditor.Flush();    // 促使通知外界
                }
                else if (fields.Count > 1
                    || contains_field_end == true)
                {
                    // *** 粘贴进入一个或者多个完整字段
                    // 根据当前插入符所在的小 edit 区域，如果处在字段名和指示符区域，则本字段前面插入；处在内容区域上，则在本字段后面插入
                    /*
                    List<string> addFields = new List<string>();
                    // 甩掉第一个 i = 1
                    for (int i = 0; i < fields.Count; i++)
                    {
                        addFields.Add(fields[i]);
                    }
                    */
                    var addFields = fields.ToList();

                    int nIndex = this.MarcEditor.FocusedFieldIndex;
                    Debug.Assert(nIndex >= 0 && nIndex < this.MarcEditor.Record.Fields.Count, "Menu_Paste()，FocusFieldIndex越界。");
                    int col = this.MarcEditor.m_nFocusCol;
                    int nStartIndex = nIndex + 1;   // 后插
                    if (col < 3)    // 2024/7/5
                        nStartIndex = nIndex;   // 前插
                    int nNewFieldsCount = addFields.Count;

                    // TODO: 提取到 MarcEditor 中成为函数
                    {
                        this.MarcEditor.Record.Fields.InsertInternal(
                            nStartIndex,
                            addFields);
                        // 加入操作历史
                        this.MarcEditor.AppendInsertFields(nStartIndex,
                            this.MarcEditor.GetFields(nStartIndex, addFields.Count));
                    }

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
                this.TextWithHeight = this.Text.Remove(this.SelectionStart, this.SelectionLength);
                this.SelectionStart = nStart;

                // 字段名，确保3字符
                if (this.MarcEditor.m_nFocusCol == 1)
                {
                    if (this.Text.Length < 3)
                        this.TextWithHeight = this.Text.PadRight(3, ' ');
                }
                // 字段指示符，确保2字符
                else if (this.MarcEditor.m_nFocusCol == 2)
                {
                    if (this.Text.Length < 2)
                        this.TextWithHeight = this.Text.PadRight(2, ' ');
                }
            }
        }

        private void Menu_Undo(System.Object sender, System.EventArgs e)
        {
            /*
            // Determine if last operation can be undone in text box.   
            if (this.CanUndo == true)
            {
                // Undo the last operation.
                this.Undo();
                // Clear the undo buffer to prevent last action from being redone.
                this.ClearUndo();
            }
            */

            if (this.MarcEditor.CanUndo() == true)
            {
                this.MarcEditor.Undo();
            }
            else
                Console.Beep();
        }

        private void Menu_Redo(System.Object sender, System.EventArgs e)
        {
            if (this.MarcEditor.CanRedo() == true)
            {
                this.MarcEditor.Redo();
            }
            else
                Console.Beep();
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

            if (e.Button == MouseButtons.Left)
            {
                _selection_start = this.SelectionStart;
                //Debug.WriteLine($"mousedown1 记忆 _current_selection_start={_selection_start}");

                MemoryCaretX();
            }

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

            if (e.Button == MouseButtons.Left)
            {
                _selection_start = this.SelectionStart;
                //Debug.WriteLine($"mousedown2 记忆 _current_selection_start={_selection_start}");
            }

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

        /*
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentOutOfRangeException
Message: 索引和计数必须引用该字符串内的位置。
参数名: count
Stack:
在 System.String.RemoveInternal(Int32 startIndex, Int32 count)
在 System.String.Remove(Int32 startIndex, Int32 count)
在 DigitalPlatform.Marc.MyEdit.OnKeyPress(KeyPressEventArgs e)
在 System.Windows.Forms.Control.ProcessKeyEventArgs(Message& m)
在 System.Windows.Forms.Control.ProcessKeyMessage(Message& m)
在 System.Windows.Forms.Control.WmKeyChar(Message& m)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.TextBoxBase.WndProc(Message& m)
在 System.Windows.Forms.TextBox.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.30.6506.29202, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1
本机 MAC 地址: xxx 
操作时间 2017/10/26 9:28:53 (Thu, 26 Oct 2017 09:28:53 +0800) 
         * * */
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
#if REMOVED
            {
                // testing
                base.OnKeyPress(e);
                return;
            }
#endif

#if CTRL_K
            _k = false;
#endif
            int col = this.m_marcEditor.m_nFocusCol;
            switch (e.KeyChar)
            {
                case '#':
                    if (col == 1)
                    {
                        e.Handled = true;
                        Console.Beep(); // 表示拒绝了输入的字符
                        return;
                    }
                    break;
                case '\\':
                    {
                        if (col != 3)
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

                        this.TextWithHeight = strTemp;
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
                case (char)Keys.LineFeed:
                    {
                        if (this.MarcEditor.EnterAsAutoGenerate == true)
                        {
                            GenerateDataEventArgs ea = new GenerateDataEventArgs();
                            this.m_marcEditor.OnGenerateData(ea);
                        }
                        else
                        {
                            bool insert_mode = false;
                            if (insert_mode)
                            {
                                // 后插字段
                                // this.MarcEditor.InsertAfterFieldWithoutDlg();

                                // parameters:
                                //      nAutoComplate   0: false; 1: true; -1:保持当前记忆状态
                                this.MarcEditor.InsertField(this.MarcEditor.FocusedFieldIndex, 0, 1);   // false, true
                            }
                            else
                            {
                                // 到下一行内容开头
                                if (this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                                {
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, 3);
                                    this.SelectionStart = 0;
                                }
                            }
                        }
                        e.Handled = true;
                        return;
                    }
                case (char)Keys.Back:
                    {
                        // TODO: 可以改为把当前位置删除为空格，并向左移动一个字符
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
                            // 为调试准备的信息
                            string strDebugInfo = "删除前的 text [" + this.Text + "] hex[" + GetTextHex(this.Text) + "], nStart=" + nStart + ", this.Text.Length=" + this.Text.Length;

                            try
                            {
                                // 如果删除的正好是方向字符，那么也要追加删除其左方一个普通字符
                                if (this.Text[nStart] == 0x200e)    // && this.Text.Length >= nStart + 1 + 1
                                {
                                    // 一同删除
                                    this.TextWithHeight = this.Text.Remove(
                                        nStart - 1,
                                        2);
                                    this.SelectionStart = nStart - 1;
                                    // 2011/12/5 上面两行曾经有BUG
                                    e.Handled = true;
                                }
                                // 如果删除位置的左方正好是方向字符，也要一并删除
                                else if (nStart > 0
                                    && this.Text[nStart - 1] == 0x200e)
                                {
                                    // 一同删除
                                    this.TextWithHeight = this.Text.Remove(nStart - 1, 2);
                                    this.SelectionStart = nStart - 1;
                                    e.Handled = true;
                                }
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                throw new Exception("Backspace 发生异常:" + strDebugInfo, ex);
                            }
                        }
#endif

                    }
                    break;
                default:
                    {
                        if ((col == 1
                            || col == 2)
                            && StringUtil.IsHanzi(e.KeyChar))
                        {
                            Console.Beep(); // 表示拒绝了输入的字符
                            e.Handled = true;
                            break;
                        }

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
                                if (this.Text.Length >= this.MaxLength) // 2009/3/6 changed
                                {
                                    // 为调试准备的信息
                                    string strDebugInfo = "删除前的 text [" + this.Text + "] hex[" + GetTextHex(this.Text) + "], this.MaxLength=" + this.MaxLength + ", this.Text.Length=" + this.Text.Length;

                                    try
                                    {
                                        this.TextWithHeight = this.Text.Remove(this.SelectionStart, 1 + (this.Text.Length - this.MaxLength));
                                        this.SelectionStart = nOldSelectionStart;
                                    }
                                    catch (ArgumentOutOfRangeException ex)
                                    {
                                        throw new Exception("default overwrite 发生异常:" + strDebugInfo, ex);
                                    }
                                }
                                this.ContentIsNull = false; // 2017/1/15 防止首次在 MyEdit 中输入无法兑现到内存
                            }
                            else
                            {
                                // Console.Beep(); // 表示拒绝了输入的字符
                            }
                        }
                    }
                    break;
            }

            base.OnKeyPress(e);
        }

        static string GetTextHex(string strText)
        {
            StringBuilder result = new StringBuilder();
            int i = 0;
            foreach (char ch in strText)
            {
                string strHex = Convert.ToString(ch, 16).PadLeft(2, '0');

                result.Append(i.ToString() + ":" + strHex + ",");
                i++;
            }

            return result.ToString();
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
            return IsInForbiddenPos(this.Text, index);
        }

        static bool IsInForbiddenPos(string text, int index)
        {
            if (index <= 0)
                return false;

            if (index >= text.Length)
                return false;

            char left = text[index - 1];
            char current = text[index];

            // 插入符避免处在方向符号和子字段符号(31)
            if (left == 0x200e
                && (current == Record.KERNEL_SUBFLD || current == 0x001f))
                return true;

            return false;
        }
#endif

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            _selection_start = this.SelectionStart;
            //Debug.WriteLine($"ontextchanged 记忆 _current_selection_start={_selection_start}");

        }

        int _inSendKey = 0;
        int _selection_start = -1;
        int _caret_pos = -1;

        void ComputeCaretPos()
        {
            var current_selection_start = this.SelectionStart;
            var current_selection_length = this.SelectionLength;
            var shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            if (shift == false)
            {
                _selection_start = current_selection_start;
                //Debug.WriteLine($"compute 记忆 _current_selection_start={_selection_start}");
            }

            //Debug.WriteLine($"current_s_s={current_selection_start}, current_s_l={current_selection_length}, _selection_start={_selection_start}");

            _caret_pos = current_selection_start;
            if (shift)
            {
                if (current_selection_start == _selection_start)
                    _caret_pos = current_selection_start + current_selection_length;
                else
                    _caret_pos = current_selection_start;
            }
        }

        // 上下移动插入符时要遵循的 x 坐标像素位置。这个位置会被左右移动插入符时改变
        int _upDown_x = 0;

        void MemoryCaretX(bool delay = false)
        {
            if (_inSimulateClicking)
                return;
            if (delay)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(100);
                    this.TryInvoke(() =>
                    {
                        MemoryCaretX(false);
                    });
                });
                return;
            }

            POINT point = new POINT();
            point.x = 0;
            point.y = 0;
            bool bRet = API.GetCaretPos(ref point);
            _upDown_x = point.x;
        }

        // 按下键
        protected override void OnKeyDown(KeyEventArgs e)
        {
#if REMOVED
            {
                // testing
                Debug.WriteLine($"OnKeyDown {e}");
                if (e.KeyCode == Keys.A)
                {
                    int i = 0;
                    i++;
                }
                base.OnKeyDown(e);
                return;
            }
#endif

            if (_inSendKey > 0)
            {
                base.OnKeyDown(e);
                return;
            }

            // 确保阻塞的 DoMemoryCaretPos() 执行
            var shift = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            int col = this.MarcEditor.m_nFocusCol;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        if (this.MarcEditor.BlockUp())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.GetEditCurrentCaretPos(this,
                        out int x,
                        out int y);
                        // TODO: 向上离开当前字段，如果检测到此时按下了 Shift，要把当前字段作为起点，进入连续定义字段块的状态
                        if (y == 0 && this.MarcEditor.FocusedFieldIndex != 0)		// 虽然现在是0，但是马上会是-1
                        {
                            if (this.MarcEditor.BlockStart(-1))
                            {
                                e.Handled = true;
                                return;
                            }

                            /*
                            // 先做
                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);
                            */

                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex - 1,
                                col);

                            _inSimulateClicking = true;
                            try
                            {
                                POINT point = new POINT();
                                point.x = _upDown_x;
                                point.y = this.ClientSize.Height - 2;
                                API.SendMessage(this.Handle,
                                    API.WM_LBUTTONDOWN,
                                    new UIntPtr(API.MK_LBUTTON),    //	UIntPtr wParam,
                                    API.MakeLParam(point.x, point.y));

                                API.SendMessage(this.Handle,
                                    API.WM_LBUTTONUP,
                                    new UIntPtr(API.MK_LBUTTON),    //	UIntPtr wParam,
                                    API.MakeLParam(point.x, point.y));
                            }
                            finally
                            {
                                _inSimulateClicking = false;
                            }
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
                        if (this.MarcEditor.BlockDown())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.GetEditCurrentCaretPos(this,
                            out int x,
                            out int y);
                        int nTemp = API.GetEditLines(this);
                        if (y >= nTemp - 1
                            && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                        {
                            var has_selection = this.SelectionLength > 0;
                            var at_tail = _caret_pos >= this.Text.Length;

                            if (at_tail == false || has_selection == true)
                            {
                                if (this.MarcEditor.BlockStart(1))
                                {
                                    e.Handled = true;
                                    return;
                                }
                            }


                            /*
                            // 先做
                            POINT point = new POINT();
                            point.x = 0;
                            point.y = 0;
                            bool bRet = API.GetCaretPos(ref point);
                            */

                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, col);

                            if (!(at_tail == false || has_selection == true))
                            {
                                // (当前字段已经处在下一个字段上)定义本字段为一个块
                                if (this.MarcEditor.BlockStart(0))   // 0, 1
                                {
                                    e.Handled = true;
                                    return;
                                }
                            }


                            {
                                _inSimulateClicking = true;
                                try
                                {
                                    POINT point = new POINT();
                                    point.x = _upDown_x;
                                    point.y = 1;
                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONDOWN,
                                        new UIntPtr(API.MK_LBUTTON),    //	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));

                                    API.SendMessage(this.Handle,
                                        API.WM_LBUTTONUP,
                                        new UIntPtr(API.MK_LBUTTON),    //	UIntPtr wParam,
                                        API.MakeLParam(point.x, point.y));
                                }
                                finally
                                {
                                    _inSimulateClicking = false;
                                }
                                e.Handled = true;
                            }

                        }
#if BIDI_SUPPORT
                        // 插入符避免处在方向符号和子字段符号(31)之间
                        API.PostMessage(this.Handle, WM_ADJUST_CARET, 0, 0);
#endif
                    }
                    break;
                case Keys.Left:
                    {
                        if (this.SelectionLength > 0 && shift == false)
                        {
                            this.SelectionLength = 0;
                            e.Handled = true;
                            return;
                            break;
                        }

                        MemoryCaretX(true);

                        if (this.MarcEditor.BlockUp())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.PostMessage(this.MarcEditor.Handle,
    MarcEditor.WM_LEFTRIGHT_MOVED,
    0,
    0);


                        if (this.SelectionStart != 0)
                        {
#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间

                            if (IsInForbiddenPos(_caret_pos - 1) == true
                                || IsInForbiddenPos(_caret_pos) == true)
                            {
                                if (shift)
                                {
                                    // 模拟左键
                                    _inSendKey++;
                                    try
                                    {
                                        SendKeys.SendWait("{LEFT}");
                                    }
                                    finally
                                    {
                                        _inSendKey--;
                                    }
                                    // 2024/7/5
                                    if (this.SelectionStart == 0)
                                        goto LEFT_MOST;
                                }
                                else
                                    this.SelectionStart--;
                            }
#endif
                            break;
                        }

                    LEFT_MOST:
                        Debug.Assert(this.SelectionStart == 0);

                        if (col == 1)
                        {
                            if (this.MarcEditor.FocusedFieldIndex > 0)
                            {
                                if (this.MarcEditor.BlockStart(0))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                // 到达上一行的末尾
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex - 1, 3);
                                this.SelectionStart = this.Text.Length;
                                e.Handled = true;
                                return;
                            }
                        }
                        else if (col == 2)
                        {
                            if (this.MarcEditor.BlockStart(0))
                            {
                                e.Handled = true;
                                return;
                            }

                            // 从指示符到字段名
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                            this.SelectionStart = 2;
                            e.Handled = true;
                            return;
                        }
                        else if (col == 3)
                        {
                            if (this.MarcEditor.BlockStart(0))
                            {
                                e.Handled = true;
                                return;
                            }

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
                        if (this.SelectionLength > 0 && shift == false)
                        {
                            this.SelectionStart = this.SelectionStart + this.SelectionLength;
                            this.SelectionLength = 0;
                            e.Handled = true;
                            return;
                            break;
                        }

                        MemoryCaretX(true);

                        if (this.MarcEditor.BlockDown())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.PostMessage(this.MarcEditor.Handle,
MarcEditor.WM_LEFTRIGHT_MOVED,
0,
0);

                        if (col == 1)
                        {
                            // 从字段名到指示符
                            if ((shift == false && _caret_pos >= 2)
                                || (shift == true && _caret_pos >= 3))
                            {
                                if (this.MarcEditor.BlockStart(0))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                // 控制字段没有指示符, 直接到内容
                                if (Record.IsControlFieldName(this.MarcEditor.FocusedField.Name) == true)
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                else
                                    this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 2);

                                this.SelectionStart = 0;
                                e.Handled = true;
                            }
                        }
                        else if (col == 2)
                        {
                            // 从指示符到内容
                            // if (this.SelectionStart == 1 || this.SelectionStart == 2)
                            if ((shift == false && _caret_pos >= 1)
                                || (shift == true && _caret_pos >= 2))
                            {
                                if (this.MarcEditor.BlockStart(0))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                this.SelectionStart = 0;
                                e.Handled = true;
                            }
                        }
                        else if (col == 3)
                        {
                            // TODO: 记下 按下 Shift 以后首次 left right 移动时候的起点位置。
                            // 根据当前 caret 在起点位置的左边或者右边，进行不同的判断

                            // Debug.WriteLine($"shift={shift}, caret_pos={_caret_pos}");

                            // 从内容末尾到下一行首部
                            if ((shift == false && _caret_pos >= this.Text.Length
                                || (shift == true && _caret_pos >= this.Text.Length))
                                && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                            {
                                var has_selection = this.SelectionLength > 0;
                                if (has_selection)
                                {
                                    if (this.MarcEditor.BlockStart(+1))   // 0, 1
                                    {
                                        e.Handled = true;
                                        return;
                                    }
                                }

                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, 1);
                                this.SelectionStart = 0;

                                if (has_selection == false)
                                {
                                    // (当前字段已经处在下一个字段上)定义本字段为一个块
                                    if (this.MarcEditor.BlockStart(0))   // 0, 1
                                    {
                                        e.Handled = true;
                                        // return;
                                    }
                                }

                                e.Handled = true;
                                break;
                            }

#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间
                            if (IsInForbiddenPos(_caret_pos + 1) == true)
                            {
                                if (shift)
                                {
                                    // 模拟右键
                                    _inSendKey++;
                                    try
                                    {
                                        SendKeys.SendWait("{RIGHT}");
                                    }
                                    finally
                                    {
                                        _inSendKey--;
                                    }
                                }
                                else
                                    this.SelectionStart++;
                            }
#endif
                        }

                    }
                    break;
                case Keys.End:
                    {
                        MemoryCaretX(true);

                        if (col == 3)
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
                        MemoryCaretX(true);

                        if (this.SelectionStart == 0)
                        {
                            if (col == 3)
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
                            else if (col == 2)
                            {
                                // 目前正在指示符上, 那就到字段名上
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                                this.SelectionStart = 0;
                                e.Handled = true;
                                return;
                            }
                            else if (col == 1)
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
                        if (e.Control)
                            break;

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
                        if (e.Control)
                            break;
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
                        if (col == 1
                            || col == 2)
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
                        if (col == 1
                            || col == 2)
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


                                this.TextWithHeight = this.Text.Insert(nStart, " ");
#if BIDI_SUPPORT
                                if (this.Text[nStart + 1] == 0x200e
                                    && this.Text.Length >= nStart + 2 + 1)
                                    this.TextWithHeight = this.Text.Remove(nStart + 1, 2);
                                else
                                    this.TextWithHeight = this.Text.Remove(nStart + 1, 1);
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
                                    this.TextWithHeight = this.Text.Remove(this.SelectionStart, 2);
                                    this.SelectionStart = nStart;
                                    e.Handled = true;
                                }
                                // 如果删除位置的左方正好是方向字符
                                else if (this.SelectionStart > 0
                                    && this.Text[this.SelectionStart - 1] == 0x200e)
                                {
                                    // 一同删除
                                    int nStart = this.SelectionStart;
                                    this.TextWithHeight = this.Text.Remove(this.SelectionStart - 1, 2);
                                    this.SelectionStart = nStart - 1;
                                    e.Handled = true;
                                }
                            }
#endif

                        }
                    }
                    break;
#if REMOVED
                case Keys.A:
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        MessageBox.Show(this, "catch Ctrl+A");
                        e.Handled = true;
                    }
                    break;
#endif
                default:
                    break;
            }

            base.OnKeyDown(e);

            // 延时执行切换区域的动作。目的是等待确保 TextBox 执行击键完成
            Task.Run(async () =>
            {
                await Task.Delay(100);
                this.TryInvoke(() =>
                {
                    DoMemoryCaretPos(e.KeyCode);
                });
            });
        }

        void DoMemoryCaretPos(Keys KeyCode)
        {
            /*
             * 经过推测，TextBox 内的执行顺序如下：
             * OnKeyDown()
             * 
             * 改变 SelectionStart 和 SelectionLength
             * 
             * OnMouseDown()
             * 
             * 可以看出，OnKeyDown() 时刻，SelectionStart 和 SelectionLength 还是上一次的状态，本次击键的改变还没有来得及发生
             * */
            ComputeCaretPos();

            KeyEventArgs e = new KeyEventArgs(KeyCode);

#if REMOVED
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        if (this.MarcEditor.BlockUp())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.GetEditCurrentCaretPos(this,
                        out int x,
                        out int y);
                        // TODO: 向上离开当前字段，如果检测到此时按下了 Shift，要把当前字段作为起点，进入连续定义字段块的状态
                        if (y == 0 && this.MarcEditor.FocusedFieldIndex != 0)		// 虽然现在是0，但是马上会是-1
                        {
                            if (this.MarcEditor.BlockStart(-1))
                            {
                                e.Handled = true;
                                return;
                            }

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
                        if (this.MarcEditor.BlockDown())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.GetEditCurrentCaretPos(this,
                            out int x,
                            out int y);
                        int nTemp = API.GetEditLines(this);
                        if (y >= nTemp - 1
                            && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                        {
                            if (this.MarcEditor.BlockStart(1))
                            {
                                e.Handled = true;
                                return;
                            }

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
                        if (this.MarcEditor.BlockUp())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.PostMessage(this.MarcEditor.Handle,
    MarcEditor.WM_LEFTRIGHT_MOVED,
    0,
    0);


                        if (this.SelectionStart != 0)
                        {
#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间

                            if (IsInForbiddenPos(this.SelectionStart - 1) == true
                                || IsInForbiddenPos(this.SelectionStart) == true)
                            {
                                if (this.SelectionLength > 0)
                                {
                                    // 模拟左键
                                    _inSendKey++;
                                    try
                                    {
                                        SendKeys.SendWait("{LEFT}");
                                    }
                                    finally
                                    {
                                        _inSendKey--;
                                    }
                                    // 2024/7/5
                                    if (this.SelectionStart == 0)
                                        goto LEFT_MOST;
                                }
                                else
                                    this.SelectionStart--;
                            }
#endif
                            break;
                        }

                    LEFT_MOST:
                        Debug.Assert(this.SelectionStart == 0);

                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                            if (this.MarcEditor.FocusedFieldIndex > 0)
                            {
                                if (this.MarcEditor.BlockStart(-1))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                // 到达上一行的末尾
                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex - 1, 3);
                                this.SelectionStart = this.Text.Length;
                                e.Handled = true;
                                return;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 2)
                        {
                            if (this.MarcEditor.BlockStart(0))
                            {
                                e.Handled = true;
                                return;
                            }

                            // 从指示符到字段名
                            this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 1);
                            this.SelectionStart = 2;
                            e.Handled = true;
                            return;
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            if (this.MarcEditor.BlockStart(0))
                            {
                                e.Handled = true;
                                return;
                            }

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
                        if (this.MarcEditor.BlockDown())
                        {
                            e.Handled = true;
                            return;
                        }

                        API.PostMessage(this.MarcEditor.Handle,
MarcEditor.WM_LEFTRIGHT_MOVED,
0,
0);

                        if (this.MarcEditor.m_nFocusCol == 1)
                        {
                            // 从字段名到指示符
                            if ((shift == false && _caret_pos >= 2)
                                || (shift == true && _caret_pos >= 3))
                            {
                                if (this.MarcEditor.BlockStart(0))
                                {
                                    e.Handled = true;
                                    return;
                                }

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
                            // if (this.SelectionStart == 1 || this.SelectionStart == 2)
                            if ((shift == false && _caret_pos >= 1)
                                || (shift == true && _caret_pos >= 2))
                            {
                                if (this.MarcEditor.BlockStart(0))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex, 3);
                                this.SelectionStart = 0;
                                e.Handled = true;
                            }
                        }
                        else if (this.MarcEditor.m_nFocusCol == 3)
                        {
                            // TODO: 记下 按下 Shift 以后首次 left right 移动时候的起点位置。
                            // 根据当前 caret 在起点位置的左边或者右边，进行不同的判断

                            // Debug.WriteLine($"shift={shift}, caret_pos={_caret_pos}");

                            // 从内容末尾到下一行首部
                            if ((shift == false && _caret_pos >= this.Text.Length
                                || (shift == true && _caret_pos >= this.Text.Length))
                                && this.MarcEditor.FocusedFieldIndex < this.MarcEditor.Record.Fields.Count - 1)
                            {
                                if (this.MarcEditor.BlockStart(+1))
                                {
                                    e.Handled = true;
                                    return;
                                }

                                this.MarcEditor.SetActiveField(this.MarcEditor.FocusedFieldIndex + 1, 1);
                                this.SelectionStart = 0;
                                e.Handled = true;
                                break;
                            }

#if BIDI_SUPPORT
                            // 插入符避免处在方向符号和子字段符号(31)之间
                            if (IsInForbiddenPos(this.SelectionStart + this.SelectionLength + 1) == true)
                            {
                                if (this.SelectionLength > 0)
                                {
                                    // 模拟右键
                                    _inSendKey++;
                                    try
                                    {
                                        SendKeys.SendWait("{RIGHT}");
                                    }
                                    finally
                                    {
                                        _inSendKey--;
                                    }
                                }
                                else
                                    this.SelectionStart++;
                            }
#endif
                        }

                    }
                    break;

            }

#endif
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
#if REMOVED
            // testing
            base.OnKeyUp(e);
            return;
#endif

            switch (e.KeyCode)
            {
                case Keys.PageUp:
                case Keys.PageDown:
                    if (e.Control)
                        break;
                    e.Handled = true;
                    return;
            }

            //键不带Shift时，把MarcEditor的Shift选中起始位置设为0
            //if (e.Shift == false)
            //    this.MarcEditor.nStartFieldIndex = -1;

            base.OnKeyUp(e);
            this.MarcEditor.Flush();    // 2011/8/8

            switch (e.KeyCode)
            {
                case Keys.PageUp:
                case Keys.PageDown:
                    if (e.Control)
                        break;
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
                            // SetHeight();
#if REMOVED
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
#endif
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

            // 2024/7/4
            if (this.MarcEditor.SelectedFieldIndices.Count <= 1)
            {
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
        }

        int _lineCount = 1;

        public void ClearSetHeight()
        {
            _lineCount = 0;
        }

        // 根据文本内容的折行 自动调整 TextBox 高度
        public void SetHeight()
        {
            var numberOfLines = API.SendMessage(this.Handle, API.EM_GETLINECOUNT, 0, 0).ToInt32();
            numberOfLines = Math.Max(1, numberOfLines);

            if (numberOfLines != _lineCount)
            {
                int nBorderWidth = 0;
                if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
                    nBorderWidth = SystemInformation.Border3DSize.Height * 2;
                else if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
                    nBorderWidth = SystemInformation.BorderSize.Height * 2;

                /*
                 * Set TextBox Height -
http://www.codeproject.com/Articles/29140/Set-TextBox-Height
Textboxes have a 3-pixel lower and 4-pixel upper white space around
the font height. Therefore, the calculation increases the height by 7
pixels.
                 * */
                int nNewHeight = (this.Font.Height) * numberOfLines + nBorderWidth; // +7
                if (this.Height != nNewHeight)
                {
                    this.Height = nNewHeight;
                    // this.OnHeightChanged();
                    this.MarcEditor.Flush();
                    this.MarcEditor.AfterItemHeightChanged(this.MarcEditor.FocusedFieldIndex,
    -1);
                }

                _lineCount = numberOfLines;
            }
        }

        // 修改 Text 属性的同时自动修正 TextBox 的 Height
        public string TextWithHeight
        {
            set
            {
                // TextChanged 事件被触发
                base.Text = value;
            }
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                // 禁止 TextChanged 事件被触发
                bool changed = false;
                if (this.m_marcEditor != null)
                {
                    this.m_marcEditor._disableTextChanged++;
                    changed = true;
                }
                try
                {
                    base.Text = value;
                }
                finally
                {
                    if (changed)
                        this.m_marcEditor._disableTextChanged--;
                }
            }
        }


        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (this.Visible == true)
                _lineCount = 0; // 迫使后面重新初始化 Height
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

            //Debug.WriteLine("little edit Enter");
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            //Debug.WriteLine("little edit leave");
        }

        // http://stackoverflow.com/a/3955553/168235
        #region Custom Word-Wrapping (Output Window)

        const uint EM_SETWORDBREAKPROC = 0x00D0;

        [DllImport("user32.dll")]
        extern static IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

        delegate int EditWordBreakProc(IntPtr text, int pos_in_text, int bCharSet, int action);

        private delegate int SetWordBreakProc(IntPtr text, int pos_in_text, int bCharSet, int action);
        private readonly SetWordBreakProc _wordBreakCallback = (text, pos_in_text, bCharSet, action) => { return pos_in_text; };

        void SetNewWordBreak()
        {
            IntPtr ptr_func = Marshal.GetFunctionPointerForDelegate(_wordBreakCallback);

            SendMessage(this.Handle, EM_SETWORDBREAKPROC, IntPtr.Zero, ptr_func);
        }

        #endregion
    }
}
