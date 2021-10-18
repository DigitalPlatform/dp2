using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class Item
    {
        public string ItemRecPath { get; set; }
        public string ItemBarcode { get; set; }
        public string Location { get; set; }
        public string AccessNo { get; set; }
        public string BiblioRecPath { get; set; }

        public DateTime CreateTime { get; set; }
        public string State { get; set; }

        public decimal Price { get; set; }
        public string Unit { get; set; }

        public string Borrower { get; set; }
        public DateTime BorrowTime { get; set; }
        public string BorrowPeriod { get; set; }
        public DateTime ReturningTime { get; set; }   // 预计还回时间

        // 2021/10/15
        public string BorrowID { get; set; }

        public Item Clone()
        {
            Item result = new Item();
            this.CopyTo(result);
            return result;
        }

        public void CopyTo(Item another)
        {
            if (this == another)
                throw new ArgumentException("不能用 this 来调用 CopyTo()");
            another.ItemRecPath = this.ItemRecPath;
            another.ItemBarcode = this.ItemBarcode;
            another.Location = this.Location;
            another.AccessNo = this.AccessNo;
            another.BiblioRecPath = this.BiblioRecPath;
            another.CreateTime = this.CreateTime;
            another.State = this.State;
            another.Price = this.Price;
            another.Unit = this.Unit;
            another.Borrower = this.Borrower;
            another.BorrowTime = this.BorrowTime;
            another.BorrowPeriod = this.BorrowPeriod;
            another.ReturningTime = this.ReturningTime;
            another.BorrowID = this.BorrowID;
        }

        public void ClearBorrowInfo()
        {
            this.Borrower = null;
            this.BorrowTime = DateTime.MinValue;
            this.BorrowPeriod = null;
            this.ReturningTime = DateTime.MinValue;
            this.BorrowID = null;
        }

        // 从 XML 记录变换
        // parameters:
        //      strLogCreateTime    日志操作记载的创建时间。若不是创建动作的其他时间，不要放在这里
        public static int FromXml(XmlDocument dom,
            string strItemRecPath,
            string strBiblioRecPath,
            string strLogCreateTime,
            ref Item line,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (line == null)
                line = new Item();

            line.ItemRecPath = strItemRecPath;
            line.ItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            if (string.IsNullOrEmpty(line.ItemBarcode))
            {
                string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                line.ItemBarcode = "@refID:" + strRefID;
            }

            string location = StringUtil.GetPureLocationString(
            DomUtil.GetElementText(dom.DocumentElement,
            "location"));    // 要变为纯净的地点信息，即不包含 #reservation 之类
            line.Location = Item.CanonicalizeLocationString(location);

            line.AccessNo = DomUtil.GetElementText(dom.DocumentElement,
                "accessNo");
            line.BiblioRecPath = strBiblioRecPath;
            line.State = DomUtil.GetElementText(dom.DocumentElement,
    "state");

            line.Borrower = DomUtil.GetElementText(dom.DocumentElement,
    "borrower");
            line.BorrowTime = Replication.GetLocalTime(DomUtil.GetElementText(dom.DocumentElement,
    "borrowDate"));
            line.BorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
"borrowPeriod");
            // line.ReturningTime = GetLocalTime(DomUtil.GetElementText(dom.DocumentElement, "returningDate"));
            line.BorrowID = DomUtil.GetElementText(dom.DocumentElement,
    "borrowID");


            if (line.BorrowTime != DateTime.MinValue)
            {
                // parameters:
                //      strBorrowTime   借阅起点时间。u 格式
                //      strReturningTime    返回应还时间。 u 格式
                nRet = Replication.BuildReturingTimeString(line.BorrowTime,
    line.BorrowPeriod,
    out DateTime returningTime,
    out strError);
                if (nRet == -1)
                {
                    line.ReturningTime = DateTime.MinValue;
                }
                else
                    line.ReturningTime = returningTime;
            }
            else
                line.ReturningTime = DateTime.MinValue;

            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
    "price");
            nRet = Replication.ParsePriceString(strPrice,
    out decimal value,
    out string strUnit,
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
                    // TODO: Replication 里是否已经有特定函数?
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

            if (DateTime.TryParse(strTime, out DateTime createTime) == true)
                line.CreateTime = createTime;
            return 0;
        }

        // 把 location 字符串变换为便于处理的形态
        // 阅览室 --> /阅览室
        // 海淀分馆/阅览室 --> 海淀分馆/阅览室
        // #reservation,阅览室 --> /阅览室
        public static string CanonicalizeLocationString(string text)
        {
            text = StringUtil.GetPureLocation(text);
            // 分析 strLocation 是否属于总馆形态，比如“阅览室”
            // 如果是总馆形态，则要在前部增加一个 / 字符，以保证可以正确匹配 map 值
            // ‘/’字符可以理解为在馆代码和阅览室名字之间插入的一个必要的符号。这是为了弥补早期做法的兼容性问题
            Replication.ParseCalendarName(text,
        out string strLibraryCode,
        out string strRoom);
            if (string.IsNullOrEmpty(strLibraryCode))
                text = "/" + strRoom;
            return text;
        }
    }


}
