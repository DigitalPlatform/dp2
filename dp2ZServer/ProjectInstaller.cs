using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using System.ServiceProcess;

using DigitalPlatform;
using DigitalPlatform.Install;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2ZServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        // string EncryptKey = "dp2zserver_password_key";

        private System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        public ProjectInstaller()
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
            this.serviceInstaller1.DisplayName = "dp2 Z39.50 Service";
            this.serviceInstaller1.ServiceName = "dp2ZService";
            this.serviceInstaller1.Description = "Z39.50服务器，数字平台北京软件有限责任公司 http://dp2003.com";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
            /*
            this.serviceInstaller1.ServicesDependedOn = new string[] {
                "W3SVC"};
             * */
            /* 因为Z39.50服务器可能依赖其他机器的dp2Library
            this.serviceInstaller1.ServicesDependedOn = new string[] {
                "dp2LibraryService"};
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

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

#if NO
            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            string strDataDir = "";

            bool bWriteInstanceInfo = false;
            bool bUpgrade = false;  // 是否为升级安装? 所谓升级安装就是发现数据目录已经存在了

            int nRet = 0;
            string strError = "";

            // ebug.Assert(false, "");

            bool bDialogOpened = false;
            InstallParamDlg param_dlg = new InstallParamDlg();
            GuiUtil.AutoSetDefaultFont(param_dlg);
            // 从XML文件中装载已有的信息到对话框
            // return:
            //      -1  error
            //      0   not load
            //      1   loaded
            LoadExistingInfoFromDp2zserverXmlFile(
               param_dlg,
               strRootDir,
               out strError);

            string strInstanceName = "";
            string[] existing_urls = null;
            string strCertSN = "";
            // 获得instance信息
            // parameters:
            //      urls 获得绑定的Urls
            // return:
            //      false   instance没有找到
            //      true    找到
            bool bRet = InstallHelper.GetInstanceInfo("dp2ZServer",
                0,
            out strInstanceName,
            out strDataDir,
            out existing_urls,
            out strCertSN);

            strDataDir = strRootDir;

            string strExistingXmlFile = PathUtil.MergePath(strRootDir, "unioncatalog.xml");
            if (File.Exists(strExistingXmlFile) == false)
            {

                param_dlg.ShowDialog(ForegroundWindow.Instance);

                if (param_dlg.DialogResult == DialogResult.Cancel)
                {
                    throw new Exception("安装被放弃");
                }

                bDialogOpened = true;

                // 创建unioncatalog.xml文件
                // return:
                //      -1  error, install faild
                //      0   succeed
                //      1   suceed, but some config ignored
                nRet = WriteUnionCatalogXmlFile(
                    param_dlg,
                    strRootDir,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }
            else
                bUpgrade = true;


        END1:

            // if (existing_urls == null || existing_urls.Length == 0)
            {
                string[] default_urls = new string[] {
                    //"net.tcp://localhost:7001/gcatserver/",
                    //"net.pipe://localhost/gcatserver/",
                    "http://localhost/unioncatalog/"
                };

                List<string> urls = new List<string>(existing_urls == null ? new string[0] : existing_urls);
                if (urls.Count == 0)
                {
                    urls.AddRange(default_urls);
                }

                WcfBindingDlg binding_dlg = new WcfBindingDlg();
                GuiUtil.AutoSetDefaultFont(binding_dlg);
                binding_dlg.Text = "请指定 UnionCatalogServer 服务器的通讯协议";
                binding_dlg.Urls = StringUtil.FromListString(urls);
                binding_dlg.DefaultUrls = default_urls;
                binding_dlg.NetPipeEnabled = false;
                binding_dlg.NetTcpEnabled = false;
                binding_dlg.HttpComment = "适用于Intranet和Internet";
                binding_dlg.StartPosition = FormStartPosition.CenterScreen;

            REDO_BINDING:
                if (binding_dlg.ShowDialog(ForegroundWindow.Instance) != DialogResult.OK)
                    throw new Exception("用户取消安装。");

                existing_urls = binding_dlg.Urls;

                // 检查和其他产品的bindings是否冲突
                // return:
                //      -1  出错
                //      0   不重
                //      1    重复
                nRet = InstallHelper.IsGlobalBindingDup(string.Join(";", existing_urls),
                    "dp2ZServer",
                    out strError);
                if (nRet != 0)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "协议绑定有问题: " + strError + "\r\n\r\n请重新指定协议绑定");
                    goto REDO_BINDING;
                }

                bWriteInstanceInfo = true;
            }

            if (bWriteInstanceInfo == true)
            {
                // 设置instance信息
                InstallHelper.SetInstanceInfo(
                "dp2ZServer",
                0,
                "",
                strDataDir,
                existing_urls,
                strCertSN);
            }

            strExistingXmlFile = PathUtil.MergePath(strRootDir, "dp2zserver.xml");
            if (File.Exists(strExistingXmlFile) == false)
            {

                if (bDialogOpened == false)
                {
                    param_dlg.ShowDialog(ForegroundWindow.Instance);

                    if (param_dlg.DialogResult == DialogResult.Cancel)
                    {
                        throw new Exception("安装被放弃");
                    }

                    bDialogOpened = true;
                }
                // 写入dp2zserver.xml文件
                // return:
                //      -1  error, install faild
                //      0   succeed
                nRet = WriteDp2zserverXmlFile(
                    param_dlg,
                    strRootDir,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }
#endif
        }

#if NO
        // 创建unioncatalog.xml文件
        // return:
        //      -1  error, install faild
        //      0   succeed
        //      1   suceed, but some config ignored
        int WriteUnionCatalogXmlFile(
            InstallParamDlg dlg,
            string strRootDir,
            out string strError)
        {
            strError = "";

            string strXmlFileName = PathUtil.MergePath(strRootDir, "unioncatalog.xml");
            string strOriginXmlFileName = PathUtil.MergePath(strRootDir, "~unioncatalog.xml");

            string strTemp = "";

            XmlDocument dom = new XmlDocument();

            if (File.Exists(strXmlFileName) == true)
            {
                strTemp = strXmlFileName;
                try
                {
                    dom.Load(strXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryServer /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }
            else
            {
                strTemp = strOriginXmlFileName;

                try
                {
                    dom.Load(strOriginXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryServer /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strOriginXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("libraryServer");

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。缺乏<libraryServer>元素";
                return -1;
            }

            Debug.Assert(node != null, "");

            DomUtil.SetAttr(node, "url", dlg.LibraryWsUrl);

            DomUtil.SetAttr(node, "username", dlg.UserName);
            DomUtil.SetAttr(node, "password", EncryptPassword(dlg.Password));

            try
            {
                dom.Save(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "XML文件 " + strXmlFileName + " 保存时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                return -1;
            }

            return 0;
        }

#endif

#if NO
        // 从XML文件中装载已有的信息到对话框
        // return:
        //      -1  error
        //      0   not load
        //      1   loaded
        int LoadExistingInfoFromDp2zserverXmlFile(
            InstallParamDlg dlg,
            string strRootDir,
            out string strError)
        {
            strError = "";

            string strXmlFileName = PathUtil.MergePath(strRootDir, "dp2zserver.xml");
            string strOriginXmlFileName = PathUtil.MergePath(strRootDir, "~dp2zserver.xml");

            string strTemp = "";

            XmlDocument dom = new XmlDocument();

            if (File.Exists(strXmlFileName) == true)
            {
                strTemp = strXmlFileName;
                try
                {
                    dom.Load(strXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }
            else
            {
                strTemp = strOriginXmlFileName;

                try
                {
                    dom.Load(strOriginXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strOriginXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。";
                return -1;
            }

            Debug.Assert(node != null, "");

            string strUserName = DomUtil.GetAttr(node, "username");
            string strPassword = DomUtil.GetAttr(node, "password");
            strPassword = DecryptPasssword(strPassword);

            string strAnonymousUserName = DomUtil.GetAttr(node, "anonymousUserName");
            string strAnonymousPassword = DomUtil.GetAttr(node, "anonymousPassword");
            strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

            string strUrl = DomUtil.GetAttr(node, "url");

            dlg.UserName = strUserName;
            dlg.Password = strPassword;
            dlg.AnonymousUserName = strAnonymousUserName;
            dlg.AnonymousPassword = strAnonymousPassword;

            if (String.IsNullOrEmpty(strUrl) == false)
                dlg.LibraryWsUrl = strUrl;

            return 1;
        }
#endif

        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);

            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

#if NO1111111111111
            // 创建dp2zserver.xml文件内容

            int nRet = 0;
            string strError = "";

                    // 写入XML文件
        // return:
        //      -1  error, install faild
        //      0   succeed
        //      1   suceed, but some config ignored
            nRet = WriteXmlFile(strRootDir, 
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance,
                    strError);
                strError = "dp2ZServer安装未完成: " + strError;
            }
            else 
            {

                strError = "dp2ZServer安装成功。";
            }
#endif

            // 创建事件日志目录

            // Create the source, if it does not already exist.
            if (!EventLog.SourceExists("dp2ZServer"))
            {
                EventLog.CreateEventSource("dp2ZServer", "DigitalPlatform");
            }

            EventLog Log = new EventLog();
            Log.Source = "dp2ZServer";
            Log.WriteEntry("dp2ZServer安装成功。", EventLogEntryType.Information);
        }

#if NO
        // 写入sp2zserver.xml文件
        // return:
        //      -1  error, install faild
        //      0   succeed
        int WriteDp2zserverXmlFile(
            InstallParamDlg dlg,
            string strRootDir,
            out string strError)
        {
            strError = "";

            string strXmlFileName = PathUtil.MergePath(strRootDir, "dp2zserver.xml");
            string strOriginXmlFileName = PathUtil.MergePath(strRootDir, "~dp2zserver.xml");

            bool bExist = true;

            string strTemp = "";

            XmlDocument dom = new XmlDocument();

            if (File.Exists(strXmlFileName) == true)
            {
                strTemp = strXmlFileName;
                try
                {
                    dom.Load(strXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                    bExist = false;
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }
            else
            {
                strTemp = strOriginXmlFileName;

                bExist = false;

                try
                {
                    dom.Load(strOriginXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strOriginXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }


            XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。";
                return -1;
                /*
                dom.LoadXml("<root><libraryserver /></root>");
                bExist = false;
                XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");
                 * */
            }

            Debug.Assert(node != null, "");

