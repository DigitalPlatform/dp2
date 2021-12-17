using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using dp2Circulation.ISO2709Statis;
using DigitalPlatform;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// ISO2709 统计窗
    /// </summary>
    public partial class Iso2709StatisForm : MyScriptForm
    {
        OpenMarcFileDlg _openMarcFileDialog = null;
        // public HtmlViewerForm ErrorInfoForm = null;

        // bool Running = false;   // 正在执行运算

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        DigitalPlatform.Stop stop = null;
#endif

        // public ScriptManager ScriptManager = new ScriptManager();

        Iso2709Statis objStatis = null;
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
        public Iso2709StatisForm()
        {
            InitializeComponent();

            _openMarcFileDialog = new OpenMarcFileDlg();
            _openMarcFileDialog.IsOutput = false;
            this.tabPage_source.Padding = new Padding(4, 4, 4, 4);
            this.tabPage_source.Controls.Add(_openMarcFileDialog.MainPanel);
            _openMarcFileDialog.MainPanel.Dock = DockStyle.Fill;

        }

        private async void Iso2709StatisForm_Load(object sender, EventArgs e)
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
    "iso2709_statis_projects.xml");

#if NO
            ScriptManager.applicationInfo = Program.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                Program.MainForm.DataDir + "\\iso2709_statis_projects.xml";
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

            // 输入的ISO2709文件名
            this._openMarcFileDialog.FileName = Program.MainForm.AppInfo.GetString(
                "iso2709statisform",
                "input_iso2709_filename",
                "");

            // 编码方式
            this._openMarcFileDialog.EncodingName = Program.MainForm.AppInfo.GetString(
    "iso2709statisform",
    "input_iso2709_file_encoding",
    "");

            this._openMarcFileDialog.MarcSyntax = Program.MainForm.AppInfo.GetString(
    "iso2709statisform",
    "input_marc_syntax",
    "unimarc");

            this._openMarcFileDialog.Mode880 = Program.MainForm.AppInfo.GetBoolean(
    "iso2709statisform",
    "input_mode880",
    false);

            // 方案名
            this.comboBox_projectName.Text = Program.MainForm.AppInfo.GetString(
                "iso2709statisform",
                "projectname",
                "");

            /*
            var ret = await Program.MainForm.EnsureConnectLibraryServerAsync();
            if (ret == false)
            {
                MessageBox.Show(this, "连接到 dp2library 失败，ISO2709 统计窗无法打开");
                this.Close();
            }
            */
        }

        private void Iso2709StatisForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Iso2709StatisForm_FormClosed(object sender, FormClosedEventArgs e)
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
                // 输入的ISO2709文件名
                Program.MainForm.AppInfo.SetString(
                    "iso2709statisform",
                    "input_iso2709_filename",
                    this._openMarcFileDialog.FileName);

                // 编码方式
                Program.MainForm.AppInfo.SetString(
        "iso2709statisform",
        "input_iso2709_file_encoding",
        this._openMarcFileDialog.EncodingName);

                Program.MainForm.AppInfo.SetString(
    "iso2709statisform",
    "input_marc_syntax",
    this._openMarcFileDialog.MarcSyntax);

                Program.MainForm.AppInfo.SetBoolean(
    "iso2709statisform",
    "input_mode880",
    this._openMarcFileDialog.Mode880);

                // 方案名
                Program.MainForm.AppInfo.SetString(
                    "iso2709statisform",
                    "projectname",
                    this.comboBox_projectName.Text);
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

        // 创建缺省的 main.cs 文件
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

                sw.WriteLine("using dp2Circulation;");

                sw.WriteLine("using DigitalPlatform.Xml;");
                sw.WriteLine("using DigitalPlatform.Marc;");

                sw.WriteLine("public class MyStatis : Iso2709Statis");

                sw.WriteLine("{");

                sw.WriteLine("	public override void OnBegin(object sender, StatisEventArgs e)");
                sw.WriteLine("	{");
                sw.WriteLine("	}");

                sw.WriteLine("}");
            }
        }

        private void button_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "Iso2709StatisForm";
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
            this._openMarcFileDialog.MainPanel.Enabled = bEnable;
            // this.button_findInputIso2709Filename.Enabled = bEnable;

            this.button_getProjectName.Enabled = bEnable;

            this.button_next.Enabled = bEnable;

            this.button_projectManage.Enabled = bEnable;
        }

        public override int RunScript(string strProjectName,
    string strProjectLocate,
    string strInitialParamString,
    out string strError,
    out string strWarning)
        {
            strWarning = "";

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

                if (strProjectName == "#将dt1000书目MARC转换为dp2的bdf格式")
                {
                    objStatis = new ConvertDt1000ToBdf();
                }
                else if (strProjectName == "#将dt1000读者MARC转换为dp2的XML格式")
                {
                    objStatis = new ConvertDt1000ReaderToXml();
                }
                else
                {
                    nRet = PrepareScript(strProjectName,
                        strProjectLocate,
                        out objStatis,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                if (strInitialParamString == "test_compile")
                    return 0;

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;
                objStatis.InputFilename = this._openMarcFileDialog.FileName;

                // 执行脚本的OnInitial()

                // 触发Script中OnInitial()代码
                // OnInitial()和OnBegin的本质区别, 在于OnInitial()适合检查和设置面板参数
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnInitial(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        goto END1;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
                }

                // 触发Script中OnBegin()代码
                // OnBegin()中仍然有修改MainForm面板的自由
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnBegin(this, args);
                    if (args.Continue == ContinueType.SkipAll)
                        goto END1;
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
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
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        goto ERROR1;
                    }
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
            out Iso2709Statis objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~iso2709_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

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
									Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "Iso2709StatisForm",
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

            // 得到Assembly中Iso2709Statis派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.Iso2709Statis");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.Iso2709Statis 派生类。";
                goto ERROR1;
            }
            // new一个Iso2709Statis派生对象
            objStatis = (Iso2709Statis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Iso2709Statis派生类设置参数
            objStatis.Iso2709StatisForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 对每个Iso2709Statis记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(out string strError)
        {
            strError = "";
            // int nRet = 0;
            // long lRet = 0;
            Encoding encoding = null;

            if (string.IsNullOrEmpty(this._openMarcFileDialog.EncodingName) == true)
            {
                strError = "尚未选定 ISO2709 文件的编码方式";
                return -1;
            }

            if (StringUtil.IsNumber(this._openMarcFileDialog.EncodingName) == true)
                encoding = Encoding.GetEncoding(Convert.ToInt32(this._openMarcFileDialog.EncodingName));
            else
                encoding = Encoding.GetEncoding(this._openMarcFileDialog.EncodingName);

#if NO
            // 清除错误信息窗口中残余的内容
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

            string strInputFileName = "";

            try
            {
                strInputFileName = this._openMarcFileDialog.FileName;

                Stream file = null;

                try
                {
                    file = File.Open(strInputFileName,
                        FileMode.Open,
                        FileAccess.Read);
                }
                catch (Exception ex)
                {
                    strError = "打开文件 " + strInputFileName + " 失败: " + ex.Message;
                    return -1;
                }

                this.progressBar_records.Minimum = 0;
                this.progressBar_records.Maximum = (int)file.Length;
                this.progressBar_records.Value = 0;

                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取ISO2709记录 ...");
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
                                    "Iso2709StatisForm",
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

                        // 从ISO2709文件中读入一条MARC记录
                        // return:
                        //	-2	MARC格式错
                        //	-1	出错
                        //	0	正确
                        //	1	结束(当前返回的记录有效)
                        //	2	结束(当前返回的记录无效)
                        int nRet = MarcUtil.ReadMarcRecord(file,
                            encoding,
                            true,	// bRemoveEndCrLf,
                            true,	// bForce,
                            out string strMARC,
                            out strError);
                        if (nRet == -2 || nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
                                "读入MARC记录(" + nCount.ToString() + ")出错: " + strError + "\r\n\r\n确实要中断当前批处理操作?",
                                "Iso2709StatisForm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                            {
                                break;
                            }
                            else
                            {
                                strError = "读入MARC记录(" + nCount.ToString() + ")出错: " + strError;
                                GetErrorInfoForm().WriteHtml(strError + "\r\n");
                                continue;
                            }
                        }

                        if (nRet != 0 && nRet != 1)
                            return 0;	// 结束

                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个 ISO2709 记录");
                        this.progressBar_records.Value = (int)file.Position;

                        // 跳过太短的记录
                        if (string.IsNullOrEmpty(strMARC) == true
                            || strMARC.Length <= 24)
                            continue;

                        if (this._openMarcFileDialog.Mode880 == true
                            && (this._openMarcFileDialog.MarcSyntax == "usmarc" || this._openMarcFileDialog.MarcSyntax == "<自动>"))
                        {
                            MarcRecord temp = new MarcRecord(strMARC);
                            MarcQuery.ToParallel(temp);
                            strMARC = temp.Text;
                        }

                        // 触发Script中OnRecord()代码
                        if (objStatis != null)
                        {
                            objStatis.MARC = strMARC;
                            objStatis.CurrentRecordIndex = i;
                            objStatis.Syntax = this._openMarcFileDialog.MarcSyntax;

                            StatisEventArgs args = new StatisEventArgs();
                            objStatis.OnRecord(this, args);
                            if (args.Continue == ContinueType.SkipAll)
                                return 1;
                            if (args.Continue == ContinueType.Error)
                            {
                                strError = args.ParamString;
                                return -1;
                            }
                        }

                        nCount++;
                    }

                    /*
                    Global.WriteHtml(this.webBrowser_batchAddItemPrice,
                        "处理结束。共增补价格字符串 " + nCount.ToString() + " 个。\r\n");
                     * */

                    return 0;
                }
                finally
                {
                    /*
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                     * */

                    if (file != null)
                        file.Close();
                }
            }
            finally
            {
            }

            // return 0;
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (string.IsNullOrEmpty(this._openMarcFileDialog.FileName) == true)
                {
                    strError = "尚未指定输入的ISO2709文件名";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this._openMarcFileDialog.EncodingName) == true)
                {
                    strError = "尚未选定 ISO2709 文件的编码方式";
                    goto ERROR1;
                }

                this.tabControl_main.SelectedTab = this.tabPage_selectProject;
                return;
            }


            if (this.tabControl_main.SelectedTab == this.tabPage_selectProject)
            {
                string strProjectName = this.comboBox_projectName.Text;

                if (String.IsNullOrEmpty(strProjectName) == true)
                {
                    strError = "尚未指定方案名";
                    this.comboBox_projectName.Focus();
                    goto ERROR1;
                }

                int nRet = 0;

                string strProjectLocate = "";

                if (strProjectName.StartsWith("#") == false)
                {
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
                        goto ERROR1;
                    }
                    if (nRet == -1)
                    {
                        strError = "scriptManager.GetProjectData() error ...";
                        goto ERROR1;
                    }
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

        private void button_getProjectName_Click(object sender, EventArgs e)
        {
            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = this.ScriptManager;
            dlg.ProjectName = this.comboBox_projectName.Text;
            dlg.NoneProject = false;

            Program.MainForm.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.comboBox_projectName.Text = dlg.ProjectName;
        }

#if NO
        private void button_findInputIso2709Filename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的ISO2709文件名";
            dlg.FileName = this.textBox_inputIso2709Filename.Text;
            dlg.Filter = "ISO2709文件 (*.iso)|*.iso|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputIso2709Filename.Text = dlg.FileName;

        }
