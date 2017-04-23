using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Web;   // HttpUtility

using DigitalPlatform;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.dp2.Statis;

//using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    internal partial class BookshelfForm : Form
    {
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        public BookshelfForm()
        {
            InitializeComponent();
        }

        private void BookshelfForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(Program.MainForm.stopManager, true);	// 和容器关联
        }


        private void BookshelfForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }

        }

        private void BookshelfForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
        }

        private void button_create_Click(object sender, EventArgs e)
        {

        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            Program.MainForm.Channel_BeforeLogin(sender, e);    // 2015/11/8
        }

        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            Program.MainForm.Channel_AfterLogin(sender, e);    // 2015/11/8
        }


        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        private void BookshelfForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13 
            Program.MainForm.stopManager.Active(this.stop);

        }
    }
}