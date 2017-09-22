using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Web;

using UpgradeUtil;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 和 复制数据库 有关的代码
    /// </summary>
    public partial class MainForm : Form
    {
        UpgradeUtil.JidaoControl jidaoControl1;

        // 所有dp2数据库的XML定义
        XmlDocument Dp2DatabaseDom = null;

        // 累积馆藏地点列表
        public Hashtable m_locations = new Hashtable();

        // 累积采购资金来源列表
        public Hashtable m_sources = new Hashtable();

        // 累积采购书商列表
        public Hashtable m_sellers = new Hashtable();

        // 累积采购类别列表
        public Hashtable m_orderclasses = new Hashtable();

        DtlpIO DumpRecord = new DtlpIO();

        bool m_bFirst = false;
        int m_nRecordCount = 0;
        bool m_bSetRange = false;
        Int64 m_nCurPos = 0;
        long m_nRangeStart = 0;
        long m_nRangeEnd = 0;

        public long m_lSeed = 0;




        // 获得一个代表当前超期事项的唯一性字符串
        public string GetOverdueID()
        {
            // 获得一个自从应用启动以来的增量序号
            long lNumber = Interlocked.Increment(ref m_lSeed);

            // 获得代表当前时间的ticks
            long lTicks = DateTime.Now.Ticks;

            return lTicks.ToString() + "-" + lNumber.ToString();
        }

        // 获得全部读者库名
        void GetReaderDbNames(out List<string> reader_dbnames)
        {
            reader_dbnames = new List<string>();

            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strDatabaseName = dtlp_item.Text;
                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                if (StringUtil.IsInList("读者库", strCreatingType) == true)
                {
                    reader_dbnames.Add(strDatabaseName);
                }
            }
        }

        // 获得全部期刊书目库名
        void GetIssueDbNames(out List<string> issue_dbnames)
        {
            issue_dbnames = new List<string>();

            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strDatabaseName = dtlp_item.Text;
                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                if (StringUtil.IsInList("书目库", strCreatingType) == true
                    && StringUtil.IsInList("期刊", strCreatingType) == true)
                {

                    issue_dbnames.Add(strDatabaseName);
                }
            }
        }

        // 获得全部书目库名
        public void GetBiblioDbNames(out List<string> biblio_dbnames)
        {
            biblio_dbnames = new List<string>();

            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strDatabaseName = dtlp_item.Text;
                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                if (StringUtil.IsInList("书目库", strCreatingType) == true)
                {
                    biblio_dbnames.Add(strDatabaseName);
                }
            }
        }

        // 检查看看期刊书目库中的记录有没有期刊信息不正确的问题
        // parameters:
        //      bWarning    是否出现过警告
        // return:
        //      -1  error
        //      0   没有问题
        //      1   有问题
        int VerifyIssueInfo(List<string> biblio_dbnames,
            out bool bWarning,
            out string strError)
        {
            strError = "";
            bWarning = false;

            if (this.jidaoControl1 == null)
                this.jidaoControl1 = new UpgradeUtil.JidaoControl();

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在检查期刊库内记录 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                AppendHtml(
"====================<br/>"
+ "检查期刊库内记录<br/>"
+ "====================<br/><br/>");


                AppendHtml(
                    "准备检查 " + biblio_dbnames.Count.ToString() + " 个期刊库内的全部记录...<br/>");

                List<string> barcodes = new List<string>();

                for (int i = 0; i < biblio_dbnames.Count; i++)
                {
                    string strIssueDbName = biblio_dbnames[i];

                    bool bWarning_1 = false;
                    int nRet = DoCheckOneIssueDatabase(strIssueDbName,
                        out bWarning_1,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                    }

                    if (bWarning_1 == true)
                        bWarning = true;
                }


                if (bWarning == true)
                {
                    AppendHtml(
                         "***注意***<br/>在检查期刊记录过程中，出现了警告信息。"
                         + "建议先去dt1000系统内消除这些警告的问题，然后再重新进行升级<br/>");
                    return 1;   // 发现了问题
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.EnableControls(true);
            }
            return 0; // 没有问题
        }

        // 检查一个期刊书目库
        // parameters:
        //      bWarning    是否出现过警告
        // return:
        //      -1  error
        //      0   数据库内没有数据
        //      1   成功完成提取
        int DoCheckOneIssueDatabase(string strDatabaseName,
            out bool bWarning,
            out string strError)
        {
            strError = "";
            bWarning = false;
            int nRet = 0;


            string strWarning = "";

            string strStartNo = "0000001";
            string strEndNo = "9999999";

            // 准备开始获取数据的循环
            // return:
            //		-1	出错
            //		0	正常
            //		1	需要正常结束循环
            nRet = PrepareDataLoop(this.textBox_dtlpAsAddress.Text + "/" + strDatabaseName,
                ref strStartNo,
                ref strEndNo,
                out strError);
            if (nRet == -1)
            {
                strError = "在准备检查dt1000期刊库 '" + strDatabaseName + "' 中期刊数据的时候发生错误: " + strError;
                return -1;
            }
            if (nRet == 1)
            {
                AppendHtml(
                    "期刊库 " + strDatabaseName + " 内没有记录<br/>");
                return 0;
            }

            AppendHtml(
                "期刊库 " + strDatabaseName + " 内记录起始号为 " + strStartNo + ", 结束号为 " + strEndNo + "<br/>");


            for (; ; ) // begin of data loop
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        DialogResult msgResult = MessageBox.Show(this,
                            "检查期刊库 '" + strDatabaseName + "' 内数据的操作正在进行。\r\n\r\n确实要停止处理?",
                            "UpgradeDt1000ToDp2",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (msgResult == DialogResult.No)
                        {
                            stop.Continue();
                        }
                        else
                            break;
                    }
                }

                // 顺次获取下一条记录
                // return:
                //		-1	出错
                //		0	正常
                //		1	需要正常结束循环
                //		2	不处理本次记录,但继续循环
                nRet = this.NextRecord(out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    goto END1;
                if (nRet == 2)
                    continue;

                stop.SetMessage("正在检查期刊记录 " + DumpRecord.m_strPath + " " + (m_nRecordCount - 1).ToString() + "/" + (m_nRangeEnd - m_nRangeStart + 1).ToString());

                string strRecordID = DumpRecord.m_strCurNumber;

                // 检查一条期刊记录
                // return:
                //      -1  error
                //      0   suceed
                nRet = CheckIssueRecord(strDatabaseName,
                    strRecordID,
                    DumpRecord.m_strRecord,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                {
                    AppendHtml(
                        "检查期刊记录 " + strDatabaseName + "/" + strRecordID + " 时发生错误: " + strError + "<br/>");
                    // goto ERROR1;
                }

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    AppendHtml(
                        "检查期刊记录 " + strDatabaseName + "/" + strRecordID + " 时出现警告: " + strWarning + "<br/>");
                    bWarning = true;
                }

            }
        END1:
            stop.SetMessage("检查期刊库 '" + strDatabaseName + "' 内数据完成");

            AppendHtml(
                "共检查期刊库 " + strDatabaseName + " 内记录 " + this.m_nRecordCount.ToString() + " 条<br/>");

            return 1;
        ERROR1:
            return -1;
        }

        // 检查一条期刊记录
        // return:
        //      -1  error
        //      0   succeed
        int CheckIssueRecord(
            string strDatabseName,
            string strRecordID,
            string strMARC,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // return:
            //      -1  检查操作失败
            //      0   数据没有错
            //      1   数据有错
            nRet = this.jidaoControl1.Check(strMARC,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                strWarning = strError;
                strError = "";
                return 0;
            }

            return 0;
        }

        // 检查看看读者库中的记录有没有使用重复的证条码
        // parameters:
        //      bWarning    是否出现过警告
        // return:
        //      -1  error
        //      0   没有问题
        //      1   有重复证条码问题
        int VerifyDupReaderBarcode(List<string> reader_dbnames,
            out bool bWarning,
            out string strError)
        {
            strError = "";
            bWarning = false;

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在检查读者库内记录 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                AppendHtml(
"====================<br/>"
+ "检查读者库内记录<br/>"
+ "====================<br/><br/>");


                AppendHtml(
                    "准备检查 " + reader_dbnames.Count.ToString() + " 个读者库内的全部记录...<br/>");

                List<string> barcodes = new List<string>();

                for (int i = 0; i < reader_dbnames.Count; i++)
                {
                    string strReaderDbName = reader_dbnames[i];

                    bool bWarning_1 = false;
                    int nRet = DoCheckOneReaderDatabase(strReaderDbName,
                        ref barcodes,
                        out bWarning_1,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                    }

                    if (bWarning_1 == true)
                        bWarning = true;
                }

                AppendHtml(
                    "提取 " + reader_dbnames.Count.ToString() + " 个读者库内的证条码信息全部完成<br/><br/>");

                // 排序
                barcodes.Sort();

                List<string> dup_barcodes = new List<string>();
                // 查重
                for (int i = 0; i < barcodes.Count; i++)
                {
                    string strBarcode = barcodes[i];
                    bool bDup = false;
                    for (int j = i + 1; j < barcodes.Count; j++)
                    {
                        if (barcodes[j] == strBarcode)
                        {
                            bDup = true;
                            barcodes.RemoveAt(j);
                            j--;
                        }
                        else
                            break;
                    }

                    if (bDup == true)
                        dup_barcodes.Add(strBarcode);
                }

                if (bWarning == true)
                {
                    AppendHtml(
                         "***注意***<br/>在检查读者记录过程中，出现了警告信息。"
                         + "建议先去dt1000系统内消除这些警告的问题，然后再重新进行升级<br/>");
                }


                if (dup_barcodes.Count > 0)
                {
                    string strText = Global.MakeListString(dup_barcodes,
                        "<br/>");
                    AppendHtml(
                         "***注意***<br/>发现有 " + dup_barcodes.Count.ToString() + " 个读者证条码有重复情况：<br/>"
                         + strText
                         + "<br/><br/>请务必先去dt1000系统内消除上述证条码重复的问题，然后再重新进行升级<br/>");
                    return 1;    // 有重复证条码问题
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.EnableControls(true);
            }
            return 0; // 没有重复证条码问题
        }

        // TODO: 要把警告情况反馈给调主，以便决定是否停止处理。
        // 提取一个读者库的读者条码信息
        // parameters:
        //      bWarning    是否出现过警告
        // return:
        //      -1  error
        //      0   数据库内没有数据
        //      1   成功完成提取
        int DoCheckOneReaderDatabase(string strDatabaseName,
            ref List<string> barcodes,
            out bool bWarning,
            out string strError)
        {
            strError = "";
            bWarning = false;
            int nRet = 0;


            string strWarning = "";

            string strStartNo = "0000001";
            string strEndNo = "9999999";

            // 准备开始获取数据的循环
            // return:
            //		-1	出错
            //		0	正常
            //		1	需要正常结束循环
            nRet = PrepareDataLoop(this.textBox_dtlpAsAddress.Text + "/" + strDatabaseName,
                ref strStartNo,
                ref strEndNo,
                out strError);
            if (nRet == -1)
            {
                strError = "在准备提取dt1000读者库 '" + strDatabaseName + "' 中读者条码的时候发生错误: " + strError;
                return -1;
            }
            if (nRet == 1)
            {
                AppendHtml(
                    "读者库 " + strDatabaseName + " 内没有记录<br/>");
                return 0;
            }

            AppendHtml(
                "读者库 " + strDatabaseName + " 内记录起始号为 " + strStartNo + ", 结束号为 " + strEndNo + "<br/>");


            for (; ; ) // begin of data loop
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        DialogResult msgResult = MessageBox.Show(this,
                            "提取读者库 '" + strDatabaseName + "' 内的证条码号的操作正在进行。\r\n\r\n确实要停止处理?",
                            "UpgradeDt1000ToDp2",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (msgResult == DialogResult.No)
                        {
                            stop.Continue();
                        }
                        else
                            break;
                    }
                }

                // 顺次获取下一条记录
                // return:
                //		-1	出错
                //		0	正常
                //		1	需要正常结束循环
                //		2	不处理本次记录,但继续循环
                nRet = this.NextRecord(out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    goto END1;
                if (nRet == 2)
                    continue;

                stop.SetMessage("正在提取读者记录 " + DumpRecord.m_strPath + " " + (m_nRecordCount - 1).ToString() + "/" + (m_nRangeEnd - m_nRangeStart + 1).ToString());

                string strRecordID = DumpRecord.m_strCurNumber;

                // 提取一条读者记录内的读者证条码
                // return:
                //      -1  error
                //      0   suceed
                nRet = CheckReaderRecord(strDatabaseName,
                    strRecordID,
                    DumpRecord.m_strRecord,
                    ref barcodes,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    AppendHtml(
                        "提取读者记录 " + strDatabaseName + "/" + strRecordID + " 时发生错误: " + strError + "<br/>");
                }

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    AppendHtml(
                        "提取读者记录 " + strDatabaseName + "/" + strRecordID + " 时出现警告: " + strWarning + "<br/>");
                    bWarning = true;
                }

            }
        END1:
            stop.SetMessage("提取读者库 '" + strDatabaseName + "' 内证条码号完成");

            AppendHtml(
                "共提取读者库 " + strDatabaseName + " 内记录 " + this.m_nRecordCount.ToString() + " 条<br/>");

            return 1;
        ERROR1:
            return -1;
        }

        // 提取一条读者记录内的读者证条码
        // return:
        //      -1  error
        //      0   succeed
        int CheckReaderRecord(
            string strDatabseName,
            string strRecordID,
            string strMARC,
            ref List<string> barcodes,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 读者证条码
            string strBarcode = "";

            // 以字段/子字段名从记录中得到第一个子字段内容。
            // parameters:
            //		strMARC	机内格式MARC记录
            //		strFieldName	字段名。内容为字符
            //		strSubfieldName	子字段名。内容为1字符
            // return:
            //		""	空字符串。表示没有找到指定的字段或子字段。
            //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
            strBarcode = MarcUtil.GetFirstSubfield(strMARC,
                "100",
                "a");

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strWarning += "MARC记录中缺乏100$a读者证条码号; ";
            }
            else
            {
                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    true,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "100$中的读者证条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                barcodes.Add(strBarcode);
            }

            DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);

            // 证号
            // 2008/10/14 new add
            string strCardNumber = "";

            strCardNumber = MarcUtil.GetFirstSubfield(strMARC,
                "100",
                "b");

            if (String.IsNullOrEmpty(strCardNumber) == true)
            {
            }
            else
            {
            }

            DomUtil.SetElementText(dom.DocumentElement, "cardNumber", strCardNumber);

            // 密码
            string strPassword = "";
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
    "080",
    "a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                try
                {
                    strPassword = Cryptography.GetSHA1(strPassword);
                }
                catch
                {
                    strError = "将密码明文转换为SHA1时发生错误";
                    return -1;
                }

                DomUtil.SetElementText(dom.DocumentElement, "password", strPassword);
            }

            // 读者类型
            string strReaderType = "";
            strReaderType = MarcUtil.GetFirstSubfield(strMARC,
    "110",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "readerType", strReaderType);

            /*
            // 发证日期
            DomUtil.SetElementText(dom.DocumentElement, "createDate", strCreateDate);
             * */

            // 失效期
            string strExpireDate = "";
            strExpireDate = MarcUtil.GetFirstSubfield(strMARC,
                "110",
                "d");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                string strToday = DateTimeUtil.DateTimeToString8(DateTime.Now);

                // 2009/2/26 new add
                // 兼容4/6字符形态
                if (strExpireDate.Length == 4)
                {
                    strExpireDate = strExpireDate + "0101";
                }
                else if (strExpireDate.Length == 6)
                {
                    strExpireDate = strExpireDate + "01";
                }

                if (strExpireDate.Length != 8)
                {
                    strWarning += "110$d中的失效期  '" + strExpireDate + "' 应为8字符。升级程序自动以 " + strToday + " 充当失效期; ";
                    strExpireDate = strToday;   // 2008/8/26 new add
                }

                // 2008/10/28 changed

                Debug.Assert(strExpireDate.Length == 8, "");

                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strExpireDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "MARC数据中110$d日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strExpireDate = strToday;   // 2008/8/26 new add

                        // 2008/10/28 new add
                        nRet = Date8toRfc1123(strExpireDate,
                            out strTarget,
                            out strError);
                        Debug.Assert(nRet != -1, "");
                    }

                    strExpireDate = strTarget;
                }

                DomUtil.SetElementText(dom.DocumentElement, "expireDate", strExpireDate);
            }

            // 停借原因
            string strState = "";
            strState = MarcUtil.GetFirstSubfield(strMARC,
    "982",
    "b");
            if (String.IsNullOrEmpty(strState) == false)
            {

                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            // 姓名
            string strName = "";
            strName = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "a");
            if (String.IsNullOrEmpty(strName) == true)
            {
                strWarning += "MARC记录中缺乏200$a读者姓名; ";
            }

            DomUtil.SetElementText(dom.DocumentElement, "name", strName);


            // 姓名拼音
            string strNamePinyin = "";
            strNamePinyin = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "A");
            if (String.IsNullOrEmpty(strNamePinyin) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "namePinyin", strNamePinyin);
            }

            // 性别
            string strGender = "";
            strGender = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "gender", strGender);


            /*
            // 生日
            // 2008/10/14 new add 未证实
            string strBirthday = "";
            strBirthday = MarcUtil.GetFirstSubfield(strMARC,
                "200",
                "c");

            DomUtil.SetElementText(dom.DocumentElement, "birthday", strBirthday);
             * */


            // 身份证号

            // 单位
            string strDepartment = "";
            strDepartment = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "department", strDepartment);

            // 地址
            string strAddress = "";
            strAddress = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "address", strAddress);

            // 邮政编码
            string strZipCode = "";
            strZipCode = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "zipcode", strZipCode);

            // 电话
            string strTel = "";
            strTel = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "tel", strTel);

            // email

            // 所借阅的各册
            string strField986 = "";
            string strNextFieldName = "";
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
            if (nRet == 1)
            {
                XmlNode nodeBorrows = dom.CreateElement("borrows");
                nodeBorrows = dom.DocumentElement.AppendChild(nodeBorrows);

                string strWarningParam = "";
                nRet = CreateBorrowsNode(nodeBorrows,
                    strField986,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据986字段内容创建<borrows>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField988 = "";
            // 违约金记录
            nRet = MarcUtil.GetField(strMARC,
    "988",
    0,
    out strField988,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得988字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeOverdues = dom.CreateElement("overdues");
                nodeOverdues = dom.DocumentElement.AppendChild(nodeOverdues);

                string strWarningParam = "";
                nRet = CreateOverduesNode(nodeOverdues,
                    strField988,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据988字段内容创建<overdues>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField984 = "";
            // 预约信息
            nRet = MarcUtil.GetField(strMARC,
    "984",
    0,
    out strField984,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得984字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeReservations = dom.CreateElement("reservations");
                nodeReservations = dom.DocumentElement.AppendChild(nodeReservations);

                string strWarningParam = "";
                nRet = CreateReservationsNode(nodeReservations,
                    strField984,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据984字段内容创建<reservations>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }

            // 遮盖MARC记录中的808$a内容
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
"080",
"a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                MarcUtil.ReplaceField(ref strMARC,
                    "080",
                    0,
                    "080  " + new String(MarcUtil.SUBFLD, 1) + "a********");
            }

            // 保留原始记录供参考
            string strPlainText = strMARC.Replace(MarcUtil.SUBFLD, '$');
            strPlainText = strPlainText.Replace(new String(MarcUtil.FLDEND, 1), "#\r\n");
            if (strPlainText.Length > 24)
                strPlainText = strPlainText.Insert(24, "\r\n");

            DomUtil.SetElementText(dom.DocumentElement, "originMARC", strPlainText);

            return 0;
        }

        // 复制dtlp数据库内的全部数据到对应的dp2数据库中
        int CopyDatabases(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.jidaoControl1 == null)
                this.jidaoControl1 = new UpgradeUtil.JidaoControl();

            // 获得所有dp2数据库的XML定义
            string strDatabaseDefs = "";
            nRet = GetAllDatabaseInfo(out strDatabaseDefs,
                out strError);
            if (nRet == -1)
                return -1;

            this.Dp2DatabaseDom = new XmlDocument();
            try
            {
                this.Dp2DatabaseDom.LoadXml(strDatabaseDefs);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            this.m_locations.Clear();
            this.m_sources.Clear();
            this.m_sellers.Clear();
            this.m_orderclasses.Clear();

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在升级数据库内记录 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                AppendHtml(
"====================<br/>"
+ "升级数据库记录<br/>"
+ "====================<br/><br/>");


                AppendHtml(
                    "准备升级 " + this.listView_dtlpDatabases.CheckedItems.Count.ToString() + " 个数据库内的全部记录...<br/>");

                for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
                {
                    ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                    string strDatabaseName = dtlp_item.Text;
                    string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                    nRet = DoCopyOneDatabase(strDatabaseName,
                        strCreatingType,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        AppendHtml(
    "升级数据库 " + strDatabaseName + " 内记录时发生错误: " + HttpUtility.HtmlEncode(strError) + "<br/><br/>");

                        if (i < listView_dtlpDatabases.CheckedItems.Count - 1)
                        {
                            // 继续下一个数据库
                            DialogResult msgResult = MessageBox.Show(this,
                                "继续复制其余的数据库?",
                                "UpgradeDt1000ToDp2",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (msgResult == DialogResult.No)
                            {
                                string strDatabaseNames = "";
                                for (int j = i; j < listView_dtlpDatabases.CheckedItems.Count; j++)
                                {
                                    dtlp_item = listView_dtlpDatabases.CheckedItems[i];
                                    strDatabaseName = dtlp_item.Text;
                                    if (String.IsNullOrEmpty(strDatabaseNames) == false)
                                        strDatabaseNames += ",";
                                    strDatabaseNames += strDatabaseName;
                                }

                                AppendHtml(
                                    "下列数据库 " + strDatabaseNames + " 被整个放弃升级。<br/><br/>");
                                return 0;
                            }
                        }

                    }
                }

                AppendHtml(
                    "升级 " + this.listView_dtlpDatabases.CheckedItems.Count.ToString() + " 个数据库内记录的操作全部完成。<br/><br/>");

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.EnableControls(true);
            }
            return 0;
        }

        // 复制一个数据库
        // return:
        //      -1  error
        //      0   数据库内没有数据
        //      1   成功完成复制
        int DoCopyOneDatabase(string strDatabaseName,
            string strCreatingType,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strWarning = "";

            string strStartNo = "0000001";
            string strEndNo = "9999999";

            // 准备开始获取数据的循环
            // return:
            //		-1	出错
            //		0	正常
            //		1	需要正常结束循环
            nRet = PrepareDataLoop(this.textBox_dtlpAsAddress.Text + "/" + strDatabaseName,
                ref strStartNo,
                ref strEndNo,
                out strError);
            if (nRet == -1)
            {
                strError = "在准备复制dt1000数据库 '" + strDatabaseName + "' 到dp2的时候发生错误: " + strError;
                return -1;
            }
            if (nRet == 1)
            {
                AppendHtml(
                    "数据库 " + strDatabaseName + " 内没有记录<br/>");
                return 0;
            }

            AppendHtml(
                "数据库 " + strDatabaseName + " 内记录起始号为 " + strStartNo + ", 结束号为 " + strEndNo + "<br/>");


            for (; ; ) // begin of data loop
            {
                Application.DoEvents();

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        DialogResult msgResult = MessageBox.Show(this,
                            "复制数据库 '" + strDatabaseName + "' 内的数据正在进行。\r\n\r\n确实要停止处理?",
                            "UpgradeDt1000ToDp2",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (msgResult == DialogResult.No)
                        {
                            stop.Continue();
                        }
                        else
                            break;
                    }
                }

                // 顺次获取下一条记录
                // return:
                //		-1	出错
                //		0	正常
                //		1	需要正常结束循环
                //		2	不处理本次记录,但继续循环
                nRet = this.NextRecord(out strError);
                if (nRet == -1)
                {
                    AppendHtml(
                        "获得dt1000数据库记录 " + strDatabaseName + "/" + DumpRecord.m_strCurNumber + " 时发生错误: " + strError + "<br/>");

                    DialogResult msgResult = MessageBox.Show(this,
    "获得dt1000数据库记录 " + strDatabaseName + "/" + DumpRecord.m_strCurNumber + " 时发生错误: " + strError + "。\r\n\r\n重试还是停止处理?\r\n(Yes 重试本条; No 跳过本条处理后一条; Cancel 停止处理)",
    "UpgradeDt1000ToDp2",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (msgResult == DialogResult.Cancel)
                        goto ERROR1;

                    // 继续处理下一条
                    if (msgResult == DialogResult.No)
                    {
                        DumpRecord.IncreaseNextNumber();
                        AppendHtml(
                            "操作者选择继续处理下一条<br/>");
                    }

                    this.DtlpChannel = null;
                    continue;
                }
                if (nRet == 1)
                    goto END1;
                if (nRet == 2)
                    continue;

                stop.SetMessage("正在升级数据记录 " + DumpRecord.m_strPath + " " + (m_nRecordCount - 1).ToString() + "/" + (m_nRangeEnd - m_nRangeStart + 1).ToString());

                string strRecordID = DumpRecord.m_strCurNumber;

                // DumpRecord.m_strRecord
                if (StringUtil.IsInList("书目库", strCreatingType) == true)
                {
                    nRet = SaveBiblioRecord(strDatabaseName,
                        strRecordID,
                        strCreatingType,
                        DumpRecord.m_strRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError += "\r\n***警告：数据库 " + strDatabaseName + " 内记录ID " + strRecordID + " 以后的其余记录被放弃复制。";
                        goto ERROR1;
                    }
                }
                else if (StringUtil.IsInList("读者库", strCreatingType) == true)
                {
                    // 保存一条读者记录
                    // return:
                    //      -1  error
                    //      0   suceed
                    //      1   读者证条码发生了重复。读者记录没有写入
                    nRet = SaveReaderRecord(strDatabaseName,
                        strRecordID,
                        DumpRecord.m_strRecord,
                        out strWarning,
                        out strError);
                    if (nRet == -1)
                    {
                        strError += "\r\n***警告：数据库 " + strDatabaseName + " 内记录ID " + strRecordID + " 以后的其余记录被放弃复制。";
                        goto ERROR1;
                    }
                    if (nRet == 1)
                    {
                        AppendHtml(
                            "写入读者记录 " + strDatabaseName + "/" + strRecordID + " 时发生错误: " + strError + "<br/>");
                    }

                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        AppendHtml(
                            "写入读者记录 " + strDatabaseName + "/" + strRecordID + " 时出现警告: " + strWarning + "<br/>");
                    }
                }
                else
                {
                    // TODO: 报错?
                }
            }
        END1:
            stop.SetMessage("升级数据库 '" + strDatabaseName + "' 内记录完成");

            AppendHtml(
                "共完成升级数据库 " + strDatabaseName + " 内记录 " + this.m_nRecordCount.ToString() + " 条<br/>");

            return 1;
        ERROR1:
            return -1;
        }

        // 保存一条书目记录
        int SaveBiblioRecord(
            string strDatabaseName,
            string strRecordID,
            string strCreatingType,
            string strMARC,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strBiblioRecPath = strDatabaseName + "/" + strRecordID;



            string strMarcSyntax = "unimarc";

            if (StringUtil.IsInList("unimarc", strCreatingType, true) == true)
                strMarcSyntax = "unimarc";
            else if (StringUtil.IsInList("usmarc", strCreatingType, true) == true)
                strMarcSyntax = "usmarc";
            Debug.Assert(strMarcSyntax == "unimarc" || strMarcSyntax == "usmarc", "");

            if (strMarcSyntax == "unimarc")
            {
                // 检查是否为混入书目库的读者数据？
                // 读者证条码
                string strReaderBarcode = "";

                // 以字段/子字段名从记录中得到第一个子字段内容。
                // parameters:
                //		strMARC	机内格式MARC记录
                //		strFieldName	字段名。内容为字符
                //		strSubfieldName	子字段名。内容为1字符
                // return:
                //		""	空字符串。表示没有找到指定的字段或子字段。
                //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
                strReaderBarcode = MarcUtil.GetFirstSubfield(strMARC,
                    "100",
                    "a");
                if (String.IsNullOrEmpty(strReaderBarcode) == false
                    && strReaderBarcode.Length < 30)
                {
                    AppendHtml(
                        "警告: 书目记录 " + strBiblioRecPath + " 疑似dt1000读者数据格式<br/>");
                }
            }

            // 将MARC格式转换为MARCXML格式
            string strBiblioXml = "";
            nRet = MarcUtil.Marc2Xml(
                strMARC,
                strMarcSyntax,
                out strBiblioXml,
                out strError);
            if (nRet == -1)
                return -1;

            string strOutputBiblioRecPath = "";
            byte[] baOutputTimestamp = null;
            long lRet = this.Channel.SetBiblioInfo(
                this.stop,
                "new",
                strBiblioRecPath,
                "xml",
                strBiblioXml,
                null,
                "", //
                out strOutputBiblioRecPath,
                out baOutputTimestamp,
                out strError);
            if (lRet == -1)
                return -1;

            // 2008/12/28 new add
            string strIssueDbName = "";

            // 根据书目库名获得对应的期库名
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetIssueDbName(strDatabaseName,
                out strIssueDbName,
                out strError);
            if (nRet == -1)
                return -1;

            bool bSeries = false;
            if (nRet == 1)
            {
                Debug.Assert(String.IsNullOrEmpty(strIssueDbName) == false, "");
                bSeries = true;
            }
            else
            {
                Debug.Assert(String.IsNullOrEmpty(strIssueDbName) == true, "");
            }

            if (bSeries == true)
            {
                int nThisIssueCount = 0;

                string strWarning = "";

                nRet = DoIssueRecordsUpload(
                    strOutputBiblioRecPath,
                    strIssueDbName,
                    strRecordID,
                    strMARC,
                    strMarcSyntax,
                    out nThisIssueCount,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                {
                    // return -1;
                    AppendHtml(
                        "错误: 书目记录 " + strOutputBiblioRecPath + " 的期信息处理过程发生错误(该记录未被正常处理): " + HttpUtility.HtmlEncode(strError) + "<br/>");
                }

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    string strText = "";
                    string[] lines = strWarning.Split(new char[] { ';', '\r' });
                    for (int i = 0; i < lines.Length; i++)
                    {
                        strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                    }
                    AppendHtml(
                        "记录 " + strOutputBiblioRecPath + " 的期信息处理过程发出警告: <br/>" + strText);
                }
            }


            string strEntityDbName = "";

            // 根据书目库名获得对应的实体库名
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetEntityDbName(strDatabaseName,
                out strEntityDbName,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                Debug.Assert(String.IsNullOrEmpty(strEntityDbName) == false, "");

                int nThisEntityCount = 0;

                string strWarning = "";

                // 将一条MARC记录中包含的实体信息变成XML格式并上传
                // parameters:
                //      strEntityDbName 实体数据库名
                //      strParentRecordID   父记录ID
                //      strMARC 父记录MARC
                nRet = DoEntityRecordsUpload(
                    bSeries,
                    strOutputBiblioRecPath,
                    strEntityDbName,
                    strRecordID,
                    strMARC,
                    strMarcSyntax,
                    out nThisEntityCount,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    string strText = "";
                    string[] lines = strWarning.Split(new char[] { ';', '\r' });
                    for (int i = 0; i < lines.Length; i++)
                    {
                        strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                    }
                    AppendHtml(
                        "记录 " + strOutputBiblioRecPath + " 的册信息处理过程发出警告: <br/>" + strText);
                }
            }


            string strOrderDbName = "";

            // 根据书目库名获得对应的订购库名
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetOrderDbName(strDatabaseName,
                out strOrderDbName,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                Debug.Assert(String.IsNullOrEmpty(strOrderDbName) == false, "");

                int nThisOrderCount = 0;

                string strWarning = "";

                if (bSeries == true)
                {
                    nRet = DoSeriesOrderRecordsUpload(
                        strOutputBiblioRecPath,
                        strOrderDbName,
                        strRecordID,
                        strMARC,
                        strMarcSyntax,
                        out nThisOrderCount,
                        out strWarning,
                        out strError);
                }
                else
                {
                    // 将一条MARC记录中包含的订购信息变成XML格式并上传
                    // parameters:
                    //      strEntityDbName 实体数据库名
                    //      strParentRecordID   父记录ID
                    //      strMARC 父记录MARC
                    nRet = DoBookOrderRecordsUpload(
                        strOutputBiblioRecPath,
                        strOrderDbName,
                        strRecordID,
                        strMARC,
                        strMarcSyntax,
                        out nThisOrderCount,
                        out strWarning,
                        out strError);
                }
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(strWarning) == false)
                {
                    string strText = "";
                    string[] lines = strWarning.Split(new char[] { ';', '\r' });
                    for (int i = 0; i < lines.Length; i++)
                    {
                        strText += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
                    }
                    AppendHtml(
                        "记录 " + strOutputBiblioRecPath + " 的订购信息处理过程发出警告: <br/>" + strText);
                }
            }

            return 0;
        }

        // 根据书目库名获得对应的实体库名
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetEntityDbName(string strBiblioDbName,
            out string strEntityDbName,
            out string strError)
        {
            strEntityDbName = "";
            strError = "";

            XmlNode nodeDatabase = this.Dp2DatabaseDom.DocumentElement.SelectSingleNode("database[@name='" + strBiblioDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "书目库 '" + strBiblioDbName + "' 的<database>元素在XML定义中没有找到";
                return -1;
            }

            strEntityDbName = DomUtil.GetAttr(nodeDatabase, "entityDbName");

            if (String.IsNullOrEmpty(strEntityDbName) == true)
                return 0;   // not found entity db
            return 1;   // found
        }

        // 根据书目库名获得对应的订购库名
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetOrderDbName(string strBiblioDbName,
            out string strOrderDbName,
            out string strError)
        {
            strOrderDbName = "";
            strError = "";

            XmlNode nodeDatabase = this.Dp2DatabaseDom.DocumentElement.SelectSingleNode("database[@name='" + strBiblioDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "书目库 '" + strBiblioDbName + "' 的<database>元素在XML定义中没有找到";
                return -1;
            }

            strOrderDbName = DomUtil.GetAttr(nodeDatabase, "orderDbName");

            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return 0;   // not found order db
            return 1;   // found
        }

        // 根据书目库名获得对应的期库名
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetIssueDbName(string strBiblioDbName,
            out string strIssueDbName,
            out string strError)
        {
            strIssueDbName = "";
            strError = "";

            XmlNode nodeDatabase = this.Dp2DatabaseDom.DocumentElement.SelectSingleNode("database[@name='" + strBiblioDbName + "']");

            if (nodeDatabase == null)
            {
                strError = "书目库 '" + strBiblioDbName + "' 的<database>元素在XML定义中没有找到";
                return -1;
            }

            strIssueDbName = DomUtil.GetAttr(nodeDatabase, "issueDbName");

            if (String.IsNullOrEmpty(strIssueDbName) == true)
                return 0;   // not found issue db
            return 1;   // found
        }

        // 将一条MARC记录中包含的实体信息变成XML格式并上传
        // parameters:
        //      bSeries 是否为期刊库? 如果为期刊库，则需要把907和986合并；否则要把906和986合并
        //      strEntityDbName 实体数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int DoEntityRecordsUpload(
            bool bSeries,
            string strBiblioRecPath,
            string strEntityDbName,
            string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            out int nThisEntityCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            nThisEntityCount = 0;

            int nRet = 0;

            string strField906or907 = "";
            string strField986 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";

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
                strBiblioPrice = Global.CanonicalizePrice(strBiblioPrice, false);
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

            List<EntityInfo> entityArray = new List<EntityInfo>();

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
                    strParentRecordID,
                    strGroup,
                    strMARC,
                    strBiblioPrice,
                    group.strMergeComment,
                    // nReaderBarcodeLength,
                    out strXml,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建记录id " + strParentRecordID + " 之实体(序号) " + Convert.ToString(g + 1) + "时发生错误: " + strError;
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
                    strError = "创建记录id " + strParentRecordID + " 之实体(序号) " + Convert.ToString(g + 1) + "时发生错误: "
                        + "entity xml装入XMLDOM时发生错误: " + ex.Message;
                    return -1;
                }
                string strLocation = DomUtil.GetElementText(entity_dom.DocumentElement, "location");

                // 允许馆藏地点值为空
                if (String.IsNullOrEmpty(strLocation) == true)
                    strLocation = "";
                /*
                {
                    object o = m_locations[strLocation];
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
                    m_locations[strLocation] = (object)count;
                }
                 * */
                FillValueTable(this.m_locations,
                    strLocation);

                // 保存到服务器
                EntityInfo info = new EntityInfo();
                info.RefID = GenRefID();
                info.Action = "new";
                if (this.checkBox_copyDatabase_checkEntityDup.Checked == true)
                    info.Style = "force,noeventlog";
                else
                    info.Style = "force,nocheckdup,noeventlog";   // 必须用forcenew 而不能用 new。因为后者会在保存记录的时候，自动去掉borrower等字段。

                info.NewRecPath = "";
                info.NewRecord = strXml;
                info.NewTimestamp = null;

                entityArray.Add(info);

                /*
                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";
                string strTargetPath = strEntityDbName + "/?";


                // 保存Xml记录
                long lRet = channel.DoSaveTextRes(strTargetPath,
                    strXml,
                    false,	// bIncludePreamble
                    "",//strStyle,
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);

                if (lRet == -1)
                {
                    return -1;
                }*/

                nThisEntityCount++;

            }

            // 复制到目标
            EntityInfo[] entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            EntityInfo[] errorinfos = null;
            nRet = SaveEntityRecords(strBiblioRecPath,
                entities,
                out errorinfos,
                out strError);
            if (nRet == -1)
                return -1;

            // 检查个别错误
            string strTempWarning = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo info = errorinfos[i];

                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    if (String.IsNullOrEmpty(strTempWarning) == false)
                        strTempWarning += "; ";

                    // string strSummary = GetLocationSummary(info.NewRecord);

                    strTempWarning += /*strSummary + " 的实体记录: " + */info.ErrorInfo;
                }
            }

            if (String.IsNullOrEmpty(strTempWarning) == false)
            {
                if (String.IsNullOrEmpty(strWarning) == false)
                    strWarning += "; ";

                strWarning = strWarning + "书目记录 " + strBiblioRecPath + " 下属的实体记录创建时发生下列错误: \r" + strTempWarning;
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

        static string GetLocationSummary(string strXml)
        {
            string strResult = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strResult = "XML装载到DOM时发生错误: " + ex.Message;
                return strResult;
            }

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            string strRegisterNo = DomUtil.GetElementText(dom.DocumentElement,
                "registerNo");

            if (String.IsNullOrEmpty(strBarcode) == false)
            {
                strResult = "册条码为 '" + strBarcode + "'";
                return strResult;
            }

            strResult = "登录号为 '" + strRegisterNo + "'";
            return strResult;
        }

        public static string GenRefID()
        {
            return Guid.NewGuid().ToString();
        }

        // 保存实体记录
        // 不负责刷新界面和报错
        int SaveEntityRecords(string strBiblioRecPath,
            EntityInfo[] entities,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存册信息 ...");
            stop.BeginLoop();

            this.Update();
             * */

            try
            {
                long lRet = this.Channel.SetEntities(
                    stop,
                    strBiblioRecPath,
                    entities,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                 * */
            }

            return 1;
        ERROR1:
            return -1;
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

        // 构造实体XML记录
        // parameters:
        //      strParentID 父记录ID
        //      strGroup    待转换的图书种记录的986字段中某子字段组片断
        //      strXml      输出的实体XML记录
        //      strBiblioPrice  种价格。当缺乏册例外价格的时候，自动加入种价格
        // return:
        //      -1  出错
        //      0   成功
        int BuildEntityXmlRecord(
            bool bSeries,
            string strParentID,
            string strGroup,
            string strMARC,
            string strBiblioPrice,
            string strMergeComment,
            // int nReaderBarcodeLength,
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
            DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

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
                strPrice = Global.CanonicalizePrice(strPrice, false);
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
                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    true,
                    strBorrower,
                    out strError);
                if (nRet == -1)
                    return -1;


                // 检查条码长度
                if (nRet != 0)
                {
                    strWarning += "$r中读者证条码号 '" + strBorrower + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBorrower = strBorrower.ToUpper();  // 2008/10/24 new add

                DomUtil.SetElementText(dom.DocumentElement, "borrower", strBorrower);
            }

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

                    nRet = Date8toRfc1123(strBorrowDate,
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
                        nRet = JidaoControl.SetOperation(
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

            strXml = dom.OuterXml;
            return 0;
        }

        public static int Date8toRfc1123(string strOrigin,
out string strTarget,
out string strError)
        {
            strError = "";
            strTarget = "";

            // strOrigin = strOrigin.Replace("-", "");

            // 格式为 20060625， 需要转换为rfc
            if (strOrigin.Length != 8)
            {
                strError = "源日期字符串 '" + strOrigin + "' 格式不正确，应为8字符";
                return -1;
            }


            IFormatProvider culture = new CultureInfo("zh-CN", true);

            DateTime time;
            try
            {
                time = DateTime.ParseExact(strOrigin, "yyyyMMdd", culture);
            }
            catch
            {
                strError = "日期字符串 '" + strOrigin + "' 字符串转换为DateTime对象时出错";
                return -1;
            }

            time = time.ToUniversalTime();
            strTarget = DateTimeUtil.Rfc1123DateTimeString(time);


            return 0;
        }


        // 根据一个MARC字段，创建Group数组
        // 必须符合下列定义：
        // 将$a放入到Barcode
        // 将$h放入到RegisterNo
        public int BuildGroups(string strField,
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

        // 合并907和986字段内容
        int MergeField907and986(string strField907,
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
        int MergeField906and986(string strField906,
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

        // 针对一个（册信息）子字段组的描述
        public class ItemGroup
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

        // 保存一条读者记录
        // return:
        //      -1  error
        //      0   suceed
        //      1   读者证条码发生了重复。读者记录没有写入
        int SaveReaderRecord(
            string strDatabseName,
            string strRecordID,
            string strMARC,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            // 要把原来在cross阶段作的违约金从分变换到元的事情，放在这里做了
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 读者证条码
            string strBarcode = "";

            // 以字段/子字段名从记录中得到第一个子字段内容。
            // parameters:
            //		strMARC	机内格式MARC记录
            //		strFieldName	字段名。内容为字符
            //		strSubfieldName	子字段名。内容为1字符
            // return:
            //		""	空字符串。表示没有找到指定的字段或子字段。
            //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
            strBarcode = MarcUtil.GetFirstSubfield(strMARC,
                "100",
                "a");

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strWarning += "MARC记录中缺乏100$a读者证条码号; ";
            }
            else
            {
                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    true,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "100$中的读者证条码号 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add
            }

            DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);


            // 证号
            // 2008/10/14 new add
            string strCardNumber = "";

            strCardNumber = MarcUtil.GetFirstSubfield(strMARC,
                "100",
                "b");

            if (String.IsNullOrEmpty(strCardNumber) == true)
            {
            }
            else
            {
            }

            DomUtil.SetElementText(dom.DocumentElement, "cardNumber", strCardNumber);



            // 密码
            string strPassword = "";
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
    "080",
    "a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                try
                {
                    strPassword = Cryptography.GetSHA1(strPassword);
                }
                catch
                {
                    strError = "将密码明文转换为SHA1时发生错误";
                    return -1;
                }

                DomUtil.SetElementText(dom.DocumentElement, "password", strPassword);
            }

            // 读者类型
            string strReaderType = "";
            strReaderType = MarcUtil.GetFirstSubfield(strMARC,
    "110",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "readerType", strReaderType);

            /*
            // 发证日期
            DomUtil.SetElementText(dom.DocumentElement, "createDate", strCreateDate);
             * */

            // 失效期
            string strExpireDate = "";
            strExpireDate = MarcUtil.GetFirstSubfield(strMARC,
                "110",
                "d");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                string strToday = DateTimeUtil.DateTimeToString8(DateTime.Now);

                // 2009/2/26 new add
                // 兼容4/6字符形态
                if (strExpireDate.Length == 4)
                {
                    strExpireDate = strExpireDate + "0101";
                }
                else if (strExpireDate.Length == 6)
                {
                    strExpireDate = strExpireDate + "01";
                }

                if (strExpireDate.Length != 8)
                {
                    strWarning += "110$d中的失效期  '" + strExpireDate + "' 应为8字符。升级程序自动以 " + strToday + " 充当失效期; ";
                    strExpireDate = strToday;   // 2008/8/26 new add
                }

                // 2008/10/28 changed

                Debug.Assert(strExpireDate.Length == 8, "");

                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strExpireDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "MARC数据中110$d日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strExpireDate = strToday;   // 2008/8/26 new add

                        // 2008/10/28 new add
                        nRet = Date8toRfc1123(strExpireDate,
                            out strTarget,
                            out strError);
                        Debug.Assert(nRet != -1, "");
                    }

                    strExpireDate = strTarget;
                }


                DomUtil.SetElementText(dom.DocumentElement, "expireDate", strExpireDate);
            }

            // 押金
            // 2008/11/13 new add
            string strForegift = "";
            strForegift = MarcUtil.GetFirstSubfield(strMARC,
                "110",
                "e");
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                long foregift = 0;
                try
                {
                    foregift = Convert.ToInt64(strForegift);
                }
                catch (Exception /*ex*/)
                {
                    strWarning += "MARC数据中110$e押金字符串 '" + strForegift + "' 格式错误";
                    strForegift = "";
                    goto SKIP_COMPUTE_FOREGIFT;
                }

                double new_foregift = (double)foregift / (double)100;
                strForegift = "CNY" + new_foregift.ToString();
            }

        SKIP_COMPUTE_FOREGIFT:
            if (String.IsNullOrEmpty(strForegift) == false)
            {

                DomUtil.SetElementText(dom.DocumentElement, "foregift", strForegift);
            }


            // 停借原因
            string strState = "";
            strState = MarcUtil.GetFirstSubfield(strMARC,
    "982",
    "b");
            if (String.IsNullOrEmpty(strState) == false)
            {

                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            // 姓名
            string strName = "";
            strName = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "a");
            if (String.IsNullOrEmpty(strName) == true)
            {
                strWarning += "MARC记录中缺乏200$a读者姓名; ";
            }

            DomUtil.SetElementText(dom.DocumentElement, "name", strName);


            // 姓名拼音
            string strNamePinyin = "";
            strNamePinyin = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "A");
            if (String.IsNullOrEmpty(strNamePinyin) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "namePinyin", strNamePinyin);
            }

            // 性别
            string strGender = "";
            strGender = MarcUtil.GetFirstSubfield(strMARC,
    "200",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "gender", strGender);

            /*
            // 生日
            // 2008/10/14 new add 未证实
            string strBirthday = "";
            strBirthday = MarcUtil.GetFirstSubfield(strMARC,
                "200",
                "c");

            DomUtil.SetElementText(dom.DocumentElement, "birthday", strBirthday);
             * */

            // 身份证号

            // 单位
            string strDepartment = "";
            strDepartment = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "department", strDepartment);

            // 地址
            string strAddress = "";
            strAddress = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "address", strAddress);

            // 邮政编码
            string strZipCode = "";
            strZipCode = MarcUtil.GetFirstSubfield(strMARC,
    "400",
    "a");

            DomUtil.SetElementText(dom.DocumentElement, "zipcode", strZipCode);

            // 电话
            string strTel = "";
            strTel = MarcUtil.GetFirstSubfield(strMARC,
    "300",
    "b");

            DomUtil.SetElementText(dom.DocumentElement, "tel", strTel);

            // email

            // 所借阅的各册
            string strField986 = "";
            string strNextFieldName = "";
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
            if (nRet == 1)
            {
                XmlNode nodeBorrows = dom.CreateElement("borrows");
                nodeBorrows = dom.DocumentElement.AppendChild(nodeBorrows);

                string strWarningParam = "";
                nRet = CreateBorrowsNode(nodeBorrows,
                    strField986,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据986字段内容创建<borrows>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField988 = "";
            // 违约金记录
            nRet = MarcUtil.GetField(strMARC,
    "988",
    0,
    out strField988,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得988字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeOverdues = dom.CreateElement("overdues");
                nodeOverdues = dom.DocumentElement.AppendChild(nodeOverdues);

                string strWarningParam = "";
                nRet = CreateOverduesNode(nodeOverdues,
                    strField988,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据988字段内容创建<overdues>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }


            string strField984 = "";
            // 预约信息
            nRet = MarcUtil.GetField(strMARC,
    "984",
    0,
    out strField984,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得984字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeReservations = dom.CreateElement("reservations");
                nodeReservations = dom.DocumentElement.AppendChild(nodeReservations);

                string strWarningParam = "";
                nRet = CreateReservationsNode(nodeReservations,
                    strField984,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据984字段内容创建<reservations>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }

            string strField989 = "";
            // 借阅历史
            nRet = MarcUtil.GetField(strMARC,
    "989",
    0,
    out strField989,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得989字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeBorrowHistory = dom.CreateElement("borrowHistory");
                nodeBorrowHistory = dom.DocumentElement.AppendChild(nodeBorrowHistory);

                string strWarningParam = "";
                nRet = CreateBorrowHistoryNode(nodeBorrowHistory,
                    strField989,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据989字段内容创建<borrowHistory>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

            }

            // 遮盖MARC记录中的808$a内容
            strPassword = MarcUtil.GetFirstSubfield(strMARC,
"080",
"a");
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                MarcUtil.ReplaceField(ref strMARC,
                    "080",
                    0,
                    "080  " + new String(MarcUtil.SUBFLD, 1) + "a********");
            }

            // 保留原始记录供参考
            string strPlainText = strMARC.Replace(MarcUtil.SUBFLD, '$');
            strPlainText = strPlainText.Replace(new String(MarcUtil.FLDEND, 1), "#\r\n");
            if (strPlainText.Length > 24)
                strPlainText = strPlainText.Insert(24, "\r\n");

            DomUtil.SetElementText(dom.DocumentElement, "originMARC", strPlainText);

            // 是在dp2library层专门允许写入带有流通信息的readerxml记录，还是直接用rmsws API进行写入？
            // 实体信息乃至书目信息的升级也有类似问题

            string strRecPath = strDatabseName + "/" + strRecordID;
            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;

            ErrorCodeValue kernel_errorcode;

        REDO_SAVE:

            long lRet = this.Channel.SetReaderInfo(this.stop,
                "forcechange",  // forcenew可能会运行得快一些，但是不能保持和原dt1000 id的一致性
                strRecPath,
                dom.OuterXml,
                "", // strOldXml,
                null,   // baOldTimestamp,
                out strExistingXml,
                out strSavedXml,
                out strSavedRecPath,
                out baNewTimestamp,
                out kernel_errorcode,
                out strError);
            if (lRet == -1)
            {
                // 读者证条码发生了重复
                if (this.Channel.ErrorCode == ErrorCode.ReaderBarcodeDup)
                {
                    // TODO: 将读者条码改变后重试写入?
                    strError += "记录 " + strRecPath + " 没有被写入。";
                    return 1;
                }

                DialogResult msgResult = MessageBox.Show(this,
                    "保存记录 '" + strRecPath + "' 时发生错误：" + strError + "\r\n\r\n重试保存?",
                    "UpgradeDt1000ToDp2",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (msgResult == DialogResult.Retry)
                    goto REDO_SAVE;

                return -1;
            }


            return 0;
        }

        // 创建<borrows>节点的下级内容
        int CreateBorrowsNode(XmlNode nodeBorrows,
            string strField986,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

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
                nRet = MarcUtil.GetGroup(strField986,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

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
                    strBarcode = strSubfield.Substring(1);

                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

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

                if (nRet != 0)
                {
                    strWarning += "986字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                XmlNode nodeBorrow = nodeBorrows.OwnerDocument.CreateElement("borrow");
                nodeBorrow = nodeBorrows.AppendChild(nodeBorrow);

                DomUtil.SetAttr(nodeBorrow, "barcode", strBarcode);

                // borrowDate属性
                // 第一次借书日期
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "986$t子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "borrowDate", strBorrowDate);
                }

                // no属性
                // 从什么数字开始计数？
                string strNo = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "y",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strNo = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strNo) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "no", strNo);
                }




                // borrowPeriod属性

                // 根据应还日期计算出来?

                // 应还日期
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "v",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "986$v子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }
                else
                {
                    if (strBorrowDate != "")
                    {
                        strWarning += "986字段中子字段组 " + Convert.ToString(g + 1) + " 有 $t 子字段内容而没有 $v 子字段内容, 不正常; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$v日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false
                    && String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    // 计算差额天数
                    DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
                    DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                    TimeSpan delta = timeend - timestart;

                    string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                    DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                }

                // 续借的日期
                if (strNo != "")
                {
                    string strRenewDate = "";
                    nRet = MarcUtil.GetSubfield(strGroup,
                        ItemType.Group,
                        "x",
                        0,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (strSubfield.Length >= 1)
                    {
                        strRenewDate = strSubfield.Substring(1);

                        if (strRenewDate.Length != 8)
                        {
                            strWarning += "986$x子字段内容 '" + strRenewDate + "' 的长度不是8字符; ";
                        }
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        string strTarget = "";
                        nRet = Date8toRfc1123(strRenewDate,
                            out strTarget,
                            out strError);
                        if (nRet == -1)
                        {
                            strWarning += "986字段中$x日期字符串转换格式为rfc1123时发生错误: " + strError;
                            strRenewDate = "";
                        }
                        else
                        {
                            strRenewDate = strTarget;
                        }
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        DomUtil.SetAttr(nodeBorrow, "borrowDate", strRenewDate);
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false
    && String.IsNullOrEmpty(strReturnDate) == false)    // && String.IsNullOrEmpty(strBorrowDate) == false
                    {
                        // 重新计算差额天数
                        DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strRenewDate);
                        DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                        TimeSpan delta = timeend - timestart;

                        string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                        DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                    }
                }
            }

            return 0;
        }

        // 创建<overdues>节点的下级内容
        int CreateOverduesNode(XmlNode nodeOverdues,
            string strField988,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

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
                nRet = MarcUtil.GetGroup(strField988,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

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
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

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


                if (nRet != 0)
                {
                    strWarning += "988字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                string strCompleteDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "d",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strCompleteDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strCompleteDate) == false)
                    continue; // 如果已经交了罚金，这个子字段组就忽略了

                XmlNode nodeOverdue = nodeOverdues.OwnerDocument.CreateElement("overdue");
                nodeOverdue = nodeOverdues.AppendChild(nodeOverdue);

                DomUtil.SetAttr(nodeOverdue, "barcode", strBarcode);

                // borrowDate属性
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "e",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "988$e子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$e日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "borrowDate", strBorrowDate);
                }

                // returnDate属性
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);

                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "988$t子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "returnDate", strReturnDate);  // 2006/12/29 changed
                }

                // borrowPeriod未知
                //   DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strBorrowPeriod);

                // price和type属性是为兼容dt1000数据而设立的属性
                // 而over超期天数属性就空缺了

                // price属性
                string strPrice = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strPrice = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    // 是否需要转换为带货币单位的, 带小数部分的字符串?
                    if (StringUtil.IsPureNumber(strPrice) == true)
                    {
                        // 只有纯数字才作

                        long lOldPrice = 0;

                        try
                        {
                            lOldPrice = Convert.ToInt64(strPrice);
                        }
                        catch
                        {
                            strWarning += "价格字符串 '' 格式不正确，应当为纯数字。";
                            goto SKIP_11;
                        }

                        // 转换为元
                        double dPrice = ((double)lOldPrice) / 100;

                        strPrice = "CNY" + dPrice.ToString();
                    }

                SKIP_11:

                    DomUtil.SetAttr(nodeOverdue, "price", strPrice);
                }

                // type属性
                string strType = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strType = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strType) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "type", strType);
                }

                // 2007/9/27 new add
                DomUtil.SetAttr(nodeOverdue, "id", "upgrade-" + this.GetOverdueID());   // 2008/2/8 new add "upgrade-"
            }

            return 0;
        }


        // 创建<reservations>节点的下级内容
        // 待做内容：
        // 1)如果实体库已经存在，这里需要增加相关操作实体库的代码。
        // 也可以专门用一个读者记录和实体记录对照修改的阶段，来处理相互的关系
        // 2)暂时没有处理已到的预约册的信息升级功能，而是丢弃了这些信息
        int CreateReservationsNode(XmlNode nodeReservations,
            string strField984,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

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
                nRet = MarcUtil.GetGroup(strField984,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

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
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

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

                if (nRet != 0)
                {
                    strWarning += "984字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                string strArriveDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strArriveDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strArriveDate) == false)
                    continue; // 如果已经到书，这个子字段组就忽略了

                XmlNode nodeRequest = nodeReservations.OwnerDocument.CreateElement("request");
                nodeRequest = nodeReservations.AppendChild(nodeRequest);

                DomUtil.SetAttr(nodeRequest, "items", strBarcode);

                // requestDate属性
                string strRequestDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strRequestDate = strSubfield.Substring(1);

                    if (strRequestDate.Length != 8)
                    {
                        strWarning += "984$b子字段内容 '" + strRequestDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strRequestDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "984字段中$b日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strRequestDate = "";
                    }
                    else
                    {
                        strRequestDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    DomUtil.SetAttr(nodeRequest, "requestDate", strRequestDate);
                }

            }

            return 0;
        }

        // 创建<borrowHistory>节点的下级内容
        int CreateBorrowHistoryNode(XmlNode nodeBorrowHistory,
            string strField989,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            XmlNode nodePrev = null;    // 插入参考节点
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
                nRet = MarcUtil.GetGroup(strField989,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

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
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;


                strBarcode = strBarcode.ToUpper();

                XmlNode nodeBorrow = nodeBorrowHistory.OwnerDocument.CreateElement("borrow");

                // If refChild is a null reference (Nothing in Visual Basic), insert newChild at the end of the list of child nodes
                nodeBorrow = nodeBorrowHistory.InsertBefore(nodeBorrow, nodePrev);
                nodePrev = nodeBorrow;

                // 删除超过100个的子节点
                if (nodeBorrowHistory.ChildNodes.Count > 100)
                {
                    XmlNode temp = nodeBorrowHistory.ChildNodes[nodeBorrowHistory.ChildNodes.Count - 1];
                    nodeBorrowHistory.RemoveChild(temp);
                }

                DomUtil.SetAttr(nodeBorrow, "barcode", strBarcode);

                // borrowDate属性
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "t",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strBorrowDate = "";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "borrowDate", strBorrowDate);
                }
            }

            /*
            // delete more than 100
            if (nodeBorrowHistory.ChildNodes.Count > 100)
            {
                XmlNodeList nodes = nodeBorrowHistory.SelectNodes("borrow");
                for (int i = 100; i < nodes.Count; i++)
                {
                    nodeBorrowHistory.RemoveChild(nodes[i]);
                }
            }*/

            return 0;
        }

        // 准备开始获取数据的循环
        // return:
        //		-1	出错
        //		0	正常
        //		1	需要正常结束循环
        int PrepareDataLoop(string strDbPath,
            ref string strStartNo,
            ref string strEndNo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 准备唯一的通道
            if (this.DtlpChannel == null)
            {
                this.DtlpChannel = this.DtlpChannelArray.CreateChannel(0);
            }

            this.DumpRecord.Initial(this.DtlpChannelArray,
                    strDbPath,
                    strStartNo,
                    strEndNo);

            // 校准首尾号码
            if (true)
            {
                nRet = DumpRecord.VerifyRange(out strError);
                if (nRet == -1)
                {
                    if (DumpRecord.ErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
                        this.DtlpChannel = null;

                    return 1;
                }
                if (nRet == 2)
                {	// 书目库为空
                    strError = "数据库 " +
                        strDbPath
                        + " 中没有记录...";
                    return 1;
                }
                if (nRet == 1)
                {
                    // 更改显示
                    strStartNo = DumpRecord.m_strStartNumber;
                    strEndNo = DumpRecord.m_strEndNumber;
                }
            }

            m_nRecordCount = -1;
            m_bSetRange = false;

            m_bFirst = false;

            if (m_nRecordCount == -1)
                m_bFirst = true;

            return 0;

            /*
            ERROR1:
                return -1;
                */

        }

        // 顺次获取下一条记录
        // return:
        //		-1	出错
        //		0	正常
        //		1	需要正常结束循环
        //		2	不处理本次记录,但继续循环
        int NextRecord(
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 准备唯一的通道
            if (this.DtlpChannel == null)
            {
                this.DtlpChannel = this.DtlpChannelArray.CreateChannel(0);
            }



            nRet = DumpRecord.NextRecord(ref m_nRecordCount,
                out strError);
            if (nRet == -1)
            {
                if (DumpRecord.ErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
                    this.DtlpChannel = null;

                strError = "NextRecord() error: " + strError;
                goto ERROR1;
            }
            if (m_bFirst == true)
            {
                m_nRangeStart = Convert.ToInt64(DumpRecord.m_strStartNumber);
                m_nRangeEnd = Convert.ToInt64(DumpRecord.m_strEndNumber);
                this.m_bSetRange = true;

                this.stop.SetProgressRange(m_nRangeStart, m_nRangeEnd);


                m_bFirst = false;
            }

            // 超过面板指定的范围
            if (nRet == 1)
                goto END1;

            // 没有找到记录
            if (nRet == 2)
            {
                strError = "记录 " + DumpRecord.m_strCurNumber + " 没有找到。批处理结束";
                goto END1;

                /*
                if (checkBox_forceLoop.Checked == false)
                {
                    statusBar_main.Text = "记录 " + DumpRecord.m_strCurNumber + " 没有找到。批处理结束";
                    goto END1;
                }

                // 不终止循环，试探性地读后面的记录
                DumpRecord.m_strStartNumber = DumpRecord.m_strCurNumber;
                statusBar_main.Text = "试探:" + DumpRecord.m_strCurNumber;

                m_nRecordCount = -1;
                return 2;	// 继续循环
                 * */
            }
            Debug.Assert(nRet == 0, "nRet必须==0");

            m_nCurPos = Convert.ToInt64(DumpRecord.m_strCurNumber);

            if (m_bSetRange == true)
            {
                this.stop.SetProgressValue(m_nCurPos);
            }

            // string strRecPath = "/" + strDbPath + "/ctlno/" + DumpRecord.m_strCurNumber;


            /*
                // 这些变量要先初始化,因为filter代码可能用到这些Batch成员.
                if (batchObj != null)
                {
                    batchObj.MarcRecord = DumpRecord.m_strRecord;	// MARC记录体
                    batchObj.MarcRecordChanged = false;	// 为本轮Script运行准备初始状态
                    batchObj.RecPath = DumpRecord.m_strPath;	// 记录路径
                    batchObj.RecIndex = m_nRecordCount - 1;	// 当前记录在一批中的序号
                    batchObj.TimeStamp = DumpRecord.m_baTimeStamp;
                }
             * */


            return 0;	// 正常
        END1:
            return 1;	// 需要结束循环
        ERROR1:
            return -1;	// 出错
        }

        #region 期数据的升级

        // 将一条MARC记录中包含的期信息变成XML格式并上传
        // parameters:
        //      strIssueDbName 订购数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int DoIssueRecordsUpload(
            string strBiblioRecPath,
            string strIssueDbName,
            string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            out int nThisIssueCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            nThisIssueCount = 0;

            int nRet = 0;

            List<string> Xmls = null;

            nRet = this.jidaoControl1.Upgrade(strMARC,
                this.textBox_dp2UserName.Text,
                out Xmls,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }

            string strWarningParam = "";

            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";

            List<EntityInfo> issueArray = new List<EntityInfo>();

            for (int i = 0; i < Xmls.Count; i++)
            {
                string strXml = Xmls[i];
                Debug.Assert(String.IsNullOrEmpty(strXml) == false, "");


                // 保存到服务器
                EntityInfo info = new EntityInfo();
                info.RefID = GenRefID();
                info.Action = "new";
                info.Style = "force,nocheckdup,noeventlog";

                info.NewRecPath = "";
                info.NewRecord = strXml;
                info.NewTimestamp = null;

                issueArray.Add(info);

                nThisIssueCount++;
            }



            // 复制到目标
            EntityInfo[] issues = new EntityInfo[issueArray.Count];
            for (int i = 0; i < issueArray.Count; i++)
            {
                issues[i] = issueArray[i];
            }

            EntityInfo[] errorinfos = null;
            nRet = SaveIssueRecords(strBiblioRecPath,
                issues,
                out errorinfos,
                out strError);
            if (nRet == -1)
                return -1;

            // 检查个别错误
            string strTempWarning = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo info = errorinfos[i];

                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    if (String.IsNullOrEmpty(strTempWarning) == false)
                        strTempWarning += "; ";

                    // string strSummary = GetLocationSummary(info.NewRecord);

                    strTempWarning += /*strSummary + " 的实体记录: " + */info.ErrorInfo;
                }
            }

            if (String.IsNullOrEmpty(strTempWarning) == false)
            {
                if (String.IsNullOrEmpty(strWarning) == false)
                    strWarning += "; ";

                strWarning = strWarning + "书目记录 " + strBiblioRecPath + " 下属的期记录创建时发生下列错误: \r" + strTempWarning;
            }

            return 0;
        }

        // 保存订购记录
        int SaveIssueRecords(string strBiblioRecPath,
            EntityInfo[] issues,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            try
            {
                long lRet = this.Channel.SetIssues(
                    stop,
                    strBiblioRecPath,
                    issues,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
            }

            return 1;
        ERROR1:
            return -1;
        }

        #endregion

        #region 订购数据的升级

        // 将一条MARC记录中包含的期刊采购信息变成XML格式并上传
        // parameters:
        //      strOrderDbName 订购数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int DoSeriesOrderRecordsUpload(
            string strBiblioRecPath,
            string strOrderDbName,
            string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            out int nThisOrderCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            nThisOrderCount = 0;

            int nRet = 0;

            string strField910 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";

            List<EntityInfo> orderArray = new List<EntityInfo>();

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
                        nThisOrderCount,
                        strParentRecordID,
                        strGroup,
                        strMARC,
                        out strXml,
                        out strWarningParam,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "创建记录id " + strParentRecordID + " 之订购(序号) " + Convert.ToString(g + 1) + "时发生错误: " + strError;
                        return -1;
                    }

                    if (String.IsNullOrEmpty(strWarningParam) == false)
                        strWarning += strWarningParam + "; ";

                    // 保存到服务器
                    EntityInfo info = new EntityInfo();
                    info.RefID = GenRefID();
                    info.Action = "new";
                    info.Style = "force,nocheckdup,noeventlog";   // 必须用forcenew 而不能用 new。因为后者会在保存记录的时候，自动去掉borrower等字段。

                    info.NewRecPath = "";
                    info.NewRecord = strXml;
                    info.NewTimestamp = null;

                    orderArray.Add(info);

                    nThisOrderCount++;
                }

            }



            // 复制到目标
            EntityInfo[] orders = new EntityInfo[orderArray.Count];
            for (int i = 0; i < orderArray.Count; i++)
            {
                orders[i] = orderArray[i];
            }

            EntityInfo[] errorinfos = null;
            nRet = SaveOrderRecords(strBiblioRecPath,
                orders,
                out errorinfos,
                out strError);
            if (nRet == -1)
                return -1;

            // 检查个别错误
            string strTempWarning = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo info = errorinfos[i];

                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    if (String.IsNullOrEmpty(strTempWarning) == false)
                        strTempWarning += "; ";

                    // string strSummary = GetLocationSummary(info.NewRecord);

                    strTempWarning += /*strSummary + " 的实体记录: " + */info.ErrorInfo;
                }
            }

            if (String.IsNullOrEmpty(strTempWarning) == false)
            {
                if (String.IsNullOrEmpty(strWarning) == false)
                    strWarning += "; ";

                strWarning = strWarning + "书目记录 " + strBiblioRecPath + " 下属的订购记录创建时发生下列错误: \r" + strTempWarning;
            }

            return 0;
        }


        // 将一条MARC记录中包含的图书采购信息变成XML格式并上传
        // TODO: 完成期刊订购数据的升级
        // parameters:
        //      strOrderDbName 订购数据库名
        //      strParentRecordID   父记录ID
        //      strMARC 父记录MARC
        int DoBookOrderRecordsUpload(
            string strBiblioRecPath,
            string strOrderDbName,
            string strParentRecordID,
            string strMARC,
            string strMarcSyntax,
            out int nThisOrderCount,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            nThisOrderCount = 0;

            int nRet = 0;

            string strField960 = "";
            string strNextFieldName = "";

            string strWarningParam = "";

            // 规范化parent id，去掉前面的'0'
            strParentRecordID = strParentRecordID.TrimStart(new char[] { '0' });
            if (String.IsNullOrEmpty(strParentRecordID) == true)
                strParentRecordID = "0";

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

            List<EntityInfo> orderArray = new List<EntityInfo>();

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
                    strParentRecordID,
                    strGroup,
                    strMARC,
                    out strXml,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "创建记录id " + strParentRecordID + " 之订购(序号) " + Convert.ToString(g + 1) + "时发生错误: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    strWarning += strWarningParam + "; ";

                // 保存到服务器
                EntityInfo info = new EntityInfo();
                info.RefID = GenRefID();
                info.Action = "new";
                info.Style = "force,nocheckdup,noeventlog";   // 必须用forcenew 而不能用 new。因为后者会在保存记录的时候，自动去掉borrower等字段。

                info.NewRecPath = "";
                info.NewRecord = strXml;
                info.NewTimestamp = null;

                orderArray.Add(info);

                nThisOrderCount++;
            }

            // 复制到目标
            EntityInfo[] orders = new EntityInfo[orderArray.Count];
            for (int i = 0; i < orderArray.Count; i++)
            {
                orders[i] = orderArray[i];
            }

            EntityInfo[] errorinfos = null;
            nRet = SaveOrderRecords(strBiblioRecPath,
                orders,
                out errorinfos,
                out strError);
            if (nRet == -1)
                return -1;

            // 检查个别错误
            string strTempWarning = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                EntityInfo info = errorinfos[i];

                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    if (String.IsNullOrEmpty(strTempWarning) == false)
                        strTempWarning += "; ";

                    // string strSummary = GetLocationSummary(info.NewRecord);

                    strTempWarning += /*strSummary + " 的实体记录: " + */info.ErrorInfo;
                }
            }

            if (String.IsNullOrEmpty(strTempWarning) == false)
            {
                if (String.IsNullOrEmpty(strWarning) == false)
                    strWarning += "; ";

                strWarning = strWarning + "书目记录 " + strBiblioRecPath + " 下属的订购记录创建时发生下列错误: \r" + strTempWarning;
            }

            return 0;
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
            string strParentID,
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
            DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

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

                    nRet = Date8toRfc1123(strOrderTime,
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
            string strParentID,
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
            DomUtil.SetElementText(dom.DocumentElement, "parent", strParentID);

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

                    nRet = Date8toRfc1123(strOrderTime,
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
            // TODO: 是否等于更藏分配策略?

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


        // 保存订购记录
        int SaveOrderRecords(string strBiblioRecPath,
            EntityInfo[] orders,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存册信息 ...");
            stop.BeginLoop();

            this.Update();
             * */

            try
            {
                long lRet = this.Channel.SetOrders(
                    stop,
                    strBiblioRecPath,
                    orders,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                 * */
            }

            return 1;
        ERROR1:
            return -1;
        }

        #endregion
    }


    // 针对一个普通子字段组的描述
    public class NormalGroup
    {
        public string strValue = "";

    }
}
