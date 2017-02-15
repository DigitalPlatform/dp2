using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;

// MEMO: 订单打印后，可以保留“电子订单” XML 格式，供以后参考。数据库内各种数据可能会后来变化，但电子订单固化记忆了打印瞬间的数据状态

namespace dp2Circulation
{
    /// <summary>
    /// 若干订单构成的数组
    /// </summary>
    public class OrderListColletion : IEnumerable
    {
        List<OrderList> _lists = new List<OrderList>();

        public IEnumerator GetEnumerator()
        {
            return _lists.GetEnumerator();
        }

        // 将多个 OrderListItem 事项加入应去的 OrderList
        public void AddRange(List<OrderListItem> items)
        {
            foreach (OrderListItem item in items)
            {
                OrderList list = FindList(item.Seller);
                if (list == null)
                {
                    list = new OrderList();
                    list.Seller = item.Seller;
                    this._lists.Add(list);
                }
                list.Add(item);
            }
        }

        public OrderList FindList(string seller)
        {
            foreach (OrderList list in _lists)
            {
                if (list.Seller == seller)
                    return list;
            }

            return null;
        }

        public void Clear()
        {
            _lists.Clear();
        }
    }

    /// <summary>
    /// 一张订单的内存结构
    /// </summary>
    public class OrderList
    {
        public string Seller { get; set; }

        List<OrderListItem> _items = new List<OrderListItem>();

        public void AddRange(List<OrderListItem> items)
        {
            _items.AddRange(items);
        }

        public void Add(OrderListItem item)
        {
            _items.Add(item);
        }

        public void FillInWebBrowser(WebBrowser webBrowser)
        {
            OutputBegin(webBrowser);
            int nStart = 0;
            foreach (OrderListItem item in _items)
            {
                OutputLine(webBrowser, item, ref nStart);
            }
            OutputEnd(webBrowser);
        }

        static string TABLE = "TABLE";
        static string TR = "TR";
        static string TD = "TD";

        void OutputBegin(WebBrowser webBrowser1)
        {
            StringBuilder text = new StringBuilder();

            string strBinDir = Program.MainForm.UserDir;

            string strCssUrl = Path.Combine(Program.MainForm.UserDir, "Order\\OrderSheet_light.css");
            string strOrderSheetJs = Path.Combine(Program.MainForm.UserDir, "Order\\OrderSheet.js");
            string strOrderBaseJs = Path.Combine(Program.MainForm.UserDir, "Order\\OrderBase.js");
            string strLink = "\r\n<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strScriptHead = "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\"></script>"
                + "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\"></script>"
                // + "\r\n<script type=\"text/javascript\" src=\"%bindir%/order/jquery.scrollIntoView.min.js\"></script>"
                + "\r\n<script type='text/javascript' charset='UTF-8' src='" + strOrderSheetJs + "'></script>"
                + "\r\n<script type='text/javascript' charset='UTF-8' src='" + strOrderBaseJs + "' ></script>";
            // string strStyle = "<link href=\"%bindir%/select2/select2.min.css\" rel=\"stylesheet\" />" +
            // "<script src=\"%bindir%/select2/select2.min.js\"></script>";
            text.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                // + strStyle.Replace("%bindir%", strBinDir)
                + "</head><body>");

            AppendHtml(webBrowser1, text.ToString(), false);
            text.Clear();
        }

        int _tableCount = 0;

        void OutputLine(WebBrowser webBrowser1,
            OrderListItem item,
            ref int nStart)
        {
            StringBuilder text = new StringBuilder();

            if (_tableCount == 0)
                text.Append("\r\n<" + TABLE + " class=''>");

            text.Append("\r\n\t<" + TR + " class='check' biblio-recpath='" + item.BiblioStore.RecPath + "' " + strOnClick + ">");
            // text.Append("\r\n\t\t<" + TD + " class='biblio-index'><div>" + (nStart + 1).ToString() + "</div></" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap' colspan='10'>"
                // + "<div class='biblio-head'>" + HttpUtility.HtmlEncode(item.RecPath) + "</div>"
                + "<div class='biblio-table-container'>" + BuildBiblioHtml(item) + "</div>"
                + "</" + TD + ">");

            text.Append("\r\n\t</" + TR + ">");

            text.Append("\r\n\t</" + TR + ">");

            text.Append(GetTitleLine());
            text.Append(BuildLineHtml(item, nStart, null));

            nStart++;

            if (_tableCount >= 9)
            {
                text.Append("\r\n</" + TABLE + ">");
                _tableCount = 0;
            }
            else
                _tableCount++;

