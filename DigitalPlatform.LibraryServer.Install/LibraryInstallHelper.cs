
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.Install;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.LibraryServer
{
    public class LibraryInstallHelper
    {
        // TODO: 可考虑用另一个相同功能的函数替代
        // 删除所有用到的内核数据库
        // 专门开发给安装程序卸载时候使用
        public static int DeleteAllDatabase(
            RmsChannel channel,
            XmlDocument cfg_dom,
            out string strError)
        {
            strError = "";

            string strTempError = "";

            long lRet = 0;

            // 大书目库
            XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("itemdbgroup/database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                // 实体库
                string strEntityDbName = DomUtil.GetAttr(node, "name");

                if (String.IsNullOrEmpty(strEntityDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strEntityDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除实体库 '" + strEntityDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }

                // 订购库
                string strOrderDbName = DomUtil.GetAttr(node, "orderDbName");

                if (String.IsNullOrEmpty(strOrderDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strOrderDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除订购库 '" + strOrderDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }

                // 期库
                string strIssueDbName = DomUtil.GetAttr(node, "issueDbName");

                if (String.IsNullOrEmpty(strIssueDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strIssueDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除期库 '" + strIssueDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }

                // 2011/2/21
                // 评注库
                string strCommentDbName = DomUtil.GetAttr(node, "commentDbName");

                if (String.IsNullOrEmpty(strCommentDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strCommentDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除评注库 '" + strCommentDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }

                // 小书目库
                string strBiblioDbName = DomUtil.GetAttr(node, "biblioDbName");

                if (String.IsNullOrEmpty(strBiblioDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strBiblioDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除小书目库 '" + strBiblioDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }
            }

            // 读者库
            nodes = cfg_dom.DocumentElement.SelectNodes("readerdbgroup/database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strDbName = DomUtil.GetAttr(node, "name");

                if (String.IsNullOrEmpty(strDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除读者库 '" + strDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }
            }

            // 预约到书队列库
            XmlNode arrived_node = cfg_dom.DocumentElement.SelectSingleNode("arrived");
            if (arrived_node != null)
            {
                string strArrivedDbName = DomUtil.GetAttr(arrived_node, "dbname");
                if (String.IsNullOrEmpty(strArrivedDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strArrivedDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除预约到书库 '" + strArrivedDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }
            }

            // 违约金库
            XmlNode amerce_node = cfg_dom.DocumentElement.SelectSingleNode("amerce");
            if (amerce_node != null)
            {
                string strAmerceDbName = DomUtil.GetAttr(amerce_node, "dbname");
                if (String.IsNullOrEmpty(strAmerceDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strAmerceDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除违约金库 '" + strAmerceDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }
            }

            // 消息库
            XmlNode message_node = cfg_dom.DocumentElement.SelectSingleNode("message");
            if (message_node != null)
            {
                string strMessageDbName = DomUtil.GetAttr(message_node, "dbname");
                if (String.IsNullOrEmpty(strMessageDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strMessageDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除消息库 '" + strMessageDbName + "' 内数据时候发生错误：" + strTempError + "; ";
                    }
                }
            }

            // 实用库
            nodes = cfg_dom.DocumentElement.SelectNodes("utilDb/database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strDbName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");
                if (String.IsNullOrEmpty(strDbName) == false)
                {
                    lRet = channel.DoDeleteDB(strDbName,
                        out strTempError);
                    if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                    {
                        strError += "删除类型为 " + strType + " 的实用库 '" + strDbName + "' 内数据时发生错误：" + strTempError + "; ";
                    }
                }
            }


            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }

        // 删除应用服务器在dp2Kernel内核中创建的数据库
        // parameters:
        //      strXmlFileName  library.xml 文件的全路径
        // return:
        //      -1  出错
        //      0   用户放弃删除
        //      1   已经删除
        public static int DeleteKernelDatabases(
            IWin32Window owner,
            string strInstanceName,
            string strXmlFilename,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            DialogResult result = MessageBox.Show(owner == null ? ForegroundWindow.Instance : owner,
                "是否要删除应用服务器实例 '" + strInstanceName + "' 在数据库内核中创建过的全部数据库?\r\n\r\n(注：如果现在不删除，将来也可以用内核管理(dp2manager)工具进行删除)",
                "安装 dp2Library",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == DialogResult.No)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFilename);
            }
            catch (Exception ex)
            {
                strError = "XML文件 '" + strXmlFilename + "' 装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode rmsserver_node = dom.DocumentElement.SelectSingleNode("rmsserver");
            if (rmsserver_node == null)
            {
                strError = "<rmsserver>元素没有找到";
                return -1;
            }
            string strKernelUrl = DomUtil.GetAttr(rmsserver_node, "url");
            if (String.IsNullOrEmpty(strKernelUrl) == true)
            {
                strError = "<rmsserver>元素的url属性为空";
                return -1;
            }

            using (RmsChannelCollection channels = new RmsChannelCollection())
            {
                RmsChannel channel = channels.GetChannel(strKernelUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    return -1;
                }

                string strUserName = DomUtil.GetAttr(rmsserver_node, "username");
                string strPassword = DomUtil.GetAttr(rmsserver_node, "password");

                string EncryptKey = "dp2circulationpassword";
                try
                {
                    strPassword = Cryptography.Decrypt(
                        strPassword,
                        EncryptKey);
                }
                catch
                {
                    strError = "<rmsserver>元素password属性中的密码设置不正确";
                    return -1;
                }


                nRet = channel.Login(strUserName,
                    strPassword,
                    out strError);
                if (nRet == -1)
                {
                    strError = "以用户名 '" + strUserName + "' 和密码登录内核时失败: " + strError;
                    return -1;
                }

                nRet = DeleteAllDatabase(
                    channel,
                    dom,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 1;
            }
        }

        // 探测数据目录，是否已经存在数据，是不是属于升级情形
        // return:
        //      -1  error
        //      0   数据目录不存在
        //      1   数据目录存在，但是xml文件不存在
        //      2   xml文件已经存在
        public static int DetectDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            DirectoryInfo di = new DirectoryInfo(strDataDir);
            if (di.Exists == false)
            {
                strError = "目录 '" + strDataDir + "' 不存在";
                return 0;
            }

            string strExistingLibraryFileName = Path.Combine(strDataDir,
                "library.xml");
            if (File.Exists(strExistingLibraryFileName) == true)
                return 2;

            strError = "文件 '" + strExistingLibraryFileName + "' 不存在";
            return 1;
        }

        // 要求操作者用 supervisor 账号登录一次。以便后续进行各种重要操作。
        // 只需要 library.xml 即可，不需要 dp2library 在运行中。
        // return:
        //      -2  实例没有找到
        //      -1  出错
        //      0   放弃验证
        //      1   成功
        public static int LibrarySupervisorLoginByDataDir(IWin32Window owner,
            string strDataDir,
            string strComment,
            out string strError)
        {
            strError = "";

            LibraryInstanceInfo info = null;

            string strFileName = Path.Combine(strDataDir, "library.xml");
            // 注意：本函数不会修改传入的 info 中的 InstanceName 成员
            // return:
            //      -1  出错
            //      0   实例没有找到
            //      1   成功
            int nRet = GetLibraryInstanceInfoFromXml(strFileName,
                ref info,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return -2;

            if (string.IsNullOrEmpty(info.SupervisorUserName) == true)
            {
                // TODO: 此时是否可以不用验证了呢?
                strError = "配置文件 '" + strFileName + "' 中，没有找到具有 managedatabase 权限的管理员账户，因此无法验证操作者身份";
                return -1;
            }

            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.Comment = strComment;
            dlg.ServerUrl = "配置文件 '" + strFileName + "'";
            dlg.UserName = info.SupervisorUserName;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            REDO_LOGIN:
            dlg.ShowDialog(owner);

            if (dlg.DialogResult == DialogResult.Cancel)
                return 0;

            if (info.Version <= 2.0)
            {
                // 以前的做法
                if (dlg.Password != info.SupervisorPassword)
                {
                    MessageBox.Show(owner, "密码不正确。请重新输入密码");
                    goto REDO_LOGIN;
                }
            }
            else
            {
                // 新的做法
                // return:
                //      -1  出错
                //      0   不匹配
                //      1   匹配
                nRet = LibraryServerUtil.MatchUserPassword(dlg.Password,
                    info.SupervisorPassword,
                    out strError);
                if (nRet == -1)
                {
                    strError = "MatchUserPassword() error: " + strError;
                    return -1;
                }
                Debug.Assert(nRet == 0 || nRet == 1, "");
                if (nRet == 0)  // 2016/12/26 从 == 1 修改为 == 0
                {
                    MessageBox.Show(owner, "密码不正确。请重新输入密码");
                    goto REDO_LOGIN;
                }
            }

            return 1;
        }

        // (本函数适用于标准版，不适用于 dp2libraryxe)
        // 要求操作者用 supervisor 账号登录一次。以便后续进行各种重要操作。
        // 只需要 library.xml 即可，不需要 dp2library 在运行中。
        // return:
        //      -2  实例没有找到
        //      -1  出错
        //      0   放弃验证
        //      1   成功
        public static int LibrarySupervisorLoginByInstanceName(IWin32Window owner,
            string strInstanceName,
            string strComment,
            out string strError)
        {
            strError = "";

            LibraryInstanceInfo info = null;

            // 从注册表和 library.xml 文件中获得实例信息
            // return:
            //      -1  出错
            //      0   实例没有找到
            //      1   成功
            int nRet = GetLibraryInstanceInfo(
                strInstanceName,
                out info,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }
            if (nRet == 0)
            {
                strError = "实例 '" + strInstanceName + "' 没有找到";
                return -2;
            }

            if (string.IsNullOrEmpty(info.SupervisorUserName) == true)
            {
                // TODO: 此时是否可以不用验证了呢?
                strError = "实例 '" + strInstanceName + "' 的账户中，没有找到具有 managedatabase 权限的管理员账户，因此无法验证操作者身份";
                return -1;
            }

            ConfirmSupervisorDialog dlg = new ConfirmSupervisorDialog();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.Comment = strComment;
            dlg.ServerUrl = "实例 '" + strInstanceName + "'";
            dlg.UserName = info.SupervisorUserName;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            REDO_LOGIN:
            dlg.ShowDialog(owner);

            if (dlg.DialogResult == DialogResult.Cancel)
                return 0;

            if (info.Version <= 2.0)
            {
                // 以前的做法
                if (dlg.Password != info.SupervisorPassword)
                {
                    MessageBox.Show(owner, "密码不正确。请重新输入密码");
                    goto REDO_LOGIN;
                }
            }
            else
            {
                // 新的做法
                // return:
                //      -1  出错
                //      0   不匹配
                //      1   匹配
                nRet = LibraryServerUtil.MatchUserPassword(dlg.Password,
                    info.SupervisorPassword,
                    out strError);
                if (nRet == -1)
                {
                    strError = "MatchUserPassword() error: " + strError;
                    return -1;
                }
                Debug.Assert(nRet == 0 || nRet == 1, "");
                if (nRet == 0)  // 2016/12/26 从 == 1 修改为 == 0
                {
                    MessageBox.Show(owner, "密码不正确。请重新输入密码");
                    goto REDO_LOGIN;
                }
            }

            return 1;
        }

        // 在 library.xml 中标记，已经创建过初始书目库了
        // return:
        //      -1  出错
        //      0   成功
        public static int MaskDefaultDatabaseCreated(string strDataDir,
            out string strError)
        {
            strError = "";

            string strFileName = Path.Combine(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFileName + "' 装载到 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null)
            {
                strError = "文件 '" + strFileName + "' 格式不正确，缺乏根元素";
                return -1;
            }

            dom.DocumentElement.SetAttribute("_initialDatabase", "true");
            return 0;
        }

        // 从注册表和 library.xml 文件中获得实例信息
        // parameters:
        //      
        // return:
        //      -1  出错
        //      0   实例没有找到
        //      1   成功
        public static int GetLibraryInstanceInfo(
            string strInstanceNameParam,
            out LibraryInstanceInfo info,
            out string strError)
        {
            strError = "";

            info = new LibraryInstanceInfo();

            string strInstanceName = "";
            string strDataDir = "";
            string strCertificatSN = "";
            string[] existing_urls = null;

            for (int i = 0; ; i++)
            {
                bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertificatSN);
                if (bRet == false)
                {
                    strError = "实例 '" + strInstanceNameParam + "' 不存在";
                    return 0;
                }

                if (strInstanceName == strInstanceNameParam)
                {
                    info.InstanceName = strInstanceName;
                    info.Urls = existing_urls;
                    info.DataDir = strDataDir;
                    break;
                }
            }

            string strFileName = Path.Combine(strDataDir, "library.xml");
            if (File.Exists(strFileName) == false)
            {
                strError = "实例 '" + strInstanceNameParam + "' 的 library.xml 文件不存在";
                return 0;
            }

            return GetLibraryInstanceInfoFromXml(strFileName,
            ref info,
            out strError);
#if NO
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFileName + "' 装载到 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null)
            {
                strError = "文件 '" + strFileName + "' 格式不正确，缺乏根元素";
                return -1;
            }

            info.Version = LibraryServerUtil.GetLibraryXmlVersion(dom);

            // supervisor
            // XmlElement nodeSupervisor = dom.DocumentElement.SelectSingleNode("accounts/account[@type='']") as XmlElement;

            XmlElement nodeSupervisor = null;

            // 找到第一个具备 managedatabase 权限用户
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("accounts/account[@type='']");
            if (nodes.Count > 0)
            {
                foreach (XmlElement account in nodes)
                {
                    string strRights = account.GetAttribute("rights");
                    if (StringUtil.IsInList("managedatabase", strRights) == true)
                    {
                        nodeSupervisor = account;
                        break;
                    }
                }
            }

            if (nodeSupervisor != null)
            {
                info.SupervisorUserName = nodeSupervisor.GetAttribute("name");
                info.SupervisorPassword = nodeSupervisor.GetAttribute("password");

                if (info.Version <= 2.0)
                {
                    // library.xml 2.00 及以前的做法
                    try
                    {
                        info.SupervisorPassword = Cryptography.Decrypt(info.SupervisorPassword, "dp2circulationpassword");
                    }
                    catch
                    {
                        strError = "<account password='???' /> 中的密码不正确";
                        return -1;
                    }
                    // 得到 supervisor 密码的明文
                }
            }

            string strValue = dom.DocumentElement.GetAttribute("_initialDatabase");
            if (DomUtil.IsBooleanTrue(strValue, false) == true)
                info.InitialDatabase = true;
            else
                info.InitialDatabase = false;
            return 1;
#endif
        }

        const string EncryptKey = "dp2circulationpassword";

        // 注意：本函数不会修改传入的 info 中的 InstanceName 成员
        // return:
        //      -1  出错
        //      0   实例没有找到
        //      1   成功
        static int GetLibraryInstanceInfoFromXml(string strFileName,
            ref LibraryInstanceInfo info,
            out string strError)
        {
            strError = "";
            if (info == null)
                info = new LibraryInstanceInfo();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFileName + "' 装载到 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null)
            {
                strError = "文件 '" + strFileName + "' 格式不正确，缺乏根元素";
                return -1;
            }

            info.Dom = dom;

            info.Version = LibraryServerUtil.GetLibraryXmlVersion(dom);

            XmlElement rmsserver = dom.DocumentElement.SelectSingleNode("rmsserver") as XmlElement;
            if (rmsserver != null)
            {
                info.KernelUrl = rmsserver.GetAttribute("url");
                info.KernelUserName = rmsserver.GetAttribute("username");
                info.KernelPassword = rmsserver.GetAttribute("password");

                try
                {
                    info.KernelPassword = Cryptography.Decrypt(
                        info.KernelPassword,
                        EncryptKey);
                }
                catch
                {
                    strError = "<rmsserver> 元素 password 属性中的密码设置不正确";
                    return -1;
                }
            }

            // supervisor
            // XmlElement nodeSupervisor = dom.DocumentElement.SelectSingleNode("accounts/account[@type='']") as XmlElement;

            XmlElement nodeSupervisor = null;

            // TODO: 找到所有具备 managedatabase 权限的用户信息返回
            // 找到第一个具备 managedatabase 权限用户
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("accounts/account[@type='']");
            if (nodes.Count > 0)
            {
                foreach (XmlElement account in nodes)
                {
                    string strRights = account.GetAttribute("rights");
                    if (StringUtil.IsInList("managedatabase", strRights) == true)
                    {
                        nodeSupervisor = account;
                        break;
                    }
                }
            }

            if (nodeSupervisor != null)
            {
                info.SupervisorUserName = nodeSupervisor.GetAttribute("name");
                info.SupervisorPassword = nodeSupervisor.GetAttribute("password");

                if (info.Version <= 2.0)
                {
                    // library.xml 2.00 及以前的做法
                    try
                    {
                        info.SupervisorPassword = Cryptography.Decrypt(info.SupervisorPassword, "dp2circulationpassword");
                    }
                    catch
                    {
                        strError = "<account password='???' /> 中的密码不正确";
                        return -1;
                    }
                    // 得到 supervisor 密码的明文
                }
            }

            string strValue = dom.DocumentElement.GetAttribute("_initialDatabase");
            if (DomUtil.IsBooleanTrue(strValue, false) == true)
                info.InitialDatabase = true;
            else
                info.InitialDatabase = false;
            return 1;
        }

        public class RestoreLibraryParamBase
        {
            public string InstanceName { get; set; }
            public bool StartInstanceOnFinish { get; set; } // 是否要在完成时重新启动实例?

            public string DataDir { get; set; }
            public string BackupFileName { get; set; }
            // 临时文件目录。临时文件目录的根目录。
            public string TempDirRoot { get; set; }
            // 是否为快速模式
            public bool FastMode { get; set; }
            // 错误日志文件名
            public string LogFileName { get; set; }

        }

        public class RestoreLibraryParam : RestoreLibraryParamBase
        {
            public Stop Stop { get; set; }
            // [out]
            public string ErrorInfo { get; set; }
        }

        // 在 dp2library 没有启动的情况下，用大备份文件恢复 dp2library 内全部配置和数据库
        // 本函数不会出现任何窗口，是静默运行的
        public static bool RestoreLibrary(
#if NO
            Stop stop,
            string strDataDir,
            string strBackupFileName,
            string strTempDirRoot,
            bool bFastMode,
            out string strError
#endif
RestoreLibraryParam param
            )
        {
            string strError = "";
            int nRet = 0;

            if (param.Stop != null)
                param.Stop.SetMessage("正在检测数据目录 " + param.DataDir);
            // return:
            //      -1  error
            //      0   数据目录不存在
            //      1   数据目录存在，但是xml文件不存在
            //      2   xml文件已经存在
            nRet = DetectDataDir(param.DataDir,
            out strError);
            if (nRet != 2)
            {
                strError = "在探测现有 library.xml 文件过程中出现错误: " + strError;
                param.ErrorInfo = strError;
                return false;
            }

            string strLibraryXmlFileName = Path.Combine(param.DataDir, "library.xml");

            LibraryInstanceInfo info = null;

            if (param.Stop != null)
                param.Stop.SetMessage("正在从 " + strLibraryXmlFileName + " 获得参数");

            // 从现有 library.xml 中得到各种配置参数
            nRet = GetLibraryInstanceInfoFromXml(strLibraryXmlFileName,
                ref info,
                out strError);
            if (nRet == -1)
            {
                param.ErrorInfo = strError;
                return false;
            }

            if (string.IsNullOrEmpty(info.KernelUrl))
            {
                strError = "library.xml 中尚未配置 dp2Kernel Server URL";
                param.ErrorInfo = strError;
                return false;
            }

            string strMode = "full";
            string strExt = Path.GetExtension(param.BackupFileName);
            if (strExt == ".dp2bak")
                strMode = "full";
            else
                strMode = "blank";

            string strBackupFileName = param.BackupFileName;
            if (strBackupFileName.ToLower().EndsWith(".dbdef.zip"))
                strBackupFileName = strBackupFileName.Substring(0, strBackupFileName.Length - ".dbdef.zip".Length);
            else
                strBackupFileName = Path.Combine(Path.GetDirectoryName(param.BackupFileName),
                Path.GetFileNameWithoutExtension(param.BackupFileName));

            // 数据库定义文件名
            string strDbDefFileName = strBackupFileName + ".dbdef.zip";
            if (File.Exists(strDbDefFileName) == false)
            {
                strError = "数据库定义文件 '" + strDbDefFileName + "' 不存在";
                param.ErrorInfo = strError;
                return false;
            }

            string strDataFileName = strBackupFileName + ".dp2bak";

            if (strMode == "full" && File.Exists(strDataFileName) == false)
            {
                param.ErrorInfo = "全部恢复 方式下，大备份数据文件 '" + strDataFileName + "' 不存在，无法进行恢复";
                return false;
            }

            XmlDocument new_library_dom = new XmlDocument();

            using (RmsChannelCollection channels = new RmsChannelCollection())
            {
                channels.AskAccountInfo += new AskAccountInfoEventHandle((o, e) =>
                {
                    e.UserName = info.KernelUserName;
                    e.Password = info.KernelPassword;

                    e.Result = 1;
                });

                RmsChannel channel = channels.GetChannel(info.KernelUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    param.ErrorInfo = strError;
                    return false;
                }

                if (param.Stop != null)
                    param.Stop.SetMessage("正在删除实例原有的全部数据库 ...");

                // 先删除当前实例的全部数据库
                nRet = DeleteAllDatabases(
                    param.Stop,
                    channel,
                    info.Dom,
                    out strError);
                if (nRet == -1)
                {
                    param.ErrorInfo = strError;
                    return false;
                }

                string strTempDir = Path.Combine(param.TempDirRoot, "def");
                PathUtil.CreateDirIfNeed(strTempDir);
                try
                {
                    nRet = DatabaseUtility.CreateDatabases(
                        param.Stop,
                        channel,
                        strDbDefFileName,
                        strTempDir,
                        out strError);
                    if (nRet == -1)
                    {
                        param.ErrorInfo = strError;
                        return false;
                    }

                    // 装载 library.xml
                    string strNewLibrarXmlFileName = Path.Combine(strTempDir, "_datadir\\library.xml");
                    try
                    {
                        new_library_dom.Load(strNewLibrarXmlFileName);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载文件 " + strNewLibrarXmlFileName + " 到 XMLDOM 时出错: " + ex.Message;
                        param.ErrorInfo = strError;
                        return false;
                    }

                    {
                        if (param.Stop != null)
                            param.Stop.SetMessage("正在修改 library.xml 文件");

                        // 用 .dbdef.zip 中的 library.xml 内容替换当前 library.xml 部分内容
                        XmlDocument target_dom = null;
                        nRet = MergeLibraryXml(info.Dom,
                        new_library_dom,
                        out target_dom,
                        out strError);
                        if (nRet == -1)
                        {
                            param.ErrorInfo = strError;
                            return false;
                        }

                        // TODO: 备份操作前的 library.xml ?
                        target_dom.Save(strLibraryXmlFileName);
                    }

                    // 拷贝 数据目录下的 cfgs 子目录
                    string strSourceCfgsDir = Path.Combine(strTempDir, "_datadir\\cfgs");
                    string strTargetCfgsDir = Path.Combine(param.DataDir, "cfgs");
                    nRet = PathUtil.CopyDirectory(strSourceCfgsDir, strTargetCfgsDir, true, out strError);
                    if (nRet == -1)
                    {
                        param.ErrorInfo = "复制数据目录的 cfgs 子目录(" + strSourceCfgsDir + " --> " + strTargetCfgsDir + ")时出错: " + strError;
                        return false;
                    }

                    // 删除 dp2library 数据目录中所有后台任务的断点信息，以避免克隆后旧的后台任务被从断点位置继续执行
                    // 2017/9/20
                    nRet = DeleteLibraryTempFiles(param.DataDir,
            out strError);
                    if (nRet == -1)
                    {
                        param.ErrorInfo = "删除数据目录的临时文件时出错: " + strError;
                        return false;
                    }
                }
                finally
                {
                    PathUtil.RemoveReadOnlyAttr(strTempDir);    // 避免 .zip 文件中有有只读文件妨碍删除

                    PathUtil.DeleteDirectory(strTempDir);
                }

                if (strMode == "full"
                    && File.Exists(strDataFileName))
                {
                    // 导入 .dp2bak 文件内的全部数据
                    nRet = ImportBackupData(
                        param.Stop,
                        channel,
                        strDataFileName,
                        param.FastMode,
                        out strError);
                    if (nRet == -1)
                    {
                        param.ErrorInfo = strError;
                        return false;
                    }
                }
            }

            return true;
        }

        // 删除 dp2library 数据目录中的临时文件。一般用于恢复了一个实例以后，清除以前实例残留的临时文件
        static int DeleteLibraryTempFiles(string strDataDir,
            out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();

            {
                string strFileName = Path.Combine(strDataDir, "operlog\\spare_operlog.bin");
                if (TryDelete(strFileName, out strError) == false)
                    errors.Add(strError);
            }

            {
                string strLogDir = Path.Combine(strDataDir, "log");
                List<string> filenames = new List<string>();
                DirectoryInfo di = new DirectoryInfo(strLogDir);
                try
                {
                    FileInfo[] fis = di.GetFiles("*_lasttime.txt");
                    foreach (FileInfo fi in fis)
                    {
                        filenames.Add(fi.FullName);
                    }

                    fis = di.GetFiles("*.breakpoint");
                    foreach (FileInfo fi in fis)
                    {
                        filenames.Add(fi.FullName);
                    }

                    foreach (string strFileName in filenames)
                    {
                        if (TryDelete(strFileName, out strError) == false)
                            errors.Add(strError);
                    }
                }
                catch (DirectoryNotFoundException)
                {

                }
                catch (Exception ex)
                {
                    errors.Add("删除目录 '" + strLogDir + "' 中的若干文件时出现异常: " + ExceptionUtil.GetExceptionText(ex));
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors);
                return -1;
            }

            return 0;
        }

        static bool TryDelete(string strFileName, out string strError)
        {
            strError = "";
            try
            {
                File.Delete(strFileName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                return true;
            }
            catch (Exception ex)
            {
                strError = "删除文件 '" + strFileName + "' 时出现异常: " + ex.Message;
                return false;
            }
        }

        // 合并新旧两个 library.xml。
        // 旧的 library.xml 指即将被替换的当前 library.xml
        // 新的 library.xml 指从 .dbdef.zip 中提取出来的 library.xml
        static int MergeLibraryXml(XmlDocument old_dom,
            XmlDocument new_dom,
            out XmlDocument target_dom,
            out string strError)
        {
            strError = "";

            // target_dom 以 new_dom 为基础内容，然后开始修改
            target_dom = new XmlDocument();
            target_dom.LoadXml(new_dom.OuterXml);

            // old_dom:root uid --> target_dom
            CopyAttr(old_dom,
            target_dom,
            ".",
            "uid");

            // old_dom:rmsserver 元素 --> target_dom
            CopyElement(
            old_dom,
            target_dom,
            "rmsserver");

            // old_dom:mongoDB 元素 --> target_dom
            CopyElement(
            old_dom,
            target_dom,
            "mongoDB");

            {
                // TODO: message 元素的 dbname 属性要用新的值
                string strMessageDbName = "";
                XmlNode node = new_dom.DocumentElement.SelectSingleNode("message/@dbname");
                if (node != null)
                    strMessageDbName = node.Value;

                // old_dom:message 元素 --> target_dom
                CopyElement(
                old_dom,
                target_dom,
                "message");


                SetAttr(target_dom,
    "message",
    "dbname",
            strMessageDbName);
            }

            // old_dom:opacServer 元素 --> target_dom
            CopyElement(
            old_dom,
            target_dom,
            "opacServer");

            // 
            // old_dom:externalMessageInterface 元素 --> target_dom
            CopyElement(
            old_dom,
            target_dom,
            "externalMessageInterface");

            return 0;
        }

        static void SetAttr(XmlDocument target_dom,
    string element_name,
    string attr_name,
            string attr_value)
        {
            XmlElement t = target_dom.DocumentElement.SelectSingleNode(element_name) as XmlElement;
            if (t == null)
            {
                if (attr_value == null)
                    return;
                t = target_dom.CreateElement(element_name);
                t.OwnerDocument.DocumentElement.AppendChild(t);
            }

            t.SetAttribute(attr_name, attr_value);
        }

        static void CopyAttr(XmlDocument old_dom,
            XmlDocument target_dom,
            string element_name,
            string attr_name)
        {
            string v = "";
            XmlNode v_node = old_dom.DocumentElement.SelectSingleNode(element_name + "/@" + attr_name);
            if (v_node != null)
                v = v_node.Value;

            XmlElement t = target_dom.DocumentElement.SelectSingleNode(element_name) as XmlElement;
            if (t == null)
            {
                if (v_node == null)
                    return;
                t = target_dom.CreateElement(element_name);
                t.OwnerDocument.DocumentElement.AppendChild(t);
            }

            t.SetAttribute(attr_name, v);
        }

        static void CopyElement(
            XmlDocument old_dom,
            XmlDocument target_dom,
            string element_name)
        {
            XmlElement t = target_dom.DocumentElement.SelectSingleNode(element_name) as XmlElement;
            if (t == null)
            {
                t = target_dom.CreateElement(element_name);
                t.OwnerDocument.DocumentElement.AppendChild(t);
            }

            XmlElement s = old_dom.DocumentElement.SelectSingleNode(element_name) as XmlElement;
            if (s == null)
            {
                t.ParentNode.RemoveChild(t);
                return;
            }

            DomUtil.SetElementOuterXml(t, s.OuterXml);
        }

        // 删除全部数据库
        public static int DeleteAllDatabases(
            Stop stop,
            RmsChannel channel,
            XmlDocument cfg_dom,
            out string strError)
        {
            strError = "";

            List<string> dbnames = new List<string>();

            {
                XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("itemdbgroup/database");
                foreach (XmlElement database in nodes)
                {
                    dbnames.Add(database.GetAttribute("name"));
                    dbnames.Add(database.GetAttribute("biblioDbName"));
                    dbnames.Add(database.GetAttribute("orderDbName"));
                    dbnames.Add(database.GetAttribute("issueDbName"));
                    dbnames.Add(database.GetAttribute("commentDbName"));
                }
            }

            {
                XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("readerdbgroup/database/@name");
                foreach (XmlNode name in nodes)
                {
                    if (string.IsNullOrEmpty(name.Value) == false)
                        dbnames.Add(name.Value);
                }
            }

            {
                XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("message/@dbname | arrived/@dbname | pinyin/@dbname | gcat/@dbname | word/@dbname | amerce/@dbname | invoice/@dbname | utilDb/database/@name");
                foreach (XmlNode name in nodes)
                {
                    if (string.IsNullOrEmpty(name.Value) == false)
                        dbnames.Add(name.Value);
                }
            }

            StringUtil.RemoveBlank(ref dbnames);
            StringUtil.RemoveDupNoSort(ref dbnames);

            int i = 0;
            List<string> errors = new List<string>();
            foreach (string dbname in dbnames)
            {
                if (stop != null)
                    stop.SetMessage("正在删除数据库 " + dbname + " " + (i + 1) + "/" + dbnames.Count);

                long lRet = channel.DoDeleteDB(dbname,
    out strError);
                if (lRet == -1 && channel.ErrorCode != ChannelErrorCode.NotFound)
                {
                    errors.Add("删除数据库 '" + dbname + "' 时发生错误：" + strError);
                }

                i++;
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }

            return 0;
        }

        // 导入 .dp2bak 文件内的全部数据
        static int ImportBackupData(
            Stop stop,
            RmsChannel channel,
            string strBackupFileName,
            bool bFastMode,
            out string strError)
        {
            strError = "";

            int CHUNK_SIZE = 150 * 1024;    // 70

            long lTotalCount = 0;

            List<string> target_dburls = new List<string>();

            ImportUtil import_util = new ImportUtil();
            int nRet = import_util.Begin(null,
                null,   // this.AppInfo,
                strBackupFileName,
                out strError);
            if (nRet == -1 || nRet == 1)
                return -1;

            ProgressEstimate estimate = new ProgressEstimate();

            try // open import util
            {
                bool bDontPromptTimestampMismatchWhenOverwrite = false;
                // DbNameMap map = new DbNameMap();
                long lSaveOffs = -1;

                estimate.SetRange(0, import_util.Stream.Length);
                estimate.StartEstimate();

                if (stop != null)
                    stop.SetProgressRange(0, import_util.Stream.Length);

                List<UploadRecord> records = new List<UploadRecord>();
                int nBatchSize = 0;
                for (int index = 0; ; index++)
                {
                    //Application.DoEvents();	// 出让界面控制权
                    channel.DoIdle();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
#if NO
                        if (stop.State != 0)
                        {
                            DialogResult result = MessageBox.Show(this,
                                "确实要中断当前批处理操作?",
                                "导入数据",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                            else
                            {
                                stop.Continue();
                            }
                        }
#endif

                    UploadRecord record = null;

                    {
                        if (lSaveOffs != -1 && import_util.Stream.Position != lSaveOffs)
                        {
                            StreamUtil.FastSeek(import_util.Stream, lSaveOffs);
                        }
                    }

                    // return:
                    //		-1	出错
                    //		0	正常
                    //		1	结束。此次API不返回有效的记录
                    nRet = import_util.ReadOneRecord(out record,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    {
                        // 保存每次读取后的文件指针位置
                        lSaveOffs = import_util.Stream.Position;
                    }

                    //if (nRet == 1)
                    //    break;
                    bool bNeedPush = false;

                    if (nRet == 0)
                    {
                        Debug.Assert(record != null, "");

                        // 准备目标路径
                        {
                            string strLongPath = channel.Url + "?" + record.RecordBody.Path;

                            ResPath respath = new ResPath(strLongPath);
                            record.Url = respath.Url;
                            record.RecordBody.Path = respath.Path;

                            // 记载每个数据库的 URL
                            string strDbUrl = GetDbUrl(strLongPath);
                            if (target_dburls.IndexOf(strDbUrl) == -1)
                            {
                                // 每个数据库要进行一次快速模式的准备操作
                                if (bFastMode == true)
                                {
                                    nRet = ManageKeysIndex(
                                        channel,
                                        strDbUrl,
                                        "beginfastappend",
                                        "正在对数据库 " + strDbUrl + " 进行快速导入模式的准备工作 ...",
                                        out strError);
                                    if (nRet == -1)
                                        return -1;
                                }
                                target_dburls.Add(strDbUrl);
                            }
                        }

                        // 是否要把积累的记录推送出去进行写入?
                        // 要进行以下检查：
                        // 1) 当前记录和前一条记录之间，更换了服务器
                        // 2) 累积的记录尺寸超过要求
                        // 3) 当前记录是一条超大的记录 (这是因为要保持从文件中读出的顺序来写入(例如追加时候的号码增量顺序)，就必须在单条写入本条前，先写入积累的那些记录)
                        if (records.Count > 0)
                        {
                            if (record.TooLarge() == true)
                                bNeedPush = true;
                            else if (nBatchSize + record.RecordBody.Xml.Length > CHUNK_SIZE)
                                bNeedPush = true;
                            else
                            {
                                if (LastUrl(records) != record.Url)
                                    bNeedPush = true;
                            }
                        }
                    }
                    else
                    {
                        record = null;
                        bNeedPush = true;
                    }

                    if (bNeedPush == true)
                    {
                        List<UploadRecord> save_records = new List<UploadRecord>();
                        save_records.AddRange(records);

                        {
                            while (records.Count > 0)
                            {
                                // 将 XML 记录成批写入数据库
                                // return:
                                //      -1  出错
                                //      >=0 本次已经写入的记录个数。本函数返回时 records 集合的元素数没有变化(但元素的Path和Timestamp会有变化)，如果必要调主可截取records集合中后面未处理的部分再次调用本函数
                                nRet = ImportUtil.WriteRecords(
                                    null,
                                    stop,
                                    channel,
                                    bFastMode,
                                    false,  // dlg.InsertMissing,
                                    records,
                                    ref bDontPromptTimestampMismatchWhenOverwrite,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                if (nRet == 0)
                                {
                                    // TODO: 或可以改为单条写入
                                    strError = "WriteRecords() error :" + strError;
                                    return -1;
                                }
                                Debug.Assert(nRet <= records.Count, "");
                                records.RemoveRange(0, nRet);
                                lTotalCount += nRet;
                            }
                        }

                        // if (dlg.ImportObject)
                        {
                            // 上载对象
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = import_util.UploadObjects(
                                stop,
                                channel,
                                save_records,
                                true,
                                ref bDontPromptTimestampMismatchWhenOverwrite,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        nBatchSize = 0;
                        stop.SetProgressValue(import_util.Stream.Position);

                        stop.SetMessage("已经写入记录 " + lTotalCount.ToString() + " 条。"
+ "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(import_util.Stream.Position)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
                    }

                    // 如果 记录的 XML 尺寸太大不便于成批上载，需要在单独直接上载
                    if (record != null && record.TooLarge() == true)
                    {
                        // if (dlg.ImportDataRecord)
                        {
                            // 写入一条 XML 记录
                            // return:
                            //      -1  出错
                            //      0   邀请中断整个处理
                            //      1   成功
                            //      2   跳过本条，继续处理后面的
                            nRet = ImportUtil.WriteOneXmlRecord(
                                null,
                                stop,
                                channel,
                                record,
                                ref bDontPromptTimestampMismatchWhenOverwrite,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 0)
                                return -1;
                        }

                        // if (dlg.ImportObject)
                        {
                            List<UploadRecord> temp = new List<UploadRecord>
                            {
                                record
                            };
                            // 上载对象
                            // return:
                            //      -1  出错
                            //      0   成功
                            nRet = import_util.UploadObjects(
                                stop,
                                channel,
                                temp,
                                true,
                                ref bDontPromptTimestampMismatchWhenOverwrite,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }

                        lTotalCount += 1;
                        continue;
                    }

                    if (record != null)
                    {
                        records.Add(record);
                        if (record.RecordBody != null && record.RecordBody.Xml != null)
                            nBatchSize += record.RecordBody.Xml.Length;
                    }

                    if (record == null)
                        break;  // 延迟 break
                }

                Debug.Assert(records.Count == 0, "");
#if NO
                // 最后提交一次
                if (records.Count > 0)
                {
                    List<UploadRecord> save_records = new List<UploadRecord>();
                    save_records.AddRange(records);

                    // if (dlg.ImportDataRecord)
                    {
                        while (records.Count > 0)
                        {
                            // 将 XML 记录成批写入数据库
                            // return:
                            //      -1  出错
                            //      >=0 本次已经写入的记录个数。本函数返回时 records 集合的元素数没有变化(但元素的Path和Timestamp会有变化)，如果必要调主可截取records集合中后面未处理的部分再次调用本函数
                            nRet = ImportUtil.WriteRecords(
                                null,
                                null,   // stop,
                                channel,
                                bFastMode,
                                false,  // dlg.InsertMissing,
                                records,
                                ref bDontPromptTimestampMismatchWhenOverwrite,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 0)
                            {
                                strError = "WriteRecords() error :" + strError;
                                goto ERROR1;
                            }
                            Debug.Assert(nRet <= records.Count, "");
                            records.RemoveRange(0, nRet);
                            lTotalCount += nRet;
                        }
                    }

                    // if (dlg.ImportObject)
                    {
                        // 上载对象
                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = import_util.UploadObjects(
                            null,   // stop,
                            channel,
                            save_records,
                            ref bDontPromptTimestampMismatchWhenOverwrite,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    nBatchSize = 0;
#if NO
                        stop.SetProgressValue(import_util.Stream.Position);

                        stop.SetMessage("已经写入记录 " + lTotalCount.ToString() + " 条。"
    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(import_util.Stream.Position)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
#endif
                    records.Clear();
                    nBatchSize = 0;
                }
#endif

                return 0;
            }// close import util
            finally
            {
#if NO
                if (bFastMode == true)
                {
                    foreach (string url in target_dburls)
                    {
                        if (stop != null)
                            stop.SetMessage("正在对数据库 " + url + " 进行快速导入模式的最后收尾工作，请耐心等待 ...");

                        LibraryChannelManager.Log?.Debug($"开始对数据库{url}进行快速导入模式的最后收尾工作");
                        try
                        {
                            // TODO: 在错误日志中记载开始和结束时间
                            // 如果捕获到异常，还要记载一下尚未来得及处理的 url
                            nRet = ManageKeysIndex(
                                channel,
                                url,
                                "endfastappend",
                                "正在对数据库 " + url + " 进行快速导入模式的收尾工作，请耐心等待 ...",
                                out string strQuickModeError);
                            if (nRet == -1)
                                throw new Exception(strQuickModeError);
                        }
                        catch (Exception ex)
                        {
                            LibraryChannelManager.Log?.Debug($"对数据库{url}进行快速导入模式的最后收尾工作阶段出现异常: {ExceptionUtil.GetExceptionText(ex)}\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}");
                            throw new Exception($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}", ex);
                        }
                        finally
                        {
                            LibraryChannelManager.Log?.Debug($"结束对数据库{url}进行快速导入模式的最后收尾工作");
                        }
                    }
                    if (stop != null)
                        stop.SetMessage("");
                }
#endif
                if (bFastMode == true)
                {
                    EndFastAppend(stop,
    channel,
    target_dburls);
                }
                import_util.End();
            }
        }

        static void EndFastAppend(Stop stop,
            RmsChannel channel,
            List<string> target_dburls)
        {
            int nRet = 0;
            foreach (string url in target_dburls)
            {
                if (stop?.State != 0)
                    throw new Exception("快速导入收尾阶段被强行中断，恢复没有完成");

                if (stop != null)
                    stop.SetMessage("正在对数据库 " + url + " 进行快速导入模式的最后收尾工作，请耐心等待 ...");

                LibraryChannelManager.Log?.Debug($"开始对数据库{url}进行快速导入模式的最后收尾工作");
                try
                {
                    // TODO: 在错误日志中记载开始和结束时间
                    // 如果捕获到异常，还要记载一下尚未来得及处理的 url
                    //                  start_endfastappend 启动“结束快速追加”任务。返回 0 表示任务启动并已经完成；返回 1 表示任务启动成功，但还需要后面用探寻功能来观察它是否结束
                    nRet = ManageKeysIndex(
                        channel,
                        url,
                        "start_endfastappend",
                        "正在对数据库 " + url + " 启动快速导入模式的收尾工作，请耐心等待 ...",
                        out string strQuickModeError);
                    if (nRet == -1)
                        throw new Exception(strQuickModeError);
                    if (nRet == 1)
                    {
                        while (true)
                        {
                            if (stop?.State != 0)
                                throw new Exception("快速导入收尾阶段被强行中断，恢复没有完成");

                            //                  detect_endfastappend 探寻任务的状态。返回 0 表示任务尚未结束; 1 表示任务已经结束
                            nRet = ManageKeysIndex(
        channel,
        url,
        "detect_endfastappend",
        "正在对数据库 " + url + " 进行快速导入模式的收尾工作，请耐心等待 ...",
        out strQuickModeError);
                            if (nRet == -1)
                                throw new Exception(strQuickModeError);
                            if (nRet == 1)
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LibraryChannelManager.Log?.Debug($"对数据库{url}进行快速导入模式的最后收尾工作阶段出现异常: {ExceptionUtil.GetExceptionText(ex)}\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}");
                    throw new Exception($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}", ex);
                }
                finally
                {
                    LibraryChannelManager.Log?.Debug($"结束对数据库{url}进行快速导入模式的最后收尾工作");
                }
            }
            if (stop != null)
                stop.SetMessage("");
        }

        static string GetDbUrl(string strLongPath)
        {
            ResPath respath = new ResPath(strLongPath);
            respath.MakeDbName();
            return respath.FullPath;
        }

        // 集合中最后一个元素的 Url
        static string LastUrl(List<UploadRecord> records)
        {
            Debug.Assert(records.Count > 0, "");
            UploadRecord last_record = records[records.Count - 1];
            return last_record.Url;
        }

        static int ManageKeysIndex(
            RmsChannel channel,
    string strDbUrl,
    string strAction,
    string strMessage,
    out string strError)
        {
            strError = "";

            ResPath respath = new ResPath(strDbUrl);

            TimeSpan old_timeout = channel.Timeout;
            if (strAction == "endfastappend")
            {
                // 收尾阶段可能要耗费很长的时间
                channel.Timeout = new TimeSpan(3, 0, 0);
            }

            try
            {
                long lRet = channel.DoRefreshDB(
                    strAction,
                    respath.Path,
                    false,
                    out strError);
                if (lRet == -1)
                {
                    strError = "管理数据库 '" + respath.Path + "' 时出错: " + strError;
                    return -1;
                }
                // 2019/4/27
                return (int)lRet;
            }
            finally
            {
                if (strAction == "endfastappend")
                {
                    channel.Timeout = old_timeout;
                }
            }
        }

    }

    /// <summary>
    /// dp2Library 实例信息
    /// </summary>
    public class LibraryInstanceInfo
    {
        public string InstanceName = "";
        public string[] Urls = null;
        public string DataDir = "";
        public string SupervisorUserName = "";
        public string SupervisorPassword = "";  // 这是 hash 以后的密码，不是明文

        public bool InitialDatabase = false;

        // 2015/5/20
        public double Version = 2.0;    // library.xml 文件的版本

        // 2017/8/27
        public string KernelUrl { get; set; }
        public string KernelUserName { get; set; }
        public string KernelPassword { get; set; }

        // 2017/8/29
        public XmlDocument Dom { get; set; }
    }
}