#if NO1111111111111
            string strUserName = DomUtil.GetAttr(node, "username");
            string strPassword = DomUtil.GetAttr(node, "password");
            strPassword = DecryptPasssword(strPassword);

            string strAnonymousUserName = DomUtil.GetAttr(node, "anonymousUserName");
            string strAnonymousPassword = DomUtil.GetAttr(node, "anonymousPassword");
            strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

            string strUrl = DomUtil.GetAttr(node, "url");

            InstallParamDlg dlg = new InstallParamDlg();
            InstallHelper.AutoSetDefaultFont(dlg);
            dlg.UserName = strUserName;
            dlg.Password = strPassword;
            dlg.AnonymousUserName = strAnonymousUserName;
            dlg.AnonymousPassword = strAnonymousPassword;

            if (String.IsNullOrEmpty(strUrl) == false)
                dlg.LibraryWsUrl = strUrl;

            dlg.ShowDialog(ForegroundWindow.Instance);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                if (bExist == true)
                    return 1;

                strError = "您放弃了指定 dp2library 管理帐户 和 匿名登录帐户。安装完成后您需要手动设置 " + strXmlFileName + " 配置文件，否则系统可能无法正常运行";
                return -1;
            }
#endif

            DomUtil.SetAttr(node, "url", dlg.LibraryWsUrl);

            DomUtil.SetAttr(node, "username", dlg.UserName);
            DomUtil.SetAttr(node, "password", EncryptPassword(dlg.Password));

            DomUtil.SetAttr(node, "anonymousUserName",
                String.IsNullOrEmpty(dlg.AnonymousUserName) == true ? null : dlg.AnonymousUserName);

            if (String.IsNullOrEmpty(dlg.AnonymousUserName) == true)
                DomUtil.SetAttr(node, "anonymousPassword", null);
            else
                DomUtil.SetAttr(node, "anonymousPassword", EncryptPassword(dlg.AnonymousPassword));

            try
            {
                dom.Save(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "XML文件 " + strXmlFileName + " 保存时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                return -1;
            }

            return 0;
        }
