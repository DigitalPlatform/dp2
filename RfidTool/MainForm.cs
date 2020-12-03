using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidTool
{
    public partial class MainForm : Form
    {
        ScanDialog _scanDialog = new ScanDialog();

        #region floating message
        internal FloatingMessageForm _floatingMessage = null;

        public FloatingMessageForm FloatingMessageForm
        {
            get
            {
                return this._floatingMessage;
            }
            set
            {
                this._floatingMessage = value;
            }
        }

        public void ShowMessageAutoClear(string strMessage,
string strColor = "",
int delay = 2000,
bool bClickClose = false)
        {
            _ = Task.Run(() =>
            {
                ShowMessage(strMessage,
    strColor,
    bClickClose);
                System.Threading.Thread.Sleep(delay);
                // 中间一直没有变化才去消除它
                if (_floatingMessage.Text == strMessage)
                    ClearMessage();
            });
        }

        public void ShowMessage(string strMessage,
    string strColor = "",
    bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        // 线程安全
        public void ClearMessage()
        {
            if (this._floatingMessage == null)
                return;

            this._floatingMessage.Text = "";
        }

        #endregion

        public MainForm()
        {
            InitializeComponent();

            this.MenuItem_exit.Click += MenuItem_exit_Click;
            this.MenuItem_writeBookTags.Click += MenuItem_writeBookTags_Click;

            _scanDialog.FormClosing += _scanDialog_FormClosing;

            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

                this.Move += (s1, o1) =>
                {
                    if (this._floatingMessage != null)
                        this._floatingMessage.OnResizeOrMove();
                };
            }
        }

        private void _scanDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        // 开始(扫描并)写入图书标签
        private void MenuItem_writeBookTags_Click(object sender, System.EventArgs e)
        {
            // 把扫描对话框打开
            if (_scanDialog.Visible == false)
                _scanDialog.Show(this);

        }

        // 退出
        private void MenuItem_exit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var ret = ClientInfo.Initial("TestShelfLock");
            if (ret == false)
            {
                Application.Exit();
                return;
            }

            this.ShowMessage("正在连接 RFID 读卡器");
            _ = Task.Run(() =>
            {
                DataModel.InitialDriver();
                this.ClearMessage();
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DataModel.ReleaseDriver();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientInfo.Finish();
        }
    }
}
