using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// 文件版本管理器
    /// 保存文件的版本信息
    /// </summary>
    public class FileVersionManager
    {
        XmlDocument dom = null;

        string m_strXmlFileName = "";	// 存储缓冲对照信息的xml文件

        bool m_bChanged = false;

        public void Load(string strXmlFileName)
        {
            string strError = "";
            dom = new XmlDocument();

            m_strXmlFileName = strXmlFileName;	// 出错后也需要

            try
            {
                dom.Load(strXmlFileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                dom.LoadXml("<root/>");	// 虽然返回出错,但是dom是正确初始化了的
                m_bChanged = true;
                return;
            }

            Debug.Assert(dom.DocumentElement != null, "");
            return;
        }

        public void AutoSave()
        {
            if (m_bChanged == false)
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

            dom.Save(strXmlFileName);
            m_bChanged = false;

            return 0;
        }

        // 查找配置文件网络路径所对应的本地文件
        // return:
        //		0	not found
        //		1	found
        public int GetFileVersion(string strFilePath,
            out string strTimeStamp)
        {
            strFilePath = strFilePath.ToLower();	// 导致大小写不敏感
            strFilePath = strFilePath.Replace("/", "\\");

            XmlElement file_node = (XmlElement)dom.DocumentElement.SelectSingleNode("file[@path='" + strFilePath + "']");
            if (file_node == null)
            {
                strTimeStamp = "";
                return 0;	// not found
            }

            strTimeStamp = file_node.GetAttribute("timestamp");

            return 1;
        }

        public void SetFileVersion(string strFilePath,
            string strTimeStamp)
        {
            strFilePath = strFilePath.ToLower();	// 导致大小写不敏感
            strFilePath = strFilePath.Replace("/", "\\");

            XmlElement file_node = (XmlElement)dom.DocumentElement.SelectSingleNode("file[@path='" + strFilePath + "']");
            if (file_node == null)
            {
                file_node = dom.CreateElement("file");
                dom.DocumentElement.AppendChild(file_node);
                file_node.SetAttribute("path", strFilePath);
            }

            file_node.SetAttribute("timestamp", strTimeStamp);
            m_bChanged = true;
        }
    }

}
