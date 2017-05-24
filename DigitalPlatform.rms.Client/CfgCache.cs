using System;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// 配置文件前端缓冲
	/// </summary>
	public class CfgCache
	{
		XmlDocument dom = null;

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
					PathUtil.TryCreateDir(m_strTempDir);
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

		// 获得一个临时文件名
		// 临时文件创建在 m_strTempDir目录中
		string NewTempFileName()
		{
			if (m_strTempDir == "")
				return Path.GetTempFileName();

			string strFileName = "";
			for(int i=0; ; i++) 
			{
				strFileName = PathUtil.MergePath(m_strTempDir, Convert.ToString(i) + ".tmp");

				FileInfo fi = new FileInfo(strFileName);
				if (fi.Exists == false) 
				{
					// 创建一个0 byte的文件
					using(FileStream f = File.Create(strFileName))
                    {

                    }
					return strFileName;
				}
			}
		}

		public int Load(string strXmlFileName,
			out string strError)
		{
			strError = "";
			dom = new XmlDocument();

			m_strXmlFileName = strXmlFileName;	// 出错后也需要

			try 
			{
				dom.Load(strXmlFileName);
			}
			catch (Exception ex)
			{
                strError = ExceptionUtil.GetAutoText(ex);
				dom.LoadXml("<root/>");	// 虽然返回出错,但是dom是正确初始化了的
				return -1;
			}



			return 0;
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

			dom.Save(strXmlFileName);
			m_bChanged = false;

			return 0;
		}

		// 查找配置文件网络路径所对应的本地文件
		// return:
		//		0	not found
		//		1	found
		public int FindLocalFile(string strCfgPath,
			out string strLocalName,
			out string strTimeStamp)
		{
			strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

			XmlNode node = dom.DocumentElement.SelectSingleNode("cfg[@path='" +strCfgPath+ "']");

			if (node == null) 
			{
				strLocalName = "";
				strTimeStamp = "";

				return 0;	// not found
			}

			strLocalName = DomUtil.GetAttr(node, "localname");

			if (strLocalName == "")
				goto DELETE;

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
			dom.DocumentElement.RemoveChild(node);
			m_bChanged = true;
			AutoSave();
			return 0;	// not found

		}

		// 为一个网络路径准备本地文件
		public int PrepareLocalFile(string strCfgPath,
			out string strLocalName)
		{
			strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

			XmlNode node = dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

			if (node != null)
			{
				// 节点已经存在
				strLocalName = DomUtil.GetAttr(node, "localname");
				Debug.Assert(strLocalName != "", "已经存在的节点中localname属性为空");
			}
			else
			{
				node = dom.CreateElement("cfg");
				DomUtil.SetAttr(node, "path", strCfgPath);
				strLocalName = NewTempFileName();
				DomUtil.SetAttr(node, "localname", strLocalName);

				node = dom.DocumentElement.AppendChild(node);
				m_bChanged = true;
				AutoSave();
			}

			return 1;
		}

		// 为已经存在的节点设置时间戳值
		public int SetTimeStamp(string strCfgPath,
			string strTimeStamp,
			out string strError)
		{
			strError = "";

			strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感

			XmlNode node = dom.DocumentElement.SelectSingleNode("cfg[@path='" + strCfgPath + "']");

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
		public void Clear()
		{
			XmlNodeList nodes = dom.DocumentElement.SelectNodes("cfg");

			for(int i=0;i<nodes.Count;i++)
			{
				string strLocalName = DomUtil.GetAttr(nodes[i], "localname");

				if (strLocalName != "")
				{
					File.Delete(strLocalName);
				}
			}

			// 删除所有<cfg>节点
			for(int i=0;i<nodes.Count;i++)
			{
				dom.DocumentElement.RemoveChild(nodes[i]);
			}
			m_bChanged = true;
			AutoSave();
		}

		public int Delete(string strCfgPath,
			out string strError)
		{
			strError = "";

			strCfgPath = strCfgPath.ToLower();	// 导致大小写不敏感


			XmlNode node = dom.DocumentElement.SelectSingleNode("cfg[@path='" +strCfgPath+ "']");

			if (node == null)
			{
				strError = "属性path值为 '" + strCfgPath + "'的<cfg>元素不存在...";
				return -1;
			}
			string strLocalName = DomUtil.GetAttr(node, "localname");

			if (strLocalName != "")
			{
				File.Delete(strLocalName);
			}
			dom.DocumentElement.RemoveChild(node);

			m_bChanged = true;
			AutoSave();
			return 0;
		}
	}
}
