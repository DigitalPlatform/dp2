using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Diagnostics;
using System.IO;

using System.Threading;
using System.Resources;
using System.Globalization;

//using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Script;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 显示同种之中全部册信息的控件
    /// </summary>
    [ToolboxData("<{0}:ItemsControl runat=server></{0}:ItemsControl>")]
    public class ItemsControl : WebControl, INamingContainer
    {
        public string FocusRecPath = "";    // 需要显示出来的焦点评注记录的路径

        public string Barcode = "";
        public string BiblioRecPath = "";
        public string WarningText = "";

        public bool Wrapper = false;

        public ItemDispStyle ItemDispStyle = ItemDispStyle.Items;

        // string tempOutput = "";

        // // ItemConverter ItemConverter = null;

        List<string> tempItemBarcodes = null;

        // 2007/10/18
        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.ItemsControl.cs",
                typeof(ItemsControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {
                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        // TODO: 从Session中取消
        // 册行数
        public int LineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "ItemsControl_LineCount"];
                return (o == null) ? 5 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "ItemsControl_LineCount"] = (object)value;
            }
        }

        public void Clear()
        {
            this.StartIndex = 0;
        }

        public int StartIndex
        {
            get
            {
                object o = ViewState[this.ID + "ItemsControl_StartIndex"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "ItemsControl_StartIndex"] = value;
            }
        }

        public int PageMaxLines
        {
            get
            {
                object o = ViewState[this.ID + "ItemsControl_PageMaxLines"];
                if (o == null)
                    return 10;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "ItemsControl_PageMaxLines"] = value;
            }
        }

        // 计算出页码总数
        public int GetPageCount(int nResultCount)
        {
            int nDelta = nResultCount % this.PageMaxLines;
            if (nDelta > 0)
                return (nResultCount / this.PageMaxLines) + 1;
            return (nResultCount / this.PageMaxLines);
        }

        // 条码号列表
        public List<string> ItemBarcodes
        {
            get
            {
                object o = this.Page.Session[this.ID + "ItemsControl_ItemBarcodes"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session[this.ID + "ItemsControl_ItemBarcodes"] = (object)value;
            }
        }

        // 设置结果集有关数量参数
        public void SetResultInfo(int nResultCount)
        {
            int nPageNo = this.StartIndex / this.PageMaxLines;

            int nPageCount = GetPageCount(nResultCount);

            LiteralControl resultinfo = (LiteralControl)this.FindControl("resultinfo");

            if (nResultCount != 0)
            {
                resultinfo.Text = string.Format(this.GetString("册共n个"),  // "册共 {0} 个"
                    nResultCount.ToString());
                // "册共 " + Convert.ToString(nResultCount) + " 个";
            }
            else
            {
                resultinfo.Text = this.GetString("无册");   // "(无册)"
                // "(无册)";
            }

            /*
            if (nResultCount != 0)  // 2007/8/24 new changed
                resultinfo.Text = "册共 " + Convert.ToString(nResultCount) + " 个, 分 " + Convert.ToString(nPageCount) + " 页显示, 当前为第 " + Convert.ToString(nPageNo + 1) + "页。";
            else
                resultinfo.Text = "(无册)";
            */
#if NO
            LiteralControl maxpagecount = (LiteralControl)this.FindControl("maxpagecount");
            maxpagecount.Text = string.Format(this.GetString("共n页"),  // (共 {0} 页)
                nPageCount.ToString());
                // " (共 " + Convert.ToString(nPageCount) + " 页)";

            LiteralControl currentpageno = (LiteralControl)this.FindControl("currentpageno");
            currentpageno.Text = Convert.ToString(nPageNo + 1);
#endif

            PageSwitcherControl pager = (PageSwitcherControl)this.FindControl("pager");
            pager.CurrentPageNo = nPageNo;
            pager.TotalCount = nPageCount;
            if (nPageCount <= 1)
            {
                pager.Visible = false;
            }
            else
            {
                pager.Visible = true;
            }

            PlaceHolder pageswitcher = (PlaceHolder)this.FindControl("pageswitcher");
            if (nPageCount <= 1)
            {
                pageswitcher.Visible = false;
                resultinfo.Visible = false; // 2009/6/10
            }
            else
            {
                pageswitcher.Visible = true;
                resultinfo.Visible = true;
            }
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='debugline'><td colspan='13'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            text.Text = strText;
        }

        void SetDebugInfo(string strSpanClass,
            string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            if (strSpanClass == "errorinfo")
                text.Text = "<div class='errorinfo-frame'><div class='" + strSpanClass + "'>" + strText + "</div></div>";
            else
                text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }

        public int ResultCount
        {
            get
            {
                return GetResultCount();
            }
            set
            {
                SetResultCount(value);
            }
        }

        void SetResultCount(int nResultCount)
        {
            // PlaceHolder line = (PlaceHolder)FindControl("debugline");
            HiddenField hidden = (HiddenField)this.FindControl("resultcount");
            hidden.Value = nResultCount.ToString();
        }

        int GetResultCount()
        {
            // PlaceHolder line = (PlaceHolder)FindControl("debugline");
            HiddenField hidden = (HiddenField)this.FindControl("resultcount");
            if (String.IsNullOrEmpty(hidden.Value) == true)
                return 0;

            return Convert.ToInt32(hidden.Value);
        }

        void CreateCmdLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<tr class='cmdline'><td colspan='13'><div class='reservation'>"));

            // 读者证条码号和前面文字一体的PlaceHolder,便于一起显示和隐藏
            PlaceHolder reservationreaderbarcode_holder = new PlaceHolder();
            reservationreaderbarcode_holder.ID = "reservationreaderbarcode_holder";
            line.Controls.Add(reservationreaderbarcode_holder);

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("针对读者");    //  "针对读者(证条码号)";
            reservationreaderbarcode_holder.Controls.Add(literal);

            TextBox reservationreaderbarcode = new TextBox();
            reservationreaderbarcode.Text = "";
            reservationreaderbarcode.ID = "reservationreaderbarcode";
            reservationreaderbarcode_holder.Controls.Add(reservationreaderbarcode);

            Button reservationbutton = new Button();
            reservationbutton.ID = "reservationbutton";
            reservationbutton.Text = this.GetString("加入预约列表");
            reservationbutton.Click += new EventHandler(reservationbutton_Click);
            line.Controls.Add(reservationbutton);

            line.Controls.Add(new LiteralControl(
    "</div>"));

            ///

            PlaceHolder pageswitcher = new PlaceHolder();
            pageswitcher.ID = "pageswitcher";
            line.Controls.Add(pageswitcher);

            // 信息文字

            pageswitcher.Controls.Add(new LiteralControl(
                "<div class='pager'> -- "
            ));

            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            pageswitcher.Controls.Add(resultinfo);

            pageswitcher.Controls.Add(new LiteralControl(
                " -- "
            ));

            PageSwitcherControl pager = new PageSwitcherControl();
            pager.Wrapper = false;  // 不要外围的<div>
            pager.ID = "pager";
            pager.PageSwitch += new PageSwitchEventHandler(pager_PageSwitch);
            pageswitcher.Controls.Add(pager);

            pageswitcher.Controls.Add(new LiteralControl(
    "</div>"));

#if NO
            LinkButton firstpage = new LinkButton();
            firstpage.ID = "first";
            firstpage.Text = this.GetString("首页");
            firstpage.CssClass = "firstpage";
            firstpage.Click += new EventHandler(firstpage_Click);
            pageswitcher.Controls.Add(firstpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton prevpage = new LinkButton();
            prevpage.ID = "prev";
            prevpage.Text = this.GetString("前页");
            prevpage.CssClass = "prevpage";
            prevpage.Click += new EventHandler(prevpage_Click);
            pageswitcher.Controls.Add(prevpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            literal = new LiteralControl();
            literal.ID = "currentpageno";
            literal.Text = "";
            pageswitcher.Controls.Add(literal);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));


            LinkButton nextpage = new LinkButton();
            nextpage.ID = "next";
            nextpage.Text = this.GetString("后页");
            nextpage.CssClass = "nextpage";
            nextpage.Click += new EventHandler(nextpage_Click);
            pageswitcher.Controls.Add(nextpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton lastpage = new LinkButton();
            lastpage.ID = "last";
            lastpage.Text = this.GetString("末页");
            lastpage.CssClass = "lastpage";
            lastpage.Click += new EventHandler(lastpage_Click);
            pageswitcher.Controls.Add(lastpage);

            literal = new LiteralControl();
            literal.Text = "  |  ";
            pageswitcher.Controls.Add(literal);

            Button gotobutton = new Button();
            gotobutton.ID = "gotobutton";
            gotobutton.Text = this.GetString("跳到");
            gotobutton.CssClass = "goto";
            gotobutton.Click += new EventHandler(gotobutton_Click);
            pageswitcher.Controls.Add(gotobutton);

            literal = new LiteralControl();
            literal.Text = " " + this.GetString("第") + " ";    //  " 第 ";
            pageswitcher.Controls.Add(literal);


            TextBox textbox = new TextBox();
            textbox.ID = "gotopageno";
            textbox.Width = new Unit("40");
            textbox.CssClass = "gotopageno";
            pageswitcher.Controls.Add(textbox);

            /*
            literal = new LiteralControl();
            literal.Text = " 页";
            pageswitcher.Controls.Add(literal);
             * */

            literal = new LiteralControl();
            literal.ID = "maxpagecount";
            literal.Text = " (共 " + Convert.ToString(1/*this.PageCount*/) + " 页)";
            pageswitcher.Controls.Add(literal);

            ///
#endif

            line.Controls.Add(new LiteralControl("</td></tr>"));

        }

        void pager_PageSwitch(object sender, PageSwitchEventArgs e)
        {
            this.StartIndex = this.PageMaxLines * e.GotoPageNo;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        // 跳到指定的页号
        void gotobutton_Click(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)this.FindControl("gotopageno");

            int nPageNo = 0;

            try
            {
                nPageNo = Convert.ToInt32(textbox.Text);
            }
            catch
            {
                return;
            }

            if (nPageNo < 1)
                nPageNo = 1;
            this.StartIndex = this.PageMaxLines * (nPageNo - 1);

            // TODO: 放到Render()中检查
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        void lastpage_Click(object sender, EventArgs e)
        {
            int delta = this.ResultCount % this.PageMaxLines;
            if (delta > 0)
                this.StartIndex = (this.ResultCount / this.PageMaxLines) * this.PageMaxLines;
            else
                this.StartIndex = Math.Max(0, (this.ResultCount / this.PageMaxLines) * this.PageMaxLines - 1);
            /*
            this.StartIndex = -1;   // 暗号
             * */
        }

        void nextpage_Click(object sender, EventArgs e)
        {
            this.StartIndex += this.PageMaxLines;

            // TODO: 放到Render()中检查
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        void prevpage_Click(object sender, EventArgs e)
        {
            this.StartIndex -= this.PageMaxLines;
            if (this.StartIndex < 0)
                this.StartIndex = 0;
        }

        void firstpage_Click(object sender, EventArgs e)
        {
            this.StartIndex = 0;
        }

        void reservationbutton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = "";

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            // 如果尚未登录
            if (loginstate == LoginState.NotLogin)
            {
                this.SetDebugInfo("errorinfo", this.GetString("尚未登录，不能使用预约功能"));    // "尚未登录，不能使用预约功能"
                return;
                /*
                this.Page.Response.Redirect("login.aspx");
                this.Page.Response.End();
                 * */
            }

            // 如果是访客身份
            if (loginstate == LoginState.Public)
            {
                this.SetDebugInfo("errorinfo", this.GetString("当前为访客身份，不能使用预约功能"));
                return;
            }

            for (int i = 0; i < this.LineCount; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("line" + Convert.ToString(i) + "checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.ItemBarcodes.Count <= i)
                    {
                        this.SetDebugInfo("errorinfo", "ItemBarcodes失效...");
                        return;
                    }
                    string strBarcode = this.ItemBarcodes[i];

                    if (strBarcodeList != "")
                        strBarcodeList += ",";
                    strBarcodeList += strBarcode;
                    checkbox.Checked = false;
                }
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            Debug.Assert(String.IsNullOrEmpty(sessioninfo.UserID) == false, "");

            // 获得读者证条码号
            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(sessioninfo.UserID) == false
                && sessioninfo.IsReader == true
                && sessioninfo.ReaderInfo != null)
            {
                // 如果为一般读者身份，帐户信息中具有证条码号
                strReaderBarcode = sessioninfo.ReaderInfo.Barcode;
            }
            else
            {
                // 否则从textbox中取得当时输入的证条码号

                TextBox reservationreaderbarcode = (TextBox)this.FindControl("reservationreaderbarcode");
                Debug.Assert(reservationreaderbarcode != null, "");

                strReaderBarcode = reservationreaderbarcode.Text;
            }

            if (strReaderBarcode == "")
            {
                SetDebugInfo("errorinfo", this.GetString("尚未指定读者证条码号, 无法进行预约操作")); // "尚未指定读者证条码号, 无法进行预约操作。"
                return;
            }

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string strError = "";
                long lRet = // sessioninfo.Channel.
                    channel.Reservation(null,
                    "new",
                    strReaderBarcode,
                    strBarcodeList,
                    out strError);
                if (lRet == -1)
                    SetDebugInfo("errorinfo", strError);
                else
                {
                    string strMessage = this.GetString("预约成功");   // "预约成功。请看“<a href='./reservationinfo.aspx'>预约</a>”中的新增信息。";

                    // 成功时也可能有提示信息
                    if (String.IsNullOrEmpty(strError) == false)
                        strMessage += "<br/><br/>" + strError;

                    SetDebugInfo(strMessage);
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            // 清除读者记录缓存
            sessioninfo.ClearLoginReaderDomCache();
        }

#if NO
        // 创建标题行
        void CreateTitleLine(PlaceHolder line)
        {
            line.Controls.Clear();

            string strText = "";

            strText += "<tr class='columntitle'>";

            strText += "<td nowrap class='no'>"
                + this.GetString("册序号")
                + "</td>";

            strText += "<td nowrap class='barcode'>"
                + this.GetString("册条码号")
                + "</td>";

            strText += "<td nowrap class='state'>"
                + this.GetString("状态")
                + "</td>";

            strText += "<td nowrap class='location'>"
                + this.GetString("馆藏地")
                + "</td>";

            strText += "<td nowrap class='accessNo'>"
                + this.GetString("索取号")
                + "</td>";    // 2009/4/7

            strText += "<td nowrap class='publishTime'>"
                + this.GetString("出版日期")
                + "</td>";    // 2010/4/22

            strText += "<td nowrap class='volume'>"
                + this.GetString("卷期")
                + "</td>";    // 2009/4/7

            strText += "<td nowrap class='price'>"
                + this.GetString("价格")
                + "</td>";

            strText += "<td nowrap class='comment'>"
                + this.GetString("注释")
                + "</td>";

            /*
            strText += "<td nowrap>借者证条码号</td>";

            strText += "<td nowrap>续借次</td>";
             * */

            strText += "<td nowrap class='borrows'>"
                + this.GetString("借阅情况")
                + "</td>";

            /*
            strText += "<td nowrap>借阅期限</td>";

            strText += "<td nowrap>超期情况</td>";
             * */

            strText += "<td nowrap class='reservations'>"
                + this.GetString("预约情况")
                + "</td>";

            /*
            strText += "<td nowrap>种id</td>";
             * */

            strText += "</tr>";

            line.Controls.Add(new LiteralControl(strText));
        }
#endif

        // 创建标题行
        // 根据配置对某些列进行了隐藏
        void CreateTitleLine(PlaceHolder line,
            List<string> hideColumns)
        {
            line.Controls.Clear();

            string strText = "";

            strText += "<tr class='columntitle'>";

            strText += "<td nowrap class='no'>"
                + this.GetString("册序号")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("barcode") == -1)
                strText += "<td nowrap class='barcode'>"
                + this.GetString("册条码号")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("state") == -1)
                strText += "<td nowrap class='state'>"
                + this.GetString("状态")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("location") == -1)
                strText += "<td nowrap class='location'>"
                + this.GetString("馆藏地")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("accessNo") == -1)
                strText += "<td nowrap class='accessNo'>"
                + this.GetString("索取号")
                + "</td>";    // 2009/4/7

            if (hideColumns == null
                || hideColumns.IndexOf("publishTime") == -1)
                strText += "<td nowrap class='publishTime'>"
                + this.GetString("出版日期")
                + "</td>";    // 2010/4/22

            if (hideColumns == null
                || hideColumns.IndexOf("volume") == -1)
                strText += "<td nowrap class='volume'>"
                + this.GetString("卷期")
                + "</td>";    // 2009/4/7

            if (hideColumns == null
                || hideColumns.IndexOf("price") == -1)
                strText += "<td nowrap class='price'>"
                + this.GetString("价格")
                + "</td>";

            if (hideColumns == null
               || hideColumns.IndexOf("comment") == -1)
                strText += "<td nowrap class='comment'>"
                + this.GetString("注释")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("borrows") == -1)
                strText += "<td nowrap class='borrows'>"
                + this.GetString("借阅情况")
                + "</td>";

            if (hideColumns == null
                || hideColumns.IndexOf("reservations") == -1)
                strText += "<td nowrap class='reservations'>"
                + this.GetString("预约情况")
                + "</td>";

            if (hideColumns == null
    || hideColumns.IndexOf("borrowcount") == -1)
                strText += "<td nowrap class='borrowcount'>"
                + this.GetString("流通次数")
                + "</td>";


            strText += "</tr>";

            line.Controls.Add(new LiteralControl(strText));
        }

        PlaceHolder NewLine(int index,
            Control insertbefore)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + Convert.ToString(index);

            if (insertbefore == null)
                this.Controls.Add(line);
            else
            {
                int pos = this.Controls.IndexOf(insertbefore);
                this.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(index) + "left";
            literal.Text = "<tr class='content'><td>";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);

            return line;
        }

        public bool Active
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active");
                if (String.IsNullOrEmpty(s.Value) == true)
                    return false;
                if (s.Value == "0")
                    return false;
                return true;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active");
                s.Value = value == true ? "1" : "0";
            }
        }

        // 布局控件
        protected override void CreateChildControls()
        {
            // 表格开头

            if (this.Wrapper == true)
            {
                this.Controls.Add(new AutoIndentLiteral(
                    "<%begin%>"
                    + this.GetPrefixString(
                    this.GetString("册信息"),
                    "content_wrapper")));
            }

            string strClass = "items";
            if (this.Wrapper == true)
                strClass += " wrapper";

            //  width='100%' cellspacing='1' cellpadding='4'
            this.Controls.Add(new AutoIndentLiteral("<%begin%><table class='" + strClass + "'>"));

            HiddenField active = new HiddenField();
            active.ID = "active";
            active.Value = "1";
            this.Controls.Add(active);

            /*
            // 信息文字
            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            this.Controls.Add(resultinfo);
             * */

            // 期刊封面图片区
            PlaceHolder coverline = new PlaceHolder();
            coverline.ID = "coverline";
            this.Controls.Add(coverline);

            // 标题行
            PlaceHolder titleline = new PlaceHolder();
            titleline.ID = "titleline";
            this.Controls.Add(titleline);

            CreateTitleLine(titleline, null);

            // 每一行一个占位控件
            for (int i = 0; i < this.LineCount; i++)
            {
                PlaceHolder line = NewLine(i, null);
                line.Visible = false;
            }

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "cmdline";
            this.Controls.Add(cmdline);

            CreateCmdLine(cmdline);

            // 隐藏字段，用来转值
            HiddenField hidden = new HiddenField();
            hidden.ID = "resultcount";
            this.Controls.Add(hidden);


            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            debugline.Visible = false;
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);


            // 表格结尾
            /*
            literal = new LiteralControl();
            literal.ID = "end";
            literal.Text = "</table>";
             * */

            this.Controls.Add(new AutoIndentLiteral("<%end%></table>"));

            if (this.Wrapper == true)
            {
                this.Controls.Add(new AutoIndentLiteral("<%end%>" + this.GetPostfixString()));
            }
        }

        /*
         * Barcode和BiblioRecPath是否具有值, 分为3种情况
         * 1)Barcode和BiblioRecPath都有值。
         *      这通常表示需要通过种记录路径获得全部册显示出来，把特定的barcode对应行加亮显示
         *      如果此时ItemDispStyle为Item，则不好理解，这时只需指定Barcode，而Biblio不需要有值
         * 2)Barcode有值
         *      这需要先获得册记录，然后从册记录<parent>中获得种记录的id
         * 3)BiblioRecPath有值
         *      这表示需要显示同种的全部册
         */

        protected override void Render(HtmlTextWriter output)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.Barcode) == true
                && String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "Barcode和BiblioRecPath均为空, 无法显示";
                goto ERROR1;
            }

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            /*
            if (sessioninfo.Account == null)
            {
                //strError = "尚未登录";
                //goto ERROR1;
                this.Page.Response.Redirect("login.aspx");
                this.Page.Response.End();
                return;
            }
             * */

            // 设置 预约按钮文字和状态
            Button reservationbutton = (Button)this.FindControl("reservationbutton");
            /*
            if (sessioninfo.Account == null
                || (sessioninfo.Account != null && sessioninfo.Account.UserID == "public"))    // 2006/12/24
             * */
            if (loginstate == LoginState.NotLogin
                || loginstate == LoginState.Public)    // 2007/7/10
            {
                reservationbutton.Text = this.GetString("加入预约列表");
                reservationbutton.Enabled = false;
            }
            else
            {
                // Debug.Assert(sessioninfo.ReaderInfo != null);
                if (sessioninfo.ReaderInfo == null
                    || String.IsNullOrEmpty(sessioninfo.ReaderInfo.Name) == true)
                {
                    // 没有姓名
                    reservationbutton.Text = this.GetString("加入预约列表");
                }
                else
                {
                    // 有姓名
                    reservationbutton.Text = string.Format(
                        this.GetString("加入someone的预约列表"), // "加入 {0} 的预约列表"
                        sessioninfo.ReaderInfo.Name);
                    // "加入 " + sessioninfo.Account.Name + " 的预约列表";
                }

                reservationbutton.Enabled = true;
            }

            PlaceHolder reservationreaderbarcode_holder = (PlaceHolder)this.FindControl("reservationreaderbarcode_holder");

            /*
            if (sessioninfo.Account == null // 2007/7/10
                || sessioninfo.Account.Barcode != ""
                || sessioninfo.Account.UserID == "public")    // 2006/12/24
             * */
            if (loginstate == LoginState.Librarian)
            {
                // 图书馆员才能动态指定读者证条码号
                reservationreaderbarcode_holder.Visible = true;

                // 放入可能的内容 2008/9/27
                if (sessioninfo.ReaderInfo != null
                    && String.IsNullOrEmpty(sessioninfo.ReaderInfo.ReaderDomBarcode) == false)
                {
                    TextBox reservationreaderbarcode = (TextBox)this.FindControl("reservationreaderbarcode");
                    Debug.Assert(reservationreaderbarcode != null, "");

                    reservationreaderbarcode.Text = sessioninfo.ReaderInfo.ReaderDomBarcode;
                }
            }
            else
            {
                reservationreaderbarcode_holder.Visible = false;
            }

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string strBiblioRecPath = this.BiblioRecPath;

                string strOutputItemPath = "";
                string strItemXml = "";
                byte[] item_timestamp = null;
                // 如果this.BiblioRecPath为空, 并且要求显示同种全部册
                // 那只能通过this.Barcode取出一个册记录, 从中才能得知种记录的路径
                if (String.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    bool bGetItemXml = true;
                    if ((this.ItemDispStyle & ItemDispStyle.Items) == ItemDispStyle.Items)
                        bGetItemXml = false;

                    string strBiblio = "";

                    long lRet = // sessioninfo.Channel.
                        channel.GetItemInfo(
                    null,
                    this.Barcode,
                    (bGetItemXml == true) ? "xml" : "", // strResultType
                    out strItemXml,
                    out strOutputItemPath,
                    out item_timestamp,
                    "recpath",  // strBiblioType
                    out strBiblio,
                    out strBiblioRecPath,
                    out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    if (lRet == 0)
                    {
                        strError = "册条码号 '" + this.Barcode + "' 没有找到";
                        goto ERROR1;
                    }

                    if (lRet > 1)
                    {
                        strError = "册条码号 '" + this.Barcode + "' 命中 " + nRet.ToString() + " 条记录";
                        goto ERROR1;
                    }

                    Debug.Assert(string.IsNullOrEmpty(strBiblioRecPath) == false, "");
                    this.BiblioRecPath = strBiblioRecPath;
                }

                if ((this.ItemDispStyle & ItemDispStyle.Items) == ItemDispStyle.Items)
                {
                    string strLibraryCode = (string)this.Page.Session["librarycode"];

                    // 检索出该种的所有册
#if NO
                    sessioninfo.ItemLoad += new ItemLoadEventHandler(SessionInfo_ItemLoad);
#endif
                    tempItemBarcodes = new List<string>();
                    //                 tempOutput = "";
                    // tempOutput = output;
                    try
                    {
                        // return:
                        //      -2  实体库没有定义
                        //      -1  出错
                        //      其他  命中的全部结果数量。
                        nRet = OpacSearchItems(
                            app,
                            channel,
                            SessionInfo_ItemLoad,
                            strBiblioRecPath,
                            this.StartIndex,
                            this.PageMaxLines,
                            this.Lang,
                            strLibraryCode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "sessioninfo.SearchItems() error :" + strError;
                            goto ERROR1;
                        }
                        if (nRet == -2)
                        {
                            this.Visible = false;
                            return;
                        }
                    }
                    finally
                    {
#if NO
                        sessioninfo.ItemLoad -= new ItemLoadEventHandler(SessionInfo_ItemLoad);
#endif
                    }

                    this.ItemBarcodes = this.tempItemBarcodes;

                    this.ResultCount = nRet;    // 2009/6/9 add

                    SetResultInfo(nRet);

                    if (nRet == 0)
                    {
                        // 如果一册也没有, 则不出现命令按钮
                        /*
                        Button button = (Button)this.FindControl("reservationbutton");
                        if (button != null)
                            button.Visible = false;
                         */
                        PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
                        if (cmdline != null)
                            cmdline.Visible = false;

                        this.SetDebugInfo("none", this.GetString("无馆藏"));  // "(无馆藏)"

                        // 不显示空的表格
                        if (this.DisplayBlankTable == false)
                        {
                            this.Visible = false;
                            return;
                        }
                    }
                }
                else if ((this.ItemDispStyle & ItemDispStyle.Item) == ItemDispStyle.Item)
                {
                    if (strItemXml == "")
                        throw new Exception("册记录尚未准备好");

                    this.tempItemBarcodes = new List<string>();

                    ItemLoadEventArgs e = new ItemLoadEventArgs();
                    e.Index = 0;
                    e.Path = strOutputItemPath;
                    e.Count = 1;
                    e.Xml = strItemXml;
                    e.Timestamp = item_timestamp;
                    SessionInfo_ItemLoad(this, e);

                    this.ItemBarcodes = this.tempItemBarcodes;
                }

                FillCoverImage(channel);

                if (String.IsNullOrEmpty(this.WarningText) == false)
                    SetDebugInfo(this.WarningText);

                if (this.Active == false)
                    reservationbutton.Enabled = false;

                base.Render(output);
                return;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            ERROR1:
            // output.Write(strError);
            // 2011/4/21
            this.SetDebugInfo("errorinfo", strError);
            base.Render(output);
        }

        // 检索出册数据
        // 带有偏移量的版本
        // 2009/6/9 
        // return:
        //      -2  实体库没有定义
        //      -1  出错
        //      其他  命中的全部结果数量。
        //
        public static int OpacSearchItems(
            OpacApplication app,
            LibraryChannel channel,
            ItemLoadEventHandler itemLoadProc,
            string strBiblioRecPath,
            int nStart,
            int nMaxCount,
            string strLang,
            string strLibraryCode,
            out string strError)
        {
            strError = "";
            // string strXml = "";

            // LibraryChannel channel = this.GetChannel(true, this.m_strParameters);
            try
            {
                string strStyle = "opac";
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    strStyle += ",getotherlibraryitem";
                else
                    strStyle += ",librarycode:" + strLibraryCode;

                long lStart = nStart;
                long lCount = nMaxCount;
                long lTotalCount = 0;
                for (; ; )
                {
                    EntityInfo[] iteminfos = null;
                    long lRet = // this.Channel.
                        channel.GetEntities(
                        null,
                        strBiblioRecPath,
                        lStart,
                        lCount,
                        strStyle,
                        strLang,
                        out iteminfos,
                        out strError);
                    if (lRet == -1)
                    {
                        if (//this.Channel.
                            channel.ErrorCode == ErrorCode.ItemDbNotDef)
                            return -2;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        strError = "没有找到";
                        return 0;
                    }

                    lTotalCount = lRet;

                    if (lCount < 0)
                        lCount = lTotalCount - lStart;

                    if (lStart + lCount > lTotalCount)
                        lCount = lTotalCount - lStart;

                    // 处理
                    for (int i = 0; i < iteminfos.Length; i++)
                    {
                        EntityInfo info = iteminfos[i];

                        if (//this.ItemLoad != null
                            itemLoadProc != null)
                        {
                            ItemLoadEventArgs e = new ItemLoadEventArgs();
                            e.Path = info.OldRecPath;
                            e.Index = i;    // +nStart;
                            e.Count = nMaxCount;    // (int)lTotalCount - nStart;
                            e.Timestamp = info.OldTimestamp;
                            e.Xml = info.OldRecord;

                            //this.ItemLoad(this, e);
                            itemLoadProc(null, e);
                        }
                    }

                    lStart += iteminfos.Length;
                    lCount -= iteminfos.Length;

                    if (lStart >= lTotalCount)
                        break;
                    if (lCount <= 0)
                        break;
                }

                return (int)lTotalCount;
            }
            finally
            {
                // this.ReturnChannel(channel);
            }
        }

        string GetIssueDbName(
            OpacApplication app,
            string strItemRecPath)
        {
            string strError = "";
            string strItemDbName = StringUtil.GetDbName(strItemRecPath);
            string strBiblioDbName = "";

            // 根据实体库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = app.GetBiblioDbNameByItemDbName(strItemDbName,
                out strBiblioDbName,
                out strError);
            if (nRet != 1)
                return "";

            string strIssueDbName = "";
            // 根据书目库名, 找到对应的期库名
            // return:
            //      -1  出错
            //      0   没有找到(书目库)
            //      1   找到
            nRet = app.GetIssueDbName(strBiblioDbName,
                out strIssueDbName,
                out strError);
            if (nRet == 1)
                return strIssueDbName;
            return "";
        }

        void FillCoverImage(LibraryChannel channel)
        {
            if (_issue_query_strings.Count == 0)
                return;
            PlaceHolder line = (PlaceHolder)this.FindControl("coverline");
            if (line == null)
                return;

            string displayBlank = this.DisplayBlankIssueCover;  // 缺省 "displayBlank,hideWhenAllBlank"
            string strPrefferSize = this.IssueCoverSize;    //  "MediumImage"; // "LargeImage",

            int nNotBlankCount = 0;
            StringBuilder text = new StringBuilder();
            foreach (IssueString s in _issue_query_strings)
            {
                string strUri = "";
                string strError = "";

                // 获得指定一期的封面图片 URI
                // parameters:
                //      strBiblioPath   书目记录路径
                //      strQueryString  检索词。例如 “2005|1|1000|50”。格式为 年|期号|总期号|卷号。一般为 年|期号| 即可。
                int nRet = channel.GetIssueCoverImageUri(null,
                    this.BiblioRecPath,
                    s.Query,
                    strPrefferSize,
                    out strUri,
                    out strError);
                if (nRet == -1)
                {
                    strError = "(用户 '" + channel.UserName + "') " + strError;
                    text.Append("<div>" + HttpUtility.HtmlEncode(strError) + "</div>");
                    continue;
                }

                OpacApplication app = (OpacApplication)this.Page.Application["app"];

                string strUrl = "";

                if (string.IsNullOrEmpty(strUri))
                {
                    if (StringUtil.IsInList("displayBlank", displayBlank) == false)
                        continue;

                    if (strPrefferSize == "LargeImage")
                        strUrl = MyWebPage.GetStylePath(app, "blankcover_large.png");
                    else
                        strUrl = MyWebPage.GetStylePath(app, "blankcover_medium.png");
                }
                else
                {
                    strUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strUri);
                    nNotBlankCount++;
                }

                text.Append("<div class='issue_cover' ><img src='" + strUrl + "' alt='封面图像' ></img><div class='issue_no'>" + HttpUtility.HtmlEncode(s.Volume) + "</div></div>");
            }

            if (StringUtil.IsInList("hideWhenAllBlank", displayBlank) == true
                && nNotBlankCount == 0)
            {

            }
            else
            {
                LiteralControl literal = new LiteralControl();
                literal.ID = "";
                literal.Text = "<div class='issue_cover_frame' >" + text.ToString() + "</div>";
                line.Controls.Add(literal);
            }

            _issue_query_strings.Clear();
        }

        List<string> m_hidecolumns = null;

        void SessionInfo_ItemLoad(object sender, ItemLoadEventArgs e)
        {
            int nRet = 0;
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            bool bAllowReservation = true;  // 尚未登录、访客身份等原因导致的允许和不允许使用预约checkbox状态
            string strAllowReservationReason = ""; // 原因字符串，用于tooltips提示

            if (loginstate == LoginState.NotLogin)
            {
                bAllowReservation = false;
                strAllowReservationReason = "当前尚未登录";
            }
            else if (loginstate == LoginState.Public)
            {
                bAllowReservation = false;
                strAllowReservationReason = "当前为访客身份";
            }

            // 返回前被过滤掉的记录
            if (string.IsNullOrEmpty(e.Xml) == true)
                return;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(e.Xml);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(e.Index));
            if (line == null)
            {
                PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");
                line = NewLine(e.Index, cmdline);
                this.LineCount++;
            }
            line.Visible = true;

            LiteralControl left = (LiteralControl)line.FindControl("line" + Convert.ToString(e.Index) + "left");
            CheckBox checkbox = (CheckBox)line.FindControl("line" + Convert.ToString(e.Index) + "checkbox");
            LiteralControl right = (LiteralControl)line.FindControl("line" + Convert.ToString(e.Index) + "right");

            string strResult = "";

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            // string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");

            // 2015/4/3
            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strBarcode) == true)
                strBarcode = "@refID:" + strRefID;

            this.tempItemBarcodes.Add(strBarcode);

            if (this.m_hidecolumns == null)
            {

                string strItemDbName = StringUtil.GetDbName(e.Path);
                string strBiblioDbName = "";

                // 根据实体库名, 找到对应的书目库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = app.GetBiblioDbNameByItemDbName(strItemDbName,
                    out strBiblioDbName,
                    out strError);

                if (string.IsNullOrEmpty(strBiblioDbName) == false)
                {
                    this.m_hidecolumns = GetHideColumns(app,
                         strBiblioDbName);

                    // 重新设置栏目标题行
                    if (this.m_hidecolumns != null && this.m_hidecolumns.Count > 0)
                    {
                        PlaceHolder titleline = (PlaceHolder)this.FindControl("titleline");

                        CreateTitleLine(titleline,
    this.m_hidecolumns);
                    }
                }
                else
                    this.m_hidecolumns = new List<string>();
            }

            /*
            string strColor = "bgcolor=#ffffff";
            if (strBarcode == this.Barcode)
            {
                strColor = "bgcolor=#bbbbff";	// 当前正在借阅的册
            }
             */
            string strClass = "content";
            string strAnchor = "";
            if ((strBarcode == this.Barcode
                && String.IsNullOrEmpty(strBarcode) == false// 排除空的条码号 2009/8/4 changed
                )
                || e.Path == this.FocusRecPath)
            {
                strClass = "content active";    // 2009/8/4 changed
                strAnchor = "<a name='active'></a>";
            }

            // 是否和合订成员册?
            bool bMember = false;
            XmlNode nodeBindingParent = dom.DocumentElement.SelectSingleNode("binding/bindingParent");
            if (nodeBindingParent != null)
            {
                bMember = true;
                strClass += " bindingmember";
            }

            // 状态
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            // 2015/1/9
            if (StringUtil.IsInList("注销", strState) == true)
                strClass += " absolute";
            if (StringUtil.IsInList("加工中", strState) == true)
                strClass += " repairing";

            strResult += "\r\n<tr class='" + strClass + "' ><td class='no'>" + strAnchor;

            // 左
            left.Text = strResult;

            // checkbox
            checkbox.Text = Convert.ToString(e.Index + this.StartIndex + 1);


            // 馆藏地点
            string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");

