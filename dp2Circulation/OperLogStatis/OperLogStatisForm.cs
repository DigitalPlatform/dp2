using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 日志统计窗
    /// </summary>
    public partial class OperLogStatisForm : MyScriptForm
    {
#if NO
        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();
#endif

        OperLogStatis objStatis = null;
        Assembly AssemblyMain = null;

#if NO
        int AssemblyVersion 
        {
            get
            {
                return MainForm.OperLogStatisAssemblyVersion;
            }
            set
            {
                MainForm.OperLogStatisAssemblyVersion = value;
            }
        }
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public OperLogStatisForm()
        {
            InitializeComponent();
        }

        private void OperLogStatisForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            ScriptManager.CfgFilePath = Path.Combine(
    Program.MainForm.UserDir,
    "statis_projects.xml");

#if NO
            ScriptManager.applicationInfo = Program.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                Program.MainForm.DataDir + "\\statis_projects.xml";
            ScriptManager.DataDir = Program.MainForm.DataDir;

            ScriptManager.CreateDefaultContent -= new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
            ScriptManager.CreateDefaultContent += new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

            try
            {
                ScriptManager.Load();
            }
            catch (FileNotFoundException)
            {
                // 不必报错 2009/2/4
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
#endif

            // 方案名
            this.textBox_projectName.Text = Program.MainForm.AppInfo.GetString(
                "operlogstatisform",
                "projectname",
                "");

            // 起始日期
            this.dateControl_start.Text = Program.MainForm.AppInfo.GetString(
                 "operlogstatisform",
                 "start_date",
                 "");

            // 结束日期
            this.dateControl_end.Text = Program.MainForm.AppInfo.GetString(
                "operlogstatisform",
                "end_date",
                "");

            /*
            // 如何输出表格
            this.checkBox_startToEndTable.Checked = Program.MainForm.AppInfo.GetBoolean(
                "operlogstatisform",
                "startToEndTable",
                true);
            this.checkBox_perYearTable.Checked = Program.MainForm.AppInfo.GetBoolean(
                "operlogstatisform",
                "perYearTable",
                false);
            this.checkBox_perMonthTable.Checked = Program.MainForm.AppInfo.GetBoolean(
                "operlogstatisform",
                "perMonthTable",
                false);
            this.checkBox_perDayTable.Checked = Program.MainForm.AppInfo.GetBoolean(
                "operlogstatisform",
                "perDayTable",
                false);
             * */

        }

        private void OperLogStatisForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif
        }

        private void OperLogStatisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            // 方案名
            Program.MainForm.AppInfo.SetString(
                "operlogstatisform",
                "projectname",
                this.textBox_projectName.Text);

            // 起始日期
            Program.MainForm.AppInfo.SetString(
                "operlogstatisform",
                "start_date",
                this.dateControl_start.Text);
            // 结束日期
            Program.MainForm.AppInfo.SetString(
                "operlogstatisform",
                "end_date",
                this.dateControl_end.Text);

            /*
            // 如何输出表格
            Program.MainForm.AppInfo.SetBoolean(
                "operlogstatisform",
                "startToEndTable",
                this.checkBox_startToEndTable.Checked);
            Program.MainForm.AppInfo.SetBoolean(
                "operlogstatisform",
                "perYearTable",
                this.checkBox_perYearTable.Checked);
            Program.MainForm.AppInfo.SetBoolean(
                "operlogstatisform",
                "perMonthTable",
                this.checkBox_perMonthTable.Checked);
            Program.MainForm.AppInfo.SetBoolean(
                "operlogstatisform",
                "perDayTable",
                this.checkBox_perDayTable.Checked);
             * */

        }

        internal override void CreateDefaultContent(CreateDefaultContentEventArgs e)
        {
            string strPureFileName = Path.GetFileName(e.FileName);

            if (String.Compare(strPureFileName, "main.cs", true) == 0)
            {
                CreateDefaultMainCsFile(e.FileName);
                e.Created = true;
            }
            else
            {
                e.Created = false;
            }
        }
        // 
        /// <summary>
        /// 创建缺省的 main.cs 文件
        /// </summary>
        /// <param name="strFileName">文件全路径</param>
        static void CreateDefaultMainCsFile(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Windows.Forms;");
                sw.WriteLine("using System.IO;");
                sw.WriteLine("using System.Text;");
                sw.WriteLine("using System.Xml;");
                sw.WriteLine("");

                //sw.WriteLine("using DigitalPlatform.MarcDom;");
                //sw.WriteLine("using DigitalPlatform.Statis;");
                sw.WriteLine("using dp2Circulation;");

                sw.WriteLine("using DigitalPlatform.Xml;");

                sw.WriteLine("public class MyStatis : OperLogStatis");

                sw.WriteLine("{");

                sw.WriteLine("	public override void OnBegin(object sender, StatisEventArgs e)");
                sw.WriteLine("	{");
                sw.WriteLine("	}");

                sw.WriteLine("}");
            }
        }

        // 方案管理
        private void button_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "OperLogStatisForm";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = Program.MainForm.AppInfo;
            dlg.DataDir = Program.MainForm.DataDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.button_getProjectName.Enabled = bEnable;

            this.dateControl_start.Enabled = bEnable;
            this.dateControl_end.Enabled = bEnable;

            this.button_next.Enabled = bEnable;

            this.button_projectManage.Enabled = bEnable;
        }

        int RunScript(string strProjectName,
            string strProjectLocate,
            out string strError)
        {
            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在执行脚本 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            _dllPaths.Clear();
            _dllPaths.Add(strProjectLocate);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            try
            {
                int nRet = 0;
                strError = "";

                // Assembly assemblyMain = null;

                this.objStatis = null;
                this.AssemblyMain = null;

                // 2009/11/5 changed
                // 防止以前残留的打开的文件依然没有关闭
                Global.ForceGarbageCollection();

                /*
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                */

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    // out assemblyMain,
                    out objStatis,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
                 * 
                 * 
                string strDllName = "";
                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out strDllName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                System.AppDomain NewAppDomain = System.AppDomain.CreateDomain("NewApplicationDomain");

                ObjectHandle h = NewAppDomain.CreateInstanceFrom(strDllName,
                    "scriptcode.MyStatis");
                objStatis = (Statis)h.Unwrap();

                m_strMainCsDllName = strDllName;

                // 为Statis派生类设置参数
                objStatis.OperLogStatisForm = this;
                objStatis.ProjectDir = strProjectLocate;
                 * */

                // this.AssemblyMain = assemblyMain;

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;
                objStatis.StartDate = this.dateControl_start.Value;
                objStatis.EndDate = this.dateControl_end.Value;

                // 执行脚本的OnInitial()

                // 触发Script中OnInitial()代码
                // OnInitial()和OnBegin的本质区别, 在于OnInitial()适合检查和设置面板参数
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnInitial(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        goto END1;
                }

                // 触发Script中OnBegin()代码
                // OnBegin()中仍然有修改MainForm面板的自由
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnBegin(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        goto END1;
                }

                // 循环
                nRet = DoLoop(DoRecord, out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    goto END1;  // TODO: SkipAll如何执行? 是否连OnEnd也不执行了？

            END1:
                // 触发Script的OnEnd()代码
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnEnd(this, args);
                }

                return 0;

            ERROR1:
                return -1;

            }
            catch (Exception ex)
            {
                strError = "脚本执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                if (objStatis != null)
                    objStatis.FreeResources();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.AssemblyMain = null;

                EnableControls(true);
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
        }

#if NO
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Debug.Assert(false, "");

            string strName = args.Name;

            // return this.AssemblyMain;
            return Assembly.LoadFile(m_strMainCsDllName);

            // return null;
        }
