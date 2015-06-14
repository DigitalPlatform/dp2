using System;
using System.Windows.Forms;

using DigitalPlatform;

namespace DigitalPlatform.Marc
{
	public class ValueEditBox : TextBox
	{
		public MarcFixedFieldControl fixedFieldCtrl = null;
		public int nIndex = -1;
		
		protected override void OnGotFocus(EventArgs e)
		{
            base.OnGotFocus(e);    // ??

            /* 要变成异步的调用
			fixedFieldCtrl.nCurLine = this.nIndex;
			fixedFieldCtrl.ShowValueList(this.SelectionStart,
                this.MaxLength,
                this.Text); // 还需要给出当前插入符位置
             * */
            fixedFieldCtrl.BeginShowValueList(this.nIndex,
                this.SelectionStart,
                this.MaxLength,
                this.Text);

		}

        // 接管Ctrl+各种键
        protected override bool ProcessDialogKey(
            Keys keyData)
        {

            // Ctrl + A 自动录入功能
            if ((keyData & Keys.Control) == Keys.Control
                && (keyData & (~Keys.Control)) == Keys.A)
            {

                // MessageBox.Show(this, "Ctrl+A");
                string strValue = "";
                string strError = "";

                //
                Cursor oldcursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;   // 出现沙漏

                int nOldSelectionStart = this.SelectionStart;
                this.Enabled = false;
                //

                int nRet = fixedFieldCtrl.GetDefaultValue(this.nIndex,
                    out strValue,
                    out strError);

                //
                this.Enabled = true;
                this.Focus();
                this.SelectionStart = nOldSelectionStart;

                this.Cursor = oldcursor;
                //


                if (nRet == 0)
                {
                    Console.Beep();
                }
                else if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                {
                    if (strValue == null)
                        strValue = "";

                    if (strValue.Length < this.MaxLength)
                    {
                        strValue = strValue.PadRight(this.MaxLength, ' ');
                        Console.Beep();
                    }
                    else if (strValue.Length > this.MaxLength)
                    {
                        strValue = strValue.Substring(0, this.MaxLength);
                        Console.Beep();
                    }

                    this.Text = strValue;
                }

                return true;
            }

            return false;
        }

        // 按下键
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    {
                        if (Control.ModifierKeys == Keys.Shift)
                        {
                            if (this.nIndex != 0)
                            {
                                fixedFieldCtrl.SwitchFocus(this.nIndex - 1,
                                    CaretPosition.FirstChar);
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            if (this.nIndex < fixedFieldCtrl.LineCount - 1)
                            {
                                fixedFieldCtrl.SwitchFocus(this.nIndex + 1,
                                    CaretPosition.FirstChar);
                                e.Handled = true;
                            }
                        }
                    }
                    break;
                case Keys.Up:
                    {
                        if (this.nIndex != 0)
                        {
                            fixedFieldCtrl.SwitchFocus(this.nIndex - 1,
                                CaretPosition.FirstChar);
                            e.Handled = true;
                        }
                    }
                    break;
                case Keys.Down:
                    {
                        if (this.nIndex < fixedFieldCtrl.LineCount - 1)
                        {
                            fixedFieldCtrl.SwitchFocus(this.nIndex + 1,
                                CaretPosition.FirstChar);
                            e.Handled = true;
                        }
                    }
                    break;
                case Keys.Left:
                    {
                        if (this.SelectionStart != 0)
                            break;

                        if (this.nIndex != 0)
                        {
                            fixedFieldCtrl.SwitchFocus(this.nIndex - 1,
                                CaretPosition.LastChar);
                            e.Handled = true;
                        }


                    }
                    break;
                case Keys.Right:    // 右方向键
                    {
                        if (this.SelectionStart < this.MaxLength - 1)
                            break;

                        if (this.nIndex < fixedFieldCtrl.LineCount - 1)
                        {
                            fixedFieldCtrl.SwitchFocus(this.nIndex + 1,
                                CaretPosition.FirstChar);
                            e.Handled = true;
                        }

                    }
                    break;
                case Keys.End:
                    {

                    }
                    break;
                case Keys.Home:
                    {

                    }
                    break;
                case Keys.PageUp:
                    {

                    }
                    break;
                case Keys.PageDown:
                    {

                    }
                    break;
                case Keys.Insert:
                    {

                    }
                    break;
                case Keys.Delete:
                    {
                        // 禁止Delete键的作用 2008/5/27
                        Console.Beep();
                        e.Handled = true;
                    }
                    break;
                case Keys.Back:
                    {
                        /* 不知这里为什么不管用
                        // 禁止Backspace键的作用 2008/7/4
                        Console.Beep();
                        e.Handled = true;
                        return;
                         * */
                    }
                    break;
                default:
                    break;
            }

            base.OnKeyDown(e);
        }

        public void NextLineIfNeed()
        {
            if (this.SelectionStart >= this.MaxLength)
            {
                if (this.nIndex < fixedFieldCtrl.LineCount - 1)
                {
                    ValueEditBox target = fixedFieldCtrl.SwitchFocus(this.nIndex + 1,
                        CaretPosition.FirstChar);

                    if (target != null)
                    {
                        // 2011/3/20
                        // 刷新值列表
                        fixedFieldCtrl.ShowValueList(target.SelectionStart,
                            target.MaxLength,
                            target.Text);
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            // 刷新值列表
            fixedFieldCtrl.ShowValueList(this.SelectionStart,
                this.MaxLength,
                this.Text); 

        }

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
                        int nKey = API.LoWord(m.WParam.ToInt32());
                        switch (nKey)
                        {
                            case (int)Keys.Tab:
                                {
                                    // 禁止Tab键的作用 2008/7/28
                                    Console.Beep();
                                    return;
                                }
                                break;
                            case (int)Keys.Enter:
                                {
                                }
                                break;
                            case (int)Keys.Back:
                                {
                                    // 禁止Backspace键的作用 2008/7/4
                                    Console.Beep();
                                    return;
                                }
                                break;
                            default:
                                {
                                        if ((Control.ModifierKeys == Keys.Control)
                                            // || Control.ModifierKeys == Keys.Shift    // 2009/9/18 changed
                                            || Control.ModifierKeys == Keys.Alt)
                                        {
                                            break;
                                        }
                                        int nOldSelectionStart = this.SelectionStart;
                                        if (nOldSelectionStart < this.Text.Length)
                                        {
                                            // 避免多余的TextChanged动作
                                            this.fixedFieldCtrl.m_nDisableTextChanged++;
                                            this.Text = this.Text.Remove(this.SelectionStart, 1);
                                            this.fixedFieldCtrl.m_nDisableTextChanged--;
                                            this.SelectionStart = nOldSelectionStart;
                                        }

                                        base.DefWndProc(ref m);

                                        if (this.SelectionStart >= this.MaxLength
                                            && this.nIndex < fixedFieldCtrl.LineCount - 1)
                                        {
                                                fixedFieldCtrl.SwitchFocus(this.nIndex + 1,
                                                    CaretPosition.FirstChar);
                                        }

                                        return;
                                }
                                break;
                        }
                    }
                    break;
            }
            base.DefWndProc(ref m);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                fixedFieldCtrl.ChangeFocusDisplay(this);

                if (this.SelectionStart == this.MaxLength)
                {
                    this.SelectionStart = this.MaxLength - 1;
                    this.SelectionLength = 0;
                    return;
                }

                // 刷新值列表
                fixedFieldCtrl.ShowValueList(this.SelectionStart,
                    this.MaxLength,
                    this.Text); 

            }
        }
	}
}
