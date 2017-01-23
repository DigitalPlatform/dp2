using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using System.Collections;

namespace dp2Circulation
{
    /// <summary>
    /// 批订购窗口
    /// </summary>
    public partial class BatchOrderForm : MyForm
    {
        BatchOrderScript _script = new BatchOrderScript();

        List<BiblioStore> _lines = new List<BiblioStore>();

        Hashtable _recPathTable = new Hashtable();  // biblio_recpath --> BiblioStore

        Values _values = null;

        public BatchOrderForm()
        {
            InitializeComponent();

            _script.BatchOrderForm = this;
            this.webBrowser1.ObjectForScripting = _script;
        }

        private void BatchOrderForm_Load(object sender, EventArgs e)
        {

        }

        public override void EnableControls(bool bEnable)
        {
            this.toolStrip1.Enabled = bEnable;
        }

        public int LoadLines(
            List<string> recpaths,
            out string strError)
        {
            strError = "";

            this._lines.Clear();
            this._recPathTable.Clear();

            this._values = new Values();

            LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新浏览行 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, recpaths.Count);

                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                loader.Stop = stop;
                loader.RecPaths = recpaths;
                loader.Format = "id,cols,xml";

                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(record.Path == recpaths[i], "");

                    if (stop != null)
                    {
                        stop.SetMessage("正在刷新浏览行 " + record.Path + " ...");
                        stop.SetProgressValue(i);
                    }

                    BiblioStore line = new BiblioStore();
                    line.Orders = new List<OrderStore>();
                    line.RecPath = record.Path;
                    line.Xml = record.RecordBody.Xml;
                    this._lines.Add(line);
                    this._recPathTable[line.RecPath] = line;

                    // 装入订购记录
                    SubItemLoader sub_loader = new SubItemLoader();
                    sub_loader.BiblioRecPath = line.RecPath;
                    sub_loader.Channel = channel;
                    sub_loader.Stop = stop;
                    sub_loader.DbType = "order";

                    sub_loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                    sub_loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                    foreach (EntityInfo info in sub_loader)
                    {
                        if (info.ErrorCode != ErrorCodeValue.NoError)
                        {
                            strError = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                            return -1;
                        }

                        OrderStore order = new OrderStore();
                        line.Orders.Add(order);
                        order.RecPath = info.OldRecPath;
                        order.Xml = info.OldRecord;
                        order.OldXml = info.OldRecord;
                        order.Timestamp = info.OldTimestamp;

#if NO
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(info.OldRecord);
#endif
                    }
                    i++;
                }

