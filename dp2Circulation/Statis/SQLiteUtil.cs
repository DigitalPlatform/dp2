using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Xml;
using System.IO;

using System.Data.SQLite;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 针对 SQLite 数据库进行操作的实用类
    /// </summary>
    class SQLiteUtil
    {
        public static string GetLocalTime(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                return "";

            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strTime, "s");
            }
            catch
            {
                return "";
            }
        }

        public static void SetParameter(SQLiteCommand command,
            string strName,
            string strValue)
        {
            SQLiteParameter param =
command.Parameters.Add(strName,
DbType.String);
            param.Value = strValue;
        }

        public static void SetParameter(SQLiteCommand command,
    string strName,
    long value)
        {
            SQLiteParameter param =
command.Parameters.Add(strName,
DbType.Int64);
            param.Value = value;
        }

        public static string GetConnectionString(string strDataDir,
            string strDatabaseFileName)
        {
            return "Data Source=" + Path.Combine(strDataDir, strDatabaseFileName)
    + ";Page Size=8192";   // Synchronues=OFF;;Cache Size=70000
        }

        public static string GetDatabaseFilePath(string strDataDir,
    string strDatabaseFileName)
        {
            return Path.Combine(strDataDir, strDatabaseFileName);
        }
    }

    // 操作日志库
    // 创建和维护表
    static class OperLogTable
    {
        // 全部数据库类型
        public static string[] DbTypes = {
                                  "circu",
                                  "patron",
                                  "biblio",
                                  "item",
                                  "order",
                                  "issue",
                                  "comment",
                                  "amerce",
                                  "passgate",
                                  "getres",
                                  };

        public static int IndexOfType(string strType)
        {
            int i = 0;
            foreach (string type in DbTypes)
            {
                if (type == strType)
                    return i;
                i++;
            }

            return -1;
        }

        // 创建日志表
        public static int CreateOperLogTable(
            string strDbType,
            string strConnectionString,
            out string strError)
        {
            strError = "";

            string strTableName = "operlog" + strDbType;
            string strFields = "";
            if (strDbType == "circu")
            {
                strFields = "itembarcode nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "patron")
            {
                strFields = "readerrecpath nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "biblio")
            {
                strFields = "bibliorecpath nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "item")
            {
                strFields = "bibliorecpath nvarchar (255) NULL ," + " "
                    // + "itembarcode nvarchar (255) NULL ," + " "
                + "itemrecpath nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "order")
            {
                strFields = "bibliorecpath nvarchar (255) NULL ," + " "
                + "itemrecpath nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "issue")
            {
                strFields = "bibliorecpath nvarchar (255) NULL ," + " "
                + "itemrecpath nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "comment")
            {
                strFields = "bibliorecpath nvarchar (255) NULL ," + " "
                + "itemrecpath nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "amerce")
            {
                strFields = "price integer NULL ," + " "
                + "unit nvarchar (255) NULL ," + " "
                + "amercerecpath nvarchar (255) NULL ," + " "
                + "reason nvarchar (255) NULL ," + " "
                + "itembarcode nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "passgate")
            {
                strFields = // "librarycode nvarchar (255) NULL ," + " " +
                "gatename nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " ";
            }
            else if (strDbType == "getres")
            {
                strFields = "xmlrecpath nvarchar (255) NULL ," + " "
                + "objectid nvarchar (255) NULL ," + " "
                + "size nvarchar (255) NULL ," + " "
                + "mime nvarchar (255) NULL ," + " ";
            }
            else
            {
                strError = "未知的数据库类型 '" + strDbType + "'";
                return -1;
            }

            // 创建表
            string strCommand = "DROP TABLE if exists " + strTableName + " ;\n"
                + "CREATE TABLE " + strTableName + " "
                + "(" + " "
                + "date nvarchar (255) NULL," + " "
                + "no integer ," + " "
                + "subno integer ," + " "
                + "librarycode nvarchar (255) NULL," + " "  // 2016/5/5
                + "operation nvarchar (255) NULL," + " "
                + "action nvarchar (255) NULL ," + " "

#if NO
                + "itembarcode nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " "
#endif
 + strFields
                + "operator nvarchar (255) NULL," + " "
                + "opertime nvarchar (255) NULL  "
                + ") ; ";

            strCommand += " CREATE UNIQUE INDEX " + strTableName + "_id_index \n"
    + " ON " + strTableName + " (date, no, subno) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "建表出错\r\n"
                            + ex.Message + "\r\n"
                            + "SQL 命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }

        // 创建报表阶段需要的附加索引
        public static int CreateAdditionalIndex(
            string strDbType,
            string strConnectionString,
            out string strError)
        {
            strError = "";

            string strTableName = "operlog" + strDbType;
            string strCommand = "";

            if (strDbType == "circu")
            {
                strCommand = "CREATE INDEX IF NOT EXISTS " + strTableName + "_itembarcode_index \n"
         + " ON " + strTableName + " (itembarcode); \n"
         + " CREATE INDEX IF NOT EXISTS " + strTableName + "_readerbarcode_index "
         + " ON " + strTableName + " (readerbarcode); \n";
            }
            else if (strDbType == "patron")
            {

            }

            if (string.IsNullOrEmpty(strCommand) == false)
            {
                using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(strCommand,
        connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            strError = "创建索引出错.\r\n"
                                + ex.Message + "\r\n"
                                + "SQL命令:\r\n"
                                + strCommand;
                            return -1;
                        }

                    } // end of using command
                }
            }

            return 0;
        }

        // 删除创建报表阶段才需要的附加索引
        public static int DeleteAdditionalIndex(
            string strDbType,
            string strConnectionString,
            out string strError)
        {
            strError = "";

            string strTableName = "operlog" + strDbType;
            string strCommand = "";

            if (strDbType == "circu")
            {
                strCommand = "DROP INDEX IF EXISTS " + strTableName + "_itembarcode_index;  "
        + " DROP INDEX IF EXISTS " + strTableName + "_readerbarcode_index;";
            }
            else if (strDbType == "patron")
            {

            }

            if (string.IsNullOrEmpty(strCommand) == false)
            {
                using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(strCommand,
        connection))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            strError = "删除索引出错.\r\n"
                                + ex.Message + "\r\n"
                                + "SQL命令:\r\n"
                                + strCommand;
                            return -1;
                        }

                    } // end of using command
                }
            }

            return 0;
        }
    }

    // 日志行 基础类
    class OperLogLineBase
    {
        public string Date = "";  // 所在日志文件日期，8 字符
        public long No = 0;
        public long SubNo = 0;  // 2014/6/16 子序号。用于区分一个日志记录拆分为多个的情况
        public string LibraryCode = ""; // 2016/5/5
        public string Operation = "";
        public string Action = "";
        public string OperTime = "";
        public string Operator = "";    // 2014/4/20

        public static MainForm MainForm = null;

        // 根据日志 XML 记录填充数据
        // 本函数负责填充基类的数据成员
        public virtual int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";
            lines = null;

            if (string.IsNullOrEmpty(strDate) == true
                || strDate.Length != 8)
            {
                strError = "strDate 的值 '" + strDate + "' 格式错误，应该为 8 字符的数字";
                return -1;
            }

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strOperTime = DomUtil.GetElementText(dom.DocumentElement,
                "operTime");
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");

            Debug.Assert(strDate.Length == 8, "");
            this.Date = strDate;
            this.No = lIndex;
            this.SubNo = 0;
            this.LibraryCode = "," + strLibraryCode + ",";  // 这样便于构造 SQL like 语句
            this.Operation = strOperation;
            this.Action = strAction;
            this.OperTime = SQLiteUtil.GetLocalTime(strOperTime);
            this.Operator = strOperator;

            return 0;
        }

        // 复制成员
        public void CopyTo(OperLogLineBase another)
        {
            another.Date = this.Date;
            another.No = this.No;
            another.SubNo = this.SubNo;
            another.LibraryCode = this.LibraryCode;
            another.Operation = this.Operation;
            another.Action = this.Action;
            another.OperTime = this.OperTime;
            another.Operator = this.Operator;
        }

        // 构造一个写入命令片断
        public virtual void BuildWriteCommand(SQLiteCommand command,
    int i,
    bool bInsertOrReplace,
    StringBuilder text)
        {
            throw new Exception("尚未实现 BuidWriteCommand()");
        }
    }

    class OperLogLinesBase : List<OperLogLineBase>
    {
        // 能使用 .Count

        // 把所有累积的行写入数据库。然后清空
        public int WriteToDb(
            SQLiteConnection connection,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";

            if (this.Count == 0)
                return 0;

            Debug.WriteLine("WriteToDb() this.Count=" + this.Count);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {
                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (OperLogLineBase line in this)
                    {
                        line.BuildWriteCommand(command,
                 i,
                 bInsertOrReplace,
                 text);
                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }

                mytransaction.Commit();
            }
            return 0;
        }
    }

    // 日志行缓冲数组。用于累积后写入
    class OperLogLines<T> : OperLogLinesBase
        where T : OperLogLineBase, new()
    {
        // List<T> _array = new List<T>();

        // 在内存中增加一行
        public virtual int AddLine(XmlDocument dom,
            string strDate,
            long lIndex,
            out string strError)
        {
            strError = "";

            T line = new T();
            List<OperLogLineBase> lines = null;
            int nRet = line.SetData(dom,
                strDate,
                lIndex,
                out lines,
                out strError);
            if (nRet == -1)
                return -1;

            this.Add(line);
            if (lines != null)
            {
                // this.AddRange(lines);
                foreach (OperLogLineBase current in lines)
                {
                    this.Add(current);
                }
            }
            return 0;
        }

#if NO
        // 把所有累积的行写入数据库。然后清空
        public int WriteToDb(
            SQLiteConnection connection,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";

            if (this.Count == 0)
                return 0;

            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {

                StringBuilder text = new StringBuilder(4096);
                int i = 0;
                foreach (T line in this)
                {
#if NO
                    if (bInsertOrReplace == true)
                        text.Append(" INSERT OR REPLACE ");
                    else
                        text.Append(" INSERT ");

                    text.Append(
    " INTO operlog (date, no, subno, operation, action, itembarcode, readerbarcode, opertime) "
    + " VALUES("
    + "@date" + i
    + ", @no" + i
 + ", @subno" + i
   + ", @operation" + i
    + ", @action" + i
    + ", @readerrecpath" + i
                        // + ", @location" + i
    + ", @readerbarcode" + i
                        // + ", @librarycode" + i
    + ", @opertime" + i + ")"
    + " ; ");
                    SQLiteUtil.SetParameter(command,
                        "@date" + i,
                        line.Date);
                    SQLiteUtil.SetParameter(command,
                        "@no" + i,
                        line.No.ToString());
                    SQLiteUtil.SetParameter(command,
    "@subno" + i,
    this.SubNo.ToString());
                    SQLiteUtil.SetParameter(command,
                        "@operation" + i,
                        line.Operation);
                    SQLiteUtil.SetParameter(command,
                        "@action" + i,
                        line.Action);

                    SQLiteUtil.SetParameter(command,
                        "@readerrecpath" + i,
                        line.ReaderRecPath);

                    SQLiteUtil.SetParameter(command,
"@readerbarcode" + i,
line.ReaderBarcode);

                    SQLiteUtil.SetParameter(command,
     "@opertime" + i,
     line.OperTime);
#endif
                    line.BuidWriteCommand(command,
             i,
             bInsertOrReplace,
             text);

                    i++;
                }

                IDbTransaction trans = null;

                trans = connection.BeginTransaction();
                try
                {
                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
                }
                finally
                {
                    if (trans != null)
                        trans.Rollback();
                }
            }

            return 0;
        }
#endif
    }

    // 若干日志表的缓冲区
    class MultiBuffer
    {
        List<OperLogLinesBase> _buffers = new List<OperLogLinesBase>();

        public void Initial()
        {
            // 下标是对应的
            foreach (string type in OperLogTable.DbTypes)
            {
                if (type == "circu")
                    this._buffers.Add(new OperLogLines<CircuOperLogLine>() as OperLogLinesBase);
                else if (type == "patron")
                    this._buffers.Add(new OperLogLines<PatronOperLogLine>() as OperLogLinesBase);
                else if (type == "biblio")
                    this._buffers.Add(new OperLogLines<BiblioOperLogLine>() as OperLogLinesBase);
                else if (type == "item")
                    this._buffers.Add(new OperLogLines<ItemOperLogLine>() as OperLogLinesBase);
                else if (type == "order")
                    this._buffers.Add(new OperLogLines<OrderOperLogLine>() as OperLogLinesBase);
                else if (type == "issue")
                    this._buffers.Add(new OperLogLines<IssueOperLogLine>() as OperLogLinesBase);
                else if (type == "comment")
                    this._buffers.Add(new OperLogLines<CommentOperLogLine>() as OperLogLinesBase);
                else if (type == "amerce")
                    this._buffers.Add(new OperLogLines<AmerceOperLogLine>() as OperLogLinesBase);
                else if (type == "passgate")
                    this._buffers.Add(new OperLogLines<PassGateOperLogLine>() as OperLogLinesBase);
                else if (type == "getres")
                    this._buffers.Add(new OperLogLines<GetResOperLogLine>() as OperLogLinesBase);
                else
                    throw new Exception("未知的 dbtype '" + type + "'");
            }
        }

        // 在内存中增加一行
        // return:
        //      -2  不能识别的 strOperation 类型
        //      -1  出错
        //      0   成功
        public int AddLine(
            string strOperation,
            XmlDocument dom,
            string strDate,
            long lIndex,
            out string strError)
        {
            if (strOperation == "borrow" || strOperation == "return")
            {
                Debug.Assert(OperLogTable.IndexOfType("circu") == 0, "");
                OperLogLines<CircuOperLogLine> lines = this._buffers[0] as OperLogLines<CircuOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setReaderInfo")
            {
                Debug.Assert(OperLogTable.IndexOfType("patron") == 1, "");
                OperLogLines<PatronOperLogLine> lines = this._buffers[1] as OperLogLines<PatronOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setBiblioInfo")
            {
                Debug.Assert(OperLogTable.IndexOfType("biblio") == 2, "");
                OperLogLines<BiblioOperLogLine> lines = this._buffers[2] as OperLogLines<BiblioOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setEntity")
            {
                Debug.Assert(OperLogTable.IndexOfType("item") == 3, "");
                OperLogLines<ItemOperLogLine> lines = this._buffers[3] as OperLogLines<ItemOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setOrder")
            {
                Debug.Assert(OperLogTable.IndexOfType("order") == 4, "");
                OperLogLines<OrderOperLogLine> lines = this._buffers[4] as OperLogLines<OrderOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setIssue")
            {
                Debug.Assert(OperLogTable.IndexOfType("issue") == 5, "");
                OperLogLines<IssueOperLogLine> lines = this._buffers[5] as OperLogLines<IssueOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "setComment")
            {
                Debug.Assert(OperLogTable.IndexOfType("comment") == 6, "");
                OperLogLines<CommentOperLogLine> lines = this._buffers[6] as OperLogLines<CommentOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "amerce")
            {
                Debug.Assert(OperLogTable.IndexOfType("amerce") == 7, "");
                OperLogLines<AmerceOperLogLine> lines = this._buffers[7] as OperLogLines<AmerceOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "passgate")
            {
                Debug.Assert(OperLogTable.IndexOfType("passgate") == 8, "");
                OperLogLines<PassGateOperLogLine> lines = this._buffers[8] as OperLogLines<PassGateOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            else if (strOperation == "getRes")
            {
                Debug.Assert(OperLogTable.IndexOfType("getres") == 9, "");
                OperLogLines<GetResOperLogLine> lines = this._buffers[9] as OperLogLines<GetResOperLogLine>;
                return lines.AddLine(dom, strDate, lIndex, out strError);
            }
            strError = "不能识别的 strOperation '" + strOperation + "'";
            return -2;
        }

        const int INSERT_BATCH = 100;  // 300;

        // 把所有累积的行写入数据库。然后清空
        public int WriteToDb(
            SQLiteConnection connection,
            bool bInsertOrReplace,
            bool bForce,
            out string strError)
        {
            int i = 0;
            int nWriteCount = 0;
            foreach (OperLogLinesBase lines in this._buffers)
            {
                if (lines.Count >= INSERT_BATCH
|| (lines.Count > 0 && bForce))
                {
#if NO
                    if (i == 6)
                    {
                        Debug.Assert(false, "");
                    }
#endif

                    // 写入数据库一次
                    int nRet = lines.WriteToDb(
                             connection,
                             true,
                             out strError);
                    if (nRet == -1)
                        return -1;
                    lines.Clear();
#if NO
                strLastDate = item.Date;
                lLastIndex = item.Index + 1;
#endif
                    nWriteCount++;
                }

                i++;
            }

            strError = "";
            if (nWriteCount > 0)
                return 1;
            return 0;
        }
    }

    // 读者操作 日志行
    class PatronOperLogLine : OperLogLineBase
    {
        // 特有的字段
        public string ReaderRecPath = "";
        public string ReaderBarcode = "";

        static string TableName
        {
            get
            {
                return "operlogpatron";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            XmlNode record = dom.DocumentElement.SelectSingleNode("record");
            if (record == null)
                record = dom.DocumentElement.SelectSingleNode("oldRecord");

            if (record != null)
            {
                this.ReaderRecPath = DomUtil.GetAttr(record,
                    "recPath");
                string strRecord = record.InnerText;
                XmlDocument reader_dom = new XmlDocument();
                try
                {
                    reader_dom.LoadXml(strRecord);
                    this.ReaderBarcode = DomUtil.GetElementText(reader_dom.DocumentElement,
                        "barcode");
                }
                catch
                {
                }
            }

            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
            int i,
            bool bInsertOrReplace,
            StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, readerrecpath, readerbarcode, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @readerrecpath" + i
+ ", @readerbarcode" + i
+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
    "@subno" + i,
    this.SubNo.ToString());

            SQLiteUtil.SetParameter(command,
    "@librarycode" + i,
    this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@readerrecpath" + i,
                this.ReaderRecPath);

            SQLiteUtil.SetParameter(command,
"@readerbarcode" + i,
this.ReaderBarcode);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);

            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);
        }
    }

    // 编目操作 每行
    class BiblioOperLogLine : OperLogLineBase
    {
        // 特有的字段
        public string BiblioRecPath = "";

        static string TableName
        {
            get
            {
                return "operlogbiblio";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            XmlNode record = dom.DocumentElement.SelectSingleNode("record");
            if (record == null || strAction == "delete")    // action 为 delete 的时候， 2013.2 以前的版本会具有一个 <recorde> 元素，但 recPath 属性为空
                record = dom.DocumentElement.SelectSingleNode("oldRecord");

            if (record != null)
            {
                this.BiblioRecPath = DomUtil.GetAttr(record,
                    "recPath");
            }
            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
    int i,
    bool bInsertOrReplace,
    StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, bibliorecpath, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @bibliorecpath" + i

+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
"@subno" + i,
this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
    "@librarycode" + i,
    this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@bibliorecpath" + i,
                this.BiblioRecPath);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
                "@opertime" + i,
                this.OperTime);
        }

    }

    // order isue comment 每行的基类
    class ItemOperLogLine : OperLogLineBase
    {
        // 特有的字段
        public string ItemRecPath = "";
        public string BiblioRecPath = "";

        public virtual string TableName
        {
            get
            {
                return "operlogitem";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            XmlElement record = dom.DocumentElement.SelectSingleNode("record") as XmlElement;
            if (record == null)
                record = dom.DocumentElement.SelectSingleNode("oldRecord") as XmlElement;

            if (record != null)
            {
                this.ItemRecPath = DomUtil.GetAttr(record,
                    "recPath");
                string strParentID = record.GetAttribute("parent_id");
                if (string.IsNullOrEmpty(strParentID) == true)
                {
                    string strRecord = record.InnerText.Trim();
                    if (string.IsNullOrEmpty(strRecord) == false)
                    {
                        XmlDocument reader_dom = new XmlDocument();
                        try
                        {
                            reader_dom.LoadXml(strRecord);
                            strParentID = DomUtil.GetElementText(reader_dom.DocumentElement,
                                "parent");
                        }
                        catch (Exception ex)
                        {
                            // 2016/12/6 返回 -1
                            strError = "ItemOperLogLine.SetData() 内部出现异常: " + ExceptionUtil.GetExceptionText(ex);
                            Program.MainForm.WriteErrorLog(strError + "\r\nXML记录: " + dom.OuterXml);
                            return -1;
                        }
                    }
                }

                if (string.IsNullOrEmpty(strParentID) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strParentID) == false, "");

                    Debug.Assert(this.TableName.IndexOf("operlog") == 0, "");
                    string strDbType = this.TableName.Substring("operlog".Length);

                    // 根据实体库名得到书目库名
                    this.BiblioRecPath = MainForm.BuildBiblioRecPath(strDbType, // "item",
                        this.ItemRecPath,
                        strParentID);
                }
            }

            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
            int i,
            bool bInsertOrReplace,
            StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, itemrecpath, bibliorecpath, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @itemrecpath" + i
+ ", @bibliorecpath" + i
+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
"@subno" + i,
this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
"@librarycode" + i,
this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@itemrecpath" + i,
                this.ItemRecPath);

            SQLiteUtil.SetParameter(command,
"@bibliorecpath" + i,
this.BiblioRecPath);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);
        }
    }

    // 订购操作 每行
    class OrderOperLogLine : ItemOperLogLine
    {
        public override string TableName
        {
            get
            {
                return "operlogorder";
            }
        }
    }

    // 期操作 每行
    class IssueOperLogLine : ItemOperLogLine
    {
        public override string TableName
        {
            get
            {
                return "operlogissue";
            }
        }
    }

    // 评注操作
    class CommentOperLogLine : ItemOperLogLine
    {
        public override string TableName
        {
            get
            {
                return "operlogcomment";
            }
        }
    }

    // 交费操作
    class AmerceOperLogLine : OperLogLineBase
    {
        // 特有的字段
        public string AmerceRecPath = "";
        public string ItemBarcode = "";
        public string ReaderBarcode = "";
        public string Unit = "";
        public long Price = 0;  // 分
        public string Reason = "";
        public string ID = "";  // 2016/12/6

        static string TableName
        {
            get
            {
                return "operlogamerce";
            }
        }

        static XmlElement GetOverdueByID(string strReaderRecord, string strID)
        {
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(strReaderRecord);
            XmlElement overdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + strID + "']") as XmlElement;
            if (overdue == null)
                return null;    // not found
            return overdue;
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            StringBuilder debugInfo = new StringBuilder();

            this.ReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");

            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            // 建立交费记录
            int i = 0;
            if (strAction == "amerce"
                || strAction == "undo") // 2016/12/6 增加
            {
                XmlNodeList records = dom.DocumentElement.SelectNodes("amerceRecord");
#if NO
            if (record != null)
            {
                this.AmerceRecPath = DomUtil.GetAttr(record,
                    "recPath");
                string strRecord = record.InnerText;
                XmlDocument reader_dom = new XmlDocument();
                try
                {
                    reader_dom.LoadXml(strRecord);
                    this.ReaderBarcode = DomUtil.GetElementText(reader_dom.DocumentElement,
                        "readerBarcode");
                    this.ItemBarcode = DomUtil.GetElementText(reader_dom.DocumentElement,
                        "itemBarcode");

                    string strPrice = DomUtil.GetElementText(reader_dom.DocumentElement,
                        "price");
                    long value = 0;
                    string strUnit = "";
                    nRet = ParsePriceString(strPrice,
            out value,
            out strUnit,
            out strError);
                    if (nRet == -1)
                    {
                        this.Unit = "";
                        this.Price = 0;
                    }
                    else
                    {
                        this.Unit = strUnit;
                        this.Price = value;
                    }

                    this.Reason = DomUtil.GetElementText(reader_dom.DocumentElement,
                        "reason");
                }
                catch
                {
                }
            }
#endif
                foreach (XmlElement record in records)
                {
                    if (i == 0)
                        FillRecord(strAction, record, this, debugInfo);
                    else
                    {
                        if (lines == null)
                            lines = new List<OperLogLineBase>();
                        AmerceOperLogLine line = new AmerceOperLogLine();
                        (this as OperLogLineBase).CopyTo(line);
                        line.SubNo = i;
                        FillRecord(strAction, record, line, debugInfo);
                        lines.Add(line);
                    }

                    i++;
                }
            }

            // 建立价格变更记录
            {
                // modifyprice 动作，并没有对应的 amerceRecord 元素，因为尚未交费，只是修改了金额
                // 所以需要选出全部 amerceItem 元素
                XmlNodeList temp_items = dom.DocumentElement.SelectNodes("amerceItems/amerceItem");
                List<XmlElement> items = new List<XmlElement>();
                foreach (XmlElement item in temp_items)
                {
                    if (item.GetAttributeNode("newPrice") != null)
                        items.Add(item);
                    else
                    {
                        if (strAction == "modifyprice")
                        {
                            strError = "action 为 modifyprice 的日志记录中，出现了 amerceItem 元素缺乏 newPrice 属性的情况，格式错误";
                            return -1;
                        }
                        continue;   // action 为 amerce 则有可能并不修改金额
                    }
                }

                if (items.Count > 0)
                {
                    string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
                    string strOldRecord = DomUtil.GetElementText(dom.DocumentElement, "oldReaderRecord");
                    if (string.IsNullOrEmpty(strOldRecord))
                    {
                        // strError = "amerce 类型的日志记录要求具备 oldReaderRecord 元素文本内容，需要用详细级获取日志信息";
                        // return -1;
                        strError = "ReportForm SetData(): amerce 类型的日志记录要求具备 oldReaderRecord 元素文本内容，此日志记录并不具备(可能属于早期的不完备的日志记录)。因此无法计算修改金额的差值。strDate=" + strDate + ", lIndex=" + lIndex;
                        Program.MainForm.WriteErrorLog(strError);
                    }
                    else
                    {
                        foreach (XmlElement item in items)
                        {
                            string strID = item.GetAttribute("id");
                            string strNewPrice = null;
                            if (item.GetAttributeNode("newPrice") != null)
                                strNewPrice = item.GetAttribute("newPrice");
                            else
                            {
                                if (strAction == "modifyprice")
                                {
                                    strError = "action 为 modifyprice 的日志记录中，出现了 amerceItem 元素缺乏 newPrice 属性的情况，格式错误";
                                    return -1;
                                }
                                continue;   // action 为 amerce 则有可能并不修改金额
                            }

                            // oldPrice 需要从 oldReaderRecord 元素中获得
                            XmlElement overdue = GetOverdueByID(strOldRecord, strID);
                            if (overdue == null)
                            {
                                strError = "日志记录格式错误: 根据id '" + strID + "' 在日志记录<oldReaderRecord>元素内没有找到对应的<overdue>元素";
                                return -1;
                            }

                            if (i == 0)
                                FillRecordByOverdue(overdue,
                        strReaderBarcode,
                        strNewPrice,
                        this,
                        debugInfo);
                            else
                            {
                                if (lines == null)
                                    lines = new List<OperLogLineBase>();
                                AmerceOperLogLine line = new AmerceOperLogLine();
                                (this as OperLogLineBase).CopyTo(line);
                                line.SubNo = i;
                                FillRecordByOverdue(overdue,
                        strReaderBarcode,
                        strNewPrice,
                        line,
                        debugInfo);
                                lines.Add(line);
                            }

                            i++;
                        }
                    }
                }
            }

            // 2016/12/6
            if (debugInfo.Length > 0)
                strError = debugInfo.ToString();
            return 0;
        }

        // 2016/5/20
        // 根据读者记录中 overdue 内容填充 line 的各个成员
        static void FillRecordByOverdue(XmlElement overdue,
            string strReaderBarcode,
            string strNewPrice,
            AmerceOperLogLine line,
            StringBuilder debugInfo)
        {
            if (overdue == null)
                return;

            string strError = "";
            line.AmerceRecPath = "";
            line.Action = "modifyprice";
            try
            {
                line.ReaderBarcode = strReaderBarcode;
                line.ItemBarcode = overdue.GetAttribute("barcode");
                line.ID = overdue.GetAttribute("id");   // 2016/12/6

                // 变化的金额
                string strOldPrice = overdue.GetAttribute("price");
                List<string> prices = new List<string>();
                if (string.IsNullOrEmpty(strNewPrice) == false)
                    prices.Add(strNewPrice);
                if (string.IsNullOrEmpty(strOldPrice) == false)
                    prices.Add("-" + strOldPrice);

                string strResult = "";
                int nRet = PriceUtil.TotalPrice(prices,
        out strResult,
        out strError);
                if (nRet == -1)
                {
                    // return -1;
                    if (debugInfo != null)
                        debugInfo.Append("FillRecordByOverdue() TotalPrice() 解析金额字符串 '" + StringUtil.MakePathList(prices) + "' 时出错(已被当作 0 处理): " + strError + "\r\n");
                    return;
                }

                long value = 0;
                string strUnit = "";
                nRet = ParsePriceString(strResult,
        out value,
        out strUnit,
        out strError);
                if (nRet == -1)
                {
                    if (debugInfo != null)
                        debugInfo.Append("FillRecordByOverdue() 解析金额字符串 '" + strResult + "' 时出错(已被当作 0 处理): " + strError + "\r\n");

                    line.Unit = "";
                    line.Price = 0;
                }
                else
                {
                    line.Unit = strUnit;
                    line.Price = value;
                }

                line.Reason = overdue.GetAttribute("reason");
            }
            catch (Exception ex)
            {
                if (debugInfo != null)
                    debugInfo.Append("FillRecordByOverdue() 出现异常: " + ExceptionUtil.GetExceptionText(ex) + "\r\n");
            }
        }

        // 根据 amerceRecord 元素内容填充 line 的各个成员
        // parameters:
        //      strAction   amerce / undo
        static void FillRecord(
            string strAction,
            XmlElement record,
            AmerceOperLogLine line,
            StringBuilder debugInfo)
        {
            if (record == null)
                return;

            string strError = "";
            line.AmerceRecPath = DomUtil.GetAttr(record,
                "recPath");
            line.Action = strAction;    //  "amerce"; 2016/12/6 修改为 strAction
            string strRecord = record.InnerText;
            XmlDocument amerce_dom = new XmlDocument();
            try
            {
                amerce_dom.LoadXml(strRecord);
                line.ReaderBarcode = DomUtil.GetElementText(amerce_dom.DocumentElement,
                    "readerBarcode");
                line.ItemBarcode = DomUtil.GetElementText(amerce_dom.DocumentElement,
                    "itemBarcode");
                line.ID = DomUtil.GetElementText(amerce_dom.DocumentElement,
                    "id");

                string strPrice = DomUtil.GetElementText(amerce_dom.DocumentElement,
                    "price");
                long value = 0;
                string strUnit = "";
                int nRet = ParsePriceString(strPrice,
        out value,
        out strUnit,
        out strError);
                if (nRet == -1)
                {
                    if (debugInfo != null)
                        debugInfo.Append("FillRecord() 解析金额字符串 '" + strPrice + "' 时出错(已被当作 0 处理): " + strError + "\r\n");

                    line.Unit = "";
                    line.Price = 0;
                }
                else
                {
                    line.Unit = strUnit;
                    line.Price = value;
                }

                line.Reason = DomUtil.GetElementText(amerce_dom.DocumentElement,
                    "reason");
            }
            catch (Exception ex)
            {
                if (debugInfo != null)
                    debugInfo.Append("FillRecord() 出现异常: " + ExceptionUtil.GetExceptionText(ex) + "\r\n");
            }
        }

        // 按照时间单位,把时间值零头去除,正规化,便于后面计算差额
        /// <summary>
        /// 按照时间基本单位，去掉零头，便于互相计算(整单位的)差额。
        /// </summary>
        /// <param name="strUnit">时间单位。day/hour之一。如果为空，相当于 day</param>
        /// <param name="time">要处理的时间。为 GMT 时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int RoundTime(string strUnit,
            ref DateTime time,
            out string strError)
        {
            strError = "";

            // 算法是先转换为本地时间，去掉零头，再转换回 GMT 时间
            // time = time.ToLocalTime();
            if (strUnit == "day" || string.IsNullOrEmpty(strUnit) == true)
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }
            // time = time.ToUniversalTime();

            return 0;
        }

        // parameters:
        //      strBorrowTime   借阅起点时间。u 格式
        //      strReturningTime    返回应还时间。 u 格式
        internal static int BuildReturingTimeString(string strBorrowTime,
            string strBorrowPeriod,
            out string strReturningTime,
            out string strError)
        {
            strError = "";
            strReturningTime = "";

            if (string.IsNullOrEmpty(strBorrowTime) == true)
                return 0;

            long lValue = 0;
            string strUnit = "";
            // 分析期限参数
            int nRet = StringUtil.ParsePeriodUnit(strBorrowPeriod,
                out lValue,
                out strUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "期限字符串 '" + strBorrowPeriod + "' 格式不合法: " + strError;
                return -1;
            }

            DateTime borrowdate;

#if NO
            try
            {
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowTime);
            }
            catch
            {
                strError = "借阅日期值 '"+strBorrowTime+"' 格式错误";
                return -1;
            }
