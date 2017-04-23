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
using System.Web;   // HttpUtility

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.dp2.Statis;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 读者统计窗
    /// </summary>
    public partial class ReaderStatisForm : MyScriptForm
    {
        ReaderStatis objStatis = null;
        Assembly AssemblyMain = null;

#if NO
        public Stop Stop
        {
            get
            {
                return this.stop;
            }
        }
#endif

#if NO
        int AssemblyVersion
        {
            get
            {
                return MainForm.ReaderStatisAssemblyVersion;
            }
            set
            {
                MainForm.ReaderStatisAssemblyVersion = value;
            }
        }
#endif

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

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderStatisForm()
        {
            InitializeComponent();
        }

        private void ReaderStatisForm_Load(object sender, EventArgs e)
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
    "reader_statis_projects.xml");

#if NO
            ScriptManager.applicationInfo = Program.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                Program.MainForm.DataDir + "\\reader_statis_projects.xml";
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

            this.radioButton_inputStyle_barcodeFile.Checked = Program.MainForm.AppInfo.GetBoolean(
                "readerstatisform",
                "inputstyle_barcodefile",
                false);

            this.radioButton_inputStyle_recPathFile.Checked = Program.MainForm.AppInfo.GetBoolean(
                "readerstatisform",
                "inputstyle_recpathfile",
                false);

            this.radioButton_inputStyle_readerDatabase.Checked = Program.MainForm.AppInfo.GetBoolean(
                "readerstatisform",
                "inputstyle_readerdatabase",
                true);


            // 输入的条码号文件名
            this.textBox_inputBarcodeFilename.Text = Program.MainForm.AppInfo.GetString(
                "readerstatisform",
                "input_barcode_filename",
                "");

            // 输入的记录路径文件名
            this.textBox_inputRecPathFilename.Text = Program.MainForm.AppInfo.GetString(
                "readerstatisform",
                "input_recpath_filename",
                "");


            // 输入的读者库名
            this.comboBox_inputReaderDbName.Text = Program.MainForm.AppInfo.GetString(
                "readerstatisform",
                "input_readerdbname",
                "<全部>");

            // 方案名
            this.textBox_projectName.Text = Program.MainForm.AppInfo.GetString(
                "readerstatisform",
                "projectname",
                "");

            // 部门名称列表
            this.textBox_departmentNames.Text = Program.MainForm.AppInfo.GetString(
                 "readerstatisform",
                 "departments",
                 "*");

            // 读者类型列表
            this.textBox_readerTypes.Text = Program.MainForm.AppInfo.GetString(
                 "readerstatisform",
                 "readertypes",
                 "*");

            // 办证日期范围
            this.textBox_createTimeRange.Text = Program.MainForm.AppInfo.GetString(
                 "readerstatisform",
                 "create_timerange",
                 "");


            // 失效日期范围
            this.textBox_expireTimeRange.Text = Program.MainForm.AppInfo.GetString(
                "readerstatisform",
                "expire_timerange",
                "");

            // 如何输出表格
            this.checkBox_departmentTable.Checked = Program.MainForm.AppInfo.GetBoolean(
                "readerstatisform",
                "departmentTable",
                false);

            // SetInputPanelEnabled();

        }

        private void ReaderStatisForm_FormClosing(object sender, FormClosingEventArgs e)
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



        private void ReaderStatisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "readerstatisform",
                    "inputstyle_barcodefile",
                    this.radioButton_inputStyle_barcodeFile.Checked);

                Program.MainForm.AppInfo.SetBoolean(
                    "readerstatisform",
                    "inputstyle_recpathfile",
                    this.radioButton_inputStyle_recPathFile.Checked);

                Program.MainForm.AppInfo.SetBoolean(
                    "readerstatisform",
                    "inputstyle_readerdatabase",
                    this.radioButton_inputStyle_readerDatabase.Checked);

                // 输入的条码号文件名
                Program.MainForm.AppInfo.SetString(
                    "readerstatisform",
                    "input_barcode_filename",
                    this.textBox_inputBarcodeFilename.Text);

                // 输入的记录路径文件名
                Program.MainForm.AppInfo.SetString(
                    "readerstatisform",
                    "input_recpath_filename",
                    this.textBox_inputRecPathFilename.Text);

                // 输入的读者库名
                Program.MainForm.AppInfo.SetString(
                    "readerstatisform",
                    "input_readerdbname",
                    this.comboBox_inputReaderDbName.Text);

                // 方案名
                Program.MainForm.AppInfo.SetString(
                    "readerstatisform",
                    "projectname",
                    this.textBox_projectName.Text);

                // 部门名称列表
                Program.MainForm.AppInfo.SetString(
                     "readerstatisform",
                     "departments",
                     this.textBox_departmentNames.Text);

                // 读者类型列表
                Program.MainForm.AppInfo.SetString(
                     "readerstatisform",
                     "readertypes",
                     this.textBox_readerTypes.Text);

                // 办证日期范围
                Program.MainForm.AppInfo.SetString(
                     "readerstatisform",
                     "create_timerange",
                     this.textBox_createTimeRange.Text);

                // 失效日期范围
                Program.MainForm.AppInfo.GetString(
                    "readerstatisform",
                    "expire_timerange",
                    this.textBox_expireTimeRange.Text);

                // 如何输出表格
                Program.MainForm.AppInfo.SetBoolean(
                    "readerstatisform",
                    "departmentTable",
                    this.checkBox_departmentTable.Checked);
            }

