using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms
{
    // stopword对应的类
	public class StopwordCfg
	{
		//public XmlDocument dom = null;

		Hashtable tableStopwordTable = new Hashtable();

		public StopwordCfg()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
/*
		public int Initial(string strStopwordFileName,
			out string strError)
		{
			strError = "";

			Debug.Assert(strStopwordFileName != "" && strStopwordFileName != null,"strStopwordFileName参数不能为null或空。");


			if (File.Exists(strStopwordFileName) == false)
			{
				strError = "stopword角色对应的物理文件不存在";
				return -1;
			}
			// 如果stopword文件的内容为空，则没有可去的非用字，正常结束
			StreamReader sw = new StreamReader(strStopwordFileName,Encoding.UTF8);
			string strStopwordText = sw.ReadToEnd();
			sw.Close();
			if (strStopwordText == "")
				return 0;


			dom = new XmlDocument();
			try
			{
				this.dom.Load(strStopwordFileName);
			}
			catch(Exception ex)
			{
				strError = "加载stopword配置文件'" + strStopwordFileName + "'到dom时出错：" + ex.Message;
				return -1;
			}


			string strXpath = "//stopwordTable";

			XmlNodeList nodeListStopwordTable = dom.DocumentElement.SelectNodes(strXpath);
			for(int i=0;i<nodeListStopwordTable.Count;i++)
			{
				XmlNode nodeStopwordTable = nodeListStopwordTable[i];
				
				string strName = DomUtil.GetAttr(nodeStopwordTable,"name");
				StopwordTable stopwordTable = new StopwordTable(nodeStopwordTable);

				if (i == 0)
				{
					this.tableStopwordTable[""] = stopwordTable;
				}
				// 加到Hashtable数组里
				this.tableStopwordTable[strName] = stopwordTable;
			}

			return 0;
		}
*/


        public int Initial(XmlNode nodeRoot,
            out string strError)
        {
            strError = "";

            Debug.Assert(nodeRoot != null, "nodeRoot参数不能为null或空。");


            string strXpath = "//stopwordTable";

            XmlNodeList nodeListStopwordTable = nodeRoot.SelectNodes(strXpath);
            for (int i = 0; i < nodeListStopwordTable.Count; i++)
            {
                XmlNode nodeStopwordTable = nodeListStopwordTable[i];

                string strName = DomUtil.GetAttr(nodeStopwordTable, "name");
                StopwordTable stopwordTable = new StopwordTable(nodeStopwordTable);

                if (i == 0)
                {
                    // 第一个多存储一次，便于将来用空名字值来获得
                    this.tableStopwordTable[""] = stopwordTable;
                }
                // 加到Hashtable数组里
                this.tableStopwordTable[strName] = stopwordTable;
            }

            return 0;
        }
		
		// 对一个字符串数组进行去非用字
		// parameter:
		//		texts	待加工的字符串数组
		//		strStopwordFileName	非字用文件名
		//		strStopwordTable	具体使用非用字哪个表 为""或null表示取第一个表
		//		strError	out 出错信息
		// return:
		//		-1	出错
		//		0	成功
		public int DoStopword(string strStopwordTableName,
			ref List<string> texts,
			out string strError)
		{
			strError = "";

			//if (this.dom == null)
			//	return 0;

			StopwordTable stopwordTable = (StopwordTable)this.tableStopwordTable[strStopwordTableName];
			if (stopwordTable == null)
			{
				strError = "没找到名为'" + strStopwordTableName + "'的<stopwordTable>元素。";
				return -1;
			}

			for(int i=0;i<texts.Count;i++)
			{
				texts[i] = DeleteStopword(texts[i],
					stopwordTable);
			}
			return 0;
		}

		// 对一个字符串删除非用字
		// parameter:
		//		strText	待加工的字符串
		//		aSeparator	间隔符数组
		//		aWord	非用字数组
		// return:
		//		string 去非用字后的字符串
		public string DeleteStopword(string strText,
			StopwordTable stopwordTable)
		{
			// -----------------将间隔符转换为'^'---------------
			string strResult = strText;
			for(int i=0; i< stopwordTable.aSeparator.Count ; i++)
			{
				string strOneSeparator = (string)stopwordTable.aSeparator[i];
				strResult = strResult.Replace(strOneSeparator,"^");
			}
			strResult = "^" + strResult + "^";
	
			//---------------------去非用字------------
			int nPosition;
			int nLength;
			for(int i=0;i<stopwordTable.aWord.Count;i++)
			{
				string strOneWord = (string)stopwordTable.aWord[i];
				int nStart = 0;
				while(true)
				{
                    /*
					nPosition = StringUtil.FindSubstring(strResult,
						strOneWord,
						nStart);
                     * */
                    // 2012/2/20 试图修改，未经测试，速度是否能快点也未知
                    nPosition = strResult.IndexOf(strOneWord,
                        nStart,
                        StringComparison.InvariantCultureIgnoreCase);

					if (nPosition<0)
						break;
					nLength = strOneWord.Length;
					nStart += nLength;
					string strStart = strResult.Substring(nPosition-1,1);
					string strEnd = strResult.Substring(nPosition+nLength,1);
					string strStopwordStart = strOneWord.Substring(0,1);
					string strStopwordEnd = strOneWord.Substring(strOneWord.Length-1);

					if (((strStart == "^") 
						|| (StringUtil.IsChineseChar(strStart) == true) 
						|| (StringUtil.IsChineseChar(strStopwordStart) == true))  && ((strEnd == "^") 
						|| (StringUtil.IsChineseChar(strEnd) == true) 
						|| (StringUtil.IsChineseChar(strStopwordEnd) == true)) )
					{
						strResult = strResult.Remove(nPosition,nLength);

                        // 2013/7/25
                        if (nStart >= nPosition && nStart < nPosition + nLength)
                            nStart = nPosition;
					}
				}
			}
			strResult = strResult.Replace("^","");
			return strResult;
		}

		public int IsInStopword(string strSplitChar,
			string strStopwordTableName,
			out bool bInStopword,
			out string strError)
		{
			bInStopword = false;
			strError = "";

			StopwordTable stopwordTable = (StopwordTable)this.tableStopwordTable[strStopwordTableName];
			if (stopwordTable == null)
			{
				strError = "没找到名为'" + strStopwordTableName + "'的<stopwordTable>元素。";
				return -1;
			}

			foreach(string strSep in stopwordTable.aSeparator)
			{
				if (strSep == strSplitChar)
				{
					bInStopword = true;
					return 0;
				}
			}

			foreach(string strWord in stopwordTable.aWord)
			{
				if (strWord == strSplitChar)
				{
					bInStopword = true;
					return 0;
				}
			}
			return 0;
		}

		
	}

	public class StopwordTable
	{
		public string Name = "";
		public XmlNode m_node = null;
		
		public ArrayList aSeparator = new ArrayList();
		public ArrayList aWord = new ArrayList();

		public StopwordTable(XmlNode node)
		{
			this.Initial(node);

		}

		public void Initial(XmlNode node)
		{
            Debug.Assert(node != null, "Initial()调用错误，node参数值不能为null。");
			this.m_node = node;
			string strName = DomUtil.GetAttr(node,"name");
			this.Name = strName;


			string strXpath = "";
			
			// 获得分隔符数组
			strXpath = "separator/t";
			XmlNodeList listSeparator =	this.m_node.SelectNodes(strXpath);
			foreach(XmlNode nodeSeparator in listSeparator)
			{
				string strText = nodeSeparator.InnerText.Trim();  // 2012/2/16
				if (string.IsNullOrEmpty(strText) == false)
				{
                    // 2017/5/17
                    if (strText == "\\_")   // \_ 被当作 _
                        strText = "_";
					else if (strText == "_")    // _ 被当作空格
						strText = " ";

					this.aSeparator.Add(strText);
				}
			}
	
			// 获得非用字数组
			strXpath = "word/t";
			XmlNodeList listWord = this.m_node.SelectNodes(strXpath);;
			foreach(XmlNode nodeWord in listWord)
			{
				string strText = nodeWord.InnerText.Trim();  // 2012/2/16
				if (string.IsNullOrEmpty(strText) == false)
					this.aWord.Add(strText);
			}	
		}


	}

}
