using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Marc
{
	public class MarcXmlWriter 
	{
		public bool WriteMarcPrefix = true;  //是否写前缀
		public bool WriteXsi = false;         //是否写xsi
		public string MarcPrefix = "marc";    //前缀
		public string MarcNameSpaceUri = DigitalPlatform.Xml.Ns.unimarcxml;	// "http://www.loc.gov/MARC21/slim"; //marc的命名空间

		private XmlTextWriter writer = null;   //XmlTextWriter对象
		public Formatting m_Formatting = Formatting.None; //格式
		public int m_Indentation  = 2;  //缩进量


		public MarcXmlWriter()
		{
		}

		public MarcXmlWriter(Stream w,	
			Encoding encoding)// : base(w, encoding)
		{
			writer = new XmlTextWriter(w, encoding);
		}

		public MarcXmlWriter(string filename,
			Encoding encoding)// : base(filename, encoding)
		{
			writer = new XmlTextWriter(filename, encoding);
		}

		/*
		public void Test()
		{
			writer.BaseStream.Seek(999*1024*1024, SeekOrigin.Current);

		}
		*/

		//关闭write
		public void Close()
		{
			if (writer != null)
				writer.Close();
		}

		public void Flush()
		{
			if (writer != null)
				writer.Flush();
		}

		public Formatting Formatting
		{
			get 
			{
				return m_Formatting;
			}
			set
			{
				m_Formatting = value;
				if (writer != null) 
				{
					writer.Formatting = value;
				}
			}
		}

		public int Indentation
		{
			get
			{
				return m_Indentation;
			}
			set
			{
				m_Indentation = value;
				if (writer != null)
				{
					writer.Indentation = m_Indentation;
				}
			}
		}

		//写开头，包括:
		//<? xml version='1.0' encoding='utf-8'?>
		//collection根元素，及根据情况判断是否带命名空间
		public int WriteBegin()
		{
			writer.WriteStartDocument();

			if (WriteMarcPrefix == false)
				writer.WriteStartElement("", "collection", MarcNameSpaceUri);
			else
				writer.WriteStartElement(MarcPrefix,
					"collection", MarcNameSpaceUri);

            // dprms名字空间 2010/11/15
            writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);


			if (WriteXsi == true) 
			{
                /* 2010/10/28
                writer.WriteAttributeString("xmlns:xsi",
					"http://www.w3.org/2001/XMLSchema-instance");
                 * 上面用法有问题。会造成大量重复的nsuri存在。应该采用下面的用法：
                    writer.WriteAttributeString("xmlns", "dc", null,
    "http://purl.org/dc/elements/1.1/");
                 * */
                writer.WriteAttributeString("xmlns","xsi",null,
					"http://www.w3.org/2001/XMLSchema-instance");
				writer.WriteAttributeString("xsi","schemaLocation",null,
					"http://www.loc.gov/MARC21/slim http://www.loc.gov/standards/marcxml/schema/MARC21slim.xsd");
			}
			return 0;
		}

		//关闭collection
		public int WriteEnd()
		{
			writer.WriteEndElement();
			return 0;
		}

		public int WriteRecord(
			string strMARC,
			out string strError)
		{
			strError = "";
			int nRet = 0;
			string [] saField = null;
			nRet = MarcUtil.ConvertMarcToFieldArray(strMARC,
				out saField,
				out strError);
			if (nRet == -1)
				return -1;
			return WriteRecord(
				saField,
				out strError);
		}

		// return:
		//		0	成功
		//		-1	出错
		public int WriteRecord(
			string[] saField,
			out string strError)
		{
			string strFieldName = null;
			int nRet;

			strError = "";

			long lStart = writer.BaseStream.Position;
			Debug.Assert(writer.BaseStream.CanSeek == true, "writer.BaseStream.CanSeek != true");

			//try 
			//{
			//根据WriteMarcPrefix的值，确定是否对元素record加命名空间
			if (WriteMarcPrefix == false)
				writer.WriteStartElement("record");
			else
				writer.WriteStartElement(MarcPrefix,
					"record", MarcNameSpaceUri);

            if (String.IsNullOrEmpty(writer.LookupPrefix("dprms")) == true)
            {
                // dprms名字空间 2010/11/15
                writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
            }

			//循环，写头标区及每个子段
			for(int i=0;i<saField.Length;i++) 
			{
				string strLine = saField[i];
				string strInd1 = null;
				string strInd2 = null;
				string strContent = null;

				// 头标区
				if (i == 0) 
				{
					//多截少添
					if (strLine.Length > 24)
						strLine = strLine.Substring(0,24);
					else 
					{
						while(strLine.Length < 24 ) 
						{
							strLine += " ";
						}
					}

					if (WriteMarcPrefix == false)
						writer.WriteElementString("leader", strLine);
					else
						writer.WriteElementString("leader", MarcNameSpaceUri, strLine);

					continue;
				}

				Debug.Assert(strLine != null, "");

				//不合法的字段,不算数
				if (strLine.Length < 3)
					continue;

				strFieldName = strLine.Substring(0,3);
				if (strLine.Length >= 3)
					strContent = strLine.Substring(3);
				else
					strContent = "";

				// control field  001-009没有子字段
				if ( (String.Compare(strFieldName, "001") >= 0
					&& String.Compare(strFieldName, "009") <= 0 )
					|| String.Compare(strFieldName, "-01") == 0)
				{
					if (WriteMarcPrefix == false)
						writer.WriteStartElement("controlfield");
					else
						writer.WriteStartElement(MarcPrefix,
							"controlfield", MarcNameSpaceUri);


					writer.WriteAttributeString("tag", strFieldName);

					writer.WriteString(strContent);
					writer.WriteEndElement();
					continue;
				}

				if (strLine.Length == 3)
				{
					strInd1 = " ";
					strInd2 = " ";
					strContent = "";
				}
					//字段长度等于4的情况,这样做是为了防止越界
				else if (strLine.Length == 4) 
				{
					strInd1 = strContent[0].ToString();
					strInd2 = " ";
					strContent = "";
				}
				else 
				{
					strInd1 = strContent[0].ToString();
					strInd2 = strContent[1].ToString();
					strContent = strContent.Substring(2);
				}

				// 普通字段
				if (WriteMarcPrefix == false)
					writer.WriteStartElement("datafield");
				else
					writer.WriteStartElement(MarcPrefix,
						"datafield", MarcNameSpaceUri);

				writer.WriteAttributeString("tag", strFieldName);
				writer.WriteAttributeString("ind1", strInd1);
				writer.WriteAttributeString("ind2", strInd2);

				//得到子字段数组
                /*
				string[] aSubfield = null;
				nRet = MarcUtil.GetSubfield(strContent,
					out aSubfield);
				if (nRet == -1)  //GetSubfield()出错
				{
					continue;
				}
                 * */

                string[] aSubfield = strContent.Split(new char[] { (char)31 });
                if (aSubfield == null)
                {
                    // 不太可能发生
                    continue;
                }


				//循环写子字段
				for(int j=0;j<aSubfield.Length;j++) 
				{
                    string strValue = aSubfield[j];
                    string strSubfieldName = "";
                    string strSubfieldContent = "";

                    if (j == 0)
                    {
                        // 第一个空字符串要被跳过。其余的，将来返还时会用来产生一个单独的 31 字符
                        if (string.IsNullOrEmpty(aSubfield[0]) == true)
                            continue;
                        strSubfieldName = null; // 表示后面不要创建code属性
                        strSubfieldContent = strValue;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(strValue) == false)
                            strSubfieldName = strValue.Substring(0, 1);
                        if (string.IsNullOrEmpty(strValue) == false)
                            strSubfieldContent = strValue.Substring(1);
                    }

					if (WriteMarcPrefix == false)
						writer.WriteStartElement("subfield");
					else
						writer.WriteStartElement(MarcPrefix,
							"subfield", MarcNameSpaceUri);

                    if (strSubfieldName != null)
					    writer.WriteAttributeString("code", strSubfieldName);
                    writer.WriteString(strSubfieldContent); //注意这里是否有越界的危险
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			return 0;

			
			/*
			}

			catch (Exception ex)
			{
				//writer.BaseStream.Seek(lStart, SeekOrigin.Begin);
				writer.BaseStream.SetLength(lStart);
				writer.BaseStream.Seek(0, SeekOrigin.End);

				strError = ex.Message;
				return -1;
			}
			*/
			


		}

	}


}