#if NO
            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.Close();
                }
                catch
                {
                }
            }
#endif
        }

        private void button_inputCreateTimeRange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            DateTime start;
            DateTime end;

            nRet = Global.ParseTimeRangeString(this.textBox_createTimeRange.Text,
                false,
                out start,
                out end,
                out strError);
            /*
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }*/


            TimeRangeDlg dlg = new TimeRangeDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "办证日期范围";
            dlg.StartDate = start;
            dlg.EndDate = end;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            this.textBox_createTimeRange.Text = Global.MakeTimeRangeString(dlg.StartDate, dlg.EndDate);

        }

        private void button_inputExpireTimeRange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            DateTime start;
            DateTime end;

            nRet = Global.ParseTimeRangeString(this.textBox_expireTimeRange.Text,
                false,
                out start,
                out end,
                out strError);
            /*
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }*/


            TimeRangeDlg dlg = new TimeRangeDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "失效期范围";
            dlg.StartDate = start;
            dlg.EndDate = end;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            this.textBox_expireTimeRange.Text = Global.MakeTimeRangeString(dlg.StartDate, dlg.EndDate);

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

        // 创建缺省的main.cs文件
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

                sw.WriteLine("public class MyStatis : ReaderStatis");

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
            dlg.HostName = "ReaderStatisForm";
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

            this.textBox_createTimeRange.Enabled = bEnable;
            this.textBox_expireTimeRange.Enabled = bEnable;

            this.checkBox_departmentTable.Enabled = bEnable;

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

                this.objStatis = null;
                this.AssemblyMain = null;

                // 2009/11/5
                // 防止以前残留的打开的文件依然没有关闭
                Global.ForceGarbageCollection();

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out objStatis,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;

                objStatis.DepartmentNames = this.textBox_departmentNames.Text;
                objStatis.ReaderTypes = this.textBox_readerTypes.Text;

                objStatis.CreateTimeRange = this.textBox_createTimeRange.Text;
                objStatis.ExpireTimeRange = this.textBox_expireTimeRange.Text;

                // TODO: 把两个时间范围给负责解析好?

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
                nRet = DoLoop(out strError);
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

                this.AssemblyMain = null;

                EnableControls(true);
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            }
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out ReaderStatis objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~reader_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

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
                "ReaderStatisForm",
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
                "dp2Circulation.ReaderStatis");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.ReaderStatis 派生类。";
                goto ERROR1;
            }
            // new一个Statis派生对象
            objStatis = (ReaderStatis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Statis派生类设置参数
            objStatis.ReaderStatisForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 对每个读者记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            // List<string> LogFileNames = null;

            // 清除错误信息窗口中残余的内容
#if NO
            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.HtmlString = "<pre>";
                }
                catch
                {
                }
            }
