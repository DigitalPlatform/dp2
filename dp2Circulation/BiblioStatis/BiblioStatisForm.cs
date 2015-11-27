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
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;

using DigitalPlatform.dp2.Statis;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 书目统计窗
    /// </summary>
    public partial class BiblioStatisForm : MyScriptForm
    {
        /// <summary>
        /// 获取批次号key+count值列表
        /// </summary>
        public event GetKeyCountListEventHandler GetBatchNoTable = null;

        // public HtmlViewerForm ErrorInfoForm = null;

        // bool Running = false;   // 正在执行运算

        /*
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
         * */

        // public ScriptManager ScriptManager = new ScriptManager();

        BiblioStatis objStatis = null;
        Assembly AssemblyMain = null;

        Assembly AssemblyFilter = null;
        MyFilterDocument MarcFilter = null;


        void DisposeBilbioStatisObject()
        {
            BiblioStatis temp = this.objStatis;
            this.objStatis = null;
            if (temp != null)
            {
                temp.Dispose();
                GC.WaitForPendingFinalizers();
            }
        }

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
        public BiblioStatisForm()
        {
            InitializeComponent();
        }

        private void BiblioStatisForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            /*
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
             * */

            ScriptManager.CfgFilePath =
                Path.Combine(this.MainForm.DataDir, "biblio_statis_projects.xml");

#if NO
            ScriptManager.applicationInfo = this.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                this.MainForm.DataDir + "\\biblio_statis_projects.xml";
            ScriptManager.DataDir = this.MainForm.DataDir;

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

            // batchno
            this.GetBatchNoTable -= new GetKeyCountListEventHandler(BiblioStatisForm_GetBatchNoTable);
            this.GetBatchNoTable += new GetKeyCountListEventHandler(BiblioStatisForm_GetBatchNoTable);

            this.radioButton_inputStyle_recPathFile.Checked = this.MainForm.AppInfo.GetBoolean(
                "bibliostatisform",
                "inputstyle_recpathfile",
                false);


            this.radioButton_inputStyle_biblioDatabase.Checked = this.MainForm.AppInfo.GetBoolean(
                "bibliostatisform",
                "inputstyle_bibliodatabase",
                true);

            this.radioButton_inputStyle_recPaths.Checked = this.MainForm.AppInfo.GetBoolean(
    "bibliostatisform",
    "inputstyle_recpaths",
    false);


            // 输入的记录路径文件名
            this.textBox_inputRecPathFilename.Text = this.MainForm.AppInfo.GetString(
                "bibliostatisform",
                "input_recpath_filename",
                "");


            // 输入的书目库名
            this.comboBox_inputBiblioDbName.Text = this.MainForm.AppInfo.GetString(
                "bibliostatisform",
                "input_bibliodbname",
                "<全部>");

            // 方案名
            this.textBox_projectName.Text = this.MainForm.AppInfo.GetString(
                "bibliostatisform",
                "projectname",
                "");


            // 记录路径
            this.textBox_inputStyle_recPaths.Text = this.MainForm.AppInfo.GetString(
                "bibliostatisform",
                "recpaths",
                "").Replace(",","\r\n");

        }

        void BiblioStatisForm_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                "", // 不分图书和期刊
                "biblio",
                this.stop,
                this.Channel);

