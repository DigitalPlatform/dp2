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

// 2013/3/26 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 册统计窗
    /// </summary>
    public partial class ItemStatisForm : MyScriptForm
    {
        // 数据库类型
        /// <summary>
        /// 数据库类型。 item/order/issue/comment 之一
        /// </summary>
        public string DbType = "item";  // comment order issue

        /// <summary>
        /// 是否要一开始就获得书目记录 XML
        /// </summary>
        public bool FirstGetBiblbioXml = false;

        /// <summary>
        /// 获取批次号key+count值列表
        /// </summary>
        public event GetKeyCountListEventHandler GetBatchNoTable = null;

#if NO
        /// <summary>
        /// 错误信息窗
        /// </summary>
        public HtmlViewerForm ErrorInfoForm = null;
#endif

        // bool Running = false;   // 正在执行运算

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm
        {
            get
            {
                return (MainForm)this.MdiParent;
            }
        }
        
        DigitalPlatform.Stop stop = null;
#endif

#if NO
        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();
#endif

        ItemStatis objStatis = null;
        Assembly AssemblyMain = null;

        Assembly AssemblyFilter = null;
        AnotherFilterDocument MarcFilter = null;

#if NO
        /// <summary>
        /// 进度控制
        /// </summary>
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
                return MainForm.ItemStatisAssemblyVersion;
            }
            set
            {
                MainForm.ItemStatisAssemblyVersion = value;
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
        public ItemStatisForm()
        {
            InitializeComponent();
        }

        private void ItemStatisForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif
            ScriptManager.CfgFilePath =
    this.MainForm.DataDir + "\\" + this.DbType + "_statis_projects.xml";

#if NO
            ScriptManager.applicationInfo = this.MainForm.AppInfo;
            ScriptManager.CfgFilePath =
                this.MainForm.DataDir + "\\"+this.DbType+"_statis_projects.xml";
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
            this.GetBatchNoTable -= new GetKeyCountListEventHandler(ItemStatisForm_GetBatchNoTable);
            this.GetBatchNoTable += new GetKeyCountListEventHandler(ItemStatisForm_GetBatchNoTable);

            this.radioButton_inputStyle_barcodeFile.Checked = this.MainForm.AppInfo.GetBoolean(
                this.DbType + "statisform",
                "inputstyle_barcodefile",
                false);

            this.radioButton_inputStyle_recPathFile.Checked = this.MainForm.AppInfo.GetBoolean(
                this.DbType + "statisform",
                "inputstyle_recpathfile",
                false);

            /*
            this.radioButton_inputStyle_batchNo.Checked = this.MainForm.AppInfo.GetBoolean(
                "itemstatisform",
                "inputstyle_batchno",
                false);
             * */


            this.radioButton_inputStyle_readerDatabase.Checked = this.MainForm.AppInfo.GetBoolean(
                this.DbType + "statisform",
                "inputstyle_itemdatabase",
                true);


            // 输入的条码号文件名
            this.textBox_inputBarcodeFilename.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "statisform",
                "input_barcode_filename",
                "");

            // 输入的记录路径文件名
            this.textBox_inputRecPathFilename.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "statisform",
                "input_recpath_filename",
                "");

            // 批次号
            this.tabComboBox_inputBatchNo.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "statisform",
                "input_batchno",
                "");

            // 输入的实体库名
            this.comboBox_inputItemDbName.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "statisform",
                "input_itemdbname",
                "<全部>");

            // 方案名
            this.textBox_projectName.Text = this.MainForm.AppInfo.GetString(
                this.DbType + "statisform",
                "projectname",
                "");

            // 馆藏地点列表
            this.textBox_locationNames.Text = this.MainForm.AppInfo.GetString(
                 this.DbType + "statisform",
                 "locations",
                 "*");

            // 册类型列表
            this.textBox_itemTypes.Text = this.MainForm.AppInfo.GetString(
                 this.DbType + "statisform",
                 "itemtypes",
                 "*");

            this.SetWindowTitle();
        }

        void SetWindowTitle()
        {
                this.Text = this.DbTypeCaption + "统计窗";
                // this.label_entityDbName.Text = this.DbTypeCaption + "库(&D)";

                if (this.DbType == "item")
                {
                    this.radioButton_inputStyle_barcodeFile.Visible = true;
                    this.textBox_inputBarcodeFilename.Visible = true;
                    this.button_findInputBarcodeFilename.Visible = true;

                    if (this.tabControl_main.TabPages.IndexOf(this.tabPage_filter) == -1)
                    {
                        this.tabControl_main.TabPages.Insert(1, this.tabPage_filter);
                    }

                    this.label_inputItemDbName.Text = "实体库名(&I)";
                }
                else
                {
                    this.radioButton_inputStyle_barcodeFile.Visible = false;
                    this.textBox_inputBarcodeFilename.Visible = false;
                    this.button_findInputBarcodeFilename.Visible = false;

                    this.tabControl_main.TabPages.Remove(this.tabPage_filter);
                    this.AddFreeControl(this.tabPage_filter);   // 2015/11/7

                    this.label_inputItemDbName.Text = this.DbTypeCaption + "库名(&I)";
                }
        }

        /// <summary>
        /// 数据库类型的显示用字符串
        /// </summary>
        public string DbTypeCaption
        {
            get
            {
                if (this.DbType == "item")
                    return "册";
                else if (this.DbType == "comment")
                    return "评注";
                else if (this.DbType == "order")
                    return "订购";
                else if (this.DbType == "issue")
                    return "期";
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");
            }
        }

        void ItemStatisForm_GetBatchNoTable(object sender, GetKeyCountListEventArgs e)
        {
            Global.GetBatchNoTable(e,
                this,
                "", // 目前不分图书和期刊。 TODO: 其实将来可以把pubtype列表定为3态，其中一个是“书+刊”
                this.DbType,    // "item",
                this.stop,
                this.Channel);
        }

        private void ItemStatisForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void ItemStatisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
                    this.DbType + "statisform",
                    "inputstyle_barcodefile",
                    this.radioButton_inputStyle_barcodeFile.Checked);

                this.MainForm.AppInfo.SetBoolean(
                    this.DbType + "statisform",
                    "inputstyle_recpathfile",
                    this.radioButton_inputStyle_recPathFile.Checked);

                /*
                this.MainForm.AppInfo.SetBoolean(
                    "itemstatisform",
                    "inputstyle_batchno",
                    this.radioButton_inputStyle_batchNo.Checked);
                 * */

                this.MainForm.AppInfo.SetBoolean(
                    this.DbType + "statisform",
                    "inputstyle_itemdatabase",
                    this.radioButton_inputStyle_readerDatabase.Checked);


                // 输入的条码号文件名
                this.MainForm.AppInfo.SetString(
                    this.DbType + "statisform",
                    "input_barcode_filename",
                    this.textBox_inputBarcodeFilename.Text);

                // 输入的记录路径文件名
                this.MainForm.AppInfo.SetString(
                    this.DbType + "statisform",
                    "input_recpath_filename",
                    this.textBox_inputRecPathFilename.Text);

                // 批次号
                this.MainForm.AppInfo.SetString(
                    this.DbType + "statisform",
                    "input_batchno",
                    this.tabComboBox_inputBatchNo.Text);

                // 输入的实体库名
                this.MainForm.AppInfo.SetString(
                    this.DbType + "statisform",
                    "input_itemdbname",
                    this.comboBox_inputItemDbName.Text);

                // 方案名
                this.MainForm.AppInfo.SetString(
                    this.DbType + "statisform",
                    "projectname",
                    this.textBox_projectName.Text);

                // 馆藏地点列表
                this.MainForm.AppInfo.SetString(
                     this.DbType + "statisform",
                     "locations",
                     this.textBox_locationNames.Text);

                // 册类型列表
                this.MainForm.AppInfo.SetString(
                     this.DbType + "statisform",
                     "itemtypes",
                     this.textBox_itemTypes.Text);
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

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

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

        // 创建缺省的main.cs文件
        /// <summary>
        /// 创建缺省的main.cs文件
        /// </summary>
        /// <param name="strFileName">要创建的文件名</param>
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


                sw.WriteLine("public class MyStatis : ItemStatis");

                sw.WriteLine("{");

                sw.WriteLine("	public override void OnBegin(object sender, StatisEventArgs e)");
                sw.WriteLine("	{");
                sw.WriteLine("	}");

                sw.WriteLine("}");
            }
        }

        // 创建缺省的marcfilter.fltx文件
        /// <summary>
        /// 创建缺省的marcfilter.fltx文件
        /// </summary>
        /// <param name="strFileName">要创建的 MARC 过滤器文件名</param>
        public static void CreateDefaultMarcFilterFile(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {

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

            }
        }

        private void button_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            if (this.DbType == "item")
                dlg.HostName = "ItemStatisForm";
            else if (this.DbType == "order")
                dlg.HostName = "OrderStatisForm";
            else if (this.DbType == "issue")
                dlg.HostName = "IssueStatisForm";
            else if (this.DbType == "comment")
                dlg.HostName = "CommentStatisForm";
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

        // TODO: OnEnd()有可能抛出异常，要能够截获和处理
        int RunScript(string strProjectName,
            string strProjectLocate,
            out string strError)
        {
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

                // 2009/11/5
                // 防止以前残留的打开的文件依然没有关闭
                /*
                if (this.objStatis != null)
                {
                    try
                    {
                        this.objStatis.FreeResources();
                    }
                    catch
                    {
                    }
                }
                 * */

                this.objStatis = null;
                this.AssemblyMain = null;
                AnotherFilterDocument filter = null;

                // 2009/11/5
                // 防止以前残留的打开的文件依然没有关闭
                Global.ForceGarbageCollection();

                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out objStatis,
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

                objStatis.ProjectDir = strProjectLocate;
                objStatis.Console = this.Console;

                objStatis.LocationNames = this.textBox_locationNames.Text;

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
                strError = "脚本 '" + strProjectName + "' 执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
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
            out ItemStatis objStatis,
            out AnotherFilterDocument filter,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;
            filter = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~item_statis_main_" + Convert.ToString(AssemblyVersion++) + ".dll");    // ++

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
   									Environment.CurrentDirectory + "\\digitalplatform.Script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            string strHostName = "";
            if (this.DbType == "item")
                strHostName = "ItemStatisForm";
            else if (this.DbType == "order")
                strHostName = "OrderStatisForm";
            else if (this.DbType == "issue")
                strHostName = "IssueStatisForm";
            else if (this.DbType == "comment")
                strHostName = "CommentStatisForm";


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                strHostName,
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
                "dp2Circulation.ItemStatis");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.ItemStatis 派生类。";
                goto ERROR1;
            }
            // new一个Statis派生对象
            objStatis = (ItemStatis)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Statis派生类设置参数
            objStatis.ItemStatisForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            ////////////////////////////
            // 装载marfilter.fltx
            string strFilterFileName = strProjectLocate + "\\marcfilter.fltx";

            if (FileUtil.FileExist(strFilterFileName) == true)
            {
                filter = new AnotherFilterDocument();
                filter.ItemStatis = objStatis;
                filter.strOtherDef = entryClassType.FullName + " ItemStatis = null;";


                filter.strPreInitial = " AnotherFilterDocument doc = (AnotherFilterDocument)this.Document;\r\n";
                filter.strPreInitial += " ItemStatis = ("
                    + entryClassType.FullName + ")doc.ItemStatis;\r\n";

                try
                {
                    filter.Load(strFilterFileName);
                }
                catch (Exception ex)
                {
                    strError = "文件 " + strFilterFileName + " 装载到MarcFilter时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                nRet = filter.BuildScriptFile(strProjectLocate + "\\marcfilter.fltx.cs",
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
                string[] saAdditionalRef = filter.GetRefs();

                // 合并的链接库
                string[] saTotalFilterRef = new string[saAddRef1.Length + saAdditionalRef.Length];
                Array.Copy(saAddRef1, saTotalFilterRef, saAddRef1.Length);
                Array.Copy(saAdditionalRef, 0,
                    saTotalFilterRef, saAddRef1.Length,
                    saAdditionalRef.Length);


                string strfilterCsDllName = strProjectLocate + "\\~marcfilter_" + Convert.ToString(AssemblyVersion++) + ".dll";

                // 创建Project中Script的Assembly
                nRet = ScriptManager.BuildAssembly(
                    strHostName,
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

        internal int DoMarcFilter(
            int nIndex,
            string strMarcRecord,
            string strMarcSyntax,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.MarcFilter == null)
                return 0;

            // 触发filter中的Record相关动作
            nRet = this.MarcFilter.DoRecord(
                null,
                strMarcRecord,
                strMarcSyntax,
                nIndex,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 对每个实体记录进行循环
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


            // 馆藏地点过滤列表
            string strLocationList = this.textBox_locationNames.Text.Trim();
            if (String.IsNullOrEmpty(strLocationList) == true)
                strLocationList = "*";

            string[] locations = strLocationList.Split(new char[] { ',' });

            StringMatchList location_matchlist = new StringMatchList(locations);

            // 实体类型过滤列表
            string strItemTypeList = this.textBox_itemTypes.Text.Trim();
            if (String.IsNullOrEmpty(strItemTypeList) == true)
                strItemTypeList = "*";

            string[] itemtypes = strItemTypeList.Split(new char[] { ',' });

            StringMatchList itemtype_matchlist = new StringMatchList(itemtypes);

            // 记录路径临时文件
            string strTempRecPathFilename = Path.GetTempFileName();

            string strInputFileName = "";   // 外部制定的输入文件，为条码号文件或者记录路径文件格式
            string strAccessPointName = "";

            try
            {

                if (this.InputStyle == ItemStatisInputStyle.BatchNo)
                {
                    nRet = SearchItemRecPath(
                        this.tabComboBox_inputBatchNo.Text,
                        strTempRecPathFilename,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    strInputFileName = strTempRecPathFilename;
                    strAccessPointName = "记录路径";
                }
                else if (this.InputStyle == ItemStatisInputStyle.BarcodeFile)
                {
                    Debug.Assert(this.DbType == "item", "");

                    strInputFileName = this.textBox_inputBarcodeFilename.Text;
                    strAccessPointName = "册条码";
                }
                else if (this.InputStyle == ItemStatisInputStyle.RecPathFile)
                {
                    strInputFileName = this.textBox_inputRecPathFilename.Text;
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
                stop.Initial("正在获取册记录 ...");
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
                                    this.DbType + "statisform",
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

                        OutputDebugInfo("处理行" + (i + 1).ToString() + " '" + strRecPathOrBarcode + "'");

                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个"+this.DbTypeCaption+"记录，" + strAccessPointName + "为 " + strRecPathOrBarcode);
                        this.progressBar_records.Value = (int)sr.BaseStream.Position;

                        // 获得册记录
                        string strOutputRecPath = "";
                        byte[] baTimestamp = null;


                        string strResult = "";

                        string strAccessPoint = "";
                        if (this.InputStyle == ItemStatisInputStyle.BatchNo)
                            strAccessPoint = "@path:" + strRecPathOrBarcode;
                        else if (this.InputStyle == ItemStatisInputStyle.RecPathFile)
                            strAccessPoint = "@path:" + strRecPathOrBarcode;
                        else if (this.InputStyle == ItemStatisInputStyle.BarcodeFile)
                            strAccessPoint = strRecPathOrBarcode;
                        else
                        {
                            Debug.Assert(false, "");
                        }

                        string strBiblio = "";
                        string strBiblioRecPath = "";
                        string strBiblioType = "recpath";
                        if (this.FirstGetBiblbioXml == true)
                            strBiblioType = "xml";

                        if (this.DbType == "item")
                        {
                            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                            lRet = Channel.GetItemInfo(
                                stop,
                                strAccessPoint,
                                "xml", // strResultType,
                                out strResult,
                                out strOutputRecPath,
                                out baTimestamp,
                                strBiblioType,
                                out strBiblio,
                                out strBiblioRecPath,
                                out strError);
                        }
                        else if (this.DbType == "order")
                        {
                            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                            lRet = Channel.GetOrderInfo(
                                stop,
                                strAccessPoint,
                                "xml", // strResultType,
                                out strResult,
                                out strOutputRecPath,
                                out baTimestamp,
                                strBiblioType,
                                out strBiblio,
                                out strBiblioRecPath,
                                out strError);
                        }
                        else if (this.DbType == "issue")
                        {
                            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                            lRet = Channel.GetIssueInfo(
                                stop,
                                strAccessPoint,
                                "xml", // strResultType,
                                out strResult,
                                out strOutputRecPath,
                                out baTimestamp,
                                strBiblioType,
                                out strBiblio,
                                out strBiblioRecPath,
                                out strError);
                        }
                        else if (this.DbType == "comment")
                        {
                            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                            lRet = Channel.GetCommentInfo(
                                stop,
                                strAccessPoint,
                                "xml", // strResultType,
                                out strResult,
                                out strOutputRecPath,
                                out baTimestamp,
                                strBiblioType,
                                out strBiblio,
                                out strBiblioRecPath,
                                out strError);
                        }                        
                        if (lRet == -1)
                        {
                            strError = "获得"+this.DbTypeCaption+"记录 " + strAccessPoint + " 时发生错误: " + strError;
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet == 0)
                        {
                            strError = "" + strAccessPointName + " " + strRecPathOrBarcode + " 对应的XML数据没有找到。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (lRet > 1)
                        {
                            strError = "" + strAccessPointName + " " + strRecPathOrBarcode + " 对应数据多于一条。";
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        string strXml = "";

                        strXml = strResult;


                        // 看看是否在希望统计的范围内
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "册记录装入DOM发生错误: " + ex.Message;
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        if (this.DbType == "item")
                        {
                            // 按照馆藏地点筛选
                            if (this.textBox_locationNames.Text != ""
                                && this.textBox_locationNames.Text != "*")
                            {
                                // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                                string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                                    "location");
                                if (location_matchlist.Match(strLocation) == false)
                                {
                                    OutputDebugInfo("馆藏地 '"+strLocation+"' 被筛选去除");
                                    continue;
                                }
                            }

                            // 按照册类型筛选
                            if (this.textBox_itemTypes.Text != ""
                                && this.textBox_itemTypes.Text != "*")
                            {
                                // 注：空字符串或者"*"表示什么都满足。也就等于不使用此筛选项

                                string strItemType = DomUtil.GetElementText(dom.DocumentElement,
                                    "bookType");
                                if (itemtype_matchlist.Match(strItemType) == false)
                                {
                                    OutputDebugInfo("册类型 '" + strItemType + "' 被筛选去除");
                                    continue;
                                }
                            }
                        }

                        // Debug.Assert(false, "");

                        // strXml中为册记录

                        // 触发Script中OnRecord()代码
                        if (objStatis != null)
                        {
                            objStatis.Xml = strXml;
                            objStatis.Timestamp = baTimestamp;
                            objStatis.ItemDom = dom;
                            objStatis.CurrentRecPath = strOutputRecPath;
                            objStatis.CurrentRecordIndex = i;
                            objStatis.CurrentBiblioRecPath = strBiblioRecPath;

                            if (this.FirstGetBiblbioXml == true)
                            {
                                objStatis.m_strBiblioXml = strBiblio;
                            }
                            else
                            {
                                objStatis.m_strBiblioXml = null;   // 迫使用到的时候重新获取
                            }

                            objStatis.m_biblioDom = null;   // 迫使用到的时候重新获取
                            objStatis.m_strMarcRecord = null;   // 迫使用到的时候重新获取
                            objStatis.m_strMarcSyntax = null;   // 迫使用到的时候重新获取


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

        void OutputDebugInfo(string strText)
        {
            if (this.checkBox_selectProject_outputDebugInfo.Checked == true)
                GetErrorInfoForm().WriteHtml(strText + "\r\n");
        }

        // 注意：上级函数RunScript()已经使用了BeginLoop()和EnableControls()
        // 检索获得特定批次号，或者所有册记录路径(输出到文件)
        int SearchItemRecPath(
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

                    // 不指定批次号，意味着特定库全部记录。2013/1/25
                    if (String.IsNullOrEmpty(strBatchNo) == true)
                    {
                        if (this.DbType == "item")
                        {
                            // TODO: 是否应该用__id更合适？因为一些记录没有册条码号
                            lRet = Channel.SearchItem(stop,
                                this.comboBox_inputItemDbName.Text,
                                 "",
                                 -1,
                                 "__id", // 2013/1/25   // "册条码",
                                 "left",
                                 this.Lang,
                                 null,   // strResultSetName
                                 "",    // strSearchStyle
                                 "", // strOutputStyle
                                 out strError);
                        }
                        else if (this.DbType == "order")
                        {
                            lRet = Channel.SearchOrder(stop,
                                this.comboBox_inputItemDbName.Text,
                                 "",
                                 -1,
                                 "__id",
                                 "left",
                                 this.Lang,
                                 null,   // strResultSetName
                                 "",    // strSearchStyle
                                 "", // strOutputStyle
                                 out strError);
                        }
                        else if (this.DbType == "issue")
                        {
                            lRet = Channel.SearchIssue(stop,
                                this.comboBox_inputItemDbName.Text,
                                 "",
                                 -1,
                                 "__id",
                                 "left",
                                 this.Lang,
                                 null,   // strResultSetName
                                 "",    // strSearchStyle
                                 "", // strOutputStyle
                                 out strError);
                        }
                        else if (this.DbType == "comment")
                        {
                            lRet = Channel.SearchComment(stop,
                                this.comboBox_inputItemDbName.Text,
                                 "",
                                 -1,
                                 "__id",
                                 "left",
                                 this.Lang,
                                 null,   // strResultSetName
                                 "",    // strSearchStyle
                                 "", // strOutputStyle
                                 out strError);
                        }
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 指定批次号。特定库。
                        if (this.DbType == "item")
                        {
                            lRet = Channel.SearchItem(stop,
                                    this.comboBox_inputItemDbName.Text,
                                    strBatchNo,
                                    -1,
                                    "批次号",
                                    "exact",
                                    this.Lang,
                                    null,   // strResultSetName
                                    "",    // strSearchStyle
                                    "", // strOutputStyle
                                    out strError);
                        }
                        else if (this.DbType == "order")
                        {
                            lRet = Channel.SearchOrder(stop,
                                    this.comboBox_inputItemDbName.Text,
                                    strBatchNo,
                                    -1,
                                    "批次号",
                                    "exact",
                                    this.Lang,
                                    null,   // strResultSetName
                                    "",    // strSearchStyle
                                    "", // strOutputStyle
                                    out strError);
                        }
                        else if (this.DbType == "issue")
                        {
                            lRet = Channel.SearchIssue(stop,
                                    this.comboBox_inputItemDbName.Text,
                                    strBatchNo,
                                    -1,
                                    "批次号",
                                    "exact",
                                    this.Lang,
                                    null,   // strResultSetName
                                    "",    // strSearchStyle
                                    "", // strOutputStyle
                                    out strError);
                        }
                        else if (this.DbType == "comment")
                        {
                            lRet = Channel.SearchComment(stop,
                                    this.comboBox_inputItemDbName.Text,
                                    strBatchNo,
                                    -1,
                                    "批次号",
                                    "exact",
                                    this.Lang,
                                    null,   // strResultSetName
                                    "",    // strSearchStyle
                                    "", // strOutputStyle
                                    out strError);
                        }
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

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (this.DbType == "item")
                {
                    if (this.radioButton_inputStyle_barcodeFile.Checked == true)
                    {
                        if (this.textBox_inputBarcodeFilename.Text == "")
                        {
                            strError = "尚未指定输入的条码号文件名";
                            goto ERROR1;
                        }
                    }
                }
                if (this.radioButton_inputStyle_recPathFile.Checked == true)
                {
                    if (this.textBox_inputRecPathFilename.Text == "")
                    {
                        strError = "尚未指定输入的记录路径文件名";
                        goto ERROR1;
                    }
                }
                else
                {
                    if (this.comboBox_inputItemDbName.Text == "")
                    {
                        strError = "尚未指定"+this.DbTypeCaption+"库名";
                        goto ERROR1;
                    }
                }

                if (this.DbType == "item")
                {
                    // 切换到过滤特性page
                    this.tabControl_main.SelectedTab = this.tabPage_filter;
                }
                else
                {
                    this.tabControl_main.SelectedTab = this.tabPage_selectProject;
                }
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
            printform.MainForm = this.MainForm;

            Debug.Assert(this.objStatis != null, "");
            printform.Filenames = this.objStatis.OutputFileNames;

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

                this.tabComboBox_inputBatchNo.Enabled = false;
                this.comboBox_inputItemDbName.Enabled = false;
            }
            else if (this.radioButton_inputStyle_recPathFile.Checked == true)
            {
                this.textBox_inputBarcodeFilename.Enabled = false;
                this.button_findInputBarcodeFilename.Enabled = false;

                this.textBox_inputRecPathFilename.Enabled = true;
                this.button_findInputRecPathFilename.Enabled = true;


                this.tabComboBox_inputBatchNo.Enabled = false;
                this.comboBox_inputItemDbName.Enabled = false;
            }
            else
            {
                this.textBox_inputBarcodeFilename.Enabled = false;
                this.button_findInputBarcodeFilename.Enabled = false;

                this.textBox_inputRecPathFilename.Enabled = false;
                this.button_findInputRecPathFilename.Enabled = false;

                this.tabComboBox_inputBatchNo.Enabled = true;
                this.comboBox_inputItemDbName.Enabled = true;
            }
        }

        // 输入风格
        /// <summary>
        /// 输入方式
        /// </summary>
        public ItemStatisInputStyle InputStyle
        {
            get
            {
                if (this.radioButton_inputStyle_barcodeFile.Checked == true)
                    return ItemStatisInputStyle.BarcodeFile;
                else if (this.radioButton_inputStyle_recPathFile.Checked == true)
                    return ItemStatisInputStyle.RecPathFile;
                else
                    return ItemStatisInputStyle.BatchNo;
            }
        }

        private void button_findInputBarcodeFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的册条码号文件名";
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

            dlg.Title = "请指定要打开的册记录路径文件名";
            dlg.FileName = this.textBox_inputRecPathFilename.Text;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_inputRecPathFilename.Text = dlg.FileName;

        }

        private void comboBox_inputItemDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_inputItemDbName.Items.Count > 0)
                return;

            this.comboBox_inputItemDbName.Items.Add("<全部>");
            this.comboBox_inputItemDbName.Items.Add("<全部期刊>");
            this.comboBox_inputItemDbName.Items.Add("<全部图书>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                    string strDbName = "";
                    if (this.DbType == "item")
                        strDbName = prop.ItemDbName;
                    else if (this.DbType == "order")
                        strDbName = prop.OrderDbName;
                    else if (this.DbType == "issue")
                        strDbName = prop.IssueDbName;
                    else if (this.DbType == "comment")
                        strDbName = prop.CommentDbName;

                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    this.comboBox_inputItemDbName.Items.Add(strDbName);
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

        /// <summary>
        /// 获得书目信息
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strBiblioType">要获得的信息格式。可以用多种格式之一：xml / html / text / @??? / summary / outputpath</param>
        /// <param name="strBiblio">返回书目信息</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1：出错，错误信息在参数 strError 中返回； 0：没有找到； 1：找到</returns>
        public int GetBiblioInfo(string strBiblioRecPath,
            string strBiblioType,
            out string strBiblio,
            out string strError)
        {
            strError = "";
            strBiblio = "";

            string strBiblioXml = "";   // 向服务器提供的XML记录
            long lRet = this.Channel.GetBiblioInfo(
                null,   // this.stop,
                strBiblioRecPath,
                strBiblioXml,
                strBiblioType,
                out strBiblio,
                out strError);
            return (int)lRet;
        }

        private void ItemStatisForm_Activated(object sender, EventArgs e)
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

    }

    /// <summary>
    /// 侧统计窗的输入方式
    /// </summary>
    public enum ItemStatisInputStyle
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
        /// 批次号 （包含全库情况）
        /// </summary>
        BatchNo = 3,    // 批次号 （包含全库情况）
    }

    /// <summary>
    /// 用于册统计的 FilterDocument 派生类(MARC 过滤器文档类)
    /// </summary>
    public class AnotherFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对象
        /// </summary>
        public ItemStatis ItemStatis = null;
    }
}