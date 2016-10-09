using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DigitalPlatform.DataMining
{
    public partial class BrowserDialog : Form
    {
        public BrowserDialog()
        {
            InitializeComponent();
        }

        private void BrowserDialog_Load(object sender, EventArgs e)
        {

        }

        ManualResetEvent _doc_complete = new ManualResetEvent(false); 

        public int LoadPage(string url, out string strError)
        {
            strError = "";

#if NO
            _doc_complete.Reset();
            this.webBrowser1.Navigate(url);
            _doc_complete.WaitOne();
#endif
            this._complete = false;
            this.webBrowser1.Navigate(url);
            while(_complete == false)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            return 0;
        }

        bool _complete = false;

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            _complete = true;
            _doc_complete.Set();
        }

        private void BrowserDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _doc_complete.Set();
        }

        private void BrowserDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
