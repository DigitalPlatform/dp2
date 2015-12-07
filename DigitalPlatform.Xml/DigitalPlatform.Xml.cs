using System;
using System.Collections;

using System.IO;
using System.Text;

using System.Xml;
using System.Text.RegularExpressions;

namespace DigitalPlatform.Xml
{	
	public class Ns
	{
		public const string dc = "http://purl.org/dc/elements/1.1/";
		public const string xlink = "http://www.w3.org/1999/xlink";
		public const string xml = "http://www.w3.org/XML/1998/namespace";
        public const string usmarcxml = "http://www.loc.gov/MARC21/slim";
		public const string unimarcxml = "http://dp2003.com/UNIMARC";
	}

	public class DpNs
	{
		public const string dprms = "http://dp2003.com/dprms";
		public const string dpdc = "http://dp2003.com/dpdc";
		public const string unimarcxml = "http://dp2003.com/UNIMARC";
	}

	// 方便将字符串中的xml敏感字符转换为实体方式.
	// 本来可以用类中static函数实现功能, 但是考虑到每次函数操作时,
	// new XmlTextWriter和StringWriter对象耗费时间和资源,
	// 因此,本类设计成对象内包含上述2对象,在使用中,建议实例化
	// 本类对象后, 长久保留, 只需用WriteString()+GetString()就可实现
	// 所需功能。
	// 反过来说，如果每次使用都是实例化本类对象然后立即销毁，本类的优势
	// 就无法体现了。可以考虑另行设计类中static函数实现同样功能。
	public class XmlStringWriter
	{
		public XmlTextWriter _xmlTextWriter = null;
		public TextWriter _textWrite = null;

		public XmlStringWriter()
		{
			ClearTextWriter();
		}

		public void ClearTextWriter()
		{
            this.Close();

			_textWrite = new StringWriter ();
			_xmlTextWriter = new XmlTextWriter(_textWrite);
			//xmlTextWriter.Formatting = Formatting.Indented ;
		}

		public void WriteElement(string strElementName,string strText)
		{
			_xmlTextWriter.WriteStartElement (strElementName);

			_xmlTextWriter.WriteString (strText);

			_xmlTextWriter.WriteEndElement ();
		}

		public string GetString(string strText)
		{
			_xmlTextWriter.WriteString (strText);
			return _textWrite.ToString ();
		}

		public void WriteString(string strText)
		{
			_xmlTextWriter.WriteString (strText);
		}

		public string GetString()
		{
			return _textWrite.ToString ();
		}

		public void FreeTextWrite()
		{
#if NO
			_textWrite = null;
			_xmlTextWriter = null;
#endif
            this.Close();
		}

        public void Close()
        {
            if (_xmlTextWriter != null)
            {
                _xmlTextWriter.Close();
                _xmlTextWriter = null;
            }
            
            if (_textWrite != null)
            {
                _textWrite.Close();
                _textWrite = null;
            }
        }
	}
	//

	// 设计意图:用于存放命名空间
	// 内部包含一个XmlNamespacemanager成员，用数据dom来创建
	// 对于单独的数据dom，把数据dom包含的命名空间找到，并加到m_nsmgr里
	// 对于配置文件，还需在使用时传入一个数据dom，才会创建m_nsmgr，并把配置dom命名空间找到，加到m_nsmgr里
	// 外部在SelectNodes()或SelectSingleNode()时，只需用使用该对象的m_nsmgr即可。
	public class PrefixURIColl : ArrayList
	{
		public XmlNamespaceManager nsmgr = null;

		public int nSeed = 1;

		#region 构造函数

		//strDataFileName:数据文件名
		public PrefixURIColl(string strDataFileName)
		{
			CreateNSOfData(strDataFileName);
		}
		//dom_data:数据dom
		public PrefixURIColl(XmlDocument dom_data)
		{
			CreateNSOfData(dom_data);
		}

		//dom_data:数据dom
		//dom_cfg:配置dom
		public PrefixURIColl(string strDataFileName,
			string strCfgFileName)
		{
			CreateNSOfCfg(strDataFileName,
				strCfgFileName);
		}

		//dom_data:数据dom
		//dom_cfg:配置dom
		public PrefixURIColl(XmlDocument domData,
			XmlDocument domCfg)
		{
			CreateNSOfCfg(domData,domCfg);
		}

		public PrefixURIColl(XmlDocument domData,
			string strCfgFileName)
		{
			CreateNSOfCfg(domData,strCfgFileName);
		}

		#endregion 

		#region 创建函数

