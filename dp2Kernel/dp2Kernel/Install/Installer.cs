using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.ServiceProcess;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Install;
using DigitalPlatform.Text;

namespace dp2Kernel
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        public Installer()
        {
            InitializeComponent();

            this.ServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // ServiceProcessInstaller1
            // 
            this.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ServiceProcessInstaller1.Password = null;
            this.ServiceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DisplayName = "dp2 Kernel Service";
            this.serviceInstaller1.ServiceName = "dp2KernelService";
            this.serviceInstaller1.Description = "dp2内核，数字平台北京软件有限责任公司 http://dp2003.com";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
            /*
            this.serviceInstaller1.ServicesDependedOn = new string[] {
                "W3SVC"};
             * */
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
				this.ServiceProcessInstaller1,
				this.serviceInstaller1});

            this.serviceInstaller1.Committed += new InstallEventHandler(serviceInstaller1_Committed); 
        }

        void serviceInstaller1_Committed(object sender, InstallEventArgs e)
        {
            // MessageBox.Show(ForegroundWindow.Instance, "auto start service");
            try
            {
                ServiceController sc = new ServiceController(this.serviceInstaller1.ServiceName);
                sc.Start();
            }
            catch (Exception ex)
            {
                // 报错，但是不停止安装
                MessageBox.Show(ForegroundWindow.Instance,
                    "安装已经完成，但启动 '" + this.serviceInstaller1.ServiceName + "' 失败： " + ex.Message);
            }
        }

        static string UnQuote(string strText)
        {
            return strText.Replace("'", "");
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(System.Collections.IDictionary savedState)
        {
            base.Install(savedState);

            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;

#if OLD_MSI
            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            Debug.Assert(String.IsNullOrEmpty(strRootDir) == false, "");

            // 可以提示，程序文件已经被刷新

            InstanceDialog dlg = new InstanceDialog();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.SourceDir = strRootDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(ForegroundWindow.Instance);

            if (dlg.DialogResult == DialogResult.Cancel)
                throw new InstallException("用户取消安装。");

            if (dlg.Changed == true)
            {
                // 兑现修改

            }

            // stateSaver["upgrade"] = bUpgrade;
#endif
        }

#if OLD_MSI
        // 探测数据目录是否存在
        // parameters:
        //      strRootDir   安装目录
        //      strResultStartFileName  [out]返回探测到的start.xml文件全路径
        //      strResultDataDir    [out]返回测到的数据目录
        // return:
        //      -1  出错
        //      0   不存在
        //      1   存在
        public int DetectDataDir(string strRootDir,
            out string strResultStartFileName,
            out string strResultDataDir,
            out string strError)
        {
            strError = "";

            strResultStartFileName = "";
            strResultDataDir = "";

            string strTempStartXmlFileName = PathUtil.MergePath(strRootDir,
                "start.xml");

            if (File.Exists(strTempStartXmlFileName) == true)
            {
                strResultStartFileName = strTempStartXmlFileName;

                // 已存在start.xml文件
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strTempStartXmlFileName);
                }
                catch (Exception ex)
                {
                    strError = "加载start.xml到dom出错：" + ex.Message;
                    return -1;
                }

                string strDataDir = DomUtil.GetAttr(dom.DocumentElement, "datadir");
                if (strDataDir == "")
                {
                    strError = "start.xml文件中根元素未定义'datadir'属性，或'datadir'属性值为空。";
                    return -1;
                }

                if (Directory.Exists(strDataDir) == false)
                {
                    strError = "start.xml文件中根元素'datadir'属性定义的数据目录在本地不存在。";
                    return -1;
                }

                strResultDataDir = strDataDir;


                string strDatabasesXmlFileName = PathUtil.MergePath(strDataDir,
                    "databases.xml");
                if (File.Exists(strDatabasesXmlFileName) == false)
                {
                    strError = "start.xml文件中根元素'datadir'属性定义的数据目录 '" + strDataDir + "' 中不存在'databases.xml'文件，因此该数据目录不合法";
                    return -1;
                }

                return 1;
            }
            else
            {
                string strDataDir = PathUtil.MergePath(strRootDir, "data");
                if (Directory.Exists(strDataDir) == true)
                {
                    strResultDataDir = strDataDir;

                    string strDatabasesFileName = PathUtil.MergePath(strDataDir, "databases.xml");
                    if (File.Exists(strDatabasesFileName) == false)
                    {
                        strError = "安装目录 '"+strRootDir+"' 中存在'data'数据目录，但该目录中不包含'databases.xml'文件， 因此该数据目录不合法。";
                        return -1;
                    }

                    return 1;
                }
            }
            return 0;
        }

        // 创建数据目录
        public int CreateDataDir(
            string strRootDir,
            string strDefaultInstanceName,
            string strDefaultDataDir,
            out string strResultStartFileName,
            out string strResultDataDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            strResultStartFileName = "";
            strResultDataDir = "";

            bool bFinished = false;

            // 要求在temp内准备要安装的数据文件(初次安装而不是升级安装)
            string strTempDataDir = PathUtil.MergePath(strRootDir, "temp");


            REDO_INPUT:
            // 获得数据目录
            DataDirDlg datadir_dlg = new DataDirDlg();
            GuiUtil.AutoSetDefaultFont(datadir_dlg);
            datadir_dlg.Comment = "请指定一个独立的数据目录, 用于存储dp2Kernel内核的各种配置信息。";
            datadir_dlg.MessageBoxTitle = "setup_dp2Kernel";
            datadir_dlg.DataDir = strDefaultDataDir;

            datadir_dlg.StartPosition = FormStartPosition.CenterScreen;
            datadir_dlg.ShowDialog(ForegroundWindow.Instance);
            if (datadir_dlg.DialogResult != DialogResult.OK)
            {
                strError = "用户放弃指定数据目录。安装未完成。";
                throw new InstallException(strError);
            }

            string strDataDir = datadir_dlg.DataDir;

            string strExistingDatabasesFileName = PathUtil.MergePath(strDataDir,
                "databases.xml");

            if (File.Exists(strExistingDatabasesFileName) == true)
            {
                // 从以前的rmsws数据目录升级
                string strText = "数据目录 '" + strDataDir + "' 中已经存在以前的数据库内核版本遗留下来的数据文件。\r\n\r\n确实要利用这个数据目录来进行升级安装么?\r\n(注意：如果利用以前的rmsws的数据目录来进行升级安装，则必须先行卸载rmsws，以避免它和(正在安装的)dp2Kernel同时运行引起冲突)\r\n\r\n(是)继续进行升级安装 (否)重新指定数据目录 (取消)放弃安装";
                DialogResult result = MessageBox.Show(
                    ForegroundWindow.Instance,
    strText,
    "setup_dp2kernel",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                {
                    strError = "用户放弃指定数据目录。安装未完成。";
                    throw new InstallException(strError);
                }

                if (result == DialogResult.No)
                    goto REDO_INPUT;

                // 创建start.xml文件
                string strStartXmlFileName = PathUtil.MergePath(strRootDir, "start.xml");
                nRet = this.CreateStartXml(strStartXmlFileName,
                    strDataDir,
                    out strError);
                if (nRet == -1)
                    throw new InstallException(strError);

                // 删除临时目录
                try
                {
                    Directory.Delete(strTempDataDir, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ForegroundWindow.Instance,
                        "删除临时目录'" + strTempDataDir + "'出错：" + ex.Message);
                }

                strResultStartFileName = strStartXmlFileName;
                strResultDataDir = strDataDir; 
                return 0;
            }

            // Debug.Assert(false, "");

            nRet = PathUtil.CopyDirectory(strTempDataDir,
                strDataDir,
                true,
                out strError);
            if (nRet == -1)
            {
                strError = "拷贝临时目录 '" + strTempDataDir + "' 到数据目录'" + strDataDir + "'发生错误：" + strError;
                Debug.Assert(false, "");
                throw new InstallException(strError);
            }

            try
            {

                strResultDataDir = strDataDir;

                // 删除临时目录
                try
                {
                    Directory.Delete(strTempDataDir, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ForegroundWindow.Instance,
                        "删除临时目录'" + strTempDataDir + "'出错：" + ex.Message);
                }


                // 调对话框得到数据源配置参数
                MsSqlServerDataSourceDlg datasource_dlg = new MsSqlServerDataSourceDlg();
                GuiUtil.AutoSetDefaultFont(datasource_dlg);

                datasource_dlg.Comment = "dp2Kernel内核的数据库功能是基于 SQL Server 2000 以上版本实现的。请设置下列SQL Server相关参数。";
                datasource_dlg.StartPosition = FormStartPosition.CenterScreen;
                datasource_dlg.InstanceName = strDefaultInstanceName;
                datasource_dlg.KernelLoginName = strDefaultInstanceName; // 2010/12/15
                datasource_dlg.ShowDialog(ForegroundWindow.Instance);
                if (datasource_dlg.DialogResult != DialogResult.OK)
                {
                    strError = "放弃设置数据源，安装未完成。";
                    throw new InstallException(strError);
                }

                string strDatabasesFileName = strDataDir + "\\" + "databases.xml";
                nRet = this.ModifyDatabasesXml(strDatabasesFileName,
                    datasource_dlg.SqlServerName,
                    false, // datasource_dlg.SSPI,
                    datasource_dlg.KernelLoginName,
                    datasource_dlg.KernelLoginPassword,
                    datasource_dlg.InstanceName,
                    out strError);
                if (nRet == -1)
                    throw new InstallException(strError);


                // 设置root密码
                RootUserDlg root_dlg = new RootUserDlg();
                GuiUtil.AutoSetDefaultFont(root_dlg);
                root_dlg.UserName = "root";
                root_dlg.Rights = "this:management;children_database:management;children_directory:management;children_leaf:management;descendant_directory:management;descendant_record:management;descendant_leaf:management";
                root_dlg.StartPosition = FormStartPosition.CenterScreen;
                root_dlg.ShowDialog(ForegroundWindow.Instance);
                if (root_dlg.DialogResult != DialogResult.OK)
                {
                    strError = "放弃设置root用户特性，安装未完成。";
                    throw new InstallException(strError);
                }

                nRet = ModifyRootUser(strDataDir,
                    root_dlg.UserName,
                    root_dlg.Password,
                    root_dlg.Rights,
                    out strError);
                if (nRet == -1)
                    throw new InstallException(strError);


                // 创建start.xml文件
                string strStartXmlFileName = PathUtil.MergePath(strRootDir, "start.xml");
                nRet = this.CreateStartXml(strStartXmlFileName,
                    strDataDir,
                    out strError);
                if (nRet == -1)
                    throw new InstallException(strError);



                bFinished = true;

                strResultStartFileName = strStartXmlFileName;
                return 0;

            }
            finally
            {
                if (bFinished == false)
                {
                    try
                    {
                        Directory.Delete(strDataDir, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ForegroundWindow.Instance,
                            "回滚时删除数据目录'" + strDataDir + "'出错：" + ex.Message);
                    }
                }
            }
        }

        // 创建start.xml文件
        // parameters:
        //      strFileName databases.xml文件名
        private int CreateStartXml(string strFileName,
            string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                string strXml = "<root datadir=''/>";

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                DomUtil.SetAttr(dom.DocumentElement, "datadir", strDataDir);

                dom.Save(strFileName);

                return 0;
            }
            catch (Exception ex)
            {
                strError = "创建start.xml文件出错：" + ex.Message;
                return -1;
            }
        }

        // 修改databases.xml里的几项参数
        // parameters:
        //      strFileName databases.xml文件名
        private int ModifyDatabasesXml(string strFileName,
            string strSqlServerName,
            bool bSSPI,
            string strUserName,
            string strPassword,
            string strInstanceName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "加载databases.xml到dom出错：" + ex.Message;
                return -1;

            }

            XmlNode nodeDatasource = dom.DocumentElement.SelectSingleNode("datasource");
            if (nodeDatasource == null)
            {
                strError = "databases.xml不合法，根下的<datasource>元素不存在。";
                return -1;
            }

            if (bSSPI == true)
                DomUtil.SetAttr(nodeDatasource, "mode", "SSPI");
            else
                DomUtil.SetAttr(nodeDatasource, "mode", null);

            DomUtil.SetAttr(nodeDatasource, "servername", strSqlServerName);
            DomUtil.SetAttr(nodeDatasource, "userid", strUserName);

            strPassword = Cryptography.Encrypt(strPassword, "dp2003");
            DomUtil.SetAttr(nodeDatasource, "password", strPassword);


            XmlNode nodeDbs = dom.DocumentElement.SelectSingleNode("dbs");
            if (nodeDbs == null)
            {
                strError = "databases.xml不合法，根下的<dbs>元素不存在。";
                return -1;
            }
            DomUtil.SetAttr(nodeDbs, "instancename", strInstanceName);

            dom.Save(strFileName);

            return 0;
        }


        // 修改root用户记录文件
        int ModifyRootUser(string strDataDir,
            string strUserName,
            string strPassword,
            string strRights,
            out string strError)
        {
            strError = "";

            string strFileName = PathUtil.MergePath(strDataDir, "userdb\\0000000001.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载root用户记录文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement, "name", strUserName);
            DomUtil.SetElementText(dom.DocumentElement, "password",
                Cryptography.GetSHA1(strPassword));


            XmlNode nodeServer = dom.DocumentElement.SelectSingleNode("server");
            if (nodeServer == null)
            {
                Debug.Assert(false, "不可能的情况");
                return -1;
            }

            DomUtil.SetAttr(nodeServer, "rights", strRights);

            dom.Save(strFileName);

            return 0;
        }

#endif

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(System.Collections.IDictionary savedState)
        {
            // int nRet = 0;
            // string strError = "";

#if NO
            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;

            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);
#endif

            // 创建事件日志目录
            if (!EventLog.SourceExists("dp2Kernel"))
            {
                EventLog.CreateEventSource("dp2Kernel", "DigitalPlatform");
            }
            EventLog Log = new EventLog();
            Log.Source = "dp2Kernel";

            Log.WriteEntry("dp2Kernel安装成功。", EventLogEntryType.Information);

            base.Commit(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;

#if OLD_MSI
            String strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            DialogResult result;

            string strText = "是否完全卸载？\r\n\r\n"
                + "单击'是'，则把全部实例的数据目录删除，所有的库配置信息丢失，所有的实例信息丢失。以后安装时需要重新安装数据目录和数据库。\r\n\r\n"
                + "单击'否'，不删除数据目录，仅卸载执行程序，下次安装时可以继续使用已存在的库配置信息。升级安装前的卸载应选此项。";
            result = MessageBox.Show(ForegroundWindow.Instance,
                strText,
                "卸载 dp2Kernel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                InstanceDialog dlg = new InstanceDialog();
                GuiUtil.AutoSetDefaultFont(dlg);
                dlg.Text = "彻底卸载所有实例和数据目录";
                dlg.Comment = "下列实例将被全部卸载。请仔细确认。一旦卸载，全部数据目录和实例信息将被删除，并且无法恢复。";
                dlg.UninstallMode = true;
                dlg.SourceDir = strRootDir;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(ForegroundWindow.Instance);

                if (dlg.DialogResult == DialogResult.Cancel)
                {
                    MessageBox.Show(ForegroundWindow.Instance,
                        "已放弃卸载全部实例和数据目录。仅仅卸载了执行程序。");
                }
            }

            // InstallHelper.DeleteSetupCfgFile(strRootDir);
#endif
        }
    }
}
