using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;  // DpNs
using System.Linq;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// 用于将 MARC 机内格式字符串转换为 XML 格式的类
    /// </summary>
	public class MarcXmlWriter : IDisposable
    {
        public bool WriteMarcPrefix = true;  //是否写前缀
        public bool WriteXsi = false;         //是否写xsi

        //public string MarcPrefix = "marc";    //前缀
        //public string MarcNameSpaceUri = DigitalPlatform.Xml.Ns.unimarcxml; // "http://www.loc.gov/MARC21/slim"; //marc的命名空间

        private XmlTextWriter _writer = null;   //XmlTextWriter对象
        public Formatting m_Formatting = Formatting.None; //格式
        public int m_Indentation = 2;  //缩进量

        public void Dispose()
        {
            if (this._writer != null)
            {
                this._writer.Close();
                this._writer = null;
            }
        }

        public MarcXmlWriter()
        {
        }

        public MarcXmlWriter(Stream w,
            Encoding encoding)// : base(w, encoding)
        {
            _writer = new XmlTextWriter(w, encoding);
        }

        // 2015/5/10
        public MarcXmlWriter(TextWriter w)// : base(w, encoding)
        {
            _writer = new XmlTextWriter(w);
        }

        public MarcXmlWriter(string filename,
            Encoding encoding)// : base(filename, encoding)
        {
            _writer = new XmlTextWriter(filename, encoding);
        }

        /*
		public void Test()
		{
			writer.BaseStream.Seek(999*1024*1024, SeekOrigin.Current);
		}
		*/

        // 关闭write
        public void Close()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }

        public void Flush()
        {
            if (_writer != null)
                _writer.Flush();
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
                if (_writer != null)
                {
                    _writer.Formatting = value;
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
                if (_writer != null)
                {
                    _writer.Indentation = m_Indentation;
                }
            }
        }

        // 写开头，包括:
        // <? xml version='1.0' encoding='utf-8'?>
        // collection 根元素，及根据情况判断是否带命名空间
        // parameters:
        //      namespace_type  空/usmarc/unimarc
        public int WriteBegin(string namespace_type = "")
        {
            _writer.WriteStartDocument();

            /*
            if (WriteMarcPrefix == false)
                _writer.WriteStartElement("", "collection", MarcNameSpaceUri);
            else
                _writer.WriteStartElement(MarcPrefix,
                    "collection", MarcNameSpaceUri);
            */

            if (namespace_type == "unimarc")
            {
                _writer.WriteStartElement("unimarc",
    "collection", DpNs.unimarcxml);
            }
            else if (namespace_type == "usmarc")
            {
                _writer.WriteStartElement("usmarc",
"collection", Ns.usmarcxml);
            }
            else
            {
                // 2022/6/17
                _writer.WriteStartElement("dprms", "collection", DpNs.dprms);
            }

            // dprms名字空间 2010/11/15
            _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            if (WriteXsi == true)
            {
                /* 2010/10/28
                writer.WriteAttributeString("xmlns:xsi",
					"http://www.w3.org/2001/XMLSchema-instance");
                 * 上面用法有问题。会造成大量重复的nsuri存在。应该采用下面的用法：
                    writer.WriteAttributeString("xmlns", "dc", null,
    "http://purl.org/dc/elements/1.1/");
                 * */
                _writer.WriteAttributeString("xmlns", "xsi", null,
                    "http://www.w3.org/2001/XMLSchema-instance");
                _writer.WriteAttributeString("xsi", "schemaLocation", null,
                    "http://www.loc.gov/MARC21/slim http://www.loc.gov/standards/marcxml/schema/MARC21slim.xsd");
            }
            return 0;
        }

        // 关闭collection
        public int WriteEnd()
        {
            _writer.WriteEndElement();
            return 0;
        }

        // 将 MARC 机内格式字符串写入。也就是执行转换的意思。调用后， XML 结果需要从 TextWriter 中取出
        public int WriteRecord(
            string strMarcSyntax,
            string strMARC,
            string path,
            string timestamp,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            string[] saField = null;
            nRet = MarcUtil.ConvertMarcToFieldArray(strMARC,
                out saField,
                out strError);
            if (nRet == -1)
                return -1;
            string prefix = strMarcSyntax;
            string namespace_uri = "";
            if (strMarcSyntax == "unimarc")
            {
                namespace_uri = DpNs.unimarcxml;
            }
            else if (strMarcSyntax == "usmarc")
            {
                namespace_uri = Ns.usmarcxml;
            }
            else
            {
                // 当作 UNIMARC
                namespace_uri = DpNs.unimarcxml;
            }

            return WriteRecord(
                strMarcSyntax,
                namespace_uri,
                saField,
                path,
                timestamp,
                out strError);
        }

        // return:
        //		0	成功
        //		-1	出错
        public int WriteRecord(
            string MarcPrefix,
            string MarcNameSpaceUri,
            string[] saField,
            string path,
            string timestamp,
            out string strError)
        {
            string strFieldName = null;
            // int nRet;

            strError = "";

            // long lStart = writer.BaseStream.Position;
            // Debug.Assert(writer.BaseStream.CanSeek == true, "writer.BaseStream.CanSeek != true");

            //根据WriteMarcPrefix的值，确定是否对元素record加命名空间
            if (WriteMarcPrefix == false)
                _writer.WriteStartElement("record");
            else
                _writer.WriteStartElement(MarcPrefix,
                    "record", MarcNameSpaceUri);

            if (String.IsNullOrEmpty(_writer.LookupPrefix(DpNs.dprms)) == true)
            {
                // dprms名字空间 2010/11/15
                _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
            }

            if (string.IsNullOrEmpty(path) == false)
            {
                _writer.WriteAttributeString("path", DpNs.dprms, path);
            }

            if (string.IsNullOrEmpty(timestamp) == false)
            {
                _writer.WriteAttributeString("timestamp", DpNs.dprms, timestamp);
            }

            //循环，写头标区及每个字段
            for (int i = 0; i < saField.Length; i++)
            {
                string strLine = saField[i];
                string strInd1 = null;
                string strInd2 = null;
                string strContent = null;

                // 头标区
                if (i == 0)
                {
                    // 多截少添
                    if (strLine.Length > 24)
                        strLine = strLine.Substring(0, 24);
                    else
                    {
                        while (strLine.Length < 24)
                        {
                            strLine += " ";
                        }
                    }

                    strLine = ReplaceInvalidXmlChars(strLine);

                    if (WriteMarcPrefix == false)
                        _writer.WriteElementString("leader", strLine);
                    else
                        _writer.WriteElementString("leader", MarcNameSpaceUri, strLine);

                    continue;
                }

                Debug.Assert(strLine != null, "");

                // 不合法的字段,不算数
                if (strLine.Length < 3)
                    continue;

                strFieldName = strLine.Substring(0, 3);
                if (strLine.Length >= 3)
                    strContent = strLine.Substring(3);
                else
                    strContent = "";

                // control field  001-009 没有子字段
                /*
				if ( (String.Compare(strFieldName, "001") >= 0
					&& String.Compare(strFieldName, "009") <= 0 )
					|| String.Compare(strFieldName, "-01") == 0)
                 * */
                if (MarcUtil.IsControlFieldName(strFieldName) == true)
                {
                    if (WriteMarcPrefix == false)
                        _writer.WriteStartElement("controlfield");
                    else
                        _writer.WriteStartElement(MarcPrefix,
                            "controlfield", MarcNameSpaceUri);

                    _writer.WriteAttributeString("tag", strFieldName);

                    _writer.WriteString(ReplaceInvalidXmlChars(strContent));
                    _writer.WriteEndElement();
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
                    _writer.WriteStartElement("datafield");
                else
                    _writer.WriteStartElement(MarcPrefix,
                        "datafield", MarcNameSpaceUri);

                _writer.WriteAttributeString("tag", ReplaceInvalidXmlChars(strFieldName));
                _writer.WriteAttributeString("ind1", ReplaceInvalidXmlChars(strInd1));
                _writer.WriteAttributeString("ind2", ReplaceInvalidXmlChars(strInd2));

                // 得到子字段数组

                string[] aSubfield = strContent.Split(new char[] { (char)31 });
                if (aSubfield == null)
                {
                    // 不太可能发生
                    continue;
                }

                // 循环写子字段
                for (int j = 0; j < aSubfield.Length; j++)
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
                        _writer.WriteStartElement("subfield");
                    else
                        _writer.WriteStartElement(MarcPrefix,
                            "subfield", MarcNameSpaceUri);

                    if (strSubfieldName != null)
                        _writer.WriteAttributeString("code", ReplaceInvalidXmlChars(strSubfieldName));
                    _writer.WriteString(strSubfieldContent); //注意这里是否有越界的危险
                    _writer.WriteEndElement();
                }

                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            return 0;
        }

        // https://stackoverflow.com/questions/8170739/dealing-with-invalid-xml-hexadecimal-characters
        public static string RemoveInvalidXmlChars(string content)
        {
            return new string(content.Where(ch => System.Xml.XmlConvert.IsXmlChar(ch)).ToArray());
        }

        // 2022/6/28
        public static string ReplaceInvalidXmlChars(
            string content,
            char replaceChar = '*')
        {
            StringBuilder results = new StringBuilder();
            foreach(char ch in content)
            {
                if (System.Xml.XmlConvert.IsXmlChar(ch))
                    results.Append(ch);
                else
                    results.Append(replaceChar);
            }

            return results.ToString();
        }

        public int WriteXChangeRecord(
        string strMarcSyntax,
        string strType,
        string strMARC,
        out string strError)
        {
            strError = "";
            int nRet = 0;
            nRet = MarcUtil.ConvertMarcToFieldArray(strMARC,
                out string [] saField,
                out strError);
            if (nRet == -1)
                return -1;
            string prefix = strMarcSyntax;
            string namespace_uri = Ns.marcxchange;

            return WriteRecord(
                strMarcSyntax,
                namespace_uri,
                strType,
                saField,
                out strError);
        }

        // return:
        //		0	成功
        //		-1	出错
        public int WriteRecord(
            string MarcPrefix,
            string MarcNameSpaceUri,
            string strType,
            string[] saField,
            out string strError)
        {
            string strFieldName = null;
            // int nRet;

            strError = "";

            // long lStart = writer.BaseStream.Position;
            // Debug.Assert(writer.BaseStream.CanSeek == true, "writer.BaseStream.CanSeek != true");

            //根据WriteMarcPrefix的值，确定是否对元素record加命名空间
            if (WriteMarcPrefix == false)
                _writer.WriteStartElement("record");
            else
                _writer.WriteStartElement(MarcPrefix,
                    "record", MarcNameSpaceUri);
            if (MarcNameSpaceUri == Ns.marcxchange)
            {
                // 写入 format 属性
                string format = "UNIMARC";
                if (MarcPrefix == "usmarc")
                    format = "MARC21";
                _writer.WriteAttributeString("format", format);

                // 写入 type 属性
                if (String.IsNullOrEmpty(strType) == true)
                    strType = "Bibliographic";
                _writer.WriteAttributeString("type", strType);
            }

            if (String.IsNullOrEmpty(_writer.LookupPrefix("dprms")) == true)
            {
                // dprms名字空间
                _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);
            }

            //循环，写头标区及每个子段
            for (int i = 0; i < saField.Length; i++)
            {
                string strLine = saField[i];
                string strInd1 = null;
                string strInd2 = null;
                string strContent = null;

                // 头标区
                if (i == 0)
                {
                    // 多截少添
                    if (strLine.Length > 24)
                        strLine = strLine.Substring(0, 24);
                    else
                    {
                        while (strLine.Length < 24)
                        {
                            strLine += " ";
                        }
                    }

                    if (WriteMarcPrefix == false)
                        _writer.WriteElementString("leader", strLine);
                    else
                        _writer.WriteElementString("leader", MarcNameSpaceUri, strLine);

                    continue;
                }

                Debug.Assert(strLine != null, "");

                // 不合法的字段,不算数
                if (strLine.Length < 3)
                    continue;

                strFieldName = strLine.Substring(0, 3);
                if (strLine.Length >= 3)
                    strContent = strLine.Substring(3);
                else
                    strContent = "";

                // control field  001-009 没有子字段
                /*
				if ( (String.Compare(strFieldName, "001") >= 0
					&& String.Compare(strFieldName, "009") <= 0 )
					|| String.Compare(strFieldName, "-01") == 0)
                 * */
                if (MarcUtil.IsControlFieldName(strFieldName) == true)
                {
                    if (WriteMarcPrefix == false)
                        _writer.WriteStartElement("controlfield");
                    else
                        _writer.WriteStartElement(MarcPrefix,
                            "controlfield", MarcNameSpaceUri);

                    _writer.WriteAttributeString("tag", strFieldName);

                    _writer.WriteString(strContent);
                    _writer.WriteEndElement();
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
                    _writer.WriteStartElement("datafield");
                else
                    _writer.WriteStartElement(MarcPrefix,
                        "datafield", MarcNameSpaceUri);

                _writer.WriteAttributeString("tag", strFieldName);
                _writer.WriteAttributeString("ind1", strInd1);
                _writer.WriteAttributeString("ind2", strInd2);

                // 得到子字段数组

                string[] aSubfield = strContent.Split(new char[] { (char)31 });
                if (aSubfield == null)
                {
                    // 不太可能发生
                    continue;
                }

                // 循环写子字段
                for (int j = 0; j < aSubfield.Length; j++)
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
                        _writer.WriteStartElement("subfield");
                    else
                        _writer.WriteStartElement(MarcPrefix,
                            "subfield", MarcNameSpaceUri);

                    if (strSubfieldName != null)
                        _writer.WriteAttributeString("code", strSubfieldName);
                    _writer.WriteString(strSubfieldContent); //注意这里是否有越界的危险
                    _writer.WriteEndElement();
                }

                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            return 0;
        }
    }
}
