using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Range;

namespace dp2Circulation
{
    /// <summary>
    /// 打印 HTML 内容的窗口
    /// </summary>
    public partial class HtmlPrintForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        AutoResetEvent eventPrintComplete = new AutoResetEvent(false);	// true : initial state is signaled 

        // 文件名数组
        /// <summary>
        /// 文件名集合。用于设定需要打印的那些 HTML 文件名
        /// </summary>
        public List<string> Filenames = new List<string>();

        int m_nCurrenPageNo = 0;  // 当前显示页

        bool m_bShowDialog = false; // 第一页打印的时候是否出现打印对话框

        /// <summary>
        /// 构造函数
        /// </summary>
        public HtmlPrintForm()
        {
            InitializeComponent();
        }

        private void HtmlPrintForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            // 把第一页装入
            this.LoadPageFile();

            this.EnableButtons();

            DisplayPageInfoLine();

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
        }

        private void HtmlPrintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
        }

        private void button_prevPage_Click(object sender, EventArgs e)
        {
            if (this.Filenames == null)
                return;

            if (this.m_nCurrenPageNo == 0)
                return;

            this.m_nCurrenPageNo--;
            this.LoadPageFile();

            this.EnableButtons();
            this.DisplayPageInfoLine();

        }

        private void button_nextPage_Click(object sender, EventArgs e)
        {
            if (this.Filenames == null)
                return;

            if (this.m_nCurrenPageNo >= this.Filenames.Count - 1)
                return;

            this.m_nCurrenPageNo++;
            this.LoadPageFile();

            this.EnableButtons();
            this.DisplayPageInfoLine();

        }

        private void button_firstPage_Click(object sender, EventArgs e)
        {
            if (this.Filenames == null)
                return;

            if (this.m_nCurrenPageNo == 0)
                return;

            this.m_nCurrenPageNo = 0;
            this.LoadPageFile();

            this.EnableButtons();
            this.DisplayPageInfoLine();
        }

        private void button_lastPage_Click(object sender, EventArgs e)
        {
            if (this.Filenames == null)
                return;

            if (this.m_nCurrenPageNo >= this.Filenames.Count - 1)
                return;

            this.m_nCurrenPageNo = this.Filenames.Count - 1;
            this.LoadPageFile();

            this.EnableButtons();
            this.DisplayPageInfoLine();
        }

        // 打印
        private void button_print_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (Control.ModifierKeys == Keys.Control)
                this.m_bShowDialog = true;  // 强制出现打印对话框
            else
                this.m_bShowDialog = false;

            RangeList rl = null;    // rl==null表示全部打印

            if (String.IsNullOrEmpty(this.textBox_printRange.Text) == false)
            {
                try
                {
                    rl = new RangeList(this.textBox_printRange.Text);
                }
                catch (Exception ex)
                {
                    strError = "打印范围字符串格式错误: " + ex.Message;
                    goto ERROR1;
                }
            }

            int nCopies = 1;

            try
            {
                nCopies = Convert.ToInt32(this.textBox_copies.Text);
            }
            catch
            {
                strError = "份数值格式错误";
                goto ERROR1;
            }

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在打印 ...");
            stop.BeginLoop();
            this.Update();
            this.MainForm.Update();

            int nPrinted = 0;

            try
            {

                this.webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
                this.eventPrintComplete.Reset();

                // Debug.Assert(false, "");

                for (int c = 0; c < nCopies; c++)
                {

                    // 打印表格各页
                    for (int i = 0; i < this.Filenames.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权


                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                        }


                        if (rl == null
                            || rl.IsInRange(i + 1, false) == true)
                        {
                            // MessageBox.Show(this, "once");
                            nPrinted++;

                            stop.SetMessage("正在打印第 " + (i + 1).ToString() + " 页...");

                            this.m_nCurrenPageNo = i;

                            this.LoadPageFile();    // 通过completed事件来驱动打印。

                            while (true)
                            {
                                Application.DoEvents();	// 出让界面控制权
                                if (eventPrintComplete.WaitOne(100, true) == true)
                                    break;
                            }
                        }
                    }
                }

                this.webBrowser1.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("打印完成。共打印 " + nPrinted.ToString() + "页。");

                EnableControls(true);
            }

            if (nPrinted == 0)
            {
                MessageBox.Show(this, "您所指定的打印页码范围 '" 
                    +this.textBox_printRange.Text + "' 没有找到匹配的页。");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Debug.Assert(false, "");
            this.EnableButtons();
            this.DisplayPageInfoLine();

            this.Update();

            if (this.m_bShowDialog == true)
            {
                this.webBrowser1.ShowPrintDialog();
                m_bShowDialog = false;
            }
            else
                this.webBrowser1.Print();

            this.eventPrintComplete.Set();
        }

        void DoStop(object sender, StopEventArgs e)
        {
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.button_print.Enabled = bEnable;
            this.textBox_printRange.Enabled = bEnable;
            this.textBox_copies.Enabled = bEnable;

            if (bEnable == false)
            {
                this.button_nextPage.Enabled = false;
                this.button_prevPage.Enabled = false;
            }
            else
            {
                this.EnableButtons();
            }
        }

        void LoadPageFile()
        {
            if (this.Filenames == null || this.Filenames.Count == 0)
            {
                Global.ClearHtmlPage(this.webBrowser1, this.MainForm.DataDir);
                return;
            }
            this.webBrowser1.Navigate(this.Filenames[this.m_nCurrenPageNo]);
        }

        void DisplayPageInfoLine()
        {
            this.label_pageInfo.Text = (this.m_nCurrenPageNo + 1).ToString() 
                + " / "
                + (this.Filenames == null ? "0" : this.Filenames.Count.ToString());
        }

        void EnableButtons()
        {
            if (this.Filenames == null || this.Filenames.Count == 0)
            {
                this.button_prevPage.Enabled = false;
                this.button_nextPage.Enabled = false;

                this.button_firstPage.Enabled = false;
                this.button_lastPage.Enabled = false;

                this.button_print.Enabled = false;
                return;
            }

            this.button_print.Enabled = true;

            if (this.m_nCurrenPageNo == 0)
            {
                this.button_firstPage.Enabled = false;
                this.button_prevPage.Enabled = false;
            }
            else
            {
                this.button_firstPage.Enabled = true;
                this.button_prevPage.Enabled = true;
            }


            if (this.m_nCurrenPageNo >= this.Filenames.Count - 1)
            {
                this.button_lastPage.Enabled = false;
                this.button_nextPage.Enabled = false;
            }
            else
            {
                this.button_lastPage.Enabled = true;
                this.button_nextPage.Enabled = true;
            }
        }

        // 
        /// <summary>
        /// 将HTML字符串打印出来
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void PrintHtmlString(string strHtml)
        {
            Global.SetHtmlString(this.webBrowser1, strHtml);
            this.webBrowser1.Print();
        }
    }
}