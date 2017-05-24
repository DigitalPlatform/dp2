using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{

#if NO
    /// <summary>
    /// 读者权限 属性页
    /// </summary>
    public partial class ManagerForm
    {
        int m_nRightsTableXmlVersion = 0;
        int m_nRightsTableHtmlVersion = 0;
        int m_nRightsTableTypesVersion = 0;

#if NO
        // types编辑界面 --> DOM中的<readerTypes>和<bookTypes>部分
        // 调用前dom中应当已经装入了权限XML代码
        void TypesToRightsXml(ref XmlDocument dom)
        {
            string strReaderTypesXml = BuildTypesXml(this.textBox_loanPolicy_readerTypes);
            string strBookTypesXml = BuildTypesXml(this.textBox_loanPolicy_bookTypes);

            {
                XmlNode nodeReaderTypes = dom.DocumentElement.SelectSingleNode("readerTypes");
                if (nodeReaderTypes == null)
                {
                    nodeReaderTypes = dom.CreateElement("readerTypes");
                    dom.DocumentElement.AppendChild(nodeReaderTypes);
                }

                nodeReaderTypes.InnerXml = strReaderTypesXml;
            }

            {
                XmlNode nodeBookTypes = dom.DocumentElement.SelectSingleNode("bookTypes");
                if (nodeBookTypes == null)
                {
                    nodeBookTypes = dom.CreateElement("bookTypes");
                    dom.DocumentElement.AppendChild(nodeBookTypes);
                }

                nodeBookTypes.InnerXml = strBookTypesXml;
            }
        }
#endif

        /*public*/ void FinishLibraryCodeTextbox()
        {
                            // 把当前textbox中的收尾
                if (m_currentLibraryCodeItem != null)
                {
                    LibraryCodeInfo info = (LibraryCodeInfo)m_currentLibraryCodeItem.Tag;
                    if (info == null)
                    {
                        info = new LibraryCodeInfo();
                        m_currentLibraryCodeItem.Tag = info;
                    }

                    if (info.BookTypeList != this.textBox_loanPolicy_bookTypes.Text)
                    {
                        info.BookTypeList = this.textBox_loanPolicy_bookTypes.Text;
                        info.Changed = true;
                    }
                    if (info.ReaderTypeList != this.textBox_loanPolicy_readerTypes.Text)
                    {
                        info.ReaderTypeList = this.textBox_loanPolicy_readerTypes.Text;
                        info.Changed = true;
                    }
                }

        }

        // types编辑界面 --> DOM中的<readerTypes>和<bookTypes>部分
        // 调用前dom中应当已经装入了权限XML代码
        bool TypesToRightsXml(ref XmlDocument dom)
        {
            // 结束一下当前textbox
            FinishLibraryCodeTextbox();

            bool bChanged = false;

            XmlNode root = dom.DocumentElement;

            // 对每个馆代码循环
            foreach (ListViewItem item in this.listView_loanPolicy_libraryCodes.Items)
            {
                // string strLibraryCode = item.Text;
                LibraryCodeInfo info = (LibraryCodeInfo)item.Tag;

                /*
                if (info.Changed == false)
                    continue;
                 * */
                string strLibraryCode = info.LibraryCode;
                string strReaderTypesXml = BuildTypesXml(info.ReaderTypeList);
                string strBookTypesXml = BuildTypesXml(info.BookTypeList);

                string strFilter = "";
                if (string.IsNullOrEmpty(strLibraryCode) == false)
                {
                    XmlNode temp = root.SelectSingleNode("//library[@code='" + strLibraryCode + "']");
                    if (temp == null)
                    {
                        temp = dom.CreateElement("library");
                        root.AppendChild(temp);
                        DomUtil.SetAttr(temp, "code", strLibraryCode);
                    }
                    root = temp;
                }
                else
                {
                    strFilter = "[count(ancestor::library) = 0]";
                }

                {
                XmlNode nodeReaderTypes = root.SelectSingleNode("descendant::readerTypes" + strFilter);

                    if (nodeReaderTypes == null)
                    {
                        nodeReaderTypes = dom.CreateElement("readerTypes");
                        root.AppendChild(nodeReaderTypes);
                    }

                    nodeReaderTypes.InnerXml = strReaderTypesXml;
                }

                {
                    XmlNode nodeBookTypes = root.SelectSingleNode("descendant::bookTypes" + strFilter);


                    if (nodeBookTypes == null)
                    {
                        nodeBookTypes = dom.CreateElement("bookTypes");
                        root.AppendChild(nodeBookTypes);
                    }

                    nodeBookTypes.InnerXml = strBookTypesXml;
                }

                if (info.Changed == true)
                    bChanged = true;

                info.Changed = false;
            }

            return bChanged;
        }

#if NO
        // DOM中的<readerTypes>和<bookTypes>部分 --> types编辑界面
        void RightsXmlToTypes(XmlDocument dom,
            string strLibraryCode)
        {
            string strFilter = "";

            XmlNode root = dom.DocumentElement;
            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("//library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return;
                root = temp;
            }
            else
            {
                strFilter = "[count(ancestor::library) = 0]";
            }


            // readertypes
            
            {
                XmlNodeList nodes = root.SelectNodes("descendant::readerTypes/item" + strFilter);
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }

                if (this.textBox_loanPolicy_readerTypes.Text != strText)
                {
                    //this.textBox_loanPolicy_readerTypes.Text = strText;
                    SetText(this.textBox_loanPolicy_readerTypes,
                        strText);
                }
            }

            // booktypes
            {
                XmlNodeList nodes = root.SelectNodes("descendant::bookTypes/item" + strFilter);
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }
                if (this.textBox_loanPolicy_bookTypes.Text != strText)
                {
                    // this.textBox_loanPolicy_bookTypes.Text = strText;
                    SetText(this.textBox_loanPolicy_bookTypes,
                        strText);
                }
            }
        }
