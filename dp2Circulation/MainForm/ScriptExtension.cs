using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using System.IO;
using DigitalPlatform.CommonControl;


namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关统计方案和 C# 脚本的功能
    /// </summary>
    public partial class MainForm
    {
        #region 安装和更新统计方案

        // 从磁盘更新全部方案
        void UpdateStatisProjectsFromDisk()
        {
            string strError = "";
            int nUpdateCount = 0;
            int nRet = 0;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            // dir_dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bHideMessageBox = false;
            bool bDontUpdate = false;

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    bNewOpened = true;
                    Application.DoEvents();
                }

                try
                {
                    // return:
                    //      -2  全部放弃
                    //      -1  出错
                    //      >=0 更新数
                    nRet = UpdateProjects(form,
                        dir_dlg.SelectedPath,
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;
                    nUpdateCount += nRet;
                }
                finally
                {
                    if (bNewOpened == true)
                        form.Close();
                }
            }

            // 凭条打印
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this.OperHistory,
                    dir_dlg.SelectedPath,
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            // MainForm
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this,
                    dir_dlg.SelectedPath,
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            if (nUpdateCount > 0)
                MessageBox.Show(this, "共更新 " + nUpdateCount.ToString() + " 个方案");
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从磁盘安装全部方案
        void InstallStatisProjectsFromDisk()
        {
            string strError = "";
            int nRet = -1;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;

            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定方案所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            // dir_dlg.SelectedPath = this.textBox_outputFolder.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            // this.textBox_outputFolder.Text = dir_dlg.SelectedPath;


            // 寻找 projects.xml 文件
            string strLocalFileName = PathUtil.MergePath(dir_dlg.SelectedPath, "projects.xml");
            if (File.Exists(strLocalFileName) == false)
            {
                // strError = "您所指定的目录 '" + dir_dlg.SelectedPath + "' 中并没有包含 projects.xml 文件，无法进行安装";
                // goto ERROR1;

                // 如果没有 projects.xml 文件，则搜索全部 *.projpack 文件，并创建好一个临时的 ~projects.xml文件
                strLocalFileName = PathUtil.MergePath(this.DataDir, "~projects.xml");
                nRet = ScriptManager.BuildProjectsFile(dir_dlg.SelectedPath,
                    strLocalFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 列出已经安装的方案的URL
            List<string> installed_urls = new List<string>();
            List<Form> newly_opened_forms = new List<Form>();
            List<Form> forms = new List<Form>();

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                // bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    newly_opened_forms.Add(form);
                    Application.DoEvents();
                }

                forms.Add(form);

                dynamic o = form;
                List<string> urls = new List<string>();
                nRet = o.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 凭条打印
            {
                List<string> urls = new List<string>();
                nRet = this.OperHistory.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 框架窗口
            {
                List<string> urls = new List<string>();
                nRet = this.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            try
            {
                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                MainForm.SetControlFont(dlg, this.DefaultFont);
                dlg.XmlFilename = strLocalFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                this.AppInfo.LinkFormState(dlg,
                    "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // 分宿主进行安装
                foreach (Form form in forms)
                {
                    // 为一个统计窗安装若干方案
                    // parameters:
                    //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
                    // return:
                    //      -1  出错
                    //      >=0 安装的方案数
                    nRet = InstallProjects(
                        form,
                        GetWindowName(form),
                        dlg.SelectedProjects,
                        dir_dlg.SelectedPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // 凭条打印
                {
                    nRet = InstallProjects(
    this.OperHistory,
    "凭条打印",
    dlg.SelectedProjects,
    dir_dlg.SelectedPath,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // MainForm
                {
                    nRet = InstallProjects(
    this,
    "框架窗口",
    dlg.SelectedProjects,
    dir_dlg.SelectedPath,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }
            }
            finally
            {
                // 关闭本次新打开的窗口
                foreach (Form form in newly_opened_forms)
                {
                    form.Close();
                }
            }

            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从 dp2003.com 安装全部方案
        void InstallStatisProjects()
        {
            string strError = "";
            int nRet = -1;
            int nInstallCount = 0;

            bool bDebugger = false;
            if (Control.ModifierKeys == Keys.Control)
                bDebugger = true;

            // 下载projects.xml文件
            string strLocalFileName = PathUtil.MergePath(this.DataDir, "~temp_projects.xml");
            string strTempFileName = PathUtil.MergePath(this.DataDir, "~temp_download_projects.xml");

            try
            {
                File.Delete(strLocalFileName);
            }
            catch
            {
            }
            try
            {
                File.Delete(strTempFileName);
            }
            catch
            {
            }

            nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                "http://dp2003.com/dp2circulation/projects/projects.xml",
                strLocalFileName,
                strTempFileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 列出已经安装的方案的URL
            List<string> installed_urls = new List<string>();
            List<Form> newly_opened_forms = new List<Form>();
            List<Form> forms = new List<Form>();

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                // bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    newly_opened_forms.Add(form);
                    Application.DoEvents();
                }

                forms.Add(form);

                dynamic o = form;
                List<string> urls = new List<string>();
                nRet = o.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 凭条打印
            {
                List<string> urls = new List<string>();
                nRet = this.OperHistory.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            // 框架窗口
            {
                List<string> urls = new List<string>();
                nRet = this.ScriptManager.GetInstalledUrls(out urls,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                installed_urls.AddRange(urls);
            }

            try
            {
                SelectInstallProjectsDialog dlg = new SelectInstallProjectsDialog();
                MainForm.SetControlFont(dlg, this.DefaultFont);
                dlg.XmlFilename = strLocalFileName;
                dlg.InstalledUrls = installed_urls;
                if (bDebugger == true)
                    dlg.Category = "debugger";
                dlg.StartPosition = FormStartPosition.CenterScreen;

                this.AppInfo.LinkFormState(dlg,
                    "SelectInstallProjectsDialog_state");
                dlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                // 分宿主进行安装
                foreach (Form form in forms)
                {
                    // 为一个统计窗安装若干方案
                    // parameters:
                    //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
                    // return:
                    //      -1  出错
                    //      >=0 安装的方案数
                    nRet = InstallProjects(
                        form,
                        GetWindowName(form),
                        dlg.SelectedProjects,
                        "!url",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // 凭条打印
                {
                    nRet = InstallProjects(
                        this.OperHistory,
                        "凭条打印",
                        dlg.SelectedProjects,
                        "!url",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }

                // MainForm
                {
                    nRet = InstallProjects(
                        this,
                        "框架窗口",
                        dlg.SelectedProjects,
                        "!url",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nInstallCount += nRet;
                }
            }
            finally
            {
                // 关闭本次新打开的窗口
                foreach (Form form in newly_opened_forms)
                {
                    form.Close();
                }
            }

            MessageBox.Show(this, "共安装方案 " + nInstallCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从 dp2003.com 检查更新全部方案
        void UpdateStatisProjects()
        {
            string strError = "";
            int nUpdateCount = 0;
            int nRet = 0;

            bool bHideMessageBox = false;
            bool bDontUpdate = false;

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                bool bNewOpened = false;
                var form = GetTopChildWindow(type);
                if (form == null)
                {
                    form = (Form)Activator.CreateInstance(type);
                    form.MdiParent = this;
                    form.WindowState = FormWindowState.Minimized;
                    form.Show();
                    bNewOpened = true;
                    Application.DoEvents();
                }

                try
                {
                    // return:
                    //      -2  全部放弃
                    //      -1  出错
                    //      >=0 更新数
                    nRet = UpdateProjects(form,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;
                    nUpdateCount += nRet;
                }
                finally
                {
                    if (bNewOpened == true)
                        form.Close();
                }
            }

            // 凭条打印
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this.OperHistory,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            // MainForm
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = UpdateProjects(this,
                        "!url",
                        ref bHideMessageBox,
                        ref bDontUpdate,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nUpdateCount += nRet;
            }

            if (nUpdateCount > 0)
                MessageBox.Show(this, "共更新 " + nUpdateCount.ToString() + " 个方案");
            else
                MessageBox.Show(this, "没有发现更新");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        /// <summary>
        /// 是否隐藏浏览器控件的脚本错误提示
        /// </summary>
        public bool SuppressScriptErrors
        {
            get
            {
                return !DisplayScriptErrorDialog;
            }
        }

        // 浏览器控件允许脚本错误对话框(&S)
        /// <summary>
        /// 浏览器控件是否允许脚本错误对话框
        /// </summary>
        public bool DisplayScriptErrorDialog
        {
            get
            {

                return this.AppInfo.GetBoolean(
                    "global",
                    "display_webbrowsecontrol_scripterror_dialog",
                    false);
            }
            set
            {
                this.AppInfo.SetBoolean(
                    "global",
                    "display_webbrowsecontrol_scripterror_dialog",
                    value);
            }
        }

        // 获得统计窗的英文类型名
        static string GetTypeName(object form)
        {
            string strTypeName = form.GetType().ToString();
            int nRet = strTypeName.LastIndexOf(".");
            if (nRet != -1)
                strTypeName = strTypeName.Substring(nRet + 1);

            return strTypeName;
        }

        // 获得统计窗的汉字类型名
        static string GetWindowName(object form)
        {
            return SelectInstallProjectsDialog.GetHanziHostName(GetTypeName(form));
        }


        // 为一个统计窗安装若干方案
        // parameters:
        //      projects    待安装的方案。注意有可能包含不适合安装到本窗口的方案
        // return:
        //      -1  出错
        //      >=0 安装的方案数
        int InstallProjects(
            object form,
            string strWindowName,
            List<ProjectItem> projects,
            string strSource,
            out string strError)
        {
            strError = "";
            int nInstallCount = 0;
            int nRet = 0;

            dynamic o = form;

            o.EnableControls(false);
            try
            {
                /*
                    string strTypeName = form.GetType().ToString();
                    nRet = strTypeName.LastIndexOf(".");
                    if (nRet != -1)
                        strTypeName = strTypeName.Substring(nRet + 1);
                */
                string strTypeName = GetTypeName(form);

                foreach (ProjectItem item in projects)
                {
                    if (strTypeName != item.Host)
                        continue;

                    string strLocalFileName = "";
                    string strLastModified = "";

                    if (strSource == "!url")
                    {
                        strLocalFileName = this.DataDir + "\\~install_project.projpack";
                        string strTempFileName = this.DataDir + "\\~temp_download_webfile";

                        nRet = WebFileDownloadDialog.DownloadWebFile(
                            this,
                            item.Url,
                            strLocalFileName,
                            strTempFileName,
                            "",
                            out strLastModified,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    {
                        string strLocalDir = strSource;

                        // Uri uri = new Uri(item.Url);
                        /*
                        string strPath = item.Url;  // uri.LocalPath;
                        nRet = strPath.LastIndexOf("/");
                        if (nRet != -1)
                            strPath = strPath.Substring(nRet);
                         * */
                        string strPureFileName = ScriptManager.GetFileNameFromUrl(item.Url);

                        strLocalFileName = PathUtil.MergePath(strLocalDir, strPureFileName);

                        FileInfo fi = new FileInfo(strLocalFileName);
                        if (fi.Exists == false)
                        {
                            strError = "目录 '" + strLocalDir + "' 中没有找到文件 '" + strPureFileName + "'";
                            return -1;
                        }
                        strLastModified = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);
                    }

                    // 安装Project
                    // return:
                    //      -1  出错
                    //      0   没有安装方案
                    //      >0  安装的方案数
                    nRet = o.ScriptManager.InstallProject(
                        o is Form ? o : this,
                        strWindowName,
                        strLocalFileName,
                        strLastModified,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    nInstallCount += nRet;
                }
            }
            finally
            {
                o.EnableControls(true);
            }

            return nInstallCount;
        }

        // 更新一个窗口拥有的全部方案
        // parameters:
        //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
        // return:
        //      -2  全部放弃
        //      -1  出错
        //      >=0 更新数
        int UpdateProjects(
            object form,
            string strSource,
            ref bool bHideMessageBox,
            ref bool bDontUpdate,
            out string strError)
        {
            strError = "";
            string strWarning = "";
            string strUpdateInfo = "";
            int nUpdateCount = 0;

            dynamic o = form;

            o.EnableControls(false);
            try
            {
                // 检查更新一个容器节点下的全部方案
                // parameters:
                //      dir_node    容器节点。如果 == null 检查更新全部方案
                //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      0   成功
                int nRet = o.ScriptManager.CheckUpdate(
                    o is Form ? o : this,
                    null,
                    strSource,
                    ref bHideMessageBox,
                    ref bDontUpdate,
                    ref nUpdateCount,
                    ref strUpdateInfo,
                    ref strWarning,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == -2)
                    return -2;
            }
            finally
            {
                o.EnableControls(true);
            }
            return nUpdateCount;
        }

        #endregion


        #region Client.cs 脚本支持

        ClientHost _clientHost = null;
        public ClientHost ClientHost
        {
            get
            {
                return _clientHost;
            }
        }

        int InitialClientScript(out string strError)
        {
            strError = "";

            Assembly assembly = null;

            string strServerMappedPath = Path.Combine(this.DataDir, "servermapped");
            string strFileName = Path.Combine(strServerMappedPath, "client/client.cs");

            if (File.Exists(strFileName) == false)
                return 0;   // 脚本文件没有找到

            int nRet = PrepareClientScript(strFileName,
                out assembly,
                out _clientHost,
                out strError);
            if (nRet == -1)
            {
                strError = "初始化前端脚本 '" + Path.GetFileName(strFileName) + "' 时出错: " + strError;
                return -1;
            }

            // _clientHost.MainForm = this;

            return 0;
        }

        // 准备脚本环境
        int PrepareClientScript(string strCsFileName,
            out Assembly assembly,
            out ClientHost host,
            out string strError)
        {
            assembly = null;
            strError = "";
            host = null;

            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strCsFileName,
                -1,
                out string strContent,
                out Encoding encoding,
                out strError);
            if (nRet == -1)
                return -1;

            string strWarningInfo = "";
            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",
                                    "System.Core.dll",  // Linq 需要

                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
   									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",  // 2011/8/25 新增
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
                Environment.CurrentDirectory + "\\dp2circulation.exe",
            };

            // 2013/12/16
            nRet = ScriptManager.GetRef(strCsFileName,
    ref saAddRef,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            // 直接编译到内存
            // parameters:
            //		refs	附加的refs文件路径。路径中可能包含宏%installdir%
            nRet = ScriptManager.CreateAssembly_1(strContent,
                saAddRef,
                "",
                out assembly,
                out strError,
                out strWarningInfo);
            if (nRet == -1)
                goto ERROR1;

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Circulation.ClientHost");
            if (entryClassType == null)
            {
                strError = strCsFileName + " 中没有找到 dp2Circulation.ClientHost 派生类";
                goto ERROR1;
            }

            // new一个Host派生对象
            host = (ClientHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        #region MainForm 统计方案

        private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
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
        /// 创建缺省的、宿主为 MainFormHost 的 main.cs 文件
        /// </summary>
        /// <param name="strFileName">文件全路径</param>
        /// <returns>0: 成功</returns>
        public static int CreateDefaultMainCsFile(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.Windows.Forms;");
                sw.WriteLine("using System.IO;");
                sw.WriteLine("using System.Text;");
                sw.WriteLine("using System.Xml;");
                sw.WriteLine("");

                //sw.WriteLine("using DigitalPlatform.MarcDom;");
                //sw.WriteLine("using DigitalPlatform.Statis;");
                sw.WriteLine("using dp2Circulation;");
                sw.WriteLine("");
                sw.WriteLine("using DigitalPlatform.Xml;");
                sw.WriteLine("");

                sw.WriteLine("public class MyStatis : MainFormHost");

                sw.WriteLine("{");

                sw.WriteLine("	public override void Main(object sender, EventArgs e)");
                sw.WriteLine("	{");
                sw.WriteLine("	}");

                sw.WriteLine("}");
            }
            return 0;
        }

        private void ToolStripMenuItem_projectManage_Click(object sender, EventArgs e)
        {
            ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProjectsUrl = "http://dp2003.com/dp2circulation/projects/projects.xml";
            dlg.HostName = "MainForm";
            dlg.scriptManager = this.ScriptManager;
            dlg.AppInfo = this.AppInfo;
            dlg.DataDir = this.DataDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        string m_strProjectName = "";

        // 执行统计方案
        private void toolStripMenuItem_runProject_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = this.ScriptManager;
            dlg.ProjectName = this.m_strProjectName;
            dlg.NoneProject = false;

            this.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.m_strProjectName = dlg.ProjectName;

            //
            string strProjectLocate = "";
            // 获得方案参数
            // strProjectNamePath	方案名，或者路径
            // return:
            //		-1	error
            //		0	not found project
            //		1	found
            int nRet = this.ScriptManager.GetProjectData(
                dlg.ProjectName,
                out strProjectLocate);
            if (nRet == 0)
            {
                strError = "方案 " + dlg.ProjectName + " 没有找到...";
                goto ERROR1;
            }
            if (nRet == -1)
            {
                strError = "scriptManager.GetProjectData() error ...";
                goto ERROR1;
            }

            // 
            nRet = RunScript(dlg.ProjectName,
                strProjectLocate,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 附加的 DLL 搜索路径
        internal List<string> _dllPaths = new List<string>();

        internal Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ScriptManager.ResolveAssembly(args.Name, _dllPaths);
        }

        // 2017/4/24
        public void TestCompile(string strProjectName)
        {
            string strError = "";

            if (String.IsNullOrEmpty(strProjectName) == true)
            {
                strError = "尚未指定方案名";
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
                strError = "凭条打印方案 " + strProjectName + " 没有找到...";
                goto ERROR1;
            }
            if (nRet == -1)
            {
                strError = "scriptManager.GetProjectData() error ...";
                goto ERROR1;
            }

            nRet = PrepareScript(strProjectName,
    strProjectLocate,
    out this.objStatis,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            throw new Exception(strError);
        }

        // 注: Stop 的使用有 Bug 2016/6/28
        public int RunScript(string strProjectName,
    string strProjectLocate,
    out string strError)
        {
            /*
            EnableControls(false);

#if NO
            // BUG!!!
            Stop = new DigitalPlatform.Stop();
            Stop.Register(stopManager, true);	// 和容器关联
#endif

            this.Stop.OnStop += new StopEventHandler(this.DoStop);
            this.Stop.Initial("正在执行脚本 ...");
            this.Stop.BeginLoop();
            */
            var looping = Looping("正在执行脚本 ...");

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
                    out this.objStatis,
                    out strError);
                if (nRet == -1)
                    return -1;

                objStatis.ProjectDir = strProjectLocate;
                // objStatis.Console = this.Console;

                // 执行脚本的Main()

                if (objStatis != null)
                {
                    EventArgs args = new EventArgs();
                    objStatis.Main(this, args);
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "脚本 '" + strProjectName + "' 执行过程抛出异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
                looping.Dispose();
                /*
                this.Stop.EndLoop();
                this.Stop.OnStop -= new StopEventHandler(this.DoStop);
                this.Stop.Initial("");

#if NO
                // BUG!!!
                if (Stop != null) // 脱离关联
                {
                    Stop.Unregister();	// 和容器关联
                    Stop = null;
                }
#endif
                EnableControls(true);
                */

                this.AssemblyMain = null;
                AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            }
        }

        // 准备脚本环境
        int PrepareScript(string strProjectName,
            string strProjectLocate,
            out MainFormHost objStatis,
            out string strError)
        {
            this.AssemblyMain = null;

            objStatis = null;

            string strWarning = "";
            string strMainCsDllName = PathUtil.MergePath(this.InstanceDir, "\\~mainform_statis_main_" + Convert.ToString(this.StatisAssemblyVersion++) + ".dll");    // ++

            string strLibPaths = "\"" + this.DataDir + "\""
                + ","
                + "\"" + strProjectLocate + "\"";

            string[] saAddRef = {
                                    // 2011/4/20 增加
                                    "system.dll",
                                    "system.drawing.dll",
                                    "system.windows.forms.dll",
                                    "system.xml.dll",
                                    "System.Runtime.Serialization.dll",
                                    "System.Core.dll",  // Linq 需要

									Environment.CurrentDirectory + "\\digitalplatform.core.dll",
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
   									Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.Script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dp2.statis.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe",
            };


            // 创建Project中Script main.cs的Assembly
            // return:
            //		-2	出错，但是已经提示过错误信息了。
            //		-1	出错
            int nRet = ScriptManager.BuildAssembly(
                "MainForm",
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

            // 得到Assembly中MainFormHost派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                this.AssemblyMain,
                "dp2Circulation.MainFormHost");
            if (entryClassType == null)
            {
                strError = strMainCsDllName + "中没有找到 dp2Circulation.MainFormHost 派生类。";
                goto ERROR1;
            }
            // new一个Statis派生对象
            objStatis = (MainFormHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Statis派生类设置参数
            objStatis.MainForm = this;
            objStatis.ProjectDir = strProjectLocate;
            objStatis.InstanceDir = this.InstanceDir;

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

    }
}