#if NOOOOOOOOOOOOOOOOOOOOOOOOOO
            string strError = "";

            if (e.KeyCounts == null)
                e.KeyCounts = new List<KeyCount>();

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在列出全部编目批次号 ...");
            stop.BeginLoop();

            try
            {
                string strQueryXml = "";

                long lRet = Channel.SearchBiblio(
                    stop,
                    "<all>",    // 尽管可以用 this.comboBox_inputBiblioDbName.Text, 以便获得和少数书目库相关的批次号实例，但是容易造成误会：因为数据库名列表刷新后，这里却不会刷新？
                    "", // strBatchNo,
                    2000,   // -1,    // nPerMax
                    "batchno",
                    "left",
                    this.Lang,
                    "batchno",   // strResultSetName
                    "keycount", // strOutputStyle
                    out strQueryXml,
                    out strError);
                /*
                long lRet = Channel.SearchItem(
                    stop,
                    "<all>",
                    "", // strBatchNo
                    -1,
                    "批次号",
                    "left",
                    this.Lang,
                    "batchno",   // strResultSetName
                    "keycount", // strOutputStyle
                    out strError);
                 * */
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "没有找到任何编目批次号检索点";
                    return;
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                SearchResult[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "keycount",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        // MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (searchresults[i].Cols == null)
                        {
                            strError = "请更新应用服务器和数据库内核到最新版本";
                            goto ERROR1;
                        }

                        KeyCount keycount = new KeyCount();
                        keycount.Key = searchresults[i].Path;
                        keycount.Count = Convert.ToInt32(searchresults[i].Cols[0]);
                        e.KeyCounts.Add(keycount);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void BiblioStatisForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
             * */

        }

        private void BiblioStatisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
                    "bibliostatisform",
                    "inputstyle_recpathfile",
                    this.radioButton_inputStyle_recPathFile.Checked);


                this.MainForm.AppInfo.SetBoolean(
                    "bibliostatisform",
                    "inputstyle_bibliodatabase",
                    this.radioButton_inputStyle_biblioDatabase.Checked);

                this.MainForm.AppInfo.SetBoolean(
    "bibliostatisform",
    "inputstyle_recpaths",
    this.radioButton_inputStyle_recPaths.Checked);

                // 输入的记录路径文件名
                this.MainForm.AppInfo.SetString(
                    "bibliostatisform",
                    "input_recpath_filename",
                    this.textBox_inputRecPathFilename.Text);

                // 输入的书目库名
                this.MainForm.AppInfo.SetString(
                    "bibliostatisform",
                    "input_bibliodbname",
                    this.comboBox_inputBiblioDbName.Text);

                // 方案名
                this.MainForm.AppInfo.SetString(
                    "bibliostatisform",
                    "projectname",
                    this.textBox_projectName.Text);

                // 记录路径
                this.MainForm.AppInfo.SetString(
                    "bibliostatisform",
                    "recpaths",
                    this.textBox_inputStyle_recPaths.Text.Replace("\r\n", ","));
            }
        }

        internal override void CreateDefaultContent(CreateDefaultContentEventArgs e)
        {
            string strPureFileName = Path.GetFileName(e.FileName);

            if (String.Compare(strPureFileName, "main.cs", true) == 0)
            {
                CreateDefaultMainCsFile(e.FileName);
                e.Created = true;
            }
            else if (String.Compare(strPureFileName, "marcfilter.fltx", true) == 0)
            {
                CreateDefaultMarcFilterFile(e.FileName);
                e.Created = true;
            }
            else
            {
                e.Created = false;
            }
        }

#if NO
        private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
        {
            string strPureFileName = Path.GetFileName(e.FileName);

            if (String.Compare(strPureFileName, "main.cs", true) == 0)
            {
                CreateDefaultMainCsFile(e.FileName);
                e.Created = true;
            }
            else if (String.Compare(strPureFileName, "marcfilter.fltx", true) == 0)
            {
                CreateDefaultMarcFilterFile(e.FileName);
                e.Created = true;
            }
            else
            {
                e.Created = false;
            }

        }