#endif

        // DOM中的<readerTypes>和<bookTypes>部分 --> types编辑界面
        LibraryCodeInfo GetLibraryCodeInfo(XmlDocument dom,
            string strLibraryCode)
        {
            LibraryCodeInfo info = new LibraryCodeInfo();

            info.LibraryCode = strLibraryCode;

            string strFilter = "";
            XmlNode root = dom.DocumentElement;
            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                XmlNode temp = root.SelectSingleNode("//library[@code='" + strLibraryCode + "']");
                if (temp == null)
                    return info;
                root = temp;
            }
            else
            {
                strFilter = "[count(ancestor::library) = 0]";
            }


            // readertypes

            {
                XmlNodeList nodes = root.SelectNodes("descendant::readerTypes/item" + strFilter);
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }

                info.ReaderTypeList = strText;
            }

            // booktypes
            {
                XmlNodeList nodes = root.SelectNodes("descendant::bookTypes/item" + strFilter);
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }

                info.BookTypeList = strText;
            }

            return info;
        }


        // 在listview中列出馆代码
        void ListLibraryCodes(XmlDocument dom)
        {
            List<string> librarycodes = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                librarycodes.Add(strCode);
            }

            // 看看是否还有不属于任何<library>元素的
            nodes = dom.DocumentElement.SelectNodes("//param[count(ancestor::library) = 0]");
            if (nodes.Count > 0)
            {
                if (librarycodes.IndexOf("") == -1)
                    librarycodes.Insert(0, "");
            }

            // 
            this.m_currentLibraryCodeItem = null;
            this.listView_loanPolicy_libraryCodes.Items.Clear();
            foreach (string strLibraryCode in librarycodes)
            {
                ListViewItem item = new ListViewItem(string.IsNullOrEmpty(strLibraryCode) == true ? "<缺省>" : strLibraryCode);
                LibraryCodeInfo info = GetLibraryCodeInfo(dom,
                    strLibraryCode);
                item.Tag = info;

                this.listView_loanPolicy_libraryCodes.Items.Add(item);
            }

            this.textBox_loanPolicy_readerTypes.Text = "";
            this.textBox_loanPolicy_bookTypes.Text = "";

            // 选定第一行
            if (this.listView_loanPolicy_libraryCodes.Items.Count > 0)
                this.listView_loanPolicy_libraryCodes.Items[0].Selected = true;
        }

        void SetRightsTableHtml(string strRightsTableHtml)
        {
            Global.SetHtmlString(this.webBrowser_rightsTableHtml,
    "<html><body style='font-family:\"Microsoft YaHei\", Times, serif;'>" + strRightsTableHtml + "</body></html>");
        }

        int ListRightsTables(out string strError)
        {
            strError = "";

            if (this.LoanPolicyDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内读者流通权限定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            /*
            // 2008/10/12
            if (this.LoanPolicyDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有读者流通权限定义被修改后尚未保存。若此时重新装载读者流通权限定义，现有未保存信息将丢失。\r\n\r\n确实要重新装载? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }*/

            string strRightsTableXml = "";
            string strRightsTableHtml = "";
            //string strReaderTypesXml = "";
            //string strBookTypesXml = "";

            // 获得流通读者权限相关定义
            int nRet = GetRightsTableInfo(out strRightsTableXml,
                out strRightsTableHtml,
                //out strReaderTypesXml,
                //out strBookTypesXml,
                out strError);
            if (nRet == -1)
                return -1;

            strRightsTableXml = "<rightsTable>" + strRightsTableXml + "</rightsTable>";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strRightsTableXml);
            }
            catch (Exception ex)
            {
                strError = "strRightsTableXml装入XMLDOM时发生错误：" + ex.Message;
                return -1;
            }

            /*
            // readertypes
            this.textBox_loanPolicy_readerTypes.Text = "";
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("readerTypes/item");
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }
                this.textBox_loanPolicy_readerTypes.Text = strText;
            }

            // booktypes
            this.textBox_loanPolicy_bookTypes.Text = "";
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("bookTypes/item");
                string strText = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";
                    strText += nodes[i].InnerText;
                }
                this.textBox_loanPolicy_bookTypes.Text = strText;
            }
             * */

            // 在listview中列出馆代码
            ListLibraryCodes(dom);

            // RightsXmlToTypes(dom);

            /*
            // TODO: 为了让XML源代码不至于让人误会(想要去编辑<readerTypes>和<bookTypes>)，是否要把这两个元素去掉?
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("readerTypes | bookTypes");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    node.ParentNode.RemoveChild(node);
                }
            }*/

            string strXml = "";
            nRet = DomUtil.GetIndentXml(dom.DocumentElement.OuterXml,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.textBox_loanPolicy_rightsTableDef.Text = strXml;
            SetRightsTableHtml(strRightsTableHtml);


            this.LoanPolicyDefChanged = false;

            this.m_nRightsTableHtmlVersion = 0;
            this.m_nRightsTableXmlVersion = 0;
            this.m_nRightsTableTypesVersion = 0;

            return 1;
        }

        // 获得流通读者权限相关定义
        int GetRightsTableInfo(out string strRightsTableXml,
            out string strRightsTableHtml,
            //out string strReaderTypesXml,
            //out string strBookTypesXml,
            out string strError)
        {
            strError = "";
            strRightsTableXml = "";
            strRightsTableHtml = "";
            //strReaderTypesXml = "";
            //strBookTypesXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取读者流通权限定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTable",
                    out strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTableHtml",
                    out strRightsTableHtml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                /*
                lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "readerTypes",
                    out strReaderTypesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "bookTypes",
                    out strBookTypesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                 * */

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 保存流通读者权限相关定义
        // parameters:
        //      strRightsTableXml   流通读者权限定义XML。注意，没有根元素
        int SetRightsTableDef(string strRightsTableXml,
            //string strReaderTypesXml,
            //string strBookTypesXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者流通权限定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTable",
                    strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                /*
                lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "readerTypes",
                    strReaderTypesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "bookTypes",
                    strBookTypesXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                 * */

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

#if NO
        static void SetText(TextBox textbox, string strText)
        {
            int nStart = 0;
            int nLength = 0;

            nStart = textbox.SelectionStart;
            nLength = textbox.SelectionLength;

            textbox.Text = strText;

            textbox.SelectionStart = nStart;
            textbox.SelectionLength = nLength;
        }
#endif

        void SynchronizeLoanPolicy()
        {
            SynchronizeRightsTableAndTypes();
            SynchronizeRightsTableAndHtml();
        }

        // 同步读者权限XML定义和读者/图书类型编辑界面
        int SynchronizeRightsTableAndTypes()
        {
            string strError = "";

            if (this.m_nRightsTableXmlVersion == this.m_nRightsTableTypesVersion)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.textBox_loanPolicy_rightsTableDef.Text);
            }
            catch (Exception ex)
            {
                strError = "读者权限XML代码格式有误: " + ex.Message;
                goto ERROR1;
            }


            // XML代码更新
            if (this.m_nRightsTableXmlVersion > this.m_nRightsTableTypesVersion)
            {
                // RightsXmlToTypes(dom);
                ListLibraryCodes(dom);
                this.m_nRightsTableTypesVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

            // types编辑界面更新
            if (this.m_nRightsTableXmlVersion < this.m_nRightsTableTypesVersion)
            {
                // types编辑界面 --> DOM中的<readerTypes>和<bookTypes>部分
                // 调用前dom中应当已经装入了权限XML代码
                TypesToRightsXml(ref dom);

                // 刷新XML文本框
                string strXml = "";
                int nRet = DomUtil.GetIndentXml(dom.DocumentElement.OuterXml,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (this.textBox_loanPolicy_rightsTableDef.Text != strXml)
                {
                    // this.textBox_loanPolicy_rightsTableDef.Text = strXml;
                    SetText(this.textBox_loanPolicy_rightsTableDef,
                        strXml);
                }

                this.m_nRightsTableXmlVersion = this.m_nRightsTableTypesVersion;
                return 0;
            }

            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 同步读者权限XML定义和流通权限表HTML显示
        int SynchronizeRightsTableAndHtml()
        {
            string strError = "";

            if (this.m_nRightsTableXmlVersion == this.m_nRightsTableHtmlVersion)
                return 0;


            string strRightsTableXml = this.textBox_loanPolicy_rightsTableDef.Text;
            string strRightsTableHtml = "";

            if (String.IsNullOrEmpty(strRightsTableXml) == true)
            {
                Global.ClearHtmlPage(this.webBrowser_rightsTableHtml,
                    Program.MainForm.DataDir);
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strRightsTableXml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入XMLDOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            if (dom.DocumentElement == null)
            {
                Global.ClearHtmlPage(this.webBrowser_rightsTableHtml,
                    Program.MainForm.DataDir);
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

#if NO
            // 在发出去的XML中增加<readerTypes>和<bookTypes>参数
            string strReaderTypesXml = BuildTypesXml(this.textBox_loanPolicy_readerTypes);
            string strBookTypesXml = BuildTypesXml(this.textBox_loanPolicy_bookTypes);

            XmlNode node_readertypes = dom.CreateElement("readerTypes");
            XmlNode node_booktypes = dom.CreateElement("bookTypes");
            dom.DocumentElement.AppendChild(node_readertypes);
            dom.DocumentElement.AppendChild(node_booktypes);
            if (String.IsNullOrEmpty(strReaderTypesXml) == false)
                node_readertypes.InnerXml = strReaderTypesXml;
            if (String.IsNullOrEmpty(strBookTypesXml) == false)
                node_booktypes.InnerXml = strBookTypesXml;

#endif
            // 因为SynchronizeRightsTableAndHtml() 总是在稍后调用，所以不用操心types xml更新的问题

            strRightsTableXml = dom.DocumentElement.InnerXml;

            // EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取读者流通权限定义HTML ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "instance_rightstable_html",
                    strRightsTableXml,
                    out strRightsTableHtml,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }

                SetRightsTableHtml(strRightsTableHtml);
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                // EnableControls(true);
            }

        ERROR1:
            Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                HttpUtility.HtmlEncode(strError));
            return -1;
        }




        // 从<rightsTable>的权限定义代码中(而不是从<readerTypes>和<bookTypes>元素下)获得读者和图书类型列表
        int GetReaderAndBookTypes(out List<string> readertypes,
            out List<string> booktypes,
            out string strError)
        {
            strError = "";
            booktypes = new List<string>();
            readertypes = new List<string>();

            string strRightsTableXml = this.textBox_loanPolicy_rightsTableDef.Text;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strRightsTableXml);
            }
            catch (Exception ex)
            {
                strError = "读者权限XML代码装入DOM时出错: " + ex.Message;
                return -1;
            }

            // 选出所有<type>元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//type");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strReaderType = DomUtil.GetAttr(node, "reader");
                string strBookType = DomUtil.GetAttr(node, "book");

                if (String.IsNullOrEmpty(strReaderType) == false
                    && strReaderType != "*")
                {
                    readertypes.Add(strReaderType);
                    continue;
                }

                if (String.IsNullOrEmpty(strBookType) == false
                    && strBookType != "*")
                {
                    booktypes.Add(strBookType);
                    continue;
                }
            }

            StringUtil.RemoveDupNoSort(ref readertypes);

            StringUtil.RemoveDupNoSort(ref booktypes);

            return 0;
        }

        static List<string> MakeStringList(string strLines)
        {
            strLines = strLines.Replace("\r\n", "\r");
            string[] lines = strLines.Split(new char[] {'\r'});
            List<string> results = new List<string>();
            results.AddRange(lines);

            return results;
        }

        // 根据文本行创建types XML 片断
        static string BuildTypesXml(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<types/>");

            strText = strText.Replace("\r\n", "\n");
            string[] lines = strText.Split(new char[] {'\n'});
            foreach(string s in lines)
            {
                string strLine = s.Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                XmlNode node = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(node);
                node.InnerText = strLine;
            }

            return dom.DocumentElement.InnerXml;
        }

#if NO
        static string BuildTypesXml(TextBox textbox)
        {
            if (String.IsNullOrEmpty(textbox.Text) == true)
                return "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<types/>");

            for (int i = 0; i < textbox.Lines.Length; i++)
            {
                string strLine = textbox.Lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                XmlNode node = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(node);
                node.InnerText = strLine;
            }

            return dom.DocumentElement.InnerXml;
        }
#endif
    }

    internal class LibraryCodeInfo
    {
        /// <summary>
        /// 图书馆代码
        /// </summary>
        public string LibraryCode = "";

        /// <summary>
        /// 读者类型列表
        /// </summary>
        public string ReaderTypeList = "";

        /// <summary>
        /// 图书类型列表
        /// </summary>
        public string BookTypeList = "";

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;    // readertype 或 booktypes 内容发生了变化
    }

#endif
}
