using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Web;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;


namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 迪科远望一卡通读者信息同步 批处理任务
    /// </summary>
    public class DkywReplication : BatchTask
    {
        // 已经完成了黑名单同步的日期。如果设置了这个日期，就表明同一天内再也不用做了
        string BlackListDoneDate = "";

        // internal AutoResetEvent eventDownloadFinished = new AutoResetEvent(false);	// true : initial state is signaled 
        // bool DownloadCancelled = false;
        // Exception DownloadException = null;

        // 构造函数
        public DkywReplication(LibraryApplication app,
            string strName)
            : base(app, strName)
        {
            this.Loop = true;

            this.PerTime = 5 * 60 * 1000;	// 5分钟
        }

        public override string DefaultName
        {
            get
            {
                return "迪科远望一卡通读者信息同步";
            }
        }



        // 解析 开始 参数
        // parameters:
        //      strStart    启动字符串。格式为XML
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParseDkywReplicationStart(string strStart,
            out string strRecordID,
            out string strError)
        {
            strError = "";
            strRecordID = "";

            // int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                // strError = "启动参数不能为空";
                // return -1;
                strRecordID = "1";
                return 0;
            }

            if (strStart == "!breakpoint")
            {
                /*
                // 从断点记忆文件中读出信息
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = this.App.ReadBatchTaskBreakPointFile(
                    this.DefaultName,
                    out strStart,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ReadBatchTaskBreakPointFile时出错：" + strError;
                    this.App.WriteErrorLog(strError);
                    return -1;
                }

                // 如果nRet == 0，表示没有断点文件存在，也就没有必要的参数来启动这个任务
                if (nRet == 0)
                {
                    strError = "当前服务器没有发现 " + this.DefaultName + " 断点信息，无法启动任务";
                    return -1;
                }

                Debug.Assert(nRet == 1, "");
                this.AppendResultText("服务器记忆的 " + this.DefaultName + " 上次断点字符串为: "
                    + HttpUtility.HtmlEncode(strStart)
                    + "\r\n");
                */
                strRecordID = strStart;
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strStart);
            }
            catch (Exception ex)
            {
                strError = "装载XML字符串 '"+strStart+"'进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode nodeLoop = dom.DocumentElement.SelectSingleNode("loop");
            if (nodeLoop != null)
            {
                strRecordID = DomUtil.GetAttr(nodeLoop, "recordid");
            }

            return 0;
        }

        /*
        public static string MakeDkywReplicationParam(
    bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }*/


        // 解析通用启动参数
        // 格式
        /*
         * <root loop='...'/>
         * loop缺省为true
         * 
         * */
        public static int ParseDkywReplicationParam(string strParam,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bLoop = true;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数 '"+strParam+"' 装入XML DOM时出错: " + ex.Message;
                return -1;
            }

            // 缺省为true
            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
    "loop");
            if (strLoop.ToLower() == "no"
                || strLoop.ToLower() == "false")
                bLoop = false;
            else
                bLoop = true;

            return 0;
        }


        public override void Worker()
        {
            // 系统挂起的时候，不运行本线程
            // 2007/12/18
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;
            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // 按照缺省值来

            // 通用启动参数
            bool bLoop = true;
            int nRet = ParseDkywReplicationParam(startinfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            string strID = "";
            nRet = ParseDkywReplicationStart(startinfo.Start,
                out strID,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("启动失败: " + strError + "\r\n");
                this.Loop = false;
                return;
            }


            if (strID == "!breakpoint")
            {
                string strLastNumber = "";
                bool bTempLoop = false;

                nRet = ReadLastNumber(
                    out bTempLoop,
                    out strLastNumber,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "从断点文件中获取最大号码时发生错误: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                strID = strLastNumber;
            }

            try
            {
                // 把数据文件写入有映射关系的读者库
                this.AppendResultText("同步读者数据开始\r\n");

                string strMaxNumber = "";   // 返回操作末尾的最大号
                try
                {
                    // return:
                    //      -1  error
                    //      0   succeed
                    //      1   中断
                    nRet = WriteToReaderDb(strID,
                        out strMaxNumber,
                        out strError);
                }
                finally
                {
                    // 写入文件，记忆已经做过的最大号码
                    // 要用bLoop，这是来自启动面板的值；不能用this.Loop 因为中断时其值已经被改变
                    if (String.IsNullOrEmpty(strMaxNumber) == true)
                    {
                        // 如果运行出错或者根本没有新源记录，连一条也没有成功作过，就保持原来的断点记录号
                        // 如果写入的断点记录号是空，下次运行的时候，将从'1'开始。这一般是不能接受的
                        WriteLastNumber(bLoop, strID);
                    }
                    else
                        WriteLastNumber(bLoop, strMaxNumber);
                }

                if (nRet == -1)
                {
                    string strErrorText = "写入读者库: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                else if (nRet == 1)
                {
                    this.AppendResultText("同步读者数据被中断\r\n");
                    return;
                }
                else
                {
                    this.AppendResultText("同步读者数据完成\r\n");
                    Debug.Assert(this.App != null, "");
                }

                this.AppendResultText("兑现黑名单开始\r\n");
                // 将黑名单中的卡挂失入读者库
                // parameters:
                // return:
                //      -1  error
                //      0   succeed
                //      1   中断
                nRet = DoBlackList(out strError);
                if (nRet == -1)
                {
                    string strErrorText = "兑现黑名单: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }
                else if (nRet == 1)
                {
                    this.AppendResultText("兑现黑名单被中断\r\n");
                    return;
                }
                else
                {
                    this.AppendResultText("兑现黑名单完成\r\n");
                    Debug.Assert(this.App != null, "");
                }
            }
            finally
            {
                this.StartInfo.Start = "!breakpoint"; // 自动循环的时候，没有号码，要从断点文件中取得
            }
        }

        // new
        // 读取上次最后处理的号码
        // parameters:
        //
        // return:
        //      -1  出错
        //      0   没有找到断点信息
        //      1   找到了断点信息
        public int ReadLastNumber(
            out bool bLoop,
            out string strLastNumber,
            out string strError)
        {
            bLoop = false;
            strLastNumber = "";
            strError = "";

            string strBreakPointString = "";
            // 从断点记忆文件中读出信息
            // return:
            //      -1  error
            //      0   file not found
            //      1   found
            int nRet = this.App.ReadBatchTaskBreakPointFile(this.DefaultName,
                            out strBreakPointString,
                            out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            // return:
            //      -1  xml error
            //      0   not found
            //      1   found
            nRet = ParseBreakPointString(
                strBreakPointString,
                out bLoop,
                out strLastNumber);
            return 1;

            /*
            strError = "";
            strLastNumber = "";

            string strFileName = PathUtil.MergePath(this.App.DkywDir, "lastnumber.txt");

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(strFileName, Encoding.UTF8);
            }
            catch (FileNotFoundException )
            {
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "open file '" + strFileName + "' error : " + ex.Message;
                return -1;
            }
            try
            {
                strLastNumber = sr.ReadLine();  // 读入时间行
            }
            finally
            {
                sr.Close();
            }

            return 1;
             * */
        }

        // 构造断点字符串
        static string MakeBreakPointString(
            bool bLoop,
            string strRecordID)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                "recordID",
                strRecordID);
            DomUtil.SetElementText(dom.DocumentElement,
                "loop",
                bLoop == true ? "true" : "false");

            return dom.OuterXml;
        }

        // return:
        //      -1  xml error
        //      0   not found
        //      1   found
        static int ParseBreakPointString(
            string strBreakPointString,
            out bool bLoop,
            out string strRecordID)
        {
            bLoop = false;
            strRecordID = "";

            if (String.IsNullOrEmpty(strBreakPointString) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strBreakPointString);
            }
            catch
            {
                return -1;
            }

            string strLoop = DomUtil.GetElementText(dom.DocumentElement,
                "loop");
            if (strLoop == "true")
                bLoop = true;

            strRecordID = DomUtil.GetElementText(dom.DocumentElement,
                "recordID");

            return 1;
        }

        // new
        // 写入断点记忆文件
        public void WriteLastNumber(
            bool bLoop,
            string strLastNumber)
        {
            string strBreakPointString = MakeBreakPointString(bLoop, strLastNumber);

            // 写入断点文件
            this.App.WriteBatchTaskBreakPointFile(this.DefaultName,
                strBreakPointString);

            /*
            string strFileName = PathUtil.MergePath(this.App.DkywDir, "lastnumber.txt");

            // 删除原来的文件
            File.Delete(strFileName);

            // 写入新内容
            StreamUtil.WriteText(strFileName,
                strLastNumber);
             * */
        }

        // 获得数据字典
        // parameters:
        //      strValueFieldName   值字段名。如果为逗号分割的形态，表示要把若干字段值取出并拼接在一起
        int GetDictionary(string strTableName,
            string strCodeFieldName,
            string strValueFieldNames,
            out Hashtable result,
            out string strError)
        {
            strError = "";

            string [] value_field_names = strValueFieldNames.Split(new char[] {','});

            result = new Hashtable();

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//dkyw/dataCenter");
            if (node == null)
            {
                strError = "尚未配置<dkyw><dataCenter>参数";
                return -1;
            }
            string strConnectionString = DomUtil.GetAttr(node, "connection");
            if (String.IsNullOrEmpty(strConnectionString) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的connection属性";
                return -1;
            }

            string strDbName = DomUtil.GetAttr(node, "db");
            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的db属性";
                return -1;
            }


            SqlConnection connection = new SqlConnection(strConnectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                strError = "连接到SQL服务器失败: " + ex.Message;
                return -1;
            }

            try
            {
                SqlCommand command = null;
                SqlDataReader dr = null;

                string strCommand = "";

                strCommand = "use " + strDbName + "\r\nselect * from " + strTableName;
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    dr = command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    strError = "查询SQL时出错: "
                        + ex.Message + "; "
                        + "SQL命令: "
                        + strCommand;
                    return -1;
                }

                for (; ;)
                {
                    Thread.Sleep(1);    // 避免处理太繁忙

                    if (this.Stopped == true)
                    {
                        return 1;
                    }

                    try
                    {
                        if (dr == null || dr.HasRows == false)
                        {
                            return 0;
                        }
                        if (dr.Read() == false)
                            break;
                    }
                    catch (Exception ex)
                    {
                        strError = "读SQL表行发生错误: " + ex.Message;
                        return -1;
                    }

                    // 获得字段值
                    string strCode = GetSqlStringValue(dr, strCodeFieldName);
                    strCode = strCode.Trim();
                    string strValue = "";

                    List<string> temp_values = new List<string>();
                    for (int i = 0; i < value_field_names.Length; i++)
                    {
                        string strText = GetSqlStringValue(dr, value_field_names[i]);
                        strText = strText.Trim();

                        temp_values.Add(strText.Trim());
                    }

                    // 去掉末尾连续的空字符串
                    for (int i = temp_values.Count-1; i > 0; i--)
                    {
                        if (String.IsNullOrEmpty(temp_values[i]) == true)
                            temp_values.RemoveAt(i);
                        else
                            break;
                    }

                    for (int i = 0; i < temp_values.Count; i++)
                    {
                        if (i > 0)
                        {
                            strValue += ", ";
                        }
                        strValue += temp_values[i];
                    }

                    result[strCode] = strValue;
                }
                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetDictionary() Exception: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
            }
        }

        // 创建读者记录XML
        int BuildReaderXml(
            string strBarcode,
            string strName,
            string strGender,
            // string strReaderType,
            string strDepartment,
            string strPost,
            string strBornDate,
            string strIdCardNumber,
            string strAddress,
            string strComment,
            string strCreateDate,
            out string strXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                "barcode", strBarcode);
            /*
            DomUtil.SetElementText(dom.DocumentElement,
                "state", strState);
             * */
            DomUtil.SetElementText(dom.DocumentElement,
                "name", strName);
            DomUtil.SetElementText(dom.DocumentElement,
                "gender", strGender);
            /*
            DomUtil.SetElementText(dom.DocumentElement,
                "readerType", strReaderType);
             * */
            DomUtil.SetElementText(dom.DocumentElement,
                "department", strDepartment);
            DomUtil.SetElementText(dom.DocumentElement,
                "post", strPost);
            DomUtil.SetElementText(dom.DocumentElement,
                "bornDate", strBornDate);
            DomUtil.SetElementText(dom.DocumentElement,
                "idCardNumber", strIdCardNumber);
            if (String.IsNullOrEmpty(strAddress) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "address", strAddress);
            }
            if (String.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "comment", strComment);
            }
            DomUtil.SetElementText(dom.DocumentElement,
                "createDate", strCreateDate);

            strXml = dom.DocumentElement.OuterXml;

            return 0;
        }

        static string GetSqlStringValue(SqlDataReader dr,
            string strFieldName)
        {
            if (dr[strFieldName] is System.DBNull)
                return "";

            return (string)dr[strFieldName];
        }

        static int GetSqlIntValue(SqlDataReader dr,
    string strFieldName)
        {
            if (dr[strFieldName] is System.DBNull)
                return 0;

            return (int)dr[strFieldName];
        }


        // 将用户信息更新表(User_Infor_Message)写入读者库
        // parameters:
        //      strLastNumber   如果为空，表示全部处理
        // return:
        //      -1  error
        //      0   succeed
        //      1   中断
        int WriteToReaderDb(string strLastNumber,
            out string strMaxNumber,
            out string strError)
        {
            strError = "";
            strMaxNumber = "";
            int nRet = 0;

            /*
    <dkyw>
        <dataCenter connection="Persist Security Info=False;User ID=dp2rms;Password=dp2rms;Data Source=test111;Connect Timeout=30" db="zzdy" startTime="20:00" />
        <replication mapDbName="读者" />
    </dkyw>
             * */

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//dkyw/replication");
            if (node == null)
            {
                strError = "尚未配置<dkyw><replication>参数";
                return -1;
            }

            string strReaderDbName = DomUtil.GetAttr(node, "mapDbName");
            if (String.IsNullOrEmpty(strReaderDbName) == true)
            {
                strError = "尚未配置<dkyw/replication>元素的mapDbName属性";
                return -1;
            }

            node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//dkyw/dataCenter");
            if (node == null)
            {
                strError = "尚未配置<dkyw><dataCenter>参数";
                return -1;
            }
            string strConnectionString = DomUtil.GetAttr(node, "connection");
            if (String.IsNullOrEmpty(strConnectionString) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的connection属性";
                return -1;
            }

            string strDbName = DomUtil.GetAttr(node, "db");
            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的db属性";
                return -1;
            }

            // 身份代码字典
            Hashtable pid_table = null;
            nRet = GetDictionary("Pid_Ctrl",
                "PID",
                "PNAME",
                out pid_table,
                out strError);
            if (nRet == -1)
                return -1;

            // 部门代码字典
            Hashtable dept_table = null;
            nRet = GetDictionary("Dept_Ctrl",
                "DeptStr",
                "DeptName1,DeptName2,DeptName3,DeptName4,DeptName5",
                out dept_table,
                out strError);
            if (nRet == -1)
                return -1;

            // 职位(岗位)代码字典
            Hashtable job_table = null;
            nRet = GetDictionary("Job",
                "Code",
                "Name",
                out job_table,
                out strError);
            if (nRet == -1)
                return -1;

            SqlConnection connection = new SqlConnection(strConnectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                strError = "连接到SQL服务器失败: " + ex.Message;
                return -1;
            }

            try
            {
                SqlCommand command = null;
                SqlDataReader dr = null;

                string strCommand = "";

                strCommand = "use " + strDbName + "\r\nselect * from User_Infor_Message";
                if (String.IsNullOrEmpty(strLastNumber) == false)
                {
                    strCommand += " where IDNumber > " + strLastNumber;
                }

                strCommand += " order by IDNumber";

                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    dr = command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    strError = "查询SQL时出错: "
                        + ex.Message + "; "
                        + "SQL命令: "
                        + strCommand;
                    return -1;
                }

                // bool bRet = false;

                // 临时的SessionInfo对象
                SessionInfo sessioninfo = new SessionInfo(this.App);

                // 模拟一个账户
                Account account = new Account();
                account.LoginName = "replication";
                account.Password = "";
                account.Rights = "setreaderinfo,devolvereaderinfo";

                account.Type = "";
                account.Barcode = "";
                account.Name = "replication";
                account.UserID = "replication";
                account.RmsUserName = this.App.ManagerUserName;
                account.RmsPassword = this.App.ManagerPassword;

                sessioninfo.Account = account;

                int nRecordCount = 0;
                for (int i = 0; ; i++)
                {
                    Thread.Sleep(1);    // 避免处理太繁忙

                    if (this.Stopped == true)
                    {
                        return 1;
                    }

                    try
                    {
                        if (dr == null || dr.HasRows == false)
                        {
                            break;
                        }
                        if (dr.Read() == false)
                            break;
                    }
                    catch (Exception ex)
                    {
                        strError = "读SQL表行发生错误: " + ex.Message;
                        return -1;
                    }

                    // 获得字段值
                    int nIDNumber = GetSqlIntValue(dr, "IDNumber");

                    this.SetProgressText("同步 " + (i + 1).ToString() + " IDNumber=" + nIDNumber.ToString());
                    this.AppendResultText("同步 " + (i + 1).ToString() + " IDNumber=" + nIDNumber.ToString() + "\r\n");

                    string strMessageType = GetSqlStringValue(dr,"MessageType");
                    string strCardNo = GetSqlStringValue(dr,"CARDNO");
                    string strCardID = GetSqlStringValue(dr,"CARDID");
                    string strOldCardNo = GetSqlStringValue(dr,"OLDCARDNO");
                    string strOldCardID = GetSqlStringValue(dr,"OLDCARDID");
                    string strCardType = GetSqlStringValue(dr,"CDTYPE");
                    string strUserName = GetSqlStringValue(dr,"USERNAME");
                    string strIdType = GetSqlStringValue(dr,"IDTYPE");
                    string strIdSerial = GetSqlStringValue(dr,"IDSERIAL");
                    string strPersonID = GetSqlStringValue(dr,"PID");
                    string strDepartmentCode = GetSqlStringValue(dr,"DEPTSTR");
                    string strCountryCode = GetSqlStringValue(dr,"CTRCODE");
                    string strNationCode = GetSqlStringValue(dr,"NATCODE");
                    string strSex = GetSqlStringValue(dr,"SEX");
                    string strBirthday = GetSqlStringValue(dr,"BIRTHDAY");
                    string strInSchoolDate = GetSqlStringValue(dr,"INSCHOOL");
                    string strJobCode = GetSqlStringValue(dr,"JOBCODE");
                    string strRecType = GetSqlStringValue(dr,"RECTYPE");
                    string strGrade = GetSqlStringValue(dr,"GRADE");
                    string strIdSerial1 = GetSqlStringValue(dr,"IDSERIAL1");
                    string strOtherString = GetSqlStringValue(dr,"OTHERSTR");


                    // 规整号码字符串
                    if (String.IsNullOrEmpty(strOldCardNo) == false)
                        strOldCardNo = strOldCardNo.Trim().PadLeft(8, '0');

                    string strXml = "";
                    string strBarcode = strCardNo.Trim().PadLeft(8, '0');;

                    string strRfc1123Birthday = "";

                    if (String.IsNullOrEmpty(strBirthday) == false)
                    {
                        nRet = DateTimeUtil.Date8toRfc1123(strBirthday,
                            out strRfc1123Birthday,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    string strCreateDate = "";

                    if (String.IsNullOrEmpty(strInSchoolDate) == false)
                    {
                        nRet = DateTimeUtil.Date8toRfc1123(strInSchoolDate,
                            out strCreateDate,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    // 构造记录体
                    nRet = BuildReaderXml(
                        strBarcode,
                        strUserName,
                        strSex == "0" ? "女" : "男",
                        // (string)pid_table[strPersonID.Trim()],
                        (string)dept_table[strDepartmentCode.Trim()],
                        (string)pid_table[strPersonID.Trim()],  // (string)job_table[strJobCode.Trim()],
                        strRfc1123Birthday,
                        strIdSerial1,   // 身份证号
                        strOtherString, // address
                        "", // comment
                        strCreateDate,
                        out strXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    /*
                    if (nIDNumber > 200)
                    {
                        strError = "模拟错误";
                        return -1;
                    }*/

                    if (strMessageType == "3")
                    {
                        // 进行换卡操作
                        // parameters:
                        //      strOriginReaderXml  原始记录。里面的<barcode>元素值为新的卡号
                        //      strOldCardNo    旧的卡号。
                        // return:
                        //      -1  error
                        //      0   已经写入
                        //      1   没有必要写入
                        nRet = DoChangeCard(
                            sessioninfo,
                            strOldCardNo,
                            strReaderDbName,
                            strXml,
                            out strError);
                    }
                    else
                    {
                        // return:
                        //      -1  error
                        //      0   已经写入
                        //      1   没有必要写入
                        nRet = WriteOneReaderInfo(
                            sessioninfo,
                            strMessageType,
                            strReaderDbName,
                            strXml,
                            out strError);
                    }

                    if (nRet == -1)
                        return -1;

                    // 记录处理完成的记录ID
                    strMaxNumber = nIDNumber.ToString();

                    nRecordCount++;
                }

                this.SetProgressText("同步读者记录完成，实际处理记录 " + nRecordCount.ToString() + " 条");

                if (nRecordCount == 0)
                {
                    if (String.IsNullOrEmpty(strLastNumber) == false)
                        this.AppendResultText("没有大于记录号 " + strLastNumber + "的任何新记录\r\n");
                    else
                        this.AppendResultText("没有任何记录\r\n");
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "WriteToReaderDb() Exception: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
            }
        }


        // 进行换卡操作
        // parameters:
        //      strOriginReaderXml  原始记录。里面的<barcode>元素值为新的卡号
        //      strOldCardNo    旧的卡号。
        // return:
        //      -1  error
        //      0   已经写入
        //      1   没有必要写入
        int DoChangeCard(
            SessionInfo sessioninfo,
            string strOldCardNo,
            string strReaderDbName,
            string strOriginReaderXml,
            out string strError)
        {
            strError = "";

            string strOperType = "replace"; // replace -- 换卡； change -- 修改新记录； new -- 创建新记录

            bool bNewRecordWrited = false;  // 新记录内容是否已经写入

            // 检查参数
            if (String.IsNullOrEmpty(strOldCardNo) == true)
            {
                strError = "操作类型为 换卡 时，strOldCardNo参数值不能为空";
                return -1;
            }

            XmlDocument origin_dom = new XmlDocument();

            try
            {
                origin_dom.LoadXml(strOriginReaderXml);
            }
            catch (Exception ex)
            {
                strError = "原始XML片段装入DOM失败: " + ex.Message;
                return -1;
            }

            string strNewState = "";

            string strNewBarcode = DomUtil.GetElementText(origin_dom.DocumentElement,
                "barcode");
            if (String.IsNullOrEmpty(strNewBarcode) == true)
            {
                strError = "缺乏<barcode>元素";
                return -1;
            }

            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            /*
             * 第一步，转移流通信息
             * */
            if (strNewBarcode != strOldCardNo)
            {
            REDO_DEVOLVE:
                // 转移借阅信息
                // 将源读者记录中的<borrows>和<overdues>转移到目标读者记录中
                // result.Value:
                //      -1  error
                //      0   没有必要转移。即源读者记录中没有需要转移的借阅信息
                //      1   已经成功转移
                LibraryServerResult result1 = this.App.DevolveReaderInfo(
                    sessioninfo,
                    strOldCardNo,
                    strNewBarcode);
                if (result1.Value == -1)
                {
                    if (result1.ErrorCode == ErrorCode.SourceReaderBarcodeNotFound)
                    {
                        // 源记录没有找到。变成新创建目标读者记录的操作
                        strOperType = "create";
                    }
                    else if (result1.ErrorCode == ErrorCode.TargetReaderBarcodeNotFound)
                    {
                        // 目标记录没有找到。需要先创建目标，然后重新进行移动
                        LibraryServerResult result = this.App.SetReaderInfo(
                                sessioninfo,
                                "new",
                                strReaderDbName + "/?",
                                origin_dom.OuterXml,
                                "", // strReaderXml,
                                null,   // baTimestamp,
                                out strExistingXml,
                                out strSavedXml,
                                out strSavedRecPath,
                                out baNewTimestamp,
                                out kernel_errorcode);
                        if (result.Value == -1)
                        {
                            strError = "换卡的时候发现新记录不存在，先创建新记录，其过程发生错误：" + result.ErrorInfo;
                            return -1;
                        }

                        bNewRecordWrited = true;

                        goto REDO_DEVOLVE;
                    }
                    else
                    {
                        strError = "换卡操作中，转移流通信息('"+strOldCardNo+"' --> '"+strNewBarcode+"')的时候出错：" + result1.ErrorInfo;
                        return -1;
                    }
                }
            }
            else
            {
                // 要改的旧号和新号相同
                // 变为“只写入新记录”
                strOperType = "change";
            }

            int nRet = 0;
            string strReaderXml = "";
            string strOutputPath = "";
            byte[] baTimestamp = null;

            /*
             * 第二步，删除源记录
             * */
            if (strOperType == "replace")
            {
                // 加读锁
                // 可以避免拿到读者记录处理中途的临时状态
                this.App.ReaderLocks.LockForRead(strOldCardNo);
                try
                {
                    // 获得库中的目标读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.App.GetReaderRecXml(
                        this.RmsChannels, // sessioninfo.Channels,
                        strOldCardNo,
                        out strReaderXml,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);

                }
                finally
                {
                    this.App.ReaderLocks.UnlockForRead(strOldCardNo);
                }

                if (nRet == -1)
                {
                    strError = "换卡操作中，获得旧记录 '" + strOldCardNo + "' 时出错: " + strError;
                    return -1;
                }

                if (nRet > 1)
                {
                    strError = "条码号 " + strOldCardNo + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                    return -1;
                }

                // 卡改号
                // 这里要删除旧卡

                XmlDocument temp_dom = new XmlDocument();
                try
                {
                    temp_dom.LoadXml(strReaderXml);
                }
                catch (Exception ex)
                {
                    strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                    return -1;
                }

                /*
                DomUtil.SetElementInnerXml(temp_dom.DocumentElement,
                    "barcode", strOldCardNo);
                 * */

                LibraryServerResult result = this.App.SetReaderInfo(
                    sessioninfo,
                    "delete",
                    "", //        strRecPath,
                    "", //        strNewXml,
                    temp_dom.OuterXml,  // strOldXml
                    null,   // baTimestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedRecPath,
                    out baNewTimestamp,
                    out kernel_errorcode);
                if (result.Value == -1)
                {
                    if (result.ErrorCode == ErrorCode.ReaderBarcodeNotFound)
                    {
                        // 记录已经不存在
                    }
                    else if (result.ErrorCode == ErrorCode.HasCirculationInfo)
                    {
                        // 这种情况不太可能发生，因为转移操作已经把旧卡的流通信息清除了
                        // TODO: 是否要报错?

                        // 读者记录包含有流通信息
                        // 改为修改记录
                        // 将状态修改为“删除”，但是不删除记录
                        strNewState = "删除";
                        DomUtil.SetElementText(origin_dom.DocumentElement,
                            "state",
                            strNewState);

                        // 依然是修改目标记录

                        strOperType = "change";
                    }
                    else
                    {
                        strError = "换卡操作中，删除源记录 '"+strOldCardNo+"' 时出错: " + result.ErrorInfo;
                        return -1;
                    }
                }
            }

            // 新记录在前面已经写入
            if (bNewRecordWrited == true)
                return 0;

            /*
             * 第二步，修改目标记录
             * */

            // 加读锁
            // 可以避免拿到读者记录处理中途的临时状态
            this.App.ReaderLocks.LockForRead(strNewBarcode);

            try
            {
                // 获得库中的目标读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXml(
                    this.RmsChannels, // sessioninfo.Channels,
                    strNewBarcode,
                    out strReaderXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);

            }
            finally
            {
                this.App.ReaderLocks.UnlockForRead(strNewBarcode);
            }

            if (nRet == -1)
                return -1;
            if (nRet > 1)
            {
                strError = "条码号 " + strNewBarcode + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                return -1;
            }

            string strAction = "";
            string strRecPath = "";

            string strNewXml = "";  // 修改后的记录

            if (nRet == 0)
            {
                // 记录不存在

                // 没有命中，创建新记录
                strAction = "new";
                strRecPath = strReaderDbName + "/?";
                strReaderXml = "";  // "<root />";
            }
            else
            {
                // 记录存在

                Debug.Assert(nRet == 1, "");

                strAction = "change";
                strRecPath = strOutputPath;
            }

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(String.IsNullOrEmpty(strReaderXml) == false ? strReaderXml : "<root />");
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                return -1;
            }

            // 根据来自SQL表的数据修改或者创建记录
            // return:
            //      -1  error
            //      0   没有发生修改
            //      1   发生了修改
            nRet = ModifyReaderRecord(ref readerdom,
                origin_dom,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0) // 没有发生修改，没有必要写入
            {
                return 1;
            }

            // 修改目标读者记录
            {
                strNewXml = readerdom.OuterXml;

                LibraryServerResult result = this.App.SetReaderInfo(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        strNewXml,
                        strReaderXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strSavedRecPath,
                        out baNewTimestamp,
                        out kernel_errorcode);
                if (result.Value == -1)
                {
                    strError = "换卡操作中，修改目标记录时出错: " + result.ErrorInfo;
                    return -1;
                }
            }

            return 0;   // 正常写入了
        }

        // 进行新增、修改、删除的操作
        // parameters:
        //      strOperType     0 新增 1 删除 2 修改 3 换卡
        // return:
        //      -1  error
        //      0   已经写入
        //      1   没有必要写入
        int WriteOneReaderInfo(
            SessionInfo sessioninfo,
            string strOperType,
            string strReaderDbName,
            string strOriginReaderXml,
            out string strError)
        {
            strError = "";

            // 检查参数
            if (strOperType == "3")
            {
                strError = "操作类型为 '" + strOperType + "' (换卡) 时，不能调用WriteOneReaderInfo()函数";
                return -1;
            }

            XmlDocument origin_dom = new XmlDocument();

            try
            {
                origin_dom.LoadXml(strOriginReaderXml);
            }
            catch (Exception ex)
            {
                strError = "原始XML片段装入DOM失败: " + ex.Message;
                return -1;
            }

            string strNewState = "";

            string strBarcode = DomUtil.GetElementText(origin_dom.DocumentElement,
                "barcode");
            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "缺乏<barcode>元素";
                return -1;
            }

            int nRet = 0;
            string strReaderXml = "";
            string strOutputPath = "";
            byte[] baTimestamp = null;

            // 加读锁
            // 可以避免拿到读者记录处理中途的临时状态
            this.App.ReaderLocks.LockForRead(strBarcode);

            try
            {
                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXml(
                    this.RmsChannels, // sessioninfo.Channels,
                    strBarcode,
                    out strReaderXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);

            }
            finally
            {
                this.App.ReaderLocks.UnlockForRead(strBarcode);
            }

            if (nRet == -1)
                return -1;
            if (nRet > 1)
            {
                strError = "条码号 " + strBarcode + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                return -1;
            }

            string strAction = "";
            string strRecPath = "";

            string strNewXml = "";  // 修改后的记录


            string strExistingXml = "";
            string strSavedXml = "";
            string strSavedRecPath = "";
            byte[] baNewTimestamp = null;
            DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;


            if (nRet == 0)
            {
                // 记录不存在

                if (strOperType == "1")
                    return 0;   // 如果是删除操作，而数据库中正好没有，就算了

                // 没有命中，创建新记录
                strAction = "new";
                strRecPath = strReaderDbName + "/?";
                strReaderXml = "";  // "<root />";
            }
            else
            {
                // 记录存在

                Debug.Assert(nRet == 1, "");
                // 命中，修改后覆盖原记录

                // 删除卡
                if (strOperType == "1")
                {
                    strAction = "delete";
                    strRecPath = strOutputPath;

                    LibraryServerResult result = this.App.SetReaderInfo(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        "", //        strNewXml,
                        strReaderXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strSavedRecPath,
                        out baNewTimestamp,
                        out kernel_errorcode);
                    if (result.Value == -1)
                    {
                        if (result.ErrorCode == ErrorCode.ReaderBarcodeNotFound)
                        {
                            // 记录已经不存在
                            return 0;
                        }
                        else if (result.ErrorCode == ErrorCode.HasCirculationInfo)
                        {
                            // 读者记录包含有流通信息
                            // 改为修改记录
                            // 将状态修改为“删除”，但是不删除记录
                            strNewState = "删除";
                            DomUtil.SetElementText(origin_dom.DocumentElement,
                                "state",
                                strNewState);

                            strOperType = "2";
                            strAction = "change";
                            strRecPath = strOutputPath;

                            // TODO: 在操作日志中写入一条，请图书馆员催该读者还书？
                        }
                        else
                            return -1;
                    }
                    else
                        return 0;
                }
                else
                {
                    strAction = "change";
                    strRecPath = strOutputPath;
                }
            }

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(String.IsNullOrEmpty(strReaderXml) == false ? strReaderXml : "<root />");
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                return -1;
            }

            // 根据来自SQL表的数据修改或者创建记录
            // return:
            //      -1  error
            //      0   没有发生修改
            //      1   发生了修改
            nRet = ModifyReaderRecord(ref readerdom,
                origin_dom,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0) // 没有发生修改，没有必要写入
            {
                return 1;
            }

            // 修改读者记录
            {
                strNewXml = readerdom.OuterXml;

                LibraryServerResult result = this.App.SetReaderInfo(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        strNewXml,
                        strReaderXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strSavedRecPath,
                        out baNewTimestamp,
                        out kernel_errorcode);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
            }

            return 0;   // 正常写入了
        }



        // 根据来自SQL表的数据修改或者创建记录
        // return:
        //      -1  error
        //      0   没有发生修改
        //      1   发生了修改
        int ModifyReaderRecord(ref XmlDocument readerdom,
            XmlDocument origin_dom,
            out string strError)
        {
            strError = "";
            // int nRet = 0;
            bool bChanged = false;

            for (int i = 0; i < origin_dom.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode node = origin_dom.DocumentElement.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                string strName = node.Name;

                XmlNode node_find = readerdom.DocumentElement.SelectSingleNode(strName);
                if (node_find != null)
                {
                    if (node_find.InnerXml != node.InnerXml)
                    {
                        node_find.InnerXml = node.InnerXml;
                        bChanged = true;
                    }
                }
                else
                {
                    node_find = readerdom.CreateElement(strName);
                    readerdom.DocumentElement.AppendChild(node_find);
                    node_find.InnerXml = node.InnerXml;
                    bChanged = true;
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }


        // 将黑名单中的卡挂失入读者库
        // parameters:
        // return:
        //      -1  error
        //      0   succeed
        //      1   中断
        int DoBlackList(out string strError)
        {
            strError = "";
            int nRet = 0;

            DateTime timeStart = DateTime.Now;

            if (String.IsNullOrEmpty(this.BlackListDoneDate) == false)
            {
                // 当日没有必要重复了
                if (this.BlackListDoneDate == DateTimeUtil.DateTimeToString8(timeStart))
                {
                    this.AppendResultText("本日("+this.BlackListDoneDate+")内不再重做\r\n");
                    return 0;
                }
            }

            /*
    <dkyw>
        <dataCenter connection="Persist Security Info=False;User ID=dp2rms;Password=dp2rms;Data Source=test111;Connect Timeout=30" db="zzdy" startTime="20:00" />
        <replication mapDbName="读者" />
    </dkyw>
             * */
            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//dkyw/replication");
            if (node == null)
            {
                strError = "尚未配置<dkyw><replication>参数";
                return -1;
            }

            string strReaderDbName = DomUtil.GetAttr(node, "mapDbName");
            if (String.IsNullOrEmpty(strReaderDbName) == true)
            {
                strError = "尚未配置<dkyw/replication>元素的mapDbName属性";
                return -1;
            }

            node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//dkyw/dataCenter");
            if (node == null)
            {
                strError = "尚未配置<dkyw><dataCenter>参数";
                return -1;
            }
            string strConnectionString = DomUtil.GetAttr(node, "connection");
            if (String.IsNullOrEmpty(strConnectionString) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的connection属性";
                return -1;
            }

            string strDbName = DomUtil.GetAttr(node, "db");
            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未配置<dkyw/dataCenter>元素的db属性";
                return -1;
            }

            // 临时的SessionInfo对象
            SessionInfo sessioninfo = new SessionInfo(this.App);

            // 模拟一个账户
            Account account = new Account();
            account.LoginName = "replication";
            account.Password = "";
            account.Rights = "setreaderinfo";

            account.Type = "";
            account.Barcode = "";
            account.Name = "replication";
            account.UserID = "replication";
            account.RmsUserName = this.App.ManagerUserName;
            account.RmsPassword = this.App.ManagerPassword;

            sessioninfo.Account = account;

            /*
             * 检索出全部挂失状态的读者记录
             * */

            List<string> loss_barcodes = null;
                    // 根据读者证状态对读者库进行检索
        // parameters:
        //      strMatchStyle   匹配方式 left exact right middle
        //      strState  读者证状态
        //      bOnlyIncirculation  是否仅仅包括参与流通的数据库? true ：仅仅包括； false : 包括全部
        //      bGetPath    == true 获得path; == false 获得barcode
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
            nRet = this.App.SearchReaderState(
                sessioninfo.Channels,
                "挂失",
                "left",
                false,
                false,  // bGetPath,
                -1,
                out loss_barcodes,
                out strError);
            if (nRet == -1)
            {
                strError = "检索全部挂失读者记录信息时出错: " + strError;
                return -1;
            }

            if (nRet == 0)
            {
                if (loss_barcodes == null)
                    loss_barcodes = new List<string>();
            }

            SqlConnection connection = new SqlConnection(strConnectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                strError = "连接到SQL服务器失败: " + ex.Message;
                return -1;
            }

            try
            {
                SqlCommand command = null;
                SqlDataReader dr = null;

                string strCommand = "";

                strCommand = "use " + strDbName + "\r\nselect * from balck_list";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    dr = command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    strError = "查询SQL时出错: "
                        + ex.Message + "; "
                        + "SQL命令: "
                        + strCommand;
                    return -1;
                }

                // bool bRet = false;



                for (int i = 0; ; i++)
                {
                    Thread.Sleep(1);    // 避免处理太繁忙

                    if (this.Stopped == true)
                    {
                        return 1;
                    }

                    try
                    {
                        if (dr == null || dr.HasRows == false)
                        {
                            break;
                        }
                        if (dr.Read() == false)
                            break;
                    }
                    catch (Exception ex)
                    {
                        strError = "读SQL表行发生错误: " + ex.Message;
                        return -1;
                    }

                    // 获得字段值
                    string strCardNo = GetSqlStringValue(dr, "CardNo");
                    // 规整号码字符串
                    if (String.IsNullOrEmpty(strCardNo) == false)
                        strCardNo = strCardNo.PadLeft(8, '0');

                    this.SetProgressText("挂失 " + (i + 1).ToString() + " CardNumber=" + strCardNo);

                    // 观察集合中是否已经具有
                    int nIndex = loss_barcodes.IndexOf(strCardNo);
                    if (nIndex != -1)
                    {
                        loss_barcodes.RemoveAt(nIndex);
                        this.AppendResultText("挂失 " + (i + 1).ToString() + " CardNumber=" + strCardNo + "  原本就是挂失状态\r\n");
                        continue;
                    }

                    this.AppendResultText("挂失 " + (i + 1).ToString() + " CardNumber=" + strCardNo + " ");

                    string strLossDate = GetSqlStringValue(dr, "LossDate");

                    /*
                    string strRfc1123LossDate = "";

                    if (String.IsNullOrEmpty(strLossDate) == false)
                    {
                        nRet = DateTimeUtil.Date8toRfc1123(strLossDate,
                            out strRfc1123LossDate,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                     * */

                    // return:
                    //      -1  error
                    //      0   已经写入
                    //      1   没有必要写入
                    nRet = LossOneReaderInfo(
                            sessioninfo,
                            strCardNo,
                            strLossDate,
                            out strError);
                    if (nRet == -1)
                        return -1;

                    this.AppendResultTextNoTime(strError + "\r\n");
                }

                // Thread.Sleep(2 * 60 * 1000);    // test
                // 现在集合中剩下的，就是黑名单以外的，状态不应为“挂失”的条码号
                for (int i = 0; i < loss_barcodes.Count; i++)
                {
                    string strBarcode = loss_barcodes[i];

                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    this.SetProgressText("解挂 " + (i + 1).ToString() + " CardNumber=" + strBarcode);

                    this.AppendResultText("解挂 " + (i + 1).ToString() + " CardNumber=" + strBarcode + " ");
                    // return:
                    //      -1  error
                    //      0   已经写入
                    //      1   没有必要写入
                    nRet = UnLossOneReaderInfo(
                            sessioninfo,
                            strBarcode,
                            out strError);
                    if (nRet == -1)
                        return -1;

                    this.AppendResultTextNoTime(strError + "\r\n");
                }

                // 观察消耗的时间
                TimeSpan delta = DateTime.Now - timeStart;
                int nMaxMinutes = 5;
                if (delta.Minutes > nMaxMinutes)
                {
                    // 如果超过5分钟，则记载下当日的日期，避免同一日后面重做
                    this.BlackListDoneDate = DateTimeUtil.DateTimeToString8(timeStart);
                    this.AppendResultText("黑名单同步过程处理时间为 "+delta.ToString()+"，超过了 "+nMaxMinutes.ToString()+" 分钟，本日("+this.BlackListDoneDate+")内将不再重复处理\r\n");
                }

                this.SetProgressText("同步黑名单完成，耗费时间 " + delta.ToString() + " ");

                return 0;
            }
            catch (Exception ex)
            {
                strError = "DoBlackList() Exception: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
            finally
            {
            }
        }

        // 进行解除挂失的操作
        // parameters:
        // return:
        //      -1  error
        //      0   已经写入
        //      1   没有必要写入
        int UnLossOneReaderInfo(
            SessionInfo sessioninfo,
            string strBarcode,
            out string strError)
        {
            strError = "";

            int nRet = 0;
            string strReaderXml = "";
            string strOutputPath = "";
            byte[] baTimestamp = null;

            // 加读锁
            // 可以避免拿到读者记录处理中途的临时状态
            this.App.ReaderLocks.LockForRead(strBarcode);

            try
            {
                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXml(
                    this.RmsChannels, // sessioninfo.Channels,
                    strBarcode,
                    out strReaderXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);

            }
            finally
            {
                this.App.ReaderLocks.UnlockForRead(strBarcode);
            }

            if (nRet == -1)
                return -1;
            if (nRet > 1)
            {
                strError = "条码号 " + strBarcode + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                return -1;
            }

            if (nRet == 0)
            {
                // 记录既然不存在，就没有必要解除挂失
                strError = "读者记录不存在";
                return 1;
            }

            string strAction = "";
            string strRecPath = "";

            string strNewXml = "";  // 修改后的记录



            // 记录存在

            Debug.Assert(nRet == 1, "");
            // 命中，修改后覆盖原记录

            strAction = "change";
            strRecPath = strOutputPath;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(String.IsNullOrEmpty(strReaderXml) == false ? strReaderXml : "<root />");
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                return -1;
            }

            string strOldOuterValue = DomUtil.GetElementOuterXml(readerdom.DocumentElement,
                "state");

            string strValue = DomUtil.GetElementText(readerdom.DocumentElement,
                "state");
            string strHead = "";
            if (strValue.Length >= "挂失".Length)
                strHead = strValue.Substring(0, "挂失".Length);
            else
            {
                // 原有值不是“挂失”，放弃写入
                strError = "原有状态为 '" + strValue + "'，不是挂失状态，放弃修改";
                return 1;
            }

            if (strHead != "挂失")
            {
                // 原有值不是“挂失”，放弃写入
                strError = "原有状态为 '" + strValue + "'，不是挂失状态，放弃修改";
                return 1;
            }

            DomUtil.SetElementText(readerdom.DocumentElement,
                "state", "");
            string strNewOuterValue = DomUtil.GetElementOuterXml(readerdom.DocumentElement,
                "state");

            if (strOldOuterValue == strNewOuterValue) // 没有发生修改，没有必要写入
            {
                strError = "记录没有发生修改";
                return 1;
            }

            // 修改读者记录
            {
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedRecPath = "";
                byte[] baNewTimestamp = null;
                DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                strNewXml = readerdom.OuterXml;

                LibraryServerResult result = this.App.SetReaderInfo(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        strNewXml,
                        strReaderXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strSavedRecPath,
                        out baNewTimestamp,
                        out kernel_errorcode);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
            }

            strError = "正常写入";
            return 0;   // 正常写入了
        }

        // 进行挂失操作
        // parameters:
        // return:
        //      -1  error
        //      0   已经写入
        //      1   没有必要写入
        int LossOneReaderInfo(
            SessionInfo sessioninfo,
            string strBarcode,
            string strLossDate,
            out string strError)
        {
            strError = "";

            int nRet = 0;
            string strReaderXml = "";
            string strOutputPath = "";
            byte[] baTimestamp = null;

            // 加读锁
            // 可以避免拿到读者记录处理中途的临时状态
            this.App.ReaderLocks.LockForRead(strBarcode);

            try
            {
                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.App.GetReaderRecXml(
                    this.RmsChannels, // sessioninfo.Channels,
                    strBarcode,
                    out strReaderXml,
                    out strOutputPath,
                    out baTimestamp,
                    out strError);

            }
            finally
            {
                this.App.ReaderLocks.UnlockForRead(strBarcode);
            }

            if (nRet == -1)
                return -1;
            if (nRet > 1)
            {
                strError = "条码号 " + strBarcode + "在读者库群中检索命中 " + nRet.ToString() + " 条，请尽快更正此错误。";
                return -1;
            }

            if (nRet == 0)
            {
                // 记录既然不存在，就没有必要挂失
                strError = "读者记录不存在";
                return 1;
            }

            string strAction = "";
            string strRecPath = "";

            string strNewXml = "";  // 修改后的记录



            // 记录存在

            Debug.Assert(nRet == 1, "");
            // 命中，修改后覆盖原记录

            strAction = "change";
            strRecPath = strOutputPath;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(String.IsNullOrEmpty(strReaderXml) == false ? strReaderXml : "<root />");
            }
            catch (Exception ex)
            {
                strError = "读者XML记录装入DOM发生错误: " + ex.Message;
                return -1;
            }

            string strOldOuterValue = DomUtil.GetElementOuterXml(readerdom.DocumentElement,
                "state");
            DomUtil.SetElementText(readerdom.DocumentElement,
                "state", "挂失 (" + strLossDate + ")");
            string strNewOuterValue = DomUtil.GetElementOuterXml(readerdom.DocumentElement,
                "state");

            if (strOldOuterValue == strNewOuterValue) // 没有发生修改，没有必要写入
            {
                strError = "记录没有发生修改";
                return 1;
            }

            // 修改读者记录
            {
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedRecPath = "";
                byte[] baNewTimestamp = null;
                DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                strNewXml = readerdom.OuterXml;

                LibraryServerResult result = this.App.SetReaderInfo(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        strNewXml,
                        strReaderXml,
                        baTimestamp,
                        out strExistingXml,
                        out strSavedXml,
                        out strSavedRecPath,
                        out baNewTimestamp,
                        out kernel_errorcode);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }
            }

            strError = "正常写入";
            return 0;   // 正常写入了
        }

