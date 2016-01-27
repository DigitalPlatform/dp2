
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

//using DigitalPlatform.CirculationClient;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

namespace DigitalPlatform.LibraryServer
{
    public class LibraryInstallHelper
    {
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
                return 0;

            string strExistingLibraryFileName = Path.Combine(strDataDir,
                "library.xml");
            if (File.Exists(strExistingLibraryFileName) == true)
                return 2;

            return 1;
        }

        // 要求操作者用 supervisor 账号登录一次。以便后续进行各种重要操作。
        // 只需要 library.xml 即可，不需要 dp2library 在运行中。
        // return:
        //      -2  实例没有找到
        //      -1  出错
        //      0   放弃验证
        //      1   成功
        public static int LibrarySupervisorLogin(IWin32Window owner,
            string strInstanceName,
            string strComment,
            out string strError)
        {
            strError = "";

            LibraryInstanceInfo info = null;

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
                nRet = LibraryServerUtil.MatchUserPassword(dlg.Password,
                    info.SupervisorPassword,
                    out strError);
                if (nRet == -1)
                {
                    strError = "MatchUserPassword() error: " + strError;
                    return -1;
                }
                Debug.Assert(nRet == 0 || nRet == 1, "");
                if (nRet == 1)
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
    }
}
