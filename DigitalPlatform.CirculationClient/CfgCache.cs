using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 配置文件前端缓冲
    /// </summary>
    public class CfgCache
    {
        XmlDocument _dom = null;

        string m_strXmlFileName = "";	// 存储缓冲对照信息的xml文件

        bool m_bChanged = false;

        string m_strTempDir = "";

        bool m_bAutoSave = true;

        public CfgCache()
        {
        }

        // 获得或设置临时文件目录
        // 如果不设置临时文件目录, 则在需要创建临时文件的时候, 自动创建在系统临时文件目录中
        public string TempDir
        {
            get
            {
                return m_strTempDir;
            }
            set
            {
                m_strTempDir = value;
                // 创建目录
                if (m_strTempDir != "")
                    PathUtil.CreateDirIfNeed(m_strTempDir);
            }
        }

        // 是否在修改后立即保存到文件
        public bool InstantSave
        {
            get
            {
                return m_bAutoSave;
            }
            set
            {
                m_bAutoSave = value;
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.IO.IOException
Message: 文件“C:\Documents and Settings\Administrator\dp2Circulation_v2\cfgcache\21.file”正由另一进程使用，因此该进程无法访问此文件。
Stack:
在 System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
在 System.IO.FileStream.Init(String path, FileMode mode, FileAccess access, Int32 rights, Boolean useRights, FileShare share, Int32 bufferSize, FileOptions options, SECURITY_ATTRIBUTES secAttrs, String msgPath, Boolean bFromProxy, Boolean useLongPath)
在 System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access, FileShare share, Int32 bufferSize, FileOptions options)
在 System.IO.File.Create(String path)
在 DigitalPlatform.CirculationClient.CfgCache.NewTempFileName(Boolean bUseMacro)
在 DigitalPlatform.CirculationClient.CfgCache.PrepareLocalFile(String strCfgPath, String& strLocalName)
在 DigitalPlatform.CirculationClient.LibraryChannel.GetRes(Stop stop, CfgCache cache, String strPath, String strStyle, Byte[] remote_timestamp, String& strResult, String& strMetaData, Byte[]& baOutputTimeStamp, String& strOutputResPath, String& strError)
在 dp2Circulation.MainForm.GetCfgFile(LibraryChannel Channel, Stop stop, String strDbName, String strCfgFileName, Byte[] remote_timestamp, String& strContent, Byte[]& baOutputTimestamp, String& strError)
在 dp2Circulation.MainForm.InitialNormalDbProperties(Boolean bPrepareSearch)
在 dp2Circulation.MainForm.InitialProperties(Boolean bFullInitial, Boolean bRestoreLastOpenedWindow)


dp2Circulation 版本: dp2Circulation, Version=2.5.5759.36671, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3
本机 MAC 地址: 00016C4E0ACA 
操作时间 2015/10/10 9:53:43 (Sat, 10 Oct 2015 09:53:43 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * */
        // 获得一个临时文件名
        // 临时文件创建在 m_strTempDir目录中
        string NewTempFileName(bool bUseMacro = true)
        {
            if (m_strTempDir == "")
            {
                // 2007/12/10
                // TODO: 当人工删除了那些.file文件后，GetTempFileName() API就会用到原来被用过的那些编号，造成文件张冠李戴。
                // 建议本类设计一个种子计数器，自行实现获得临时文件的函数
                return Path.GetTempFileName();
            }

            string strFileName = "";
            for (int i = 0; ; i++)
            {
                strFileName = PathUtil.MergePath(m_strTempDir, Convert.ToString(i) + ".file");

                FileInfo fi = new FileInfo(strFileName);
                if (fi.Exists == false)
                {
                    try
                    {
                        // 创建一个0 byte的文件
                        using (FileStream f = File.Create(strFileName))
                        {

                        }
                    }
                    catch
                    {
                        continue;
                    }

                    if (bUseMacro == true)
                        return "%cfgcachedir%/" + PathUtil.PureName(strFileName);
                    return strFileName;
                }
            }
        }

        public int Load(string strXmlFileName,
            out string strError)
        {
            strError = "";
            _dom = new XmlDocument();

            m_strXmlFileName = strXmlFileName;	// 出错后也需要

            try
            {
                _dom.Load(strXmlFileName);
            }
            catch (Exception ex)
            {
                _dom.LoadXml("<root/>");	// 虽然返回出错,但是dom是正确初始化了的
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

        // 升级。清理以前的残余文件
        // 需要在 Load() 以后调用
        public void Upgrade()
        {
            double value = 0;

            if (_dom.DocumentElement != null)
            {
                string strVersion = _dom.DocumentElement.GetAttribute("version");
                if (string.IsNullOrEmpty(strVersion) == false)
                {
                    // 检查最低版本号
                    if (double.TryParse(strVersion, out value) == false)
                        value = 0;
                }
            }

            // 升级到 2
            if (value < 2 && _dom.DocumentElement != null)
            {
                _dom.DocumentElement.SetAttribute("version", "2");
                m_bChanged = true;

                this.ClearCfgCache();
            }
        }

        public void AutoSave()
        {
            if (m_bChanged == false || m_bAutoSave == false)
                return;

            string strError;
            Save(null, out strError);
        }

        // parameters:
        //		strXmlFileName	可以为null
        public int Save(string strXmlFileName,
            out string strError)
        {
            strError = "";

            if (strXmlFileName == null)
                strXmlFileName = m_strXmlFileName;

            if (strXmlFileName == null)
            {
                strError = "m_strXmlFileName尚未初始化...";
                return -1;
            }

            if (_dom != null)
                _dom.Save(strXmlFileName);
            m_bChanged = false;
            return 0;
        }

        // 查找配置文件网络路径所对应的本地文件
        // return:
        //      -1  error
        //		0	not found
        //		1	found
        public int FindLocalFile(string strCfgPath,
            out string strLocalName,
            out string strTimeStamp)
        {
            // 2015/9/9
            if (_dom == null || _dom.DocumentElement == null)
            {
                strLocalName = "";
                strTimeStamp = "";
                return -1;
            }

            strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

            XmlNode node = _dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

            if (node == null)
            {
                strLocalName = "";
                strTimeStamp = "";
                return 0;	// not found
            }

            strLocalName = DomUtil.GetAttr(node, "localname");

            if (string.IsNullOrEmpty(strLocalName) == true)
                goto DELETE;

            // 替换文件名中的宏
            strLocalName = strLocalName.Replace("%cfgcachedir%", this.m_strTempDir);

            // 检查本地文件是否存在
            FileInfo fi = new FileInfo(strLocalName);
            if (fi.Exists == false)
                goto DELETE;

            strTimeStamp = DomUtil.GetAttr(node, "timestamp");
            return 1;

        DELETE:
            strLocalName = "";
            strTimeStamp = "";

            // 删除这个信息不完整的节点
            _dom.DocumentElement.RemoveChild(node);
            m_bChanged = true;
            AutoSave();
            return 0;	// not found
        }

        // 为一个网络路径准备本地文件
        public int PrepareLocalFile(string strCfgPath,
            out string strLocalName)
        {
            strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

            XmlNode node = _dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

            if (node != null)
            {
                // 节点已经存在
                strLocalName = DomUtil.GetAttr(node, "localname");
                Debug.Assert(strLocalName != "", "已经存在的节点中localname属性为空");
            }
            else
            {
                node = _dom.CreateElement("cfg");
                DomUtil.SetAttr(node, "path", strCfgPath);
                strLocalName = NewTempFileName();
                DomUtil.SetAttr(node, "localname", strLocalName);

                node = _dom.DocumentElement.AppendChild(node);
                m_bChanged = true;
                AutoSave();
            }

            // 替换文件名中的宏
            strLocalName = strLocalName.Replace("%cfgcachedir%", this.m_strTempDir);
            return 1;
        }

        // 为已经存在的节点设置时间戳值
        public int SetTimeStamp(string strCfgPath,
            string strTimeStamp,
            out string strError)
        {
            strError = "";

            strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

            XmlNode node = _dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

            if (node == null)
            {
                strError = "属性path值为 '" + strCfgPath + "'的<cfg>元素不存在...";
                return -1;
            }

            DomUtil.SetAttr(node, "timestamp", strTimeStamp);
            m_bChanged = true;
            AutoSave();
            return 0;
        }

        // 清除全部节点
        public void ClearCfgCache()
        {
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes("cfg");

            for (int i = 0; i < nodes.Count; i++)
            {
                string strLocalName = DomUtil.GetAttr(nodes[i], "localname");

                if (string.IsNullOrEmpty(strLocalName) == false)
                {
                    // 替换文件名中的宏
                    strLocalName = strLocalName.Replace("%cfgcachedir%", this.m_strTempDir);
                    try // 2008/3/27
                    {
                        File.Delete(strLocalName);
                    }
                    catch
                    {
                    }
                }
            }

            // 删除所有<cfg>节点
            for (int i = 0; i < nodes.Count; i++)
            {
                _dom.DocumentElement.RemoveChild(nodes[i]);
            }
            m_bChanged = true;
            AutoSave();

            // 2013/4/12
            // 删除 cfgacache 子目录以后重建
            if (string.IsNullOrEmpty(this.m_strTempDir) == false)
            {
                PathUtil.DeleteDirectory(this.m_strTempDir);
                PathUtil.CreateDirIfNeed(this.m_strTempDir);
            }
        }

        public int Delete(string strCfgPath,
            out string strError)
        {
            strError = "";

            strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

            XmlNode node = _dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

            if (node == null)
            {
                strError = "属性path值为 '" + strCfgPath + "'的<cfg>元素不存在...";
                return -1;
            }
            string strLocalName = DomUtil.GetAttr(node, "localname");
            if (string.IsNullOrEmpty(strLocalName) == false)
            {
                // 替换文件名中的宏
                strLocalName = strLocalName.Replace("%cfgcachedir%", this.m_strTempDir);

                File.Delete(strLocalName);
            }

            _dom.DocumentElement.RemoveChild(node);

            m_bChanged = true;
            AutoSave();
            return 0;
        }
    }
}
