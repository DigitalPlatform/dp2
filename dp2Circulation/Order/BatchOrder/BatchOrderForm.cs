#define DIV


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
using System.Collections;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;

namespace dp2Circulation
{
    /// <summary>
    /// 批订购窗口
    /// </summary>
    public partial class BatchOrderForm : MyForm
    {
        BatchOrderScript _script = new BatchOrderScript();

#if NO
        List<BiblioStore> _lines = new List<BiblioStore>();

        Hashtable _recPathTable = new Hashtable();  // biblio_recpath --> BiblioStore
#endif
        BiblioStoreCollection _lines = new BiblioStoreCollection();

        Values _values = null;

        public BatchOrderForm()
        {
            InitializeComponent();

            _script.BatchOrderForm = this;
            this.webBrowser1.ObjectForScripting = _script;
        }

        private void BatchOrderForm_Load(object sender, EventArgs e)
        {

            this.toolStripButton_orderList.Checked = Program.MainForm.AppInfo.GetBoolean("batchOrderForm", "orderListView", true);
        }

        public override void EnableControls(bool bEnable)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.toolStrip1.Enabled = bEnable;
            }));
        }

        public void BeginLoadLine(List<string> recpaths)
        {
            Task.Factory.StartNew(() => LoadLine(recpaths));
        }

        public void LoadLine(List<string> recpaths)
        {
            string strError = "";
            int nRet = LoadLines(recpaths, out strError);
            if (nRet == -1)
            {
                this.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, strError);
                }));
            }
        }

        public int LoadLines(
            List<string> recpaths,
            out string strError)
        {
            strError = "";

            // 2017/3/9 去除可能的重复
            StringUtil.RemoveDupNoSort(ref recpaths);

            this._lines.Clear();

            this._listCollection.Clear();

            this._values = new Values();

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装载书目和订购信息 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                if (stop != null)
                    stop.SetProgressRange(0, recpaths.Count);

                BiblioLoader loader = new BiblioLoader();
                loader.Channel = channel;
                loader.Stop = stop;
                loader.RecPaths = recpaths;
                loader.Format = "table";

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                _tableCount = 0;
                OutputBegin();

                int nStart = 0;
                int i = 0;
                foreach (BiblioItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(item.RecPath == recpaths[i], "");

                    if (stop != null)
                    {
                        stop.SetMessage("正在装载书目信息 " + item.RecPath + " ...");
                        stop.SetProgressValue(i);
                    }

                    BiblioStore line = new BiblioStore();
                    line.Orders = new List<OrderStore>();
                    line.RecPath = item.RecPath;
                    line.Xml = item.Content;
                    this._lines.Add(line);
                    // this._recPathTable[line.RecPath] = line;

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

                    OutputBiblio(line, ref nStart);

                    OutputOrderList(line);
                }

                OutputEnd();

                RefreshOrderSheets();

                // FillItems(this._lines);
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

                channel.Timeout = old_timeout;
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

        public void OnSelectionChanged()
        {
            dynamic result = webBrowser1.Document.InvokeScript("getSelectionCount");
            int order_count = result.order;
            int biblio_count = result.biblio;
            if (order_count + biblio_count > 0)
            {
                this.toolStripSplitButton_newOrder.Text = "新订购 [" + (order_count + biblio_count) + "]";
                this.toolStripSplitButton_newOrder.Enabled = true;

                this.ToolStripMenuItem_newOrderTemplate.Text = "新订购(模板) [" + (order_count + biblio_count) + "]";
                this.ToolStripMenuItem_newOrderTemplate.Enabled = true;
            }
            else
            {
                this.toolStripSplitButton_newOrder.Text = "新订购";
                this.toolStripSplitButton_newOrder.Enabled = false;

                this.ToolStripMenuItem_newOrderTemplate.Text = "新订购(模板)";
                this.ToolStripMenuItem_newOrderTemplate.Enabled = false;
            }

            if (order_count > 0)
            {
                this.toolStripButton_deleteOrder.Text = "删除 [" + (order_count) + "]";
                this.toolStripButton_deleteOrder.Enabled = true;

                this.ToolStripMenuItem_quickChange.Text = "快速修改 [" + (order_count) + "] ...";
                this.ToolStripMenuItem_quickChange.Enabled = true;
            }
            else
            {
                this.toolStripButton_deleteOrder.Text = "删除";
                this.toolStripButton_deleteOrder.Enabled = false;

                this.ToolStripMenuItem_quickChange.Text = "快速修改 ...";
                this.ToolStripMenuItem_quickChange.Enabled = false;
            }

            if (biblio_count > 0)
            {
                this.toolStripButton_loadBiblio.Text = "装入种册窗 [" + Math.Min(biblio_count, 10) + "]";
                this.toolStripButton_loadBiblio.Enabled = true;

                this.toolStripMenuItem_removeSelectedBiblio.Text = "移除所选书目 [" + biblio_count + "]";
                this.toolStripMenuItem_removeSelectedBiblio.Enabled = true;
            }
            else
            {
                this.toolStripButton_loadBiblio.Text = "装入种册窗";
                this.toolStripButton_loadBiblio.Enabled = false;

                this.toolStripMenuItem_removeSelectedBiblio.Text = "移除所选书目";
                this.toolStripMenuItem_removeSelectedBiblio.Enabled = false;
            }

            // 让当前活动的 Sheet 跟随进行相关选择
            if (this._listForm != null && this._listForm.ActiveSheet != null)
            {
                string selection = (string)this.webBrowser1.Document.InvokeScript("getSelection");
                // MessageBox.Show(this, selection);
                this._listForm.ActiveSheet.
                WebBrowser.Document.InvokeScript("selectOrders", new object[] { selection, true });
            }
        }

        public void OnOrderChanged(string strBiblioRecPath,
    string strOrderRefID,
    string strFieldName,
    string strValue)
        {
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");
            order.SetFieldValue(strFieldName, strValue);

            this.Changed = true;

            this.BeginRefreshOrderSheets();
        }

        string MacroOrderXml(string strXml, string strBiblioRecPath)
        {
            if (string.IsNullOrEmpty(strXml))
                return "";

            string strError = "";

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                throw new Exception(strError);
            }

            // 遍历所有一级元素的内容
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = nodes[i].InnerText;
                if (strText.Length > 0
                    && (strText[0] == '@' || strText.IndexOf("%") != -1))
                {
                    // 兑现宏
                    nodes[i].InnerText = GetMacroValue(strText, strBiblioRecPath);
                }
            }

            return dom.DocumentElement.OuterXml;
        }

        string GetMacroValue(string strMacroName, string strBiblioRecPath)
        {
            string strError = "";
            string strResultValue = "";
            int nRet = 0;

            if (strMacroName.IndexOf("%") != -1)
            {
                ParseOneMacroEventArgs e1 = new ParseOneMacroEventArgs();
                e1.Macro = strMacroName;
                e1.Simulate = false;
                ParseOneMacro(e1);

                // m_macroutil_ParseOneMacro(this, e1);
                if (e1.Canceled == true)
                    goto CONTINUE;
                if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    return strMacroName + ":error:" + e1.ErrorInfo;
                }
                return e1.Value;
            }

        CONTINUE:
            // 书目记录XML格式
            string strXmlBody = "";

            // 获取书目记录的局部
            nRet = GetBiblioPart(strBiblioRecPath,
                strXmlBody,
                strMacroName,
                out strResultValue,
                out strError);
            if (nRet == -1)
            {
                if (String.IsNullOrEmpty(strResultValue) == true)
                    return strMacroName + ":error:" + strError;

                return strResultValue;
            }

            return strResultValue;
        }

        // 获取书目记录的局部
        int GetBiblioPart(string strBiblioRecPath,
            string strBiblioXml,
            string strPartName,
            out string strResultValue,
            out string strError)
        {
            LibraryChannel channel = this.GetChannel();

            try
            {
                Progress.SetMessage("正在装入书目记录 " + strBiblioRecPath + " 的局部 ...");
                channel.Timeout = new TimeSpan(0, 0, 10);
                long lRet = channel.GetBiblioInfo(
                    null,   // Progress.State == 0 ? Progress : null,
                    strBiblioRecPath,
                    strBiblioXml,
                    strPartName,    // 包含'@'符号
                    out strResultValue,
                    out strError);
                return (int)lRet;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // return:
        //      返回 HTML tr 元素片段
        public string NewOrder(string strBiblioRecPath, string strXml)
        {
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = new OrderStore();
            if (biblio.Orders == null)
                biblio.Orders = new List<OrderStore>();
            order.Type = "new";
            if (string.IsNullOrEmpty(strXml) == false)
                order.Xml = MacroOrderXml(strXml, strBiblioRecPath);
            biblio.Orders.Add(order);

            this.Changed = true;

            string result = BuildOrderHtml(strBiblioRecPath, order, biblio.Orders.Count - 1, "new");

            this.BeginRefreshOrderSheets();

            return result;
        }

        public void DeleteOrder(string strBiblioRecPath, string strOrderRefID)
        {
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");
            order.Type = "deleted";

            this.Changed = true;

            this.BeginRefreshOrderSheets();
        }

        public string ChangeOrder(string strBiblioRecPath, string strOrderRefID, string xml)
        {
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");

            order.Modify(xml);

            this.Changed = true;

            string result = BuildOrderHtml(strBiblioRecPath,
                order,
                biblio.Orders.IndexOf(order),
                order.Type == "new" ? "new" : "changed");

            this.BeginRefreshOrderSheets();

            return result;
        }

        public System.Reflection.IReflect VerifyOrders(string strBiblioRecPath)
        {
            List<string> list = new List<string>();
            list.Add("test1");
            list.Add("test2");
            return new Reflector(list);
        }

        public void LoadBiblio(string strBiblioRecPath)
        {
            EntityForm form = EntityForm.OpenNewEntityForm(strBiblioRecPath);
        }

        public int GetOrderCount(string strBiblioRecPath)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");

            if (biblio.Orders == null)
                return 0;
            return biblio.Orders.Count;
        }

        public int GetOrderChangedCount(string strBiblioRecPath)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");

            if (biblio.Orders == null)
                return 0;

            int count = 0;
            foreach(OrderStore order in biblio.Orders)
            {
                if (order.Type == "new" || order.Type == "deleted")
                    count++;
                else if (order.Changed == true)
                    count++;
            }
            return count;
        }

        public void RemoveBiblio(string strBiblioRecPath)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            // BiblioStore biblio = this._recPathTable[strBiblioRecPath] as BiblioStore;
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");

            this._lines.Remove(biblio);

            // 不负责从视觉上删除 Web 页面中的 TR 哟
        }

        public string EditDistribute(string strBiblioRecPath,
            string strOrderRefID)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");

            string strCopy = order.GetFieldValue("copy");

            string strNewCopy = "";
            string strOldCopy = "";
            OrderDesignControl.ParseOldNewValue(strCopy,
                out strOldCopy,
                out strNewCopy);
            int copy = -1;
            Int32.TryParse(OrderDesignControl.GetCopyFromCopyString(strOldCopy), out copy);

            string strDistribute = order.GetFieldValue("distribute");
            DistributeDialog dlg = new DistributeDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.DistributeString = strDistribute;
            if (bControl == false)
                dlg.Count = copy;
            dlg.GetValueTable += dlg_GetValueTable;
            Program.MainForm.AppInfo.LinkFormState(dlg, "DistributeDialog_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return null;
            string strNewValue = dlg.DistributeString;
            order.SetFieldValue("distribute", strNewValue);
            return strNewValue;
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = Program.MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        public string VerifyDistribute(string strBiblioRecPath,
    string strOrderRefID)
        {
            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");

            string strCopy = order.GetFieldValue("copy");

            string strNewCopy = "";
            string strOldCopy = "";
            OrderDesignControl.ParseOldNewValue(strCopy,
                out strOldCopy,
                out strNewCopy);
            int copy = -1;
            Int32.TryParse(OrderDesignControl.GetCopyFromCopyString(strOldCopy), out copy);

            string strDistribute = order.GetFieldValue("distribute");

            LocationCollection locations = new LocationCollection();
            string strError = "";
            if (locations.Build(strDistribute,
                out strError) == -1)
                return strError;
            if (locations.Count >= copy)
                return null;
            return "需要增补 " + (copy - locations.Count) + " 个去向";
        }

        public string EditRange(string strBiblioRecPath,
    string strOrderRefID)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            BiblioStore biblio = this._lines.GetByRecPath(strBiblioRecPath);
            if (biblio == null)
                throw new Exception("路径为 '" + strBiblioRecPath + " 的 BiblioStore 在内存中没有找到");
            OrderStore order = biblio.FindOrderByRefID(strOrderRefID);
            if (order == null)
                throw new Exception("参考ID为 '" + strOrderRefID + "' 的 OrderStore 在内存中没有找到");

            string strRange = order.GetFieldValue("range");

            GetTimeDialog dlg = new GetTimeDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.RangeMode = true;
            dlg.String8 = strRange;
            Program.MainForm.AppInfo.LinkFormState(dlg, "RangeDialog_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return null;
            string strNewValue = dlg.String8;
            order.SetFieldValue("range", strNewValue);
            return strNewValue;
        }


        #endregion

        #region HTML View 操作

#if NO
        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "Order\\BatchOrder_dark.css");
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
#endif

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText, bool scrollToEnd = true)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, bool>(AppendHtml), strText, scrollToEnd);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            if (scrollToEnd)
            {
                // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
                this.webBrowser1.Document.Window.ScrollTo(0,
                    this.webBrowser1.Document.Body.ScrollRectangle.Height);
            }
        }

        // parameters:
        //      strPublicationType book/series
        public static string GetOrderTitleLine(string strPublicationType)
        {
            StringBuilder text = new StringBuilder();
            text.Append("\r\n\t<" + TR + " class='title'>");
            // text.Append("<td class='nowrap'></td>");
            text.Append("\r\n\t\t<" + TD + " class='order-index nowrap'>序号</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>状态</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>书目号</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>渠道</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>经费</" + TD + ">");

            if (strPublicationType == "series")
            {
                text.Append("\r\n\t\t<" + TD + " class='nowrap' colspan='2'>时间范围</" + TD + ">");
                text.Append("\r\n\t\t<" + TD + " class='nowrap'>期数</" + TD + ">");
            }

            text.Append("\r\n\t\t<" + TD + " class='nowrap'>复本</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>单价</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap' colspan='2'>去向</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>类别</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap'>批次号</" + TD + ">");
            text.Append("\r\n\t</" + TR + ">");
            return text.ToString();
        }

        static string strOnChange = "onchange='javascript:onChanged(this);'";
        static string strOnClick = "onclick='javascript:onClicked(this);'";

        static string GetSelectList(string strText,
            string[] values,
            string col_name)
        {
            StringBuilder text = new StringBuilder();

            text.Append("<select class='list event' col-name='" + col_name + "'  " + strOnChange + ">");
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

            public string[] IssueCountValues = null;

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

                this.IssueCountValues = new string[] { "6", "12", "24", "36" };
            }
        }

        static string TABLE = "table";
        static string TR = "tr";
        static string TD = "td";

        void OutputBegin()
        {
            this.ClearMessage();

            StringBuilder text = new StringBuilder();

            // string strBinDir = Environment.CurrentDirectory;
            string strBinDir = this.MainForm.UserDir;

            string strCssUrl = Path.Combine(this.MainForm.UserDir, "Order\\BatchOrder_light.css");
            string strBatchOrderJs = Path.Combine(this.MainForm.UserDir, "Order\\BatchOrder.js");
            string strOrderBaseJs = Path.Combine(this.MainForm.UserDir, "Order\\OrderBase.js");
            string strLink = "\r\n<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' ></link>";
            string strScriptHead = "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\" ></script>"

                // "\r\n<script type=\"text/javascript\" src=\"%bindir%/order/jquery-1.12.4.min.js\" ></script>"
                + "\r\n<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\" ></script>"
                // + "\r\n<script type=\"text/javascript\" src=\"%bindir%/order/jquery.scrollIntoView.js\" ></script>"
                + "\r\n<script type='text/javascript' charset='UTF-8' src='" + strBatchOrderJs + "' ></script>"
            + "\r\n<script type='text/javascript' charset='UTF-8' src='" + strOrderBaseJs + "' ></script>";
            // string strStyle = "<link href=\"%bindir%/select2/select2.min.css\" rel=\"stylesheet\" />" +
            // "<script src=\"%bindir%/select2/select2.min.js\"></script>";
            text.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                // + strStyle.Replace("%bindir%", strBinDir)
                + "\r\n</head>\r\n<body>");

            AppendHtml(text.ToString(), false);
            text.Clear();
        }

        // return:
        //      null    strBiblioRecPath 不是书目库名
        public static string GetPublicationType(string strBiblioRecPath)
        {
            string strBiblioDbName = Global.GetDbName(strBiblioRecPath);
            BiblioDbProperty prop = Program.MainForm.GetBiblioDbProperty(strBiblioDbName);
            if (prop == null)
                return null;

            if (string.IsNullOrEmpty(prop.IssueDbName) == true)
                return "book";

            return "series";
        }

        int _tableCount = 0;

        void OutputBiblio(BiblioStore item, ref int nStart)
        {
            string strPubType = GetPublicationType(item.RecPath);

            StringBuilder text = new StringBuilder();

            if (_tableCount == 0)
                text.Append("\r\n<" + TABLE + " class='frame'>");

            int nColSpan = 10;
            if (strPubType == "series")
                nColSpan += 3;
            text.Append("\r\n\t<" + TR + " class='check biblio' biblio-recpath='" + item.RecPath + "' " + strOnClick + ">");
            text.Append("\r\n\t\t<" + TD + " class='biblio-index'><div>" + (nStart + 1).ToString() + "</div></" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='nowrap' colspan='" + nColSpan + "'>"
                + "<div class='biblio-head'>" + HttpUtility.HtmlEncode(item.RecPath) + "</div>"
                + "<div class='biblio-table-container'>" + BuildBiblioHtml(item.RecPath, item.Xml) + "</div>"
                + "</" + TD + ">");

            text.Append("\r\n\t</" + TR + ">");

            if (item.Orders.Count > 0)
            {
                text.Append(GetOrderTitleLine(strPubType));
                int i = 0;
                foreach (OrderStore order in item.Orders)
                {
                    text.Append(BuildOrderHtml(item.RecPath, order, i, null));

                    i++;
                }
            }

            nStart++;

            if (_tableCount >= 9)
            {
                text.Append("\r\n</" + TABLE + ">");
                _tableCount = 0;
            }
            else
                _tableCount++;

            AppendHtml(text.ToString(), false);
            text.Clear();

        }

        void OutputEnd()
        {
            StringBuilder text = new StringBuilder();

            if (_tableCount != 0)
            {
                text.Append("\r\n</" + TABLE + ">");
                _tableCount = 0;
            }

            text.Append("</body></html>");

            AppendHtml(text.ToString(), false);
            text.Clear();
        }

