using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using DigitalPlatform.IO;
using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 时钟窗
    /// </summary>
    public partial class ClockForm : MyForm
    {
#if NO
        int m_nIn = 0;  // 正在和服务器打交道的层数

        const int WM_PREPARE = API.WM_USER + 200;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public ClockForm()
        {
            this.UseLooping = true; // 2022/11/2

            InitializeComponent();
        }

        private void ClockForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            this.dateTimePicker1.Value = DateTime.Now;

#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // API.PostMessage(this.Handle, WM_PREPARE, 0, 0);
            this.BeginInvoke(new Action(Initial));

            this.timer1.Start();
        }

#if NO
        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            Program.MainForm.Channel_AfterLogin(sender, e);    // 2015/11/8
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            Program.MainForm.Channel_BeforeLogin(sender, e);    // 2015/11/8
        }
#endif

        private void ClockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif

            this.timer1.Stop();
        }

        private void ClockForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
        }

        private void button_set_Click(object sender, EventArgs e)
        {
            string strError = "";

            DialogResult result = MessageBox.Show(this,
"确实要把服务器时钟设置为 '" + this.TimeStringForDisplay + "' ?\r\n\r\n警告：如果服务器时间设置得不正确，会对很多流通操作产生不利影响",
"ClockForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            /*
            LibraryChannel channel = this.GetChannel();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在设置服务器当前时钟为 "+this.RFC1123TimeString+" ...");
            _stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在设置服务器当前时钟为 " + this.RFC1123TimeString + " ...",
                "disableControl");

            // int value = Interlocked.Increment(ref this.m_nIn);

            try
            {
#if NO
                if (value > 1)
                {
                    strError = "通道正在被另一操作使用，当前操作被放弃";
                    goto ERROR1;   // 防止重入
                }
#endif

                long lRet = channel.SetClock(
                    looping.stop,
                    this.RFC1123TimeString,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                // Interlocked.Decrement(ref this.m_nIn);

                looping.Dispose();
                /*
                this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                this.ReturnChannel(channel);
                */
            }

            // MessageBox.Show(this, "时钟设置成功");
            this.ShowMessage("时钟设置成功", "green", true);
            return;
        ERROR1:
            //MessageBox.Show(this, strError);
            //return;
            this.ShowMessage(strError, "red", true);
        }

        int GetServerTime(bool bChangeEnableState,
            out string strError)
        {
            strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得服务器当前时钟 ...");
            _stop.BeginLoop();

            if (bChangeEnableState == true)
                this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得服务器当前时钟 ...",
                bChangeEnableState == true ? "disableControl" : "");

            // int value = Interlocked.Increment(ref this.m_nIn);
            try
            {
#if NO
                if (value > 1)
                    return 0;   // 防止重入
#endif

                string strTime = "";
                long lRet = channel.GetClock(
                    looping.stop,
                    out strTime,
                    out strError);
                if (lRet == -1)
                    return -1;

                // 已经采用带有时区的 RFC1123 字符串显示
                this.RFC1123TimeString = strTime;
                return 0;
            }
            finally
            {
                // Interlocked.Decrement(ref this.m_nIn);

                looping.Dispose();
                /*
                if (bChangeEnableState == true)
                    this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                this.ReturnChannel(channel);
                */
            }
        }

        private void button_get_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = GetServerTime(true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            // MessageBox.Show(this, strError);
            // return;
            this.ShowMessage(strError, "red", true);
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            string strError = "";

            DialogResult result = MessageBox.Show(this,
"确实要把服务器时钟复原为和服务器硬件时钟一致?\r\n\r\n警告：如果服务器时钟设置得不正确，会对很多流通操作产生不利影响",
"ClockForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            /*
            LibraryChannel channel = this.GetChannel();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在将服务器时钟复原为硬件时钟 ...");
            _stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在将服务器时钟复原为硬件时钟 ...",
                "disableControl");

            // int value = Interlocked.Increment(ref this.m_nIn);

            try
            {
#if NO
                if (value > 1)
                {
                    strError = "通道正在被另一操作使用，当前操作被放弃";
                    goto ERROR1;   // 防止重入
                }
#endif

                long lRet = channel.SetClock(
                    looping.stop,
                    null,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                // Interlocked.Decrement(ref this.m_nIn);

                looping.Dispose();
                /*
                this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                this.ReturnChannel(channel);
                */
            }

            MessageBox.Show(this, "服务器时钟复原成功");

            // TODO: 复原后，重新获得一下，以便让操作者看到效果
            button_get_Click(sender, e);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 注意，是GMT时间
        /// <summary>
        /// 表示当前时间的 RFC1123 事件字符串
        /// </summary>
        public string RFC1123TimeString
        {
            get
            {
                if (String.IsNullOrEmpty(this.textBox_time.Text) == true)
                {
                    DateTime time = this.dateTimePicker1.Value; // .ToUniversalTime();

                    return DateTimeUtil.Rfc1123DateTimeStringEx(time);
                }
                return this.textBox_time.Text;
            }
            set
            {
                DateTime time;
                try
                {
                    time = DateTimeUtil.FromRfc1123DateTimeString(value);
                }
                catch
                {
                    MessageBox.Show(this, "时间字符串 " +value+ "格式不合法" );
                    return;
                }

                this.textBox_time.Text = value;
                this.dateTimePicker1.Value = time.ToLocalTime();
            }
        }

        string TimeStringForDisplay
        {
            get
            {
                return DateTimeUtil.LocalTime(this.RFC1123TimeString);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime time = this.dateTimePicker1.Value; // .ToUniversalTime();

            this.textBox_time.Text = DateTimeUtil.Rfc1123DateTimeStringEx(time);
        }


#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        private void ClockForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        private void checkBox_autoGet_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_autoGetServerTime.Checked == true)
            {
                // 在随时都刷新的情况下，修改时间字符串的操作变得困难，只好禁止
                this.dateTimePicker1.Enabled = false;
                this.button_set.Enabled = false;
                this.button_reset.Enabled = false;

                // 改变为true状态后，立即获得一次
                string strError = "";

                GetServerTime(false,
                    out strError);
            }
            else
            {
                this.dateTimePicker1.Enabled = true;
                this.button_set.Enabled = true;
                this.button_reset.Enabled = true;
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                if (this.checkBox_autoGetServerTime.Checked == false)
                    this.dateTimePicker1.Enabled = bEnable;
                else
                    this.dateTimePicker1.Enabled = false;

                this.button_get.Enabled = bEnable;

                if (this.checkBox_autoGetServerTime.Checked == false)
                {
                    this.button_set.Enabled = bEnable;
                    this.button_reset.Enabled = bEnable;
                }
                else
                {
                    this.button_set.Enabled = false;
                    this.button_reset.Enabled = false;
                }

                this.textBox_time.Enabled = bEnable;
            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string strError = "";

            // 刷新服务器时间显示
            if (this.checkBox_autoGetServerTime.Checked == true)
            {
                GetServerTime(false,
                    out strError);
            }

            // 刷新本地时间显示
            DateTime now = DateTime.Now;
            this.textBox_localTime.Text = now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        void Initial()
        {
            string strError = "";

            // 窗口打开后，第一次获得服务器时间显示
            GetServerTime(true, // changed
                out strError);

            // 第一次刷新本地时间显示
            DateTime now = DateTime.Now;
            this.textBox_localTime.Text = now.ToString("yyyy-MM-dd HH:mm:ss");
        }
#if NO
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PREPARE:
                    {
                        string strError = "";

                        // 窗口打开后，第一次获得服务器时间显示
                        GetServerTime(true, // changed
                            out strError);

                        // 第一次刷新本地时间显示
                        DateTime now = DateTime.Now;
                        this.textBox_localTime.Text = now.ToString("yyyy-MM-dd HH:mm:ss");

                        return;
                    }
                // break;

            }
            base.DefWndProc(ref m);
        }
#endif
    }
}