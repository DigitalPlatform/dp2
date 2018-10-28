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
    /// <summary>
    /// 读者权限 属性页
    /// </summary>
    public partial class ManagerForm
    {
        // XML 编辑界面中的内容版本
        int m_nRightsTableXmlVersion = 0;
        // 权限二维表界面中的内容版本
        int m_nRightsTableTypesVersion = 0;

        // 列出读者借阅权限定义
        // 需要在 ListCalendars() 以后调用
        int NewListRightsTables(out string strError)
        {
            strError = "";
            int nRet = 0;

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

            List<string> calendar_Names = null;
            // 装入全部日历名
            nRet = GetCalendarNames(out calendar_Names,
                out strError);
            if (nRet == -1)
                return -1;


            string strRightsTableXml = "";

            // 获得流通读者权限相关定义
            nRet = GetRightsTableInfo(out strRightsTableXml,
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

#if NO
            // 在listview中列出馆代码
            ListLibraryCodes(dom);
#endif

            string strXml = "";
            nRet = DomUtil.GetIndentXml(dom.DocumentElement.OuterXml,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.textBox_newLoanPolicy_xml.Text = strXml;

            this.loanPolicyControlWrapper1.LoanPolicyControl.CalendarList = calendar_Names;
            m_nCalendarVersion = 0;

            string strLibraryCodeList = GetLibraryCodeList();
#if NO
            if (this.Channel != null)
                strLibraryCodeList = this.Channel.LibraryCodeList;

            // 2014/5/27
            if (Global.IsGlobalUser(strLibraryCodeList) == true)
                strLibraryCodeList = StringUtil.MakePathList(Program.MainForm.GetAllLibraryCode());
#endif

            nRet = this.loanPolicyControlWrapper1.LoanPolicyControl.SetData(
                strLibraryCodeList,
                strXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.LoanPolicyDefChanged = false;

            this.m_nRightsTableXmlVersion = 0;
            this.m_nRightsTableTypesVersion = 0;

            return 1;
        }

        // 2014/9/6
        // 获得馆藏地点列表字符串
        internal string GetLibraryCodeList()
        {
            string strLibraryCodeList = "";
            if (this.Channel != null)
                strLibraryCodeList = this.Channel.LibraryCodeList;

#if NO
            // 2014/5/27
            if (Global.IsGlobalUser(strLibraryCodeList) == true)
                strLibraryCodeList = StringUtil.MakePathList(Program.MainForm.GetAllLibraryCode());
#endif
            if (Global.IsGlobalUser(strLibraryCodeList) == true)
            {
                string strError = "";
                List<string> codes = null;
                int nRet = GetAllLibraryCode(out codes, out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                strLibraryCodeList = StringUtil.MakePathList(codes);
            }

            return strLibraryCodeList;
        }

        // 2014/9/7
        // 从 AllDatabaseInfoXml 中获得全部可用的馆代码
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetAllLibraryCode(out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database[@type='reader']");
            foreach (XmlElement node in nodes)
            {
                string strLibraryCode = node.GetAttribute("libraryCode");

                results.Add(strLibraryCode);
            }

            results.Sort();
            StringUtil.RemoveDup(ref results, true);
            return 0;
        }

        // 获得流通读者权限相关定义
        int GetRightsTableInfo(out string strRightsTableXml,
            out string strError)
        {
            strError = "";
            strRightsTableXml = "";

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
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者流通权限定义 ...");
            stop.BeginLoop();

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


        // 同步读者权限XML定义和读者/图书类型编辑界面
        int SynchronizeRightsTableAndXml()
        {
            string strError = "";
            int nRet = 0;

            if (this.m_nRightsTableXmlVersion == this.m_nRightsTableTypesVersion)
                return 0;

#if NO
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.textBox_loanPolicy_rightsTableDef.Text);
            }
            catch (Exception ex)
            {
                strError = "读者权限 XML 代码格式有误: " + ex.Message;
                goto ERROR1;
            }
#endif

            // XML 代码更新
            if (this.m_nRightsTableXmlVersion > this.m_nRightsTableTypesVersion)
            {
#if NO
                string strLibraryCodeList = "";
                if (this.Channel != null)
                    strLibraryCodeList = this.Channel.LibraryCodeList;
#endif
                string strLibraryCodeList = GetLibraryCodeList();

                nRet = this.loanPolicyControlWrapper1.LoanPolicyControl.SetData(
                    strLibraryCodeList,
                    this.textBox_newLoanPolicy_xml.Text, 
                    out strError);
                if (nRet == -1)
                    goto ERROR1; 
                this.m_nRightsTableTypesVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

            // 表格界面更新
            if (this.m_nRightsTableXmlVersion < this.m_nRightsTableTypesVersion)
            {
                string strXml = this.loanPolicyControlWrapper1.LoanPolicyControl.GetData();
                if (this.textBox_newLoanPolicy_xml.Text != strXml)
                {
                    // this.textBox_newLoanPolicy_xml.Text = strXml;

                    SetText(this.textBox_newLoanPolicy_xml,
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

        // 当馆代码列表发生变动后，更新到读者权限界面中
        internal void UpdateLoanPolicyLibraryCode()
        {
            SynchronizeRightsTableAndXml();

            // 重新调用一次 SetData
            {
                string strError = "";

                string strLibraryCodeList = GetLibraryCodeList();

                // TODO: 将来可以考虑提供专门更新 strLibraryCodeList 的函数
                int nRet = this.loanPolicyControlWrapper1.LoanPolicyControl.SetData(
                    strLibraryCodeList,
                    this.textBox_newLoanPolicy_xml.Text,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }
        }

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

        bool m_bLoanPolicyDefChanged = false;

        /// <summary>
        /// 读者流通权限定义是否被修改
        /// </summary>
        public bool LoanPolicyDefChanged
        {
            get
            {
                return this.m_bLoanPolicyDefChanged;
            }
            set
            {
                this.m_bLoanPolicyDefChanged = value;
                if (value == true)
                    this.toolStripButton_newLoanPolicy_save.Enabled = true;
                else
                    this.toolStripButton_newLoanPolicy_save.Enabled = false;
            }
        }
    }
}
