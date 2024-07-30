using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
    public partial class VerifyViewerForm : Form
    {
        /// <summary>
        /// 停靠
        /// </summary>
        public event DoDockEventHandler DoDockEvent = null;

        public bool Docked = false;

        // public MainForm MainForm = null;

        public event LocateEventHandler Locate = null;

        public VerifyViewerForm()
        {
            InitializeComponent();
        }

        string _text = "";

        public string ResultString
        {
            get
            {
                // return this.textBox_verifyResult.Text;
                return _text;
            }
            set
            {
                this.TryInvoke(() =>
                {
                    // this.textBox_verifyResult.Text = value;
                    _text = value;
                    ClearHtml(CssFileName);
                    WriteHtml(this.webBrowser_verifyResult,
                        /*"<div class='line info'>" +
                        HttpUtility.HtmlEncode(_text)
                        .Replace("\r\n", "<br/>")
                        + "</div>"*/
                        BuildHtml(value));
                });
            }
        }

        static string BuildHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            StringBuilder result = new StringBuilder();
            var lines = text.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                string style = "info";
                if (line.StartsWith("[error]")
                    || line.StartsWith("[错误]"))
                    style = "error";
                else if (line.StartsWith("[warning]")
                    || line.StartsWith("[警告]"))
                    style = "warning";
                else if (line.StartsWith("[info]")
                    || line.StartsWith("[信息]"))
                    style = "info";
                else if (line.StartsWith("[succeed]")
    || line.StartsWith("[成功]"))
                    style = "succeed";


                result.AppendLine($"<div class='line {style}'>{HttpUtility.HtmlEncode(line)}</div>");
            }

            return result.ToString();
        }

        public void WriteError(string text,
            bool clear_before)
        {
            this.TryInvoke(() =>
            {
                if (clear_before)
                    ClearHtml(CssFileName);
                if (string.IsNullOrEmpty(text))
                    return;
                var lines = text.Replace("\r\n", "\n").Split('\n');
                foreach (var line in lines)
                {
                    WriteError(new VerifyError
                    {
                        Level = "error",
                        Text = line,
                    });
                }
            });
        }

        public void WriteError(VerifyError error,
            bool clear_before = false)
        {
            this.TryInvoke(() =>
            {
                if (clear_before)
                    ClearHtml(CssFileName);

                string style = "info";
                if (error.Level == "error")
                    style = "error";
                else if (error.Level == "warning")
                    style = "warning";
                else if (error.Level == "info")
                    style = "info";
                else if (error.Level == "succeed")
                    style = "succeed";

                var html = $"<div class='line {style}'>{HttpUtility.HtmlEncode(error.Text).Replace("\r\n", "<br/>")}</div>";
                WriteHtml(webBrowser_verifyResult, html);
            });
        }

        public void WriteErrors(List<VerifyError> errors,
            bool clear_before = true)
        {
            this.TryInvoke(() =>
            {
                if (clear_before)
                    ClearHtml(CssFileName);

                foreach (var error in errors)
                {
                    WriteError(error);
                }
            });
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);
        }

        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            /*
            this.MainForm.CurrentVerifyResultControl = this.textBox_verifyResult;
            if (bShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            this.Docked = true;
            this.Visible = false;
             * */
            if (this.DoDockEvent != null)
            {
                DoDockEventArgs e = new DoDockEventArgs();
                e.ShowFixedPanel = bShowFixedPanel;
                this.DoDockEvent(this, e);
            }
        }

        #region 防止控件泄露

        // 不会被自动 Dispose 的 子 Control，放在这里托管，避免内存泄漏
        List<Control> _freeControls = new List<Control>();

        public void AddFreeControl(Control control)
        {
            ControlExtention.AddFreeControl(_freeControls, control);
        }

        public void RemoveFreeControl(Control control)
        {
            ControlExtention.RemoveFreeControl(_freeControls, control);
        }

        public void DisposeFreeControls()
        {

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        #endregion

        // 2024/6/15
        public string CssFileName { get; set; }

        public void Clear()
        {
            this.TryInvoke(() =>
            {
                // this.textBox_verifyResult.Text = "";
                ClearHtml(CssFileName);
            });
        }

        public void ClearHtml(string cssFileName)
        {
            string strLink = "<link href='" + cssFileName + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            /*
            // 2009/2/11
            if (String.IsNullOrEmpty(Program.MainForm.LibraryServerDir) == false)
                strJs = "<SCRIPT language='javaSCRIPT' src='" + Program.MainForm.LibraryServerDir + "/getsummary.js" + "'></SCRIPT>";
            */
            // strJs = "<SCRIPT language='javaSCRIPT' src='" + PathUtil.MergePath(Program.MainForm.DataDir, "getsummary.js") + "'></SCRIPT>";

            {
                HtmlDocument doc = null;

                doc = this.webBrowser_verifyResult.Document;
                if (doc == null)
                {
                    this.webBrowser_verifyResult.Navigate("about:blank");
                    doc = this.webBrowser_verifyResult.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser_verifyResult,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");  // 2015/7/28

                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        public static void ScrollToEnd(WebBrowser webBrowser)
        {
            webBrowser.ScrollToEnd();
        }

        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            webBrowser.TryInvoke(() =>
            {
                int nRedoCount = 0;
            REDO:
                try
                {
                    webBrowser.Navigate(urlString);
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    /*
    System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
       在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
       在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
       在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
       在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                     * */
                    if ((uint)ex.ErrorCode == 0x800700AA)
                    {
                        nRedoCount++;
                        if (nRedoCount < 5)
                        {
                            Application.DoEvents(); // 2015/8/13
                            Thread.Sleep(200);
                            goto REDO;
                        }
                    }

                    throw ex;   // TODO: 这里的 throw 谁来捕获?
                }
            });
        }


        /*
        public TextBox ResultControl
        {
            get
            {
                return this.textBox_verifyResult;
            }
        }
        */

        public WebBrowser ResultControl
        {
            get
            {
                return this.webBrowser_verifyResult;
            }
        }

        private void textBox_verifyResult_DoubleClick(object sender, EventArgs e)
        {
            if (this.Locate == null)
                return;
            if (textBox_verifyResult.Lines.Length == 0)
                return;

            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_verifyResult,
                out x,
                out y);

            string strLine = "";

            try
            {
                strLine = textBox_verifyResult.Lines[y];
            }
            catch
            {
                return;
            }

            // 析出"(字段名，子字段名, 字符位置)"值

            int nRet = strLine.IndexOf("(");
            if (nRet == -1)
                return;
            strLine = strLine.Substring(nRet + 1);
            nRet = strLine.IndexOf(")");
            if (nRet != -1)
                strLine = strLine.Substring(0, nRet);
            strLine = strLine.Trim();

            LocateEventArgs e1 = new LocateEventArgs();
            e1.Location = strLine;
            this.Locate(this, e1);
        }

        // Dock 停靠以后，this.Visible == true，只能用 ResultControl
        void TryInvoke(Action method)
        {
            this.ResultControl.TryInvoke(method);
        }

        T TryGet<T>(Func<T> func)
        {
            return this.ResultControl.TryGet(func);
        }
    }

    //
    public delegate void DoDockEventHandler(object sender,
DoDockEventArgs e);

    public class DoDockEventArgs : EventArgs
    {
        public bool ShowFixedPanel = false; // [in]
    }

    // 
    public delegate void LocateEventHandler(object sender,
        LocateEventArgs e);

    public class LocateEventArgs : EventArgs
    {
        public string Location = "";
    }
}
