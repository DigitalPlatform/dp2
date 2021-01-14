using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;
using DigitalPlatform.Text;

namespace RfidTool
{
    static class Program
    {
        // https://stackoverflow.com/questions/8836093/how-can-i-specify-a-dllimport-path-at-runtime
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ClientInfo.TypeOfProgram = typeof(Program);
            FormClientInfo.CopyrightKey = "rfidtool_sn_key";

            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, IntPtr.Size == 8 ? "x64" : "x86");

            SetDllDirectory(assemblyPath);



            // http://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
            bool createdNew = true;
            // mutex name need contains windows account name. or us programes file path, hashed
            using (Mutex mutex = new Mutex(true, "RfidTool V1", out createdNew))
            {
                if (createdNew)
                {
                    /*
                    if (StringUtil.IsDevelopMode() == false)
                        PrepareCatchException();
                    */

                    ProgramUtil.SetDpiAwareness();

                    // Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            API.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }

#if NO

        // ׼���ӹ�δ������쳣
        static void PrepareCatchException()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        static void CurrentDomain_UnhandledException(object sender,
    UnhandledExceptionEventArgs e)
        {
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.ExceptionObject;
            string strError = GetExceptionText(ex, "");

            // TODO: ����Ϣ�ṩ������ƽ̨�Ŀ�����Ա���Ա����
            // TODO: ��ʾΪ��ɫ���ڣ���ʾ�������˼
            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2LibraryXE ����δ֪���쳣:\r\n\r\n" + strError + "\r\n---\r\n\r\n�㡰�رա����رճ���",
    "dp2LibraryXE ����δ֪���쳣",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bSendReport,
    new string[] { "�ر�" },
    "����Ϣ���͸�������");
            // �����쳣����
            if (bSendReport)
                CrashReport(strError);
        }

        static string GetExceptionText(Exception ex, string strType)
        {
            // Exception ex = (Exception)e.Exception;
            string strError = "����δ�����" + strType + "�쳣: \r\n" + ExceptionUtil.GetDebugText(ex);
            Assembly myAssembly = Assembly.GetAssembly(typeof(Program));
            strError += "\r\ndp2LibraryXE �汾: " + myAssembly.FullName;
            strError += "\r\n����ϵͳ��" + Environment.OSVersion.ToString();
            strError += "\r\n���� MAC ��ַ: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress());

            // TODO: ��������ϵͳ��һ����Ϣ

            // MainForm main_form = Form.ActiveForm as MainForm;
            if (_mainForm != null)
            {
                try
                {
                    _mainForm.WriteErrorLog(strError);
                }
                catch
                {
                    WriteWindowsLog(strError, EventLogEntryType.Error);
                }
            }
            else
                WriteWindowsLog(strError, EventLogEntryType.Error);

            return strError;
        }

        static void Application_ThreadException(object sender,
            ThreadExceptionEventArgs e)
        {
            if (bExiting == true)
                return;

            Exception ex = (Exception)e.Exception;
            string strError = GetExceptionText(ex, "�����߳�");

            bool bSendReport = true;
            DialogResult result = MessageDlg.Show(_mainForm,
    "dp2LibraryXE ����δ֪���쳣:\r\n\r\n" + strError + "\r\n---\r\n\r\n�Ƿ�رճ���?",
    "dp2LibraryXE ����δ֪���쳣",
    MessageBoxButtons.YesNo,
    MessageBoxDefaultButton.Button2,
    ref bSendReport,
    new string[] { "�ر�", "����" },
    "����Ϣ���͸�������");
            {
                if (bSendReport)
                    CrashReport(strError);
            }
            if (result == DialogResult.Yes)
            {
                //End();
                bExiting = true;
                Application.Exit();
            }
        }

        static string GetMacAddressString()
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            return StringUtil.MakePathList(macs);
        }

        static void CrashReport(string strText)
        {
            // MainForm main_form = Form.ActiveForm as MainForm;

            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            //_messageBar.BackColor = SystemColors.Info;
            //_messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2LibraryXE �����쳣";
            _messageBar.MessageText = "������ dp2003.com �����쳣���� ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            _messageBar.Show(_mainForm);
            _messageBar.Update();

            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                if (_mainForm != null)
                {
                    string strUid = _mainForm.GetLibraryXmlUid();
                    if (string.IsNullOrEmpty(strUid) == false)
                        strSender = "@" + strUid;
                    else
                        strSender = "@MAC:" + GetMacAddressString();
                }
                else
                    strSender = "@MAC:" + GetMacAddressString();

                // ��������
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2libraryxe",
                    strText,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() ���̳����쳣: " + ExceptionUtil.GetDebugText(ex);
                nRet = -1;
            }
            finally
            {
                _messageBar.Close();
                _messageBar = null;
            }

            if (nRet == -1)
            {
                strError = "�� dp2003.com �����쳣����ʱ����δ�ܷ��ͳɹ�����ϸ���: " + strError;
                MessageBox.Show(strError);
                // д�������־
                if (_mainForm != null)
                    _mainForm.WriteErrorLog(strError);
                else
                    WriteWindowsLog(strError, EventLogEntryType.Error);
            }
        }

        // д��Windowsϵͳ��־
        public static void WriteWindowsLog(string strText,
            EventLogEntryType type)
        {
            EventLog Log = new EventLog("Application");
            Log.Source = "dp2LibraryXE";
            Log.WriteEntry(strText, type);
        }
#endif
    }
}
