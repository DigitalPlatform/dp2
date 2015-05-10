using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;

namespace dp2ZServer
{
    public class UcApplication
    {
        public string DataDir = "";
        public string WsUrl = "";
        public string ManagerUserName = "";
        public string ManagerPassword = "";

        const string EncryptKey = "dp2zserver_password_key";


        // 把错误信息写到日志文件里
        public void WriteErrorLog(string strText,
            EventLogEntryType type = EventLogEntryType.Error)
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2ZServer";
            Log.WriteEntry(strText, EventLogEntryType.Error);
        }


        // parameters:
        //		strDataDir	数据目录
        //		strError	out参数，返回出错信息
        // return:
        //		-1	error
        //		0	successed
        public int Initial(string strDataDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.DataDir = strDataDir;

            this.WriteErrorLog("UnionCatalogServer 开始初始化", EventLogEntryType.Information);

            string strFilename = PathUtil.MergePath(strDataDir, "unioncatalog.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "file '" + strFilename + "' not found ...";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "装载配置文件-- '" + strFilename + "' 时发生错误，错误类型：" + ex.GetType().ToString() + "，原因：" + ex.Message;
                goto ERROR1;
            }

            // 内核参数
            // 元素<libraryServer>
            // 属性url/username/password
            XmlNode node = dom.DocumentElement.SelectSingleNode("//libraryServer");
            if (node != null)
            {
                this.WsUrl = DomUtil.GetAttr(node, "url");

                this.ManagerUserName = DomUtil.GetAttr(node,
                    "username");

                try
                {
                    this.ManagerPassword = Cryptography.Decrypt(
                        DomUtil.GetAttr(node, "password"),
                        EncryptKey);
                }
                catch
                {
                    strError = "<libraryServer>元素password属性中的密码设置不正确";
                    // throw new Exception();
                    goto ERROR1;
                }

            }
            else
            {
                strError = "<libraryServer>元素尚未配置";
                goto ERROR1;
            }

            this.WriteErrorLog("UnionCatalogServer 成功初始化。", EventLogEntryType.Information);

            return 0;
        ERROR1:
            this.WriteErrorLog(strError);
            return -1;
        }


        public void Close()
        {
        }

    }
}
