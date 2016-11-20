using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace dp2ZServer.Install
{
    public class InstallZServerUtil
    {
        static string EncryptKey = "dp2zserver_password_key";

        public static string DecryptPasssword(string strEncryptedText)
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

        public static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        // 从XML文件中装载已有的信息到对话框
        // return:
        //      -1  error
        //      0   not load
        //      1   loaded
        public static int LoadInfoFromDp2zserverXmlFile(
            string strXmlFileName,
            InstallZServerDlg dlg,
            // string strRootDir,
            out string strError)
        {
            strError = "";

            //string strXmlFileName = PathUtil.MergePath(strRootDir, "dp2zserver.xml");
            //string strOriginXmlFileName = PathUtil.MergePath(strRootDir, "~dp2zserver.xml");

            XmlDocument dom = new XmlDocument();

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
                strError = "XML文件 " + strXmlFileName + " 装载到 XMLDOM 时发生错误: " + ex.Message;
                return -1;
            }

            XmlElement node = dom.DocumentElement.SelectSingleNode("libraryserver") as XmlElement;

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                // strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。";
                strError = "配置文件中缺乏 libraryserver 元素";
                return -1;
            }

            Debug.Assert(node != null, "");

            string strUserName = node.GetAttribute("username");
            string strPassword = node.GetAttribute("password");
            strPassword = DecryptPasssword(strPassword);

            string strAnonymousUserName = node.GetAttribute("anonymousUserName");
            string strAnonymousPassword = node.GetAttribute("anonymousPassword");
            strAnonymousPassword = DecryptPasssword(strAnonymousPassword);

            string strUrl = node.GetAttribute("url");

            dlg.UserName = strUserName;
            dlg.Password = strPassword;
            dlg.AnonymousUserName = strAnonymousUserName;
            dlg.AnonymousPassword = strAnonymousPassword;

            if (String.IsNullOrEmpty(strUrl) == false)
                dlg.LibraryWsUrl = strUrl;

            XmlElement databases = dom.DocumentElement.SelectSingleNode("databases") as XmlElement;
            if (databases != null)
            {
                dlg.DatabasesXml = databases.OuterXml;
                dlg.MaxResultCount = databases.GetAttribute("maxResultCount");
                if (string.IsNullOrEmpty(dlg.MaxResultCount))
                    dlg.MaxResultCount = "-1";
            }

            XmlElement network = dom.DocumentElement.SelectSingleNode("network") as XmlElement;
            if (network != null)
            {
                string strPort = network.GetAttribute("port");
                int port = 210;
                if (string.IsNullOrEmpty(strPort) == false)
                    Int32.TryParse(strPort, out port);
                dlg.Port = port;

                dlg.MaxSessions = network.GetAttribute("maxSessions");
                if (string.IsNullOrEmpty(dlg.MaxSessions))
                    dlg.MaxSessions = "-1";
            }
            return 1;
        }

        // 写入dp2zserver.xml文件
        // return:
        //      -1  error, install faild
        //      0   succeed
        public static int WriteDp2zserverXmlFile(
            InstallZServerDlg dlg,
            string strXmlFileName,
            out string strError)
        {
            strError = "";

            //string strXmlFileName = PathUtil.MergePath(strRootDir, "dp2zserver.xml");
            //string strOriginXmlFileName = PathUtil.MergePath(strRootDir, "~dp2zserver.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root><libraryserver /></root>");
                // bExist = false;
            }
            catch (Exception ex)
            {
                strError = "XML文件 " + strXmlFileName + " 装载到XMLDOM时发生错误: " + ex.Message + "。安装的最后配置无法完成。";
                return -1;
            }

            XmlElement node = dom.DocumentElement.SelectSingleNode("libraryserver") as XmlElement;

            // 万一已经存在的文件是不正确的?
            if (node == null)
            {
                // strError = "安装前已经存在的文件 " + strTemp + " 格式不正确。";
                strError = "配置文件中缺乏 libraryserver 元素";
                return -1;
                /*
                dom.LoadXml("<root><libraryserver /></root>");
                bExist = false;
                XmlNode node = dom.DocumentElement.SelectSingleNode("libraryserver");
                 * */
            }

            Debug.Assert(node != null, "");

            node.SetAttribute("url", dlg.LibraryWsUrl);

            node.SetAttribute("username", dlg.UserName);
            node.SetAttribute("password", EncryptPassword(dlg.Password));

            node.SetAttribute("anonymousUserName",
                String.IsNullOrEmpty(dlg.AnonymousUserName) == true ? null : dlg.AnonymousUserName);

            if (String.IsNullOrEmpty(dlg.AnonymousUserName) == true)
                node.RemoveAttribute("anonymousPassword");
            else
                node.SetAttribute("anonymousPassword", EncryptPassword(dlg.AnonymousPassword));

            XmlElement databases = dom.DocumentElement.SelectSingleNode("databases") as XmlElement;
            if (databases == null)
            {
                databases = dom.CreateElement("databases");
                dom.DocumentElement.AppendChild(databases);
            }

            databases = DomUtil.SetElementOuterXml(databases, dlg.DatabasesXml) as XmlElement;
            if (dlg.MaxResultCount != "-1")
                databases.SetAttribute("maxResultCount", string.IsNullOrEmpty(dlg.MaxResultCount) ? "-1" : dlg.MaxResultCount);

            XmlElement network = dom.DocumentElement.SelectSingleNode("network") as XmlElement;
            if (network == null)
            {
                network = dom.CreateElement("network");
                dom.DocumentElement.AppendChild(network);
            }

            network.SetAttribute("port", dlg.Port.ToString());
            network.SetAttribute("maxSessions", string.IsNullOrEmpty(dlg.MaxSessions) ? "-1" : dlg.MaxSessions);

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
    }
}
