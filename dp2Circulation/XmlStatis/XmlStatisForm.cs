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

using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    ///  XML 统计窗
    /// </summary>
    public partial class XmlStatisForm : MyScriptForm
    {
        XmlStatis objStatis = null;
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
                return MainForm.XmlStatisAssemblyVersion;
            }
            set
            {
                MainForm.XmlStatisAssemblyVersion = value;
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
        public XmlStatisForm()
        {
            InitializeComponent();
        }

        private void XmlStatisForm_Load(object sender, EventArgs e)
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
    "xml_statis_projects.xml");

#if NO
            ScriptManager.applicationInfo = Program.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                Program.MainForm.DataDir + "\\xml_statis_projects.xml";
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

            // 输入的条码号文件名
            this.textBox_inputXmlFilename.Text = Program.MainForm.AppInfo.GetString(
                "xmlstatisform",
                "input_xml_filename",
                "");


            // 方案名
            this.textBox_projectName.Text = Program.MainForm.AppInfo.GetString(
                "xmlstatisform",
                "projectname",
                "");


        }

        private void XmlStatisForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void XmlStatisForm_FormClosed(object sender, FormClosedEventArgs e)
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
                // 输入的条码号文件名
                Program.MainForm.AppInfo.SetString(
                    "xmlstatisform",
                    "input_xml_filename",
                    this.textBox_inputXmlFilename.Text);

                // 方案名
                Program.MainForm.AppInfo.SetString(
                    "xmlstatisform",
                    "projectname",
                    this.textBox_projectName.Text);
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

                sw.WriteLine("using dp2Circulation;");

                sw.WriteLine("using DigitalPlatform.Xml;");

                sw.WriteLine("public class MyStatis : XmlStatis");

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
            dlg.HostName = "XmlStatisForm";
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
            this.button_findInputXmlFilename.Enabled = bEnable;

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

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out objStatis,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (strInitialParamString == "test_compile")
                    return 0;

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;
                objStatis.InputFilename = this.textBox_inputXmlFilename.Text;


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
            out XmlStatis objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~xml_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

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

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "XmlStatisForm",
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

            // 得到Assembly中XmlStatis派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.XmlStatis");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.XmlStatis 派生类。";
                goto ERROR1;
            }
            // new一个XmlStatis派生对象
            objStatis = (XmlStatis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为XmlStatis派生类设置参数
            objStatis.XmlStatisForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 对每个XML记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoLoop(out string strError)
        {
            strError = "";
            // int nRet = 0;
            // long lRet = 0;

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
            // 清除错误信息窗口中残余的内容
            ClearErrorInfoForm();

            string strInputFileName = "";

            try
            {
                strInputFileName = this.textBox_inputXmlFilename.Text;

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

                XmlTextReader reader = new XmlTextReader(file);

                this.progressBar_records.Minimum = 0;
                this.progressBar_records.Maximum = (int)file.Length;
                this.progressBar_records.Value = 0;

                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取XML记录 ...");
                stop.BeginLoop();

                EnableControls(false);
                 * */

                bool bRet = false;

                while (true)
                {
                    bRet = reader.Read();
                    if (bRet == false)
                    {
                        strError = "没有根元素";
                        return -1;
                    }
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                }

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


                        while (true)
                        {
                            bRet = reader.Read();
                            if (bRet == false)
                                return 0;
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        if (bRet == false)
                            return 0;	// 结束

                        string strXml = reader.ReadOuterXml();

                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个XML记录");
                        this.progressBar_records.Value = (int)file.Position;

                        // strXml中为XML记录
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "XML记录装入DOM发生错误: " + ex.Message;
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        // 触发Script中OnRecord()代码
                        if (objStatis != null)
                        {
                            objStatis.Xml = strXml;
                            objStatis.RecordDom = dom;
                            objStatis.CurrentRecordIndex = i;

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
                if (this.textBox_inputXmlFilename.Text == "")
                {
                    strError = "尚未指定输入的XML文件名";
                    goto ERROR1;
                }

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

        private void button_findInputXmlFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的XML文件名";
            dlg.FileName = this.textBox_inputXmlFilename.Text;
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputXmlFilename.Text = dlg.FileName;

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

        private void XmlStatisForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);
        }
    }
}