#endif
            ClearErrorInfoForm();

            // 准备几个时间参数

            DateTime startCreate = new DateTime(0);
            DateTime endCreate = new DateTime(0);

            if (this.textBox_createTimeRange.Text != "")
            {
                nRet = Global.ParseTimeRangeString(this.textBox_createTimeRange.Text,
                    false,
                    out startCreate,
                    out endCreate,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            DateTime startExpire = new DateTime(0);
            DateTime endExpire = new DateTime(0);

            if (this.textBox_expireTimeRange.Text != "")
            {
                nRet = Global.ParseTimeRangeString(this.textBox_expireTimeRange.Text,
                    false,
                    out startExpire,
                    out endExpire,
                    out strError);
                if (nRet == -1)
                    return -1;
            }



            // 部门名过滤列表
            string strDepartmentList = this.textBox_departmentNames.Text;
            if (String.IsNullOrEmpty(strDepartmentList) == true)
                strDepartmentList = "*";

            string[] departments = strDepartmentList.Split(new char[] { ',' });

            StringMatchList department_matchlist = new StringMatchList(departments);

            // 读者类型过滤列表
            string strReaderTypeList = this.textBox_readerTypes.Text;
            if (String.IsNullOrEmpty(strReaderTypeList) == true)
                strReaderTypeList = "*";

            string[] readertypes = strReaderTypeList.Split(new char[] { ',' });

            StringMatchList readertype_matchlist = new StringMatchList(readertypes);

            // 记录路径临时文件
            string strTempRecPathFilename = Path.GetTempFileName();

            string strInputFileName = "";   // 外部制定的输入文件，为条码号文件或者记录路径文件格式
            string strAccessPointName = "";

            try
            {

                if (this.InputStyle == ReaderStatisInputStyle.WholeReaderDatabase)
                {
                    nRet = SearchAllReaderRecPath(strTempRecPathFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strInputFileName = strTempRecPathFilename;
                    strAccessPointName = "记录路径";
                }
                else if (this.InputStyle == ReaderStatisInputStyle.BarcodeFile)
                {
                    strInputFileName = this.textBox_inputBarcodeFilename.Text;
                    strAccessPointName = "证条码";
                }
                else if (this.InputStyle == ReaderStatisInputStyle.RecPathFile)
                {
                    strInputFileName = this.textBox_inputRecPathFilename.Text;
                    strAccessPointName = "记录路径";
                }
                else
                {
                    Debug.Assert(false, "");
                }

                StreamReader sr = null;

                // 2008/4/3
                Encoding encoding = FileUtil.DetectTextFileEncoding(strInputFileName);

                try
                {
                    sr = new StreamReader(strInputFileName, encoding);
                }
                catch (Exception ex)
                {
                    strError = "打开文件 " + strInputFileName + " 失败: " + ex.Message;
                    return -1;
                }

                this.progressBar_records.Minimum = 0;
                this.progressBar_records.Maximum = (int)sr.BaseStream.Length;
                this.progressBar_records.Value = 0;

                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取读者记录 ...");
                stop.BeginLoop();

                EnableControls(false);
                 * */

                try
                {
                    int nCount = 0;

                    for (int i = 0; ; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                DialogResult result = MessageBox.Show(this,
                                    "准备中断。\r\n\r\n确实要中断全部操作? (Yes 全部中断；No 中断循环，但是继续收尾处理；Cancel 放弃中断，继续操作)",
                                    "ReaderStatisForm",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button3);

                                if (result == DialogResult.Yes)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                                if (result == DialogResult.No)
                                    return 0;   // 假装loop正常结束

                                stop.Continue(); // 继续循环
                            }
                        }

                        // string strItemBarcode = barcodes[i];
                        string strRecPathOrBarcode = sr.ReadLine();

                        if (strRecPathOrBarcode == null)
                            break;

                        if (String.IsNullOrEmpty(strRecPathOrBarcode) == true)
                            continue;

                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个读者记录，" + strAccessPointName + "为 " + strRecPathOrBarcode);
                        this.progressBar_records.Value = (int)sr.BaseStream.Position;

                        // 获得读者记录
                        string strOutputRecPath = "";
                        byte[] baTimestamp = null;


                        string[] results = null;

                        string strAccessPoint = "";
                        if (this.InputStyle == ReaderStatisInputStyle.WholeReaderDatabase)
                            strAccessPoint = "@path:" + strRecPathOrBarcode;
                        else if (this.InputStyle == ReaderStatisInputStyle.RecPathFile)
                            strAccessPoint = "@path:" + strRecPathOrBarcode;
                        else if (this.InputStyle == ReaderStatisInputStyle.BarcodeFile)
                            strAccessPoint = strRecPathOrBarcode;
                        else
                        {
                            Debug.Assert(false, "");
                        }

                        if (StringUtil.IsInList("xml", objStatis.XmlFormat) == false
                            && StringUtil.IsInList("advancexml", objStatis.XmlFormat) == false)
                        {
                            strError = "ReaderStatis成员XmlFormat的值应至少包含xml或advancexml之中的一个";
                            return -1;
                        }

                        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                        lRet = Channel.GetReaderInfo(
                            stop,
                            strAccessPoint,
                            objStatis.XmlFormat,    // "xml",   // strResultType
                            out results,
                            out strOutputRecPath,
                            out baTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "获得读者记录 " + strAccessPoint + " 时发生错误: " + strError;
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            /*
                            Global.WriteHtml(this.webBrowser_batchAddItemPrice,
                                "检查册记录 " + strReaderBarcode + " 时出错(1): " + strError + "\r\n");
                             * */
                            continue;
                        }

                        if (lRet == 0)
                        {
                            strError = "读者" + strAccessPointName + " " + strRecPathOrBarcode + " 对应的XML数据没有找到。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet > 1)
                        {
                            strError = "读者" + strAccessPointName + " " + strRecPathOrBarcode + " 对应数据多于一条。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        string strXml = "";

                        strXml = results[0];


                        // 看看是否在希望统计的范围内
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "读者记录装入DOM发生错误: " + ex.Message;
                            continue;
                        }

                        // 按照部门名称筛选
                        if (this.textBox_departmentNames.Text != ""
                            && this.textBox_departmentNames.Text != "*")
                        {
                            // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                                "department");
                            if (department_matchlist.Match(strDepartment) == false)
                                continue;
                        }

                        // 按照读者类型筛选
                        if (this.textBox_readerTypes.Text != ""
                            && this.textBox_readerTypes.Text != "*")
                        {
                            // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                                "readerType");
                            if (readertype_matchlist.Match(strReaderType) == false)
                                continue;
                        }

                        // Debug.Assert(false, "");

                        // 按照办证日期筛选
                        if (this.textBox_createTimeRange.Text != "")
                        {
                            // 注：空字符串表示什么都满足。也就等于不使用此筛选项

                            string strCreateDate = DomUtil.GetElementText(dom.DocumentElement, "createDate");

                            if (String.IsNullOrEmpty(strCreateDate) == false)
                            {
                                try
                                {
                                    DateTime createTime = DateTimeUtil.FromRfc1123DateTimeString(strCreateDate);
                                    createTime = createTime.ToLocalTime();

                                    if (createTime >= startCreate && createTime <= endCreate)
                                    {
                                    }
                                    else
                                        continue;
                                }
                                catch
                                {
                                    strError = "<createDate>中日期字符串 '" + strCreateDate + "' 格式错误";
                                    GetErrorInfoForm().WriteHtml(HttpUtility.HtmlEncode(strError) + "\r\n");
                                }
                            }
                        }


                        // 按照失效日期筛选
                        if (this.textBox_expireTimeRange.Text != "")
                        {
                            // 注：空字符串表示什么都满足。也就等于不使用此筛选项

                            string strExpireDate = DomUtil.GetElementText(dom.DocumentElement, "expireDate");

                            if (String.IsNullOrEmpty(strExpireDate) == false)
                            {
                                try
                                {
                                    DateTime expireTime = DateTimeUtil.FromRfc1123DateTimeString(strExpireDate);
                                    expireTime = expireTime.ToLocalTime();

                                    if (expireTime >= startExpire && expireTime <= endExpire)
                                    {
                                    }
                                    else
                                        continue;
                                }
                                catch
                                {
                                    strError = "<expireDate>中日期字符串 '" + strExpireDate + "' 格式错误";
                                    GetErrorInfoForm().WriteHtml(HttpUtility.HtmlEncode(strError) + "\r\n");
                                }
                            }
                        }


                        // strXml中为日志记录

                        // 触发Script中OnRecord()代码
                        if (objStatis != null)
                        {
                            objStatis.Xml = strXml;
                            objStatis.ReaderDom = dom;
                            objStatis.CurrentRecPath = strOutputRecPath;    // 2009/10/21 changed // BUG !!! strRecPathOrBarcode;
                            objStatis.CurrentRecordIndex = i;
                            objStatis.Timestamp = baTimestamp;

                            StatisEventArgs args = new StatisEventArgs();
                            objStatis.OnRecord(this, args);
                            if (args.Continue == ContinueType.SkipAll)
                                return 1;
                        }

                        nCount++;
                    }

                    /*
                    Global.WriteHtml(this.webBrowser_batchAddItemPrice,
                        "处理结束。共增补价格字符串 " + nCount.ToString() + " 个。\r\n");
                     * */
                }
                finally
                {
                    /*
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                     * */

                    if (sr != null)
                        sr.Close();
                }
            }
            finally
            {
                File.Delete(strTempRecPathFilename);
            }


            return 0;
        }



        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 检索获得所有读者记录路径(输出到文件)
        int SearchAllReaderRecPath(string strRecPathFilename,
            out string strError)
        {
            strError = "";

            // 创建文件
            using (StreamWriter sw = new StreamWriter(strRecPathFilename,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索 ...");
                stop.BeginLoop();

                EnableControls(false);
                 * */

                try
                {
                    long lRet = Channel.SearchReader(stop,
                        this.comboBox_inputReaderDbName.Text,
                        "",
                        -1,
                        "证条码",
                        "left",
                        this.Lang,
                        null,   // strResultSetName
                        "", // strOutputStyle
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    long lHitCount = lRet;

                    long lStart = 0;
                    long lCount = lHitCount;

                    /*
                    Global.WriteHtml(this.webBrowser_resultInfo,
        "共有 " + lHitCount.ToString() + "条读者记录。\r\n");
                     * */


                    Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                        }


                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lCount,
                            "id",   // "id,cols",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        Debug.Assert(searchresults != null, "");


                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            // sw.Write(searchresults[i].Cols[0] + "\r\n");
                            // TODO: 其实可以取记录路径，用它来获取记录比用条码更快
                            sw.Write(searchresults[i].Path + "\r\n");
                        }


                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        stop.SetMessage("共有记录 " + lHitCount.ToString() + " 个。已获得记录 " + lStart.ToString() + " 个");

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }

                }
                finally
                {
                    /*
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                     * */
                }
            }
            return 0;
        ERROR1:
            return -1;
        }

        // 下一步 按钮
        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (this.radioButton_inputStyle_barcodeFile.Checked == true)
                {
                    if (this.textBox_inputBarcodeFilename.Text == "")
                    {
                        strError = "尚未指定输入的条码号文件名";
                        goto ERROR1;
                    }
                }
                else
                {
                    if (this.comboBox_inputReaderDbName.Text == "")
                    {
                        strError = "尚未指定读者库名";
                        goto ERROR1;
                    }
                }

                // 切换到过滤特性page
                this.tabControl_main.SelectedTab = this.tabPage_filter;
                return;

            }

            if (this.tabControl_main.SelectedTab == this.tabPage_filter)
            {
                /*
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
                 * */

                // 切换到执行选择方案名page
                this.tabControl_main.SelectedTab = this.tabPage_selectProject;
                return;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_selectProject)
            {
                string strProjectName = this.textBox_projectName.Text;

                if (String.IsNullOrEmpty(strProjectName) == true)
                {
                    strError = "尚未指定方案名";
                    this.textBox_projectName.Focus();
                    goto ERROR1;
                }

                string strProjectLocate = "";
                // 获得方案参数
                // strProjectNamePath	方案名，或者路径
                // return:
                //		-1	error
                //		0	not found project
                //		1	found
                int nRet = this.ScriptManager.GetProjectData(
                    strProjectName,
                    out strProjectLocate);

                if (nRet == 0)
                {
                    strError = "方案 " + strProjectName + " 没有找到...";
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

        private void radioButton_inputStyle_barcodeFile_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled();
        }

        private void radioButton_inputStyle_recPathFile_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled();
        }

        private void radioButton_inputStyle_readerDatabase_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled();
        }

        void SetInputPanelEnabled()
        {
            if (this.radioButton_inputStyle_barcodeFile.Checked == true)
            {
                this.textBox_inputBarcodeFilename.Enabled = true;
                this.button_findInputBarcodeFilename.Enabled = true;

                this.textBox_inputRecPathFilename.Enabled = false;
                this.button_findInputRecPathFilename.Enabled = false;

                this.comboBox_inputReaderDbName.Enabled = false;
            }
            else if (this.radioButton_inputStyle_recPathFile.Checked == true)
            {
                this.textBox_inputBarcodeFilename.Enabled = false;
                this.button_findInputBarcodeFilename.Enabled = false;

                this.textBox_inputRecPathFilename.Enabled = true;
                this.button_findInputRecPathFilename.Enabled = true;


                this.comboBox_inputReaderDbName.Enabled = false;
            }
            else
            {
                this.textBox_inputBarcodeFilename.Enabled = false;
                this.button_findInputBarcodeFilename.Enabled = false;

                this.textBox_inputRecPathFilename.Enabled = false;
                this.button_findInputRecPathFilename.Enabled = false;

                this.comboBox_inputReaderDbName.Enabled = true;
            }
        }

        // 输入风格
        /// <summary>
        /// 输入方式
        /// </summary>
        public ReaderStatisInputStyle InputStyle
        {
            get
            {
                if (this.radioButton_inputStyle_barcodeFile.Checked == true)
                    return ReaderStatisInputStyle.BarcodeFile;
                else if (this.radioButton_inputStyle_recPathFile.Checked == true)
                    return ReaderStatisInputStyle.RecPathFile;
                else
                    return ReaderStatisInputStyle.WholeReaderDatabase;
            }
        }

        private void button_findInputBarcodeFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的读者证条码号文件名";
            dlg.FileName = this.textBox_inputBarcodeFilename.Text;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputBarcodeFilename.Text = dlg.FileName;
        }

        private void button_findInputRecPathFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的读者记录路径文件名";
            dlg.FileName = this.textBox_inputRecPathFilename.Text;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputRecPathFilename.Text = dlg.FileName;

        }

        private void comboBox_inputReaderDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_inputReaderDbName.Items.Count > 0)
                return;

            this.comboBox_inputReaderDbName.Items.Add("<全部>");

            if (Program.MainForm.ReaderDbNames != null)    // 2009/3/29
            {
                for (int i = 0; i < Program.MainForm.ReaderDbNames.Length; i++)
                {
                    this.comboBox_inputReaderDbName.Items.Add(Program.MainForm.ReaderDbNames[i]);
                }
            }

        }

