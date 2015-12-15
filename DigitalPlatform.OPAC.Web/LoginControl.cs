using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:LoginControl runat=server></{0}:LoginControl>")]
    public class LoginControl : WebControl, INamingContainer
    {
        // public LoginColumn ActiveLoginColumn = LoginColumn.Barcode;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.LoginControl.cs",
                typeof(LoginControl).Module.Assembly);

            return this.m_rm;
        }

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name/*"en-US"*/);

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

        LoginColumn ActiveLoginColumn
        {
            get
            {
                object o = ViewState["ActiveLoginColumn"];
                return (o == null) ? LoginColumn.Barcode : (LoginColumn)o;
            }
            set
            {
                ViewState["ActiveLoginColumn"] = (object)value;
            }
        }

        public void AdjustActiveColumn()
        {
            if ((this.LoginStyle & LoginStyle.Librarian) == 0
                && this.ActiveLoginColumn == LoginColumn.ID)
                this.ActiveLoginColumn = LoginColumn.Barcode;

            if ((this.LoginStyle & LoginStyle.Reader) == 0
                && this.ActiveLoginColumn != LoginColumn.ID)
                this.ActiveLoginColumn = LoginColumn.ID;
        }

        /*
        public LoginColumn ActiveLoginColumn
        {
            get
            {
                object o = this.Page.Session[this.ID + "LoginControl_ActiveLoginColumn"];
                return (o == null) ? LoginColumn.Barcode : (LoginColumn)o;
            }
            set
            {
                this.Page.Session[this.ID + "LoginControl_ActiveLoginColumn"] = (object)value;
            }
        }
         * */

        public event LoginEventHandler Login;
        public event LoginEventHandler AnonymouseLogin;

        public LoginStyle LoginStyle
        {
            get
            {
                object o = ViewState["LoginStyle"];
                return (o == null) ? LoginStyle.Reader : (LoginStyle)o;
            }
            set
            {
                ViewState["LoginStyle"] = (object)value;
            }
        }

        // 是否处在多选状态
        public bool InMultpleSelection
        {
            get
            {
                object o = ViewState["InMultpleSelection"];
                return (o == null) ? false : (bool)o;
            }
            set
            {
                ViewState["InMultpleSelection"] = (object)value;
            }
        }

        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        // 设置面板值
        public void SetValue(string strLoginName, string strPassword, bool bKeepLogin)
        {
            string strPrefix = "";
            string strName = "";
            StringUtil.ParseTwoPart(strLoginName, ":", out strPrefix, out strName);
            if (string.IsNullOrEmpty(strName) == true)
            {
                strName = strPrefix;
                strPrefix = "";
            }

            this.ActiveLoginColumn = GetColumnByPrefix(strPrefix);

            if (this.ActiveLoginColumn == LoginColumn.ID)
            {
                if ((this.LoginStyle & Web.LoginStyle.Librarian) == 0)
                    this.LoginStyle = Web.LoginStyle.Librarian;
            }
            else
            {
                if ((this.LoginStyle & Web.LoginStyle.Reader) == 0)
                    this.LoginStyle = Web.LoginStyle.Reader;
            }

            this.EnsureChildControls();

            TextBox password = (TextBox)this.FindControl("password");
            password.Text = strPassword;

            if (this.ActiveLoginColumn == LoginColumn.Barcode)
            {
                TextBox barcode = (TextBox)this.FindControl("barcode");
                barcode.Text = strName;
            }
            else if (this.ActiveLoginColumn == LoginColumn.NameBirthdate)
            {
                TextBox name = (TextBox)this.FindControl("name");
                TextBox dateOfBirth = (TextBox)this.FindControl("dateOfBirth");
                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(strName, "|", out strLeft, out strRight);
                name.Text = strLeft;
                dateOfBirth.Text = strRight;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Email)
            {
                TextBox email = (TextBox)this.FindControl("email");
                email.Text = strName;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Telephone)
            {
                TextBox tel = (TextBox)this.FindControl("tel");
                tel.Text = strName;
            }
            else if (this.ActiveLoginColumn == LoginColumn.IdCardNumber)
            {
                TextBox tel = (TextBox)this.FindControl("idcardnumber");
                tel.Text = strName;
            }
            else if (this.ActiveLoginColumn == LoginColumn.CardNumber)
            {
                TextBox tel = (TextBox)this.FindControl("cardnumber");
                tel.Text = strName;
            }
            else if (this.ActiveLoginColumn == LoginColumn.ID)
            {
                TextBox userid = (TextBox)this.FindControl("userid");
                userid.Text = strName;
            }

            this.KeepLogin = bKeepLogin;
        }

        protected override void CreateChildControls()
        {
            // 总表格
            LiteralControl literal = new LiteralControl();
            literal.Text =
                this.GetPrefixString(
                this.GetString("登录"),
                "login_wrapper")
                + "<table class='login'>";
            this.Controls.Add(literal);

            // tab栏目
            literal = new LiteralControl();
            literal.Text = "<tr><td>";
            this.Controls.Add(literal);

            List<LoginColumn> columns = this.CreateColumns();

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            this.Controls.Add(literal);

            // 内容区
            literal = new LiteralControl();
            literal.Text = "<tr><td><table class='content'>";
            this.Controls.Add(literal);

            /*
            // 空白行
            PlaceHolder blankline = new PlaceHolder();
            blankline.ID = "blankline";
            this.Controls.Add(blankline);

            CreateBlankLine(blankline);
             * */

            // UserID
            PlaceHolder userid = new PlaceHolder();
            userid.ID = "useridline";
            this.Controls.Add(userid);

            CreateUserID(userid);

            // barcode
            PlaceHolder barcode = new PlaceHolder();
            barcode.ID = "barcodeline";
            this.Controls.Add(barcode);

            CreateBarcode(barcode);

            // 名字+生日
            PlaceHolder namebirthdate = new PlaceHolder();
            namebirthdate.ID = "namebirthdateline";
            this.Controls.Add(namebirthdate);

            CreateNameBirthdate(namebirthdate);

            // email地址
            PlaceHolder email = new PlaceHolder();
            email.ID = "emailline";
            this.Controls.Add(email);

            CreateEmail(email);

            // 电话号码
            PlaceHolder telepone = new PlaceHolder();
            telepone.ID = "telephoneline";
            this.Controls.Add(telepone);

            CreateTelephone(telepone);

            // 身份证号
            PlaceHolder idcardnumber = new PlaceHolder();
            idcardnumber.ID = "idcardnumberline";
            this.Controls.Add(idcardnumber);

            CreateIdCardNumber(idcardnumber);

            // 证号
            PlaceHolder cardnumber = new PlaceHolder();
            cardnumber.ID = "cardnumberline";
            this.Controls.Add(cardnumber);

            CreateCardNumber(cardnumber);

            // 密码
            PlaceHolder password = new PlaceHolder();
            password.ID = "passwordline";
            this.Controls.Add(password);

            CreatePassword(password);

            // 是否保持登录状态
            PlaceHolder keeplogin = new PlaceHolder();
            keeplogin.ID = "keeploginline";
            this.Controls.Add(keeplogin);

            CreateKeepLogin(keeplogin);

            // 多选
            PlaceHolder multiple = new PlaceHolder();
            multiple.ID = "multipleline";
            this.Controls.Add(multiple);

            CreateMultipleLine(multiple);

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "cmdline";
            this.Controls.Add(cmdline);

            CreateCmdLine(cmdline);

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);

            /*
            // 占位符
            PlaceHolder placeholder = new PlaceHolder();
            placeholder.ID = "content_holder";
            this.Controls.Add(placeholder);
             * */

            /*
            // 空白行
            blankline = new PlaceHolder();
            blankline.ID = "blankline2";
            this.Controls.Add(blankline);

            CreateBlankLine(blankline);
             * */
            literal = new LiteralControl();
            literal.Text = "</table></td></tr>";
            this.Controls.Add(literal);

            // 总表格收尾
            literal = new LiteralControl();
            literal.Text = "</table>" + this.GetPostfixString();
            this.Controls.Add(literal);

            // 修正ActiveLoginColumn值
            if (columns.Count > 0 && columns.IndexOf(this.ActiveLoginColumn) == -1)
            {
                if ((this.LoginStyle & LoginStyle.Librarian) != 0)
                {
                    if (columns.IndexOf(LoginColumn.ID) != -1)
                        this.ActiveLoginColumn = LoginColumn.ID;
                    else if (columns.IndexOf(LoginColumn.Librarian) != -1)
                        this.ActiveLoginColumn = LoginColumn.Librarian;
                    else
                        this.ActiveLoginColumn = columns[0];
                }
                else if ((this.LoginStyle & LoginStyle.Librarian) == 0
                    && (this.LoginStyle & LoginStyle.Reader) != 0)
                {
                    // 寻找第一个非Guest的栏目
                    LoginColumn found = LoginColumn.None;
                    foreach (LoginColumn column in columns)
                    {
                        if (column != LoginColumn.Guest
                            && column != LoginColumn.ID
                            && column != LoginColumn.Librarian)
                        {
                            found = column;
                            break;
                        }
                    }

                    if (found == LoginColumn.None)
                        found = columns[0];
                    if (found != LoginColumn.None)
                        this.ActiveLoginColumn = found;
                }
            }

        }

        public bool AllowBlankPassword
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                bool bValue = true;
                XmlNode node = app.WebUiDom.DocumentElement.SelectSingleNode("loginControl");
                if (node != null)
                {
                    bValue = DomUtil.GetBooleanParam(node,
                        "allowBlankPassword",
                        true);
                }

                return bValue;
            }
        }

        // 是否出现 “找回密码” 链接
        public bool ResetPassword
        {
            get
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                bool bValue = true;
                XmlNode node = app.WebUiDom.DocumentElement.SelectSingleNode("loginControl");
                if (node != null)
                {
                    bValue = DomUtil.GetBooleanParam(node,
                        "resetPassword",
                        false);
                }

                return bValue;
            }
        }

        // 根据webui.xml配置文件定义，创建读者可用的几个tab column
        List<LoginColumn> CreateReaderColumns()
        {
            List<LoginColumn> results = new List<LoginColumn>();

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
                return results;
            XmlNodeList node_pages = app.WebUiDom.DocumentElement.SelectNodes("loginControl/page");

            // 缺省的效果
            if (node_pages.Count == 0)
            {
                // 访客登录
                CreateOneColumn(LoginColumn.Guest.ToString(),
                    this.GetString("访客登录"),
                    "./login.aspx?action=publiclogin");
                results.Add(LoginColumn.Guest);

                // 证条码号
                CreateOneColumn(LoginColumn.Barcode.ToString(),
                    this.GetString("证条码号"));
                results.Add(LoginColumn.Barcode);

                // 姓名+生日
                CreateOneColumn(LoginColumn.NameBirthdate.ToString(),
                    this.GetString("姓名加生日"));   // "姓名+生日"
                results.Add(LoginColumn.NameBirthdate);

                // Email
                CreateOneColumn(LoginColumn.Email.ToString(),
                    this.GetString("Email"));
                results.Add(LoginColumn.Email);

                // 电话
                CreateOneColumn(LoginColumn.Telephone.ToString(),
                    this.GetString("电话号码"));
                results.Add(LoginColumn.Telephone);

                // 身份证号
                CreateOneColumn(LoginColumn.IdCardNumber.ToString(),
                    this.GetString("身份证号"));
                results.Add(LoginColumn.IdCardNumber);

                // 馆员
                CreateOneColumn(LoginColumn.Librarian.ToString(),
                    this.GetString("馆员"),
                    "./login.aspx?loginstyle=librarian");
                results.Add(LoginColumn.Librarian);
                return results;
            }

            foreach (XmlNode node in node_pages)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strCaption = DomUtil.GetCaption(this.Lang,
                    node);

                if (strName == "证条码号")
                {
                    // 证条码号
                    CreateOneColumn(LoginColumn.Barcode.ToString(),
                        string.IsNullOrEmpty(strCaption) == false? strCaption : this.GetString("证条码号"));
                    results.Add(LoginColumn.Barcode);
                }
                else if (strName == "姓名")
                {
                    // 姓名+生日
                    CreateOneColumn(LoginColumn.NameBirthdate.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("姓名加生日"));   // "姓名+生日"
                    results.Add(LoginColumn.NameBirthdate);
               }
                else if (strName == "Email")
                {
                    // Email
                    CreateOneColumn(LoginColumn.Email.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("Email"));
                    results.Add(LoginColumn.Email);
               }
                else if (strName == "电话号码")
                {
                    // 电话
                    CreateOneColumn(LoginColumn.Telephone.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("电话号码"));
                    results.Add(LoginColumn.Telephone);
                }
                else if (strName == "身份证号")
                {
                    // 身份证号
                    CreateOneColumn(LoginColumn.IdCardNumber.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("身份证号"));
                    results.Add(LoginColumn.IdCardNumber);
                }
                else if (strName == "证号")
                {
                    // 证号
                    CreateOneColumn(LoginColumn.CardNumber.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("证号"));
                    results.Add(LoginColumn.CardNumber);
               }
                else if (strName == "访客登录")
                {
                    // 访客登录
                    CreateOneColumn(LoginColumn.Guest.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("访客登录"),
                    "./login.aspx?action=publiclogin");
                    results.Add(LoginColumn.Guest);
                }
                else if (strName == "馆员")
                {
                    // 馆员
                    CreateOneColumn(LoginColumn.Librarian.ToString(),
                        string.IsNullOrEmpty(strCaption) == false ? strCaption : this.GetString("馆员"),
                        "./login.aspx?loginstyle=librarian");
                    results.Add(LoginColumn.Librarian);
                }
            }

            return results;
        }

        // return:
        //      返回实际创建的栏目编号
        List<LoginColumn> CreateColumns()
        {
            List<LoginColumn> results = new List<LoginColumn>();

            // 栏目表格
            LiteralControl literal = new LiteralControl();
            literal.Text = "<div class='columns'>";
            this.Controls.Add(literal);

            // 左边空白
            literal = new LiteralControl();
            literal.Text = "<div class='leftblank'>&nbsp;</div>";
            this.Controls.Add(literal);

            if ((this.LoginStyle & LoginStyle.Librarian) == LoginStyle.Librarian)
            {
                // ID
                CreateOneColumn(LoginColumn.ID.ToString(),
                    this.GetString("ID"));
                results.Add(LoginColumn.ID);

                // 读者
                CreateOneColumn(LoginColumn.Patron.ToString(),
                    this.GetString("读者"),
                    "./login.aspx?loginstyle=reader");
                results.Add(LoginColumn.Patron);
            }

            if ((this.LoginStyle & LoginStyle.Reader) == LoginStyle.Reader)
            {
#if NO
                // 证条码号
                CreateOneColumn(LoginColumn.Barcode.ToString(),
                    this.GetString("证条码号"));


                // 姓名+生日
                CreateOneColumn(LoginColumn.NameBirthdate.ToString(),
                    this.GetString("姓名加生日"));   // "姓名+生日"

                // Email
                CreateOneColumn(LoginColumn.Email.ToString(),
                    this.GetString("Email"));

                // 电话
                CreateOneColumn(LoginColumn.Telephone.ToString(),
                    this.GetString("电话号码"));

                // 身份证号
                CreateOneColumn(LoginColumn.IdCardNumber.ToString(),
                    this.GetString("身份证号"));

                // 访客登录
                CreateOneColumn(LoginColumn.Guest.ToString(),
                    this.GetString("访客登录"));

                // 馆员
                CreateOneColumn(LoginColumn.Librarian.ToString(),
                    this.GetString("馆员"));
#endif
                results.AddRange( CreateReaderColumns() );
            }

            // 右边空白
            literal = new LiteralControl();
            literal.Text = "<div class='rightblank'>&nbsp;</div>";
            this.Controls.Add(literal);

            // 栏目表格收尾
            literal = new LiteralControl();
            literal.Text = "</div>";
            this.Controls.Add(literal);

            return results;
        }

        void CreateOneColumn(string strColumnName,
            string strColumnCaption)
        {
            LiteralControl literal = new LiteralControl();
            literal.Text = "<div class='";
            this.Controls.Add(literal);

            // 可以替换的class值
            literal = new LiteralControl();
            literal.ID = strColumnName + "_class";
            literal.Text = "switchdown";    // 缺省值
            this.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "'>";
            this.Controls.Add(literal);

            LinkButton barcode_button = new LinkButton();
            barcode_button.ID = "_" + strColumnName;
            barcode_button.Text = strColumnCaption;
            barcode_button.Click += new EventHandler(barcode_button_Click);
            this.Controls.Add(barcode_button);

            literal = new LiteralControl();
            literal.Text = "</div>";
            this.Controls.Add(literal);
        }

        // 创建 hyperlink 的栏目标题
        void CreateOneColumn(string strColumnName,
            string strColumnCaption,
            string strURL)
        {
            LiteralControl literal = new LiteralControl();
            literal.Text = "<div class='";
            this.Controls.Add(literal);

            // 可以替换的class值
            literal = new LiteralControl();
            literal.ID = strColumnName + "_class";
            literal.Text = "switchdown";    // 缺省值
            this.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "'>";
            this.Controls.Add(literal);

            HyperLink link = new HyperLink();
            link.Text = strColumnCaption;
            link.NavigateUrl = strURL;
            this.Controls.Add(link);

            literal = new LiteralControl();
            literal.Text = "</div>";
            this.Controls.Add(literal);
        }

        void barcode_button_Click(object sender, EventArgs e)
        {
            LinkButton button = (LinkButton)sender;
            this.ActiveLoginColumn = (LoginColumn)Enum.Parse(typeof(LoginColumn), button.ID.Substring(1));

            // TODO: 下面一段似乎不会用到了
            if (this.ActiveLoginColumn == LoginColumn.Librarian)
                this.Page.Response.Redirect("./login.aspx?loginstyle=librarian", true);
            else if (this.ActiveLoginColumn == LoginColumn.Patron)
                this.Page.Response.Redirect("./login.aspx?loginstyle=reader", true);
            else if (this.ActiveLoginColumn == LoginColumn.Guest)
            {
                if (this.AnonymouseLogin != null)
                {
                    LoginEventArgs ea = new LoginEventArgs();
                    this.AnonymouseLogin(this, ea);
                }
            }
        }

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            return "<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
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

        void CreatePassword(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='password' align='left'><td class='name'>"
                + this.GetString("密码")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "password";
            textbox.CssClass = "password";
            textbox.TextMode = TextBoxMode.Password;
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateKeepLogin(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='keeplogin' align='left'><td class='name'>"
                + ""    // this.GetString("密码")
                + "</td><td class='value'>"));

            CheckBox checkbox = new CheckBox();
            checkbox.ID = "keeplogin";
            checkbox.Text = "使我保持登录状态";
            checkbox.CssClass = "keeplogin";
            checkbox.AutoPostBack = true;
            line.Controls.Add(checkbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        public bool KeepLogin
        {
            get
            {
                this.EnsureChildControls();

                CheckBox checkbox = (CheckBox)this.FindControl("keeplogin");
                if (checkbox == null)
                    return false;

                return checkbox.Checked;
            }
            set
            {
                this.EnsureChildControls();

                CheckBox checkbox = (CheckBox)this.FindControl("keeplogin");
                if (checkbox != null)
                {
                    checkbox.Checked = value;
                }
            }
        }

        void CreateBlankLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='blank' align='left'><td class='name'></td><td class='value'></td></tr>"));
        }

        void CreateBarcode(PlaceHolder line)
        {
            string strCaption = this.GetCfgCaption("证条码号");
            if (string.IsNullOrEmpty(strCaption) == true)
                strCaption = this.GetCaption("读者证条码号");
            line.Controls.Add(new LiteralControl("<tr class='barcode' align='left'><td class='name'>"
                + strCaption
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "barcode";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateUserID(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='userid' align='left'><td class='name'>"
                + this.GetString("用户ID")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "userid";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateEmail(PlaceHolder line)
        {
            string strCaption = this.GetCfgCaption("Email");
            if (string.IsNullOrEmpty(strCaption) == true)
                strCaption = this.GetCaption("Email地址");
            line.Controls.Add(new LiteralControl("<tr class='email' align='left'><td class='name'>"
                + strCaption
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "email";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateTelephone(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='telephone' align='left'><td class='name'>"
                + this.GetCaption("电话号码")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "tel";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateIdCardNumber(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='idcardnumber' align='left'><td class='name'>"
                + this.GetCaption("身份证号")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "idcardnumber";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        string GetCfgCaption(string strName)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            XmlNode node = app.WebUiDom.DocumentElement.SelectSingleNode("loginControl/page[@name='" + strName + "']");
            if (node != null)
            {
                string strCaption = DomUtil.GetCaption(this.Lang,
        node);
                if (string.IsNullOrEmpty(strCaption) == false)
                    return strCaption;
            }

            return null;
        }

        string GetCaption(string strName)
        {
            string strCaption = GetCfgCaption(strName);
            if (string.IsNullOrEmpty(strCaption) == false)
                return strCaption;

            return this.GetString(strName);
        }

        void CreateCardNumber(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='cardnumber' align='left'><td class='name'>"
                + this.GetCaption("证号")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "cardnumber";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        // 帐户命中多条时复选
        void CreateMultipleLine(PlaceHolder line)
        {
            // 注释
            line.Controls.Add(new LiteralControl("<tr class='multiple'><td class='value' colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.Text = "<div class='comment'>";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "multiple_comment";
            literal.Text = "test comment";
            line.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</div>";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));

#if NO
            // textbox输入域
            line.Controls.Add(new LiteralControl("<tr class='multiple' align='left'><td class='name'>"
                + this.GetString("序号")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "multiple_no";
            line.Controls.Add(textbox);
#endif
            // textbox输入域
            line.Controls.Add(new LiteralControl("<tr class='multiple' align='left'><td class='name'>"
                + this.GetString("出生日期")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "dateOfBirth";
            line.Controls.Add(textbox);

            literal = new LiteralControl();
            literal.Text = "<br/><div class='comment'>"
                + this.GetString("当姓名发生重复时需填此项")
                //"注: 当姓名发生重复时需填此项。<br/>格式为8位数字(年[4位]月[2位]日[2位])，例: 19710401"
                + "</div>";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateNameBirthdate(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='name' align='left'><td class='name'>"
                + this.GetCaption("姓名")
                + "</td><td class='value'>"));

            TextBox textbox = new TextBox();
            textbox.ID = "name";
            line.Controls.Add(textbox);

            line.Controls.Add(new LiteralControl("</td></tr>"));

#if NO
            line.Controls.Add(new LiteralControl("<tr class='birthday' style='display:NONE' align='left'><td class='name'>"
                + this.GetString("生日")
                + "</td><td class='value'>"));

            textbox = new TextBox();
            textbox.ID = "birthday";
            line.Controls.Add(textbox);

            LiteralControl literal = new LiteralControl();
            literal.Text = "<br/><div class='comment'>"
                + this.GetString("当姓名发生重复时需填此项")
                //"注: 当姓名发生重复时需填此项。<br/>格式为8位数字(年[4位]月[2位]日[2位])，例: 19710401"
                + "</div>";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));
#endif
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='debugline' align='left'><td class='content' colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        public void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            text.Text = strText;
        }

        public void SetDebugInfo(string strSpanClass,
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

        void CreateCmdLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<tr class='cmdline'><td class='left' /><td class='content'>"));

            Button loginbutton = new Button();
            loginbutton.ID = "loginbutton";
            loginbutton.CssClass = "loginbutton";
            loginbutton.Text = this.GetString("登录");
            loginbutton.Click += new EventHandler(loginbutton_Click);
            line.Controls.Add(loginbutton);

#if NO
            Button anonymouseloginbutton = new Button();
            anonymouseloginbutton.ID = "anonymouseloginbutton";
            anonymouseloginbutton.CssClass = "anonymouseloginbutton";
            anonymouseloginbutton.Text = this.GetString("访客登录");
            anonymouseloginbutton.Click += new EventHandler(anonymouseloginbutton_Click);
            line.Controls.Add(anonymouseloginbutton);
#endif

            if (this.ResetPassword == true)
            {
                HyperLink link = new HyperLink();
                link.ID = "resetpassword";
                link.CssClass = "resetpassword";
                link.Text = this.GetString("找回密码");
                link.NavigateUrl = "./resetpassword.aspx";
                line.Controls.Add(link);
            }

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            // DropDownList loginstyle = (DropDownList)this.FindControl("loginstyle");

            PlaceHolder useridline = (PlaceHolder)this.FindControl("useridline");
            PlaceHolder barcodeline = (PlaceHolder)this.FindControl("barcodeline");
            PlaceHolder namebirthdateline = (PlaceHolder)this.FindControl("namebirthdateline");
            PlaceHolder emailline = (PlaceHolder)this.FindControl("emailline");
            PlaceHolder telephoneline = (PlaceHolder)this.FindControl("telephoneline");
            PlaceHolder idcardnumberline = (PlaceHolder)this.FindControl("idcardnumberline");
            PlaceHolder cardnumberline = (PlaceHolder)this.FindControl("cardnumberline");
            PlaceHolder multipleline = (PlaceHolder)this.FindControl("multipleline");

            string strClassName = this.ActiveLoginColumn.ToString() + "_class";
            LiteralControl classtext = (LiteralControl)this.FindControl(strClassName);
            if (classtext != null)
                classtext.Text = "switchup";

            if (this.ActiveLoginColumn == LoginColumn.ID)
            {
                useridline.Visible = true;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Barcode)
            {
                useridline.Visible = false;
                barcodeline.Visible = true;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.NameBirthdate)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = true;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Email)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = true;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Telephone)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = true;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.IdCardNumber)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = true;
                cardnumberline.Visible = false;
            }
            else if (this.ActiveLoginColumn == LoginColumn.CardNumber)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = true;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Guest)
            {
                useridline.Visible = false;
                barcodeline.Visible = false;
                namebirthdateline.Visible = false;
                emailline.Visible = false;
                telephoneline.Visible = false;
                idcardnumberline.Visible = false;
                cardnumberline.Visible = false;
            }
            else
            {
                throw new Exception("不支持的ActiveLoginColumn事项");
            }

            if (this.InMultpleSelection == true)
                multipleline.Visible = true;
            else
                multipleline.Visible = false;

            Button loginbutton = (Button)this.FindControl("loginbutton");
            if (loginbutton != null)
            {
                LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

                if (loginstate != LoginState.NotLogin
                    && loginstate != LoginState.Public)
                {
                    loginbutton.Text = this.GetString("重新登录");
                }
            }

            base.Render(writer);
        }

        void loginbutton_Click(object sender, EventArgs e)
        {
            if (this.Login != null)
            {
                LoginEventArgs ea = new LoginEventArgs();
                this.Login(this, ea);
            }
        }

        public int DoAnonymouseLogin(out string strError)
        {
            strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            // 触发清除读者记录缓存
            if (sessioninfo != null)
                sessioninfo.Clear();

            // 工作人员public登录
            /*
            nRet = sessioninfo.Login("public",
                     "",
                     "#web",
                     false,
                     out strError);
             * */
            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            string strParameters = "location=#opac@" + sessioninfo.ClientIP + ",type=worker";
            long lRet = sessioninfo.Login("public",
                         "",
                         strParameters,
                         "",
                         out strError);
            if (lRet != 1)
                goto ERROR1;

            return 0;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
            return -1;
        }

        // parameters:
        //      strUserName 读者证条码号。或者 "NB:姓名|出生日期(8字符)" "EM:email地址" "TP:电话号码" "ID:身份证号"
        //      strUserType reader/librarian
        // return:
        //      -1  error
        //      0   成功
        public int DoLogin(
            string strUserName,
            string strPassword,
            string strUserType,
            string strLibraryCode,
            out string strError)
        {
            strError = "";

            if (this.AllowBlankPassword == false
                && string.IsNullOrEmpty(strPassword) == true)
            {
                strError = "系统不允许使用空密码登录";
                return -1;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            // 触发清除读者记录缓存
            if (sessioninfo != null)
                sessioninfo.Clear();

            long lRet = 0;
            string strParameters = "location=#opac@" + sessioninfo.ClientIP + "";

            if (strUserType == "librarian")
            {
                // 检查几个特殊帐户
                if (strUserName == "public"
                    || strUserName == "reader"
                    )   // || userid.Text == app.ManagerUserName 有时候代理帐户是兼职的，不让登录OPAC似乎太严格了一点
                {
                    strError = "不能使用保留帐户直接登录OPAC";
                    goto ERROR1;
                }

                // 工作人员登录
                strParameters += ",type=worker";
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                lRet = sessioninfo.Login(strUserName,
                    strPassword,
                    strParameters,
                    this.KeepLogin == true ? "month" : "day",
                    out strError);
                if (lRet == 1)
                {
                    // 在 Cookies 里面留下痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        if (this.KeepLogin == true)
                            page.SetCookiesLogin("WK:" + strUserName, null, 1, 1);
                        else
                            page.SetCookiesLogin("WK:" + strUserName, null, -1, 1);
                    }
                }
                else
                {
                    // 在 Cookies 里面清除痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        page.ClearCookiesLogin("password");
                        page.SetCookiesLogin(null, null, 0, -1);    // cookies offline
                    }
                }
            }
            else if (strUserType == "reader")
            {
                // 读者身份登录
                strParameters += ",type=reader,libraryCode=" + GetLibraryCodeParam(strLibraryCode);
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                lRet = sessioninfo.Login(strUserName,
                             strPassword,
                             strParameters,
                             this.KeepLogin == true ? "month" : "day",
                             out strError);
                if (lRet == 1)
                {
                    // 在 Cookies 里面留下痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        if (this.KeepLogin == true)
                            page.SetCookiesLogin(strUserName, null, 1, 1);
                        else
                            page.SetCookiesLogin(strUserName, null, -1, 1);
                    }
                }
                else
                {
                    // 在 Cookies 里面清除痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        page.ClearCookiesLogin("password");
                        page.SetCookiesLogin(null, null, 0, -1);    // cookies offline
                    }
                }

                if (lRet > 1)
                {
                    return -1;
                }
            }
            else
            {
                strError = "未能识别的 strUserType '"+strUserType+"'";
                return -1;
            }

            if (lRet != 1)
                goto ERROR1;

            return 0;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
            return -1;
        }

        static LoginColumn GetColumnByPrefix(string strPrefix)
        {
            if (string.IsNullOrEmpty(strPrefix) == true)
                return LoginColumn.Barcode;
            else if (strPrefix == "NB")
                return LoginColumn.NameBirthdate;
            else if (strPrefix == "EM")
                return LoginColumn.Email;
            else if (strPrefix == "TP")
                return LoginColumn.Telephone;
            else if (strPrefix == "ID")
                return LoginColumn.IdCardNumber;
            else if (strPrefix == "CN")
                return LoginColumn.CardNumber;
            else if (strPrefix == "WK")
                return LoginColumn.ID;
            return LoginColumn.Barcode;
        }

        // 获得栏目的前缀字符串
        static string GetColumnPrefix(LoginColumn column)
        {
            if (column == LoginColumn.Barcode)
            {
                return "";
            }
            else if (column == LoginColumn.NameBirthdate)
            {
                return "NB";
            }
            else if (column == LoginColumn.Email)
            {
                return "EM";
            }
            else if (column == LoginColumn.Telephone)
            {
                return "TP";
            }
            else if (column == LoginColumn.IdCardNumber)
            {
                return "ID";
            }
            else if (column == LoginColumn.CardNumber)
            {
                return "CN";
            }
            else if (column == LoginColumn.ID)
                return "WK";

            return "";
        }

        public void RestorePassword()
        {
            this.EnsureChildControls();

            TextBox password = (TextBox)this.FindControl("password");
            password.Attributes["value"] = password.Text;
        }

        public int DoLogin(string strLibraryCode,
            out string strError)
        {
            strError = "";
            string strUserName = "";
            // int nRet = 0;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            // 触发清除读者记录缓存
            if (sessioninfo != null)
                sessioninfo.Clear();

            TextBox password = (TextBox)this.FindControl("password");

            if (this.AllowBlankPassword == false
    && string.IsNullOrEmpty(password.Text) == true)
            {
                strError = "系统不允许使用空密码登录";
                goto ERROR1;
            }
            
            if (this.ActiveLoginColumn == LoginColumn.Barcode)
            {
                TextBox barcode = (TextBox)this.FindControl("barcode");
                if (barcode.Text == "")
                {
                    strError = this.GetString("读者证条码号不能为空");
                    goto ERROR1;
                }
                strUserName = barcode.Text;
            }
            else if (this.ActiveLoginColumn == LoginColumn.NameBirthdate)
            {
                TextBox name = (TextBox)this.FindControl("name");
                TextBox dateOfBirth = (TextBox)this.FindControl("dateOfBirth");
                if (name.Text == "")
                {
                    strError = this.GetString("姓名不能为空");
                    goto ERROR1;
                }
                if (dateOfBirth.Text.Length > 8)
                {
                    strError = this.GetString("出生日期应为8位数字以内");   // "出生日期应为8位数字以内。例如'19710401'"
                    goto ERROR1;
                }
                strUserName = "NB:" + name.Text + "|" + dateOfBirth.Text;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Email)
            {
                TextBox email = (TextBox)this.FindControl("email");
                if (email.Text == "")
                {
                    strError = this.GetString("Email地址不能为空");
                    goto ERROR1;
                }
                strUserName = "EM:" + email.Text;
            }
            else if (this.ActiveLoginColumn == LoginColumn.Telephone)
            {
                TextBox tel = (TextBox)this.FindControl("tel");
                if (tel.Text == "")
                {
                    strError = this.GetString("电话号码不能为空");
                    goto ERROR1;
                }
                strUserName = "TP:" + tel.Text;
            }
            else if (this.ActiveLoginColumn == LoginColumn.IdCardNumber)
            {
                TextBox tel = (TextBox)this.FindControl("idcardnumber");
                if (tel.Text == "")
                {
                    strError = this.GetString("身份证号不能为空");
                    goto ERROR1;
                }
                strUserName = "ID:" + tel.Text;
            }
            else if (this.ActiveLoginColumn == LoginColumn.CardNumber)
            {
                TextBox tel = (TextBox)this.FindControl("cardnumber");
                if (tel.Text == "")
                {
                    strError = this.GetString("证号不能为空");
                    goto ERROR1;
                }
                strUserName = "CN:" + tel.Text;
            }

            long lRet = 0;
            string strParameters = "location=#opac@" + sessioninfo.ClientIP + "";

            if (this.ActiveLoginColumn == LoginColumn.ID)
            {
                TextBox userid = (TextBox)this.FindControl("userid");
                if (userid.Text == "")
                {
                    strError = this.GetString("用户ID不能为空");
                    goto ERROR1;
                }

                // 检查几个特殊帐户
                if (userid.Text == "public"
                    || userid.Text == "reader"
                    )   // || userid.Text == app.ManagerUserName 有时候代理帐户是兼职的，不让登录OPAC似乎太严格了一点
                {
                    strError = "不能使用保留帐户直接登录OPAC";
                    goto ERROR1;
                }

                /*
                // 工作人员登录
                nRet = sessioninfo.Login(userid.Text,
                     password.Text,
                     "#web",
                     false,
                     out strError);
                 * */
                strParameters += ",type=worker";
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                lRet = sessioninfo.Login(userid.Text,
                             password.Text,
                             strParameters,
                             this.KeepLogin == true ? "month" : "day",
                             out strError);
                if (lRet == 1)
                {
                    // 在 Cookies 里面留下痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        if (this.KeepLogin == true)
                            page.SetCookiesLogin( "WK:" + userid.Text, null,1, 1);
                        else
                            page.SetCookiesLogin( "WK:" + userid.Text, null,-1, 1);
                    }
                }
                else
                {
                    // 在 Cookies 里面清除痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        page.ClearCookiesLogin("password");
                        page.SetCookiesLogin(null, null, 0, -1);    // cookies offline
                    }
                }
            }
            else
            {
                int nIndex = -1;
                if (this.InMultpleSelection == true)
                {
                    /*
                    TextBox multiple_no = (TextBox)this.FindControl("multiple_no");

                    if (multiple_no.Text != "")
                    {
                        try
                        {
                            nIndex = Convert.ToInt32(multiple_no.Text);
                        }
                        catch
                        {
                        }
                    }

                    strParameters += ",index=" + nIndex.ToString();
                     * */
                }


                /*
                // 读者身份登录
                nRet = app.LoginForReader(sessioninfo,
                    strUserName,
                    password.Text,
                     "#web",
                    nIndex,
                    out strError);
                 * */
                strParameters += ",type=reader,libraryCode=" + GetLibraryCodeParam(strLibraryCode);
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                lRet = sessioninfo.Login(strUserName,
                             password.Text,
                             strParameters,
                             this.KeepLogin == true ? "month" : "day",
                             out strError);
                if (lRet == 1)
                {
                    // 在 Cookies 里面留下痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        if (this.KeepLogin == true)
                            page.SetCookiesLogin(strUserName, null, 1, 1);
                        else
                            page.SetCookiesLogin(strUserName, null, -1, 1);
                    }
                }
                else
                {
                    // 在 Cookies 里面清除痕迹
                    if (this.Page is MyWebPage)
                    {
                        MyWebPage page = this.Page as MyWebPage;
                        page.ClearCookiesLogin("password");
                        page.SetCookiesLogin(null, null, 0, -1);    // cookies offline
                    }
                }

                if (lRet > 1)
                {
                    this.InMultpleSelection = true;
                    LiteralControl multiple_comment = (LiteralControl)this.FindControl("multiple_comment");

                    // text-level: 用户提示
                    multiple_comment.Text =
                        // string.Format(this.GetString("登录中发现有多个账户符合条件"),   // "登录中发现有 {0} 个账户符合条件。请在下面输入0-{1}的序号后，重新登录"
                        string.Format(this.GetString("登录中发现有多个账户符合条件，请在下面输入出生日期后重新登录"),   // "登录中发现有 {0} 个账户符合条件。请在下面输入0-{1}的序号后，重新登录"
                        lRet.ToString(),
                        (lRet - 1).ToString());
                    // "登录中发现有 " + nRet.ToString() + " 个账户符合条件。请在下面输入0-" + (nRet-1).ToString() + "的序号后，重新登录";
                    return -1;
                }
                else
                {
                    this.InMultpleSelection = false;
                }

            }

            if (lRet != 1)
                goto ERROR1;

            return 0;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
            return -1;
        }

        // 把馆代码列表字符串变换为适合用在 Login() API 的 strParameters 参数中的形态
        public static string GetLibraryCodeParam(string strLibraryCode)
        {
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                return "";

            return strLibraryCode.Replace(",", "|");
        }
    }

    // 栏目
    public enum LoginColumn
    {
        None = -1,
        ID = 0,
        Barcode = 1,
        NameBirthdate = 2,
        Email = 3,
        Telephone = 4,
        IdCardNumber = 5,   // 2009/9/22 身份证号
        CardNumber = 6,   // 2012/11/12 证号

        Patron = 7, // 读者登录方式
        Librarian = 8,  // 馆员登录方式
        Guest = 9,  // 访客登录
    }

    public delegate void LoginEventHandler(object sender,
    LoginEventArgs e);

    public class LoginEventArgs : EventArgs
    {
    }

    [Flags]
    public enum LoginStyle
    {
        None = 0x00,
        Reader = 0x01,
        Librarian = 0x02,
    }
}