#endif

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

        private void Iso2709StatisForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

        }

        // 
        /// <summary>
        /// 修改书目信息。
        /// 请参考 dp2Library API SetBiblioInfo() 的详细说明
        /// </summary>
        /// <param name="strAction">动作</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strBiblioType">书目类型</param>
        /// <param name="strBiblio">书目记录</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="strOutputBiblioRecPath">返回实际写入的书目记录路径</param>
        /// <param name="baNewTimestamp">返回最新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SetBiblioInfo(string strAction,
                string strBiblioRecPath,
                string strBiblioType,
                string strBiblio,
                byte[] timestamp,
                out string strOutputBiblioRecPath,
                out byte[] baNewTimestamp,
                out string strError)
        {
            long lRet = Channel.SetBiblioInfo(
                stop,
                strAction,
                strBiblioRecPath,
                strBiblioType,
                strBiblio,
                timestamp,
                "",
                out strOutputBiblioRecPath,
                out baNewTimestamp,
                out strError);
            return (int)lRet;
        }

        // 
        /// <summary>
        /// 创建一个实体记录
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strItemXml">实体记录 XML</param>
        /// <param name="strStyle">创建风格。"force"表示强制写入</param>
        /// <param name="strNewItemRecPath">返回实际写入的册记录路径</param>
        /// <param name="strNewXml">返回实际写入的实体记录 XML</param>
        /// <param name="baNewTimestamp">返回实体记录的最新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int CreateItemInfo(
            string strBiblioRecPath,
            string strItemXml,
            string strStyle,
            out string strNewItemRecPath,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";

            strNewItemRecPath = "";
            strNewXml = "";
            baNewTimestamp = null;

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = Global.GetRecordID(strBiblioRecPath);

            XmlDocument item_dom = new XmlDocument();
            try
            {
                item_dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(item_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "new";
            info.NewRecPath = "";
            info.NewRecord = item_dom.OuterXml;
            info.NewTimestamp = null;
            info.Style = strStyle;

            // 
            EntityInfo[] entities = new EntityInfo[1];
            entities[0] = info;

            EntityInfo[] errorinfos = null;

            long lRet = Channel.SetEntities(
                stop,
                strBiblioRecPath,
                entities,
                out errorinfos,
                out strError);
            if (lRet == -1)
                return -1;

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        strNewItemRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        // 
        /// <summary>
        /// 创建一个订购记录
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strOrderXml">订购记录 XML</param>
        /// <param name="strStyle">创建风格。"force"表示强制写入</param>
        /// <param name="strNewOrderRecPath">返回实际写入的订购记录路径</param>
        /// <param name="strNewXml">返回实际写入的订购记录 XML</param>
        /// <param name="baNewTimestamp">返回订购记录的最新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int CreateOrderInfo(
            string strBiblioRecPath,
            string strOrderXml,
            string strStyle,
            out string strNewOrderRecPath,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";

            strNewOrderRecPath = "";
            strNewXml = "";
            baNewTimestamp = null;

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = Global.GetRecordID(strBiblioRecPath);

            XmlDocument item_dom = new XmlDocument();
            try
            {
                item_dom.LoadXml(strOrderXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(item_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "new";
            info.NewRecPath = "";
            info.NewRecord = item_dom.OuterXml;
            info.NewTimestamp = null;
            info.Style = strStyle;

            // 
            EntityInfo[] orders = new EntityInfo[1];
            orders[0] = info;

            EntityInfo[] errorinfos = null;

            long lRet = Channel.SetOrders(
                stop,
                strBiblioRecPath,
                orders,
                out errorinfos,
                out strError);
            if (lRet == -1)
                return -1;

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        strNewOrderRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

    }
}