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

/*
// https://stackoverflow.com/questions/2068159/could-not-load-file-or-assembly-microsoft-mshtml-strong-name-validation-fai
Microsoft.mshtml.dll from PIA folder is not signed.
If you have signed project, you must take version from "Primary Interop Assemblies" folder.

To do that:

1. Remove reference to Microsoft.mshtml (if you have one in your project)

2. Click "Add Reference" and than DO NOT select "Extensions" but "Browse" and point to "C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies" (for .64 bit Machines) - that version is signed.

3. Edit properties (select microsoft.mshtml reference and press F4) in order to set:

Embed Interop Types=false
Copy Local=true

4. Rebuild your project
*/

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
