using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CommonControl
{
    public partial class MessageDialog : Form
    {
        // 自动关闭时间。单位毫秒
        // 0 表示不自动关闭
        public int AutoCloseTimeout { get; set; }

        List<ButtonInfo> m_buttonInfos = new List<ButtonInfo>();

        internal string[] button_texts = null;

        public MessageBoxButtons m_buttonDef = MessageBoxButtons.OK;
        public MessageBoxDefaultButton m_defautButton = MessageBoxDefaultButton.Button1;

        Timer _timer = null;

        public MessageDialog()
        {
            InitializeComponent();
        }

        private void MessageDialog_Load(object sender, EventArgs e)
        {
            SetButtonState(this.button_texts);

            this.textBox_message.SelectionStart = this.textBox_message.Text.Length;
            this.textBox_message.DeselectAll();

            if (this.AutoCloseTimeout > 0)
            {
                _timer = new Timer();
                _timer.Interval = this.AutoCloseTimeout;
                _timer.Tick += _timer_Tick;
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            button_1_Click(sender, e);
        }

        void SetButtonState(string[] button_texts = null)
        {
            {
                ButtonInfo info = null;

                this.m_buttonInfos = new List<ButtonInfo>();
                if (this.m_buttonDef == MessageBoxButtons.OK)
                {
                    // 按钮1不使用
                    info = new ButtonInfo();
                    info.Visible = false;
                    this.m_buttonInfos.Add(info);

                    // 按钮2不使用
                    info = new ButtonInfo();
                    info.Visible = false;
                    this.m_buttonInfos.Add(info);

                    // 按钮3
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.OK;
                    info.Text = "确定";
                    info.Visible = true;
                    info.Style = "accept";
                    this.m_buttonInfos.Add(info);
                }
                else if (this.m_buttonDef == MessageBoxButtons.OKCancel)
                {
                    // 按钮1不使用
                    info = new ButtonInfo();
                    info.Visible = false;
                    this.m_buttonInfos.Add(info);


                    // 按钮2
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.OK;
                    info.Text = "确定";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button1)
                        info.Style = "accept";
                    this.m_buttonInfos.Add(info);

                    // 按钮3
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    info.Text = "取消";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button2)
                        info.Style = "accept";
                    StringUtil.SetInList(ref info.Style, "cancel", true);
                    this.m_buttonInfos.Add(info);
                }
                else if (this.m_buttonDef == MessageBoxButtons.YesNo)
                {
                    // 按钮1不使用
                    info = new ButtonInfo();
                    info.Visible = false;
                    this.m_buttonInfos.Add(info);


                    // 按钮2
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.Yes;
                    info.Text = "是";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button1)
                        info.Style = "accept";
                    this.m_buttonInfos.Add(info);

                    // 按钮3
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.No;
                    info.Text = "否";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button2)
                        info.Style = "accept";
                    StringUtil.SetInList(ref info.Style, "cancel", true);
                    this.m_buttonInfos.Add(info);
                }
                else if (this.m_buttonDef == MessageBoxButtons.YesNoCancel)
                {
                    // 按钮1
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.Yes;
                    info.Text = "是";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button1)
                        info.Style = "accept";
                    this.m_buttonInfos.Add(info);

                    // 按钮2
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.No;
                    info.Text = "否";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button2)
                        info.Style = "accept";
                    StringUtil.SetInList(ref info.Style, "cancel", true);
                    this.m_buttonInfos.Add(info);

                    // 按钮3
                    info = new ButtonInfo();
                    info.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    info.Text = "取消";
                    info.Visible = true;
                    if (this.m_defautButton == MessageBoxDefaultButton.Button3)
                        info.Style = "accept";
                    this.m_buttonInfos.Add(info);
                }
                else
                {
                    throw new Exception("不支持的 " + m_buttonDef.ToString());
                }

                // 兑现到按钮上
                if (this.m_buttonInfos.Count > 0)
                {
                    info = this.m_buttonInfos[0];
                    this.button_1.Visible = info.Visible;
                    if (info.Visible == true)
                    {
                        this.button_1.Text = info.Text;
                        if (StringUtil.IsInList("accept", info.Style) == true)
                            this.AcceptButton = this.button_1;
                        if (StringUtil.IsInList("cancel", info.Style) == true)
                            this.CancelButton = this.button_1;
                    }

                }

                if (this.m_buttonInfos.Count > 1)
                {
                    info = this.m_buttonInfos[1];
                    this.button_2.Visible = info.Visible;
                    if (info.Visible == true)
                    {
                        this.button_2.Text = info.Text;
                        if (StringUtil.IsInList("accept", info.Style) == true)
                            this.AcceptButton = this.button_2;
                        if (StringUtil.IsInList("cancel", info.Style) == true)
                            this.CancelButton = this.button_2;
                    }
                }

                if (this.m_buttonInfos.Count > 2)
                {
                    info = this.m_buttonInfos[2];
                    this.button_3.Visible = info.Visible;
                    if (info.Visible == true)
                    {
                        this.button_3.Text = info.Text;
                        if (StringUtil.IsInList("accept", info.Style) == true)
                            this.AcceptButton = this.button_3;
                        if (StringUtil.IsInList("cancel", info.Style) == true)
                            this.CancelButton = this.button_3;
                    }
                }

            }


            if (button_texts != null)
            {
                int i = 0;
                List<string> texts = new List<string>();
                foreach (ButtonInfo info in this.m_buttonInfos)
                {
                    if (i >= button_texts.Length)
                        break;
                    if (info.Visible == false)
                    {
                        texts.Add(null);
                        continue;
                    }
                    texts.Add(button_texts[i]);
                    i++;
                }

                if (texts.Count > 0)
                {
                    string strText = texts[0];

                    if (strText != null)
                        this.button_1.Text = strText;
                }

                if (texts.Count > 1)
                {
                    string strText = texts[1];

                    if (strText != null)
                        this.button_2.Text = strText;
                }

                if (texts.Count > 2)
                {
                    string strText = texts[2];

                    if (strText != null)
                        this.button_3.Text = strText;
                }
            }
