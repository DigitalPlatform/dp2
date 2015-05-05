using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    // 出纳操作显示操作是否成功的信息对话框
    // 有红黄绿三种状态
    internal partial class ChargingInfoDlg : Form
    {
        public ChargingInfoHost Host = null;

        const int WM_SWITCH_FOCUS = API.WM_USER + 200;

        public double DelayOpacity = 1.0;

        public bool Password
        {
            get
            {
                return this.textBox_fastInputText.PasswordChar == '*';
            }
            set
            {
                if (value == true)
                    this.textBox_fastInputText.PasswordChar = '*';
                else
                    this.textBox_fastInputText.PasswordChar = (char)0;
            }
        }

        public string FastInputText
        {
            get
            {
                return this.textBox_fastInputText.Text;
            }
            set
            {
                this.textBox_fastInputText.Text = value;
            }
        }

        public ChargingInfoDlg()
        {
            InitializeComponent();
        }

        public InfoColor InfoColor
        {
            get
            {
                if (this.label_colorBar.BackColor == Color.Red)
                    return InfoColor.Red;
                if (this.label_colorBar.BackColor == Color.LightCoral)
                    return InfoColor.LightRed;
                if (this.label_colorBar.BackColor == Color.Yellow)
                    return InfoColor.Yellow;
                if (this.label_colorBar.BackColor == Color.Green)
                    return InfoColor.Green;

                return InfoColor.Green;
            }
            set
            {
                if (value == InfoColor.Red)
                    this.label_colorBar.BackColor = Color.Red;
                else if (value == InfoColor.LightRed)
                    this.label_colorBar.BackColor = Color.LightCoral;
                else if (value == InfoColor.Yellow)
                    this.label_colorBar.BackColor = Color.Yellow;
                else if (value == InfoColor.Green)
                    this.label_colorBar.BackColor = Color.Green;
                else
                    this.label_colorBar.BackColor = Color.Green;
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

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 如果输入了快捷文字，停止位于被夺去的控制权里面的无休止循环
            this.textBox_fastInputText.Enabled = false; // 避免重复输入

            if (this.Host != null)
            {
                this.Host.OnStopGettingSummary(this, e);
            }
            else
            {
                // Debug.Assert(false, "没有调用中断获取书目摘要的功能，会导致对话框关闭的延迟");
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        // 省略参数的版本
        // 缺省的颜色为green
        static string Show(IWin32Window owner,
            string strText)
        {
            return Show(owner, strText, InfoColor.Green, null,
                1.0);
        }

        // 省略参数的版本
        // 缺省的颜色为green
        static string Show(IWin32Window owner,
            string strText,
            double delayOpacity)
        {
            return Show(owner, strText, InfoColor.Green, null,
                delayOpacity);
        }

        static string Show(IWin32Window owner,
            string strText,
            InfoColor infocolor)
        {
            return Show(owner, strText, infocolor, null, 1.0);
        }

        static string Show(IWin32Window owner,
            string strText,
            InfoColor infocolor,
            double delayOpacity)
        {
            return Show(owner, strText, infocolor, null, delayOpacity);
        }

        // 原始版本
        static string Show(IWin32Window owner,
            string strText,
            InfoColor infocolor,
            string strCaption,
            double delayOpacity,
            Font font = null)
        {
            ChargingInfoDlg dlg = new ChargingInfoDlg();
            if (font != null)
                MainForm.SetControlFont(dlg, font, false);

            dlg.DelayOpacity = delayOpacity;
            dlg.InfoColor = infocolor;
            dlg.MessageText = strText;
            if (strCaption != null)
                dlg.Text = strCaption;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(owner);

            return dlg.FastInputText;
        }

        public static string Show(ChargingInfoHost host,
    string strText,
    InfoColor infocolor,
    string strCaption,
    double delayOpacity,
            Font font = null)
        {
            ChargingInfoDlg dlg = new ChargingInfoDlg();
            if (font != null)
                MainForm.SetControlFont(dlg, font, false);

            dlg.Host = host;
            dlg.DelayOpacity = delayOpacity;
            dlg.InfoColor = infocolor;
            dlg.MessageText = strText;
            if (strCaption != null)
                dlg.Text = strCaption;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            host.ap.LinkFormState(dlg, "ChargingInfoDlg_state");
            dlg.ShowDialog(host.window);
            host.ap.UnlinkFormState(dlg);

            return dlg.FastInputText;
        }

        // 2009/6/2 new add
        public static string Show(ChargingInfoHost host,
            string strText,
            InfoColor infocolor,
            string strCaption,
            double delayOpacity,
            bool bPassword,
            Font font = null)
        {
            ChargingInfoDlg dlg = new ChargingInfoDlg();
            if (font != null)
                MainForm.SetControlFont(dlg, font, false);

            dlg.Host = host;
            dlg.DelayOpacity = delayOpacity;
            dlg.InfoColor = infocolor;
            dlg.MessageText = strText;
            if (strCaption != null)
                dlg.Text = strCaption;
            dlg.Password = bPassword;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            host.ap.LinkFormState(dlg, "ChargingInfoDlg_state");
            dlg.ShowDialog(host.window);
            host.ap.UnlinkFormState(dlg);

            return dlg.FastInputText;
        }

        private void ChargingInfoDlg_Load(object sender, EventArgs e)
        {
            this.textBox_message.Select(0, 0);

            API.PostMessage(this.Handle, WM_SWITCH_FOCUS,
                0, 0);

            // 准备变透明
            this.timer_transparent.Start();
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SWITCH_FOCUS:
                    {
                        this.textBox_fastInputText.SelectAll();
                        this.textBox_fastInputText.Focus();

                        return;
                    }
                // break;
            }
            base.DefWndProc(ref m);
        }

        private void timer_transparent_Tick(object sender, EventArgs e)
        {
            this.timer_transparent.Stop();
            this.Opacity = this.DelayOpacity;
        }

        private void label_colorBar_MouseDown(object sender, MouseEventArgs e)
        {
            this.Opacity = 1.0;

        }

        private void label_colorBar_MouseUp(object sender, MouseEventArgs e)
        {
            this.timer_transparent.Start();
        }

        // 恢复不透明
        private void textBox_fastInputText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.Opacity != 1.0)
                this.Opacity = 1.0;

        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            /*
            if (keyData == Keys.Enter)
            {
                this.button_OK_Click(this, null);
                return true;
            }*/

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }


            // return false;
            return base.ProcessDialogKey(keyData);
        }
    }

    /// <summary>
    /// (快速操作对话框)信息颜色
    /// </summary>
    public enum InfoColor
    {
        /// <summary>
        /// 红色。操作失败；或者禁止
        /// </summary>
        Red = 0,    // 操作失败；或者禁止

        /// <summary>
        /// 钱红色。操作可能失败，也可能成功
        /// </summary>
        LightRed = 1,   // 操作可能失败，也可能成功

        /// <summary>
        /// 黄色。操作成功，但是有后续操作需要留意
        /// </summary>
        Yellow = 2, // 操作成功，但是有后续操作需要留意

        /// <summary>
        /// 绿色。操作成功，没有后续操作
        /// </summary>
        Green = 3,  // 操作成功，没有后续操作
    }
}