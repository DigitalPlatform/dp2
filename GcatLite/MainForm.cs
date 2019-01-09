
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace GcatLite
{
    /// <summary>
    /// 通用汉语著者号码表前端
    /// </summary>
    public partial class MainForm : Form
    {
        ClipboardMonitor clipboardMonitor = new ClipboardMonitor();

        RestChannel _channel = new RestChannel("");

#if NO
        string UserName = "";
        string Password = "";
        bool SavePassword = false;
#endif

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.clipboardMonitor.ClipboardChanged += new ClipboardChangedEventHandle(clipboardMonitor_ClipboardChanged);

            this.textBox_url.Text = Properties.Settings.Default.ServerUrl;
            this.textBox_author.Text = Properties.Settings.Default.Author;
            this.textBox_number.Text = Properties.Settings.Default.Number;

            this.checkBox_copyResultToClipboard.Checked = Properties.Settings.Default.CopyResultToClipboard;
            this.checkBox_outputDebugInfo.Checked = Properties.Settings.Default.OutputDebugInfo;
            this.checkBox_selectEntry.Checked = Properties.Settings.Default.SelectEntry;
            this.checkBox_selectPinyin.Checked = Properties.Settings.Default.SelectPinyin;

            // 该checkbox的事件会自动控制this.clipboardMonitor的Chain属性.
            this.checkBox_clipboardChain.Checked = Properties.Settings.Default.ClipboardChain;

#if NO
            this.UserName = Properties.Settings.Default.UserName;
            this.SavePassword = Properties.Settings.Default.SavePassword;
            if (this.SavePassword == true)
                this.Password = Properties.Settings.Default.Password;
#endif
        }

        void clipboardMonitor_ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {

            IDataObject iData = Clipboard.GetDataObject();

            // Determines whether the data is in a format you can use.
            if (iData.GetDataPresent(DataFormats.Text))
            {
                // Yes it is, so display it in a text box.
                string strText = (String)iData.GetData(DataFormats.Text);

                // MessageBox.Show(this, strText);
                if (strText.Length > 0)
                {
                    this.textBox_author.Text = strText;
                    button_get_Click(null, EventArgs.Empty);
                }
            }
            else
            {
                // 格式无法支持, 忽略
                // MessageBox.Show(this, "Could not retrieve data off the clipboard.");
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _channel.Dispose();

            this.clipboardMonitor.ClipboardChanged -= new ClipboardChangedEventHandle(clipboardMonitor_ClipboardChanged);

            // 保存面板参数
            Properties.Settings.Default.ServerUrl = this.textBox_url.Text;
            Properties.Settings.Default.Author = this.textBox_author.Text;
            Properties.Settings.Default.Number = this.textBox_number.Text;

            Properties.Settings.Default.CopyResultToClipboard = this.checkBox_copyResultToClipboard.Checked;
            Properties.Settings.Default.OutputDebugInfo = this.checkBox_outputDebugInfo.Checked;
            Properties.Settings.Default.SelectEntry = this.checkBox_selectEntry.Checked;
            Properties.Settings.Default.SelectPinyin = this.checkBox_selectPinyin.Checked;
            Properties.Settings.Default.ClipboardChain = this.checkBox_clipboardChain.Checked;

#if NO
            Properties.Settings.Default.UserName = this.UserName;
            Properties.Settings.Default.SavePassword = this.SavePassword;
            if (this.SavePassword == true)
                Properties.Settings.Default.Password = this.Password;
            else
                Properties.Settings.Default.Password = "";
#endif

            Properties.Settings.Default.Save();
        }

        private void button_get_Click(object sender, EventArgs e)
        {
            GetNumber();
        }

        private void GetNumber()
        {
            string strError = "";

            _channel.ServerUrl = this.textBox_url.Text;

#if DYNAMIC
            dynamic questions = null;
#else
            List<Question> questions = new List<Question>();
#endif

            EnableControls(false);
            try
            {
                this.textBox_number.Text = "";
                this.textBox_debugInfo.Text = "";

                REDO:
                // result.Value:
                //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
                //		-3	需要回答问题
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                var result = _channel.GetAuthorNumber(
    this.textBox_author.Text,
    this.checkBox_selectPinyin.Checked,
    this.checkBox_selectEntry.Checked,
    this.checkBox_outputDebugInfo.Checked,
    ref questions,
    out string number,
    out string debugInfo);
                if (result.Value == -1)
                {
                    if (result.ErrorCode ==
#if DYNAMIC
                        6
#else
                        ErrorCode.NotLogin
#endif
                        )
                    {
                        // 调用 登录 API
                        // 目前用 public 登录即可
                        var login_result = _channel.Login("public",
"",
"type=worker,client=GcatLite|1.0");
                        if (login_result.Value != 1)
                        {
                            strError = login_result.ErrorInfo;
                            goto ERROR1;
                        }
                        goto REDO;
                    }

                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                // 需要前端回答问题
                if (result.Value == -3)
                {
                    // 把窗口翻动到前面
                    if (this.checkBox_clipboardChain.Checked)
                        TryBringToFront();

                    string strTitle = strError;

#if !DYNAMIC
                    Debug.Assert(questions.Count > 0, "");
#endif

                    string strQuestion = questions[questions.Count - 1].Text;

                    QuestionDlg dlg = new QuestionDlg();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.label_messageTitle.Text = strTitle;
                    dlg.textBox_question.Text = strQuestion.Replace("\n", "\r\n");
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                    {
                        strError = "放弃";
                        goto ERROR1;
                    }

                    // 为最后一个问题设置 Answer，然后重试取号 API
                    questions[questions.Count - 1].Answer = dlg.textBox_result.Text;
                    goto REDO;
                }

                this.textBox_number.Text = number;
                this.textBox_debugInfo.Text = debugInfo.Replace("\n", "\r\n");

                // 自动复制到剪贴板
                if (this.checkBox_copyResultToClipboard.Checked == true)
                {
                    this.clipboardMonitor.Ignore = true;
                    Clipboard.SetDataObject(number);
                    this.clipboardMonitor.Ignore = false;
                }

                if (result.Value == -1)
                {
                    this.textBox_author.SelectAll();
                    this.textBox_author.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
            finally
            {
                EnableControls(true);
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void EnableControls(bool bEnabled)
        {
            this.button_get.Enabled = bEnabled;

            this.textBox_url.Enabled = bEnabled;
            this.textBox_author.Enabled = bEnabled;
            this.textBox_number.Enabled = bEnabled;
            this.textBox_debugInfo.Enabled = bEnabled;

            this.checkBox_selectPinyin.Enabled = bEnabled;
            this.checkBox_outputDebugInfo.Enabled = bEnabled;
            this.checkBox_selectEntry.Enabled = bEnabled;
            this.checkBox_copyResultToClipboard.Enabled = bEnabled;
            this.checkBox_clipboardChain.Enabled = bEnabled;

            if (bEnabled == false)
                toolStripStatusLabel_main.Text = "正在处理，请稍候 ...";
            else
                toolStripStatusLabel_main.Text = "";

            this.Update();
        }

        public void TryBringToFront()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    TryBringToFront();
                }));
            }
            else
            {
                // https://stackoverflow.com/questions/5282588/how-can-i-bring-my-application-window-to-the-front
                this.WindowState = FormWindowState.Minimized;
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        // 选择状态变化为on时，也要复制到剪贴板一次
        private void checkBox_copyResultToClipboard_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_copyResultToClipboard.Checked == true)
                Clipboard.SetDataObject(this.textBox_number.Text);
        }

        private void textBox_author_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_get;
        }

        private void textBox_author_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        private void checkBox_clipboardChain_CheckedChanged(object sender, EventArgs e)
        {
            this.clipboardMonitor.Chain = this.checkBox_clipboardChain.Checked;
        }

        private void toolStripButton_copyright_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("iexplore",
            //    "http://dp2003.com/dp2bbs/article.aspx?board=%e4%ba%a7%e5%93%81%e4%b8%8e%e6%9c%8d%e5%8a%a1&id=59");
            System.Diagnostics.Process.Start(// "iexplore",
    "https://github.com/DigitalPlatform/dp2/tree/master/GcatLite");
        }

        private void toolStripButton_copyright_Click_1(object sender, EventArgs e)
        {
            CopyrightDlg dlg = new CopyrightDlg();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        private void button_defaultUrl_Click(object sender, EventArgs e)
        {
            this.textBox_url.Text = "http://dp2003.com/dp2library/rest";
        }
    }
}