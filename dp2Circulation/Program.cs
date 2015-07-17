using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace dp2Circulation
{
    static class Program
    {
        static bool bExiting = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
             //AppDomain currentDomain = AppDomain.CurrentDomain;
  //currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            Begin();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void Begin()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

#if NO
        static void End()
        {
            Application.ThreadException -= Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
#endif

        static void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            string strError = "发生未捕获的异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            MainForm main_form = Form.ActiveForm as MainForm;
            if (main_form != null)
                main_form.WriteErrorLog(strError);
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);

            // TODO: 把信息提供给数字平台的开发人员，以便纠错
            // TODO: 显示为红色窗口，表示警告的意思
            bool bTemp = false;
            DialogResult result = MessageDlg.Show(main_form,
    "dp2Circulation 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n点“关闭”即关闭程序",
    "dp2Circulation 发生未知的异常",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bTemp,
    new string[] { "关闭" });
#if NO
            if (result == DialogResult.Yes)
            {
                    bExiting = true;
                    Application.Exit();
            }
#endif

            // 崩溃报告
            CrashReport(strError);
        }

        static void Application_ThreadException(object sender, 
            ThreadExceptionEventArgs e)
        {
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            string strError = "发生未捕获的界面线程异常: \r\n" + ExceptionUtil.GetDebugText(ex);
            MainForm main_form = Form.ActiveForm as MainForm;
            if (main_form != null)
                main_form.WriteErrorLog(strError);
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);

            bool bTemp = false;
            DialogResult result = MessageDlg.Show(main_form,
    "dp2Circulation 发生未知的异常:\r\n\r\n" + strError + "\r\n---\r\n\r\n是否关闭程序?",
    "dp2Circulation 发生未知的异常",
    MessageBoxButtons.YesNo,
    MessageBoxDefaultButton.Button2,
    ref bTemp,
    new string[] { "关闭", "继续" });
            {
                CrashReport(strError);
            }
            if (result == DialogResult.Yes)
            {
                //End();
                bExiting = true;
                Application.Exit();
            }
        }

        static void CrashReport(string strText)
        {
            MainForm main_form = Form.ActiveForm as MainForm;

            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            //_messageBar.BackColor = SystemColors.Info;
            //_messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2Circulation 出现异常";
            _messageBar.MessageText = "正在向 dp2003.com 发送崩溃报告 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            _messageBar.Show(main_form);
            _messageBar.Update();

            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                if (main_form != null)
                    strSender = main_form.GetCurrentUserName() + "@" + main_form.ServerUID;
                // 崩溃报告
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2circulation",
                    strText,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                nRet = -1;
            }
            finally
            {
                _messageBar.Close();
                _messageBar = null;
            }

            if (nRet == -1)
            {
                strError = "向 dp2003.com 发送崩溃报告时出错，未能发送成功。详细情况: " + strError;
                MessageBox.Show(main_form, strError);
                // 写入错误日志
                if (main_form != null)
                    main_form.WriteErrorLog(strError);
                else
                    WriteWindowsLog(strError, EventLogEntryType.Error);
            }
        }

        // 写入Windows系统日志
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog("Application");
            Log.Source = "dp2Circulation";
            Log.WriteEntry(strText, type);
        }
    }
}