#if NO
            if (button_texts != null)
            {
                if (button_texts.Length > 0)
                {
                    string strText = button_texts[0];

                    if (strText != null)
                        this.button_1.Text = strText;
                }

                if (button_texts.Length > 1)
                {
                    string strText = button_texts[1];

                    if (strText != null)
                        this.button_2.Text = strText;
                }

                if (button_texts.Length > 2)
                {
                    string strText = button_texts[2];

                    if (strText != null)
                        this.button_3.Text = strText;
                }
            }
#endif

#if NO
            // //
            if (this.m_buttonInfos.Count > 0)
            {
                info = this.m_buttonInfos[0];
                if (info.Visible == true)
                {
                    if (StringUtil.IsInList("accept", info.Style) == true)
                        this.AcceptButton = this.button_1;
                    if (StringUtil.IsInList("cancel", info.Style) == true)
                        this.CancelButton = this.button_1;
                }

            }

            if (this.m_buttonInfos.Count > 1)
            {
                info = this.m_buttonInfos[1];
                if (info.Visible == true)
                {
                    if (StringUtil.IsInList("accept", info.Style) == true)
                        this.AcceptButton = this.button_2;
                    if (StringUtil.IsInList("cancel", info.Style) == true)
                        this.CancelButton = this.button_2;
                }
            }

            if (this.m_buttonInfos.Count > 2)
            {
                info = this.m_buttonInfos[2];
                if (info.Visible == true)
                {
                    if (StringUtil.IsInList("accept", info.Style) == true)
                        this.AcceptButton = this.button_3;
                    if (StringUtil.IsInList("cancel", info.Style) == true)
                        this.CancelButton = this.button_3;
                }
            }
#endif
        }

        private void button_1_Click(object sender, EventArgs e)
        {
            if (this.m_buttonInfos.Count <= 0)
                return;

            ButtonInfo info = this.m_buttonInfos[0];

            this.DialogResult = info.DialogResult;
            this.Close();
        }

        private void button_2_Click(object sender, EventArgs e)
        {
            if (this.m_buttonInfos.Count <= 1)
                return;

            ButtonInfo info = this.m_buttonInfos[1];

            this.DialogResult = info.DialogResult;
            this.Close();
        }

        private void button_3_Click(object sender, EventArgs e)
        {
            if (this.m_buttonInfos.Count <= 2)
                return;

            ButtonInfo info = this.m_buttonInfos[2];

            this.DialogResult = info.DialogResult;
            this.Close();
        }

        public string CheckBoxText
        {
            get
            {
                return this.checkBox_message1.Text;
            }
            set
            {
                this.checkBox_message1.Text = value;
            }
        }

        public string MessageText
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        public bool CheckBoxValue
        {
            get
            {
                return this.checkBox_message1.Checked;
            }
            set
            {
                this.checkBox_message1.Checked = value;
            }
        }

        public bool CheckBoxVisible
        {
            get
            {
                return this.checkBox_message1.Visible;
            }
            set
            {
                this.checkBox_message1.Visible = value;
            }
        }

        public static DialogResult Show(IWin32Window owner,
            string strText,
            string strCheckBoxText,
            ref bool bCheckBox)
        {
            return Show(owner, "", strText,
                strCheckBoxText, ref bCheckBox);
        }

        public static DialogResult Show(IWin32Window owner,
            string strTitle,
            string strText,
            string strCheckBoxText,
            ref bool bCheckBox)
        {
            MessageDialog dlg = new MessageDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.Text = strTitle;
            if (strCheckBoxText == null)
                dlg.CheckBoxVisible = false;
            else
                dlg.CheckBoxText = strCheckBoxText;
            dlg.MessageText = strText;
            dlg.CheckBoxValue = bCheckBox;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(owner);

            bCheckBox = dlg.CheckBoxValue;
            return dlg.DialogResult;
        }

        public static DialogResult Show(IWin32Window owner,
            string strText,
            MessageBoxButtons buttons,
            MessageBoxDefaultButton defaultbutton,
            string strCheckBoxText,
            ref bool bCheckBox,
            string[] button_texts = null,
            int nAutoCloseTimeout = 0)
        {
            MessageDialog dlg = new MessageDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.Text = "";
            if (strCheckBoxText == null)
                dlg.CheckBoxVisible = false;
            else
                dlg.CheckBoxText = strCheckBoxText;
            dlg.MessageText = strText;
            dlg.CheckBoxValue = bCheckBox;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.m_buttonDef = buttons;
            dlg.m_defautButton = defaultbutton;

            dlg.button_texts = button_texts;

            dlg.ShowDialog(owner);

            bCheckBox = dlg.CheckBoxValue;
            return dlg.DialogResult;
        }

        private void MessageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }

    public class ButtonInfo
    {
        public string Text = "";
        public DialogResult DialogResult = DialogResult.OK;
        public bool Visible = true;
        public string Style = "";   // accept/cancel
    }
}