#if NOOOOOOOOOOOOOOOOOOOOOOOOOO
        static string GetAccStatusString(string strAccStatus)
        {
            if (strAccStatus == "0")
                return "已撤户";
            if (strAccStatus == "1")
                return "有效卡";
            if (strAccStatus == "2")
                return "挂失卡";
            if (strAccStatus == "3")
                return "冻结卡";
            if (strAccStatus == "4")
                return "预撤户";
            return strAccStatus;    // 不是预定义的值
        }
        // 获得数据中心配置参数
        int GetDataCenterParam(
            out string strServerUrl,
            out string strUserName,
            out string strPassword,
            out string strError)
        {
            strError = "";
            strServerUrl =
            strUserName = "";
            strPassword = "";

            XmlNode node = this.App.LibraryCfgDom.DocumentElement.SelectSingleNode("//zhengyuan/dataCenter");

            if (node == null)
            {
                strError = "尚未配置<zhangyuan/dataCenter>元素";
                return -1;
            }

            strServerUrl = DomUtil.GetAttr(node, "url");
            strUserName = DomUtil.GetAttr(node, "username");
            strPassword = DomUtil.GetAttr(node, "password");

            return 0;
        }

        // 下载数据文件
        // parameters:
        //      strDataFileName 数据文件名。纯粹的文件名。
        //      strLocalFilePath    本地文件名
        // return:
        //      -1  出错
        //      0   正常结束
        //      1   被用户中断
        int DownloadDataFile(string strDataFileName,
            string strLocalFilePath,
            out string strError)
        {
            strError = "";

            string strServerUrl = "";
            string strUserName = "";
            string strPassword = "";

            // 获得数据中心配置参数
            int nRet = GetDataCenterParam(
                out strServerUrl,
                out strUserName,
                out strPassword,
                out strError);
            if (nRet == -1)
                return -1;

            string strPath = strServerUrl + "/" + strDataFileName;

            Uri serverUri = new Uri(strPath);

            /*
            // The serverUri parameter should start with the ftp:// scheme.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
            }
             * */


            // Get the object used to communicate with the server.
            WebClient request = new WebClient();

            this.DownloadException = null;
            this.DownloadCancelled = false;
            this.eventDownloadFinished.Reset();

            request.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(request_DownloadFileCompleted);
            request.DownloadProgressChanged += new DownloadProgressChangedEventHandler(request_DownloadProgressChanged);

            request.Credentials = new NetworkCredential(strUserName,
                strPassword);

            try
            {

                File.Delete(strLocalFilePath);

                request.DownloadFileAsync(serverUri,
                    strLocalFilePath);
            }
            catch (WebException ex)
            {
                strError = "下载数据文件 " + strPath + " 失败: " + ex.ToString();
                return -1;
            }

            // 等待下载结束

            WaitHandle[] events = new WaitHandle[2];

            events[0] = this.eventClose;
            events[1] = this.eventDownloadFinished;

            while (true)
            {
                if (this.Stopped == true)
                {
                    request.CancelAsync();
                }

                int index = WaitHandle.WaitAny(events, 1000, false);    // 每秒超时一次

                if (index == WaitHandle.WaitTimeout)
                {
                    // 超时
                }
                else if (index == 0)
                {
                    strError = "下载被关闭信号提前中断";
                    return -1;
                }
                else
                {
                    // 得到结束信号
                    break;
                }
            }

            if (this.DownloadCancelled == true)
                return 1;   // 被用户中断

            if (this.DownloadException != null)
            {
                strError = this.DownloadException.Message;
                if (this.DownloadException is WebException)
                {
                    WebException webex = (WebException)this.DownloadException;
                    if (webex.Response is FtpWebResponse)
                    {
                        FtpWebResponse ftpr = (FtpWebResponse)webex.Response;
                        if (ftpr.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            return -1;
                        }
                    }

                }
                return -1;
            }

            return 0;
        }

        void request_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if ((e.BytesReceived % 1024 * 100) == 0)
                this.AppendResultText("已下载: " + e.BytesReceived + "\r\n");
        }

        void request_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DownloadException = e.Error;
            this.DownloadCancelled = e.Cancelled;
            this.eventDownloadFinished.Set();
        }

#endif

        static string GetCurrentDate()
        {
            DateTime now = DateTime.Now;

            return now.Year.ToString().PadLeft(4, '0')
            + now.Month.ToString().PadLeft(2, '0')
            + now.Day.ToString().PadLeft(2, '0');
        }


    }
}
