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
    }
}