#endif
            if (DateTime.TryParse(strBorrowTime,
out borrowdate) == false)
            {
                strError = "借阅日期字符串 '" + strBorrowTime + "' 无法解析";
                return -1;
            }

            // 正规化时间
            nRet = RoundTime(strUnit,
                ref borrowdate,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta;

            if (strUnit == "day")
                delta = new TimeSpan((int)lValue, 0, 0, 0);
            else if (strUnit == "hour")
                delta = new TimeSpan((int)lValue, 0, 0);
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            DateTime timeEnd = borrowdate + delta;

            // 正规化时间
            nRet = RoundTime(strUnit,
                ref timeEnd,
                out strError);
            if (nRet == -1)
                return -1;

            strReturningTime = timeEnd.ToString("s");

            return 0;
        }

        internal static int ParsePriceString(string strPrice,
            out long value,
            out string strUnit,
            out string strError)
        {
            value = 0;
            strUnit = "";
            strError = "";

            if (string.IsNullOrEmpty(strPrice) == true)
                return 0;

#if NO

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";

            // 分析价格参数
            // 允许前面出现+ -号
            // return:
            //      -1  出错
            //      0   成功
            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;
            strUnit = strPrefix + strPostfix;
            decimal v = 0;
            if (decimal.TryParse(strValue, out v) == false)
            {
                strError = "金额字符串 '" + strPrice + "' 中数字部分 '" + strValue + "' 格式不正确";
                return -1;
            }
#endif
            CurrencyItem item = null;
            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out item,
                out strError);
            if (nRet == -1)
                return -1;

            strUnit = item.Prefix + item.Postfix;
            try
            {
                value = (long)(item.Value * 100);
            }
            catch (Exception ex)
            {
                // 2016/3/31
                strError = "元值 '" + item.Value.ToString() + "' 折算为分值的时候出现异常：" + ex.Message;
                return -1;
            }
            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
            int i,
            bool bInsertOrReplace,
            StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, amercerecpath, readerbarcode, itembarcode, price, unit, reason, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i

