using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;
using System.Windows.Forms;

using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using System.Drawing;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 图形控件的状态用字符串表示
    /// </summary>
    public static class GuiState
    {
        static string EncryptKey = "ui_key";

        internal static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        internal static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        // 恢复 DpTable 的状态
        // 目前是栏宽度
        static void SetDpTableState(DpTable table, string strText)
        {
            string strState = "";
            if (IsType(strText, table, out strState) == false)
                return;
            DpTable.SetColumnHeaderWidth(table,
                strState,
                true);
        }

        static string GetDpTableState(DpTable table)
        {
            return table.GetType().ToString() + ":" + DpTable.GetColumnWidthListString(table);
        }

        static void SetSavePasswordState(SavePassword container, string strText)
        {
            if (IsType(strText, container, out string strState) == false)
                return;

            TextBox textbox = container.PasswordTextBox;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string strPassword = (string)table["password"];
                string strSave = (string)table["save"];

                bool bSave = DomUtil.IsBooleanTrue(strSave);
                container.SaveOrNotCheckBox.Checked = bSave;

                if (bSave == true && string.IsNullOrEmpty(strPassword) == false)
                {
                    container.PasswordTextBox.Text = DecryptPasssword(strPassword);
                }
            }
        }

        static string GetSavePasswordState(SavePassword container)
        {
            Hashtable table = new Hashtable();
            if (container.SaveOrNotCheckBox.Checked == true)
                table["password"] = EncryptPassword(container.PasswordTextBox.Text);
            table["save"] = container.SaveOrNotCheckBox.Checked == true ? "true" : "false";

            return container.GetType().ToString() + ":"
                + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        static void SetRadioButtonState(RadioButton radio, string strText)
        {
            string strState = "";
            if (IsType(strText, radio, out strState) == false)
                return;
            if (string.IsNullOrEmpty(strState) == false)
                radio.Checked = DomUtil.IsBooleanTrue(strState);
        }

        static string GetRadioButtonState(RadioButton radio)
        {
            return radio.GetType().ToString() + ":" + (radio.Checked == true ? "1" : "0");
        }

        static void SetCheckedComboBoxState(CheckedComboBox combobox, string strText, object default_value)
        {
            if (string.IsNullOrEmpty(strText) == true
    && default_value != null)
            {
                if (default_value is string)
                    combobox.Text = (default_value as string);
                else
                    throw new ArgumentException("CheckedComboBox 的缺省值应当为 string 类型", "default_value");
                return;
            }

            string strState = "";
            if (IsType(strText, combobox, out strState) == false)
                return;
            combobox.Text = StringUtil.UnescapeString(strState);
        }

        static string GetCheckedComboBoxState(CheckedComboBox combobox)
        {
            return combobox.GetType().ToString() + ":" + StringUtil.EscapeString(combobox.Text, ":;,");
        }

        //
        static void SetTextBoxState(TextBox textbox, string strText, object default_value)
        {
            if (string.IsNullOrEmpty(strText) == true
    && default_value != null)
            {
                if (default_value is string)
                    textbox.Text = (default_value as string);
                else
                    throw new ArgumentException("TextBox 的缺省值应当为 string 类型", "default_value");
                return;
            }

            if (IsType(strText, textbox, out string strState) == false)
                return;

            textbox.Text = StringUtil.UnescapeString(strState);
        }

        static string GetTextBoxState(TextBox textbox)
        {
            return textbox.GetType().ToString() + ":" + StringUtil.EscapeString(textbox.Text, ":;,");
        }

        static void SetCheckedListBoxState(CheckedListBox list, string strText)
        {
            string strState = "";
            if (IsType(strText, list, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                List<string> values = StringUtil.SplitList(strState);
                for (int i = 0; i < list.Items.Count; i++)
                {
                    string s = (string)list.Items[i];

                    int index = values.IndexOf(s);
                    if (index != -1)
                        list.SetItemChecked(i, true);
#if NO
                    else
                    {
                        list.Items.Add(s);
                        list.SetItemChecked(list.Items.Count - 1, true);
                    }
#endif
                }
            }
        }

        static string GetCheckListBoxState(CheckedListBox list)
        {
            StringBuilder text = new StringBuilder();
            foreach (string s in list.CheckedItems)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(s);
            }

            return list.GetType().ToString() + ":"
                + text.ToString();
        }

        static void SetCheckBoxState(CheckBox checkbox, string strText, object default_value)
        {
            string strState = "";
            if (string.IsNullOrEmpty(strText) == true
                && default_value != null)
            {
                if (default_value is bool)
                    checkbox.Checked = (bool)default_value;
                else if (default_value is string)
                    checkbox.Checked = DomUtil.IsBooleanTrue(default_value as string);
                else
                    throw new ArgumentException("CheckBox 的缺省值应当为 bool 或 string 类型", "default_value");
                return;
            }
            if (IsType(strText, checkbox, out strState) == false)
                return;
            if (string.IsNullOrEmpty(strState) == false)
                checkbox.Checked = DomUtil.IsBooleanTrue(strState);
        }

        static string GetCheckBoxState(CheckBox checkbox)
        {
            return checkbox.GetType().ToString() + ":" + (checkbox.Checked == true ? "1" : "0");
        }

        static void SetTabControlState(TabControl tab, string strText)
        {
            string strState = "";
            if (IsType(strText, tab, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                int i = 0;
                int.TryParse(strState, out i);
                tab.SelectedIndex = i;
            }
        }

        static string GetTabControlState(TabControl tab)
        {
            return tab.GetType().ToString() + ":"
                + tab.SelectedIndex.ToString();
        }

        static void SetTabComboBoxState(TabComboBox combobox, string strText)
        {
            string strState = "";
            if (IsType(strText, combobox, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string strStyle = (string)table["style"];
                string strValue = (string)table["text"];

                if (string.IsNullOrEmpty(strValue) == false)
                {
                    ComboBoxStyle style;
                    Enum.TryParse<ComboBoxStyle>(strStyle, out style);

                    combobox.Text = strValue;

                    if (combobox.Text != strValue)
                    {
                        combobox.Items.Add(strValue);
                        combobox.Text = strValue;
                    }
                }
            }
        }

        static string GetTabComboBoxState(TabComboBox combobox)
        {
            Hashtable table = new Hashtable();
            table["style"] = combobox.DropDownStyle.ToString();
            table["text"] = combobox.Text;

            return combobox.GetType().ToString() + ":"
                + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        static void SetComboBoxTextState(ComboBoxText container,
            string strText,
            object default_value)
        {
            string strState = "";

            ComboBox combobox = container.ComboBox;

            if (string.IsNullOrEmpty(strText) == true
    && default_value != null)
            {
                if (default_value is int)
                    combobox.SelectedIndex = (int)default_value;
                else if (default_value is string)
                    combobox.Text = (string)default_value;
                else
                    throw new ArgumentException("ComboBoxText 的缺省值应当为 int 或 string 类型", "default_value");
                return;
            }

            if (IsType(strText, container, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string strStyle = (string)table["style"];
                string strValue = (string)table["text"];

                ComboBoxStyle style;
                Enum.TryParse<ComboBoxStyle>(strStyle, out style);

                if (string.IsNullOrEmpty(strValue) == false)
                {
                    combobox.Text = strValue;

                    if (combobox.Text != strValue)
                    {
                        combobox.Items.Add(strValue);
                        combobox.Text = strValue;
                    }
                }
            }
        }

        static string GetComboBoxTextState(ComboBoxText container)
        {
            ComboBox combobox = container.ComboBox;

            Hashtable table = new Hashtable();
            table["style"] = combobox.DropDownStyle.ToString();
            table["text"] = combobox.Text;

            return container.GetType().ToString() + ":"
                + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        static void SetComboBoxState(ComboBox combobox,
            string strText,
            object default_value)
        {
            string strState = "";
            if (string.IsNullOrEmpty(strText) == true
&& default_value != null)
            {
                if (default_value is int)
                    combobox.SelectedIndex = (int)default_value;
                else if (default_value is string)
                    combobox.Text = (string)default_value;
                else
                    throw new ArgumentException("ComboBox 的缺省值应当为 int 或 string 类型", "default_value");
                return;
            }

            if (IsType(strText, combobox, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string strStyle = (string)table["style"];
                string strIndex = (string)table["index"];

                ComboBoxStyle style;
                Enum.TryParse<ComboBoxStyle>(strStyle, out style);

                if (style == ComboBoxStyle.DropDownList)
                {
                    int i = 0;
                    int.TryParse(strIndex, out i);
                    try
                    {
                        combobox.SelectedIndex = i;
                    }
                    catch
                    {
                    }
                }
                else if (style == ComboBoxStyle.DropDown)
                {
                    // TODO: 恢复 .Text ?
                    int i = 0;
                    int.TryParse(strIndex, out i);
                    try
                    {
                        combobox.SelectedIndex = i;
                    }
                    catch
                    {
                    }
                }
            }
        }

        static string GetComboBoxState(ComboBox combobox)
        {
            Hashtable table = new Hashtable();
            table["style"] = combobox.DropDownStyle.ToString();
            table["index"] = combobox.SelectedIndex.ToString();

            return combobox.GetType().ToString() + ":"
                + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        // 恢复 ListView 的状态
        // 目前是栏宽度
        static void SetListViewState(ListView listview, string strText)
        {
            string strState = "";
            if (IsType(strText, listview, out strState) == false)
                return;
            ListViewUtil.SetColumnHeaderWidth(listview,
                strState,
                true);
        }

        // 观察是否为指定的类型，并返回状态内容字符串部分
        static bool IsType(string strText,
            object control,
            out string strState)
        {
            string strType = "";
            strState = "";
            StringUtil.ParseTwoPart(strText, ":", out strType, out strState);
            if (control.GetType().ToString() != strType)
                return false;
            return true;
        }

        static string GetListViewState(ListView listview)
        {
            return listview.GetType().ToString() + ":" + ListViewUtil.GetColumnWidthListString(listview);
        }

#if NO
        static void SetSplitContainerState(SplitContainer splitContainer, string strText)
        {
            string strState = "";
            if (IsType(strText, splitContainer, out strState) == false)
                return; 
            
            if (string.IsNullOrEmpty(strState) == false)
            {
                float f = 0.5F;
                float.TryParse(strState, out f);
                GuiUtil.SetSplitterState(splitContainer, f);
            }
        }

        static string GetSplitContainerState(SplitContainer splitContainer)
        {
            return splitContainer.GetType().ToString() + ":" + GuiUtil.GetSplitterState(splitContainer).ToString();
        }
#endif

        // 2015/5/25 增加存储方向的能力
        static void SetSplitContainerState(SplitContainer splitContainer, string strText)
        {
            string strState = "";
            if (IsType(strText, splitContainer, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string strRatio = (string)table["ratio"];
                string strOrientation = (string)table["orientation"];

                if (string.IsNullOrEmpty(strOrientation) == false)
                {
                    if (strOrientation == "v")
                        splitContainer.Orientation = Orientation.Vertical;
                    else
                        splitContainer.Orientation = Orientation.Horizontal;
                }

                if (string.IsNullOrEmpty(strRatio) == true)
                {
                    float f = 0.5F;
                    float.TryParse(strState, out f);    // 兼容最早的用法，状态字符串仅仅是一个数字
                    GuiUtil.SetSplitterState(splitContainer, f);
                }
                else
                {
                    float f = 0.5F;
                    float.TryParse(strRatio, out f);
                    GuiUtil.SetSplitterState(splitContainer, f);
                }

            }
        }

        static void SetFormState(Form form, string strText)
        {
            string strState = "";
            if (IsType(strText, form, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                Hashtable table = StringUtil.ParseParameters(strState, ',', '=', "url");

                string s_w = (string)table["w"];
                string s_h = (string)table["h"];
                string s_x = (string)table["x"];
                string s_y = (string)table["y"];
                string s_s = (string)table["s"];

                int w = 0;
                int.TryParse(s_w, out w);
                int h = 0;
                int.TryParse(s_h, out h);
                int x = 0;
                int.TryParse(s_x, out x);
                int y = 0;
                int.TryParse(s_y, out y);

                form.Size = new Size(w, h);
                form.Location = new Point(x, y);
                if (String.IsNullOrEmpty(s_s) == false)
                {
                    form.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState),
                        s_s);
                }
            }
        }

        static string GetSplitContainerState(SplitContainer splitContainer)
        {
            Hashtable table = new Hashtable();
            table["ratio"] = GuiUtil.GetSplitterState(splitContainer).ToString();
            table["orientation"] = splitContainer.Orientation == Orientation.Vertical ? "v" : "h";
            return splitContainer.GetType().ToString() + ":" + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        static string GetFormState(Form form)
        {
            Hashtable table = new Hashtable();

            Size size = form.Size;
            Point location = form.Location;

            if (form.WindowState != FormWindowState.Normal)
            {
                size = form.RestoreBounds.Size;
                location = form.RestoreBounds.Location;
            }

            table["w"] = size.Width.ToString();
            table["h"] = size.Height.ToString();
            table["x"] = location.X.ToString();
            table["y"] = location.Y.ToString();
            if (form.WindowState != FormWindowState.Normal)
                table["s"] = Enum.GetName(typeof(FormWindowState),
                    form.WindowState);
            return form.GetType().ToString() + ":" + StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        static void SetToolStripButtonState(ToolStripButton button, string strText)
        {
            string strState = "";
            if (IsType(strText, button, out strState) == false)
                return;
            if (DomUtil.IsBooleanTrue(strState))
                button.Checked = true;
            else
                button.Checked = false;
        }

        static string GetToolStripButtonState(ToolStripButton button)
        {
            return button.GetType().ToString() + ":" + (button.Checked == true ? "yes" : "no");
        }

        static void SetNumericUpDownState(NumericUpDown numeric, string strText)
        {
            string strState = "";
            if (IsType(strText, numeric, out strState) == false)
                return;

            if (string.IsNullOrEmpty(strState) == false)
            {
                decimal v = 0;
                decimal.TryParse(strState, out v);
                numeric.Value = v;
            }
        }

        static string GetNumericUpDownState(NumericUpDown numeric)
        {
            return numeric.GetType().ToString() + ":" + numeric.Value.ToString();
        }

        // 恢复控件的尺寸状态
        // 字符串里面每个分号间隔的部分，负责定义一个控件的状态
        public static void SetUiState(List<object> controls,
            string strStates)
        {
            string[] sections = strStates.Split(new char[] { ';' });

            int i = 0;
            foreach (object obj in controls)
            {
#if NO
                if (i >= sections.Length)
                    break;
#endif
                string strState = "";
                if (i < sections.Length)
                    strState = sections[i];

                object control = obj;

                object default_value = null;
                if (obj is ControlWrapper)
                {
                    ControlWrapper wrapper = obj as ControlWrapper;
                    control = wrapper.Control as object;
                    default_value = wrapper.DefaultValue;
                }

                if (control is ListView)
                {
                    SetListViewState(control as ListView, strState);
                }
                else if (control is SplitContainer)
                {
                    SetSplitContainerState(control as SplitContainer, strState);
                }
                else if (control is ToolStripButton)
                {
                    SetToolStripButtonState(control as ToolStripButton, strState);
                }
                else if (control is NumericUpDown)
                {
                    SetNumericUpDownState(control as NumericUpDown, strState);
                }
                else if (control is TabComboBox)
                {
                    SetTabComboBoxState(control as TabComboBox, strState);
                }
                else if (control is ComboBox)
                {
                    SetComboBoxState(control as ComboBox, strState, default_value);
                }
                else if (control is ComboBoxText)
                {
                    SetComboBoxTextState(control as ComboBoxText, strState, default_value);
                }
                else if (control is TabControl)
                {
                    SetTabControlState(control as TabControl, strState);
                }
                else if (control is CheckBox)
                {
                    SetCheckBoxState(control as CheckBox, strState, default_value);
                }
                else if (control is CheckedListBox)
                {
                    SetCheckedListBoxState(control as CheckedListBox, strState);
                }
                else if (control is TextBox)
                {
                    SetTextBoxState(control as TextBox, strState, default_value);
                }
                else if (control is SavePassword)
                {
                    SetSavePasswordState(control as SavePassword, strState);
                }
                else if (control is DpTable)
                {
                    SetDpTableState(control as DpTable, strState);
                }
                else if (control is RadioButton)
                {
                    SetRadioButtonState(control as RadioButton, strState);
                }
                else if (control is Form)
                {
                    SetFormState(control as Form, strState);
                }
                else if (control is CheckedComboBox)
                {
                    SetCheckedComboBoxState(control as CheckedComboBox, strState, default_value);
                }
                else
                    throw new ArgumentException("不支持的类型 " + control.GetType().ToString());

                i++;
            }
        }

        // 获得表示控件状态的字符串
        public static string GetUiState(List<object> controls)
        {
            StringBuilder text = new StringBuilder();

            foreach (object obj in controls)
            {
                if (text.Length > 0)
                    text.Append(";");

                object control = obj;

                // 注：本来集合中没有必要添加 ControlWrapper 对象，但可能出于和 SetUiState() 对称的角度，会复制同样的代码使用 ControlWrapper，等于缺省值部分这里无用罢了
                //object default_value = null;
                if (obj is ControlWrapper)
                {
                    ControlWrapper wrapper = obj as ControlWrapper;
                    control = wrapper.Control as object;
                    //default_value = wrapper.DefaultValue;
                }


                if (control is ListView)
                {
                    text.Append(
                        GetListViewState(control as ListView)
                        );
                }
                else if (control is SplitContainer)
                {
                    text.Append(
GetSplitContainerState(control as SplitContainer)
    );
                }
                else if (control is ToolStripButton)
                {
                    text.Append(
GetToolStripButtonState(control as ToolStripButton)
    );
                }
                else if (control is NumericUpDown)
                {
                    text.Append(
                    GetNumericUpDownState(control as NumericUpDown)
    );
                }
                else if (control is TabComboBox)
                {
                    text.Append(
                    GetTabComboBoxState(control as TabComboBox)
    );
                }
                else if (control is ComboBox)
                {
                    text.Append(
                    GetComboBoxState(control as ComboBox)
    );
                }
                else if (control is ComboBoxText)
                {
                    text.Append(
                    GetComboBoxTextState(control as ComboBoxText)
    );
                }
                else if (control is TabControl)
                {
                    text.Append(
                    GetTabControlState(control as TabControl)
    );
                }
                else if (control is CheckBox)
                {
                    text.Append(
                    GetCheckBoxState(control as CheckBox)
    );
                }
                else if (control is CheckedListBox)
                {
                    text.Append(
                    GetCheckListBoxState(control as CheckedListBox)
    );
                }
                else if (control is TextBox)
                {
                    text.Append(
GetTextBoxState(control as TextBox)
    );
                }
                else if (control is SavePassword)
                {
                    text.Append(
                    GetSavePasswordState(control as SavePassword)
    );
                }
                else if (control is DpTable)
                {
                    text.Append(
                        GetDpTableState(control as DpTable)
                        );
                }
                else if (control is RadioButton)
                {
                    text.Append(
                    GetRadioButtonState(control as RadioButton)
    );
                }
                else if (control is Form)
                    text.Append(
GetFormState(control as Form)
);
                else if (control is CheckedComboBox)
                {
                    text.Append(
GetCheckedComboBoxState(control as CheckedComboBox)
    );
                }
                else
                    throw new ArgumentException("不支持的类型 " + control.GetType().ToString());
            }

            return text.ToString();
        }
    }

    public class ComboBoxText
    {
        public ComboBox ComboBox = null;

        public ComboBoxText(ComboBox combobox)
        {
            this.ComboBox = combobox;
        }
    }

    public class SavePassword
    {
        public TextBox PasswordTextBox = null;
        public CheckBox SaveOrNotCheckBox = null;

        public SavePassword(TextBox textbox, CheckBox checkbox)
        {
            this.PasswordTextBox = textbox;
            this.SaveOrNotCheckBox = checkbox;
        }
    }

    // 包装 Control 和 缺省值
    public class ControlWrapper
    {
        public Control Control = null;
        public object DefaultValue = null;

        public ControlWrapper(Control control, object default_value)
        {
            this.Control = control;
            this.DefaultValue = default_value;
        }
    }
}