#endif

        int DoRecord(string strLogFileName,
    string strXml,
    bool bInCacheFile,
    long lHint,
    long lIndex,
    long lAttachmentTotalLength,
    object param,
    out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            string strDate = "";
            int nRet = strLogFileName.IndexOf(".");
            if (nRet != -1)
                strDate = strLogFileName.Substring(0, nRet);
            else
                strDate = strLogFileName;

            DateTime currentDate = DateTimeUtil.Long8ToDateTime(strDate);
            // strXml中为日志记录

            // 触发Script中OnRecord()代码
            if (objStatis != null)
            {
                objStatis.Xml = strXml;
                objStatis.CurrentDate = currentDate;
                objStatis.CurrentLogFileName = strLogFileName;
                objStatis.CurrentRecordIndex = lIndex;

                StatisEventArgs args = new StatisEventArgs();
                objStatis.OnRecord(this, args);
                if (args.Continue == ContinueType.SkipAll)
                    return 1;
            }

            return 0;
        }



        // 对每个日志文件，每个日志记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(
            OperLogForm.Delegate_doRecord procDoRecord,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            // long lRet = 0;

            List<string> LogFileNames = null;

            // TODO: 是否需要检查起止日期是否为空值？空值是警告还是就当作今天？

            string strStartDate = DateTimeUtil.DateTimeToString8(this.dateControl_start.Value);
            string strEndDate = DateTimeUtil.DateTimeToString8(this.dateControl_end.Value);

            string strWarning = "";

            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            nRet = MakeLogFileNames(strStartDate,
                strEndDate,
                true,
                out LogFileNames,
                out strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            string strStyle = "";
            if (Program.MainForm.AutoCacheOperlogFile == true)
                strStyle = "autocache";

            ProgressEstimate estimate = new ProgressEstimate();

            nRet = OperLogForm.ProcessFiles(this,
stop,
estimate,
Channel,
LogFileNames,
Program.MainForm.OperLogLevel,
strStyle,
"", // strFilter
Program.MainForm.OperLogCacheDir,
null,   // param,
procDoRecord,   // DoRecord,
out strError);
            if (nRet == -1)
                return -1;

            return nRet;
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            // out Assembly assemblyMain,
            out OperLogStatis objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~operlog_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

            string strLibPaths = "\"" + Program.MainForm.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									//Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.Client.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\digitalplatform.statis.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									// Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "OperLogStatisForm",
                strProjectName,
                "main.cs",
                saAddRef,
                strLibPaths,
                strMainCsDllName,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                    goto ERROR1;
                MessageBox.Show(this, strWarning);
            }


            this.AssemblyMain = Assembly.LoadFrom(strMainCsDllName);
            if (this.AssemblyMain == null)
            {
                strError = "LoadFrom " + strMainCsDllName + " fail";
                goto ERROR1;
            }

            // 得到Assembly中Statis派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.OperLogStatis");

            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.OperLogStatis 派生类。";
                goto ERROR1;
            }

            // new一个Statis派生对象
            objStatis = (OperLogStatis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            /*
            this.AssemblyMain = Assembly.LoadFrom(strMainCsDllName);
            if (this.AssemblyMain == null)
            {
                strError = "LoadFrom " + strMainCsDllName + " fail";
                goto ERROR1;
            }

            // 得到Assembly中Statis派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.OperLogStatis");


            objStatis = (Statis)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(this.AssemblyMain.FullName,
                entryClassType.FullName);

            // assemblyMain = null;

            this.m_strMainCsDllName = strMainCsDllName;
             * */



            // 为Statis派生类设置参数
            objStatis.OperLogStatisForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }


        // 准备脚本环境(2)
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out string strMainCsDllName,
            out string strError)
        {
            this.AssemblyMain = null;
            string strWarning = "";

            strMainCsDllName = strProjectLocate + "\\~main_" + Convert.ToString(AssemblyVersion) + ".dll";    // ++

            string strLibPaths = "\"" + Program.MainForm.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									//Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.Client.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\digitalplatform.statis.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									// Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "OperLogStatisForm",
                strProjectName,
                "main.cs",
                saAddRef,
                strLibPaths,
                strMainCsDllName,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                    goto ERROR1;
                MessageBox.Show(this, strWarning);
            }



            return 0;
        ERROR1:
            return -1;
        }

        // 根据日期范围，发生日志文件名
        // parameters:
        //      strStartDate    起始日期。8字符
        //      strEndDate  结束日期。8字符
        // return:
        //      -1  错误
        //      0   成功
        /// <summary>
        /// 根据日期范围，发生日志文件名
        /// </summary>
        /// <param name="strStartDate">起始日期。8字符</param>
        /// <param name="strEndDate">结束日期。8字符</param>
        /// <param name="bExt">是否包含扩展名 ".log"</param>
        /// <param name="LogFileNames">返回创建的文件名</param>
        /// <param name="strWarning">返回警告信息</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int MakeLogFileNames(string strStartDate,
            string strEndDate,
            bool bExt,  // 是否包含扩展名 ".log"
            out List<string> LogFileNames,
            out string strWarning,
            out string strError)
        {
            LogFileNames = new List<string>();
            strError = "";
            strWarning = "";
            int nRet = 0;

            if (String.Compare(strStartDate, strEndDate) > 0)
            {
                strError = "起始日期 '" + strStartDate + "' 不应大于结束日期 '" + strEndDate + "'。";
                return -1;
            }

            string strLogFileName = strStartDate;

            for (; ; )
            {
                LogFileNames.Add(strLogFileName + (bExt == true ? ".log" : ""));

                string strNextLogFileName = "";
                // 获得（理论上）下一个日志文件名
                // return:
                //      -1  error
                //      0   正确
                //      1   正确，并且strLogFileName已经是今天的日子了
                nRet = NextLogFileName(strLogFileName,
                    out strNextLogFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    if (String.Compare(strLogFileName, strEndDate) < 0)
                    {
                        strWarning = "因日期范围的尾部 " + strEndDate + " 超过今天(" + DateTime.Now.ToLongDateString() + ")，部分日期被略去...";
                        break;
                    }
                }

                strLogFileName = strNextLogFileName;
                if (String.Compare(strLogFileName, strEndDate) > 0)
                    break;
            }

            return 0;
        }

        // 获得（理论上）下一个日志文件名
        // return:
        //      -1  error
        //      0   正确
        //      1   正确，并且strLogFileName已经是今天的日子了
        static int NextLogFileName(string strLogFileName,
            out string strNextLogFileName,
            out string strError)
        {
            strError = "";
            strNextLogFileName = "";
            int nRet = 0;

            string strYear = strLogFileName.Substring(0, 4);
            string strMonth = strLogFileName.Substring(4, 2);
            string strDay = strLogFileName.Substring(6, 2);

            int nYear = 0;
            int nMonth = 0;
            int nDay = 0;

            try
            {
                nYear = Convert.ToInt32(strYear);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strYear + "' 部分格式错误";
                return -1;
            }

            try
            {
                nMonth = Convert.ToInt32(strMonth);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
                    + strMonth + "' 部分格式错误";
                return -1;
            }

            try
            {
                nDay = Convert.ToInt32(strDay);
            }
            catch
            {
                strError = "日志文件名 '" + strLogFileName + "' 中的 '"
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
                strError = "日期 " + strLogFileName + " 格式错误: " + ex.Message;
                return -1;
            }

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

            strNextLogFileName = time.Year.ToString().PadLeft(4, '0')
            + time.Month.ToString().PadLeft(2, '0')
            + time.Day.ToString().PadLeft(2, '0');

            if (bNow == true)
                return 1;

            return 0;
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        /*public*/
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

        // 下一步 按钮
        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.tabControl_main.SelectedTab == this.tabPage_selectProject)
            {
                string strProjectName = this.textBox_projectName.Text;

                if (String.IsNullOrEmpty(strProjectName) == true)
                {
                    strError = "尚未指定方案名";
                    this.textBox_projectName.Focus();
                    goto ERROR1;
                }


                // 切换到时间范围page
                this.tabControl_main.SelectedTab = this.tabPage_timeRange;
                return;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_timeRange)
            {
                // 检查两个日期是否为空，和大小关系
                if (this.dateControl_start.Value == new DateTime((long)0))
                {
                    strError = "尚未指定起始日期";
                    this.dateControl_start.Focus();
                    goto ERROR1;
                }

                if (this.dateControl_end.Value == new DateTime((long)0))
                {
                    strError = "尚未指定结束日期";
                    this.dateControl_end.Focus();
                    goto ERROR1;
                }

                if (this.dateControl_start.Value.Ticks > this.dateControl_end.Value.Ticks)
                {
                    strError = "起始日期不能大于结束日期";
                    goto ERROR1;
                }

                string strProjectName = this.textBox_projectName.Text;
                if (String.IsNullOrEmpty(strProjectName) == true)
                {
                    strError = "尚未指定方案名";
                    this.textBox_projectName.Focus();
                    goto ERROR1;
                }

#if NO
                if (this.textBox_projectName.Text[0] == '#')
                {
                    nRet = DoTask1(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    return;
                }
#endif

                string strProjectLocate = "";
                // 获得方案参数
                // strProjectNamePath	方案名，或者路径
                // return:
                //		-1	error
                //		0	not found project
                //		1	found
                nRet = this.ScriptManager.GetProjectData(
                    strProjectName,
                    out strProjectLocate);

                if (nRet == 0)
                {
                    strError = "方案 " + strProjectName + " 没有找到...";
                    this.tabControl_main.SelectedTab = this.tabPage_selectProject;
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "scriptManager.GetProjectData() error ...";
                    goto ERROR1;
                }

                // 切换到执行page
                this.tabControl_main.SelectedTab = this.tabPage_runStatis;

                this.Running = true;
                try
                {

                    nRet = RunScript(strProjectName,
                        strProjectLocate,
                        out strError);

                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.Running = false;
                }

                this.tabControl_main.SelectedTab = this.tabPage_runStatis;
                MessageBox.Show(this, "统计完成。");

                return;
            }



            if (this.tabControl_main.SelectedTab == this.tabPage_runStatis)
            {
                // 切换到...
                this.tabControl_main.SelectedTab = this.tabPage_print;

                this.button_next.Enabled = false;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得方案名
        private void button_getProjectName_Click(object sender, EventArgs e)
        {
            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = this.ScriptManager;
            dlg.ProjectName = this.textBox_projectName.Text;
            dlg.NoneProject = false;

            Program.MainForm.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_projectName.Text = dlg.ProjectName;
        }

        /// <summary>
        /// 用于输出信息的控制台(浏览器控件)
        /// </summary>
        public WebBrowser Console
        {
            get
            {
                return this.webBrowser1_running;
            }
        }

        private void button_print_Click(object sender, EventArgs e)
        {
            if (this.objStatis == null)
            {
                MessageBox.Show(this, "尚未执行统计，无法打印");
                return;
            }

            HtmlPrintForm printform = new HtmlPrintForm();

            printform.Text = "打印统计结果";
            // printform.MainForm = Program.MainForm;

            Debug.Assert(this.objStatis != null, "");
            printform.Filenames = this.objStatis.OutputFileNames;
            Program.MainForm.AppInfo.LinkFormState(printform, "printform_state");
            printform.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(printform);
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.Running == true)
                return;

            if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
                return;
            }

            this.button_next.Enabled = true;

        }

        // 
        /// <summary>
        /// 获得读者记录
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <param name="strResultTypeList">结果类型列表</param>
        /// <param name="results">返回结果字符串数组</param>
        /// <param name="strRecPath">返回读者记录路径</param>
        /// <param name="baTimestamp">返回读者记录时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到; >1: 命中多于 1 条</returns>
        public int GetReaderInfo(string strReaderBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp,
            out string strError)
        {
            long lRet = Channel.GetReaderInfo(
                stop,
                strReaderBarcode,
                strResultTypeList,
                out results,
                out strRecPath,
                out baTimestamp,
                out strError);
            return (int)lRet;
        }


        //
        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">(册条码号发生重复时)用于确认的册记录路径</param>
        /// <param name="strBiblioRecPathExclude">要排除的书目记录路径列表，用逗号间隔。除开列表中的这些书目记录路径, 才返回摘要内容, 否则仅仅返回书目记录路径</param>
        /// <param name="strBiblioRecPath">返回书目记录路径</param>
        /// <param name="strSummary">返回书目摘要</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetBiblioSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary,
            out string strError)
        {
            long lRet = Channel.GetBiblioSummary(
                stop,
                strItemBarcode,
                strConfirmItemRecPath,
                strBiblioRecPathExclude,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            return (int)lRet;
        }

        // 2012/10/6
        // 获得册记录的书目摘要
        /// <summary>
        /// 获取书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="nMaxLength">书目摘要的最大字符数。-1 表示不截断。超过这个字符数的书目摘要被截断，末尾添加"..."</param>
        /// <returns>书目摘要字符串</returns>
        public string GetItemSummary(string strItemBarcode,
            int nMaxLength = -1)
        {
            string strSummary = "";
            string strBiblioRecPath = "";
            string strError = "";
            int nRet = GetBiblioSummary(strItemBarcode,
                "",
                "",
                out strBiblioRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
                return strError;

            if (nMaxLength == -1 || strSummary.Length <= nMaxLength)
                return strSummary;

            return strSummary.Substring(0, nMaxLength) + "...";
        }

        // 2012/10/6
        // 获得读者摘要
        /// <summary>
        /// 获得读者摘要
        /// </summary>
        /// <param name="strPatronBarcode">读者证条码号</param>
        /// <returns>读者摘要</returns>
        public string GetPatronSummary(string strPatronBarcode)
        {
            string strError = "";
            string strSummary = "";

            int nRet = strPatronBarcode.IndexOf("|");
            if (nRet != -1)
                return "证条码号字符串 '" + strPatronBarcode + "' 中不应该有竖线字符";


            // 看看cache中是否已经有了
            StringCacheItem item = null;
            item = Program.MainForm.SummaryCache.SearchItem(
                "P:" + strPatronBarcode);   // 前缀是为了和册条码号区别
            if (item != null)
            {
                Application.DoEvents();
                strSummary = item.Content;
                return strSummary;
            }

            string strXml = "";
            string[] results = null;
            long lRet = Channel.GetReaderInfo(stop,
                strPatronBarcode,
                "xml",
                out results,
                out strError);
            if (lRet == -1)
            {
                strSummary = strError;
                return strSummary;
            }
            else if (lRet > 1)
            {
                strSummary = "读者证条码号 " + strPatronBarcode + " 有重复记录 " + lRet.ToString() + "条";
                return strSummary;
            }

            // 2012/10/1
            if (lRet == 0)
                return "";  // not found

            Debug.Assert(results.Length > 0, "");
            strXml = results[0];

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strSummary = "读者记录XML装入DOM时出错: " + ex.Message;
                return strSummary;
            }

            // 读者姓名
            strSummary = DomUtil.GetElementText(dom.DocumentElement,
                "name");

            // 如果cache中没有，则加入cache
            item = Program.MainForm.SummaryCache.EnsureItem(
                "P:" + strPatronBarcode);
            item.Content = strSummary;

            return strSummary;
        }

        // 
        /// <summary>
        /// 获取书目记录的局部
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strBiblioXml">书目记录 XML</param>
        /// <param name="strPartName">局部名</param>
        /// <param name="strResultValue">返回结果字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetBiblioPart(string strBiblioRecPath,
            string strBiblioXml,
            string strPartName,
            out string strResultValue,
            out string strError)
        {
            long lRet = Channel.GetBiblioInfo(
                stop,
                strBiblioRecPath,
                strBiblioXml,
                strPartName,    // 包含'@'符号
                out strResultValue,
                out strError);
            return (int)lRet;
        }

        // 本函数是不是拿给C#二次开发脚本程序用的？
        /// <summary>
        /// 获得宏值
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strMacroName">宏名</param>
        /// <returns>宏值</returns>
        public string GetMacroValue(
            string strBiblioRecPath,
            string strMacroName)
        {
            // return strMacroName + "--";
            string strError = "";
            string strResultValue = "";
            int nRet = 0;
            // 获取书目记录的局部
            nRet = GetBiblioPart(strBiblioRecPath,
                "", // strBiblioXml
                strMacroName,
                out strResultValue,
                out strError);
            if (nRet == -1)
            {
                if (String.IsNullOrEmpty(strResultValue) == true)
                    return strError;

                return strResultValue;
            }

            return strResultValue;
        }

        private void OperLogStatisForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);
        }

        private void comboBox_quickSetFilenames_SelectedIndexChanged(object sender, EventArgs e)
        {
            Delegate_QuickSetFilenames d = new Delegate_QuickSetFilenames(QuickSetFilenames);
            this.BeginInvoke(d, new object[] { sender });

        }

        delegate void Delegate_QuickSetFilenames(Control control);

        void QuickSetFilenames(Control control)
        {
            string strStartDate = "";
            string strEndDate = "";

            string strName = control.Text.Replace(" ", "").Trim();

            if (strName == "今天")
            {
                DateTime now = DateTime.Now;

                strStartDate = DateTimeUtil.DateTimeToString8(now);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                // strEndDate = DateTimeUtil.DateTimeToString8(start + new TimeSpan(7, 0,0,0));
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
                MessageBox.Show(this, "无法识别的周期 '" + strName + "'");
                return;
            }

            this.dateControl_start.Value = DateTimeUtil.Long8ToDateTime(strStartDate);
            this.dateControl_end.Value = DateTimeUtil.Long8ToDateTime(strEndDate);
        }

        // 内置统计方案 #1
        private void button_defaultProject_1_Click(object sender, EventArgs e)
        {
            this.textBox_projectName.Text = "#1";
        }


    }
}