using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// 长操作中显示信息的小窗口
    /// </summary>
    public partial class MessageBar : Form
    {
        public MessageBar()
        {
            InitializeComponent();
        }

        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        public string Title
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }

        public void SetTitle(string text)
        {
            if (this.Visible == false || this.InvokeRequired == false)
                this.Text = text;
            else
                this.Invoke((Action)(() =>
            {
                this.Text = text;
            }));
        }

        public void SetMessageText(string text)
        {
            if (this.Visible == false || this.InvokeRequired == false)
                this.label_message.Text = text;
            else
                this.Invoke((Action)(() =>
                {
                    this.label_message.Text = text;
                }));
        }

        public static MessageBar Create(IWin32Window owner,
            string title,
            string message = null)
        {
            MessageBar bar = new MessageBar
            {
                Title = title
            };
            if (message != null)
                bar.SetMessageText(message);
            bar.StartPosition = FormStartPosition.CenterScreen;
            bar.Show(owner);
            return bar;
        }
    }
}