#if NO
        // 获得错误信息窗
        HtmlViewerForm GetErrorInfoForm()
        {
            if (this.ErrorInfoForm == null
                || this.ErrorInfoForm.IsDisposed == true
                || this.ErrorInfoForm.IsHandleCreated == false)
            {
                this.ErrorInfoForm = new HtmlViewerForm();
                this.ErrorInfoForm.ShowInTaskbar = false;
                this.ErrorInfoForm.Text = "错误信息";
                this.ErrorInfoForm.Show(this);
                this.ErrorInfoForm.WriteHtml("<pre>");  // 准备文本输出
            }

            return this.ErrorInfoForm;
        }
#endif

        // 是否按照单位名来输出多个表格?
        /// <summary>
        /// 是否按照单位名来输出多个表格?
        /// </summary>
        public bool OutputDepartmentTable
        {
            get
            {
                return this.checkBox_departmentTable.Checked;
            }
            set
            {
                this.checkBox_departmentTable.Checked = value;
            }
        }

        private void button_clearCreateTimeRange_Click(object sender, EventArgs e)
        {
            this.textBox_createTimeRange.Text = "";
        }

        private void button_clearExpireTimeRange_Click(object sender, EventArgs e)
        {
            this.textBox_expireTimeRange.Text = "";
        }

        // 保存读者记录
        // 被外部C#脚本调用。因本函数在循环中被调用，不需要再调用BeginLoop()
        /// <summary>
        /// 保存读者记录。
        /// </summary>
        /// <param name="strRecPath">读者记录路径</param>
        /// <param name="strAction">动作</param>
        /// <param name="strOldXml">修改前读者记录 XML</param>
        /// <param name="baOldTimestamp">修改前读者记录的时间戳</param>
        /// <param name="strNewXml">修改后的读者记录的 XML</param>
        /// <param name="baNewTimestamp">返回最新时间戳</param>
        /// <param name="strSavedPath">返回实际保存的记录路径</param>
        /// <param name="strSavedXml">返回实际保存的读者记录 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 失败; 0: 正常; 1: 部分字段被拒绝</returns>
        public int SaveReaderRecord(
            string strRecPath,
            string strAction,
            string strOldXml,
            byte[] baOldTimestamp,
            string strNewXml,
            out byte[] baNewTimestamp,
            out string strSavedPath,
            out string strSavedXml,
            out string strError)
        {
            strError = "";
            baNewTimestamp = null;
            strSavedXml = "";
            strSavedPath = "";

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者记录");
            stop.BeginLoop();

            EnableControls(false);
             * */

            try
            {
                ErrorCodeValue kernel_errorcode;

                string strExistingXml = "";

                long lRet = Channel.SetReaderInfo(
                    stop,
                    strAction,
                    strRecPath,
                    strNewXml,
                    strOldXml, // this.readerEditControl1.OldRecord,
                    baOldTimestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                }

                return (int)lRet;
            }
            finally
            {
                /*
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                 * */
            }
        }

        private void ReaderStatisForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);
        }


    }

    /// <summary>
    /// 读者统计窗输入方式
    /// </summary>
    public enum ReaderStatisInputStyle
    {
        /// <summary>
        /// 条码号文件
        /// </summary>
        BarcodeFile = 1,  // 条码号文件
        /// <summary>
        /// 记录路径文件
        /// </summary>
        RecPathFile = 2,    // 记录路径文件
        /// <summary>
        /// 全库
        /// </summary>
        WholeReaderDatabase = 3,    // 全库
    }

    // 
    /// <summary>
    /// 一个字符串模式
    /// </summary>
    public class StringMatch
    {
        /// <summary>
        /// 是否为肯定判断。如果为 true 表示“是”为命中，如果为 false 表示“否”为命中
        /// </summary>
        public bool Is = true;  // 是否为肯定判断。如果为true表示“是”为命中，如果为false表示“否”为命中
        /// <summary>
        /// Pattern
        /// </summary>
        public string Pattern = "";
        /// <summary>
        /// WildMatch
        /// </summary>
        public WildMatch WildMatch = null;
    }

    // 
    /// <summary>
    /// 字符串模式列表快速匹配器
    /// </summary>
    public class StringMatchList : List<StringMatch>
    {
        // 
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="departments">部门数组</param>
        public StringMatchList(string[] departments)
        {
            for (int i = 0; i < departments.Length; i++)
            {
                string strPattern = departments[i];
                if (String.IsNullOrEmpty(strPattern) == true)
                    continue;

                bool bIs = true;

                if (strPattern.Length >= 1
                    && strPattern[0] == '!')
                {
                    bIs = false;
                    strPattern = strPattern.Substring(1);
                }
                else
                    bIs = true;

                WildMatch wildmatch = new WildMatch(strPattern,
                    "*?[]");    // 采用DOS通配符习惯

                StringMatch match = new StringMatch();
                match.Pattern = strPattern;
                match.WildMatch = wildmatch;
                match.Is = bIs;

                this.Add(match);
            }

            MoveReverseItemForward();
        }

        // 把否定型模式向前移动
        void MoveReverseItemForward()
        {
            int j = 0;    // 移动过区域的尾部指针
            for (int i = 0; i < this.Count; i++)
            {
                StringMatch match = this[i];
                if (match.Is == false)
                {
                    if (i != 0)
                    {
                        this.RemoveAt(i);
                        this.Insert(j, match);
                    }
                    j++;
                }
            }
        }

        // 
        /// <summary>
        /// 对一个实例进行匹配
        /// </summary>
        /// <param name="strText">姚匹配的实例</param>
        /// <returns>true: 匹配; false: 不匹配</returns>
        public bool Match(string strText)
        {
            string strResult = "";
            StringMatch match = null;
            for (int i = 0; i < this.Count; i++)
            {
                match = this[i];
                int nRet = match.WildMatch.Match(strText, out strResult);
                if (match.Is == true)
                {
                    if (nRet != -1)   // match
                        return true;
                }
                else
                {
                    if (nRet != -1)   // match
                        return false;
                }
            }

            if (match.Is == false)  // 刚才查过的最后一项是否定型
                return true;

            return false;   // not match
        }
    }

    // 一个正规字符串事项
    // 用于把不正规的文字给正规化
    /// <summary>
    /// 一个正规字符串事项
    /// </summary>
    public class RegularString
    {
        /// <summary>
        /// 正规名字
        /// </summary>
        public string RegularText = ""; // 正规名字
        /// <summary>
        /// 匹配列表
        /// </summary>
        public StringMatchList match_list = null;   // 匹配列表
    }

    // 
    /// <summary>
    /// 正规字符串数组。处理一组正规字符串规则
    /// </summary>
    public class RegularStringCollection : List<RegularString>
    {
        // 构造函数。用配置文件来构造
        // parameters:
        //      strCfgFilename  配置文件名。一个纯文本文件。要求为UTF-8编码。
        //              内容格式如下
        /*
        数学系=*数学*,!*数学组*
        物理系=*物理*,!*物理组*
         * */
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strCfgFilename">配置文件名全路径</param>
        public RegularStringCollection(string strCfgFilename)
        {
            string strError = "";

            // 2008/4/3
            Encoding encoding = FileUtil.DetectTextFileEncoding(strCfgFilename);
            try
            {
                using (StreamReader sr = new StreamReader(strCfgFilename, encoding))
                {
                    for (int i = 0; ; i++)
                    {
                        string strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        strLine = strLine.Trim();

                        // 注释行
                        if (strLine.Length >= 2)
                        {
                            if (strLine[0] == '/' && strLine[1] == '/')
                                continue;
                        }

                        int nRet = strLine.IndexOf("=");
                        if (nRet == -1)
                            throw (new Exception("行 '" + strLine + "' 格式不正确，缺乏=号"));

                        string strName = strLine.Substring(0, nRet).Trim();

                        if (String.IsNullOrEmpty(strName) == true)
                            throw (new Exception("行 '" + strLine + "' 格式不正确，=号左边缺乏正规名部分"));


                        string strList = strLine.Substring(nRet + 1).Trim();

                        if (String.IsNullOrEmpty(strList) == true)
                            throw (new Exception("行 '" + strLine + "' 格式不正确，=号右边缺乏匹配列表部分"));

                        RegularString regular = new RegularString();
                        regular.RegularText = strName;
                        regular.match_list = new StringMatchList(strList.Split(new char[] { ',' }));

                        this.Add(regular);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "从文件 " + strCfgFilename + " 读取失败: " + ex.Message;
                throw new Exception(strError);
            }
        }

        // 获得一个字符串的正规形式
        // return:
        //      null    没有命中任何匹配事项
        //      其他    正规文字
        /// <summary>
        ///  获得一个字符串的正规形式
        /// </summary>
        /// <param name="strOriginText">原始文字</param>
        /// <returns>正规形式的文字。如果为 null 表示没有命中任何匹配事项</returns>
        public string GetRegularText(string strOriginText)
        {
            for (int i = 0; i < this.Count; i++)
            {
                RegularString regular = this[i];

                if (regular.match_list.Match(strOriginText) == true)
                    return regular.RegularText;
            }

            return null;
        }
    }
}