#if NO
            bool bResultValue = false;
            string strMessageText = "";

            // 执行脚本函数ItemCanBorrow
            // parameters:
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = app.DoItemCanBorrowScriptFunction(
                false,
                sessioninfo.Account,
                dom,
                out bResultValue,
                out strMessageText,
                out strError);
            if (nRet == -1)
            {
                strMessageText = strError;
            }

            if (nRet == -2)
            {
                // 根据馆藏地点是否允许借阅, 设置checkbox状态
                List<string> locations = app.GetLocationTypes(true);
                if (locations.IndexOf(strLocation) == -1)
                {
                    checkbox.Enabled = false;
                    checkbox.ToolTip = "此册因属馆藏地点 " + strLocation + " 而不能外借。";
                }
                else
                {
                    checkbox.Enabled = bAllowReservation;
                    if (bAllowReservation == false)
                    {
                        checkbox.ToolTip = "此册因" + strAllowReservationReason + "而无法进行预约勾选操作。";
                    }
                }
            }
            else
            {
                // 根据脚本加以设置
                checkbox.Enabled = bAllowReservation == true ? bResultValue : false;
                if (bAllowReservation == false)
                {
                    checkbox.ToolTip = "此册因" + strAllowReservationReason + "而无法进行预约勾选操作。";
                }
            }
