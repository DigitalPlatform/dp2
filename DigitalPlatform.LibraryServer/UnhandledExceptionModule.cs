using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 负责处理未俘获的异常
    /// </summary>
    public class UnhandledExceptionModule : IHttpModule
    {
        static int _unhandledExceptionCount = 0;

        static string _sourceName = "dp2library";
        static object _initLock = new object();
        static bool _initialized = false;

        public void Init(HttpApplication app)
        {

            // Do this one time for each AppDomain.
            if (!_initialized)
            {
                lock (_initLock)
                {
                    if (!_initialized)
                    {

                        /*
                        string webenginePath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "webengine.dll");

                        if (!File.Exists(webenginePath))
                        {
                            throw new Exception(String.Format(CultureInfo.InvariantCulture,
                                                              "Failed to locate webengine.dll at '{0}'.  This module requires .NET Framework 2.0.",
                                                              webenginePath));
                        }

                        FileVersionInfo ver = FileVersionInfo.GetVersionInfo(webenginePath);
                        _sourceName = string.Format(CultureInfo.InvariantCulture, "ASP.NET {0}.{1}.{2}.0",
                                                    ver.FileMajorPart, ver.FileMinorPart, ver.FileBuildPart);
                         * */

                        if (!EventLog.SourceExists(_sourceName))
                        {
                            throw new Exception(String.Format(CultureInfo.InvariantCulture,
                                                              "There is no EventLog source named '{0}'. This module requires dp2library to be installed.",
                                                              _sourceName));
                        }

                        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

                        _initialized = true;
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        void OnUnhandledException(object o, UnhandledExceptionEventArgs e)
        {
            // Let this occur one time for each AppDomain.
            if (Interlocked.Exchange(ref _unhandledExceptionCount, 1) != 0)
                return;

            StringBuilder message = new StringBuilder("UnhandledException:\r\nappId=");

            string appId = (string)AppDomain.CurrentDomain.GetData(".appId");
            if (appId != null)
            {
                message.Append(appId);
            }


            Exception currentException = null;
            for (currentException = (Exception)e.ExceptionObject; currentException != null; currentException = currentException.InnerException)
            {
                message.AppendFormat("\r\nType: {0}\r\n\r\nMessage: {1}\r\n\r\nStack:\r\n{2}\r\n",
                                     currentException.GetType().FullName,
                                     currentException.Message,
                                     currentException.StackTrace);
            }

            EventLog Log = new EventLog();
            Log.Source = _sourceName;
            Log.WriteEntry(message.ToString(), EventLogEntryType.Error);
        }

    }
}