                FillItems(this._lines);
                return 0;
            }
            catch (Exception ex)
            {
                strError = "LoadLines() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.ReturnChannel(channel);

                this.EnableControls(true);
            }
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "BatchOrderForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

        // 更新到 Web 页面
        void UpdateUiLine(BiblioStore line)
        {

        }

        #region 内存 Document 操作

        public void OnOrderChanged(string strBiblioRecPath,
    string strOrderRefID,
    string strFieldName,
    string strValue)
        {
            BiblioStore biblio = this._recPathTable[strBiblioRecPath] as BiblioStore;
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");
            order.SetFieldValue(strFieldName, strValue);

            this.Changed = true;
        }

        // return:
        //      返回 HTML tr 元素片段
        public string NewOrder(string strBiblioRecPath)
        {
            BiblioStore biblio = this._recPathTable[strBiblioRecPath] as BiblioStore;
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = new OrderStore();
            if (biblio.Orders == null)
                biblio.Orders = new List<OrderStore>();
            order.Type = "new";
            biblio.Orders.Add(order);

            this.Changed = true;

            return BuildOrderHtml(strBiblioRecPath, order, biblio.Orders.Count - 1);
        }

        public void DeleteOrder(string strBiblioRecPath, string strOrderRefID)
        {
            BiblioStore biblio = this._recPathTable[strBiblioRecPath] as BiblioStore;
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");
            order.Type = "deleted";

            this.Changed = true;
        }

        #endregion

        #region HTML View 操作

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "default\\charginghistory.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        static string GetOrderTitleLine()
        {
            StringBuilder text = new StringBuilder();
            text.Append("<tr class='title'>");
            text.Append("<td class='nowrap'></td>");
            text.Append("<td class='nowrap'>序号</td>");
            text.Append("<td class='nowrap'>状态</td>");
            text.Append("<td class='nowrap'>书目号</td>");
            text.Append("<td class='nowrap'>渠道</td>");
            text.Append("<td class='nowrap'>经费</td>");
            text.Append("<td class='nowrap'>复本</td>");
            text.Append("<td class='nowrap'>单价</td>");
            text.Append("<td class='nowrap'>去向</td>");
            text.Append("<td class='nowrap'>类别</td>");
            text.Append("</tr>");
            return text.ToString();
        }


        static string GetSelectList(string strText,
            string[] values,
            string col_name)
        {
            StringBuilder text = new StringBuilder();

            text.Append("<select class='list event' col-name='" + col_name + "'>");
            int index = -1;
            if (values != null)
                index = Array.IndexOf(values, strText);
            if (index == -1)
                text.Append("<option value='" + HttpUtility.HtmlEncode(strText) + "' selected='selected'>" + HttpUtility.HtmlEncode(strText) + "</option>");

            if (values != null)
            {
                int i = 0;
                foreach (string value in values)
                {
                    text.Append("<option value='" + HttpUtility.HtmlEncode(value) + "'"
                        + (i == index ? " selected='selected' " : "")
                        + ">"
                        + HttpUtility.HtmlEncode(value)
                        + "</option>");
                    i++;
                }
            }
            text.Append("</select>");

            return text.ToString();
        }

        class Values
        {
            public string[] SellerValues = null;
            public string[] SourceValues = null;
            public string[] ClassValues = null;
            public string[] CopyNumbers = null;

            public Values()
            {
                string strError = "";
                string[] seller_values = null;
                Program.MainForm.GetValueTable("orderSeller",
                    "",
                    out seller_values,
                    out strError);
                this.SellerValues = seller_values;

                string[] source_values = null;
                Program.MainForm.GetValueTable("orderSource",
                    "",
                    out source_values,
                    out strError);
                this.SourceValues = source_values;

                string[] class_values = null;
                Program.MainForm.GetValueTable("orderClass",
                    "",
                    out class_values,
                    out strError);
                this.ClassValues = class_values;

                List<string> copyNumbers = new List<string>();
                for (int i = 1; i <= 10; i++)
                {
                    copyNumbers.Add(i.ToString());
                }
                this.CopyNumbers = copyNumbers.ToArray();
            }
        }

        void FillItems(List<BiblioStore> items)
        {
            this.ClearMessage();

            StringBuilder text = new StringBuilder();

            string strBinDir = Environment.CurrentDirectory;

            string strCssUrl = Path.Combine(this.MainForm.DataDir, "Order\\BatchOrder.css");
            string strSummaryJs = Path.Combine(this.MainForm.DataDir, "Order\\BatchOrder.js");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strScriptHead = "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\"></script>"
                + "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\"></script>"
                + "<script type='text/javascript' charset='UTF-8' src='" + strSummaryJs + "'></script>";
            // string strStyle = "<link href=\"%bindir%/select2/select2.min.css\" rel=\"stylesheet\" />" +
            // "<script src=\"%bindir%/select2/select2.min.js\"></script>";
            text.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                // + strStyle.Replace("%bindir%", strBinDir)
                + "</head><body>");

            text.Append("<table>");

            int nStart = 0;
            foreach (BiblioStore item in items)
            {
                text.Append("<tr class='check event' biblio-recpath='" + item.RecPath + "'>");
                text.Append("<td class=''>" + (nStart + 1).ToString() + "</td>");
                text.Append("<td class='nowrap' colspan='9'>" + HttpUtility.HtmlEncode(item.RecPath) + "</td>");

#if NO
                text.Append("<td>");
                text.Append("biblio");
                text.Append("</td>");
#endif

                text.Append("</tr>");


                if (item.Orders.Count > 0)
                {
                    int i = 0;
                    text.Append(GetOrderTitleLine());
                    foreach (OrderStore order in item.Orders)
                    {
                        text.Append(BuildOrderHtml(item.RecPath, order, i));

                        i++;
                    }
                }

                nStart++;
            }
            text.Append("</table>");
            text.Append("</body></html>");

            // this.AppendHtml(text.ToString());
            Global.SetHtmlString(this.webBrowser1,
    text.ToString(),
    Program.MainForm.DataDir,
    "bo_");
        }

        string BuildOrderHtml(
            string strBiblioRecPath,
            OrderStore order,
            int i)
        {
            StringBuilder text = new StringBuilder();

            XmlDocument item_dom = new XmlDocument();
            if (string.IsNullOrEmpty(order.Xml))
                order.Xml = "<root />";
            item_dom.LoadXml(order.Xml);

            // state
            string state = DomUtil.GetElementText(item_dom.DocumentElement,
                "state");
            // catalogNo
            string catalogNo = DomUtil.GetElementText(item_dom.DocumentElement,
"catalogNo");
            // seller
            string seller = DomUtil.GetElementText(item_dom.DocumentElement,
"seller");
            // source
            string source = DomUtil.GetElementText(item_dom.DocumentElement,
"source");
            // copy
            string copy = DomUtil.GetElementText(item_dom.DocumentElement,
"copy");
            // price
            string price = DomUtil.GetElementText(item_dom.DocumentElement,
"price");
            // distribute
            string distribute = DomUtil.GetElementText(item_dom.DocumentElement,
"distribute");
            // class
            string class_string = DomUtil.GetElementText(item_dom.DocumentElement,
"class");
            // class
            string refID = DomUtil.GetElementText(item_dom.DocumentElement,
"refID");
            if (string.IsNullOrEmpty(refID))
            {
                refID = Guid.NewGuid().ToString();
                order.SetFieldValue("refID", refID);
                item_dom.LoadXml(order.Xml);
            }

            text.Append("<tr class='order check event' ref-id='" + refID + "' biblio-recpath='" + strBiblioRecPath + "'>");
            text.Append("<td class='nowrap'></td>");
            text.Append("<td class=''>" + (i + 1).ToString() + "</td>");
            text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(state) + "</td>");

            text.Append("<td>");
            text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(catalogNo) + "' col-name='catalogNo' size='8'/>");
            text.Append("</td>");

            text.Append("<td>");
            text.Append(GetSelectList(seller, this._values.SellerValues, "seller"));
            text.Append("</td>");

            text.Append("<td>");
            text.Append(GetSelectList(source, this._values.SourceValues, "source"));
            text.Append("</td>");

            text.Append("<td class='copy'>");
            // text.Append(HttpUtility.HtmlEncode(copy));
            text.Append(GetSelectList(copy, this._values.CopyNumbers, "copy"));
            text.Append("</td>");

            text.Append("<td class='price'>");
            text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(price) + "' col-name='price' size='8'/>");
            text.Append("</td>");

            text.Append("<td>");
            text.Append(HttpUtility.HtmlEncode(distribute).Replace(";", ";<br/>"));
            text.Append("</td>");

            text.Append("<td>");
            text.Append(GetSelectList(class_string, this._values.ClassValues, "class"));
            text.Append("</td>");

            text.Append("</tr>");

            return text.ToString();
        }

        #endregion

        private void toolStripButton_newOrder_Click(object sender, EventArgs e)
        {
            //List<object> arg_list = new List<object>();
            webBrowser1.Document.InvokeScript("newOrder");
        }

        private void toolStripButton_deleteOrder_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("deleteOrder");
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            string strError = "";

            LibraryChannel channel = this.GetChannel();

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存订购记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                foreach (BiblioStore biblio in this._lines)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    List<EntityInfo> entities = null;
                    int nRet = BuildSaveEntities(
                        biblio,
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (entities.Count == 0)
                        continue;

                    nRet = SaveEntities(
                        channel,
                        biblio,
                        entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                this.Changed = false;
                return;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.ReturnChannel(channel);

                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int BuildSaveEntities(
            BiblioStore biblio,
            out List<EntityInfo> entityArray,
            out string strError)
        {
            strError = "";

            // Debug.Assert(this._lines != null, "");

            string id = Global.GetRecordID(biblio.RecPath);

            entityArray = new List<EntityInfo>();

            foreach (OrderStore order in biblio.Orders)
            {
                if (string.IsNullOrEmpty(order.Type) == true
                    && order.Changed == false)
                    continue;

                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(order.RefID) == true)
                {
                    order.SetFieldValue("refID", Guid.NewGuid().ToString());
                }

                info.RefID = order.RefID;

                order.SetFieldValue("parent", id);

                if (order.Type == "new")
                {
                    info.Action = "new";
                    info.NewRecPath = "";
                    info.NewRecord = order.Xml;
                    info.NewTimestamp = null;
                }
                else if (order.Type == "deleted")
                {
                    info.Action = "delete";
                    info.OldRecPath = order.RecPath;

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = order.Xml;
                    info.OldTimestamp = order.Timestamp;

                    // 删除操作要放在前面执行。否则容易出现条码号重复的情况
                    entityArray.Insert(0, info);
                    continue;
                }
                else
                {
                    info.Action = "change";
                    info.OldRecPath = order.RecPath;
                    info.NewRecPath = order.RecPath;

                    info.NewRecord = order.Xml;
                    info.NewTimestamp = null;

                    info.OldRecord = order.OldXml;
                    info.OldTimestamp = order.Timestamp;
                }

                entityArray.Add(info);
            }

            return 0;
        }

        internal int SaveEntities(
            LibraryChannel channel,
            BiblioStore biblio,
            List<EntityInfo> entities,
            out string strError)
        {
            strError = "";

            bool bWarning = false;
            EntityInfo[] errorinfos = null;
            string strWarning = "";

            int nBatch = 100;
            for (int i = 0; i < (entities.Count / nBatch) + ((entities.Count % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, entities.Count - i * nBatch);
                EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                long lRet = channel.SetOrders(
    stop,
    biblio.RecPath,
    current,
    out errorinfos,
    out strError);
                if (lRet == -1)
                    return -1;

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                string strError1 = "";
                if (RefreshOperResult(
                    biblio,
                    errorinfos,
                    out strError1) == true)
                {
                    bWarning = true;
                    strWarning += " " + strError1;
                }

                if (lRet == -1)
                    return -1;
            }

            if (string.IsNullOrEmpty(strWarning) == false)
                strError += " " + strWarning;

            if (bWarning == true)
                return -2;
            return 0;
        }

        internal static EntityInfo[] GetPart(List<EntityInfo> source,
int nStart,
int nCount)
        {
            EntityInfo[] result = new EntityInfo[nCount];
            for (int i = 0; i < nCount; i++)
            {
                result[i] = source[i + nStart];
            }
            return result;
        }

        bool RefreshOperResult(
            BiblioStore biblio,
            EntityInfo[] errorinfos,
            out string strWarning)
        {
            strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            foreach (EntityInfo info in errorinfos)
            {
                if (String.IsNullOrEmpty(info.RefID) == true)
                {
                    strWarning += " 服务器返回的EntityInfo结构中RefID为空";
                    return true;
                }

                OrderStore order = biblio.FindOrderByRefID(info.RefID);
                if (order == null)
                {
                    strWarning += " 在路径为 " + biblio.RecPath + " 的书目记录下属订购记录中没有找到参考 ID 为 '" + info.RefID + "' 的内存对象";
                    return true;
                }

                // 正常信息处理
                if (info.ErrorCode == ErrorCodeValue.NoError)
                {
                    if (info.Action == "new")
                    {
                        order.RecPath = info.NewRecPath;
                        order.OldXml = info.NewRecord;
                        order.Xml = info.NewRecord;
                        order.Timestamp = info.NewTimestamp;

                        order.Type = "";
                    }
                    else if (info.Action == "change"
                        || info.Action == "move")
                    {
                        order.RecPath = info.NewRecPath;
                        order.OldXml = info.NewRecord;
                        order.Xml = info.NewRecord;
                        order.Timestamp = info.NewTimestamp;

                        order.Type = "";
                    }

                    order.Changed = false;
                    order.ErrorInfo = "";
                    continue;
                }

                // 报错处理
                // TODO: 刷新 Web 中显示
                order.ErrorInfo = info.ErrorInfo;

                strWarning += info.RefID + " 在提交保存过程中发生错误 -- " + info.ErrorInfo + "\r\n";
            }

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改后重新提交保存";
                return true;
            }

            return false;
        }

        bool _changed = false;

        public bool Changed
        {
            get
            {
                return _changed;
            }
            set
            {
                _changed = value;
                this.toolStripButton_save.Enabled = value;
            }
        }

        private void BatchOrderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "BatchOrderFor",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void BatchOrderForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }

    public class BiblioStore
    {
        public string RecPath { get; set; }
        public string Xml { get; set; }

        public List<OrderStore> Orders { get; set; }

        public OrderStore FindOrderByRefID(string refID)
        {
            if (this.Orders == null)
                return null;
            foreach (OrderStore order in Orders)
            {
                if (order.RefID == refID)
                    return order;
            }

            return null;
        }

    }

    public class OrderStore
    {
        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }
        public bool Changed { get; set; }
        public string OldXml { get; set; }

        public string Type { get; set; }    // new/deleted 空表示一般事项
        public string ErrorInfo { get; set; }

        public string RefID
        {
            get
            {
                return GetFieldValue("refID");
            }
        }

        public void SetFieldValue(string name, string value)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            DomUtil.SetElementText(dom.DocumentElement, name, value);
            this.Xml = dom.DocumentElement.OuterXml;
            this.Changed = true;
        }

        public string GetFieldValue(string name)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            return DomUtil.GetElementText(dom.DocumentElement, name);
        }
    }
}
