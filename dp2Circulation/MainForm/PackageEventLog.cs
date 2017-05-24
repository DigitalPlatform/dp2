using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

using Ionic.Zip;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 实现打包日志文件的辅助类
    /// </summary>
    public class PackageEventLog
    {
        // return:
        //      true    继续操作
        //      false   希望中断
        public delegate bool DispInfo(string strText);


        // parameters:
        //      strDataDir  存储需要打包数据的目录。即 dp2circulation 用户目录
        //      strTempDir  临时目录。本方法要先清空这个目录，所以应该提供一个专用临时目录
        public static int Package(List<EventLog> logs,
            string strZipFileName,
            string strRangeName,
            string strDataDir,
            string strTempDir,
            DispInfo procDispInfo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            PathUtil.TryClearDir(strTempDir);

            List<string> filenames = new List<string>();

            foreach (EventLog log in logs)
            {
                // 创建 eventlog_digitalplatform.txt 文件
                string strEventLogFilename = Path.Combine(strTempDir, "eventlog_" + log.LogDisplayName + ".txt");

                //
                //
                nRet = MakeWindowsLogFile(log,
                    strEventLogFilename,
                    procDispInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet > 0)
                    filenames.Add(strEventLogFilename);
                else
                    File.Delete(strEventLogFilename);
            }

            // 创建一个描述了安装的各个实例和环境情况的文件
            string strDescriptionFilename = Path.Combine(strTempDir, "description.txt");
            try
            {
                if (procDispInfo != null)
                    procDispInfo("正在准备 description.txt 文件 ...");

                using (StreamWriter sw = new StreamWriter(strDescriptionFilename, false, Encoding.UTF8))
                {
                    sw.Write(GetEnvironmentDescription());
                }
            }
            catch (Exception ex)
            {
                strError = "输出 description.txt 时出现异常: " + ex.Message;
                return -1;
            }

            filenames.Add(strDescriptionFilename);

            // TODO: 是否复制整个数据目录？ 需要避免复制日志文件和其他尺寸很大的文件

            // 复制错误日志文件和其他重要文件
            List<string> dates = MakeDates(strRangeName); // "最近31天""最近十年""最近七天"

            // *** dp2library 各个 instance
            string strInstanceDir = Path.Combine(strTempDir, "log");
            PathUtil.TryCreateDir(strInstanceDir);

            foreach (string date in dates)
            {
#if NO
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
#endif
                if (procDispInfo != null)
                {
                    if (procDispInfo(null) == false)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                string strFilePath = Path.Combine(strDataDir, "log/log_" + date + ".txt");
                if (File.Exists(strFilePath) == false)
                    continue;
                string strTargetFilePath = Path.Combine(strInstanceDir, "log_" + date + ".txt");

                if (procDispInfo != null)
                    procDispInfo("正在复制文件 " + strFilePath);

                File.Copy(strFilePath, strTargetFilePath);
                filenames.Add(strTargetFilePath);
            }

            if (filenames.Count == 0)
                return 0;

            if (filenames.Count > 0)
            {
                // bool bRangeSetted = false;
                using (ZipFile zip = new ZipFile(Encoding.UTF8))
                {
                    // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                    // https://dotnetzip.codeplex.com/workitem/14087
                    // uncommenting the following line can be used as a work-around
                    zip.ParallelDeflateThreshold = -1;

                    foreach (string filename in filenames)
                    {
#if NO
                            Application.DoEvents();

                            if (stop != null && stop.State != 0)
                            {
                                strError = "用户中断";
                                return -1;
                            }
#endif
                        if (procDispInfo != null)
                        {
                            if (procDispInfo(null) == false)
                            {
                                strError = "用户中断";
                                return -1;
                            }
                        }

                        string strShortFileName = filename.Substring(strTempDir.Length + 1);
                        if (procDispInfo != null)
                            procDispInfo("正在压缩 " + strShortFileName);
                        string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                        zip.AddFile(filename, directoryPathInArchive);
                    }

                    if (procDispInfo != null)
                        procDispInfo("正在写入压缩文件 ...");

#if NO
                        zip.SaveProgress += (s, e) =>
                        {
                            Application.DoEvents();
                            if (stop != null && stop.State != 0)
                            {
                                e.Cancel = true;
                                return;
                            }

                            if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                            {
                                if (bRangeSetted == false)
                                {
                                    stop.SetProgressRange(0, e.EntriesTotal);
                                    bRangeSetted = true;
                                }

                                stop.SetProgressValue(e.EntriesSaved);
                            }
                        };
#endif

                    zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                    zip.Save(strZipFileName);

#if NO
                        stop.HideProgress();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
#endif
                }

                if (procDispInfo != null)
                    procDispInfo("正在删除中间文件 ...");

                // 删除原始文件
                foreach (string filename in filenames)
                {
                    File.Delete(filename);
                }

                // 删除三个子目录
                PathUtil.DeleteDirectory(Path.Combine(strTempDir, "log"));
            }

            return 0;
        }

        // 获得环境描述字符串
        public static string GetEnvironmentDescription()
        {
            //string strError = "";

            StringBuilder text = new StringBuilder();
            text.Append("信息创建时间:\t" + DateTime.Now.ToString() + "\r\n");
            text.Append("当前操作系统信息:\t" + Environment.OSVersion.ToString() + "\r\n");
            text.Append("当前操作系统版本号:\t" + Environment.OSVersion.Version.ToString() + "\r\n");
            List<string> macs = SerialCodeForm.GetMacAddress();
            text.Append("本机 MAC 地址:\t" + StringUtil.MakePathList(macs) + "\r\n");
            // https://support.microsoft.com/zh-cn/kb/2468871
            // "KB2468871" 关系到 SignalR 运行是否会出现装载 System.Core 失败的故障
            text.Append("是否安装了 KB2468871:\t" + Global.IsKbInstalled("KB2468871") + "\r\n");
            text.Append("系统进程:\r\n" + ListSystemProcess());
            text.Append("当前程序版本:\t" + Assembly.GetCallingAssembly().FullName + "\r\n");
            return text.ToString();
        }

        static string ListSystemProcess()
        {
            StringBuilder text = new StringBuilder();

            // 驱动
            {
                ServiceController[] devices = ServiceController.GetDevices();
                text.Append("--- Devices:\r\n");
                int i = 0;
                foreach (ServiceController controller in devices)
                {
                    text.Append((i + 1).ToString() + ") " + controller.DisplayName + "\r\n");
                    i++;
                }
            }

            // 系统进程
            {
                System.Diagnostics.Process[] process_list = System.Diagnostics.Process.GetProcesses();
                text.Append("--- System process:\r\n");
                int i = 0;
                foreach (Process process in process_list)
                {
                    string ModuleName = "";
                    try
                    {
                        ModuleName = process.MainModule.ModuleName;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    text.Append((i + 1).ToString() + ") " + ModuleName + "\r\n");
                    i++;
                }
            }

            return text.ToString();
        }

        // 2015/9/15
        public static void EnvironmentReport(MainForm mainForm)
        {
#if NO
            MessageBar _messageBar = null;

            _messageBar = new MessageBar();
            _messageBar.TopMost = false;
            //_messageBar.BackColor = SystemColors.Info;
            //_messageBar.ForeColor = SystemColors.InfoText;
            _messageBar.Text = "dp2Circulation 出现异常";
            _messageBar.MessageText = "正在向 dp2003.com 发送异常报告 ...";
            _messageBar.StartPosition = FormStartPosition.CenterScreen;
            _messageBar.Show(_mainForm);
            _messageBar.Update();
#endif
            int nRet = 0;
            string strError = "";
            try
            {
                string strSender = "";
                if (mainForm != null)
                    strSender = mainForm.GetCurrentUserName() + "@" + mainForm.ServerUID;
                // 崩溃报告
                nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2circulation 环境报告",
                    GetEnvironmentDescription().Replace("\t", "    "),
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex);
                nRet = -1;
            }
            finally
            {
#if NO
                _messageBar.Close();
                _messageBar = null;
#endif
            }

#if NO
            if (nRet == -1)
            {
                strError = "向 dp2003.com 发送异常报告时出错，未能发送成功。详细情况: " + strError;
                MessageBox.Show(_mainForm, strError);
                // 写入错误日志
                if (_mainForm != null)
                    _mainForm.WriteErrorLog(strError);
                else
                    WriteWindowsLog(strError, EventLogEntryType.Error);
            }
#endif
        }


        // 创建 Windows 存储事件日志的文件
        static int MakeWindowsLogFile(EventLog log,
            string strEventLogFilename,
            DispInfo procDispInfo,
            out string strError)
        {
            strError = "";
            int nLines = 0;
            try
            {
                if (procDispInfo != null)
                    procDispInfo("正在准备 Windows 事件日志 " + log.LogDisplayName + "...");

                using (StreamWriter sw = new StreamWriter(strEventLogFilename, false, Encoding.UTF8))
                {
                    foreach (EventLogEntry entry in log.Entries)
                    {
#if NO
                        Application.DoEvents();
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return -1;
                        }
#endif
                        if (procDispInfo != null)
                        {
                            if (procDispInfo(null) == false)
                            {
                                strError = "用户中断";
                                return -1;
                            }
                        }

                        // 过滤出只和 dp2circulation 有关的
                        string strMessageText = entry.Message.ToLower();
                        if (strMessageText.IndexOf("dp2circulation") != -1)
                        {
                            string strText = "*\r\n"
                                + entry.Source + " \t"
                                + entry.EntryType.ToString() + " \t"
                                + entry.TimeGenerated.ToString() + "\r\n"
                                + entry.Message + "\r\n\r\n";

                            sw.Write(strText);
                            nLines++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "输出 Windows 日志 " + log.LogDisplayName + "的信息时出现异常: " + ex.Message;
                return -1;
            }

            return nLines;
        }

        static List<string> MakeDates(string strName)
        {
            List<string> filenames = new List<string>();

            string strStartDate = "";
            string strEndDate = "";

            if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                throw new Exception("无法识别的周期 '" + strName + "'");
            }

            string strWarning = "";
            string strError = "";
            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            int nRet = MakeDates(strStartDate,
                strEndDate,
                out filenames,
                out strWarning,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#if NO
            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);
#endif

            return filenames;
        ERROR1:
            throw new Exception(strError);
        }

        static int MakeDates(string strStartDate,
    string strEndDate,
    out List<string> dates,
    out string strWarning,
    out string strError)
        {
            dates = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strDate = strStartDate;

            for (; ; )
            {
                dates.Add(strDate);

                string strNextDate = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = GetNextDate(strDate,
                    out strNextDate,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    if (String.Compare(strDate, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                Debug.Assert(strDate != strNextDate, "");

                strDate = strNextDate;
                if (String.Compare(strDate, strEndDate) > 0)
                    break;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且 strNextDate 已经是今天的日子了
        static int GetNextDate(string strDate,
            out string strNextDate,
            out string strError)
        {
            strError = "";
            strNextDate = "";
            int nRet = 0;

#if NO
            string strYear = strDate.Substring(0, 4);
            string strMonth = strDate.Substring(4, 2);
            string strDay = strDate.Substring(6, 2);

            int nYear = 0;
            int nMonth = 0;
            int nDay = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strYear + "' 部分格式错误";
                return -1;
            }

            try
            {
                nMonth = Convert.ToInt32(strMonth);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strMonth + "' 部分格式错误";
                return -1;
            }

            try
            {
                nDay = Convert.ToInt32(strDay);
            }
            catch
            {
                strError = "日志文件名 '" + strDate + "' 中的 '"
                    + strDay + "' 部分格式错误";
                return -1;
            }

            DateTime time = DateTime.Now;
            try
            {
                time = new DateTime(nYear, nMonth, nDay);
            }
            catch (Exception ex)
            {
                strError = "日期 " + strDate + " 格式错误: " + ex.Message;
                return -1;
            }
#endif
            DateTime time = DateTimeUtil.Long8ToDateTime(strDate);

            DateTime now = DateTime.Now;

            // 正规化时间
            nRet = RoundTime("day",
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = RoundTime("day",
                ref time,
                out strError);
            if (nRet == -1)
                return -1;

            bool bNow = false;
            if (time >= now)
                bNow = true;

            time = time + new TimeSpan(1, 0, 0, 0); // 后面一天

            strNextDate = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            time = time.ToLocalTime();
            if (strUnit == "day")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            time = time.ToUniversalTime();

            return 0;
        }
    }
}