+ ", @amercerecpath" + i
+ ", @readerbarcode" + i
+ ", @itembarcode" + i
+ ", @price" + i
+ ", @unit" + i
+ ", @reason" + i

+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
"@subno" + i,
this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
"@librarycode" + i,
this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@amercerecpath" + i,
                this.AmerceRecPath);

            SQLiteUtil.SetParameter(command,
"@readerbarcode" + i,
this.ReaderBarcode);

            SQLiteUtil.SetParameter(command,
"@itembarcode" + i,
this.ItemBarcode);

            SQLiteUtil.SetParameter(command,
"@price" + i,
this.Price);
            SQLiteUtil.SetParameter(command,
"@unit" + i,
this.Unit);

            SQLiteUtil.SetParameter(command,
"@reason" + i,
this.Reason);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);

        }
    }

    // 流通操作 每行
    class CircuOperLogLine : OperLogLineBase
    {
        // 册条码号
        public string ItemBarcode = "";

        // 读者证条码号
        public string ReaderBarcode = "";

        static string TableName
        {
            get
            {
                return "operlogcircu";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");

            this.ItemBarcode = strItemBarcode;
            this.ReaderBarcode = strReaderBarcode;

            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
    int i,
    bool bInsertOrReplace,
    StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, itembarcode, readerbarcode, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @itembarcode" + i
+ ", @readerbarcode" + i
+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
"@subno" + i,
this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
"@librarycode" + i,
this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@itembarcode" + i,
                this.ItemBarcode);

            SQLiteUtil.SetParameter(command,
"@readerbarcode" + i,
this.ReaderBarcode);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);
        }
    }

    // 获取对象操作 每行
    class GetResOperLogLine : OperLogLineBase
    {
        // 对象 ID
        public string ObjectID = "";

        // 元数据记录路径
        public string XmlRecPath = "";

        public string Size = "";
        public string Mime = "";

        static string TableName
        {
            get
            {
                return "operloggetres";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            string strResPath = DomUtil.GetElementText(dom.DocumentElement,
                "path");
            string strSize = DomUtil.GetElementText(dom.DocumentElement,
                "size");
            string strMime = DomUtil.GetElementText(dom.DocumentElement,
                "mime");

            string strXmlRecPath = "";
            string strObjectID = "";
            // 解析对象路径
            // parameters:
            //      strPathParam    等待解析的路径
            //      strXmlRecPath   返回元数据记录路径
            //      strObjectID     返回对象 ID
            // return:
            //      false   不是记录路径
            //      true    是记录路径
            StringUtil.ParseObjectPath(strResPath,
            out strXmlRecPath,
            out strObjectID);

            this.XmlRecPath = strXmlRecPath;
            this.ObjectID = strObjectID;
            this.Size = strSize;
            this.Mime = strMime;
            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
    int i,
    bool bInsertOrReplace,
    StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, xmlrecpath, objectid, size, mime, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @xmlrecpath" + i
+ ", @objectid" + i
+ ", @size" + i
+ ", @mime" + i
+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
                "@subno" + i,
                this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
"@librarycode" + i,
this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@xmlrecpath" + i,
                this.XmlRecPath);

            SQLiteUtil.SetParameter(command,
                "@objectid" + i,
                this.ObjectID);

            SQLiteUtil.SetParameter(command,
    "@size" + i,
    this.Size);
            SQLiteUtil.SetParameter(command,
                "@mime" + i,
                this.Mime);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);
        }
    }

    // 入馆登记 每行
    class PassGateOperLogLine : OperLogLineBase
    {
        // 馆代码
        public string LibraryCode = "";

        // 读者证条码号
        public string ReaderBarcode = "";

        // 门名称
        public string GateName = "";

        static string TableName
        {
            get
            {
                return "operlogpassgate";
            }
        }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperLogLineBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;

            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement,
    "libraryCode");

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");

            string strGateName = DomUtil.GetElementText(dom.DocumentElement,
    "gateName");

            this.LibraryCode = strLibraryCode;  // 这里和基础类的 LibraryCode 什么关系?
            this.ReaderBarcode = strReaderBarcode;
            this.GateName = strGateName;
            return 0;
        }

        public override void BuildWriteCommand(SQLiteCommand command,
    int i,
    bool bInsertOrReplace,
    StringBuilder text)
        {
            if (bInsertOrReplace == true)
                text.Append(" INSERT OR REPLACE ");
            else
                text.Append(" INSERT ");

            text.Append(
" INTO " + TableName + " (date, no, subno, librarycode, operation, action, gatename, readerbarcode, operator, opertime) "
+ " VALUES("
+ "@date" + i
+ ", @no" + i
+ ", @subno" + i
+ ", @librarycode" + i
+ ", @operation" + i
+ ", @action" + i
+ ", @gatename" + i
+ ", @readerbarcode" + i
+ ", @operator" + i
+ ", @opertime" + i + ")"
+ " ; ");
            SQLiteUtil.SetParameter(command,
                "@date" + i,
                this.Date);
            SQLiteUtil.SetParameter(command,
                "@no" + i,
                this.No.ToString());
            SQLiteUtil.SetParameter(command,
                "@subno" + i,
                this.SubNo.ToString());
            SQLiteUtil.SetParameter(command,
"@librarycode" + i,
this.LibraryCode);
            SQLiteUtil.SetParameter(command,
                "@operation" + i,
                this.Operation);
            SQLiteUtil.SetParameter(command,
                "@action" + i,
                this.Action);

            SQLiteUtil.SetParameter(command,
                "@gatename" + i,
                this.GateName);

            SQLiteUtil.SetParameter(command,
                "@readerbarcode" + i,
                this.ReaderBarcode);

            SQLiteUtil.SetParameter(command,
"@operator" + i,
this.Operator);
            SQLiteUtil.SetParameter(command,
"@opertime" + i,
this.OperTime);
        }
    }