            AppendHtml(webBrowser1, text.ToString(), false);
            text.Clear();
        }

        void OutputEnd(WebBrowser webBrowser1)
        {
            StringBuilder text = new StringBuilder();

            if (_tableCount != 0)
            {
                text.Append("\r\n</" + TABLE + ">");
                _tableCount = 0;
            }

            text.Append("</body></html>");

            AppendHtml(webBrowser1, text.ToString(), false);
            text.Clear();
        }

        public static string GetTitleLine()
        {
            StringBuilder text = new StringBuilder();
            text.Append("\r\n\t<" + TR + " class='title'>");
            // text.Append("<td class='nowrap'></td>");
            text.Append("\r\n\t\t<" + TD + " class='order-index nowrap'>序号</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>书目号</" + TD + ">");
#if NO
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>题名</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>著者</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>出版者</" + TD + ">");
#endif
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>复本</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>期数</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>单价</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>总价</" + TD + ">");
            text.Append("\r\n\t</" + TR + ">");
            return text.ToString();
        }

        static string BuildBiblioHtml(
            OrderListItem item)
        {
            string[] types = new string[] { "title", "author", "publisher" };
            StringBuilder text = new StringBuilder();
            text.Append("\r\n<table class='biblio'>");

            foreach (string type in types)
            {
                string strName ="";
                string strValue = "";
                if (type == "title")
                {
                    strName = "题名";
                    strValue = item.Title;
                }

                if (type == "author")
                {
                    strName = "著者";
                    strValue = item.Author;
                }
                
                if (type == "publisher")
                {
                    strName = "出版者";
                    strValue = item.Publisher;
                }


                string strClass = "line";
                if (string.IsNullOrEmpty(type) == false)
                    strClass += " type-" + type;
                text.Append("\r\n\t<tr class='" + strClass + "'>");
                {
                    text.Append("\r\n\t\t<td class='name'>" + HttpUtility.HtmlEncode(strName) + "</td>");
                    text.Append("\r\n\t\t<td class='value'>" + HttpUtility.HtmlEncode(strValue).Replace("\n", "<br/>") + "</td>");
                }
                text.Append("\r\n\t</tr>");
            }
            text.Append("\r\n</table>");
            return text.ToString();
        }

        static string strOnChange = "onchange='javascript:onChanged(this);'";
        static string strOnClick = "onclick='javascript:onClicked(this);'";

        string BuildLineHtml(
    OrderListItem item,
    int i,
    string strClass)
        {
            StringBuilder text = new StringBuilder();


            text.Append("\r\n\t<" + TR + " "
                + "class='item check "
                + (string.IsNullOrEmpty(strClass) ? "" : " " + strClass)
                + "' index='" + i + "' seller='" + HttpUtility.HtmlEncode(item.Seller) + "' " + strOnClick + ">");
            // text.Append("<td class='nowrap'></td>");
            text.Append("\r\n\t\t<" + TD + " class='item-index'>" + (i + 1).ToString() + "</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='catalogNo'>");
            text.Append(HttpUtility.HtmlEncode(item.CatalogNo));
            text.Append("</" + TD + ">");

#if NO
            text.Append("\r\n\t\t<" + TD + " class='title'>");
            text.Append(HttpUtility.HtmlEncode(item.Title));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='author'>");
            text.Append(HttpUtility.HtmlEncode(item.Author));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='publisher'>");
            text.Append(HttpUtility.HtmlEncode(item.Publisher));
            text.Append("</" + TD + ">");
#endif

            text.Append("\r\n\t\t<" + TD + " class='copy'>");
            text.Append(HttpUtility.HtmlEncode(item.Copy));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='issueCount'>");
            text.Append(HttpUtility.HtmlEncode(item.IssueCount));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='price'>");
            text.Append(HttpUtility.HtmlEncode(item.Price));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='totalPrice'>");
            text.Append(HttpUtility.HtmlEncode(item.TotalPrice));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t</" + TR + ">");

            return text.ToString();
        }

        public void AppendHtml(WebBrowser webBrowser1,
            string strText,
            bool scrollToEnd = true)
        {
            if (webBrowser1.InvokeRequired)
            {
                webBrowser1.BeginInvoke(new Action<WebBrowser, string, bool>(AppendHtml), webBrowser1, strText, scrollToEnd);
                return;
            }

            Global.WriteHtml(webBrowser1,
                strText);

            if (scrollToEnd)
            {
                // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
                webBrowser1.Document.Window.ScrollTo(0,
                    webBrowser1.Document.Body.ScrollRectangle.Height);
            }
        }

        // 种、册、金额累计
    }

    /// <summary>
    /// 一行订单事项
    /// </summary>
    public class OrderListItem
    {
        public string Seller { get; set; }

        public string CatalogNo { get; set; }
        public int Copy { get; set; }
        public string Price { get; set; }
        public string TotalPrice { get; set; }

        public string AcceptPrice { get; set; }
        public string IssueCount { get; set; }
        public string Range { get; set; }
        public int SubCopy { get; set; }
        public string SellerAddress { get; set; }
        public string Distribute { get; set; }

        public List<string> MergeComment { get; set; }
        public List<string> Comments { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }

        public BiblioStore BiblioStore { get; set; }

        /// <summary>
        /// 构成本行的订购记录列表
        /// </summary>
        public List<OrderStore> Orders { get; set; }

        // 将 OrderStorage 对象的信息合并到本对象
        public void Merge(OrderStore order)
        {
            string strError = "";

            if (this.Orders == null)
                this.Orders = new List<OrderStore>();

            if (this.Orders.IndexOf(order) != -1)
                throw new Exception("order 对象已经在 Orders 中存在了，不允许重复合并");

            this.Orders.Add(order);

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(order.Xml);

            Hashtable value_table = GetValues(order);
            string strSeller = (string)value_table["seller"];
            if (string.IsNullOrEmpty(this.Seller))
                this.Seller = strSeller;
            else
            {
                if (this.Seller != strSeller)
                    throw new Exception("this.Seller '" + this.Seller + "' 和即将合并的 order.Seller '" + strSeller + "' 不一致");
            }

            string strCatalogNo = (string)value_table["catalogNo"];
            if (string.IsNullOrEmpty(this.CatalogNo))
                this.CatalogNo = strCatalogNo;
            else
            {
                if (this.CatalogNo != strCatalogNo)
                    throw new Exception("this.CatalogNo '" + this.CatalogNo + "' 和即将合并的 order.CatalogNo '" + strCatalogNo + "' 不一致");
            }

            string strPrice = (string)value_table["price"];
            if (string.IsNullOrEmpty(this.Price))
                this.Price = strPrice;
            else
            {
                if (this.Price != strPrice)
                    throw new Exception("this.Price '" + this.Price + "' 和即将合并的 order.Price '" + strPrice + "' 不一致");
            }

            string strAcceptPrice = (string)value_table["acceptPrice"];
            if (string.IsNullOrEmpty(this.AcceptPrice))
                this.AcceptPrice = strAcceptPrice;
            else
            {
                if (this.AcceptPrice != strAcceptPrice)
                    throw new Exception("this.AcceptPrice '" + this.AcceptPrice + "' 和即将合并的 order.AcceptPrice '" + strAcceptPrice + "' 不一致");
            }

            string strIssueCount = (string)value_table["issueCount"];
            if (string.IsNullOrEmpty(this.IssueCount))
                this.IssueCount = strIssueCount;
            else
            {
                if (this.IssueCount != strIssueCount)
                    throw new Exception("this.IssueCount '" + this.IssueCount + "' 和即将合并的 order.IssueCount '" + strIssueCount + "' 不一致");
            }

            string strRange = (string)value_table["range"];
            if (string.IsNullOrEmpty(this.Range))
                this.Range = strRange;
            else
            {
                if (this.Range != strRange)
                    throw new Exception("this.Range '" + this.Range + "' 和即将合并的 order.Range '" + strRange + "' 不一致");
            }

            int nSubCopy = (int)value_table["subcopy"];
            if (this.SubCopy == 0)
                this.SubCopy = nSubCopy;
            else
            {
                if (this.SubCopy != nSubCopy)
                    throw new Exception("this.SubCopy '" + this.SubCopy + "' 和即将合并的 order.SubCopy '" + nSubCopy + "' 不一致");
            }

            string strSellerAddress = (string)value_table["sellerAddress"];
            if (string.IsNullOrEmpty(this.SellerAddress))
                this.SellerAddress = strSellerAddress;

            // 以下是需要累加的字段
            if (this.MergeComment == null)
                this.MergeComment = new List<string>();

            int nCopy = (int)value_table["copy"];
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            string strMergeComment = strSource + ", " + nCopy.ToString() + "册 (" + order.RecPath + ")";

            this.MergeComment.Add(strMergeComment);

            int nIssueCount = 1;
            if (string.IsNullOrEmpty(strIssueCount) == false)
            {
                Int32.TryParse(strIssueCount, out nIssueCount);
            }

            // 汇总价格
            List<string> totalprices = new List<string>();

            if (string.IsNullOrEmpty(this.TotalPrice) == false)
                totalprices.Add(this.TotalPrice);

            string strTotalPrice = "";
            if (String.IsNullOrEmpty(strPrice) == false)
            {
                int nRet = PriceUtil.MultiPrice(strPrice,
                    nCopy * nIssueCount,
                    out strTotalPrice,
                    out strError);
                if (nRet == -1)
                {
                    strError = "原始数据事项 " + order.RecPath + " 内价格字符串 '" + strPrice + "' 格式不正确: " + strError;
                    throw new Exception(strError);
                }

                totalprices.Add(strTotalPrice);
            }

            if (totalprices.Count > 1)
            {
                List<string> sum_prices = null;
                int nRet = PriceUtil.TotalPrice(totalprices,
                    out sum_prices,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }

                Debug.Assert(sum_prices.Count == 1, "");
                this.TotalPrice = sum_prices[0];
            }
            else if (totalprices.Count == 1)
                this.TotalPrice = totalprices[0];

            // 汇总注释
            if (this.Comments == null)
                this.Comments = new List<string>();

            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            if (String.IsNullOrEmpty(strComment) == false)
            {
                this.Comments.Add(strComment + " @" + order.RecPath);
            }

            // 汇总馆藏分配字符串
            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");
            if (String.IsNullOrEmpty(strDistribute) == false)
            {
                if (String.IsNullOrEmpty(this.Distribute) == true)
                    this.Distribute = strDistribute;
                else
                {
                    string strLocationString = "";
                    int nRet = LocationCollection.MergeTwoLocationString(this.Distribute,
                        strDistribute,
                        false,
                        out strLocationString,
                        out strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                    this.Distribute = strLocationString;
                }
            }

        }

        // return:
        //      true    主要字段相同，需要合并
        //      false   不需要合并
        public static bool Compare(OrderStore order1, OrderStore order2)
        {
            Hashtable value_table1 = GetValues(order1);
            Hashtable value_table2 = GetValues(order2);

            if (value_table1["seller"] != value_table2["seller"])
                return false;

            if (value_table1["price"] != value_table2["price"])
                return false;

            if (value_table1["acceptPrice"] != value_table2["acceptPrice"])
                return false;

            if (value_table1["catalogNo"] != value_table2["catalogNo"])
                return false;

            if (value_table1["issueCount"] != value_table2["issueCount"])
                return false;

            if (value_table1["range"] != value_table2["range"])
                return false;

            if (value_table1["subcopy"] != value_table2["subcopy"])
                return false;

            if (CompareAddress((string)value_table1["sellerAddress"], (string)value_table1["sellerAddress"]) != 0)
                return false;

            return true;
        }

        /// <summary>
        /// 比较两个渠道地址是否完全一致
        /// </summary>
        /// <param name="strXml1">渠道地址 XML 片断1</param>
        /// <param name="strXml2">渠道地址 XML 片断2</param>
        /// <returns>0: 完全一致; 1: 不完全一致</returns>
        public static int CompareAddress(string strXml1, string strXml2)
        {
            if (string.IsNullOrEmpty(strXml1) == true && string.IsNullOrEmpty(strXml2) == true)
                return 0;
            if (string.IsNullOrEmpty(strXml1) == true && string.IsNullOrEmpty(strXml2) == false)
                return 1;
            if (string.IsNullOrEmpty(strXml1) == false && string.IsNullOrEmpty(strXml2) == true)
                return 1;
            XmlDocument dom1 = new XmlDocument();
            XmlDocument dom2 = new XmlDocument();

            try
            {
                dom1.LoadXml("<root>" + strXml1 + "</root>");
            }
            catch (Exception ex)
            {
                throw new Exception("渠道地址XML字符串 '" + strXml1 + "' 格式不正确: " + ex.Message);
            }

            try
            {
                dom2.LoadXml("<root>" + strXml2 + "</root>");
            }
            catch (Exception ex)
            {
                throw new Exception("渠道地址XML字符串 '" + strXml2 + "' 格式不正确: " + ex.Message);
            }

            string[] elements = new string[] {
            "zipcode",
            "address",
            "department",
            "name",
            "tel",
            "email",
            "bank",
            "accounts",
            "payStyle",
            "comment"};

            foreach (string element in elements)
            {
                string v1 = DomUtil.GetElementText(dom1.DocumentElement, element);
                string v2 = DomUtil.GetElementText(dom2.DocumentElement, element);
                if (string.IsNullOrEmpty(v1) == true && string.IsNullOrEmpty(v2) == true)
                    continue;
                if (v1 != v2)
                    return 1;

            }

            return 0;
        }


        // 根据 BiblioStore 对象拆分出 OrderListItem 对象数组
        public static List<OrderListItem> SplitOrderListItems(BiblioStore biblio)
        {
            List<OrderListItem> results = new List<OrderListItem>();

            List<OrderStore> orders = new List<OrderStore>();
            // orders.AddRange(biblio.Orders);
            foreach (OrderStore order in biblio.Orders)
            {
                if (string.IsNullOrEmpty(order.GetFieldValue("state")))
                    orders.Add(order);
            }

            while (orders.Count > 0)
            {
                OrderListItem item = new OrderListItem();
                OrderStore start = orders[0];
                item.Merge(start);
                orders.RemoveAt(0);
                for (int i = 0; i < orders.Count; i++)
                {
                    OrderStore order = orders[i];
                    if (Compare(start, order) == true)
                    {
                        item.Merge(order);
                        orders.RemoveAt(i);
                        i--;
                    }
                }

                results.Add(item);
            }

            return results;
        }

        // 获得 OrderStore 对象的一些字段值，用于合并
        static Hashtable GetValues(OrderStore order)
        {
            string strError = "";

            Hashtable result = new Hashtable();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(order.Xml);

            string strRefID = DomUtil.GetElementText(dom.DocumentElement,
                "refID");

            result["seller"] = DomUtil.GetElementText(dom.DocumentElement,
                "seller");

            // 渠道地址
            result["sellerAddress"] = DomUtil.GetElementText(dom.DocumentElement,
                "sellerAddress");

            {
                // 单价
                string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                    "price");

                string strAcceptPrice = "";

                // price取其中的订购价部分
                {
                    string strOldPrice = "";
                    string strNewPrice = "";

                    // 分离 "old[new]" 内的两个值
                    OrderDesignControl.ParseOldNewValue(strPrice,
                        out strOldPrice,
                        out strNewPrice);

                    strPrice = strOldPrice;
                    strAcceptPrice = strNewPrice;
                }

                result["price"] = strPrice;
                result["acceptPrice"] = strAcceptPrice;
            }

            result["catalogNo"] = DomUtil.GetElementText(dom.DocumentElement,
    "catalogNo");


            string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                "issueCount");

            string strRange = DomUtil.GetElementText(dom.DocumentElement,
                "range");

            {
                string strTempCopy = DomUtil.GetElementText(dom.DocumentElement,
"copy");

                string strTempAcceptCopy = "";
                {
                    string strOldCopy = "";
                    string strNewCopy = "";
                    // 分离 "old[new]" 内的两个值
                    OrderDesignControl.ParseOldNewValue(strTempCopy,
                        out strOldCopy,
                        out strNewCopy);
                    strTempCopy = strOldCopy;
                    strTempAcceptCopy = strNewCopy;
                }

                int nCopy = 0;
                string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strTempCopy);
                if (string.IsNullOrEmpty(strLeftCopy) == false)
                {
                    try
                    {
                        nCopy = Convert.ToInt32(strLeftCopy);
                    }
                    catch (Exception ex)
                    {
                        strError = "原始数据事项 " + strRefID + " 内复本数字 '" + strLeftCopy + "' 格式不正确: " + ex.Message;
                        throw new Exception(strError);
                    }
                }

                result["copy"] = nCopy;

                int nSubCopy = 1;
                {
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strTempCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        try
                        {
                            nSubCopy = Convert.ToInt32(strRightCopy);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + strRefID + " 内每套册数 '" + strRightCopy + "' 格式不正确: " + ex.Message;
                            throw new Exception(strError);
                        }
                    }
                }

                result["subcopy"] = nSubCopy;

                int nAcceptSubCopy = 1;
                {
                    string strRightCopy = OrderDesignControl.GetRightFromCopyString(strTempAcceptCopy);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        try
                        {
                            nAcceptSubCopy = Convert.ToInt32(strRightCopy);
                        }
                        catch (Exception ex)
                        {
                            strError = "原始数据事项 " + strRefID + " 内已到每套册数 '" + strRightCopy + "' 格式不正确: " + ex.Message;
                            throw new Exception(strError);
                        }
                    }
                }

            }

            return result;
        }

    }


}