#endif
            XmlNode nodeCanBorrow = dom.DocumentElement.SelectSingleNode("canBorrow");
            string strCanBorrowText = "";
            bool bCanBorrow = true;
            if (nodeCanBorrow != null)
            {
                strCanBorrowText = nodeCanBorrow.InnerText;
                DomUtil.GetBooleanParam(nodeCanBorrow,
                    "canBorrow",
                    true,
                    out bCanBorrow,
                    out strError);
            }

            // 根据馆藏地点是否允许借阅, 设置checkbox状态
            if (bCanBorrow == false)
            {
                checkbox.Enabled = false;
                checkbox.ToolTip = strCanBorrowText;
            }
            else
            {
                checkbox.Enabled = bAllowReservation;
                if (bAllowReservation == false)
                {
                    checkbox.ToolTip = "此册因" + strAllowReservationReason + "而无法进行预约勾选操作。";
                }
            }

            // 右开始
            strResult = "</td>";

            // 条码号
            if (this.m_hidecolumns.IndexOf("barcode") == -1)
                strResult += "<td class='barcode'>" + strBarcode + "</td>";

            // 状态
            // string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

            // strState = strMessageText + strState;
#if NO
            string strStateMessage = DomUtil.GetElementText(dom.DocumentElement,
                "stateMessage");
            if (String.IsNullOrEmpty(strStateMessage) == false)
                strState = strStateMessage;