#endif

        // 创建缺省的main.cs文件
        static void CreateDefaultMainCsFile(string strFileName)
        {
            StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);
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


            sw.WriteLine("public class MyStatis : BiblioStatis");

            sw.WriteLine("{");

            sw.WriteLine("	public override void OnBegin(object sender, StatisEventArgs e)");
            sw.WriteLine("	{");
            sw.WriteLine("	}");

            sw.WriteLine("}");
            sw.Close();
        }

        // 创建缺省的marcfilter.fltx文件
        static void CreateDefaultMarcFilterFile(string strFileName)
        {
            StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);

            sw.WriteLine("<?xml version='1.0' encoding='utf-8'?>");
            sw.WriteLine("<filter>");
            sw.WriteLine("<using>");
            sw.WriteLine("<![CDATA[");
            sw.WriteLine("using System;");
            sw.WriteLine("using System.IO;");
            sw.WriteLine("using System.Text;");
            sw.WriteLine("using System.Windows.Forms;");
            sw.WriteLine("using DigitalPlatform.MarcDom;");
            sw.WriteLine("using DigitalPlatform.Marc;");

            sw.WriteLine("using dp2Circulation;");

            sw.WriteLine("]]>");
            sw.WriteLine("</using>");
            sw.WriteLine("	<record>");
            sw.WriteLine("		<def>");
            sw.WriteLine("		<![CDATA[");
            sw.WriteLine("			int i;");
            sw.WriteLine("			int j;");
            sw.WriteLine("		]]>");
            sw.WriteLine("		</def>");
            sw.WriteLine("		<begin>");
            sw.WriteLine("		<![CDATA[");
            sw.WriteLine("			MessageBox.Show(\"record data:\" + this.Data);");
            sw.WriteLine("		]]>");
            sw.WriteLine("		</begin>");
            sw.WriteLine("			 <field name=\"200\">");
            sw.WriteLine("");
            sw.WriteLine("			 </field>");
            sw.WriteLine("		<end>");
            sw.WriteLine("		<![CDATA[");
            sw.WriteLine("");
            sw.WriteLine("			j ++;");
            sw.WriteLine("		]]>");
            sw.WriteLine("		</end>");
            sw.WriteLine("	</record>");
            sw.WriteLine("</filter>");

            sw.Close();
        }

        private void button_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "BiblioStatisForm";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = this.MainForm.AppInfo;
            dlg.DataDir = this.MainForm.DataDir;
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

            // this.checkBox_departmentTable.Enabled = bEnable;

            this.button_next.Enabled = bEnable;

            this.button_projectManage.Enabled = bEnable;
        }

        /*
发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 dp2Circulation.BiblioStatisForm.RunScript(String strProjectName, String strProjectLocate, String strInitialParamString, String& strError, String& strWarning)
在 dp2Circulation.BiblioStatisForm.button_next_Click(Object sender, EventArgs e)
在 System.Windows.Forms.Button.OnMouseUp(MouseEventArgs mevent)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ButtonBase.WndProc(Message& m)
在 System.Windows.Forms.Button.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

         * */
        int RunScript(string strProjectName,
            string strProjectLocate,
            string strInitialParamString,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在执行脚本 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            _dllPaths.Clear();
            _dllPaths.Add(strProjectLocate);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            try
            {
                int nRet = 0;
                strError = "";
                strWarning = "";

                this.DisposeBilbioStatisObject();
                // this.objStatis = null;

                this.AssemblyMain = null;
                MyFilterDocument filter = null;

                // 2009/11/5 
                // 防止以前残留的打开的文件依然没有关闭
                Global.ForceGarbageCollection();

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out this.objStatis,
                    out filter,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                //
                if (filter != null)
                    this.AssemblyFilter = filter.Assembly;
                else
                    this.AssemblyFilter = null;

                this.MarcFilter = filter;
                //

                Debug.Assert(objStatis != null, "");

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;

                // 执行脚本的OnInitial()

                // 触发Script中OnInitial()代码
                // OnInitial()和OnBegin的本质区别, 在于OnInitial()适合检查和设置面板参数
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    args.ParamString = strInitialParamString;
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
                nRet = DoLoop(out strError,
                    out strWarning);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    goto END1;  // 实际上 SkipAll 是要执行 OnEnd() 的，而 Error 才是不执行 OnEnd()
            END1:
                // 触发Script的OnEnd()代码
                if (objStatis != null)
                {
                    StatisEventArgs args = new StatisEventArgs();
                    objStatis.OnEnd(this, args);
                    if (args.Continue == ContinueType.Error)
                    {
                        strError = args.ParamString;
                        return -1;
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
            out BiblioStatis objStatisParam,
            out MyFilterDocument filter,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatisParam = null;
            filter = null;

            string strWarning = "";

            /*
            string strInstanceDir = PathUtil.MergePath(strProjectLocate, "~bin_" + Guid.NewGuid().ToString());
            PathUtil.CreateDirIfNeed(strInstanceDir);
             * */

            string strMainCsDllName = Path.Combine(this.InstanceDir, "~biblio_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

            string strLibPaths = "\"" + this.MainForm.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.Client.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\digitalplatform.statis.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "BiblioStatisForm",
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
                "dp2Circulation.BiblioStatis");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.BiblioStatis 派生类。";
                goto ERROR1;
            }
            // new一个Statis派生对象
            objStatisParam = (BiblioStatis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Statis派生类设置参数
            objStatisParam.BiblioStatisForm = this;
            objStatisParam.ProjectDir = strProjectLocate;
            objStatisParam.InstanceDir = this.InstanceDir;

            ////

            ////////////////////////////
            // 装载marfilter.fltx
            string strFilterFileName = Path.Combine(strProjectLocate, "marcfilter.fltx");

            if (FileUtil.FileExist(strFilterFileName) == true)
            {
                filter = new MyFilterDocument();
                filter.BiblioStatis = objStatisParam;
                filter.strOtherDef = entryClassType.FullName + " BiblioStatis = null;";

                filter.strPreInitial = " MyFilterDocument doc = (MyFilterDocument)this.Document;\r\n";
                filter.strPreInitial += " BiblioStatis = ("
                    + entryClassType.FullName + ")doc.BiblioStatis;\r\n";

                try
                {
                    filter.Load(strFilterFileName);
                }
                catch (Exception ex)
                {
                    strError = "文件 " + strFilterFileName + " 装载到MarcFilter时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                nRet = filter.BuildScriptFile(Path.Combine(strProjectLocate, "marcfilter.fltx.cs"),
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 一些必要的链接库
                string[] saAddRef1 = {
										 Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.library.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
										 // Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
										 Environment.CurrentDirectory + "\\dp2circulation.exe",
										 strMainCsDllName};

                // fltx文件里显式增补的链接库
                string [] saAdditionalRef = filter.GetRefs();

                // 合并的链接库
                string [] saTotalFilterRef = new string[saAddRef1.Length + saAdditionalRef.Length];
                Array.Copy(saAddRef1, saTotalFilterRef, saAddRef1.Length);
                Array.Copy(saAdditionalRef, 0,
                    saTotalFilterRef, saAddRef1.Length,
                    saAdditionalRef.Length);

                string strfilterCsDllName = Path.Combine(strProjectLocate, "~marcfilter_" + Convert.ToString(AssemblyVersion++) + ".dll");

                // 创建Project中Script的Assembly
                nRet = ScriptManager.BuildAssembly(
                    "BiblioStatisForm",
                    strProjectName,
                    "marcfilter.fltx.cs",
                    saTotalFilterRef,
                    strLibPaths,
                    strfilterCsDllName,
                    out strError,
                    out strWarning);
                if (nRet == -2)
                    goto ERROR1;
                if (nRet == -1)
                {
                    if (strWarning == "")
                    {
                        goto ERROR1;
                    }
                    MessageBox.Show(this, strWarning);
                }

                Assembly assemblyFilter = null;

                assemblyFilter = Assembly.LoadFrom(strfilterCsDllName);
                if (assemblyFilter == null)
                {
                    strError = "LoadFrom " + strfilterCsDllName + "fail";
                    goto ERROR1;
                }

                filter.Assembly = assemblyFilter;
            }
            return 0;
        ERROR1:
            return -1;
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 对每个书目记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;
            long lRet = 0;

            bool bSyntaxWarned = false;
            bool bFilterWarned = false;

            // List<string> LogFileNames = null;

            // 清除错误信息窗口中残余的内容
            ClearErrorInfoForm();

            /*
            // 馆藏地点过滤列表
            string strLocationList = this.textBox_locationNames.Text;
            if (String.IsNullOrEmpty(strLocationList) == true)
                strLocationList = "*";

            string[] locations = strLocationList.Split(new char[] { ',' });

            StringMatchList location_matchlist = new StringMatchList(locations);

            // 读者类型过滤列表
            string strItemTypeList = this.textBox_itemTypes.Text;
            if (String.IsNullOrEmpty(strItemTypeList) == true)
                strItemTypeList = "*";

            string[] itemtypes = strItemTypeList.Split(new char[] { ',' });

            StringMatchList itemtype_matchlist = new StringMatchList(itemtypes);
             * */

            // 记录路径临时文件
            string strTempRecPathFilename = Path.GetTempFileName();

            string strInputFileName = "";   // 外部制定的输入文件，为条码号文件或者记录路径文件格式
            string strAccessPointName = "";

            try
            {

                if (this.InputStyle == BiblioStatisInputStyle.BatchNo)
                {
                    nRet = SearchBiblioRecPath(
                        this.tabComboBox_inputBatchNo.Text,
                        strTempRecPathFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strInputFileName = strTempRecPathFilename;
                    strAccessPointName = "记录路径";
                }
                else if (this.InputStyle == BiblioStatisInputStyle.RecPathFile)
                {
                    strInputFileName = this.textBox_inputRecPathFilename.Text;
                    strAccessPointName = "记录路径";
                }
                else if (this.InputStyle == BiblioStatisInputStyle.RecPaths)
                {
                    using (StreamWriter sw = new StreamWriter(strTempRecPathFilename, false, Encoding.UTF8))
                    {
                        sw.Write(this.textBox_inputStyle_recPaths.Text);
                    }

                    strInputFileName = strTempRecPathFilename;
                    strAccessPointName = "记录路径";
                }
                else
                {
                    Debug.Assert(false, "");
                }

                StreamReader sr = null;

                try
                {
                    sr = new StreamReader(strInputFileName, Encoding.UTF8);
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
                stop.Initial("正在获取书目记录 ...");
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
                                    "bibliostatisform",
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
                        string strRecPath = sr.ReadLine();

                        if (strRecPath == null)
                            break;

                        strRecPath = strRecPath.Trim();
                        nRet = strRecPath.IndexOf("\t");
                        if (nRet != -1)
                            strRecPath = strRecPath.Substring(0, nRet).Trim();

                        if (String.IsNullOrEmpty(strRecPath) == true)
                            continue;

                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个书目记录，" + strAccessPointName + "为 " + strRecPath);
                        this.progressBar_records.Value = (int)sr.BaseStream.Position;

                        // 获得书目记录
                        // string strOutputRecPath = "";
                        // byte[] baTimestamp = null;


                        string strAccessPoint = "";
                        if (this.InputStyle == BiblioStatisInputStyle.BatchNo)
                            strAccessPoint = strRecPath;
                        else if (this.InputStyle == BiblioStatisInputStyle.RecPathFile)
                            strAccessPoint = strRecPath;
                        else if (this.InputStyle == BiblioStatisInputStyle.RecPaths)
                            strAccessPoint = strRecPath;
                        else
                        {
                            Debug.Assert(false, "");
                        }

                        string strBiblio = "";
                        // string strBiblioRecPath = "";

#if NO
                        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                        lRet = Channel.GetBiblioInfo(
                            stop,
                            strAccessPoint,
                            "", // strBiblioXml
                            "xml",   // strResultType
                            out strBiblio,
                            out strError);
#endif
                        string[] formats = new string[1];
                        formats[0] = "xml";
                        string[] results = null;
                        byte[] baTimestamp = null;
                        lRet = Channel.GetBiblioInfos(
                            stop,
                            strAccessPoint,
                    "",
                            formats,
                            out results,
                            out baTimestamp,
                            out strError);

                        if (lRet == -1)
                        {
                            strError = "获得书目记录 " + strAccessPoint + " 时发生错误: " + strError;
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet == 0)
                        {
                            strError = "书目记录" + strAccessPointName + " " + strRecPath + " 对应的XML数据没有找到。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet > 1)
                        {
                            strError = "书目记录" + strAccessPointName + " " + strRecPath + " 对应数据多于一条。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (results == null || results.Length == 0)
                        {
                            strError = "书目记录" + strAccessPointName + " " + strRecPath + " 获取时 results 出错。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }
                        strBiblio = results[0];
                        objStatis.Timestamp = baTimestamp;


                        string strXml = "";

                        strXml = strBiblio;


                        // 看看是否在希望统计的范围内
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "书目记录装入DOM发生错误: " + ex.Message;
                            continue;
                        }

                        /*
                        // 按照馆藏地点筛选
                        if (this.textBox_locationNames.Text != ""
                            && this.textBox_locationNames.Text != "*")
                        {
                            // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                                "location");
                            if (location_matchlist.Match(strLocation) == false)
                                continue;
                        }

                        // 按照册类型筛选
                        if (this.textBox_itemTypes.Text != ""
                            && this.textBox_itemTypes.Text != "*")
                        {
                            // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                            string strItemType = DomUtil.GetElementText(dom.DocumentElement,
                                "bookType");
                            if (itemtype_matchlist.Match(strItemType) == false)
                                continue;
                        }
                         * */

                        // Debug.Assert(false, "");

                        // strXml中为书目记录
                        string strBiblioDbName = Global.GetDbName(strRecPath);

                        string strSyntax = this.MainForm.GetBiblioSyntax(strBiblioDbName);
                        if (String.IsNullOrEmpty(strSyntax) == true)
                            strSyntax = "unimarc";

                        bool bItemDomsCleared = false;

                        if (strSyntax == "usmarc" || strSyntax == "unimarc")
                        {
                            // 将XML书目记录转换为MARC格式
                            string strOutMarcSyntax = "";
                            string strMarc = "";

                            // 将MARCXML格式的xml记录转换为marc机内格式字符串
                            // parameters:
                            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                            nRet = MarcUtil.Xml2Marc(strXml,
                                true,   // 2013/1/12 修改为true
                                "", // strMarcSyntax
                                out strOutMarcSyntax,
                                out strMarc,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            if (String.IsNullOrEmpty(strOutMarcSyntax) == false)
                            {
                                if (strOutMarcSyntax != strSyntax
                                    && bSyntaxWarned == false)
                                {
                                    strWarning += "书目记录 " + strRecPath + " 的syntax '" + strOutMarcSyntax + "' 和其所属数据库 '" + strBiblioDbName + "' 的定义syntax '" + strSyntax + "' 不一致\r\n";
                                    bSyntaxWarned = true;
                                }
                            }

                            objStatis.MarcRecord = strMarc;

                            if (this.MarcFilter != null)
                            {
                                // 触发Script中PreFilter()代码
                                if (objStatis != null)
                                {
                                    objStatis.Xml = strXml;
                                    objStatis.BiblioDom = dom;
                                    objStatis.CurrentDbSyntax = strSyntax;  // strOutputMarcSyntax?
                                    objStatis.CurrentRecPath = strRecPath;
                                    objStatis.CurrentRecordIndex = i;
                                    bItemDomsCleared = true;
                                    objStatis.ClearItemDoms();
                                    objStatis.ClearOrderDoms();
                                    objStatis.ClearIssueDoms();
                                    objStatis.ClearCommentDoms();

                                    StatisEventArgs args = new StatisEventArgs();
                                    objStatis.PreFilter(this, args);
                                    if (args.Continue == ContinueType.SkipAll)
                                        return 1;
                                }

                                // 触发filter中的Record相关动作
                                nRet = this.MarcFilter.DoRecord(
                                    null,
                                    objStatis.MarcRecord,
                                    strOutMarcSyntax,   // 2012/9/6
                                    i,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                            }
                        }
                        else
                        {
                            objStatis.MarcRecord = "";

                            if (this.MarcFilter != null
                                && bFilterWarned == false)
                            {
                                // TODO: 是否需要警告？因为配置了filter, 但是因为所涉及的库不是MARC格式，没有办法应用
                                // 可以最后集中警告一次
                                strWarning += "当前统计方案中配置了MarcFilter，但是因为数据库 '" + strBiblioDbName + "' (可能不仅限于这一个数据库)的定义syntax '" + strSyntax + "' 不是MARC类格式，所以统计过程中至少对这个库无法启用MarcFilter功能。\r\n";
                                bFilterWarned = true;
                            }
                        }

                        // 触发Script中OnRecord()代码
                        if (objStatis != null)
                        {
                            objStatis.Xml = strXml;
                            objStatis.BiblioDom = dom;
                            objStatis.CurrentDbSyntax = strSyntax;  // strOutputMarcSyntax?
                            objStatis.CurrentRecPath = strRecPath;
                            objStatis.CurrentRecordIndex = i;
                            if (bItemDomsCleared == false)
                            {
                                objStatis.ClearItemDoms();
                                objStatis.ClearOrderDoms();
                                objStatis.ClearIssueDoms();
                                objStatis.ClearCommentDoms();
                                bItemDomsCleared = true;
                            }

                            StatisEventArgs args = new StatisEventArgs();
                            objStatis.OnRecord(this, args);
                            if (args.Continue == ContinueType.SkipAll)
                                return 1;
                        }

                        nCount++;
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
        // 检索获得特定批次号，或者所有书目记录路径(输出到文件)
        int SearchBiblioRecPath(
            string strBatchNo,
            string strRecPathFilename,
            out string strError)
        {
            strError = "";

            // 创建文件
            StreamWriter sw = new StreamWriter(strRecPathFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {
                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索 ...");
                stop.BeginLoop();

                EnableControls(false);
                 * */

                try
                {
                    long lRet = 0;
                    string strQueryXml = "";

                    // 不指定批次号，意味着特定库全部条码
                    if (String.IsNullOrEmpty(strBatchNo) == true)
                    {
                        lRet = Channel.SearchBiblio(stop,
                             this.comboBox_inputBiblioDbName.Text,
                             "",
                             -1,    // nPerMax
                             "recid",
                             "left",
                             this.Lang,
                             null,   // strResultSetName
                             "",    // strSearchStyle
                            "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 指定批次号。特定库。
                        lRet = Channel.SearchBiblio(stop,
                             this.comboBox_inputBiblioDbName.Text,
                             strBatchNo,
                             -1,    // nPerMax
                             "batchno",
                             "exact",
                             this.Lang,
                             null,   // strResultSetName
                             "",    // strSearchStyle
                            "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }

                    long lHitCount = lRet;

                    long lStart = 0;
                    long lCount = lHitCount;


                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
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
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (this.radioButton_inputStyle_recPathFile.Checked == true)
                {
                    if (this.textBox_inputRecPathFilename.Text == "")
                    {
                        strError = "尚未指定输入的记录路径文件名";
                        goto ERROR1;
                    }
                }
                else if (this.radioButton_inputStyle_recPaths.Checked == true)
                {
                    if (this.textBox_inputStyle_recPaths.Text == "")
                    {
                        strError = "尚未指定记录路径(每行一个)";
                        goto ERROR1;
                    }
                }
                else
                {
                    if (this.comboBox_inputBiblioDbName.Text == "")
                    {
                        strError = "尚未指定书目库名";
                        goto ERROR1;
                    }
                }

                // 切换到过滤特性page
                this.tabControl_main.SelectedTab = this.tabPage_filter;
                return;

            }

            if (this.tabControl_main.SelectedTab == this.tabPage_filter)
            {
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
                        "", // strInitialParamString
                        out strError,
                        out strWarning);

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

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, "警告: \r\n" + strWarning);
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
            /*
            if (this.objStatis == null)
            {
                MessageBox.Show(this, "尚未执行统计，无法打印");
                return;
            }*/

            HtmlPrintForm printform = new HtmlPrintForm();

            printform.Text = "打印统计结果";
            printform.MainForm = this.MainForm;
            if (this.objStatis != null)
                printform.Filenames = this.objStatis.OutputFileNames;
            else
                printform.Filenames = null;
            this.MainForm.AppInfo.LinkFormState(printform, "printform_state");
            printform.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(printform);

        }

        private void button_getProjectName_Click(object sender, EventArgs e)
        {
            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = this.ScriptManager;
            dlg.ProjectName = this.textBox_projectName.Text;
            dlg.NoneProject = false;

            this.MainForm.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_projectName.Text = dlg.ProjectName;
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
            if (this.radioButton_inputStyle_recPathFile.Checked == true)
            {
                this.textBox_inputRecPathFilename.Enabled = true;
                this.button_findInputRecPathFilename.Enabled = true;

                this.tabComboBox_inputBatchNo.Enabled = false;
                this.comboBox_inputBiblioDbName.Enabled = false;

                this.textBox_inputStyle_recPaths.Enabled = false;
            }
            else if (this.radioButton_inputStyle_recPaths.Checked == true)
            {
                this.textBox_inputRecPathFilename.Enabled = false;
                this.button_findInputRecPathFilename.Enabled = false;

                this.tabComboBox_inputBatchNo.Enabled = false;
                this.comboBox_inputBiblioDbName.Enabled = false;

                this.textBox_inputStyle_recPaths.Enabled = true;
            }
            else
            {
                this.textBox_inputRecPathFilename.Enabled = false;
                this.button_findInputRecPathFilename.Enabled = false;

                this.tabComboBox_inputBatchNo.Enabled = true;
                this.comboBox_inputBiblioDbName.Enabled = true;

                this.textBox_inputStyle_recPaths.Enabled = false;
            }
        }

        // 输入风格
        /// <summary>
        /// 输入方式
        /// </summary>
        public BiblioStatisInputStyle InputStyle
        {
            get
            {
                if (this.radioButton_inputStyle_recPathFile.Checked == true)
                    return BiblioStatisInputStyle.RecPathFile;
                else if (this.radioButton_inputStyle_recPaths.Checked == true)
                    return BiblioStatisInputStyle.RecPaths;
                else
                    return BiblioStatisInputStyle.BatchNo;
            }
            set
            {
                if (value == BiblioStatisInputStyle.RecPathFile)
                    this.radioButton_inputStyle_recPathFile.Checked = true;
                else if (value == BiblioStatisInputStyle.BatchNo)
                    this.radioButton_inputStyle_biblioDatabase.Checked = true;
                else if (value == BiblioStatisInputStyle.RecPaths)
                    this.radioButton_inputStyle_recPaths.Checked = true;
            }
        }

        private void button_findInputRecPathFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的书目记录路径文件名";
            dlg.FileName = this.textBox_inputRecPathFilename.Text;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputRecPathFilename.Text = dlg.FileName;
        }

        private void comboBox_inputBiblioDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_inputBiblioDbName.Items.Count > 0)
                return;

            this.comboBox_inputBiblioDbName.Items.Add("<全部>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    this.comboBox_inputBiblioDbName.Items.Add(prop.DbName);
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


        // 清除错误信息窗口中残余的内容
        void ClearErrorInfoForm()
        {
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
        }
#endif

        private void BiblioStatisForm_Activated(object sender, EventArgs e)
        {
            // MyForm里面已经作了
            // this.MainForm.stopManager.Active(this.stop);
        }

        int m_nInDropDown = 0;

        private void tabComboBox_inputBatchNo_DropDown(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            ComboBox combobox = (ComboBox)sender;
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count == 0
                    && this.GetBatchNoTable != null)
                {
                    GetKeyCountListEventArgs e1 = new GetKeyCountListEventArgs();
                    this.GetBatchNoTable(this, e1);

                    if (e1.KeyCounts != null)
                    {
                        for (int i = 0; i < e1.KeyCounts.Count; i++)
                        {
                            KeyCount item = e1.KeyCounts[i];
                            combobox.Items.Add(item.Key + "\t" + item.Count.ToString() + "笔");
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void radioButton_inputStyle_recPaths_CheckedChanged(object sender, EventArgs e)
        {
            SetInputPanelEnabled();
        }

        // 逗号间隔
        /// <summary>
        /// “数据来源”属性页中的记录路径列表字符串，多个路径用逗号间隔
        /// </summary>
        public string RecPathList
        {
            get
            {
                return this.textBox_inputStyle_recPaths.Text.Replace("\r\n", ",");
            }
            set
            {
                this.textBox_inputStyle_recPaths.Text = value.Replace(",", "\r\n");
            }
        }

        // 提供 C# 脚本调用
        /// <summary>
        /// 执行统计方案
        /// </summary>
        /// <param name="strProjectName">统计方案名</param>
        /// <param name="strInitialParamString">初始化参数字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RunProject(string strProjectName,
            string strInitialParamString,
            out string strError)
        {
            strError = "";
            string strWarning = "";

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
                    strInitialParamString,
                    out strError,
                    out strWarning);

                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.Running = false;
            }

            this.tabControl_main.SelectedTab = this.tabPage_runStatis;
            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, "警告: \r\n" + strWarning);

            // MessageBox.Show(this, "统计完成。");
            return 0;
        ERROR1:
            return -1;
        }

        /*
        // 保存实体记录
        // 不负责刷新界面和报错
        int SaveEntityRecords(string strBiblioRecPath,
            EntityInfo[] entities,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存册信息 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();


            try
            {
                long lRet = Channel.SetEntities(
                    stop,
                    strBiblioRecPath,
                    entities,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }
         * */

        // 
        /// <summary>
        /// 保存XML格式的书目记录到数据库
        /// </summary>
        /// <param name="strPath">记录路径</param>
        /// <param name="strXml">XML 记录体</param>
        /// <param name="baTimestamp">时间戳</param>
        /// <param name="strOutputPath">返回实际保存的记录路径</param>
        /// <param name="baNewTimestamp">返回最新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 保存成功</returns>
        public int SaveXmlBiblioRecordToDatabase(string strPath,
            string strXml,
            byte[] baTimestamp,
            out string strOutputPath,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            baNewTimestamp = null;
            strOutputPath = "";


            string strAction = "change";

            if (Global.IsAppendRecPath(strPath) == true)
                strAction = "new";

            /*
            if (String.IsNullOrEmpty(strPath) == true)
                strAction = "new";
            else
            {
                string strRecordID = Global.GetRecordID(strPath);
                if (String.IsNullOrEmpty(strRecordID) == true
                    || strRecordID == "?")
                    strAction = "new";
            }
            */
            long lRet = Channel.SetBiblioInfo(
                stop,
                strAction,
                strPath,
                "xml",
                strXml,
                baTimestamp,
                "",
                out strOutputPath,
                out baNewTimestamp,
                out strError);
            if (lRet == -1)
            {
                strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;
                goto ERROR1;
            }

            return 1;
        ERROR1:
            return -1;
        }
    }

    /// <summary>
    /// 书目统计窗数据输入类型
    /// </summary>
    public enum BiblioStatisInputStyle
    {
        /// <summary>
        /// 记录路径文件
        /// </summary>
        RecPathFile = 1,    // 记录路径文件
        /// <summary>
        /// 批次号 （包含全库情况）
        /// </summary>
        BatchNo = 2,    // 批次号 （包含全库情况）
        /// <summary>
        /// 记录路径
        /// </summary>
        RecPaths = 3,   // 记录路径
    }

    /// <summary>
    /// 用于书目统计的 FilterDocument 派生类(MARC 过滤器文档类)
    /// </summary>
    public class MyFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对象
        /// </summary>
        public BiblioStatis BiblioStatis = null;
    }
}