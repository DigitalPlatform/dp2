using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using dp2LibraryApiTester.TestCase;

namespace dp2LibraryApiTester
{
    public partial class MainForm : Form
    {
        CancellationTokenSource _cancelApp = new CancellationTokenSource();

        public MainForm()
        {
            InitializeComponent();

            FormClientInfo.MainForm = this;
        }

        private void MenuItem_initialEnvironment_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "暂未实现");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ret = FormClientInfo.Initial("dp2LibraryApiTester",
    () => StringUtil.IsDevelopMode());
            if (ret == false)
            {
                Application.Exit();
                return;
            }

            // ClearForPureTextOutputing(this.webBrowser1);
            ClearHtml(this.webBrowser1);

            AppendString("Form1_Load\r\n");

            LoadSettings();

            DataModel.Initial();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancelApp?.Dispose();

            DataModel.Free();

            SaveSettings();
        }

        private void MenuItem_settings_Click(object sender, EventArgs e)
        {
            OpenSettingDialog(this);
        }

        public void OpenSettingDialog(Form parent,
string style = "")
        {
            using (SettingDialog dlg = new SettingDialog())
            {
                GuiUtil.SetControlFont(dlg, parent.Font);
                ClientInfo.MemoryState(dlg, "settingDialog", "state");
                dlg.ShowDialog(parent);
            }
        }

        void LoadSettings()
        {
            this.UiState = ClientInfo.Config.Get("global", "ui_state", "");

            // 恢复 MainForm 的显示状态
            {
                var state = ClientInfo.Config.Get("mainForm", "state", "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    FormProperty.SetProperty(state, this, ClientInfo.IsMinimizeMode());
                }
            }

        }

        void SaveSettings()
        {
            // 保存 MainForm 的显示状态
            {
                var state = FormProperty.GetProperty(this);
                ClientInfo.Config.Set("mainForm", "state", state);
            }

            ClientInfo.Config?.Set("global", "ui_state", this.UiState);
            ClientInfo.Finish();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    //this.tabControl1,
                    //this.listView_writeHistory,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    //this.tabControl1,
                    //this.listView_writeHistory,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void MenuItem_test_searchReaderSafety_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    TestSearchReaderSafety.TestCross();

                    TestSearchReaderSafety.TestAll();

                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }


        #region console

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml(WebBrowser webBrowser)
        {
            string strCssUrl = Path.Combine(ClientInfo.DataDir, "history.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            {
                HtmlDocument doc = webBrowser.Document;

                if (doc == null)
                {
                    webBrowser.Navigate("about:blank");
                    doc = webBrowser.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(webBrowser,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        /// <summary>
        /// 将浏览器控件中已有的内容清除，并为后面输出的纯文本显示做好准备
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        public static void ClearForPureTextOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
    // + "<link rel='stylesheet' href='"+strCssFileName+"' type='text/css'>"
    + "<style media='screen' type='text/css'>"
    + "body { font-family:Microsoft YaHei; background-color:#555555; color:#eeeeee; } " // background-color:#555555
    + "</style>"
    + "</head><body>";

            doc = doc.OpenNew(true);
            doc.Write(strHead + "<pre style=\"font-family:Consolas; \">");  // Calibri
        }

        /// <summary>
        /// 将 HTML 信息输出到控制台，显示出来。
        /// </summary>
        /// <param name="strText">要输出的 HTML 字符串</param>
        public void WriteToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, strText);
        }

        /// <summary>
        /// 将文本信息输出到控制台，显示出来
        /// </summary>
        /// <param name="strText">要输出的文本字符串</param>
        public void WriteTextToConsole(string strText)
        {
            WriteHtml(this.webBrowser1, HttpUtility.HtmlEncode(strText));
        }

        /// <summary>
        /// 向一个浏览器控件中追加写入 HTML 字符串
        /// 不支持异步调用
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);
        }


        void AppendSectionTitle(string strText)
        {
            AppendCrLn();
            AppendString("*** " + strText + " ***\r\n");
            AppendCurrentTime();
            AppendCrLn();
        }

        void AppendCurrentTime()
        {
            AppendString("*** " + DateTime.Now.ToString() + " ***\r\n");
        }

        void AppendCrLn()
        {
            AppendString("\r\n");
        }

        // 线程安全
        // parameters:
        //      style 为 begin end warning error green 之一
        public void AppendString(string strText, string style = "")
        {
            /*
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<string>(AppendString), strText);
                return;
            }
            this.WriteTextToConsole(strText);
            ScrollToEnd();
            */
            strText = strText.Replace("\r", "\n");
            strText = strText.TrimEnd(new char[] { '\n' });
            string[] lines = strText.Split(new char[] { '\n' });
            foreach (string line in lines)
            {
                AppendHtml($"<div class='debug {style}'>" + HttpUtility.HtmlEncode(line).Replace(" ", "&nbsp;") + "</div>");
            }
        }

        // 线程安全
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.Invoke(new Action<string>(AppendHtml), strText);
                return;
            }
            this.WriteToConsole(strText);
            ScrollToEnd();
        }

        void ScrollToEnd()
        {
            this.webBrowser1.ScrollToEnd();
        }


        #endregion

        private void MenuItem_test_searchBiblioSafety_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    NormalResult result = TestSearchBiblioSafety.PrepareEnvironment();
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestSearchBiblio("SearchBiblio", "test_access2");
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestSearchBiblio("SearchBiblio", "test_access1");
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestSearchBiblio("SearchBiblio", "test_rights");
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestSearchBiblio("Search", "test_access2");
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestSearchBiblio("Search", "test_access1");
                    if (result.Value == -1) return;
                    result = TestSearchBiblioSafety.TestSearchBiblio("Search", "test_rights");
                    if (result.Value == -1) return;

                    result = TestSearchBiblioSafety.TestGetBrowseRecords();
                    if (result.Value == -1) return;

                    TestSearchBiblioSafety.Finish();
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        private void MenuItem_test_searchItemSafety_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    TestSearchItemSafety.TestAll("item");
                    TestSearchItemSafety.TestAll("issue");
                    TestSearchItemSafety.TestAll("order");
                    TestSearchItemSafety.TestAll("comment");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });

        }

        private void MenuItem_test_setReaderInfoApi_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    TestSetReaderInfoApi.TestAll();
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // 准备借阅信息链测试环境
        private void MenuItem_test_prepareCheckBorrow_prepare_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        private void MenuItem_test_prepareCheckBorrow_clear_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.Finish();
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // "册记录缺 borrower"
        private void MenuItem_test_prepareCheckBorrow_prepare_condition1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("册记录缺 borrower");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // "册记录 borrower 错位"
        private void MenuItem_test_prepareCheckBorrow_prepare_condition2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("册记录 borrower 错位");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // "册记录不存在"
        private void MenuItem_test_prepareCheckBorrow_prepare_condition3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("册记录不存在");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // "读者记录缺 borrow"
        private void MenuItem_test_prepareCheckBorrow_prepare_condition4_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("读者记录缺 borrow");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // "读者记录不存在"
        private void MenuItem_test_prepareCheckBorrow_prepare_condition5_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    PrepareCheckBorrow.TestAll("读者记录不存在");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        private async void MenuItem_test_loginApi_Click(object sender, EventArgs e)
        {
            await Process((token) =>
            {
                try
                {
                    var result = TestLoginApi.TestAll(token);
                    if (result.Value == -1)
                        DataModel.SetMessage(result.ErrorInfo, "error");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
#if REMOVED
            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancelApp.Token))
            {
                _cancelCurrent = cancel;

                await Task.Run(() =>
                {
                    EnableControls(false);
                    try
                    {
                        var result = TestLoginApi.TestAll(cancel.Token);
                        if (result.Value == -1)
                            DataModel.SetMessage(result.ErrorInfo, "error");
                    }
                    catch (Exception ex)
                    {
                        AppendString($"exception: {ex.Message}", "error");
                    }
                    finally
                    {
                        EnableControls(true);
                    }
                });
            }
#endif
        }

        private void MenuItem_test_reservation_all_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    var result = TestReservation.TestAll("");
                    if (result.Value == -1)
                        AppendString(result.ErrorInfo, "error");
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        private async void MenuItem_test_setEntitiesFieldRights_Click(object sender, EventArgs e)
        {
            await Process((token) =>
            {
                try
                {
                    NormalResult result;

                    result = TestSetEntities.TestAll("item", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }

                    result = TestSetEntities.TestAll("issue", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                    result = TestSetEntities.TestAll("order", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }

                    result = TestSetEntities.TestAll("comment", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        CancellationTokenSource _cancelCurrent = new CancellationTokenSource();


        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            _cancelCurrent?.Cancel();
        }

        void EnableControls(bool enable)
        {
            this.Invoke(new Action(() =>
            {
                this.menuStrip1.Enabled = enable;
                this.toolStripButton_stop.Enabled = !enable;
            }));
        }

        private async void MenuItem_test_getEntitiesFieldRights_Click(object sender, EventArgs e)
        {
            await Process((token) =>
            {
                try
                {
                    NormalResult result;

                    result = TestGetEntities.TestAll("item", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }

                    result = TestGetEntities.TestAll("issue", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                    result = TestGetEntities.TestAll("order", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }

                    result = TestGetEntities.TestAll("comment", token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        // delegate void delegate_process(CancellationToken token);

        async Task Process(Action<CancellationToken> action)
        {
            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancelApp.Token))
            {
                _cancelCurrent = cancel;
                EnableControls(false);
                try
                {
                    await Task.Factory.StartNew(
                        () => action(cancel.Token),
                        cancel.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                    // await Task.Run(()=>action(cancel.Token), cancel.Token);
                }
                finally
                {
                    EnableControls(true);
                }
            }
        }

        async private void MenuItem_test_setBiblioInfo_Click(object sender, EventArgs e)
        {
            await Process((token) =>
            {
                try
                {
                    NormalResult result;

                    result = TestSetBiblioInfo.TestAll(token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }

        private async void MenuItem_test_copyBiblioInfo_Click(object sender, EventArgs e)
        {
            await Process((token) =>
            {
                try
                {
                    NormalResult result;

                    result = TestCopyBiblioInfo.TestAll(token);
                    if (result.Value == -1)
                    {
                        AppendString(result.ErrorInfo, "error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AppendString($"exception: {ex.Message}", "error");
                }
            });
        }
    }
}