#if NO
        void FillItems(List<BiblioStore> items)
        {

            int nStart = 0;
            foreach (BiblioStore item in items)
            {
                // Application.DoEvents();

            }

#if NO
            Global.SetHtmlString(this.webBrowser1,
    text.ToString(),
    Program.MainForm.DataDir,
    "bo_");
#endif
        }
#endif

        string BuildOrderHtml(
            string strBiblioRecPath,
            OrderStore order,
            int i,
            string strClass)
        {
            string strPubType = GetPublicationType(strBiblioRecPath);

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

            // range
            string range = DomUtil.GetElementText(item_dom.DocumentElement,
"range");

            // issueCount
            string issueCount = DomUtil.GetElementText(item_dom.DocumentElement,
"issueCount");

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
            // refID
            string refID = DomUtil.GetElementText(item_dom.DocumentElement,
"refID");
            if (string.IsNullOrEmpty(refID))
            {
                refID = Guid.NewGuid().ToString();
                order.SetFieldValue("refID", refID);
                item_dom.LoadXml(order.Xml);
            }

            // batchNo
            string batchNo = DomUtil.GetElementText(item_dom.DocumentElement,
"batchNo");


            text.Append("\r\n\t<" + TR + " "
                + (string.IsNullOrEmpty(state) ? "" : "disabled ")
                + "class='order check event"
                + (string.IsNullOrEmpty(strClass) ? "" : " " + strClass)
                + "' ref-id='" + refID + "' biblio-recpath='" + strBiblioRecPath + "' " + strOnClick + ">");
            // text.Append("<td class='nowrap'></td>");
            text.Append("\r\n\t\t<" + TD + " class='order-index'>" + (i + 1).ToString() + "</" + TD + ">");
            text.Append("\r\n\t\t<" + TD + " class='state nowrap'>" + HttpUtility.HtmlEncode(state) + "</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='catalogNo'>");
            text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(catalogNo) + "' col-name='catalogNo' size='8' " + strOnChange + "/>");
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='seller'>");
            text.Append(GetSelectList(seller, this._values.SellerValues, "seller"));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='source'>");
            text.Append(GetSelectList(source, this._values.SourceValues, "source"));
            text.Append("</" + TD + ">");

            if (strPubType == "series")
            {
                text.Append("\r\n\t\t<" + TD + " class='range-text'>");
                text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(range) + "' col-name='range' size='20' " + strOnChange + "/>");
                text.Append("</" + TD + ">");

                text.Append("\r\n\t\t<" + TD + " class='range-button'>");
                text.Append("<button type='button' onclick='javascript:onRangeButtonClick(this);'>...</button>");
                text.Append("</" + TD + ">");

                text.Append("\r\n\t\t<" + TD + " class='issue-count'>");
                text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(issueCount) + "' col-name='issueCount' size='2' " + strOnChange + "/>");
                text.Append("</" + TD + ">");
            }

            text.Append("\r\n\t\t<" + TD + " class='copy'>");
            // text.Append(HttpUtility.HtmlEncode(copy));
            text.Append(GetSelectList(copy, this._values.CopyNumbers, "copy"));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='price'>");
            text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(price) + "' col-name='price' size='8' " + strOnChange + "/>");
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='dis-text'>");
            text.Append(HttpUtility.HtmlEncode(distribute).Replace(";", ";<br/>"));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='dis-button'>");
            text.Append("<button type='button' onclick='javascript:onDisButtonClick(this);'>...</button>");
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='order-class'>");
            text.Append(GetSelectList(class_string, this._values.ClassValues, "class"));
            text.Append("</" + TD + ">");

            text.Append("\r\n\t\t<" + TD + " class='batchNo'>");
            text.Append("<input class='list event' type='text' value='" + HttpUtility.HtmlEncode(batchNo) + "' col-name='batchNo' size='8' " + strOnChange + "/>");
            text.Append("</" + TD + ">");

            text.Append("\r\n\t</" + TR + ">");

            return text.ToString();
        }

        static string BuildBiblioHtml(
            string strBiblioRecPath,
            string strXml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            StringBuilder text = new StringBuilder();
            text.Append("\r\n<table class='biblio'>");
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in nodes)
            {
                string strName = line.GetAttribute("name");
                string strValue = line.GetAttribute("value");
                string strType = line.GetAttribute("type");

                string strClass = "line";
                if (string.IsNullOrEmpty(strType) == false)
                    strClass += " type-" + strType;
                text.Append("\r\n\t<tr class='" + strClass + "'>");
                if (strName == "_coverImage")
                {
                    string strResPath = ScriptUtil.MakeObjectUrl(strBiblioRecPath, strValue);
                    if (StringUtil.IsHttpUrl(strResPath) == false)
                        strResPath = "dpres:" + strResPath;

                    strValue = @"<img src='" + strResPath + "' alt='image'></img>";

                    text.Append("\r\n\t\t<td class='name'></td>");
                    text.Append("\r\n\t\t<td class='value'>" + strValue + "</td>");
                }
                else
                {
                    text.Append("\r\n\t\t<td class='name'>" + HttpUtility.HtmlEncode(strName) + "</td>");
                    text.Append("\r\n\t\t<td class='value'>" + HttpUtility.HtmlEncode(strValue).Replace("\n", "<br/>") + "</td>");
                }
                text.Append("\r\n\t</tr>");
            }
            text.Append("\r\n</table>");
            return text.ToString();
        }

        #endregion

        private void toolStripButton_deleteOrder_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("deleteOrder");
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            if (this.webBrowser1 != null && this.webBrowser1.Document != null && this.webBrowser1.Document.Body != null)
                this.webBrowser1.Document.Body.Focus();

            string strError = "";

            if (VerifyOrders() == false)
            {
                strError = "校验发现订购记录有错误，保存操作被放弃。请修改后重新保存。";
                goto ERROR1;
            }

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(2);

            this.ShowMessage("正在保存订购记录 ...");

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
                webBrowser1.Document.InvokeScript("clearAllChanged");
                return;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                this.EnableControls(true);

                this.ClearMessage();
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region 保存订购记录

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

        #endregion

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
                // this.toolStripButton_save.Enabled = value;
            }
        }

        private void BatchOrderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.webBrowser1 != null && this.webBrowser1.Document != null && this.webBrowser1.Document.Body != null)
                this.webBrowser1.Document.Body.Focus();

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
            Program.MainForm.AppInfo.SetBoolean("batchOrderForm",
                "orderListView",
                this.toolStripButton_orderList.Checked);

            CloseListForm();
        }

        private void toolStripSplitButton_newOrder_ButtonClick(object sender, EventArgs e)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            if (bControl == false)
                webBrowser1.Document.InvokeScript("newOrder");
            else
                ToolStripMenuItem_newOrderTemplate_Click(sender, e);
        }

        private void ToolStripMenuItem_newOrderTemplate_Click(object sender, EventArgs e)
        {
            // 看看即将插入的位置是图书还是期刊?
            string strFirstBiblioRecPath = (string)webBrowser1.Document.InvokeScript("getFirstSelectedBiblioRecPath");
            string strPubType = GetPublicationType(strFirstBiblioRecPath);

            OrderEditForm edit = new OrderEditForm();

            // edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15
            SetXml(edit.OrderEditControl,
                Program.MainForm.AppInfo.GetString("batchOrderForm", "orderRecord", "<root />"),
                strPubType);
            edit.Text = "新增订购事项";
            edit.DisplayMode = strPubType == "series" ? "simpleseries" : "simplebook";
            edit.MainForm = Program.MainForm;
            // edit.ItemControl = this;    // 2016/1/8

            Program.MainForm.AppInfo.LinkFormState(edit, "BatchOrderForm_OrderEditForm_state");
            edit.ShowDialog(this);

            string strXml = GetXml(edit.OrderEditControl);
            Program.MainForm.AppInfo.SetString("batchOrderForm", "orderRecord", strXml);

            if (edit.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            webBrowser1.Document.InvokeScript("newOrder", new object[] { strXml });
        }

        static string GetXml(ItemEditControlBase control)
        {
            string strError = "";
            string strXml = "";
            int nRet = control.GetData(false, out strXml, out strError);
            if (nRet == -1)
                throw new Exception(strError);
            return strXml;
        }

        static void SetXml(ItemEditControlBase control,
            string strXml,
            string strPublicationType)
        {
            string strError = "";

            // 去掉记录里面的 issueCount 和 range 元素
            if (string.IsNullOrEmpty(strXml) == false
                && strPublicationType == "book")
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                DomUtil.DeleteElement(dom.DocumentElement, "range");
                DomUtil.DeleteElement(dom.DocumentElement, "issueCount");
                strXml = dom.DocumentElement.OuterXml;
            }

            int nRet = control.SetData(strXml, "", null, out strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        private void ToolStripMenuItem_selectAllBiblio_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("selectAllBiblio",
                new object[] { Control.ModifierKeys == Keys.Control ? false : true }
                );
        }

        private void ToolStripMenuItem_selectAllOrder_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("selectAllOrder",
                new object[] { Control.ModifierKeys == Keys.Control ? false : true });
        }

        private void toolStripButton_loadBiblio_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("loadBiblio");
        }

        private void ToolStripMenuItem_quickChange_Click(object sender, EventArgs e)
        {
            // 看看即将插入的位置是图书还是期刊?
            string strFirstBiblioRecPath = (string)webBrowser1.Document.InvokeScript("getFirstSelectedBiblioRecPath");
            string strPubType = GetPublicationType(strFirstBiblioRecPath);

            OrderEditForm edit = new OrderEditForm();

            SetXml(edit.OrderEditControl,
                Program.MainForm.AppInfo.GetString("batchOrderForm", "quickChangeOrderRecord", "<root />"),
                strPubType);
            edit.Text = "快速修改订购事项";
            edit.DisplayMode = strPubType == "series" ? "simpleseries" : "simplebook";
            edit.MainForm = Program.MainForm;

            Program.MainForm.AppInfo.LinkFormState(edit, "BatchOrderForm_OrderEditForm_state");
            edit.ShowDialog(this);

            string strXml = GetXml(edit.OrderEditControl);
            Program.MainForm.AppInfo.SetString("batchOrderForm", "quickChangeOrderRecord", strXml);

            if (edit.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            webBrowser1.Document.InvokeScript("changeOrder", new object[] { strXml });
        }

        #region OrderListForm

        OrderListViewerForm _listForm = null;

        // parameters:
        //      bFloatingWindow 是否打开为浮动的对话框？ false 表示停靠在固定面板区
        void OpenListForm(bool bFloatingWindow)
        {
            if (this._listForm == null
                || (bFloatingWindow == true && this._listForm.Visible == false))
            {
                CloseListForm();

                this._listForm = new OrderListViewerForm();
                this._listForm.BatchOrderForm = this;
                this._listForm.FormClosed += _listForm_FormClosed;
                this._listForm.DockChanged += _listForm_DockChanged;
                // this._listForm.DoDockEvent += _listForm_DoDockEvent;
                GuiUtil.AutoSetDefaultFont(this._listForm);
                //this._keyboardForm.Text = "向导";
                //this._keyboardForm.BaseForm = this;

            }
            // this.easyMarcControl1.HideSelection = false;    // 当 EasyMarcControl 不拥有输入焦点时也能看到蓝色选定字段的标记

            if (bFloatingWindow == true)
            {
                if (_listForm.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(_listForm, "keyboardform_state");

                    _listForm.Show(this.MainForm);
                    _listForm.Activate();

                    //if (this._listForm != null)
                    //    this._listForm.SetColorStyle(this.ColorStyle);

                    this.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (_listForm.WindowState == FormWindowState.Minimized)
                        _listForm.WindowState = FormWindowState.Normal;
                    _listForm.Activate();
                }
            }
            else
            {
                if (_listForm.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentPropertyControl != this._listForm.MainControl)
                    {
                        _listForm.DoDock(true); // false 不会自动显示FixedPanel
                        // _listForm.Initialize(); // 没有 .Show() 的就用 .Initialize()
                    }
                }
            }

            this.toolStripButton_orderList.Checked = true;
            // this.checkBox_settings_keyboardWizard.Checked = true;

            BeginRefreshOrderSheets();
        }

        void _listForm_DockChanged(object sender, EventArgs e)
        {
            if (this._listForm.Docked == false)
            {
                this.FloatingOrderListForm = true;
            }
            else
            {
                // this.OpenListForm(true);
                this.FloatingOrderListForm = false;
            }
        }

