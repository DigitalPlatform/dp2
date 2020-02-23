using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class LibraryContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Biblio> Biblios { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<PassGateOper> PassGateOpers { get; set; }
        public DbSet<GetResOper> GetResOpers { get; set; }
        public DbSet<CircuOper> CircuOpers { get; set; }
        public DbSet<PatronOper> PatronOpers { get; set; }
        public DbSet<BiblioOper> BiblioOpers { get; set; }
        public DbSet<ItemOper> ItemOpers { get; set; }
        public DbSet<AmerceOper> AmerceOpers { get; set; }

        /*
        DatabaseConfig _config = null;

        public LibraryContext(DatabaseConfig config)
        {
            _config = config;
        }
        */

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseMySQL("server=localhost;database=library;user=user;password=password");
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseMySql(DatabaseConfig.BuildConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemRecPath);
                // entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Biblio>(entity =>
            {
                entity.HasKey(e => e.RecPath);
                entity.HasMany(e => e.Keys);
                /*
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(d => d.Publisher)
                  .WithMany(p => p.Books);
                  */
            });

            /*
            modelBuilder.Entity<Biblio>().Property(p => p.RecPath)
    .HasDatabaseGeneratedOption(System.ComponentModel
    .DataAnnotations.Schema.DatabaseGeneratedOption.None);
    */

            // 检索点
            modelBuilder.Entity<Key>(entity =>
            {
                entity.HasKey(e => new { e.Text, e.Type, e.BiblioRecPath });
                entity.HasOne(e => e.Biblio);
                entity.Property(e => e.BiblioRecPath).IsRequired();
                /*
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(d => d.Publisher)
                  .WithMany(p => p.Books);
                  */
            });

            modelBuilder.Entity<Patron>(entity =>
            {
                entity.HasKey(e => e.RecPath);
            });

            /*
            modelBuilder.Entity<OperBase>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            */

            modelBuilder.Entity<PassGateOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<GetResOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<CircuOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<PatronOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<BiblioOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<ItemOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
            modelBuilder.Entity<AmerceOper>(entity =>
            {
                entity.HasKey(e => new { e.Date, e.No, e.SubNo });
            });
        }
    }

    public class User
    {
        public string ID { get; set; }
        // 馆代码列表。已变换为特殊形态 ,cod1,code2, 即，确保头尾都有逗号，这样方便 IndexOf() 进行匹配
        public string LibraryCodeList { get; set; }
        public string Rights { get; set; }
    }

    // 日志行 基础类
    public class OperBase
    {
        public string Date { get; set; }  // 所在日志文件日期，8 字符
        public long No { get; set; }
        public long SubNo { get; set; }  // 子序号。用于区分一个日志记录拆分为多个的情况
        public string LibraryCode { get; set; }
        public string Operation { get; set; }
        public string Action { get; set; }
        public DateTime OperTime { get; set; }
        public string Operator { get; set; }

        public object[] GetKeys()
        {
            return new object[] { Date, No, SubNo };
        }

        // 根据日志 XML 记录填充数据
        // 本函数负责填充基类的数据成员
        public virtual int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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
            this.OperTime = Replication.GetLocalTime(strOperTime);
            this.Operator = strOperator;
            return 0;
        }

        // 复制成员
        public void CopyTo(OperBase another)
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
    }

    // 入馆登记 每行
    public class PassGateOper : OperBase
    {
        // 馆代码
        // public string LibraryCode = "";

        // 读者证条码号
        public string ReaderBarcode { get; set; }

        // 门名称
        public string GateName { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
            out string strError)
        {
            strError = "";

            int nRet = base.SetData(dom, strDate, lIndex, out lines, out strError);
            if (nRet == -1)
                return -1;


            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "readerBarcode");

            string strGateName = DomUtil.GetElementText(dom.DocumentElement,
    "gateName");

            /*
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement,
    "libraryCode");

            this.LibraryCode = strLibraryCode;  // 这里和基础类的 LibraryCode 什么关系?
            */

            this.ReaderBarcode = strReaderBarcode;
            this.GateName = strGateName;
            return 0;
        }
    }

    // 获取对象操作 每行
    public class GetResOper : OperBase
    {
        // 对象 ID
        public string ObjectID { get; set; }

        // 元数据记录路径
        public string XmlRecPath { get; set; }

        public string Size { get; set; }
        public string Mime { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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

    }

    // 流通操作 每行
    public class CircuOper : OperBase
    {
        // 册条码号
        public string ItemBarcode { get; set; }

        // 读者证条码号
        public string ReaderBarcode { get; set; }

        public DateTime ReturningTime { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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

            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");
            if (string.IsNullOrEmpty(strBorrowPeriod) == false)
            {
                // parameters:
                //      strBorrowTime   借阅起点时间。u 格式
                //      strReturningTime    返回应还时间。 u 格式
                nRet = Replication.BuildReturingTimeString(this.OperTime,
    strBorrowPeriod,
    out DateTime returningTime,
    out string error);
                if (nRet == -1)
                {
                    this.ReturningTime = DateTime.MinValue;
                }
                else
                    this.ReturningTime = returningTime;
            }
            else
                this.ReturningTime = DateTime.MinValue;

            this.ItemBarcode = strItemBarcode;
            this.ReaderBarcode = strReaderBarcode;
            return 0;
        }

    }

    // 读者操作 日志行
    public class PatronOper : OperBase
    {
        // 特有的字段
        public string ReaderRecPath { get; set; }
        public string ReaderBarcode { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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

    }

    // 编目操作 每行
    public class BiblioOper : OperBase
    {
        // 特有的字段
        public string BiblioRecPath { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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

    }

    // item order issue comment 每行的基类
    public class ItemOper : OperBase
    {
        // 特有的字段
        public string ItemRecPath { get; set; }
        public string BiblioRecPath { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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
                            // MainForm.TryWriteErrorLog(strError + "\r\nXML记录: " + dom.OuterXml);
                            return -1;
                        }
                    }
                }

                if (string.IsNullOrEmpty(strParentID) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strParentID) == false, "");

                    Debug.Assert(this.Operation.IndexOf("set") == 0, "");
                    string strDbType = this.Operation.Substring("set".Length).ToLower();

                    // 根据实体库名得到书目库名
                    this.BiblioRecPath = Replication.BuildBiblioRecPath(strDbType, // "item",
                        this.ItemRecPath,
                        strParentID);
                }
            }

            return 0;
        }

    }

    // 交费操作
    public class AmerceOper : OperBase
    {
        // 特有的字段
        public string AmerceRecPath { get; set; }
        public string ItemBarcode { get; set; }
        public string ReaderBarcode { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }  // 元值
        public string Reason { get; set; }
        public string ID { get; set; }

        // 根据日志 XML 记录填充数据
        public override int SetData(XmlDocument dom,
            string strDate,
            long lIndex,
            out List<OperBase> lines,
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
                foreach (XmlElement record in records)
                {
                    if (i == 0)
                        FillRecord(strAction, record, this, debugInfo);
                    else
                    {
                        if (lines == null)
                            lines = new List<OperBase>();
                        AmerceOper line = new AmerceOper();
                        (this as OperBase).CopyTo(line);
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
                        // MainForm.WriteErrorLog(strError);
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
                                    lines = new List<OperBase>();
                                AmerceOper line = new AmerceOper();
                                (this as OperBase).CopyTo(line);
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

        static XmlElement GetOverdueByID(string strReaderRecord, string strID)
        {
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(strReaderRecord);
            XmlElement overdue = readerdom.DocumentElement.SelectSingleNode("overdues/overdue[@id='" + strID + "']") as XmlElement;
            if (overdue == null)
                return null;    // not found
            return overdue;
        }

        // 根据读者记录中 overdue 内容填充 line 的各个成员
        static void FillRecordByOverdue(XmlElement overdue,
            string strReaderBarcode,
            string strNewPrice,
            AmerceOper line,
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

                nRet = ParsePriceString(strResult,
        out decimal value,
        out string strUnit,
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
            AmerceOper line,
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
                int nRet = ParsePriceString(strPrice,
        out decimal value,
        out string strUnit,
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

            // 分析期限参数
            int nRet = StringUtil.ParsePeriodUnit(strBorrowPeriod,
                out long lValue,
                out string strUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "期限字符串 '" + strBorrowPeriod + "' 格式不合法: " + strError;
                return -1;
            }


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
    out DateTime borrowdate) == false)
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
    out decimal value,
    out string strUnit,
    out string strError)
        {
            value = 0;
            strUnit = "";
            strError = "";

            if (string.IsNullOrEmpty(strPrice) == true)
                return 0;

            // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
            int nRet = PriceUtil.ParseSinglePrice(strPrice,
                out CurrencyItem item,
                out strError);
            if (nRet == -1)
                return -1;

            strUnit = item.Prefix + item.Postfix;
            value = item.Value;
            return 0;
        }

#if NO
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

#endif
    }

    public static class DatabaseConfig
    {
        public static string ServerName { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string DatabaseName { get; set; }

        public static string BuildConnectionString()
        {
            return $"server={ServerName};database={DatabaseName};user={UserName};password={Password};Connection Timeout=300;Keepalive=10;";
        }
    }
}
