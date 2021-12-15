using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation.ISO2709Statis
{
    // 2021/12/15
    /// <summary>
    /// 将 dt1000 的书目 MARC 记录转换为 dp2 的 bdf 格式
    /// </summary>
    public class ConvertDt1000ToBdf : Iso2709Statis
    {
        XmlTextWriter _writer = null;

        // 累积馆藏地点列表
        public Hashtable m_locations = new Hashtable();

        // 累积采购资金来源列表
        public Hashtable m_sources = new Hashtable();

        // 累积采购书商列表
        public Hashtable m_sellers = new Hashtable();

        // 累积采购类别列表
        public Hashtable m_orderclasses = new Hashtable();

        // 是否为期刊记录
        bool _bSeries = false;

        // 记到控件
        UpgradeUtil.JidaoControl jidaoControl1 = null;

        public override void FreeResources()
        {
            if (_writer != null)
                _writer.Close();
        }

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的书目转储文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            dlg.Filter = "书目转储文件 (*.bdf)|*.bdf|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                e.Continue = ContinueType.SkipAll;
                return;
            }

            DialogResult result = MessageBox.Show(this.MainForm,
    "是期刊还是图书的记录? \r\n\r\n[Yes]期刊 [No]图书",
    "ConvertDt1000ToBdf",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
                _bSeries = true;
            else
                _bSeries = false;

            try
            {
                _writer = new XmlTextWriter(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "创建文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            _writer.Formatting = Formatting.Indented;
            _writer.Indentation = 4;

            _writer.WriteStartDocument();
            _writer.WriteStartElement("dprms", "collection", DpNs.dprms);

            _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            if (this.jidaoControl1 == null)
                this.jidaoControl1 = new UpgradeUtil.JidaoControl();

            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnRecord(object sender, StatisEventArgs e)
        {
            string strError = "";

            MarcRecord record = new MarcRecord(this.MARC);

            var g01 = record.select("field[@name='-01']").FirstContent;
            var parts = StringUtil.ParseTwoPart(g01, "|");
            string path = parts[0];
            string timestamp = parts[1];

            string strXml = "";
            int nRet = MarcUtil.Marc2XmlEx(this.MARC,
    this.Syntax,
    ref strXml,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlDocument biblio_dom = new XmlDocument();
            biblio_dom.LoadXml(strXml);

            {
                // 写入 dprms:record 元素
                _writer.WriteStartElement("dprms", "record", DpNs.dprms);

                // 写入书目记录
                {
                    // 写入 dprms:biblio 元素
                    _writer.WriteStartElement("dprms", "biblio", DpNs.dprms);

                    _writer.WriteAttributeString("path", path);
                    _writer.WriteAttributeString("timestamp", timestamp);

                    biblio_dom.DocumentElement.WriteTo(_writer);
                    _writer.WriteEndElement();
                }

                // 写入实体记录
                {
                    nRet = BuildEntities(
    _bSeries,
    this.MARC,
    this.Syntax,
    out List<string> entity_xmls,
    // out int nThisEntityCount,
    out string strWarning,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        string strText = "";
                        string[] lines = strWarning.Split(new char[] { ';', '\r' });
                        for (int i = 0; i < lines.Length; i++)
                        {
                            strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                        }
                        this.WriteToConsole(
                            "记录 " + path + " 的册信息处理过程发出警告: <br/>" + strText);
                    }

                    if (entity_xmls.Count > 0)
                    {
                        _writer.WriteStartElement("dprms", "itemCollection", DpNs.dprms);
                        foreach (var xml in entity_xmls)
                        {
                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(xml);

                            _writer.WriteStartElement("dprms", "item", DpNs.dprms);
                            // writer.WriteAttributeString("path", info.OldRecPath);
                            // writer.WriteAttributeString("timestamp", ByteArray.GetHexTimeStampString(info.OldTimestamp));
                            DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                            item_dom.DocumentElement.WriteContentTo(_writer);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteEndElement();
                    }
                }

                // 写入订购记录
                {
                    string strWarning = "";
                    List<string> order_xmls = null;

                    if (_bSeries == true)
                    {
                        nRet = BuildSeriesOrderRecords(
                            //strOutputBiblioRecPath,
                            //strOrderDbName,
                            //strRecordID,
                            this.MARC,
                            this.Syntax,
                            // out nThisOrderCount,
                            out order_xmls,
                            out strWarning,
                            out strError);
                    }
                    else
                    {
                        /*
                        // 将一条MARC记录中包含的订购信息变成XML格式并上传
                        nRet = BuildBookOrderRecords(
                            //strOutputBiblioRecPath,
                            //strOrderDbName,
                            //strRecordID,
                            this.MARC,
                            this.Syntax,
                            //out nThisOrderCount,
                            out order_xmls,
                            out strWarning,
                            out strError);
                        */
                    }
                    if (nRet == -1)
                       goto ERROR1;

                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        string strText = "";
                        string[] lines = strWarning.Split(new char[] { ';', '\r' });
                        for (int i = 0; i < lines.Length; i++)
                        {
                            strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                        }
                        this.WriteToConsole(
                            "记录 " + path + " 的订购信息处理过程发出警告: <br/>" + strText);
                    }

                    if (order_xmls != null && order_xmls.Count > 0)
                    {
                        _writer.WriteStartElement("dprms", "orderCollection", DpNs.dprms);
                        foreach (var xml in order_xmls)
                        {
                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(xml);

                            _writer.WriteStartElement("dprms", "order", DpNs.dprms);
                            DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                            item_dom.DocumentElement.WriteContentTo(_writer);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteEndElement();
                    }
                }

                // 写入期记录
                if (_bSeries == true)
                {
                    nRet = this.jidaoControl1.Upgrade(this.MARC,
                        "_operator",
                        out List<string> issue_xmls,
                        out strError);
                    if (nRet == -1)
                    {
                        string strWarning = strError;
                        if (String.IsNullOrEmpty(strWarning) == false)
                        {
                            string strText = "";
                            string[] lines = strWarning.Split(new char[] { ';', '\r' });
                            for (int i = 0; i < lines.Length; i++)
                            {
                                strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                            }
                            this.WriteToConsole(
                                "记录 " + path + " 的期信息处理过程出现错误: <br/>" + strText);
                        }
                    }
                    else
                    {
                        if (issue_xmls != null
                            && issue_xmls.Count > 0)
                        {
                            _writer.WriteStartElement("dprms", "issueCollection", DpNs.dprms);
                            foreach (var xml in issue_xmls)
                            {
                                XmlDocument item_dom = new XmlDocument();
                                item_dom.LoadXml(xml);

                                _writer.WriteStartElement("dprms", "issue", DpNs.dprms);
                                DomUtil.RemoveEmptyElements(item_dom.DocumentElement);
                                item_dom.DocumentElement.WriteContentTo(_writer);
                                _writer.WriteEndElement();
                            }
                            _writer.WriteEndElement();
                        }
                    }
                }

                _writer.WriteEndElement();
            }

            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            if (_writer != null)
            {
                _writer.WriteEndElement();   // </collection>
                _writer.WriteEndDocument();

                _writer.Close();
                _writer = null;
            }
        }

        // 将一条MARC记录中包含的实体信息变成XML格式并上传
        // parameters:
        //      bSeries 是否为期刊库? 如果为期刊库，则需要把907和986合并；否则要把906和986合并
        //      strEntityDbName 实体数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int BuildEntities(
            bool bSeries,
            //string strBiblioRecPath,
            //string strEntityDbName,
            //string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            out List<string> entity_xmls,
            // out int nThisEntityCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            // nThisEntityCount = 0;
            entity_xmls = new List<string>();

            int nRet = 0;

            string strField906or907 = "";
            string strField986 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            /*
            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";
            */

            // 获得010$a
            string strBiblioPrice = "";
            if (strMarcSyntax == "unimarc")
            {
                strBiblioPrice = MarcUtil.GetFirstSubfield(strMARC,
                    "010",
                    "d");
            }
            else if (strMarcSyntax == "usmarc")
            {
                strBiblioPrice = MarcUtil.GetFirstSubfield(strMARC,
                    "020",
                    "c");
            }
            else
            {
                strError = "未知的strMarcSyntax值 '" + strMarcSyntax + "'";
                return -1;
            }

            if (String.IsNullOrEmpty(strBiblioPrice) == false)
            {
                // 正规化价格字符串
                strBiblioPrice = CanonicalizePrice(strBiblioPrice, false);
            }

            // 获得906/907字段
            nRet = MarcUtil.GetField(strMARC,
                bSeries == true ? "907" : "906",
                0,
                out strField906or907,
                out strNextFieldName);
            if (nRet == -1)
            {
                if (bSeries == true)
                    strError = "从MARC记录中获得907字段时出错";
                else
                    strError = "从MARC记录中获得906字段时出错";
                return -1;
            }
            if (nRet == 0)
                strField906or907 = "";

            // 获得986字段

            // 从记录中得到一个字段
            // parameters:
            //		strMARC		机内格式MARC记录
            //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
            //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
            //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
            //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
            //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = MarcUtil.GetField(strMARC,
                "986",
                0,
                out strField986,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得986字段时出错";
                return -1;
            }

            if (nRet == 0)
            {
                // return 0;   // 没有找到986字段
                strField986 = "";
            }
            else
            {
                // 修正986字段内容
                if (strField986.Length <= 5 + 2)
                    strField986 = "";
                else
                {
                    string strPart = strField986.Substring(5, 2);

                    string strDollarA = new string(MarcUtil.SUBFLD, 1) + "a";

                    if (strPart != strDollarA)
                    {
                        strField986 = strField986.Insert(5, strDollarA);
                    }
                }
            }

            List<ItemGroup> groups = null;

            if (bSeries == true)
            {
                // 合并907和986字段内容
                nRet = MergeField907and986(strField906or907,
                    strField986,
                    out groups,
                    out strWarningParam,
                    out strError);
            }
            else
            {
                // 合并906和986字段内容
                nRet = MergeField906and986(strField906or907,
                    strField986,
                    out groups,
                    out strWarningParam,
                    out strError);
            }
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += strWarningParam + "; ";

            // List<EntityInfo> entityArray = new List<EntityInfo>();

            // 进行子字段组循环
            for (int g = 0; g < groups.Count; g++)
            {
                ItemGroup group = groups[g];

                string strGroup = group.strValue;

                // 处理一个item

                string strXml = "";

                // 构造实体XML记录
                // parameters:
                //      strParentID 父记录ID
                //      strGroup    待转换的图书种记录的986字段中某子字段组片断
                //      strXml      输出的实体XML记录
                // return:
                //      -1  出错
                //      0   成功
                nRet = BuildEntityXmlRecord(
                    bSeries,
                    // strParentRecordID,
                    strGroup,
                    strMARC,
                    strBiblioPrice,
                    group.strMergeComment,
                    out strXml,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建实体(序号) " + Convert.ToString(g + 1) + "时发生错误: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

                // 搜集<location>值
                // 2008/8/22
                XmlDocument entity_dom = new XmlDocument();
                try
                {
                    entity_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "创建实体(序号) " + Convert.ToString(g + 1) + "时发生错误: "
                        + "entity xml装入XMLDOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strLocation = DomUtil.GetElementText(entity_dom.DocumentElement, "location");

                // 允许馆藏地点值为空
                if (String.IsNullOrEmpty(strLocation) == true)
                    strLocation = "";

                FillValueTable(this.m_locations,
                    strLocation);

                entity_xmls.Add(strXml);
            }

            return 0;
        }

        static void FillValueTable(Hashtable table,
    string strValue)
        {
            object o = table[strValue];
            long count = 0;
            if (o == null)
            {
                count = 1;
            }
            else
            {
                count = (long)o;
                count++;
            }
            table[strValue] = (object)count;
        }

        // 构造实体 XML 记录
        // parameters:
        //      strParentID 父记录ID
        //      strGroup    待转换的图书种记录的986字段中某子字段组片断
        //      strXml      输出的实体XML记录
        //      strBiblioPrice  种价格。当缺乏册例外价格的时候，自动加入种价格
        // return:
        //      -1  出错
        //      0   成功
        static int BuildEntityXmlRecord(
            bool bSeries,
            // string strParentID,
            string strGroup,
            string strMARC,
            string strBiblioPrice,
            string strMergeComment,
            out string strXml,
            out string strWarning,
            out string strError)
        {
            strXml = "";
            strWarning = "";
            strError = "";

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            /*
            // 父记录id
            DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);
            */

            // 册条码

            string strSubfield = "";
            string strNextSubfieldName = "";
            // 从字段或子字段组中得到一个子字段
            // parameters:
            //		strText		字段内容，或者子字段组内容。
            //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
            //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
            //					形式为'a'这样的。
            //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
            //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
            //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中
            int nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "a",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strBarcode = strSubfield.Substring(1);

                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                if (string.IsNullOrEmpty(strBarcode) == false)
                    DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);
            }

            // 登录号
            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "h",
    0,
    out strSubfield,
    out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strRegisterNo = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strRegisterNo) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "registerNo", strRegisterNo);
                }
            }

            // 状态?
            DomUtil.SetElementText(dom.DocumentElement, "state", "");

            // 馆藏地点
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "b",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strLocation = strSubfield.Substring(1);

                DomUtil.SetElementText(dom.DocumentElement, "location", strLocation);
            }

            // 价格
            // 先找子字段组中的$d 找不到才找982$b

            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "d",
    0,
    out strSubfield,
    out strNextSubfieldName);
            string strPrice = "";

            if (strSubfield.Length >= 1)
            {
                strPrice = strSubfield.Substring(1);
                // 正规化价格字符串
                strPrice = CanonicalizePrice(strPrice, false);
            }
            else
            {
                strPrice = strBiblioPrice;
            }

            // 如果从$d中获得的价格内容为空，则从982$b中获得
            if (String.IsNullOrEmpty(strPrice) == true)
            {
                // 以字段/子字段名从记录中得到第一个子字段内容。
                // parameters:
                //		strMARC	机内格式MARC记录
                //		strFieldName	字段名。内容为字符
                //		strSubfieldName	子字段名。内容为1字符
                // return:
                //		""	空字符串。表示没有找到指定的字段或子字段。
                //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
                strPrice = MarcUtil.GetFirstSubfield(strMARC,
                    "982",
                    "b");
            }

            if (string.IsNullOrEmpty(strPrice) == false)
                DomUtil.SetElementText(dom.DocumentElement, "price", strPrice);

            // 图书册类型
            // 先找这里的$f 如果没有，再找982$a?
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"f",
0,
out strSubfield,
out strNextSubfieldName);
            string strBookType = "";
            if (strSubfield.Length >= 1)
            {
                strBookType = strSubfield.Substring(1);
            }

            // 如果从$f中获得的册类型为空，则从982$a中获得
            if (String.IsNullOrEmpty(strBookType) == true)
            {
                // 以字段/子字段名从记录中得到第一个子字段内容。
                // parameters:
                //		strMARC	机内格式MARC记录
                //		strFieldName	字段名。内容为字符
                //		strSubfieldName	子字段名。内容为1字符
                // return:
                //		""	空字符串。表示没有找到指定的字段或子字段。
                //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
                strBookType = MarcUtil.GetFirstSubfield(strMARC,
                    "982",
                    "a");
            }

            if (string.IsNullOrEmpty(strBookType) == false)
                DomUtil.SetElementText(dom.DocumentElement, "bookType", strBookType);

            // 注释
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"z",
0,
out strSubfield,
out strNextSubfieldName);
            string strComment = "";
            if (strSubfield.Length >= 1)
            {
                strComment = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "comment", strComment);
            }

            // 借阅者
            nRet = MarcUtil.GetSubfield(strGroup,
ItemType.Group,
"r",
0,
out strSubfield,
out strNextSubfieldName);
            string strBorrower = "";
            if (strSubfield.Length >= 1)
            {
                strBorrower = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strBorrower) == false)
            {
                strBorrower = strBorrower.ToUpper();  // 2008/10/24 new add

                DomUtil.SetElementText(dom.DocumentElement, "borrower", strBorrower);

                // 借阅日期
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                string strBorrowDate = "";
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    // 格式为 20060625， 需要转换为rfc
                    if (strBorrowDate.Length == 8)
                    {
                        /*
                        IFormatProvider culture = new CultureInfo("zh-CN", true);

                        DateTime time;
                        try
                        {
                            time = DateTime.ParseExact(strBorrowDate, "yyyyMMdd", culture);
                        }
                        catch
                        {
                            strError = "子字段组中$t内容中的借阅日期 '" + strBorrowDate + "' 字符串转换为DateTime对象时出错";
                            return -1;
                        }

                        time = time.ToUniversalTime();
                        strBorrowDate = DateTimeUtil.Rfc1123DateTimeString(time);
                         * */

                        string strTarget = "";

                        nRet = ReaderInfoForm.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                        if (nRet == -1)
                        {
                            strWarning += "子字段组中$t内容中的借阅日期 '" + strBorrowDate + "' 格式出错: " + strError;
                            strBorrowDate = "";
                        }
                        else
                        {
                            strBorrowDate = strTarget;
                        }

                    }
                    else if (String.IsNullOrEmpty(strBorrowDate) == false)
                    {
                        strWarning += "$t中日期值 '" + strBorrowDate + "' 格式错误，长度应为8字符 ";
                        strBorrowDate = "";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "borrowDate", strBorrowDate);
                }

                // 借阅期限
                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "borrowPeriod", "1day"); // 象征性地为1天。因为<borrowDate>中的值实际为应还日期
                }
            }

            if (bSeries == true)
            {
                // $C几期一装? 从907$C复制过来
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "C",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strVolumeCount = "";
                if (strSubfield.Length >= 1)
                {
                    strVolumeCount = strSubfield.Substring(1);
                }

                // 卷册范围 从907$yvqn复制过来
                // $y年范围
                // $v卷范围
                // $q期范围
                // $n总期号范围?

                // $y
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "y",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strYearRange = "";
                if (strSubfield.Length >= 1)
                {
                    strYearRange = strSubfield.Substring(1);
                }

                // $v
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "v",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strVolumeRange = "";
                if (strSubfield.Length >= 1)
                {
                    strVolumeRange = strSubfield.Substring(1);
                }

                // 2010/3/30
                string strZongRange = "";
                // $z
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "z",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strZongRange = strSubfield.Substring(1);
                }

                // $q
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "q",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strIssueRange = "";
                if (strSubfield.Length >= 1)
                {
                    strIssueRange = strSubfield.Substring(1);
                }

                // 根据几个分离的信息构造dp2系统的卷册范围内容字符串
                string strVolume = BuildDp2VolumeString(strVolumeCount,
                    strYearRange,
                    strIssueRange,
                    strZongRange,
                    strVolumeRange);
                if (String.IsNullOrEmpty(strVolume) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "volume", strVolume);
                }

                // $R装订者 从907$R复制过来
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "R",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strBindOperator = "";
                if (strSubfield.Length >= 1)
                {
                    strBindOperator = strSubfield.Substring(1);
                }

                /*
                if (String.IsNullOrEmpty(strBindOperator) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "bindOperator", strBindOperator);
                }
                 * */

                // $D装订日期
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "D",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strBindOperTime = "";
                if (strSubfield.Length >= 1)
                {
                    strBindOperTime = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBindOperator) == false
                    || String.IsNullOrEmpty(strBindOperTime) == false)
                {

                    try
                    {
                        DateTime time = DateTimeUtil.Long8ToDateTime(strBindOperTime);
                        string strTime = DateTimeUtil.Rfc1123DateTimeString(time.ToUniversalTime());

                        // 设置或者刷新一个操作记载
                        nRet = SetOperation(
                            dom.DocumentElement,
                            "create",
                            strBindOperator,
                            "binding",
                            strTime,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                // 2009/9/17 new add
                // 普通图书

                // $v
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "v",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                string strVolumeRange = "";
                if (strSubfield.Length >= 1)
                {
                    strVolumeRange = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strVolumeRange) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "volume", strVolumeRange);
                }
            }

            // 状态
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "s",
                0,
                out strSubfield,
                out strNextSubfieldName);
            string strState = "";
            if (strSubfield.Length >= 1)
            {
                strState = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strState) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

            strXml = dom.OuterXml;
            return 0;
        }

        // 根据几个分离的信息构造dp2系统的卷册范围内容字符串
        static string BuildDp2VolumeString(string strVolumeCount,
            string strYearRange,
            string strIssueRange,
            string strZongRange,
            string strVolumeRange)
        {
            string strResult = "";
            if (String.IsNullOrEmpty(strYearRange) == false)
                strResult += "y." + strYearRange;

            if (String.IsNullOrEmpty(strVolumeRange) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "=";

                strResult += "v." + strVolumeRange;
            }

            if (String.IsNullOrEmpty(strIssueRange) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "=";

                strResult += "no." + strIssueRange;
            }

            if (String.IsNullOrEmpty(strZongRange) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "=";

                strResult += "总." + strZongRange;
            }

            if (String.IsNullOrEmpty(strVolumeCount) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "=";

                strResult += "c." + strVolumeCount;
            }

            return strResult;
        }

        // 设置或者刷新一个操作记载
        public static int SetOperation(
            XmlNode root,
            string strOperName,
            string strOperator,
            string strComment,
            string strTime,
            out string strError)
        {
            strError = "";

            if (root == null)
            {
                strError = "root == null";
                return -1;
            }

            XmlNode nodeOperations = root.SelectSingleNode("operations");
            if (nodeOperations == null)
            {
                nodeOperations = root.OwnerDocument.CreateElement("operations");
                root.AppendChild(nodeOperations);
            }

            XmlNode node = nodeOperations.SelectSingleNode("operation[@name='" + strOperName + "']");
            if (node == null)
            {
                node = root.OwnerDocument.CreateElement("operation");
                nodeOperations.AppendChild(node);
                DomUtil.SetAttr(node, "name", strOperName);
            }

            DomUtil.SetAttr(node, "time", strTime);
            DomUtil.SetAttr(node, "operator", strOperator);
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(node, "comment", strComment);

            return 0;
        }


        // 合并907和986字段内容
        static int MergeField907and986(string strField907,
            string strField986,
            out List<ItemGroup> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            groups = null;
            strError = "";
            strWarning = "";

            int nRet = 0;

            List<ItemGroup> groups_907 = null;
            List<ItemGroup> groups_986 = null;

            string strWarningParam = "";

            nRet = BuildGroups(strField907,
                out groups_907,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将907字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "907字段 " + strWarningParam + "; ";

            nRet = BuildGroups(strField986,
                out groups_986,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将986字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "986字段 " + strWarningParam + "; ";


            List<ItemGroup> new_groups = new List<ItemGroup>(); // 新增部分

            for (int i = 0; i < groups_907.Count; i++)
            {
                ItemGroup group907 = groups_907[i];

                bool bFound = false;
                for (int j = 0; j < groups_986.Count; j++)
                {
                    ItemGroup group986 = groups_986[j];

                    if (group907.strBarcode != "")
                    {
                        if (group907.strBarcode == group986.strBarcode)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group907,
                                "CyvqnR");

                            break;
                        }
                    }
                    else if (group907.strRegisterNo != "")
                    {
                        if (group907.strRegisterNo == group986.strRegisterNo)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group907,
                                "CyvqnR");

                            break;
                        }
                    }
                }

                if (bFound == true)
                {
                    continue;
                }

                group907.strMergeComment = "从907字段中增补过来";
                new_groups.Add(group907);
            }

            groups = new List<ItemGroup>(); // 结果数组
            groups.AddRange(groups_986);    // 先加入986内的所有事项

            if (new_groups.Count > 0)
                groups.AddRange(new_groups);    // 然后加入新增事项


            return 0;
        }

        // 合并906和986字段内容
        static int MergeField906and986(string strField906,
            string strField986,
            // int nEntityBarcodeLength,
            out List<ItemGroup> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            groups = null;
            strError = "";
            strWarning = "";

            int nRet = 0;

            List<ItemGroup> groups_906 = null;
            List<ItemGroup> groups_986 = null;

            string strWarningParam = "";

            nRet = BuildGroups(strField906,
                // nEntityBarcodeLength,
                out groups_906,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将906字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "906字段 " + strWarningParam + "; ";

            nRet = BuildGroups(strField986,
                // nEntityBarcodeLength,
                out groups_986,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将986字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += "986字段 " + strWarningParam + "; ";


            List<ItemGroup> new_groups = new List<ItemGroup>(); // 新增部分

            for (int i = 0; i < groups_906.Count; i++)
            {
                ItemGroup group906 = groups_906[i];

                bool bFound = false;
                for (int j = 0; j < groups_986.Count; j++)
                {
                    ItemGroup group986 = groups_986[j];

                    if (group906.strBarcode != "")
                    {
                        if (group906.strBarcode == group986.strBarcode)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group906,
                                "b");

                            break;
                        }
                    }
                    else if (group906.strRegisterNo != "")
                    {
                        if (group906.strRegisterNo == group986.strRegisterNo)
                        {
                            bFound = true;

                            // 重复的情况下，补充986所缺的少量子字段
                            group986.MergeValue(group906,
                                "b");

                            break;
                        }
                    }
                }

                if (bFound == true)
                {
                    continue;
                }

                group906.strMergeComment = "从906字段中增补过来";
                new_groups.Add(group906);
            }

            groups = new List<ItemGroup>(); // 结果数组
            groups.AddRange(groups_986);    // 先加入986内的所有事项

            if (new_groups.Count > 0)
                groups.AddRange(new_groups);    // 然后加入新增事项


            return 0;
        }

        // 根据一个MARC字段，创建Group数组
        // 必须符合下列定义：
        // 将$a放入到Barcode
        // 将$h放入到RegisterNo
        static int BuildGroups(string strField,
            out List<ItemGroup> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            groups = new List<ItemGroup>();
            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                // 册条码

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";
                string strRegisterNo = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                }

                if (String.IsNullOrEmpty(strBarcode) == false)
                {
                    // 去掉左边的'*'号 2006/9/2 add
                    if (strBarcode[0] == '*')
                        strBarcode = strBarcode.Substring(1);

                    /*
                    // return:
                    //      -1  error
                    //      0   OK
                    //      1   Invalid
                    nRet = VerifyBarcode(
                        false,
                        strBarcode,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 检查册条码长度
                    if (nRet != 0)
                    {
                        strWarning += "册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                    }*/

                    strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add
                }


                // 登录号
                nRet = MarcUtil.GetSubfield(strGroup,
        ItemType.Group,
        "h",
        0,
        out strSubfield,
        out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strRegisterNo = strSubfield.Substring(1);
                }

                // TODO: 需要加入检查登录号长度的代码


                ItemGroup group = new ItemGroup();
                group.strValue = strGroup;
                group.strBarcode = strBarcode;
                group.strRegisterNo = strRegisterNo;

                groups.Add(group);
            }

            return 0;
        }

        /*
~~~~~~~
乐山师院数据来源多，以前的种价格字段格式著录格式多样，有“CNY25.00元”、
“25.00”、“￥25.00元”、“￥25.00”、“CNY25.00”、“cny25.00”、“25.00
元”等等，现在他们确定以后全采用“CNY25.00”格式著录。
CALIS中，许可重复010$d来表达价格实录和获赠或其它币种价格。所以，可能乐山
师院也有少量的此类重复价格子字段的数据。
为省成本，批处理或册信息编辑窗中，建议只管一个价格字段，别的都不管（如果
没有价格字段，则转换为空而非零）。
转换时，是否可以兼顾到用中文全角输入的数字如“２５.００”或小数点是中文
全解但标点选择的是英文标点如“．”？

~~~~
处理步骤：
1) 全部字符转换为半角
2) 抽出纯数字部分
3) 观察前缀或者后缀，如果有CNY cny ￥ 元等字样，可以确定为人民币。
前缀和后缀完全为空，也可确定为人民币。
否则，保留原来的前缀。         * */
        // 正规化价格字符串
        public static string CanonicalizePrice(string strPrice,
            bool bForceCNY)
        {
            // 全角字符变换为半角
            strPrice = Global.ConvertQuanjiaoToBanjiao(strPrice);

            if (bForceCNY == true)
            {
                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                return "CNY" + strPurePrice;
            }

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            string strError = "";

            int nRet = ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return strPrice;    // 无法parse

            bool bCNY = false;
            strPrefix = strPrefix.Trim();
            strPostfix = strPostfix.Trim();

            if (String.IsNullOrEmpty(strPrefix) == true
                && String.IsNullOrEmpty(strPostfix) == true)
            {
                bCNY = true;
                goto DONE;
            }


            if (strPrefix.IndexOf("CNY") != -1
                || strPrefix.IndexOf("cny") != -1
                || strPrefix.IndexOf("ＣＮＹ") != -1
                || strPrefix.IndexOf("ｃｎｙ") != -1
                || strPrefix.IndexOf('￥') != -1
                || strPrefix == "RMB"/* 2021/12/15 */)
            {
                bCNY = true;
                goto DONE;
            }

            if (strPostfix.IndexOf("元") != -1)
            {
                bCNY = true;
                goto DONE;
            }

        DONE:
            // 人民币
            if (bCNY == true)
                return "CNY" + strValue;

            // 其他货币
            return strPrefix + strValue + strPostfix;
        }

        // 分析价格参数
        public static int ParsePriceUnit(string strString,
            out string strPrefix,
            out string strValue,
            out string strPostfix,
            out string strError)
        {
            strPrefix = "";
            strValue = "";
            strPostfix = "";
            strError = "";

            strString = strString.Trim();

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "价格字符串为空";
                return -1;
            }

            bool bInPrefix = true;

            for (int i = 0; i < strString.Length; i++)
            {
                if ((strString[i] >= '0' && strString[i] <= '9')
                    || strString[i] == '.')
                {
                    bInPrefix = false;
                    strValue += strString[i];
                }
                else
                {
                    if (bInPrefix == true)
                        strPrefix += strString[i];
                    else
                    {
                        strPostfix = strString.Substring(i).Trim();
                        break;
                    }
                }
            }

            return 0;
        }

        // 针对一个（册信息）子字段组的描述
        class ItemGroup
        {
            public string strBarcode = "";
            public string strRegisterNo = "";
            public string strValue = "";
            public string strMergeComment = ""; // 合并过程细节注释

            // 从另一Group对象中合并必要的子字段值过来
            // 2008/4/14 new add
            // parameters:
            //      strSubfieldNames    若干个需要合并的子字段名 2008/12/28 new add
            public void MergeValue(ItemGroup group,
                string strSubfieldNames)
            {
                int nRet = 0;
                // string strSubfieldNames = "b";  // 若干个需要合并的子字段名

                for (int i = 0; i < strSubfieldNames.Length; i++)
                {
                    char subfieldname = strSubfieldNames[i];

                    string strSubfieldName = new string(subfieldname, 1);

                    string strSubfield = "";
                    string strNextSubfieldName = "";

                    string strValue = "";

                    // 从字段或子字段组中得到一个子字段
                    // parameters:
                    //		strText		字段内容，或者子字段组内容。
                    //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                    //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                    //					形式为'a'这样的。
                    //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                    //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                    //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = MarcUtil.GetSubfield(this.strValue,
                        ItemType.Group,
                        strSubfieldName,
                        0,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (strSubfield.Length >= 1)
                    {
                        strValue = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                    }

                    // 如果为空，才需要看看增补
                    if (String.IsNullOrEmpty(strValue) == true)
                    {
                        string strOtherValue = "";

                        strSubfield = "";
                        nRet = MarcUtil.GetSubfield(group.strValue,
                            ItemType.Group,
                            strSubfieldName,
                            0,
                            out strSubfield,
                            out strNextSubfieldName);
                        if (strSubfield.Length >= 1)
                        {
                            strOtherValue = strSubfield.Substring(1).Trim();   // 去除左右多余的空白
                        }

                        if (String.IsNullOrEmpty(strOtherValue) == false)
                        {
                            // 替换字段中的子字段。
                            // parameters:
                            //		strField	[in,out]待替换的字段
                            //		strSubfieldName	要替换的子字段的名，内容为1字符。如果==null，表示任意子字段
                            //					形式为'a'这样的。
                            //		nIndex		要替换的子字段所在序号。如果为-1，将始终为在字段中追加新子字段内容。
                            //		strSubfield	要替换成的新子字段。注意，其中第一字符为子字段名，后面为子字段内容
                            // return:
                            //		-1	出错
                            //		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
                            //		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
                            nRet = MarcUtil.ReplaceSubfield(ref this.strValue,
                                strSubfieldName,
                                0,
                                strSubfieldName + strOtherValue);
                        }
                    }
                }
            }
        }

        #region 订购数据的升级

        // 将一条MARC记录中包含的期刊采购信息变成XML格式并上传
        // parameters:
        //      strOrderDbName 订购数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int BuildSeriesOrderRecords(
            //string strBiblioRecPath,
            //string strOrderDbName,
            //string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            //out int nThisOrderCount,
            out List<string> order_xmls,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            // nThisOrderCount = 0;
            order_xmls = new List<string>();

            int nRet = 0;

            string strField910 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            /*
            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";

            List<EntityInfo> orderArray = new List<EntityInfo>();
            */

            for (int i = 0; i <= 4; i++)
            {
                string strFieldName = "91" + i.ToString();

                // 获得91X字段
                nRet = MarcUtil.GetField(strMARC,
                    strFieldName,
                    0,
                    out strField910,
                    out strNextFieldName);
                if (nRet == -1)
                {
                    strError = "从MARC记录中获得" + strFieldName + "字段时出错";
                    return -1;
                }
                if (nRet == 0)
                    continue;

                List<NormalGroup> groups_910 = null;

                nRet = BuildNormalGroups(strField910,
                    out groups_910,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "在将" + strFieldName + "字段分析创建groups对象过程中发生错误: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

                // 进行子字段组循环
                for (int g = 0; g < groups_910.Count; g++)
                {
                    NormalGroup group = groups_910[g];

                    string strGroup = group.strValue;

                    // 处理一个item
                    string strXml = "";

                    // 构造订购XML记录
                    // parameters:
                    //      strParentID 父记录ID
                    //      strGroup    待转换的期刊种记录的91X字段中某子字段组片断
                    //      strXml      输出的订购XML记录
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = BuildSeriesOrderXmlRecord(
                        strFieldName,
                        g,  // nThisOrderCount,
                        //strParentRecordID,
                        strGroup,
                        strMARC,
                        out strXml,
                        out strWarningParam,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "创建订购(序号) " + Convert.ToString(g + 1) + "的订购记录时发生错误: " + strError;
                        return -1;
                    }

                    if (String.IsNullOrEmpty(strWarningParam) == false)
                        strWarning += strWarningParam + "; ";

                    order_xmls.Add(strXml);
                }
            }

            return 0;
        }

        // 将一条MARC记录中包含的图书采购信息变成XML格式并上传
        // TODO: 完成期刊订购数据的升级
        // parameters:
        //      strOrderDbName 订购数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int BuildBookOrderRecords(
            // string strBiblioRecPath,
            // string strOrderDbName,
            // string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            // out int nThisOrderCount,
            out List<string> order_xmls,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            // nThisOrderCount = 0;
            order_xmls = new List<string>();

            int nRet = 0;

            string strField960 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            /*
            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";
            */

            // 获得960字段
            nRet = MarcUtil.GetField(strMARC,
                "960",
                0,
                out strField960,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得960字段时出错";
                return -1;
            }
            if (nRet == 0)
                return 0;

            /*
            {
                // 修正986字段内容
                if (strField986.Length <= 5 + 2)
                    strField986 = "";
                else
                {
                    string strPart = strField986.Substring(5, 2);

                    string strDollarA = new string(MarcUtil.SUBFLD, 1) + "a";

                    if (strPart != strDollarA)
                    {
                        strField986 = strField986.Insert(5, strDollarA);
                    }
                }
            }*/

            List<NormalGroup> groups_960 = null;

            nRet = BuildNormalGroups(strField960,
                out groups_960,
                out strWarningParam,
                out strError);
            if (nRet == -1)
            {
                strError = "在将960字段分析创建groups对象过程中发生错误: " + strError;
                return -1;
            }

            if (String.IsNullOrEmpty(strWarningParam) == false)
                strWarning += strWarningParam + "; ";

            // List<EntityInfo> orderArray = new List<EntityInfo>();

            // 进行子字段组循环
            for (int g = 0; g < groups_960.Count; g++)
            {
                NormalGroup group = groups_960[g];

                string strGroup = group.strValue;

                // 处理一个item
                string strXml = "";

                // 构造订购XML记录
                // parameters:
                //      strParentID 父记录ID
                //      strGroup    待转换的图书种记录的960字段中某子字段组片断
                //      strXml      输出的订购XML记录
                // return:
                //      -1  出错
                //      0   成功
                nRet = BuildBookOrderXmlRecord(
                    g,
                    // strParentRecordID,
                    strGroup,
                    strMARC,
                    out strXml,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建订购(序号) " + Convert.ToString(g + 1) + "的订购记录时发生错误: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

                order_xmls.Add(strXml);
            }

            return 0;
        }

        // 针对一个普通子字段组的描述
        public class NormalGroup
        {
            public string strValue = "";
        }

        // 根据一个MARC字段，创建NormalGroup数组
        public int BuildNormalGroups(string strField,
            out List<NormalGroup> groups,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            groups = new List<NormalGroup>();
            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                NormalGroup group = new NormalGroup();
                group.strValue = strGroup;

                groups.Add(group);
            }

            return 0;
        }

        // 构造期刊订购XML记录
        // parameters:
        //      strOrderFieldName   订购字段名
        //      nOrderIndex 同一种内的订购记录编号，从0开始计数。注意，不再是nGroupIndex
        //      strParentID 父记录ID
        //      strGroup    待转换的期刊种记录的91X字段中某子字段组片断
        //      strXml      输出的订购XML记录
        // return:
        //      -1  出错
        //      0   成功
        int BuildSeriesOrderXmlRecord(
            string strOrderFieldName,
            int nOrderIndex,
            // string strParentID,
            string strGroup,
            string strMARC,
            out string strXml,
            out string strWarning,
            out string strError)
        {
            strXml = "";
            strWarning = "";
            strError = "";

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 父记录id
            // DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

            // 编号
            DomUtil.SetElementText(dom.DocumentElement, "index", (nOrderIndex + 1).ToString());

            // 订购批次号
            string strSubfield = "";
            string strNextSubfieldName = "";
            // 从字段或子字段组中得到一个子字段
            // parameters:
            //		strText		字段内容，或者子字段组内容。
            //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
            //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
            //					形式为'a'这样的。
            //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
            //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
            //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中
            int nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "a",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strBatchNo = strSubfield.Substring(1);

                DomUtil.SetElementText(dom.DocumentElement, "batchNo", strBatchNo);
            }

            // $y 订购年
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "y",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strRange = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strRange) == false)
                {
                    // 变换为订购时间范围
                    if (strRange.Length == 4)
                        strRange = strRange + "0101-" + strRange + "1231";

                    DomUtil.SetElementText(dom.DocumentElement, "range", strRange);
                }
            }


            // $Y 书目号(邮发号)
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "Y",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strCatalogNo = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strCatalogNo) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "catalogNo", strCatalogNo);
                }
            }

            // $t 订购日期(操作日期)
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "t",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strOrderTime = strSubfield.Substring(1).Trim();

                // 格式为 20060625， 需要转换为rfc1123
                if (strOrderTime.Length == 8)
                {
                    string strTarget = "";

                    nRet = ReaderInfoForm.Date8toRfc1123(strOrderTime,
                    out strTarget,
                    out strError);
                    if (nRet == -1)
                    {
                        strWarning += "子字段组中$t内容中的订购日期 '" + strOrderTime + "' 格式出错: " + strError;
                        strOrderTime = "";
                    }
                    else
                    {
                        strOrderTime = strTarget;
                    }

                }
                else if (String.IsNullOrEmpty(strOrderTime) == false)
                {
                    strWarning += "$t中日期值 '" + strOrderTime + "' 格式错误，长度应为8字符; ";
                    strOrderTime = "";
                }

                if (String.IsNullOrEmpty(strOrderTime) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "orderTime", strOrderTime);
                }
            }

            // 2009/2/23 changed
            string strSeller = "";

            if (strOrderFieldName == "912")
                strSeller = "直订";
            else if (strOrderFieldName == "913")
                strSeller = "交换";
            else if (strOrderFieldName == "914")
                strSeller = "呈缴";
            else
            {

                // $o 书商名称(订购方式)
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "o",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strSeller = strSubfield.Substring(1);
                }

                // 如果$o为空，则用订购字段名表达的渠道来代替
                if (String.IsNullOrEmpty(strSeller) == true)
                {
                    if (strOrderFieldName == "910")
                        strSeller = "邮发";
                    else if (strOrderFieldName == "911")
                        strSeller = "非邮发";
                }
                else
                {
                    FillValueTable(this.m_sellers,
                        strSeller);
                }
            }

            DomUtil.SetElementText(dom.DocumentElement, "seller", strSeller);

            // $b 复本量(复本数)
            int nCopy = 0;
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "b",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strCopy = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strCopy) == false)
                {
                    try
                    {
                        nCopy = Convert.ToInt32(strCopy);
                    }
                    catch
                    {
                        strWarning += "$b中复本量值 '" + strCopy + "' 格式错误，应为纯数字; ";
                    }
                }

                if (nCopy > 1000 || nCopy < 0)
                {
                    strWarning += "$b中复本量值 '" + strCopy + "' 数值可能有错误，应小于1000，并为正整数; ";
                    if (nCopy > 1000)
                        nCopy = 1000;
                    else if (nCopy < 0)
                        nCopy = 0;
                }
            }

            if (nCopy > 0)
                DomUtil.SetElementText(dom.DocumentElement, "copy", nCopy.ToString());


            // $x 订购价(单价)
            string strPrice = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "x",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strPrice = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    // TODO: 是否需要格式检查和转换?
                }
            }

            if (String.IsNullOrEmpty(strPrice) == false)
                DomUtil.SetElementText(dom.DocumentElement, "price", strPrice);

            string strJiduPrice = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "p",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strJiduPrice = strSubfield.Substring(1);
            }

            // $d 频次。即一年出多少期
            string strIssueCount = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "d",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strIssueCount = strSubfield.Substring(1);
            }

            // 如果$d没有内容而$x $p有内容，仍可以计算出出版频次
            if (strIssueCount == ""
                && (String.IsNullOrEmpty(strPrice) == false && String.IsNullOrEmpty(strJiduPrice) == false))
            {
                // TODO: 从$p(全年)除以$x(单价)的倍数，可以得出一年出多少期
                double price = 0;
                double jidu_price = 0;

                try
                {
                    price = Convert.ToDouble(strPrice);
                }
                catch
                {
                    goto SKIP_ISSUE_COUNT;
                }

                try
                {
                    jidu_price = Convert.ToDouble(strJiduPrice);
                }
                catch
                {
                    goto SKIP_ISSUE_COUNT;
                }

                double n = jidu_price / price;
                strIssueCount = n.ToString();
            }

            if (String.IsNullOrEmpty(strIssueCount) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "issueCount", strIssueCount);
            }

        SKIP_ISSUE_COUNT:

            // $k 类别(学科)
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "k",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strClass = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strClass) == false)
                {
                    /*
                    FillValueTable(this.m_orderclasses,
                        strClass);
                     * */

                    DomUtil.SetElementText(dom.DocumentElement, "class", strClass);
                }
            }


            // 状态
            // DomUtil.SetElementText(dom.DocumentElement, "state", strState);


            // 附注 $z
            string strComment = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "z",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strComment = strSubfield.Substring(1);
            }

            if (String.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "comment", strComment);
            }

            // 渠道地址
            {

                XmlDocument address_dom = new XmlDocument();
                address_dom.LoadXml("<sellerAddress />");

                // 编辑部地址 $w
                string strAddress = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "w",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strAddress = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strAddress) == false)
                {
                    DomUtil.SetElementText(address_dom.DocumentElement,
                        "address", strAddress);
                }

                // 开户行 $m
                string strBank = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "m",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBank = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBank) == false)
                {
                    DomUtil.SetElementText(address_dom.DocumentElement,
                        "bank", strBank);
                }

                // 银行账号 $h
                string strAccounts = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "h",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strAccounts = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strAccounts) == false)
                {
                    DomUtil.SetElementText(address_dom.DocumentElement,
                        "accounts", strAccounts);
                }

                // 汇款方式 $Q
                string strPayStyle = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "Q",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strPayStyle = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strPayStyle) == false)
                {
                    DomUtil.SetElementText(address_dom.DocumentElement,
                        "payStyle", strPayStyle);
                }

                if (address_dom.DocumentElement.ChildNodes.Count > 0)
                {
                    /*
                    XmlNode node = DomUtil.SetElementText(dom.DocumentElement, "sellerAddress", "");
                    node.OuterXml = address_dom.DocumentElement.OuterXml;
                     * */
                    DomUtil.SetElementInnerXml(dom.DocumentElement,
                        "sellerAddress",
                        address_dom.DocumentElement.InnerXml);
                }
            }

            strXml = dom.OuterXml;
            return 0;
        }

        // 构造图书订购XML记录
        // parameters:
        //      nGroupIndex 子字段组的编号，从0开始计数
        //      strParentID 父记录ID
        //      strGroup    待转换的图书种记录的960字段中某子字段组片断
        //      strXml      输出的订购XML记录
        // return:
        //      -1  出错
        //      0   成功
        int BuildBookOrderXmlRecord(
            int nGroupIndex,
            // string strParentID,
            string strGroup,
            string strMARC,
            out string strXml,
            out string strWarning,
            out string strError)
        {
            strXml = "";
            strWarning = "";
            strError = "";

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 父记录id
            // DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

            // 编号
            DomUtil.SetElementText(dom.DocumentElement, "index", (nGroupIndex + 1).ToString());

            // 订购批次号
            string strSubfield = "";
            string strNextSubfieldName = "";
            // 从字段或子字段组中得到一个子字段
            // parameters:
            //		strText		字段内容，或者子字段组内容。
            //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
            //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
            //					形式为'a'这样的。
            //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
            //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
            //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中
            int nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "a",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strBatchNo = strSubfield.Substring(1);

                DomUtil.SetElementText(dom.DocumentElement, "batchNo", strBatchNo);
            }

            // 书目号
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "b",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strCatalogNo = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strCatalogNo) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "catalogNo", strCatalogNo);
                }
            }

            // 订购日期
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "c",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strOrderTime = strSubfield.Substring(1).Trim();

                // 格式为 20060625， 需要转换为rfc
                if (strOrderTime.Length == 8)
                {
                    string strTarget = "";

                    nRet = ReaderInfoForm.Date8toRfc1123(strOrderTime,
                    out strTarget,
                    out strError);
                    if (nRet == -1)
                    {
                        strWarning += "子字段组中$c内容中的订购日期 '" + strOrderTime + "' 格式出错: " + strError;
                        strOrderTime = "";
                    }
                    else
                    {
                        strOrderTime = strTarget;
                    }

                }
                else if (String.IsNullOrEmpty(strOrderTime) == false)
                {
                    strWarning += "$c中日期值 '" + strOrderTime + "' 格式错误，长度应为8字符; ";
                    strOrderTime = "";
                }

                if (String.IsNullOrEmpty(strOrderTime) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement, "orderTime", strOrderTime);
                }
            }

            // 书商名称
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "d",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strSeller = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strSeller) == false)
                {
                    FillValueTable(this.m_sellers,
                        strSeller);
                    DomUtil.SetElementText(dom.DocumentElement, "seller", strSeller);
                }
            }

            // 复本量(复本数)
            int nCopy = 0;
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "e",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strCopy = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strCopy) == false)
                {
                    try
                    {
                        nCopy = Convert.ToInt32(strCopy);
                    }
                    catch
                    {
                        strWarning += "$e中复本量值 '" + strCopy + "' 格式错误，应为纯数字; ";
                    }
                }

                if (nCopy > 1000 || nCopy < 0)
                {
                    strWarning += "$e中复本量值 '" + strCopy + "' 数值可能有错误，应小于1000，并为正整数; ";
                    if (nCopy > 1000)
                        nCopy = 1000;
                    else if (nCopy < 0)
                        nCopy = 0;
                }
            }

            // 订购价(单价)
            string strPrice = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "f",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strPrice = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    // TODO: 是否需要格式检查和转换?
                }
            }

            // 类别
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "g",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strClass = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strClass) == false)
                {
                    FillValueTable(this.m_orderclasses,
                        strClass);

                    DomUtil.SetElementText(dom.DocumentElement, "class", strClass);
                }
            }

            // 订购单位 $h
            // TODO: 是否等于馆藏分配策略?

            // 到书批次号 $j
            // 到书日期 $k

            // 已到复本量 $l
            int nAcceptedCopy = 0;
            string strAcceptedCopyComment = ""; // 从已到复本量中分离出来的非数字部分
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "l",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strAcceptedCopy = strSubfield.Substring(1);

                if (String.IsNullOrEmpty(strAcceptedCopy) == false)
                {
                    RemoveNoneNumberPart(ref strAcceptedCopy,
                        out strAcceptedCopyComment);
                }

                if (String.IsNullOrEmpty(strAcceptedCopy) == false)
                {
                    try
                    {
                        nAcceptedCopy = Convert.ToInt32(strAcceptedCopy);
                    }
                    catch
                    {
                        strWarning += "$l中已到复本量值 '" + strAcceptedCopy + "' 格式错误，应为纯数字; ";
                    }
                }

                if (nAcceptedCopy > 1000 || nAcceptedCopy < 0)
                {
                    strWarning += "$l中已到复本量值 '" + nAcceptedCopy + "' 数值可能有错误，应小于1000，并为正整数; ";
                    if (nAcceptedCopy > 1000)
                        nAcceptedCopy = 1000;
                    else if (nAcceptedCopy < 0)
                        nAcceptedCopy = 0;
                }
            }

            // 馆藏分配策略
            string strDistribute = "";
            if (nAcceptedCopy > 0 && nAcceptedCopy < 100)   // 附加的限制
            {
                // 有验收的情况
                strDistribute = "(未知):" + nAcceptedCopy.ToString();

                string strIdString = "";
                for (int i = 0; i < nAcceptedCopy; i++)
                {
                    if (String.IsNullOrEmpty(strIdString) == false)
                        strIdString += ",";
                    strIdString += "#null";
                }

                strDistribute = strDistribute + "{" + strIdString + "}";

                if (nCopy > nAcceptedCopy)
                {
                    int nDelta = nCopy - nAcceptedCopy;
                    strDistribute += ";(未知):" + nDelta.ToString();
                }
            }
            else if (nCopy > 0)
            {
                // 还没有验收的情况
                strDistribute = "(未知):" + nCopy.ToString();
            }

            if (String.IsNullOrEmpty(strDistribute) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "distribute", strDistribute);
            }

            // 货币名称及结算价 $m
            string strAcceptedPrice = "";
            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "m",
    0,
    out strSubfield,
    out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strAcceptedPrice = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strAcceptedPrice) == false)
                {
                    // TODO: 是否需要格式检查和转换?
                }
            }


            // 付款凭证 $p
            // 付款日期 $q

            // 资金来源 $s
            nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "s",
    0,
    out strSubfield,
    out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                string strSource = strSubfield.Substring(1);
                if (String.IsNullOrEmpty(strSource) == false)
                {
                    FillValueTable(this.m_sources,
                        strSource);

                    DomUtil.SetElementText(dom.DocumentElement, "source", strSource);
                }
            }

            // 报销日期 $t
            // 报销凭证 $u


            // 复本数合成
            if (nAcceptedCopy > 0)
            {
                string strFinalCopy = "";
                if (nCopy > 0)
                {
                    if (nAcceptedCopy == nCopy)
                        strFinalCopy = nCopy.ToString() + "[=]";
                    else
                        strFinalCopy = nCopy.ToString() + "[" + nAcceptedCopy.ToString() + "]";
                }
                else
                {
                    if (nAcceptedCopy > 0)
                    {
                        strFinalCopy = nAcceptedCopy.ToString() + "[" + nAcceptedCopy.ToString() + "]";
                    }
                    else
                        strFinalCopy = nCopy.ToString();
                }

                DomUtil.SetElementText(dom.DocumentElement, "copy", strFinalCopy);
            }
            else
            {
                if (nCopy > 0)
                    DomUtil.SetElementText(dom.DocumentElement, "copy", nCopy.ToString());
            }

            // 单价字符串合成
            if (nAcceptedCopy > 0)
            {
                string strFinalPrice = "";
                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    if (strAcceptedPrice == strPrice)
                        strFinalPrice = strPrice + "[=]";
                    else
                        strFinalPrice = strPrice + "[" + strAcceptedPrice + "]";
                }
                else
                {
                    if (String.IsNullOrEmpty(strAcceptedPrice) == false)
                    {
                        strFinalPrice = strAcceptedPrice + "[" + strAcceptedPrice + "]";
                    }
                    else
                        strFinalPrice = strPrice;
                }

                DomUtil.SetElementText(dom.DocumentElement, "price", strFinalPrice);
            }
            else
            {
                if (String.IsNullOrEmpty(strPrice) == false)
                    DomUtil.SetElementText(dom.DocumentElement, "price", strPrice);
            }


            // 状态
            // 都设置为“已订购”? 至少到了一册的，设置为“已验收”?
            string strState = "已订购";

            if (nAcceptedCopy > 0)
                strState = "已验收";

            DomUtil.SetElementText(dom.DocumentElement, "state", strState);


            // 附注 $z
            string strComment = "";
            nRet = MarcUtil.GetSubfield(strGroup,
                ItemType.Group,
                "z",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length >= 1)
            {
                strComment = strSubfield.Substring(1);
            }

            // 加上从已到复本数中剥离的文字
            if (String.IsNullOrEmpty(strAcceptedCopyComment) == false)
            {
                if (String.IsNullOrEmpty(strComment) == false)
                    strComment += "; ";
                strComment += strAcceptedCopyComment;
            }

            if (String.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "comment", strComment);
            }

            strXml = dom.OuterXml;

            return 0;
        }

        // 分离字符串中数字和非数字部分
        static void RemoveNoneNumberPart(ref string strText,
            out string strNoneNumber)
        {
            strNoneNumber = "";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch < '0' || ch > '9')
                {
                    strNoneNumber = strText.Substring(i);
                    strText = strText.Substring(0, i);
                    return;
                }
            }

            return; // 全部都是数字部分
        }

        #endregion

    }
}