#if NO
        void _listForm_DoDockEvent(object sender, DoDockEventArgs e)
        {
            if (this._listForm.Docked == false)
            {
                if (this.MainForm.CurrentPropertyControl != this._listForm.MainControl)
                    this.MainForm.CurrentPropertyControl = this._listForm.MainControl;

                if (e.ShowFixedPanel == true)
                {
                    if (this.MainForm.PanelFixedVisible == false)
                        this.MainForm.PanelFixedVisible = true;
                    // 把 propertypage 翻出来
                    this.MainForm.ActivatePropertyPage();
                }

                this._listForm.Docked = true;
                this._listForm.Visible = false;

                this.FloatingOrderListForm = false;

                //if (this._listForm != null)
                //    this._listForm.SetColorStyle(this.ColorStyle);
            }
            else
            {
                this.OpenListForm(true);
                this.FloatingOrderListForm = true;
            }
        }
#endif

        void _listForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.toolStripButton_orderList.Checked = false;
            // this.checkBox_settings_keyboardWizard.Checked = false;
        }

        void CloseListForm()
        {
            if (this._listForm != null)
            {
                if (this.MainForm.CurrentPropertyControl == this._listForm.MainControl)
                    this.MainForm.CurrentPropertyControl = null;

                this._listForm.Close();
                this._listForm = null;
            }
        }

        bool FloatingOrderListForm
        {
            get
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                    return Program.MainForm.AppInfo.GetBoolean("batchOrderForm", "orderListFormFloating", true);
                return true;
            }
            set
            {
                if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                    Program.MainForm.AppInfo.SetBoolean("batchOrderForm", "orderListFormFloating", value);
            }
        }

        #endregion

        private void toolStripButton_orderList_CheckedChanged(object sender, EventArgs e)
        {
            // 如果按下 Ctrl 键，则表示强制打开为浮动状态
            bool bControl = Control.ModifierKeys == Keys.Control;

            if (this.toolStripButton_orderList.Checked == true)
            {
                if (bControl)
                    this.FloatingOrderListForm = true;
                OpenListForm(this.FloatingOrderListForm);
            }
            else
            {
                CloseListForm();
            }
        }

        OrderListColletion _listCollection = new OrderListColletion();

        // 创建一条书目对应的的订购列表
        void OutputOrderList(BiblioStore biblio)
        {
            List<OrderListItem> items = OrderListItem.SplitOrderListItems(biblio);

            if (items.Count > 0)
            {
                Hashtable table = biblio.GetFields();
                // 添加书目、作者、出版社栏
                foreach (OrderListItem item in items)
                {
                    item.BiblioStore = biblio;

                    string strTitle = (string)table["title"];
                    string strAuthor = (string)table["author"];

                    List<string> parts = StringUtil.ParseTwoPart(strTitle, "/");
                    if (string.IsNullOrEmpty(parts[1]) == false)
                    {
                        strTitle = parts[0].Trim();
                        strAuthor = parts[1].Trim();
                    }

                    item.Title = strTitle;
                    item.Author = strAuthor;
                    item.Publisher = (string)table["publisher"];
                    item.Isbn = (string)table["isbn"];
                    if (string.IsNullOrEmpty(item.Isbn))
                        item.Isbn = (string)table["issn"];
                    if (string.IsNullOrEmpty(item.Isbn))
                        item.Isbn = (string)table["isbnandprice"];
                }
            }
            _listCollection.AddRange(items);
        }

        List<RateItem> GetRateTable(string strSeller)
        {
            XmlDocument dom = new XmlDocument();
            string strCfgFileName = Path.Combine(Program.MainForm.UserDir, "order_rate_table.xml");
            try
            {
                dom.Load(strCfgFileName);
            }
            catch(FileNotFoundException)
            {
                return null;
            }
            catch(DirectoryNotFoundException)
            {
                return null;
            }

            dp2Circulation.ExchangeRateDialog.TableItem table = ExchangeRateDialog.FindTableItem(dom, strSeller);

            if (table == null)
                return null;

            return RateItem.ParseList(table.Table);
        }

        // exception:
        //      可能会抛出异常
        void FillSheets()
        {
            if (this._listForm == null)
                return;

            List<string> exist_names = _listForm.GetSheetNames();
            List<string> rest_names = new List<string>(exist_names);
            List<string> states = new List<string>();
            List<int> offsets = new List<int>();
            foreach (string name in exist_names)
            {
                dp2Circulation.OrderListViewerForm.Sheet sheet = _listForm.GetSheet(name);
                // 保留以前的选择状态
                states.Add((string)sheet.WebBrowser.Document.InvokeScript("getSelection"));
                offsets.Add((int)sheet.WebBrowser.Document.InvokeScript("getWindowOffset"));
                ClearHtml(sheet.WebBrowser);
            }

#if NO
            if (string.IsNullOrEmpty(this.RateList) == false)
                rate_table = RateItem.ParseList(this.RateList);
#endif

            foreach (OrderList list in _listCollection)
            {
                List<RateItem> rate_table = GetRateTable(list.Seller);

                dp2Circulation.OrderListViewerForm.Sheet sheet = _listForm.CreateSheet(list.Seller);
                list.FillInWebBrowser(sheet.WebBrowser, rate_table);

                rest_names.Remove(list.Seller);
            }

            int i = 0;
            foreach (string name in exist_names)
            {
                dp2Circulation.OrderListViewerForm.Sheet sheet = _listForm.GetSheet(name);
                // 恢复以前的选择状态
                {
                    string selection = states[i];
                    if (string.IsNullOrEmpty(selection) == false)
                        sheet.WebBrowser.Document.InvokeScript("selectOrders", new object[] { selection, false });
                }

                {
                    int offset = offsets[i];
                    if (offset != 0)
                        sheet.WebBrowser.Document.InvokeScript("setWindowOffset", new object[] { offset });
                }

                i++;
            }

            // 删除剩下的 Sheet
            foreach (string name in rest_names)
            {
                _listForm.RemoveSheet(name);
            }
        }

        public void OnSheetSelectionChanged(dp2Circulation.OrderListViewerForm.Sheet sheet)
        {
            string selection = (string)sheet.WebBrowser.Document.InvokeScript("getSelection");
            // MessageBox.Show(this, selection);
            this.webBrowser1.Document.InvokeScript("selectOrders", new object[] { selection, true });
        }

        void RefreshOrderSheets()
        {
            if (_listForm == null)
                return;

            _listCollection.Clear();
            foreach (BiblioStore line in _lines)
            {
                OutputOrderList(line);
            }
            // 填充 OrderList WebBrowser s
            try
            {
                FillSheets();
            }
            catch (Exception ex)
            {
                // TODO: 对于各种数据格式不符合的情况，要抛出特定的 Exception，这样就不用显示 Debug 信息了
                MessageBox.Show(this, ExceptionUtil.GetDebugText(ex));
            }
        }

        void BeginRefreshOrderSheets()
        {
            this.BeginInvoke(new Action(RefreshOrderSheets));
        }

        static void ClearHtml(WebBrowser webBrowser1)
        {
            HtmlDocument doc = webBrowser1.Document;

            if (doc == null)
            {
                webBrowser1.Navigate("about:blank");
                doc = webBrowser1.Document;
            }
            doc = doc.OpenNew(true);
        }

