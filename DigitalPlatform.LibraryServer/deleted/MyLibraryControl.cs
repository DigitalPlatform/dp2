using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
namespace DigitalPlatform.LibraryServer
{
#if NOOOOOOOOOOOOOOO
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:MyLibraryControl runat=server></{0}:MyLibraryControl>")]
    public class MyLibraryControl : WebControl, INamingContainer
    {
        /*
        public SessionInfo SessionInfo = null;
        public CirculationApplication App = null;
         */

        public string Barcode = "";

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        /*
        // 违约信息内容行数
        public int OverdueLineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "MyLibraryControl_OverdueLineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MyLibraryControl_OverdueLineCount"] = (object)value;
            }
        }
         */

        // 借阅信息内容行数
        public int BorrowLineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "MyLibraryControl_BorrowLineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "MyLibraryControl_BorrowLineCount"] = (object)value;
            }
        }

        // 预约信息内容行数
        public int ReservationLineCount
        {
            get
            {
                object o = this.Page.Session["MyLibraryControl_ReservationLineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session["MyLibraryControl_ReservationLineCount"] = (object)value;
            }
        }

        // 已借阅条码号列表
        public List<string> BorrowBarcodes
        {
            get
            {
                object o = this.Page.Session["MyLibraryControl_BorrowBarcodes"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session["MyLibraryControl_BorrowBarcodes"] = (object)value;
            }
        }

        // 已预约条码号列表
        public List<string> ReservationBarcodes
        {
            get
            {
                object o = this.Page.Session["MyLibraryControl_ReservationBarcodes"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session["MyLibraryControl_ReservationBarcodes"] = (object)value;
            }
        }

        // 布局控件
        protected override void CreateChildControls()
        {
            // 采用圆形角的框子

            // 重要标识: 证条码号、姓名、证颁发和有效期
            PlaceHolder identity = new PlaceHolder();
            identity.ID = "identity";
            this.Controls.Add(identity);

            CreateIdentityControls(identity);

            LiteralControl sep = new LiteralControl();
            sep.Text = "<br/>";
            this.Controls.Add(sep);


            // 一般信息：性别住址等等 可以初始化为收缩状态
            PlaceHolder generalinfo = new PlaceHolder();
            generalinfo.ID = "generalinfo";
            this.Controls.Add(generalinfo);

            CreateGeneralInfoControls(generalinfo);

            sep = new LiteralControl();
            sep.Text = "<br/>";
            this.Controls.Add(sep);


            // 违约信息
            PlaceHolder overdueinfo = new PlaceHolder();
            overdueinfo.ID = "overdueinfo";
            this.Controls.Add(overdueinfo);

            CreateOverdueInfoControls(overdueinfo);

            sep = new LiteralControl();
            sep.Text = "<br/>";
            this.Controls.Add(sep);


            // 借阅信息
            PlaceHolder borrowinfo = new PlaceHolder();
            borrowinfo.ID = "borrowinfo";
            this.Controls.Add(borrowinfo);

            CreateBorrowInfoControls(borrowinfo);

            sep = new LiteralControl();
            sep.Text = "<br/>";
            this.Controls.Add(sep);

            // 预约请求
            PlaceHolder reservation = new PlaceHolder();
            reservation.ID = "reservation";
            this.Controls.Add(reservation);

            CreateReservationControls(reservation);

            sep = new LiteralControl();
            sep.Text = "<br/>";
            this.Controls.Add(sep);

            // 其他命令
            PlaceHolder commands = new PlaceHolder();
            commands.ID = "commands";
            this.Controls.Add(commands);

            CreateCommandsControls(commands);


            // 自己定制的焦点的文献（定题检索）
            PlaceHolder prefer = new PlaceHolder();
            prefer.ID = "prefer";
            this.Controls.Add(prefer);

        }

        void CreateRoundTableStart(Control parent,
            string strID,
            string strTitleText,
            out Button button)
        {
            button = null;

            string strResult = "";
            strResult += "<table class='roundtable' cellpadding='0' cellspacing='0' border='0' width='100%'>";
            strResult += "  <tr>";
            strResult += "      <td>";
            strResult += "          <div id='unRestricted'>";
            strResult += "              <div class='topCorners'>";
            strResult += "                  <div class='l1'></div>";
            strResult += "                  <div class='l2'></div>";
            strResult += "                  <div class='l3'></div>";
            strResult += "                  <div class='l4'></div>";
            strResult += "              </div>";

            strResult += "              <div id='quickInfoHeader'>";
            strResult += "                  <div style='text-align:left;'>";

            LiteralControl literal = new LiteralControl();
            literal.Text = strResult;
            parent.Controls.Add(literal);


            button = new Button();
            button.ID = strID + "_expandbutton";
            button.Text = "-";
            parent.Controls.Add(button);

            literal = new LiteralControl();
            literal.ID = strID + "_titletext";
            literal.Text = strTitleText;
            parent.Controls.Add(literal);

            strResult =  "                  </div>";
            strResult += "              </div>";

            literal = new LiteralControl();
            literal.Text = strResult;
            parent.Controls.Add(literal);

           
        }

        /*
        string GetRoundTableStart(string strTableTitle)
        {
            string strResult = "";
            strResult += "<table class='roundtable' cellpadding='0' cellspacing='0' border='0' width='100%'>";
            strResult += "  <tr>";
            strResult += "      <td>";
            strResult += "          <div id='unRestricted'>";
            strResult += "              <div class='topCorners'>";
            strResult += "                  <div class='l1'></div>";
            strResult += "                  <div class='l2'></div>";
            strResult += "                  <div class='l3'></div>";
            strResult += "                  <div class='l4'></div>";
            strResult += "              </div>";

            strResult += "              <div id='quickInfoHeader'>";
            strResult += "                  <div style='text-align:left;'>"+strTableTitle+"</div>";
            strResult += "              </div>";

            return strResult;
        }
         */

        string GetRoundTableEnd()
        {
            string strResult = "";

            strResult += "              <div class='bottomCorners'>";
            strResult += "			        <div class='l4'></div>";
            strResult += "                  <div class='l3'></div>";
            strResult += "                  <div class='l2'></div>";
            strResult += "                  <div class='l1'></div>";
            strResult += "              </div>";

		    strResult += "          </div>";
	        strResult += "      </td>";
            strResult += "  </tr>";
            strResult += "</table>";

            return strResult;
        }

        // 创建初始的识别信息信息内部行布局
        void CreateIdentityControls(PlaceHolder parent)
        {
            Button identityExpandButton = null;
            CreateRoundTableStart(parent, "identity", "识别信息", out identityExpandButton);
            identityExpandButton.Click += new EventHandler(identityExpandButton_Click);

            PlaceHolder identity = new PlaceHolder();
            identity.ID = "identity_content";
            parent.Controls.Add(identity);

            LiteralControl titleline = new LiteralControl();
            titleline.ID = "identity_titleline";
            titleline.Text = "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            // titleline.Text += "<tr class='roundtitle'><td nowrap>事项</td><td nowrap>值</td></tr>";
            identity.Controls.Add(titleline);

            // 每一行一个占位控件
            NewIdentityLine(identity, "姓名", "identity_name", true);
            NewIdentityLine(identity, "读者证条码号", "identity_barcode", true);
            NewIdentityLine(identity, "状态", "identity_state", true);
            NewIdentityLine(identity, "读者类别", "identity_readertype", false);
            NewIdentityLine(identity, "发证日期", "identity_createdate", false);
            NewIdentityLine(identity, "失效日期", "identity_expiredate", false);


            // 最后一个空白行
            LiteralControl literal = new LiteralControl();
            literal.ID = "identity_blank";
            literal.Text = "<tr class='roundcontent'><td colspan='9'>&nbsp;</td></tr>";
            identity.Controls.Add(literal);


            literal = new LiteralControl();
            literal.ID = "identity_tableend";
            literal.Text = "</table>";
            identity.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text =  GetRoundTableEnd();
            parent.Controls.Add(literal);
        }

        void identityExpandButton_Click(object sender, EventArgs e)
        {
            ExpandContent("identity");
        }

        void ExpandContent(string strID)
        {
            Button button = (Button)this.FindControl(strID + "_expandbutton");
            Control control = this.FindControl(strID + "_content");
            if (control.Visible == true)
            {
                control.Visible = false;
                button.Text = "+";
            }
            else
            {
                control.Visible = true;
                button.Text = "-";
            }
        }

        PlaceHolder NewIdentityLine(Control parent,
            string strName,
            string strValueControlID,
            bool bLarge)
        {

            string strClass = "";
            if (bLarge == true)
                strClass = " class='largecontent' ";

            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + strValueControlID;

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");


            parent.Controls.Add(line);

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='roundcontentdark'><td>" + strName + "</td><td "+strClass+" >";
            line.Controls.Add(literal);

            // 值
            literal = new LiteralControl();
            literal.ID = strValueControlID;
            literal.Text = "(空)";
            line.Controls.Add(literal);

            // 右侧文字
            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        // 一般命令
        void CreateCommandsControls(PlaceHolder parent)
        {
            // 修改密码
            HyperLink changepassword = new HyperLink();
            changepassword.Text = "修改密码";
            changepassword.NavigateUrl = "./changereaderpassword.aspx";
            parent.Controls.Add(changepassword);

            // 修改个人信息

        }

        // 创建初始的一般信息内部行布局
        void CreateGeneralInfoControls(PlaceHolder parent)
        {
            /*
            LiteralControl tablebegin = new LiteralControl();
            tablebegin.ID = "generalinfo_tablebegin";
            tablebegin.Text = GetRoundTableStart("一般信息") + "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            generalinfo.Controls.Add(tablebegin);
             */
            Button generalinfoExpandButton = null;
            CreateRoundTableStart(parent, "generalinfo", "一般信息", out generalinfoExpandButton);
            generalinfoExpandButton.Click += new EventHandler(generalinfoExpandButton_Click);


            PlaceHolder generalinfo = new PlaceHolder();
            generalinfo.ID = "generalinfo_content";
            parent.Controls.Add(generalinfo);


            LiteralControl titleline = new LiteralControl();
            titleline.ID = "generalinfo_titleline";
            titleline.Text = "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            // titleline.Text += "<tr class='roundtitle'><td nowrap>事项</td><td nowrap>值</td></tr>";
            generalinfo.Controls.Add(titleline);

            // 每一行一个占位控件
            NewGeneralInfoLine(generalinfo, "性别", "generalinfo_gender");
            NewGeneralInfoLine(generalinfo, "生日", "generalinfo_birthday");
            NewGeneralInfoLine(generalinfo, "身份证号", "generalinfo_idcardnumber");
            NewGeneralInfoLine(generalinfo, "单位", "generalinfo_department");
            NewGeneralInfoLine(generalinfo, "地址", "generalinfo_address");
            NewGeneralInfoLine(generalinfo, "电话", "generalinfo_tel");
            NewGeneralInfoLine(generalinfo, "Email地址", "generalinfo_email");


            // 最后一个空白行
            LiteralControl literal = new LiteralControl();
            literal.ID = "generalinfo_blank";
            literal.Text = "<tr class='roundcontent'><td colspan='9'>&nbsp;</td></tr>";
            generalinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.ID = "generalinfo_tableend";
            literal.Text = "</table>";
            generalinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = GetRoundTableEnd();
            parent.Controls.Add(literal);


        }

        void generalinfoExpandButton_Click(object sender, EventArgs e)
        {
            ExpandContent("generalinfo");
        }

        PlaceHolder NewGeneralInfoLine(Control parent,
    string strName,
    string strValueControlID)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + strValueControlID;

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            parent.Controls.Add(line);

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='roundcontentdark'><td>" + strName + "</td><td>";
            line.Controls.Add(literal);

            // 值
            literal = new LiteralControl();
            literal.ID = strValueControlID;
            literal.Text = "(空)";
            line.Controls.Add(literal);

            // 右侧文字
            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        // 创建初始的违约信息内部行布局
        void CreateOverdueInfoControls(PlaceHolder parent)
        {
            Button overdueinfoExpandButton = null;
            CreateRoundTableStart(parent, "overdueinfo", "违约信息", out overdueinfoExpandButton);
            overdueinfoExpandButton.Click += new EventHandler(overdueinfoExpandButton_Click);

            PlaceHolder overdueinfo = new PlaceHolder();
            overdueinfo.ID = "overdueinfo_content";
            parent.Controls.Add(overdueinfo);


            LiteralControl titleline = new LiteralControl();
            titleline.ID = "overdueinfo_titleline";
            titleline.Text = "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            titleline.Text += "<tr class='roundtitle'><td nowrap>册条码号</td><td nowrap width='50%'>摘要</td><td nowrap>违约情况</td><td nowrap>借阅日期</td><td nowrap>期限</td><td nowrap>还书日期</td></tr>";
            overdueinfo.Controls.Add(titleline);

            /*
            // 每一行一个占位控件
            for (int i = 0; i < this.OverdueLineCount; i++)
            {
                PlaceHolder line = NewOverdueLine(overdueinfo, i, null);
                line.Visible = true;
            }
             */

            // 表格行插入点
            PlaceHolder insertpos = new PlaceHolder();
            insertpos.ID = "overdueinfo_insertpos";
            overdueinfo.Controls.Add(insertpos);

            // 最后一个空白行
            LiteralControl literal = new LiteralControl();
            literal.ID = "overdueinfo_blank";
            literal.Text = "<tr class='roundcontent'><td colspan='9'>&nbsp;</td></tr>";
            overdueinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.ID = "overdueinfo_tableend";
            literal.Text = "</table>";
            overdueinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = GetRoundTableEnd();
            parent.Controls.Add(literal);


        }

        void overdueinfoExpandButton_Click(object sender, EventArgs e)
        {
            // off管辖内的checkbox
            OffCheckBoxes("overdueinfo");

            ExpandContent("overdueinfo");
        }

        //

        // 创建初始的借阅信息内部行布局
        void CreateBorrowInfoControls(PlaceHolder parent)
        {
            Button borrowinfoExpandButton = null;
            CreateRoundTableStart(parent, "borrowinfo", "借阅信息", out borrowinfoExpandButton);
            borrowinfoExpandButton.Click += new EventHandler(borrowinfoExpandButton_Click);

            PlaceHolder borrowinfo = new PlaceHolder();
            borrowinfo.ID = "borrowinfo_content";
            parent.Controls.Add(borrowinfo);


            LiteralControl titleline = new LiteralControl();
            titleline.ID = "borrowinfo_titleline";
            titleline.Text = "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            titleline.Text += "<tr class='roundtitle'><td nowrap>册条码号</td><td nowrap width='50%'>摘要</td><td nowrap>续借次</td><td nowrap>借阅日期</td><td nowrap>期限</td><td nowrap>操作者</td><td nowrap>是否超期</td><td nowrap>备注</td></tr>";
            borrowinfo.Controls.Add(titleline);

            // 每一行一个占位控件
            for (int i = 0; i < this.BorrowLineCount; i++)
            {
                PlaceHolder line = NewBorrowLine(borrowinfo, i, null);
                line.Visible = true;
            }

            // 表格行插入点
            PlaceHolder insertpos = new PlaceHolder();
            insertpos.ID = "borrowinfo_insertpos";
            borrowinfo.Controls.Add(insertpos);


            //
            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "borrowinfo_cmdline";
            borrowinfo.Controls.Add(cmdline);

            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='roundcontent'><td colspan='8'>";
            cmdline.Controls.Add(literal);

            Button renewButton = new Button();
            renewButton.ID = "borrowinfo_renewbutton";
            renewButton.Text = "续借";
            renewButton.Click +=new EventHandler(renewButton_Click);
            cmdline.Controls.Add(renewButton);
            renewButton = null;

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            cmdline.Controls.Add(literal);

            cmdline = null;

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "borrowinfo_debugline";
            borrowinfo.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='roundcontent'><td colspan='8'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "borrowinfo_debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;

            //
            /*

            // 最后一个空白行
            LiteralControl literal = new LiteralControl();
            literal.ID = "borrowinfo_blank";
            literal.Text = "<tr class='roundcontent'><td colspan='9'>&nbsp;</td></tr>";
            borrowinfo.Controls.Add(literal);
             */


            literal = new LiteralControl();
            literal.ID = "borrowinfo_tableend";
            literal.Text = "</table>";
            borrowinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = GetRoundTableEnd();
            parent.Controls.Add(literal);


        }

        void renewButton_Click(object sender, EventArgs e)
        {
            List<string> barcodes = this.GetCheckedBorrowBarcodes();

            if (barcodes.Count == 0)
            {
                SetBorrowDebugInfo("尚未选择要续借的事项。");
                return;
            }

            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            for (int i = 0; i < barcodes.Count; i++)
            {
                string strItemBarcode = barcodes[i];
                //string strItemRecord = "";
                //string strReaderRecord = "";

                string strReaderBarcode = "";
                if (String.IsNullOrEmpty(this.Barcode) == false)
                    strReaderBarcode = this.Barcode;
                else
                    strReaderBarcode = sessioninfo.Account.Barcode;

                if (String.IsNullOrEmpty(strReaderBarcode) == true)
                {
                    SetBorrowDebugInfo("尚未指定读者证条码号。操作失败。");
                    return;
                }

                string[] aDupPath = null;
                string[] item_records = null;
                string[] reader_records = null;
                string[] biblio_records = null;
                BorrowInfo borrow_info = null;

                LibraryServerResult result = app.Borrow(
                    sessioninfo,
                    true,
                    strReaderBarcode,
                    strItemBarcode,
                    null,
                    false,
                    null,
                    "", // style
                    "",
                    out item_records,
                    "",
                    out reader_records,
                    "",
                    out biblio_records,
                    out aDupPath,
                    out borrow_info);

                if (result.Value == -1)
                {
                    SetBorrowDebugInfo(result.ErrorInfo);
                    return;
                }
            }


            SetBorrowDebugInfo("续借成功。");
        }

        void SetBorrowDebugInfo(string strText)
        {
            LiteralControl text = (LiteralControl)this.FindControl("borrowinfo_debugtext");

            text.Text = strText;
        }

        void borrowinfoExpandButton_Click(object sender, EventArgs e)
        {
            // off管辖内的checkbox
            OffCheckBoxes("borrowinfo");

            ExpandContent("borrowinfo");
        }

        // 创建初始的预约信息内部行布局
        void CreateReservationControls(PlaceHolder parent)
        {
            parent.Controls.Clear();

            /*
            LiteralControl tablebegin = new LiteralControl();
            tablebegin.ID = "reservation_tablebegin";
            tablebegin.Text = GetRoundTableStart("预约信息") + "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            reservation.Controls.Add(tablebegin);
             */
            Button reservationExpandButton = null;
            CreateRoundTableStart(parent, "reservation", "预约信息", out reservationExpandButton);
            reservationExpandButton.Click += new EventHandler(reservationExpandButton_Click);

            PlaceHolder reservation = new PlaceHolder();
            reservation.ID = "reservation_content";
            parent.Controls.Add(reservation);


            LiteralControl titleline = new LiteralControl();
            titleline.ID = "reservation_titleline";
            titleline.Text = "<table cellpadding='0' cellspacing='0' border='0' width='100%'>";
            titleline.Text += "<tr  class='roundtitle'><td nowrap>册条码号</td><td nowrap>到达情况</td><td nowrap width='50%'>摘要</td><td nowrap>请求日期</td><td nowrap>操作者</td></tr>";
            reservation.Controls.Add(titleline);

            // 每一行一个占位控件
            for (int i = 0; i < this.ReservationLineCount; i++)
            {
                PlaceHolder line = NewReservationLine(reservation, i, null);
                line.Visible = true;
            }

            // 表格行插入点
            PlaceHolder insertpos = new PlaceHolder();
            insertpos.ID = "reservation_insertpos";
            reservation.Controls.Add(insertpos);

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "reservation_cmdline";
            reservation.Controls.Add(cmdline);

            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='roundcontent'><td colspan='5'>";
            cmdline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "&nbsp;(注：删除状态为“已到书”的行表示读者放弃取书。如果要去图书馆正常取书，请一定不要删除这样的行，待取书完成后软件会自动删除)<br/>";
            cmdline.Controls.Add(literal);

            Button reservationDeleteButton = new Button();
            reservationDeleteButton.ID = "reservation_deletebutton";
            reservationDeleteButton.Text = "删除";
            reservationDeleteButton.Click -= new EventHandler(reservationDeleteButton_Click);
            reservationDeleteButton.Click += new EventHandler(reservationDeleteButton_Click);
            cmdline.Controls.Add(reservationDeleteButton);
            reservationDeleteButton = null;


            literal = new LiteralControl();
            literal.Text = "&nbsp;";
            cmdline.Controls.Add(literal);


            Button reservationMergeButton = new Button();
            reservationMergeButton.ID = "reservation_mergebutton";
            reservationMergeButton.Text = "合并";
            reservationMergeButton.Click -= new EventHandler(reservationMergeButton_Click);
            reservationMergeButton.Click += new EventHandler(reservationMergeButton_Click);
            cmdline.Controls.Add(reservationMergeButton);
            reservationMergeButton = null;

            literal = new LiteralControl();
            literal.Text = "&nbsp;";
            cmdline.Controls.Add(literal);


            Button reservationSplitButton = new Button();
            reservationSplitButton.ID = "reservation_splitbutton";
            reservationSplitButton.Text = "拆散";
            reservationSplitButton.Click -= new EventHandler(reservationSplitButton_Click);
            reservationSplitButton.Click += new EventHandler(reservationSplitButton_Click);
            cmdline.Controls.Add(reservationSplitButton);
            reservationSplitButton = null;


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            cmdline.Controls.Add(literal);

            cmdline = null;

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "reservation_debugline";
            reservation.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='roundcontent'><td colspan='5'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "reservation_debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;


            // 表格结尾
            literal = new LiteralControl();
            literal.ID = "reservation_tableend";
            literal.Text = "</table>";
            reservation.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = GetRoundTableEnd();
            parent.Controls.Add(literal);
        }

        void reservationExpandButton_Click(object sender, EventArgs e)
        {
            // off管辖内的checkbox
            OffCheckBoxes("reservation");

            ExpandContent("reservation");
        }

        void OffCheckBoxes(string strID)
        {

            for (int i = 0; ; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl(strID + "_line" + Convert.ToString(i) + "checkbox");
                if (checkbox == null)
                    break;
                if (checkbox.Checked == true)
                    checkbox.Checked = false;
            }
        }

        string GetChekcedReservationBarcodes()
        {
            string strBarcodeList = "";

            PlaceHolder reservation = (PlaceHolder)this.FindControl("reservation");

            for (int i = 0; i < this.ReservationLineCount; i++)
            {
                CheckBox checkbox = (CheckBox)reservation.FindControl("reservation_line" + Convert.ToString(i) + "checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.ReservationBarcodes.Count <= i)
                    {
                        //this.SetReservationDebugInfo("ReservationBarcodes失效...");
                        //return null;
                        throw new Exception("ReservationBarcodes失效...");
                    }
                    string strBarcode = this.ReservationBarcodes[i];

                    if (strBarcodeList != "")
                        strBarcodeList += ",";
                    strBarcodeList += strBarcode;
                    checkbox.Checked = false;
                }
            }

            return strBarcodeList;
        }

        List<string> GetCheckedBorrowBarcodes()
        {
            List<string> barcodes = new List<string>();

            for (int i = 0; i < this.BorrowLineCount; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("borrowinfo_line" + Convert.ToString(i) + "checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.BorrowBarcodes.Count <= i)
                    {
                        throw new Exception("BorrowBarcodes失效...");
                    }
                    string strBarcode = this.BorrowBarcodes[i];

                    barcodes.Add(strBarcode);

                    checkbox.Checked = false;
                }
            }

            return barcodes;
        }

        // 预约：拆散请求
        void reservationSplitButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                SetReservationDebugInfo("尚未选择要拆散的预约事项。");
                return;
            }

            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.Barcode) == false)
                strReaderBarcode = this.Barcode;
            else
                strReaderBarcode = sessioninfo.Account.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                SetBorrowDebugInfo("尚未指定读者证条码号。操作失败。");
                return;
            }

            LibraryServerResult result = app.Reservation(sessioninfo,
                "split",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                SetReservationDebugInfo(result.ErrorInfo);
            else
                SetReservationDebugInfo("拆散预约信息成功。请看预约列表。");
        }

        // 预约：合并请求
        void reservationMergeButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                SetReservationDebugInfo("尚未选择要合并的预约事项。");
                return;
            }

            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.Barcode) == false)
                strReaderBarcode = this.Barcode;
            else
                strReaderBarcode = sessioninfo.Account.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                SetBorrowDebugInfo("尚未指定读者证条码号。操作失败。");
                return;
            }

            LibraryServerResult result = app.Reservation(sessioninfo,
                "merge",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                SetReservationDebugInfo(result.ErrorInfo);
            else
                SetReservationDebugInfo("合并预约信息成功。请看预约列表。");


        }

        // 预约：删除请求
        void reservationDeleteButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                SetReservationDebugInfo("尚未选择要删除的预约事项。");
                return;
            }

            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.Barcode) == false)
                strReaderBarcode = this.Barcode;
            else
                strReaderBarcode = sessioninfo.Account.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                SetBorrowDebugInfo("尚未指定读者证条码号。操作失败。");
                return;
            }

            LibraryServerResult result = app.Reservation(sessioninfo,
                "delete",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                SetReservationDebugInfo(result.ErrorInfo);
            else
                SetReservationDebugInfo("删除预约信息成功。请看预约列表。");

            // Button button = (Button)FindControl("reservation_deletebutton");
        }

        PlaceHolder NewOverdueLine(Control parent,
            int index,
            Control insertbefore)
        {
            if (parent != null && insertbefore != null)
            {
                if (insertbefore.Parent != parent)
                    throw new Exception("插入参考位置和父Control之间, 父子关系不正确");
            }


            PlaceHolder line = new PlaceHolder();
            line.ID = "overdueinfo_line" + Convert.ToString(index);

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            if (insertbefore == null)
                parent.Controls.Add(line);
            else
            {
                int pos = parent.Controls.IndexOf(insertbefore);
                if (pos == -1)
                    throw new Exception("插入参照对象没有找到");
                parent.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "overdueinfo_line" + Convert.ToString(index) + "left";
            literal.Text = "<tr><td></td></tr>";
            line.Controls.Add(literal);

            /*
            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "overdueinfo_line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "overdueinfo_line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);
             */


            return line;
        }

        PlaceHolder NewBorrowLine(Control parent,
            int index,
            Control insertbefore)
        {
            if (parent != null && insertbefore != null)
            {
                if (insertbefore.Parent != parent)
                    throw new Exception("插入参考位置和父Control之间, 父子关系不正确");
            }


            PlaceHolder line = new PlaceHolder();
            line.ID = "borrowinfo_line" + Convert.ToString(index);

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            if (insertbefore == null)
                parent.Controls.Add(line);
            else
            {
                int pos = parent.Controls.IndexOf(insertbefore);
                if (pos == -1)
                    throw new Exception("插入参照对象没有找到");
                parent.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "borrowinfo_line" + Convert.ToString(index) + "left";
            literal.Text = "<tr><td>";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "borrowinfo_line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "borrowinfo_line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        PlaceHolder NewReservationLine(Control parent,
    int index,
    Control insertbefore)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "reservation_line" + Convert.ToString(index);

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            if (insertbefore == null)
                parent.Controls.Add(line);
            else
            {
                int pos = parent.Controls.IndexOf(insertbefore);
                parent.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "reservation_line" + Convert.ToString(index) + "left";
            literal.Text = "<tr><td>";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "reservation_line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "reservation_line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        void SetReservationDebugInfo(string strText)
        {
            PlaceHolder reservation = (PlaceHolder)this.FindControl("reservation");

            PlaceHolder line = (PlaceHolder)reservation.FindControl("reservation_debugline");
            LiteralControl text = (LiteralControl)line.FindControl("reservation_debugtext");

            text.Text = strText;
        }

        void RenderIdentity(XmlDocument dom)
        {
            // 识别信息
            PlaceHolder identity = (PlaceHolder)this.FindControl("identity");

            LiteralControl literal = (LiteralControl)identity.FindControl("identity_name");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "name");

            literal = (LiteralControl)identity.FindControl("identity_barcode");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            literal = (LiteralControl)identity.FindControl("identity_state");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "state");


            literal = (LiteralControl)identity.FindControl("identity_readertype");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "readerType");

            literal = (LiteralControl)identity.FindControl("identity_createdate");
            literal.Text = ItemConverter.LocalDate(DomUtil.GetElementText(dom.DocumentElement, "createDate"));

            literal = (LiteralControl)identity.FindControl("identity_expiredate");
            literal.Text = ItemConverter.LocalDate(DomUtil.GetElementText(dom.DocumentElement, "expireDate"));
        }

        void RenderGeneralInfo(XmlDocument dom)
        {
            // 一般信息
            PlaceHolder generalinfo = (PlaceHolder)this.FindControl("generalinfo");

            LiteralControl literal = (LiteralControl)generalinfo.FindControl("generalinfo_gender");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "gender");

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_birthday");
            literal.Text = ItemConverter.LocalDate(DomUtil.GetElementText(dom.DocumentElement, "birthday"));

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_idcardnumber");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "idCardNumber");

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_department");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "department");

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_address");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "address");

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_tel");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "tel");

            literal = (LiteralControl)generalinfo.FindControl("generalinfo_email");
            literal.Text = DomUtil.GetElementText(dom.DocumentElement, "email");

        }

        void RenderOverdue(
            LibraryApplication app,
            SessionInfo sessioninfo,
            XmlDocument dom)
        {
            
            PlaceHolder overdueinfo = (PlaceHolder)this.FindControl("overdueinfo");

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            for (int i = 0; i < nodes.Count; i++)
            {
                PlaceHolder line = (PlaceHolder)overdueinfo.FindControl("overdueinfo_line" + Convert.ToString(i));
                if (line == null)
                {
                    Control insertpos = overdueinfo.FindControl("overdueinfo_insertpos");
                    line = NewOverdueLine(insertpos.Parent, i, insertpos);
                }
                line.Visible = true;

                LiteralControl left = (LiteralControl)line.FindControl("overdueinfo_line" + Convert.ToString(i) + "left");


                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strItemRecPath = DomUtil.GetAttr(node, "recPath");

                string strReason = DomUtil.GetAttr(node, "reason");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DomUtil.GetAttr(node, "returnDate");

                string strClass = " class='roundcontentdark' ";

                if ((i % 2) == 1)
                    strClass = " class='roundcontentlight' ";

                string strResult = "<tr " + strClass + " nowrap><td nowrap>";

                strResult += "&nbsp;";

                strResult += "<a href='book.aspx?barcode=" + strBarcode + "&itemrecpath=" + strItemRecPath + "'>"
                    + strBarcode + "</a></td>";

                // 获得摘要
                string strSummary = "";
                string strBiblioRecPath = "";
                LibraryServerResult result = app.GetBiblioSummary(
                    sessioninfo,
                    strBarcode,
                    strItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1 || result.Value == 0)
                    strSummary = result.ErrorInfo;

                strResult += "<td width='50%'>" + strSummary + "</td>";
                strResult += "<td >" + strReason + "</td>";
                strResult += "<td nowrap>" + ItemConverter.LocalTime(strBorrowDate) + "</td>";
                strResult += "<td nowrap>" + strPeriod + "</td>";
                strResult += "<td>" + strReturnDate + "</td>";
                strResult += "</tr>";

                left.Text = strResult;
            }

            // 把多余的行隐藏起来
            for (int i = nodes.Count; ; i++)
            {

                PlaceHolder line = (PlaceHolder)overdueinfo.FindControl("overdueinfo_line" + Convert.ToString(i));
                if (line == null)
                    break;

                line.Visible = false;
            }

        }


        void RenderBorrow(
            LibraryApplication app,
            SessionInfo sessioninfo,
            XmlDocument dom)
        {
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");

            // 获得日历
            string strError = "";
            Calendar calendar = null;
            int nRet = app.GetReaderCalendar(strReaderType, out calendar, out strError);
            if (nRet == -1)
            {
                this.SetBorrowDebugInfo(strError);
                calendar = null;
            }

            // 借阅的册
            PlaceHolder borrowinfo = (PlaceHolder)this.FindControl("borrowinfo");

            // 清空集合
            this.BorrowBarcodes = new List<string>();

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            this.BorrowLineCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {

                PlaceHolder line = (PlaceHolder)borrowinfo.FindControl("borrowinfo_line" + Convert.ToString(i));
                if (line == null)
                {
                    Control insertpos = borrowinfo.FindControl("borrowinfo_insertpos");
                    line = NewBorrowLine(insertpos.Parent, i, insertpos);
                    // this.BorrowLineCount++;
                }
                line.Visible = true;

                LiteralControl left = (LiteralControl)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "left");
                CheckBox checkbox = (CheckBox)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "checkbox");
                LiteralControl right = (LiteralControl)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "right");


                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");

                // 添加到集合
                this.BorrowBarcodes.Add(strBarcode);

                string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string strColor = "bgcolor=#ffffff";

                string strOverDue = "";

                // string strError = "";
                // 检查超期情况。
                // return:
                //      -1  数据格式错误
                //      0   没有发现超期
                //      1   发现超期   strError中有提示信息
                //      2   已经在宽限期内，很容易超期 2009/3/13
                nRet = app.CheckPeriod(
                    calendar, 
                    strBorrowDate,
                    strPeriod,
                    out strError);
                if (nRet == -1)
                    strOverDue = strError;
                else
                {
                   strOverDue = strError;	// 其他无论什么情况都显示出来
                }

                string strResult = "";

                string strClass = " class='roundcontentdark' ";

                if ((i % 2) == 1)
                    strClass = " class='roundcontentlight' ";

                strResult += "<tr " + strClass + strColor + "  nowrap><td nowrap>";
                // 左
                left.Text = strResult;

                // checkbox
                // checkbox.Text = Convert.ToString(i + 1);

                // 右开始
                strResult = "&nbsp;";

                strResult += "<a href='book.aspx?barcode=" + strBarcode + "&borrower=" + strReaderBarcode + "'>"
                    + strBarcode + "</a></td>";

                // 获得摘要
                string strSummary = "";
                string strBiblioRecPath = "";
                LibraryServerResult result = app.GetBiblioSummary(
                    sessioninfo,
                    strBarcode,
                    null,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1 || result.Value == 0)
                    strSummary = result.ErrorInfo;

                strResult += "<td width='50%'>" + strSummary + "</td>";
                strResult += "<td nowrap align='right'>" + strNo + "</td>";
                strResult += "<td nowrap>" + ItemConverter.LocalTime(strBorrowDate) + "</td>";
                strResult += "<td nowrap>" + strPeriod + "</td>";
                strResult += "<td nowrap>" + strOperator + "</td>";
                strResult += "<td>" + strOverDue + "</td>";
                strResult += "<td>" + strRenewComment.Replace(";", "<br/>") +

    "</td>";
                strResult += "</tr>";

                right.Text = strResult;

            }

            // 把多余的行隐藏起来
            for (int i = nodes.Count; ; i++)
            {

                PlaceHolder line = (PlaceHolder)borrowinfo.FindControl("borrowinfo_line" + Convert.ToString(i));
                if (line == null)
                    break;

                line.Visible = false;
            }

        }

        void RenderReservation(LibraryApplication app,
            SessionInfo sessioninfo,
            XmlDocument dom)
        {
            // 预约请求
            PlaceHolder reservation = (PlaceHolder)this.FindControl("reservation");
            this.ReservationBarcodes = new List<string>();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("reservations/request");
            this.ReservationLineCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                PlaceHolder line = (PlaceHolder)reservation.FindControl("reservation_line" + Convert.ToString(i));
                if (line == null)
                {
                    Control insertpos = reservation.FindControl("reservation_insertpos");
                    line = NewReservationLine(insertpos.Parent, i, insertpos);
                    //this.ReservationLineCount++;
                }
                line.Visible = true;

                LiteralControl left = (LiteralControl)line.FindControl("reservation_line" + Convert.ToString(i) + "left");
                CheckBox checkbox = (CheckBox)line.FindControl("reservation_line" + Convert.ToString(i) + "checkbox");
                LiteralControl right = (LiteralControl)line.FindControl("reservation_line" + Convert.ToString(i) + "right");


                XmlNode node = nodes[i];
                string strBarcodes = DomUtil.GetAttr(node, "items");

                this.ReservationBarcodes.Add(strBarcodes);

                string strRequestDate = ItemConverter.LocalTime(DomUtil.GetAttr(node, "requestDate"));

                string strResult = "";

                string strClass = " class='roundcontentdark' ";

                if ((i % 2) == 1)
                    strClass = " class='roundcontentlight' ";


                strResult += "<tr " + strClass + "><td nowrap>";

                // 左
                left.Text = strResult;

                // 右开始
                strResult = "&nbsp;";

                //strResult += "" + strBarcodes + "</td>";

                strResult += "" + MakeBarcodeListHyperLink(strBarcodes, ",") + "</td>";

                // 操作者
                string strOperator = DomUtil.GetAttr(node, "operator");
                // 状态
                string strArrivedDate = DomUtil.GetAttr(node, "arrivedDate");
                string strState = DomUtil.GetAttr(node, "state");
                // 2007/1/18
                string strArrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");
                if (strState == "arrived")
                {
                    strArrivedDate = ItemConverter.LocalTime(strArrivedDate);
                    strState = "册 "+strArrivedItemBarcode+" 已于 " + strArrivedDate + " 到书";
                }
                strResult += "<td>" + strState + "</td>";

                string strSummary = app.GetBarcodesSummary(
                    sessioninfo,
                    strBarcodes,
                    "html",
                    "");

                /*
                string strSummary = "";

                string strPrevBiblioRecPath = "";
                string[] barcodes = strBarcodes.Split(new char[] {','});
                for (int j = 0; j < barcodes.Length; j++)
                {
                    string strBarcode = barcodes[j];
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // 获得摘要
                    string strOneSummary = "";
                    string strBiblioRecPath = "";

                    Result result = app.GetBiblioSummary(sessioninfo,
        strBarcode,
        strPrevBiblioRecPath,   // 前一个path
        out strBiblioRecPath,
        out strOneSummary);
                    if (result.Value == -1 || result.Value == 0)
                        strOneSummary = result.ErrorInfo;

                    if (strOneSummary == "" 
                        && strPrevBiblioRecPath == strBiblioRecPath)
                        strOneSummary = "(同上)";

                    strSummary += strBarcode + " : " + strOneSummary + "<br/>";

                    strPrevBiblioRecPath = strBiblioRecPath;
                }
                 */


                strResult += "<td width='50%'>" + strSummary + "</td>";
                strResult += "<td nowrap>" + strRequestDate + "</td>";
                strResult += "<td nowrap>" + strOperator + "</td>";
                strResult += "</tr>";

                right.Text = strResult;

            }

            // 把多余的行隐藏起来
            for (int i = nodes.Count; ; i++)
            {
                PlaceHolder line = (PlaceHolder)reservation.FindControl("reservation_line" + Convert.ToString(i));
                if (line == null)
                    break;

                line.Visible = false;
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            string strError = "";
            LibraryApplication app = (LibraryApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            if (sessioninfo.Account == null)
            {
                // output.Write("尚未登录");
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            string strBarcode = "";
            if (this.Barcode == "")
                strBarcode = sessioninfo.Account.Barcode;
            else
                strBarcode = this.Barcode;

            if (strBarcode == "")
            {
                strError = "读者证条码号为空，无法定位读者记录。";
                goto ERROR1;
            }

            string strXml = "";
            string strOutputPath = "";
            // 获得读者记录
            int nRet = app.GetReaderRecXml(
                sessioninfo.Channels,
                strBarcode,
                out strXml,
                out strOutputPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
                goto ERROR1;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载读者XML记录进入DOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // 兑现识别信息
            RenderIdentity(dom);

            // 兑现一般信息
            RenderGeneralInfo(dom);

            // 兑现 违约信息
            RenderOverdue(app,
                sessioninfo,
                dom);

            // 兑现 借阅信息
            RenderBorrow(app,
                sessioninfo,
                dom);

            // 兑现 预约请求
            RenderReservation(app,
                sessioninfo,
                dom);



            base.Render(output);
            return;

        ERROR1:
            output.Write(strError);
        }




        static string MakeBarcodeListHyperLink(string strBarcodes,
            string strSep)
        {
            string strResult = "";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int i = 0; i < barcodes.Length; i++)
            {
                string strBarcode = barcodes[i];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                if (strResult != "")
                    strResult += strSep;
                strResult += "<a href='book.aspx?barcode=" + strBarcode + "&forcelogin=on'>"
                    + strBarcode + "</a>";
            }

            return strResult;
        }

        /*
        protected override void RenderContents(HtmlTextWriter output)
        {
            // output.Write(Text);

            string strError = "";
            string strResult = "";
            int nRet = GetReaderHtml(
                this.App,
                this.SessionInfo,
                this.Barcode,
                out strResult,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                output.Write(strError);
                return;
            }

            output.Write(strResult);
        }

        static int GetReaderHtml(
            CirculationApplication app,
            SessionInfo sessioninfo,
            string strBarcode,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            string strXml = "";
            string strOutputPath = "";

            int nRet = app.GetReaderXml(
                sessioninfo,
                strBarcode,
                out strXml,
                out strOutputPath,
                out strError);
            if (nRet == 0)
            {
                strError = "条码为 '"+strBarcode+"' 的读者记录没有找到";
                return 0;
            }
            if (nRet == -1)
                goto ERROR1;

            // 将读者记录数据从XML格式转换为HTML格式
            nRet = app.ConvertReaderXmlToHtml(
                app.CfgDir + "\\readeropac.cs",
                app.CfgDir + "\\readeropac.cs.ref",
                strXml,
                OperType.None,
                null,
                "",
                out strResult,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 1;
        ERROR1:
            return -1;
        }
         */

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
    }
#endif
}
