using System;
using System.Xml;
using System.IO;

namespace DigitalPlatform.Xml
{
    /*

    <?xml version='1.0' encoding='utf-8' ?>
    <stringtable>
        <s id="1">
            <v lang="zh-CN">中文</v>
            <v lang="en">Chinese</v>
        </s>
        <s id="中文id">
            <v lang="en">Chinese value</v>
            <v lang="zh-CN">中文值</v>

        </s>


    </stringtable>

    */

    /* 后来改为规范的语言表示方法
    <stringtable>
    <!-- /////////////////////////////////// login ////////////////////////////-->

        <s id="用户名">
            <v lang="zh-CN">用户名</v>
            <v lang="en-us">User name</v>
        </s>
        <s id="密码">
            <v lang="zh-CN">密码</v>
            <v lang="en-us">Password</v>
        </s>
    </stringtable>	
    */


    /// <summary>
	/// 多语种字符串对照
	/// </summary>
	public class StringTable
	{
		XmlDocument dom = new XmlDocument();

		public string ContainerElementName = "stringtable";
        public string CurrentLang = "zh-CN"; // 缺省为中文
		public string DefaultValue = "????";
		public bool ThrowException = false;

		public string ItemElementName = "s";
		public string ValueElementName = "v";
		public string IdAttributeName = "id";

		public StringTable()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public StringTable(string strFileName)
		{
			this.dom.PreserveWhitespace = true; //设PreserveWhitespace为true

			dom.Load(strFileName);
		}

		public StringTable(Stream s)
		{
			dom.Load(s);
		}

		// 以指定的语言得到或设置字符串
		public string this[string strID, string strLang]
		{
			get 
			{
				return GetString(strID,
					strLang,
					ThrowException,
					this.DefaultValue);
			}
			set 
			{
			}
		}

		// 以当前语言得到或者设置字符串
		public string this[string strID]
		{
			get 
			{
				return GetString(strID,
					CurrentLang,
					ThrowException,
					this.DefaultValue);
			}
			set 
			{
			}

		}

		// 成对出现的字符串
		public string[] GetStrings(string strLang)
		{
			string xpath = "";

			xpath = "//"
				+ ContainerElementName 
				+ "/"+ItemElementName + "/"
				+ ValueElementName + "[@lang='" + strLang + "']";

			XmlNodeList nodes = dom.DocumentElement.SelectNodes(xpath);

			string [] result = new string [nodes.Count*2];

			for(int i=0;i<nodes.Count;i++)
			{
				result[i*2] = DomUtil.GetAttr(nodes[i].ParentNode, "id");
				result[i*2 + 1] = DomUtil.GetNodeText(nodes[i]);
			}

			return result;
		}

		public string GetString(string strID,
			string strLang,
			bool bThrowException,
			string strDefault)
		{
			XmlNode node = null;

			string xpath = "";
			
			if (strLang == null || strLang == "")
			{
				xpath = "//"
					+ ContainerElementName 
					+ "/" + ItemElementName + "[@"
                    + IdAttributeName + "='" + strID + "']/"
					+ ValueElementName;

				node = dom.DocumentElement.SelectSingleNode(xpath);
			}
			else 
			{
			REDO:
				xpath = "//"
					+ ContainerElementName 
					+ "/"+ItemElementName +"[@"
					+ IdAttributeName + "='" + strID + "']/"
					+ ValueElementName + "[@lang='" +strLang + "']";

				node = dom.DocumentElement.SelectSingleNode(xpath);

				// 任延华加
				if (node == null)
				{
					int nIndex = strLang.IndexOf('-');
					if (nIndex != -1)
					{
						strLang = strLang.Substring(nIndex+1);
						goto REDO;
					}
				}
			}
			if (node == null) 
			{
				if (bThrowException)
					throw(new StringNotFoundException("id为" +strID+ "lang为"+strLang+"的字符串没有找到"));

				if (strDefault == "@id")
					return strID;

				return strDefault;
			}

			return DomUtil.GetNodeText(node);
		}


	}

	// 字符串在对照表中没有找到
	public class StringNotFoundException : Exception
	{
		public StringNotFoundException (string s) : base(s)
		{
		}
	}
}