#endif
            if (bCanBorrow == false)
                strState += "[不可借]";

            if (bMember == true)
                StringUtil.SetInList(ref strState, "已装入合订册", true);

            if (this.m_hidecolumns.IndexOf("state") == -1)
            {
                string strFragment = (strState == "" ? "&nbsp;" : strState);
                if (string.IsNullOrEmpty(strCanBorrowText) == false)
                    strFragment = "<a title='" + HttpUtility.HtmlEncode(strCanBorrowText) + "'>" + strFragment + "</a>";
                strResult += "<td class='state'>" + strFragment + "</td>";
            }

            // 馆藏地

            string strLocationCaption = "";
            string strLocationUrl = "";
            string strDisplayText = "";
            // 获得馆藏地点的显示字符串
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetLocationDisplayString(
                this.Lang,  // "zh",
                strLocation,
                out strLocationCaption,
                out strLocationUrl,
                out strError);
            if (nRet == -1)
                strDisplayText = strError;
            else
            {
                if (String.IsNullOrEmpty(strLocationUrl) == false)
                {
                    strDisplayText = "<a href='" + strLocationUrl + "'>" + strLocationCaption + "</a>";
                }
                else
                {
                    strDisplayText = strLocationCaption;
                }
            }

            if (this.m_hidecolumns.IndexOf("location") == -1)
                strResult += "<td class='location'>" + (strDisplayText == "" ? "&nbsp;" : strDisplayText) + "</td>";

            // 索取号
            // 2009/4/7
            string strAccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
            if (this.m_hidecolumns.IndexOf("accessNo") == -1)
            {
                string strAccessNoText = "";
                string strAccessNoUrl = "./location.aspx?location="
                    + HttpUtility.UrlEncode(strLocation)
                    + "&accessNo=" + HttpUtility.UrlEncode(strAccessNo);
                if (String.IsNullOrEmpty(strAccessNoUrl) == false)
                {
                    strAccessNoText = "<a href='" + strAccessNoUrl + "'>"
                        + HttpUtility.HtmlEncode(StringUtil.GetPlainTextCallNumber(strAccessNo))
                        + "</a>";
                }
                else
                {
                    strAccessNoText = HttpUtility.HtmlEncode(StringUtil.GetPlainTextCallNumber(strAccessNo));
                }

                strResult += "<td class='accessNo'>" + (strAccessNoText == "" ? "&nbsp;" : strAccessNoText) + "</td>";
            }

            // 出版日期
            // 2009/4/7
            string strPublishTime = DomUtil.GetElementText(dom.DocumentElement, "publishTime");
            if (this.m_hidecolumns.IndexOf("publishTime") == -1)
                strResult += "<td class='publishTime'>" + (strPublishTime == "" ? "&nbsp;" : GetDisplayPublishTime(strPublishTime)) + "</td>";

            // 卷期信息
            // 2009/4/7
            string strVolume = DomUtil.GetElementText(dom.DocumentElement, "volume");
            if (this.m_hidecolumns.IndexOf("volume") == -1)
                strResult += "<td class='volume'>" + (strVolume == "" ? "&nbsp;" : strVolume) + "</td>";

            // 累积期定位字符串
            if (string.IsNullOrEmpty(strVolume) == false)
            {
                string strIssueDbName = GetIssueDbName(
                    app,
                    e.Path);
                if (string.IsNullOrEmpty(strIssueDbName) == false)
                {
                    List<IssueString> query_strings = dp2StringUtil.GetIssueQueryStringFromItemXml(dom);
                    foreach (IssueString s in query_strings)
                    {
                        if (IndexOf(_issue_query_strings, s.Query) == -1)
                            _issue_query_strings.Add(s);
                    }
                }
            }

            // 价格
            string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
            if (this.m_hidecolumns.IndexOf("price") == -1)
                strResult += "<td class='price'>" + (strPrice == "" ? "&nbsp;" : strPrice) + "</td>";

            // 注释
            string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            if (this.m_hidecolumns.IndexOf("comment") == -1)
                strResult += "<td class='comment'>" + (strComment == "" ? "&nbsp;" : strComment) + "</td>";


            // 借者条码
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            bool bMyselfBorrower = false;
            /*
            bool bLibrarian = false;

            if (sessioninfo.Account != null
                && sessioninfo.Account.Type != "reader")
                bLibrarian = true;
            else
                bLibrarian = false;
             * */

            // 看看是否自己已经借阅的
            if (String.IsNullOrEmpty(strBorrower) == false
                && loginstate != LoginState.NotLogin
                /*sessioninfo.Account != null*/)
            {

                if (loginstate == LoginState.Reader
                    /*sessioninfo.Account.Type == "reader"*/
                    && sessioninfo.ReaderInfo.Barcode == strBorrower
                    && String.IsNullOrEmpty(strBorrower) == false)
                {
                    Debug.Assert(sessioninfo.ReaderInfo != null);

                    bMyselfBorrower = true;
                    checkbox.Enabled = false;
                    checkbox.ToolTip = "此册已经被本人借阅, 在还回前，不能预约。";
                }
            }

            // 如果不允许预约在架的图书
            if (checkbox.Enabled == true
                && app.CanReserveOnshelf == false
                && String.IsNullOrEmpty(strBorrower) == true)
            {
                checkbox.Enabled = false;
                checkbox.ToolTip = "在架(未被借阅的)册，不能预约。";
            }


            // 对于读者，隐去除自己以外的其他人的证条码号
            // 对于访客登录或者没有登录，也是隐去证条码号 2007/9/4
            if (loginstate == LoginState.Public
                || loginstate == LoginState.NotLogin
                || (loginstate == LoginState.Reader
                && bMyselfBorrower == false))
            {
                int nLength = strBorrower.Length;
                strBorrower = "";
                strBorrower = strBorrower.PadLeft(nLength, '*');
            }



            // strResult += "<td class='borrower'>";

            string strBorrowString = "";

            if (loginstate == LoginState.Librarian
                && strBorrower != "")
            {
                // TODO: 要改造readerinfo.aspx
                string strBorrowerUrl = "./readerinfo.aspx?barcode=" + strBorrower;
                // strResult += "<a href='"+strBorrowerUrl+"'>" + strBorrower + "</a>";
                strBorrowString = "<a href='" + strBorrowerUrl + "'>" + strBorrower + "</a>";
            }
            else
            {
                /*
                strResult += (strBorrower == "" ? "&nbsp;" : strBorrower);
                if (bMyself == true)
                    strResult += "(我自己)";
                 * */
                strBorrowString = strBorrower;
                if (bMyselfBorrower == true)
                    strBorrowString += this.GetString("我自己");  // "(我自己)"

            }


            // strResult += "</td>";


            // 续借次
            string strNo = DomUtil.GetElementText(dom.DocumentElement, "no");
            // strResult += "<td class='renewno'>" + (strNo == "" ? "&nbsp;" : strNo) + "</td>";

            // 借阅日期
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
            string strTime = strBorrowDate;
            if (String.IsNullOrEmpty(strTime) == false)
            {
                try
                {
                    strTime = DateTimeUtil.LocalTime(strTime);
                }
                catch
                {
                    strTime = "时间格式错误 -- " + strTime;
                }
            }

            // <borrowerReaderType>是2009/9/18以后为实体记录新增的一个元素，是从读者记录中<readerType>中复制过来的
            string strBorrowerReaderType = DomUtil.GetElementText(dom.DocumentElement, "borrowerReaderType");

            // strTime = strTime.Replace(" ", "<br/>");    // 插入一个回车，以便占据的宽度更小一点
            // strResult += "<td class='borrowdate' nowrap>" + (strTime == "" ? "&nbsp;" : strTime) + "</td>";


            // 借阅期限
            string strPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            // strResult += "<td class='borrowperiod'>" + (strPeriod == "" ? "&nbsp;" : strPeriod) + "</td>";


            // string strError = "";
            string strOverDue = ""; // 超期情况字符串。已经被规范过，不超期的时候这个字符串为空值
            string strOriginOverdue = "";   // 超期情况字符串，没有加工过，如果是不超期的时候，则会说还有多少天到期
            long lOver = 0;
            string strPeriodUnit = "";

            strClass = "borrows";