		//单独的数据dom
		public void CreateNSOfData(string strDataFileName)
		{
			XmlDocument domData = new XmlDocument ();
			try
			{
				domData.Load(strDataFileName);
			}
			catch(Exception ex)
			{
				throw(new Exception ("CreateNSOfData()里，加载dom不合法" + ex.Message));
			}
			CreateNSOfData(domData);
		}
		public void CreateNSOfData(XmlDocument domData)
		{
			XmlNode root = domData.DocumentElement ;
			AddNS(root);
			this.Sort ();
			this.DumpRep ();

			//if (this.Count > 0)
			//{
			this.nsmgr = new XmlNamespaceManager (domData.NameTable );
			Add2nsmgr();
			//}
		}

		public void AddNS(XmlNode node)
		{
			if (node.NodeType != XmlNodeType.Element )
				return;

			PrefixURI prefixUri= new PrefixURI ();
			prefixUri.strPrefix = node.Prefix ;
			prefixUri.strURI = node.NamespaceURI ;

			prefixUri.strNodeName = node.Name ;

			if (prefixUri.strNodeName != ""
				&& prefixUri.strURI != "")
			{
				if (prefixUri.strPrefix == "")
				{
					prefixUri.strPrefix = "__pub" + Convert.ToString (nSeed);
					nSeed++;
				}
				this.Add (prefixUri);
			}

			foreach(XmlNode child in node.ChildNodes )
			{
				AddNS(child);
			}
		}

		//对于配置文件
		public void CreateNSOfCfg(string strDataFileName,
			string strCfgFileName)
		{
			XmlDocument domData = new XmlDocument ();
			try
			{
				domData.Load(strDataFileName);
			}
			catch(Exception ex)
			{
				throw(new Exception ("CreateNSOfCfg()里，加载数据dom不合法" + ex.Message));
			}

			CreateNSOfCfg(domData,strCfgFileName);
		}

		public void CreateNSOfCfg(XmlDocument domData,
			string strCfgFileName)
		{
			XmlDocument domCfg = new XmlDocument ();
			try
			{
				domCfg.Load(strCfgFileName);
			}
			catch(Exception ex1)
			{
				throw(new Exception ("CreateNSOfCfg()里，加载配置dom不合法" + ex1.Message));
			}
			CreateNSOfCfg(domData,domCfg);
		}

		public void CreateNSOfCfg(XmlDocument domData,
			XmlDocument domCfg)
		{
			XmlNodeList nsitemList = domCfg.DocumentElement.SelectNodes ("/root/nstable/item");
			foreach(XmlNode nsitemNode in nsitemList)
			{
				XmlNode nsNode = nsitemNode.SelectSingleNode ("nameSpace");
				XmlNode prefixNode = nsitemNode.SelectSingleNode ("prefix");

				PrefixURI prefixUri = new PrefixURI();
                if (prefixNode != null)
				    prefixUri.strPrefix = DomUtil.GetNodeText(prefixNode);
                if (nsNode != null)
				    prefixUri.strURI  = DomUtil.GetNodeText(nsNode);
				
				if (prefixUri.strPrefix != ""
					&& prefixUri.strURI != "")  //在配置文件里不允许前缀为空
				{
					this.Add (prefixUri);
				}
			}

			this.Sort ();
			this.DumpRep ();

			//if (this.Count > 0)
			//{
			this.nsmgr = new XmlNamespaceManager (domData.NameTable );
			Add2nsmgr();
			//}
		}


		#endregion


		//将本集合的值对加到nsmgr里
		public void Add2nsmgr()
		{
			foreach(PrefixURI ns in this)
			{
				this.nsmgr.AddNamespace (ns.strPrefix ,ns.strURI );
			}
		}

		//去重
		public void DumpRep()
		{
			int i,j;
			for(i=0;i<this.Count ;i++)
			{
				PrefixURI prefixUri1 = (PrefixURI)this[i];
				for(j=i+1;j<this.Count;j++)
				{
					PrefixURI prefixUri2 = (PrefixURI)this[j];
					if (prefixUri1.strPrefix == prefixUri2.strPrefix 
						&& prefixUri1.strURI == prefixUri2.strURI )
					{
						j--;
						this.Remove (prefixUri2);
					}
	
				}
			}
		}
		
		//信息函数
		public string Dump()
		{
			string strInfo = "";

			//strInfo += this.m_nsmgr .LookupNamespace("pub") + "\r\n";
			foreach(PrefixURI ns in this)
			{
				strInfo += "strPrefix:" + ns.strPrefix + "---" + "strURI:" + ns.strURI + "---" + "strNodeName:" + ns.strNodeName + "\r\n";
			}

			return strInfo;
		}
	}

	// 设计意图:存放命令空间对
	// 编写者：任延华
	public class PrefixURI:IComparable
	{
		public string strPrefix;  //前缀
		public string strURI;     //URI
		public string strNodeName;

		//隐式执行，可能直接通过DpKey的对象实例来访问
		//obj: 比较的对象
		//0表示相等，其它表示不等
		public int CompareTo(object obj)
		{
			PrefixURI prefixURI = (PrefixURI)obj;
			return String.Compare(this.strPrefix,prefixURI.strPrefix );
		}
	}



}	