#endif


#if NO111111111111
        // 写入XML文件
        // return:
        //      -1  error, install faild
        //      0   succeed
        //      1   suceed, but some config ignored
        int WriteXmlFile(out string strError)
        {
            strError = "";

            // Debug.Assert(false, "");

            string strDirectory = Environment.SystemDirectory;
            strDirectory = PathUtil.MergePath(strDirectory, "dp2zserver");


            string strXmlFileName = PathUtil.MergePath(strDirectory, "dp2zserver.xml");
            string strOriginXmlFileName = PathUtil.MergePath(strDirectory, "~dp2zserver.xml");

            bool bExist = true;

            string strTemp = "";

            XmlDocument dom = new XmlDocument();

            if (File.Exists(strXmlFileName) == true)
            {
                strTemp = strXmlFileName;
                try
                {
                    dom.Load(strXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                    bExist = false;
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }
            else
            {
                strTemp = strOriginXmlFileName;

                bExist = false;

                try
                {
                    dom.Load(strOriginXmlFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root><libraryserver /></root>");
                }
                catch (Exception ex)
                {
                    strError = "XML文件 " + strOriginXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                    return -1;
                }
            }


            XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。";
                return -1;
                /*
                dom.LoadXml("<root><libraryserver /></root>");
                bExist = false;
                XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");
                 * */
            }

            Debug.Assert(node != null, "");

            string strUserName = DomUtil.GetAttr(node, "username");
            string strPassword = DomUtil.GetAttr(node, "password");
            strPassword = DecryptPasssword(strPassword);

            string strAnonymousUserName = DomUtil.GetAttr(node, "anonymousUserName");
            string strAnonymousPassword = DomUtil.GetAttr(node, "anonymousPassword");
            strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

            string strUrl = DomUtil.GetAttr(node, "url");

            InstallParamDlg dlg = new InstallParamDlg();
            InstallHelper.AutoSetDefaultFont(dlg);
            dlg.UserName = strUserName;
            dlg.Password = strPassword;
            dlg.AnonymousUserName = strAnonymousUserName;
            dlg.AnonymousPassword = strAnonymousPassword;

            if (String.IsNullOrEmpty(strUrl) == false)
                dlg.LibraryWsUrl = strUrl;

            dlg.ShowDialog(ForegroundWindow.Instance);

            if (dlg.DialogResult == DialogResult.Cancel)
            {
                if (bExist == true)
                    return 1;

                strError = "您放弃了指定 dp2library 管理帐户 和 匿名登录帐户。安装完成后您需要手动设置 "+strXmlFileName+" 配置文件，否则系统可能无法正常运行";
                return -1;
            }

            DomUtil.SetAttr(node, "url", dlg.LibraryWsUrl);

            DomUtil.SetAttr(node, "username", dlg.UserName);
            DomUtil.SetAttr(node, "password", EncryptPassword(dlg.Password));

            DomUtil.SetAttr(node, "anonymousUserName", 
                String.IsNullOrEmpty(dlg.AnonymousUserName) == true ? null : dlg.AnonymousUserName);

            if (String.IsNullOrEmpty(dlg.AnonymousUserName) == true)
                DomUtil.SetAttr(node, "anonymousPassword", null);
            else
                DomUtil.SetAttr(node, "anonymousPassword", EncryptPassword(dlg.AnonymousPassword));

            try
            {
                dom.Save(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "XML文件 " + strXmlFileName + " 保存时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                return -1;
            }

            return 0;
        }

#endif

#if NO
        public string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }

            }

            return "";
        }

        public string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, this.EncryptKey);
        }
#endif
    }
}