#if NO
        public string RateList
        {
            get
            {
                return Program.MainForm.AppInfo.GetString("batchOrderForm", "rateList", "");
            }
            set
            {
                Program.MainForm.AppInfo.SetString("batchOrderForm", "rateList", value);
            }
        }
#endif

        private void toolStripButton_rate_Click(object sender, EventArgs e)
        {
            ExchangeRateDialog dlg = new ExchangeRateDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.CfgFileName = Path.Combine(Program.MainForm.UserDir, "order_rate_table.xml");
            Program.MainForm.AppInfo.LinkFormState(dlg, "BatchOrderForm_OrderEditForm_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.BeginRefreshOrderSheets();
        }

        private void toolStripButton_test_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                webBrowser1.Document.InvokeScript("clearAllErrorInfo");
                return;
            }


            // webBrowser1.Document.InvokeScript("test");
            VerifyOrders();
        }

        // 校验所有订购记录的格式合法性
        // return:
        //      true    合法
        //      false   有不合法的事项
        bool VerifyOrders()
        {
            // TODO: 首先清除以前残留的报错信息
            webBrowser1.Document.InvokeScript("clearAllErrorInfo");

            int nErrorCount = 0;
            foreach (BiblioStore biblio in this._lines)
            {
                // 将出错信息存储到内存结构
                if (biblio.VerifyOrders() == false)
                {
                    foreach (OrderStore order in biblio.Orders)
                    {
                        // 将出错信息显示到 WebControl 页面
                        if (string.IsNullOrEmpty(order.ErrorInfo) == false)
                            webBrowser1.Document.InvokeScript("setErrorInfo", new object[] { order.RefID, order.ErrorInfo });
                    }
                    nErrorCount++;
                }
            }

            if (nErrorCount > 0)
                return false;
            return true;
        }

        // 选择所有包含订购的书目
        private void toolStripMenuItem_selectAllBiblio_hasOrders_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("selectAllBiblioHasOrder",
    new object[] { Control.ModifierKeys == Keys.Control ? false : true }
    );

        }

        // 选择所有不包含订购的书目
        private void toolStripMenuItem_selectAllBiblio_noOrder_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("selectAllBiblioNoOrder",
    new object[] { Control.ModifierKeys == Keys.Control ? false : true }
    );

        }

        private void toolStripMenuItem_removeSelectedBiblio_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("removeSelectedBiblio");
            this.BeginRefreshOrderSheets();
        }
    }

    /// <summary>
    /// 一个书目记录的内存结构。包含从属的下级订购记录
    /// </summary>
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

        // 获得书名、作者、出版社 三个字段内容
        public Hashtable GetFields()
        {
            Hashtable results = new Hashtable();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in nodes)
            {
                // string strName = line.GetAttribute("name");
                string strType = line.GetAttribute("type");

                if (string.IsNullOrEmpty(strType))
                    continue;

                string strValue = line.GetAttribute("value");

                if (results.ContainsKey(strType) == false)
                    results[strType] = strValue;
            }

            return results;
        }

        // 校验所有下属订购记录的格式合法性
        // return:
        //      true    合法
        //      false   有不合法的事项
        public bool VerifyOrders()
        {
            string strError = "";
            int nRet = 0;

            string strPubType = BatchOrderForm.GetPublicationType(this.RecPath);
            bool bStrict = true;    // 是否要严格检查

            foreach (OrderStore order in Orders)
            {
                order.ErrorInfo = "";
            }

            int nErrorCount = 0;
            foreach (OrderStore order in Orders)
            {
                string strState = order.GetFieldValue("state");
                if (string.IsNullOrEmpty(strState) == false)
                    continue;

                List<string> errors = new List<string>();

                // 检查去向字符串
                string strLocationString = order.GetFieldValue("distribute");
                LocationCollection locations = new LocationCollection();
                nRet = locations.Build(strLocationString, out strError);
                if (nRet == -1)
                    errors.Add("馆藏分配去向字符串 '" + strLocationString + "' 格式错误: " + strError);

                // price 和 totalPrice
                string strPrice = order.GetFieldValue("price");
                string strTotalPrice = order.GetFieldValue("totalPrice");

                if (String.IsNullOrEmpty(strTotalPrice) == true)
                {
                    if (String.IsNullOrEmpty(strPrice) == true)
                        errors.Add("尚未输入价格");
                }
                else
                {
                    if (String.IsNullOrEmpty(strState) == true
                        && String.IsNullOrEmpty(strPrice) == false)
                        errors.Add("当输入了价格 ('" + strPrice + "') 时，必须把总价格设置为空 (但现在为 '" + strTotalPrice + "')");
                }

                // copy

                string strCopyString = order.GetFieldValue("copy");
                if (String.IsNullOrEmpty(strCopyString) == true)
                    errors.Add("尚未输入复本数");

                // range 和 issueCount
                if (strPubType == "series")
                {
                    string strRangeString = order.GetFieldValue("range");

                    if (String.IsNullOrEmpty(strRangeString) == true)
                        errors.Add("尚未输入时间范围");
                    else
                    {
                        if (strRangeString.Length != (2 * 8 + 1))
                            errors.Add("尚未输入完整的时间范围");
                        else
                        {
                            // return:
                            //      -1  error
                            //      0   succeed
                            nRet = OrderDesignControl.VerifyDateRange(strRangeString,
                                out strError);
                            if (nRet == -1)
                                errors.Add(strError);
                        }
                    }

                    string strIssueCountString = order.GetFieldValue("issueCount");
                    if (String.IsNullOrEmpty(strIssueCountString) == true)
                        errors.Add("尚未输入期数");
                }

                if (bStrict == true)
                {
                    string strSource = order.GetFieldValue("source");
                    string strSeller = order.GetFieldValue("seller");

                    if (String.IsNullOrEmpty(strSource) == true
                        && strSeller != "交换" && strSeller != "赠")
                        errors.Add("尚未输入经费来源");

                    if (strSeller == "交换" || strSeller == "赠")
                    {
                        if (String.IsNullOrEmpty(strSource) == false)
                            errors.Add("如果渠道为 交换 或 赠，则经费来源必须为空");
                    }

                    if (String.IsNullOrEmpty(strSeller) == true)
                        errors.Add("尚未输入渠道");

                    // 书目号可以为空

                    string strClass = order.GetFieldValue("class");
                    if (String.IsNullOrEmpty(strClass) == true)
                        errors.Add("尚未输入类别");

                }

                if (errors.Count > 0)
                {
                    order.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                    nErrorCount++;
                }
            }

            if (bStrict == true)
            {
                // 检查 渠道 + 经费来源 + 价格 3元组是否有重复
                for (int i = 0; i < this.Orders.Count; i++)
                {
                    OrderStore order = this.Orders[i];

                    string strState = order.GetFieldValue("state");
                    if (string.IsNullOrEmpty(strState) == false)
                        continue;

                    List<string> errors0 = new List<string>();

                    string strLocationString = order.GetFieldValue("distribute");
                    LocationCollection locations = new LocationCollection();
                    nRet = locations.Build(strLocationString, out strError);
                    if (nRet == -1)
                        errors0.Add("馆藏分配去向字符串 '" + strLocationString + "' 格式错误: " + strError);

                    if (errors0.Count > 0)
                    {
                        order.ErrorInfo = StringUtil.MakePathList(errors0, "; ");
                        nErrorCount++;
                    }

                    string strUsedLibraryCodes = StringUtil.MakePathList(locations.GetUsedLibraryCodes());
#if NO

                    // 检查馆代码是否在管辖范围内
                    // 只检查修改过的事项
                    if (IsChangedItem(item) == true
                        && this.VerifyLibraryCode != null)
                    {
                        VerifyLibraryCodeEventArgs e = new VerifyLibraryCodeEventArgs();
                        e.LibraryCode = strUsedLibraryCodes;
                        this.VerifyLibraryCode(this, e);
                        if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 馆藏分配去向错误: " + e.ErrorInfo;
                            return -1;
                        }
                    }
#endif
                    string strSeller = order.GetFieldValue("seller");
                    string strSource = order.GetFieldValue("source");
                    string strPrice = order.GetFieldValue("price");

                    for (int j = i + 1; j < this.Orders.Count; j++)
                    {
                        OrderStore temp_order = this.Orders[j];

                        string strTempState = temp_order.GetFieldValue("state");
                        if (string.IsNullOrEmpty(strTempState) == false)
                            continue;

                        List<string> errors = new List<string>();

                        string strTempLocationString = temp_order.GetFieldValue("distribute");
                        LocationCollection temp_locations = new LocationCollection();
                        nRet = temp_locations.Build(strTempLocationString, out strError);
                        if (nRet == -1)
                            errors.Add("馆藏分配去向字符串 '" + strTempLocationString + "' 格式错误: " + strError);

                        string strTempUsedLibraryCodes = StringUtil.MakePathList(temp_locations.GetUsedLibraryCodes());

                        string strTempSeller = temp_order.GetFieldValue("seller");
                        string strTempSource = temp_order.GetFieldValue("source");
                        string strTempPrice = temp_order.GetFieldValue("price");

                        if (strPubType == "book")
                        {
                            // 对图书检查四元组
                            if (strSeller == strTempSeller
                                && strSource == strTempSource
                                && strPrice == strTempPrice
                                && strUsedLibraryCodes == strTempUsedLibraryCodes)
                            {
                                // string strDebugInfo = "strPrice='"+strPrice+"' strTempPrice='"+strTempPrice+"'";
                                errors.Add("第 " + (i + 1).ToString() + " 行 和 第 " + (j + 1) + " 行之间 渠道/经费来源/价格/馆藏分配去向(中所含的馆代码) 四元组重复，需要将它们合并为一行");
                            }
                        }
                        else
                        {
                            // 对期刊检查五元组
                            if (strSeller == strTempSeller
                                && strSource == strTempSource
                                && strPrice == strTempPrice
                                && order.GetFieldValue("range") == temp_order.GetFieldValue("range")
                                && strUsedLibraryCodes == strTempUsedLibraryCodes)
                                errors.Add("第 " + (i + 1).ToString() + " 行 和 第 " + (j + 1) + " 行之间 渠道/经费来源/时间范围/价格/馆藏分配去向(中所含的馆代码) 五元组重复，需要将它们合并为一行");
                        }

                        // 
                        if (errors.Count > 0)
                        {
                            temp_order.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                            nErrorCount++;
                        }
                    }

                }
            }

            if (nErrorCount > 0)
                return false;
            return true;
        }
    }

    /// <summary>
    /// 一个订购记录的内存结构
    /// </summary>
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

        // parameters:
        //      template_xml    描述修改要求的 XML 字符串。元素值空表示不修改这个元素，为 "[空]"表示修改为空
        public void Modify(string template_xml)
        {
            XmlDocument temp_dom = new XmlDocument();
            temp_dom.LoadXml(template_xml);

            foreach (XmlNode node in temp_dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                string value = node.InnerText.Trim();
                if (string.IsNullOrEmpty(value))
                    continue;   // 空表示不修改
                if (value == "[空]")
                    value = "";

                SetFieldValue(node.Name, value);
            }
        }
    }
}
