using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Data;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.dp2.Statis;

namespace dp2Circulation
{


#if NO
    public class OperLogStatisForm111
    {
        Task1 _task1 = new Task1();

        int DoTask1(out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在执行统计任务 ...");
            stop.BeginLoop();

            try
            {
                int nRet = DoTask1Begin(out strError);
                if (nRet == -1)
                    return -1;
                // 循环
                nRet = DoTask1Loop(out strError);
                if (nRet == -1)
                    return -1;
                nRet = DoTask1End(out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }
        }

        int DoTask1Begin(out string strError)
        {
            strError = "";

            _task1 = new Task1();

            return 0;
        }

#if NO
        // 任务1，每次记录处理
        int DoTask1Record(string strLogFileName,
string strXml,
bool bInCacheFile,
long lHint,
long lIndex,
long lAttachmentTotalLength,
object param,
out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            string strDate = "";
            int nRet = strLogFileName.IndexOf(".");
            if (nRet != -1)
                strDate = strLogFileName.Substring(0, nRet);
            else
                strDate = strLogFileName;

            DateTime currentDate = DateTimeUtil.Long8ToDateTime(strDate);
            // strXml中为日志记录
#if NO
            // 触发Script中OnRecord()代码
            if (objStatis != null)
            {
                objStatis.Xml = strXml;
                objStatis.CurrentDate = currentDate;
                objStatis.CurrentLogFileName = strLogFileName;
                objStatis.CurrentRecordIndex = lIndex;

                StatisEventArgs args = new StatisEventArgs();
                objStatis.OnRecord(this, args);
                if (args.Continue == ContinueType.SkipAll)
                    return 1;
            }
#endif

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "Load Xml to DOM error: " + ex.Message;
                return -1;
            }

            // this.WriteTextToConsole(this.CurrentLogFileName + ":" + this.CurrentRecordIndex.ToString() + "\r\n");

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");

            if (strOperation != "borrow" && strOperation != "return")
                return 0;

            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");

            string strAccessClass = "";	// 索书号中的分类号
            string strBiblioRecPath = "";	// 书目记录路径

            string strLocation = "";    // 册馆藏地点

            XmlNode nodeItem = null;
            string strItemXml = DomUtil.GetElementText(dom.DocumentElement,
                    "itemRecord", out nodeItem);
            string strItemRecPath = "";
            if (nodeItem != null)
                strItemRecPath = DomUtil.GetAttr(nodeItem, "recPath");

            // 册记录相关的书目记录路径，这个后面统一提取，就不用日志记录中的数据了

            XmlNode nodeReader = null;
            string strReaderXml = DomUtil.GetElementText(dom.DocumentElement,
                    "readerRecord", out nodeReader);
            string strReaderRecPath = DomUtil.GetAttr(nodeReader, "recPath");
            string strReaderDbName = Global.GetDbName(strReaderRecPath);



            return 0;
        }

#endif

        int DoTask1End(out string strError)
        {
            strError = "";


            return 0;
        }