#if NO
    class OperLogLine
    {
        public string Date = "";  // 所在日志文件日期，8 字符
        public long No = 0;
        public string Operation = "";
        public string Action = "";
        public string OperTime = "";

        // 册条码号
        public string ItemBarcode = "";
        // public string ItemRecPath = "";

        // 馆藏地点
        // public string ItemLocation = "";

        // 读者证条码号
        public string ReaderBarcode = "";
        // 馆代码
        // public string LibraryCode = "";



        // 创建日志表
        public static int CreateOperLogTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists operlog ;\n"
                + "CREATE TABLE operlog "
                + "(" + " "
                + "date nvarchar (255) NULL," + " "
                + "no integer ," + " "
                + "operation nvarchar (255) NULL," + " "
                + "action nvarchar (255) NULL ," + " "
                + "itembarcode nvarchar (255) NULL ," + " "
                // + "location nvarchar (255) NULL ," + " "
                + "readerbarcode nvarchar (255) NULL ," + " "
                // + "librarycode nvarchar (255) NULL ," + " "
                + "opertime nvarchar (255) NULL  "
                + ") ; ";

            strCommand += " CREATE UNIQUE INDEX operlog_id_index \n"
    + " ON operlog (date, no) ;\n";


            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        // 创建报表阶段需要的附加索引
        public static int CreateAdditionalIndex(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 
            string strCommand = "CREATE INDEX IF NOT EXISTS operlog_itembarcode_index \n"
    + " ON operlog (itembarcode); \n"
    + " CREATE INDEX IF NOT EXISTS operlog_readerbarcode_index "
    + " ON operlog (readerbarcode); \n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "创建索引出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }

        // 删除创建报表阶段才需要的附加索引
        public static int DeleteAdditionalIndex(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 
            string strCommand = "DROP INDEX IF EXISTS operlog_itembarcode_index;  "
    + " DROP INDEX IF EXISTS operlog_readerbarcode_index;";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "删除索引出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }

        //  XML 记录变换为 SQL 记录
        public static int Xml2Line(XmlDocument dom,
            string strDate,
            long lIndex,
            out OperLogLine line,
            out string strError)
        {
            strError = "";
            line = null;

            if (string.IsNullOrEmpty(strDate) == true
                || strDate.Length != 8)
            {
                strError = "strDate 的值 '" + strDate + "' 格式错误，应该为 8 字符的数字";
                return -1;
            }

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");
            string strOperTime = DomUtil.GetElementText(dom.DocumentElement,
                "operTime");

            line = new OperLogLine();
            Debug.Assert(strDate.Length == 8, "");
            line.Date = strDate;
            line.No = lIndex;
            line.Operation = strOperation;
            line.Action = strAction;
            line.ItemBarcode = strItemBarcode;
            // 馆藏地点需要另行获得
            line.ReaderBarcode = strReaderBarcode;
            line.OperTime = SQLiteUtil.GetLocalTime(strOperTime);
            // line.LibraryCode = strLibraryCode;

            return 0;
        }

        // 插入一批日志记录
        public static int AppendOperLogLines(
            SQLiteConnection connection,
            List<OperLogLine> lines,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";

            if (lines.Count == 0)
                return 0;

            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {

                StringBuilder text = new StringBuilder(4096);
                int i = 0;
                foreach (OperLogLine line in lines)
                {
                    if (bInsertOrReplace == true)
                        text.Append(" INSERT OR REPLACE ");
                    else
                        text.Append(" INSERT ");

                    text.Append(
    " INTO operlog (date, no, subno, operation, action, itembarcode, readerbarcode, opertime) "
    + " VALUES("
    + "@date" + i
    + ", @no" + i
+ ", @subno" + i
    + ", @operation" + i
    + ", @action" + i
    + ", @itembarcode" + i
                        // + ", @location" + i
    + ", @readerbarcode" + i
                        // + ", @librarycode" + i
    + ", @opertime" + i + ")"
    + " ; ");
                    SQLiteUtil.SetParameter(command,
                        "@date" + i,
                        line.Date);
                    SQLiteUtil.SetParameter(command,
                        "@no" + i,
                        line.No.ToString());
                    SQLiteUtil.SetParameter(command,
"@subno" + i,
line.SubNo.ToString());
                    SQLiteUtil.SetParameter(command,
                        "@operation" + i,
                        line.Operation);
                    SQLiteUtil.SetParameter(command,
                        "@action" + i,
                        line.Action);

                    SQLiteUtil.SetParameter(command,
                        "@itembarcode" + i,
                        line.ItemBarcode);

#if NO
                    SQLiteUtil.SetParameter(command,
     "@location" + i,
     line.ItemLocation);
#endif

                    SQLiteUtil.SetParameter(command,
"@readerbarcode" + i,
line.ReaderBarcode);

#if NO
                    SQLiteUtil.SetParameter(command,
     "@librarycode" + i,
     line.LibraryCode);
#endif

                    SQLiteUtil.SetParameter(command,
     "@opertime" + i,
     line.OperTime);

                    i++;
                }

                IDbTransaction trans = null;

                trans = connection.BeginTransaction();
                try
                {
                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
                }
                finally
                {
                    if (trans != null)
                        trans.Rollback();
                }
            }

            return 0;
        }
    }

#endif

    class ItemLine
    {
        public string ItemRecPath = "";
        public string ItemBarcode = "";
        public string Location = "";
        public string AccessNo = "";
        public string BiblioRecPath = "";

        // 2014/4/7
        public string CreateTime = "";
        public string State = "";

        // 2014/6/12
        public long Price = 0;
        public string Unit = "";

        // 2014/5/29
        // public bool Full = true;    // 是否包含了全部字段信息。 == true 表示全部; == false 表示只包含了 borrower 等字段信息
        public int Level = 0;   // 包含了何种字段信息？ 
        // 0 全部字段;
        // 1 brrower borrowtime borrowperiod returningtime itemrecpath 字段; 
        // 2 borrower itemrecpath 字段

        public string Borrower = "";
        public string BorrowTime = "";
        public string BorrowPeriod = "";
        public string ReturningTime = "";   // 预计还回时间

        static string[] all_fields = {
                "itemrecpath",
                "itembarcode",
                "location",
                "accessno",
                "state",
                "createtime",
                "price",
                "unit",
                "borrower",
                "borrowtime",
                "borrowperiod",
                "returningtime",
                "bibliorecpath"
                                      };
        static string[] borrow_fields = {
                // "itemrecpath",
                "itembarcode",
                "borrower",
                "borrowtime",
                "borrowperiod",
                "returningtime",
                                      };
        static string[] borrower_fields = {
                // "itemrecpath",
                "itembarcode",
                "borrower",
                                      };

        // 创建册记录表
        public static int CreateItemTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists item ;\n"
                + "CREATE TABLE item "
                + "(" + " "
                + "itemrecpath nvarchar (255) NULL UNIQUE," + " "
                + "itembarcode nvarchar (255) NULL ," + " "
                + "location nvarchar (255) NULL ," + " "
                + "accessno nvarchar (255) NULL ,"
                + "state nvarchar (255) NULL ,"
                + "createtime nvarchar (255) NULL ,"

                + "price integer NULL ,"
                + "unit nvarchar (255) NULL ,"

                + "borrower nvarchar (255) NULL ,"
                + "borrowtime nvarchar (255) NULL ,"
                + "borrowperiod nvarchar (255) NULL ,"
                + "returningtime nvarchar (255) NULL ,"

                + "bibliorecpath nvarchar (255) NULL" + " "
                + ") ; \n";
            strCommand += " CREATE INDEX item_barcode_index \n"
+ " ON item (itembarcode) ;\n";
            strCommand += " CREATE INDEX item_itemrecpath_index \n"
+ " ON item (itemrecpath) ;\n";
            strCommand += " CREATE INDEX item_biliorecpath_index \n"
+ " ON item (bibliorecpath) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        static int BuildInsertOrReplaceCommand(
            string strTableName,
            List<string> all_fields,
            int param_index,
            ref StringBuilder text,
            out string strError)
        {
            strError = "";

            if (text == null)
                text = new StringBuilder(4096);

            text.Append("INSERT OR REPLACE into " + strTableName + " (");
            int i = 0;
            foreach (string s in all_fields)
            {
                if (i > 0)
                    text.Append(",");
                text.Append(s);
                i++;
            }
            text.Append(") VALUES ( ");

            List<string> old_fields = new List<string>();
            i = 0;
            foreach (string s in all_fields)
            {
                if (i > 0)
                    text.Append(",");
                text.Append("@" + s + param_index.ToString());
                i++;
            }
            text.Append(" ) ;");
            return 0;
        }

        static int BuildInsertOrReplaceCommand(
            string strTableName,
            string strKeyFieldName,
            List<string> new_fields,
            List<string> all_fields,
            int param_index,
            ref StringBuilder text,
            out string strError)
        {
            strError = "";

            if (text == null)
                text = new StringBuilder(4096);

            text.Append("INSERT OR REPLACE INTO " + strTableName + " (");
            int i = 0;
            foreach (string s in all_fields)
            {
                if (i > 0)
                    text.Append(",");
                text.Append(s);
                i++;
            }
            text.Append(") SELECT ");

            List<string> old_fields = new List<string>();
            i = 0;
            foreach (string s in all_fields)
            {
                if (i > 0)
                    text.Append(",");
                if (new_fields.IndexOf(s) == -1)
                {
                    text.Append("old.");
                    old_fields.Add(s);
                }
                else
                    text.Append("new.");
                text.Append(s);
                i++;
            }

            if (old_fields.IndexOf(strKeyFieldName) == -1)
                old_fields.Add(strKeyFieldName);

            text.Append(" FROM ( SELECT ");
            i = 0;
            foreach (string s in new_fields)
            {
                if (i > 0)
                    text.Append(",");
                text.Append("@" + s + param_index.ToString() + " AS " + s);
                i++;
            }
            text.Append(") AS new LEFT JOIN ( SELECT ");
            i = 0;
            foreach (string s in old_fields)
            {
                if (i > 0)
                    text.Append(",");
                text.Append(s);
                i++;
            }
            text.Append(" FROM " + strTableName);
            text.Append(" WHERE " + strKeyFieldName + " = @" + strKeyFieldName + param_index.ToString() + " ) ");
            text.Append(" AS old ON old." + strKeyFieldName + " = new." + strKeyFieldName + "; ");
            return 0;
        }

        // 插入一批册记录
        public static int AppendItemLines(
            SQLiteConnection connection,
            List<ItemLine> lines,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (lines.Count == 0)
                return 0;

            Debug.WriteLine("AppendItemLines() lines.Count=" + lines.Count);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {
                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (ItemLine line in lines)
                    {
                        if (line.Level == 0)
                        {
                            // 替换全部字段
                            nRet = BuildInsertOrReplaceCommand(
        "item",
        new List<string>(all_fields),
        i,
        ref text,
        out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else if (line.Level == 1)
                        {
                            // 替换部分字段
                            nRet = BuildInsertOrReplaceCommand(
                "item",
                "itembarcode",
                new List<string>(borrow_fields),
                new List<string>(all_fields),
                i,
                ref text,
                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(line.ItemBarcode) == false, "ItemBarcode 不能为空");
                        }
                        else if (line.Level == 2)
                        {
                            // 替换部分字段
                            nRet = BuildInsertOrReplaceCommand(
                "item",
                "itembarcode",
                new List<string>(borrower_fields),
                new List<string>(all_fields),
                i,
                ref text,
                out strError);
                            if (nRet == -1)
                                return -1;

                            Debug.Assert(string.IsNullOrEmpty(line.ItemBarcode) == false, "ItemBarcode 不能为空");
                        }

                        SQLiteUtil.SetParameter(command,
                            "@itemrecpath" + i,
                            line.ItemRecPath);
                        SQLiteUtil.SetParameter(command,
         "@itembarcode" + i,
         line.ItemBarcode);

                        SQLiteUtil.SetParameter(command,
         "@location" + i,
         line.Location);

                        SQLiteUtil.SetParameter(command,
    "@accessno" + i,
    line.AccessNo);

                        SQLiteUtil.SetParameter(command,
    "@state" + i,
    line.State);

#if NO
                    if (string.IsNullOrEmpty(line.CreateTime) == false)
                    {
                        SQLiteUtil.SetParameter(command,
    "@createtime" + i,
    line.CreateTime);
                    }
#endif
                        SQLiteUtil.SetParameter(command,
    "@createtime" + i,
    line.CreateTime);

                        SQLiteUtil.SetParameter(command,
    "@price" + i,
    line.Price);

                        SQLiteUtil.SetParameter(command,
    "@unit" + i,
    line.Unit);

                        SQLiteUtil.SetParameter(command,
    "@borrower" + i,
    line.Borrower);
                        SQLiteUtil.SetParameter(command,
    "@borrowtime" + i,
    line.BorrowTime);
                        SQLiteUtil.SetParameter(command,
    "@borrowperiod" + i,
    line.BorrowPeriod);
                        SQLiteUtil.SetParameter(command,
    "@returningtime" + i,
    line.ReturningTime);

                        SQLiteUtil.SetParameter(command,
         "@bibliorecpath" + i,
         line.BiblioRecPath);

                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }

            return 0;
        }

#if NO
        // 插入一批册记录
        public static int AppendItemLines(
            SQLiteConnection connection,
            List<ItemLine> lines,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (lines.Count == 0)
                return 0;

            Debug.WriteLine("AppendItemLines() lines.Count=" + lines.Count);

            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {
                StringBuilder text = new StringBuilder(4096);
                int i = 0;
                foreach (ItemLine line in lines)
                {
#if NO
                    // TODO: 难点在于如果记录存在，就不要覆盖以前的 createtime 值
                    if (bInsertOrReplace == true)
                        text.Append(" INSERT OR REPLACE ");
                    else
                        text.Append(" INSERT ");

                    if (line.Full == true)
                    {
                        text.Append(
        " INTO item (itemrecpath, itembarcode, location, accessno, state, "
        + (string.IsNullOrEmpty(line.CreateTime) == false ? "createtime," : "")
        + " price, unit, "
        + " borrower,borrowtime,borrowperiod,returningtime,bibliorecpath) "
        + " VALUES(@itemrecpath" + i
        + ", @itembarcode" + i
        + ", @location" + i
        + ", @accessno" + i
        + ", @state" + i

        + (string.IsNullOrEmpty(line.CreateTime) == false ? (", @createtime" + i) : "")

        + ", @price" + i
        + ", @unit" + i

        + ", @borrower" + i
        + ", @borrowtime" + i
        + ", @borrowperiod" + i
        + ", @returningtime" + i

        + ", @bibliorecpath" + i + ")"
        + " ; ");
                    }
                    else
                    {
                        text.Append(
" INTO item (itemrecpath, itembarcode, "
        + " borrower,borrowtime,borrowperiod,returningtime) "

+ " VALUES(@itemrecpath" + i
+ ", @itembarcode" + i
+ ", @borrower" + i
+ ", @borrowtime" + i
+ ", @borrowperiod" + i
+ ", @returningtime" + i + ")"
+ " ; ");

                    }
#endif

                    if (line.Level == 0)
                    {
                        // 替换全部字段
                        nRet = BuildInsertOrReplaceCommand(
    "item",
    new List<string>(all_fields),
    i,
    ref text,
    out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else if (line.Level == 1)
                    {
                        // 替换部分字段
                        nRet = BuildInsertOrReplaceCommand(
            "item",
            "itembarcode",
            new List<string>(borrow_fields),
            new List<string>(all_fields),
            i,
            ref text,
            out strError);
                        if (nRet == -1)
                            return -1;

                        Debug.Assert(string.IsNullOrEmpty(line.ItemBarcode) == false, "ItemBarcode 不能为空");
                    }
                    else if (line.Level == 2)
                    {
                        // 替换部分字段
                        nRet = BuildInsertOrReplaceCommand(
            "item",
            "itembarcode",
            new List<string>(borrower_fields),
            new List<string>(all_fields),
            i,
            ref text,
            out strError);
                        if (nRet == -1)
                            return -1;

                        Debug.Assert(string.IsNullOrEmpty(line.ItemBarcode) == false, "ItemBarcode 不能为空");
                    }

                    SQLiteUtil.SetParameter(command,
                        "@itemrecpath" + i,
                        line.ItemRecPath);
                    SQLiteUtil.SetParameter(command,
     "@itembarcode" + i,
     line.ItemBarcode);

                    SQLiteUtil.SetParameter(command,
     "@location" + i,
     line.Location);

                    SQLiteUtil.SetParameter(command,
"@accessno" + i,
line.AccessNo);

                    SQLiteUtil.SetParameter(command,
"@state" + i,
line.State);

#if NO
                    if (string.IsNullOrEmpty(line.CreateTime) == false)
                    {
                        SQLiteUtil.SetParameter(command,
    "@createtime" + i,
    line.CreateTime);
                    }
#endif
                    SQLiteUtil.SetParameter(command,
"@createtime" + i,
line.CreateTime);

                    SQLiteUtil.SetParameter(command,
"@price" + i,
line.Price);

                    SQLiteUtil.SetParameter(command,
"@unit" + i,
line.Unit);

                    SQLiteUtil.SetParameter(command,
"@borrower" + i,
line.Borrower);
                    SQLiteUtil.SetParameter(command,
"@borrowtime" + i,
line.BorrowTime);
                    SQLiteUtil.SetParameter(command,
"@borrowperiod" + i,
line.BorrowPeriod);
                    SQLiteUtil.SetParameter(command,
"@returningtime" + i,
line.ReturningTime);

                    SQLiteUtil.SetParameter(command,
     "@bibliorecpath" + i,
     line.BiblioRecPath);

                    i++;
                }

                IDbTransaction trans = null;

                trans = connection.BeginTransaction();
                try
                {
                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
                }
                finally
                {
                    if (trans != null)
                        trans.Rollback();
                }
            }

            return 0;
        }

#endif

        //  XML 记录变换为 SQL 记录
        // parameters:
        //      strLogCreateTime    日志操作记载的创建时间。不是创建动作的其他时间，不要放在这里
        public static int Xml2Line(XmlDocument dom,
            string strItemRecPath,
            string strBiblioRecPath,
            string strLogCreateTime,
            out ItemLine line,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            line = new ItemLine();
            line.ItemRecPath = strItemRecPath;
            line.ItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            // 2016/9/26
            if (string.IsNullOrEmpty(line.ItemBarcode))
            {
                string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                line.ItemBarcode = "@refID:" + strRefID;
            }

            line.Location = StringUtil.GetPureLocationString(
                DomUtil.GetElementText(dom.DocumentElement,
                "location"));    // 要变为纯净的地点信息，即不包含 #reservation 之类
            line.AccessNo = DomUtil.GetElementText(dom.DocumentElement,
                "accessNo");
            line.BiblioRecPath = strBiblioRecPath;
            line.State = DomUtil.GetElementText(dom.DocumentElement,
    "state");

            line.Borrower = DomUtil.GetElementText(dom.DocumentElement,
    "borrower");
            line.BorrowTime = SQLiteUtil.GetLocalTime(DomUtil.GetElementText(dom.DocumentElement,
    "borrowDate"));
            line.BorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
"borrowPeriod");
            // line.ReturningTime = GetLocalTime(DomUtil.GetElementText(dom.DocumentElement, "returningDate"));

            if (string.IsNullOrEmpty(line.BorrowTime) == false)
            {
                string strReturningTime = "";
                // parameters:
                //      strBorrowTime   借阅起点时间。u 格式
                //      strReturningTime    返回应还时间。 u 格式
                nRet = AmerceOperLogLine.BuildReturingTimeString(line.BorrowTime,
    line.BorrowPeriod,
    out strReturningTime,
    out strError);
                if (nRet == -1)
                {
                    line.ReturningTime = "";
                }
                else
                    line.ReturningTime = strReturningTime;
            }
            else
                line.ReturningTime = "";

            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
    "price");
            long value = 0;
            string strUnit = "";
            nRet = AmerceOperLogLine.ParsePriceString(strPrice,
    out value,
    out strUnit,
    out strError);
            if (nRet == -1)
            {
                line.Price = 0;
                line.Unit = "";
            }
            else
            {
                line.Price = value;
                line.Unit = strUnit;
            }

            string strTime = "";
            XmlNode node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
            if (node != null)
            {
                strTime = DomUtil.GetAttr(node, "time");
                try
                {
                    strTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(strTime, "u");
                }
                catch
                {
                }
            }
            if (string.IsNullOrEmpty(strTime) == true)
            {
                // 如果 operations 里面没有信息
                // 采用日志记录的时间
                try
                {
                    strTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(strLogCreateTime, "u");
                }
                catch
                {
                }
            }
            line.CreateTime = strTime;
            return 0;
        }

        public static int DeleteItemLine(
SQLiteConnection connection,
string strItemRecPath,
out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder(4096);
            // int i = 0;
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {
                    SQLiteUtil.SetParameter(command,
    "@itemrecpath",
    strItemRecPath);

                    // 删除 item 记录
                    text.Append("delete from item where itemrecpath = @itemrecpath ;");

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }

            return 0;
        }
    }

    class ReaderLine
    {
        public string ReaderRecPath = "";
        public string ReaderBarcode = "";
        public string LibraryCode = "";
        public string Department = "";
        public string ReaderType = "";
        public string Name = "";
        public string State = "";   // 2014/11/6

        // 创建读者记录表
        public static int CreateReaderTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists reader ;\n"
                + "CREATE TABLE reader "
                + "(" + " "
                + "readerrecpath nvarchar (255) NULL UNIQUE," + " "
                + "readerbarcode nvarchar (255) NULL ," + " "
                + "librarycode nvarchar (255) NULL ," + " "
                + "department nvarchar (255) NULL ,"
                + "readertype nvarchar (255) NULL ," + " "
                + "state nvarchar (255) NULL ," + " "
                + "name nvarchar (255) NULL" + " "
                + ") ; \n";
            strCommand += " CREATE INDEX reader_barcode_index \n"
+ " ON reader (readerbarcode) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        //  XML 记录变换为 SQL 记录
        public static int Xml2Line(XmlDocument dom,
            string strReaderRecPath,
            string strLibraryCode,
            out ReaderLine line,
            out string strError)
        {
            strError = "";

            line = new ReaderLine();

            line.ReaderRecPath = strReaderRecPath;
            line.ReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            line.LibraryCode = strLibraryCode;

            line.Department = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            line.ReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            line.Name = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            line.State = DomUtil.GetElementText(dom.DocumentElement,
    "state");
            return 0;
        }

        // 插入一批读者记录
        public static int AppendReaderLines(
            SQLiteConnection connection,
            List<ReaderLine> lines,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";

            if (lines.Count == 0)
                return 0;

            Debug.WriteLine("AppendReaderLines() lines.Count=" + lines.Count);
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {

                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (ReaderLine line in lines)
                    {
                        if (bInsertOrReplace == true)
                            text.Append(" INSERT OR REPLACE ");
                        else
                            text.Append(" INSERT ");

                        text.Append(
        " INTO reader (readerrecpath, readerbarcode, librarycode, department, readertype, state, name) "
        + " VALUES(@readerrecpath" + i
        + ", @readerbarcode" + i
        + ", @librarycode" + i
        + ", @department" + i
        + ", @readertype" + i
        + ", @state" + i
        + ", @name" + i + ")"
        + " ; ");
                        SQLiteUtil.SetParameter(command,
                            "@readerrecpath" + i,
                            line.ReaderRecPath);
                        SQLiteUtil.SetParameter(command,
                            "@readerbarcode" + i,
                            line.ReaderBarcode);

                        SQLiteUtil.SetParameter(command,
                            "@librarycode" + i,
                            line.LibraryCode);

                        SQLiteUtil.SetParameter(command,
                            "@department" + i,
                            line.Department);

                        SQLiteUtil.SetParameter(command,
                            "@readertype" + i,
                            line.ReaderType);

                        SQLiteUtil.SetParameter(command,
        "@state" + i,
        line.State);

                        SQLiteUtil.SetParameter(command,
                            "@name" + i,
                            line.Name);

                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                }
                mytransaction.Commit();
            }
            return 0;
        }

        public static int DeleteReaderLine(
SQLiteConnection connection,
string strReaderRecPath,
out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder(4096);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                //int i = 0;
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {
                    SQLiteUtil.SetParameter(command,
    "@readerrecpath",
    strReaderRecPath);

                    // 删除 item 记录
                    text.Append("delete from reader where readerrecpath = @readerrecpath ;");

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }
            return 0;
        }
    }

    class BiblioLine
    {
        public string BiblioRecPath = "";
        public string Summary = "";
        public string Title = "";
        public string Author = "";

        // 创建书目记录表
        public static int CreateBiblioTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists biblio ;\n"
                + "CREATE TABLE biblio "
                + "(" + " "
                + "bibliorecpath nvarchar (255) NULL UNIQUE," + " "
                + "summary nvarchar (4000) NULL" + " "
                + ") ; \n";
            strCommand += " CREATE INDEX biblio_recpath_index \n"
+ " ON biblio (bibliorecpath) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        // 插入一批书目记录
        public static int AppendBiblioLines(
            SQLiteConnection connection,
            List<BiblioLine> lines,
            bool bInsertOrReplace,
            out string strError)
        {
            strError = "";

            if (lines.Count == 0)
                return 0;

            Debug.WriteLine("AppendBiblioLines() lines.Count=" + lines.Count);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {

                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (BiblioLine line in lines)
                    {
                        if (bInsertOrReplace == true)
                            text.Append(" INSERT OR REPLACE ");
                        else
                            text.Append(" INSERT ");

                        text.Append(
        " INTO biblio (bibliorecpath, summary) "
        + " VALUES(@bibliorecpath" + i
        + ", @summary" + i + ")"
        + " ; ");
                        SQLiteUtil.SetParameter(command,
                            "@bibliorecpath" + i,
                            line.BiblioRecPath);
                        SQLiteUtil.SetParameter(command,
         "@summary" + i,
         line.Summary);
                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }

            return 0;
        }

    }

    class ClassLine
    {
        public string BiblioRecPath = "";
        public string Class = "";

        // 创建分类号记录表
        public static int CreateClassTable(
            string strConnectionString,
            string strClassTableName,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists " + strClassTableName + " ;\n"
                + "CREATE TABLE " + strClassTableName + " "
                + "(" + " "
                + "bibliorecpath nvarchar (255) NULL," + " "
                + "class nvarchar (255) NULL" + " "
                + ") ; \n";
            strCommand += " CREATE INDEX " + strClassTableName + "_recpath_index \n"
+ " ON " + strClassTableName + " (bibliorecpath) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        // 插入一批分类号记录
        // TODO: 可以把 class 和 bibliorecpath 联合起来做唯一性约束
        public static int AppendClassLines(
            SQLiteConnection connection,
            string strClassTableName,
            List<ClassLine> lines,
            out string strError)
        {
            strError = "";

            Debug.WriteLine("AppendClassLines() lines.Count=" + lines.Count);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {

                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (ClassLine line in lines)
                    {
                        text.Append(
        " INSERT INTO " + strClassTableName + " (bibliorecpath, class) "
        + " VALUES(@bibliorecpath" + i
        + ", @class" + i + ")"
        + " ; ");
                        SQLiteUtil.SetParameter(command,
                            "@bibliorecpath" + i,
                            line.BiblioRecPath);
                        SQLiteUtil.SetParameter(command,
         "@class" + i,
         line.Class);
                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }

            return 0;
        }

        // 准备复制出一个没有重复事项的新的分类号记录表
        public static int CreateDistinctClassTable(
            string strConnectionString,
            string strSourceTableName,
            string strTargetTableName,
            out string strError)
        {
            strError = "";

            Debug.WriteLine("CreateDistinctClassTable()");

            // 创建表
            string strCommand = "DROP TABLE if exists " + strTargetTableName + " ; "
                + "CREATE TABLE " + strTargetTableName + " AS "
                + "select * from " + strSourceTableName + " group by bibliorecpath ;\n";

            strCommand += " CREATE INDEX IF NOT EXISTS " + strTargetTableName + "_recpath_index \n"
+ " ON " + strTargetTableName + " (bibliorecpath) ;\n";


            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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
                            + "SQL 命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }

        // 删除分类号记录表
        public static int DeleteClassTable(
            string strConnectionString,
            string strSourceTableName,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists " + strSourceTableName + " ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        strError = "删除表时出错.\r\n"
                            + ex.Message + "\r\n"
                            + "SQL 命令:\r\n"
                            + strCommand;
                        return -1;
                    }

                } // end of using command
            }

            return 0;
        }
    }

    // dp2library 用户信息
    class UserLine
    {
        public string ID = "";
        public string LibraryCodeList = "";
        public string Rights = "";

        // 创建用户记录表
        public static int CreateUserTable(
            string strConnectionString,
            out string strError)
        {
            strError = "";

            // 创建表
            string strCommand = "DROP TABLE if exists user ;\n"
                + "CREATE TABLE user "
                + "(" + " "
                + "id nvarchar (255) NULL," + " "
                + "rights nvarchar (255) NULL," + " "
                + "librarycodelist nvarchar (255) NULL" + " "
                + ") ; \n";
            strCommand += " CREATE INDEX user_id_index \n"
+ " ON user (id) ;\n";

            using (SQLiteConnection connection = new SQLiteConnection(strConnectionString))
            {
                connection.Open();

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

        // 插入一批用户记录
        public static int AppendUserLines(
            SQLiteConnection connection,
            List<UserLine> lines,
            out string strError)
        {
            strError = "";

            Debug.WriteLine("AppendUserLines() lines.Count=" + lines.Count);

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand command = new SQLiteCommand("",
    connection))
                {

                    StringBuilder text = new StringBuilder(4096);
                    int i = 0;
                    foreach (UserLine line in lines)
                    {
                        text.Append(
        " INSERT INTO user (id, librarycodelist, rights) "
        + " VALUES(@id" + i
        + ", @librarycodelist" + i
        + ", @rights" + i + ")"
        + " ; ");
                        SQLiteUtil.SetParameter(command,
                            "@id" + i,
                            line.ID);
                        SQLiteUtil.SetParameter(command,
         "@librarycodelist" + i,
         line.LibraryCodeList);
                        SQLiteUtil.SetParameter(command,
         "@rights" + i,
         line.Rights);
                        i++;
                    }

                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                }

                mytransaction.Commit();
            }
            return 0;
        }

    }
}
