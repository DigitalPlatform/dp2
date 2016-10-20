using mshtml;
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
            while (_complete == false)
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

        public string GetQikanGuid()
        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser1.Document.DomDocument;
            IHTMLControlRange imgRange = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();

            string html = webBrowser1.DocumentText;

            foreach (IHTMLElement ele in doc.all)
            {
                if (!(ele is IHTMLAnchorElement))
                    continue;
                IHTMLAnchorElement anchor = ele as IHTMLAnchorElement;
                string href = anchor.href;
                if (string.IsNullOrEmpty(href) == false && href.IndexOf("magdetails") != -1)
                {
                    string word = "magdetails/";
                    int index = href.IndexOf(word);
                    if (index == -1)
                        return null;
                    string result = href.Substring(index + word.Length);
                    index = result.IndexOf("/");
                    if (index == -1)
                        return null;
                    return result.Substring(0, index);
                }
            }

            return null;
        }

        public bool CopyImageToClipboard()
        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser1.Document.DomDocument;
            IHTMLControlRange imgRange = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();

            foreach (IHTMLImgElement img in doc.images)
            {
                string alt = img.alt;
                if (alt.IndexOf("期") != -1)
                {
                    imgRange.add(img as IHTMLControlElement);

                    imgRange.execCommand("Copy", false, null);
                    return true;
                }
            }

            return false;
        }

        public bool CopyImageToClipboard1()
        {
            IHTMLDocument2 doc = (IHTMLDocument2)webBrowser1.Document.DomDocument;
            IHTMLControlRange imgRange = (IHTMLControlRange)((HTMLBody)doc.body).createControlRange();

            foreach (IHTMLImgElement img in doc.images)
            {
                IHTMLElement parent = ((IHTMLElement)img).parentElement;
                if (parent.className != "magazine-info-img")
                    continue;
                {
                    imgRange.add(img as IHTMLControlElement);

                    imgRange.execCommand("Copy", false, null);
                    return true;
                }
            }

            return false;
        }

    }
}