#if NO
            if (String.IsNullOrEmpty(strBorrowDate) == false)
            {
                // 获得日历
                Calendar calendar = null;

                if (String.IsNullOrEmpty(strBorrowerReaderType) == false)
                {
                    nRet = app.GetReaderCalendar(strBorrowerReaderType,
                        out calendar,
                        out strError);
                    if (nRet == -1)
                    {
                        calendar = null;
                    }
                }

                // 检查超期情况。
                // return:
                //      -1  数据格式错误
                //      0   没有发现超期
                //      1   发现超期   strError中有提示信息
                //      2   已经在宽限期内，很容易超期 2009/3/13
                nRet = app.CheckPeriod(
                    calendar,   // 2009/9/18 changed
                    strBorrowDate,
                    strPeriod,
                    out lOver,
                    out strPeriodUnit,
                    out strError);

                strOriginOverdue = strError;

                if (nRet == -1)
                    strOverDue = strError;  // 错误信息

                if (nRet == 1)
                    strOverDue = this.GetString("已超期");
                else if (nRet == 2) // 2009/9/18
                    strOverDue = this.GetString("已在宽限期内，即将超期");

                /*
                if (nRet == 1 || nRet == 0)
                    strOverDue = strError;	// "已超期";
                 * */
                if (nRet == 1)
                    strClass = "borrows over";
                else if (nRet == 2) // 2009/9/18
                    strClass = "borrows warning";
                else if (nRet == 0 && lOver >= -5)
                    strClass = "borrows warning";

                strPeriod = app.GetDisplayTimePeriodStringEx(strPeriod);
            }