        // 对每个日志文件，每个日志记录进行循环
        // return:
        //      0   普通返回
        //      1   要全部中断
        int DoTask1Loop(out string strError)
        {
            strError = "";
            int nRet = 0;
            // long lRet = 0;

            List<string> LogFileNames = null;

            // TODO: 是否需要检查起止日期是否为空值？空值是警告还是就当作今天？

            string strStartDate = DateTimeUtil.DateTimeToString8(this.dateControl_start.Value);
            string strEndDate = DateTimeUtil.DateTimeToString8(this.dateControl_end.Value);

            string strWarning = "";

            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            nRet = MakeLogFileNames(strStartDate,
                strEndDate,
                true,
                out LogFileNames,
                out strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

#if NO
            string strStyle = "";
            if (this.MainForm.AutoCacheOperlogFile == true)
                strStyle = "autocache";
#endif

            ProgressEstimate estimate = new ProgressEstimate();

#if NO
            nRet = OperLogForm.ProcessFiles(this,
stop,
estimate,
Channel,
LogFileNames,
this.MainForm.OperLogLevel,
strStyle,
this.MainForm.OperLogCacheDir,
null,   // param,
procDoRecord,   // DoRecord,
out strError);
            if (nRet == -1)
                return -1;
#endif

            OperLogLoader loader = new OperLogLoader();
            loader.Channel = this.Channel;
            loader.Stop = this.Stop;
            loader.owner = this;
            loader.estimate = estimate;
            loader.FileNames = LogFileNames;
            loader.nLevel = this.MainForm.OperLogLevel;
            loader.AutoCache = false;
            loader.CacheDir = "";


            List<OperLogLine> lines = new List<OperLogLine>();

            foreach (OperLogItem item in loader)
            {
                string strXml = item.Xml;

                if (string.IsNullOrEmpty(strXml) == true)
                    continue;

                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "Load Xml to DOM error: " + ex.Message;
                        return -1;
                    }

                    string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                    if (strOperation != "borrow" && strOperation != "return")
                        continue;

                    string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
                    string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");

#if NO
                    XmlNode nodeItem = null;
                    string strItemXml = DomUtil.GetElementText(dom.DocumentElement,
                            "itemRecord", out nodeItem);
                    string strItemRecPath = "";
                    if (nodeItem != null)
                        strItemRecPath = DomUtil.GetAttr(nodeItem, "recPath");

                    // 册记录相关的书目记录路径，这个后面统一提取，就不用日志记录中的数据了

#endif
                    XmlNode nodeReader = null;
                    string strReaderXml = DomUtil.GetElementText(dom.DocumentElement,
                            "readerRecord", out nodeReader);
                    string strReaderRecPath = DomUtil.GetAttr(nodeReader, "recPath");
                    string strReaderDbName = Global.GetDbName(strReaderRecPath);
                    // TODO: 根据读者库名获得馆代码
                    string strLibraryCode = "";

                    string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                        "itemBarcode");
                    string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                        "readerBarcode");
                    string strOperTime = DomUtil.GetElementText(dom.DocumentElement,
                        "operTime");

                    OperLogLine line = new OperLogLine();
                    line.ItemBarcode = strItemBarcode;
                    // 馆藏地点需要另行获得
                    line.ReaderBarcode = strReaderBarcode;
                    line.OperTime = strOperTime;
                    line.LibraryCode = strLibraryCode;

                    lines.Add(line);

                }


                if (lines.Count > 300)
                {
                    // 写入数据库一次

                    lines.Clear();
                }

            }

            if (lines.Count > 0)
            {
                // 写入数据库一次
            }

            return nRet;
        }

        // 填充册记录的馆藏地点列，填充馆代码列
        // 因为这些字段将用于切分表格
        int FillItemAndReaderFields(List<OperLogLine> lines,
            out string strError)
        {
            strError = "";


            return 0;
        }

        // 插入一批日志记录
        int AppendOperLogLines(
            SQLiteConnection connection,
            List<OperLogLine> lines,
            out string strError)
        {
            strError = "";

            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {

                StringBuilder text = new StringBuilder(4096);
                int i = 0;
                foreach (OperLogLine line in lines)
                {
                    text.Append(
    " INSERT INTO records(operation, action, itembarcode, location, readerbarcode, librarycode, opertime) "
    + " VALUES(@operation" + i
    + ", @action" + i
    + ", @itembarcode" + i
    + ", @location" + i
    + ", @readerbarcode" + i
    + ", @librarycode" + i
    + ", @opertime" + i + ")"
    + " ; ");
                    SQLiteUtil.SetParameter(command,
                        "@operation" + i,
                        line.Operation);
                    SQLiteUtil.SetParameter(command,
    "@action" + i,
    line.Action); 
                    
                    SQLiteUtil.SetParameter(command,
     "@itembarcode" + i,
     line.ItemBarcode);
                    
                    SQLiteUtil.SetParameter(command,
     "@location" + i,
     line.ItemLocation);
                    
                    SQLiteUtil.SetParameter(command,
     "@librarycode" + i,
     line.LibraryCode);
                    
                    SQLiteUtil.SetParameter(command,
     "@opertime" + i,
     line.OperTime);

                    i++;
                }

                command.CommandText = text.ToString();
                int nCount = command.ExecuteNonQuery();

            }

            return 0;
        }

        // 创建日志表
        int CreateOperLogTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "CREATE TABLE records "
                + "(" + " "
                // + "id nvarchar (255) NULL UNIQUE," + " "
                + "operation nvarchar (255) NULL," + " "
                + "action nvarchar (255) NULL ," + " "
                + "itembarcode nvarchar (255) NULL ," + " "
                + "location nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " "
                + "librarycode nvarchar (255) NULL ," + " "
                + "opertime nvarchar (255) NULL  "
                + ") ; ";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "建表出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }
    }


#endif

    // 任务数据结构
    public class Task1
    {
        TableCollection tables = new TableCollection();

    }


}