#endif


            string strType = "";
            XmlNode nodeOverdueInfo = dom.DocumentElement.SelectSingleNode(
                "overdueInfo");
            if (nodeOverdueInfo != null)
            {
                strOverDue = nodeOverdueInfo.InnerText;
                strType = DomUtil.GetAttr(nodeOverdueInfo, "type");
            }

            if (String.IsNullOrEmpty(strType) == false)
                strClass += "" + strType;

            XmlNode nodeOriginOverdueInfo = dom.DocumentElement.SelectSingleNode(
    "originOverdueInfo");
            if (nodeOriginOverdueInfo != null)
            {
                strOriginOverdue = nodeOriginOverdueInfo.InnerText;
                strPeriodUnit = DomUtil.GetAttr(nodeOriginOverdueInfo, "unit");
                string strOver = DomUtil.GetAttr(nodeOriginOverdueInfo, "over");
                string strCalendarName = DomUtil.GetAttr(nodeOriginOverdueInfo, "calendar");
            }


            // 超期情况
            // strResult += "<td class='"+strClass+"'>" + (strOverDue == "" ? "&nbsp;" : strOverDue) + "</td>";

            // TODO: 颜色最好根据是否超期而加以变化?

            string strText = "";
            if (String.IsNullOrEmpty(strBorrowString) == false)
            {
                strText += this.GetString("借阅者") + ": " + strBorrowString;

                if (string.IsNullOrEmpty(strNo) == false
                    && strNo != "0")
                {
                    strText += "<br/>" + this.GetString("续借次") + ": " + strNo;
                }

                strPeriod = app.GetDisplayTimePeriodStringEx(strPeriod);

                strText += "<br/>" + this.GetString("借阅日期") + ": " + strTime + "<br/>"
                    + this.GetString("借阅期限") + ": " + strPeriod;
                // + " (" + lOver.ToString() + ")";

                if (String.IsNullOrEmpty(strOverDue) == false)
                {
                    strText += "<br/>" + this.GetString("是否超期") + ": "
                        + strOverDue;
                }
            }

            // 2009/8/5
            // tips text
            if (String.IsNullOrEmpty(strOriginOverdue) == false
                && String.IsNullOrEmpty(strText) == false)
            {
                strText = "<a title=\"" + strOriginOverdue.Replace("\"", "'") + "\">" + strText + "</a>";
            }

            if (this.m_hidecolumns.IndexOf("borrows") == -1)
                strResult += "<td class='" + strClass + "' nowrap>" + strText + "</td>";

            // 预约情况
            if (this.m_hidecolumns.IndexOf("reservations") == -1)
            {
                string strReservations = "";
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("reservations/request");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strReader = DomUtil.GetAttr(node, "reader");
                    string strRequestDate = DateTimeUtil.LocalDate(DomUtil.GetAttr(node, "requestDate"));

                    bool bMyselfReserver = false;

                    if (/*sessioninfo.Account != null
                    && sessioninfo.Account.Type == "reader"*/
                        loginstate == LoginState.Reader
                        && sessioninfo.ReaderInfo.Barcode == strReader
                        && String.IsNullOrEmpty(strReader) == false)
                    {
                        bMyselfReserver = true;
                    }

                    // 对于读者，隐去除自己以外的其他人的证条码号
                    if (loginstate == LoginState.NotLogin
                        || loginstate == LoginState.Public  // 2009/4/10
                                                            /*sessioninfo.Account == null*/
                        || (loginstate == LoginState.Reader
                            /*sessioninfo.Account != null
                            && sessioninfo.Account.Type == "reader"*/
                            && bMyselfReserver == false))
                    {
                        int nLength = strReader.Length;
                        strReader = "";
                        strReader = strReader.PadLeft(nLength, '*');
                    }

                    // TODO: 要改造readerinfo.aspx
                    string strReaderUrl = "./readerinfo.aspx?barcode=" + strReader;
                    if (loginstate == LoginState.Librarian)
                    {
                        strReservations += this.GetString("读者证条码号") + ": " + "<a href='" + strReaderUrl + "'>" + strReader + "</a>"
                             + "; "
                             + this.GetString("请求日期") + ": " + strRequestDate + "<br/>";
                    }
                    else
                    {
                        string strReaderString = strReader;

                        if (bMyselfReserver == true)
                            strBorrowString += this.GetString("我自己");  // "(我自己)"


                        strReservations += this.GetString("读者证条码号")
                            + ": " + strReaderString + "; "
                            + this.GetString("请求日期")
                            + ": " + strRequestDate + "<br/>";
                    }
                }

                strResult += "<td class='reservations'>" + (strReservations == "" ? "&nbsp;" : strReservations) + "</td>";
            }

            // 流通次
            if (this.m_hidecolumns.IndexOf("borrowcount") == -1)
            {
                string strCirculationCount = "";
                XmlNode nodeCount = dom.DocumentElement.SelectSingleNode("borrowHistory/@count");
                if (nodeCount != null)
                    strCirculationCount = nodeCount.Value;
                strResult += "<td class='borrowcount'>" + (string.IsNullOrEmpty(strCirculationCount) == true ? "&nbsp;" : strCirculationCount) + "</td>";
            }

            /*
            // 从属种记录id
            strResult += "<td class='parent'>" + DomUtil.GetElementText(dom.DocumentElement, "parent") +

    "</td>";
             * */

            strResult += "</tr>";

            right.Text = strResult;
            return;
            ERROR1:
            // tempOutput += strError;
            this.Page.Response.Write(strError);
        }

        // 出现过的期 检索字符串
        // 格式为 年份|期号|...
        List<IssueString> _issue_query_strings = new List<IssueString>();

        static int IndexOf(List<IssueString> strings, string query)
        {
            int i = 0;
            foreach (IssueString s in strings)
            {
                if (s.Query == query)
                    return i;
                i++;
            }

            return -1;
        }

        public static string GetDisplayPublishTime(string strPublishTime)
        {
            int nRet = strPublishTime.IndexOf("-");
            if (nRet == -1)
                return GetOneDisplayPublishTime(strPublishTime);

            string strLeft = strPublishTime.Substring(0, nRet).Trim();
            string strRight = strPublishTime.Substring(nRet + 1).Trim();

            string strLeftResult = GetOneDisplayPublishTime(strLeft);
            string strRightResult = GetOneDisplayPublishTime(strRight);

            return strLeftResult + "~" + strRightResult;
        }

        // 将出版日期字符串转换为适合显示的格式
        public static string GetOneDisplayPublishTime(string strPublishTime)
        {
            int nLength = strPublishTime.Length;
            if (nLength > 8)
                strPublishTime = strPublishTime.Insert(8, ":");

            if (nLength > 6)
                strPublishTime = strPublishTime.Insert(6, "-");
            if (nLength > 4)
                strPublishTime = strPublishTime.Insert(4, "-");

            return strPublishTime;
        }

        string DisplayBlankIssueCover
        {
            get
            {
                string strDefault = "displayBlank,hideWhenAllBlank";
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)
                    return strDefault;

                return DomUtil.GetStringParam(
                    app.WebUiDom.DocumentElement,
                    "itemsControl",
                    "displayBlankIssueCover",
                    strDefault);
            }
        }

        string IssueCoverSize
        {
            get
            {
                string strDefaultValue = "LargeImage";
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)
                    return strDefaultValue;

                XmlNode node = app.WebUiDom.DocumentElement.SelectSingleNode(
    "itemsControl/@issueCoverSize");
                if (node == null)
                    return strDefaultValue;

                string strValue = node.Value;
                if (string.IsNullOrEmpty(strValue) == true)
                    return strDefaultValue;
                return strValue;
            }
        }

        // 是否要显示空的表格？
        /*
<itemsControl>
<properties displayBlankTable='true'/>
</itemsControl>
 * */
        bool DisplayBlankTable
        {
            get
            {
                bool bDefaultValue = false;  // 2012/12/20 修改为 false
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                if (app == null)
                {
                    return bDefaultValue;
                }
                XmlNode nodeItem = app.WebUiDom.DocumentElement.SelectSingleNode(
                    "itemsControl/properties");

                if (nodeItem == null)
                    return bDefaultValue;

                string strError = "";
                bool bValue;
                DomUtil.GetBooleanParam(nodeItem,
                    "displayBlankTable",
                    bDefaultValue,
                    out bValue,
                    out strError);
                return bValue;
            }
        }

        /*
         * 
	<itemsControl>
		<columns type="series" hideColumns="price,accessNo"/>
		<columns type="book" hideColumns="price"/>
	</itemsControl>         * 
         * */
        // 2012/6/2
        // 获得需要隐藏的栏目列表
        static List<string> GetHideColumns(OpacApplication app,
            string strBiblioDbName)
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = app.WebUiDom.DocumentElement.SelectNodes(
                "itemsControl/columns");

            if (nodes.Count == 0)
                return results;

            string strHideColumns = "";

            foreach (XmlNode node in nodes)
            {
                string strCurrentType = DomUtil.GetAttr(node, "type");
                string strCurrentHideColumns = DomUtil.GetAttr(node, "hideColumns");

                string strCurrentDbType = "";
                ItemDbCfg cfg = app.GetBiblioDbCfg(strBiblioDbName);
                if (String.IsNullOrEmpty(cfg.IssueDbName) == true)
                    strCurrentDbType = "book";
                else
                    strCurrentDbType = "series";

                // 如果数据库类型能够匹配
                if (StringUtil.IsInList(strCurrentDbType, strCurrentType) == true)
                {
                    strHideColumns = strCurrentHideColumns;
                    break;
                }

                // 如果书目库名能够匹配
                if (StringUtil.IsInList(strBiblioDbName, strCurrentType) == true)
                {
                    strHideColumns = strCurrentHideColumns;
                    break;
                }
            }

            if (string.IsNullOrEmpty(strHideColumns) == true)
                return results; // 没有匹配的

            return StringUtil.SplitList(strHideColumns);
        }

        // 获得馆藏地点的显示字符串
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetLocationDisplayString(
            string strLang,
            string strLocation,
            out string strCaption,
            out string strUrl,
            out string strError)
        {
            strError = "";
            strCaption = "";
            strUrl = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                return -1;
            }

            // '//'使得locationDisplay元素除了放在根下，也可以放到<itemsControl>元素下面。
            XmlNode nodeItem = app.WebUiDom.DocumentElement.SelectSingleNode(
                "//locationDisplay/item[@value='" + strLocation + "']");

            if (nodeItem == null)
            {
                strCaption = strLocation;
                return 0;
            }

            strCaption = DomUtil.GetCaption(strLang, nodeItem);
            strUrl = DomUtil.GetAttr(nodeItem, "url");
            return 1;
        }

        // 通过册条码号得知从属的种记录路径
        // parameters:
        //      strBorrower 借阅者证条码号。用于条码号重复的时候附加判断。
        // return:
        //      -1  error
        //      0   册记录没有找到(strError中有说明信息)
        //      1   找到
        public int GetBiblioRecPath(string strBarcode,
            string strBorrower,
            out string strBiblioRecPath,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            // Debug.Assert(false, "需要改造");
            // TODO: 构造warning
            strWarning = "";
            return app.GetBiblioRecPath( // sessioninfo,
                strBarcode,
                // strBorrower,
                out strBiblioRecPath,
                // out strWarning,
                out strError);
        }

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            return "<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"    //  
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>" + strTitle + "</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>";
        }

        public string GetPostfixString()
        {
            return "</div>";
        }

        #region 书库位置相关函数

        // parameters:
        //      strImageFileName    返回图像文件名。为纯文件名，不带路径部分
        //      strDataCoords       返回书架顶点坐标
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static int MatchImageFile(
            string strLocationDir,
            string strLocation,
            string strAccessNo,
            out string strImageFileName,
            out string strDataCoords,
            out int nImageWidth,
            out int nImageHeight,
            out string strDescription,
            out string strError)
        {
            strImageFileName = "";
            strDataCoords = "";
            strError = "";
            nImageWidth = 0;
            nImageHeight = 0;
            strDescription = "";

            // 去掉不参与排序的行
            strAccessNo = StringUtil.BuildLocationClassEntry(strAccessNo);

            try
            {
                DirectoryInfo di = new DirectoryInfo(strLocationDir);

                // TODO: 可以先探测一下 馆藏地点字符串 的第一部分和全部为文件名的XML文件是否存在，存在则直接使用，不再探测其他文件

                // 列出所有xml文件
                FileInfo[] fis = di.GetFiles("*.xml");

                foreach (FileInfo info in fis)
                {
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    int nRet = MatchOneImageFile(
                        info.FullName,
                        strLocation,
                        strAccessNo,
                        out strImageFileName,
                        out strDataCoords,
                        out nImageWidth,
                        out nImageHeight,
                        out strDescription,
                        out strError);
                    if (nRet != 0)
                        return nRet;
                }
            }
            catch (DirectoryNotFoundException)
            {
                strError = "目录 '" + strLocationDir + "' 尚未创建， 无法匹配图像文件";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "MatchImageFile() 出现异常 :" + ex.Message;
                return -1;
            }

            return 0;
        }

        static void IntegerTopPoint(ref string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return;
            string[] parts = strText.Split(new char[] { ',' });
            if (parts.Length < 2)
                return;
            string strValue1 = "";
            int nRet = parts[0].IndexOf(".");
            if (nRet != -1)
                strValue1 = parts[0].Substring(0, nRet).Trim();
            else
                strValue1 = parts[0];

            string strValue2 = "";
            nRet = parts[1].IndexOf(".");
            if (nRet != -1)
                strValue2 = parts[1].Substring(0, nRet).Trim();
            else
                strValue2 = parts[1];

            strText = strValue1 + "," + strValue2;
        }

        static void ParseTwoInteger(string strText,
            out int v1,
            out int v2)
        {
            v1 = 0;
            v2 = 0;
            if (string.IsNullOrEmpty(strText) == true)
                return;
            string[] parts = strText.Split(new char[] { ',' });
            if (parts.Length == 1)
            {
                if (Int32.TryParse(parts[0], out v1) == false)
                    throw new Exception("字符串 '" + strText + "' 格式错误，第一部分应该为数字");
                return;
            }

            if (Int32.TryParse(parts[0], out v1) == false)
                throw new Exception("字符串 '" + strText + "' 格式错误，第一部分 '" + parts[0] + "' 应该为数字");
            if (Int32.TryParse(parts[1], out v2) == false)
                throw new Exception("字符串 '" + strText + "' 格式错误，第二部分 '" + parts[1] + "' 应该为数字");
        }

        static void ParseTwoDouble(string strText,
        out double v1,
        out double v2)
        {
            v1 = 0;
            v2 = 0;
            if (string.IsNullOrEmpty(strText) == true)
                return;
            string[] parts = strText.Split(new char[] { ',' });
            if (parts.Length == 1)
            {
                if (double.TryParse(parts[0], out v1) == false)
                    throw new Exception("字符串 '" + strText + "' 格式错误，第一部分应该为数字");
                return;
            }

            if (double.TryParse(parts[0], out v1) == false)
                throw new Exception("字符串 '" + strText + "' 格式错误，第一部分 '" + parts[0] + "' 应该为数字");
            if (double.TryParse(parts[1], out v2) == false)
                throw new Exception("字符串 '" + strText + "' 格式错误，第二部分 '" + parts[1] + "' 应该为数字");
        }

        // parameters:
        //      strRange    xxxx~xxxx;xxxx~xxxx;...
        // return:
        //      -1  出错
        //      0   没有匹配上
        //      1   匹配上了
        static int MatchAccessNo(string strRangeList,
            string strAccessNo,
            out string strError)
        {
            strError = "";

            string[] ranges = strRangeList.Split(new char[] { ',', ';' });

            foreach (string s in ranges)
            {
                string strRange = s.Trim();
                if (string.IsNullOrEmpty(strRange) == true)
                    continue;

                string[] parts = strRange.Split(new char[] { '~' });
                if (parts.Length != 2)
                {
                    strError = "字符串 '" + strRangeList + "' 中单个索取号范围字符串 '" + strRange + "' 格式不正确";
                    return -1;
                }

                if (StringUtil.CompareAccessNo(strAccessNo, parts[0]) >= 0
                    && StringUtil.CompareAccessNo(strAccessNo, parts[1]) <= 0)
                    return 1;
            }
            return 0;
        }

        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        static int MatchOneImageFile(
            string strXmlFilePath,
            string strLocation,
            string strAccessNo,
            out string strImageFileName,
            out string strDataCoords,
            out int nImageWidth,
            out int nImageHeight,
            out string strDescription,
            out string strError)
        {
            strImageFileName = "";
            strDataCoords = "";
            strError = "";
            nImageWidth = 0;
            nImageHeight = 0;
            strDescription = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFilePath);
            }
            catch (Exception ex)
            {
                strError = "XML文件 '" + strXmlFilePath + "' 装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlNode node in nodes)
            {
                string strRoomName = DomUtil.GetAttr(node, "roomName");
                if (strRoomName != strLocation)
                    continue;

                string strRange = DomUtil.GetAttr(node, "accessNoRange");

                // return:
                //      -1  出错
                //      0   没有匹配上
                //      1   匹配上了
                int nRet = MatchAccessNo(strRange,
                    strAccessNo,
                    out strError);
                if (nRet == -1)
                {
                    strError = "位置定义文件中下列片断 '" + node.OuterXml + "' 有错:" + strError;
                    return -1;
                }

                if (nRet == 1)
                {
                    string strTopPoint = DomUtil.GetAttr(node, "topPoint");
                    double x = 0;
                    double y = 0;

                    try
                    {
                        ParseTwoDouble(strTopPoint,
                            out x,
                            out y);
                    }
                    catch (Exception ex)
                    {
                        strError = "位置定义文件中下列片断 '" + node.OuterXml + "' 中 topPoint 属性值  '" + strTopPoint + "' 格式错误: " + ex.Message;
                        return -1;
                    }


                    strImageFileName = Path.GetFileName(strXmlFilePath);

                    // 去掉末尾的'.xml'部分
                    nRet = strImageFileName.LastIndexOf(".");
                    if (nRet != -1)
                        strImageFileName = strImageFileName.Substring(0, nRet);

                    // 
                    string strImageSize = DomUtil.GetAttr(dom.DocumentElement,
                        "imagePixelSize");
                    try
                    {
                        ParseTwoInteger(strImageSize,
                out nImageWidth,
                out nImageHeight);
                    }
                    catch (Exception ex)
                    {
                        strError = "根元素 imagePixelSize 属性值  '" + strImageSize + "' 格式错误: " + ex.Message;
                        return -1;
                    }

                    // 校正 topPoint
                    string strViewportSize = DomUtil.GetAttr(dom.DocumentElement,
                        "viewportSize");
                    if (string.IsNullOrEmpty(strViewportSize) == false)
                    {
                        int nViewportWidth = 0;
                        int nViewportHeight = 0;
                        try
                        {
                            ParseTwoInteger(strViewportSize,
                    out nViewportWidth,
                    out nViewportHeight);
                        }
                        catch (Exception ex)
                        {
                            strError = "根元素 viewportSize 属性值  '" + strViewportSize + "' 格式错误: " + ex.Message;
                            return -1;
                        }

                        x = (double)nImageWidth * (x / (double)nViewportWidth);
                        y = (double)nImageHeight * (y / (double)nViewportHeight);
                    }

                    x -= 15 / 2;
                    y -= 23 - 4;

                    strDataCoords = ((int)x).ToString() + "," + ((int)y).ToString();

                    string strNo = DomUtil.GetAttr(node, "no");
                    strDescription = "索取号 '" + strAccessNo + "' 馆藏地 '" + strRoomName + "'";
                    if (string.IsNullOrEmpty(strNo) == false)
                        strDescription += " 架号 '" + strNo + "'";
                    return 1;
                }
            }

            return 0;
        }

        #endregion
    }

    public enum ItemDispStyle
    {
        Item = 0x02,    // 显示当前册
        Items = 0x04,   // 显示同种的全部册
    }
}

