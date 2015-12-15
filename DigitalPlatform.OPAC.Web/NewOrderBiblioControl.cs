using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Diagnostics;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:NewOrderBiblioControl runat=server></{0}:NewOrderBiblioControl>")]
    public class NewOrderBiblioControl : WebControl, INamingContainer
    {
        public bool Wrapper = false;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.NewOrderBiblioControl.cs",
                typeof(NewOrderBiblioControl).Module.Assembly);

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


        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        void CreateInfoLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='infoline'><td colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "infotext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void SetInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("infoline");
            LiteralControl text = (LiteralControl)line.FindControl("infotext");
            text.Text = strText;
        }

        void CreateInputLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<tr class='inputline'><td colspan='2'>"));

            PlaceHolder edit_holder = new PlaceHolder();
            edit_holder.ID = "edit_holder";
            line.Controls.Add(edit_holder);

            edit_holder.Controls.Add(new LiteralControl("<table class='edit'>"));


            // 存储库
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("存储库");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            // DropDown
            DropDownList store_dbname = new DropDownList();
            store_dbname.ID = "store_dbname";
            // store_dbname.Width = new Unit("100%");
            store_dbname.CssClass = "store_dbname";
            edit_holder.Controls.Add(store_dbname);

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            List<string> dbnames = app.GetOrderRecommendStoreDbNames();
            store_dbname.Items.Clear();
            if (dbnames.Count > 0)
            {
                for (int i = 0; i < dbnames.Count; i++)
                {
                    store_dbname.Items.Add(dbnames[i]);
                }
            }
            else
            {
                // 还没有定义 读者新书目 存储库

                LiteralControl comment = new LiteralControl();
                comment.ID = "comment";
                comment.Text = "<span class='comment'>还没有定义任何角色名为 'orderRecommendStore' 的读者创建新书目存储库...</span>";
                edit_holder.Controls.Add(comment);
            }

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 题名
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("题名");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_title = new TextBox();
            edit_biblio_title.Text = "";
            edit_biblio_title.ID = "edit_biblio_title";
            edit_biblio_title.CssClass = "biblio_title";
            edit_holder.Controls.Add(edit_biblio_title);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 责任者
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("责任者");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_author = new TextBox();
            edit_biblio_author.Text = "";
            edit_biblio_author.ID = "edit_biblio_author";
            edit_biblio_author.CssClass = "biblio_author";
            edit_holder.Controls.Add(edit_biblio_author);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 出版者
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("出版者");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_publisher = new TextBox();
            edit_biblio_publisher.Text = "";
            edit_biblio_publisher.ID = "edit_biblio_publisher";
            edit_biblio_publisher.CssClass = "biblio_publisher";
            edit_holder.Controls.Add(edit_biblio_publisher);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // ISBN/ISSN
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("ISBN/ISSN");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_isbn = new TextBox();
            edit_biblio_isbn.Text = "";
            edit_biblio_isbn.ID = "edit_biblio_isbn";
            edit_biblio_isbn.CssClass = "biblio_isbn";
            edit_holder.Controls.Add(edit_biblio_isbn);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 价格
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("价格");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_price = new TextBox();
            edit_biblio_price.Text = "";
            edit_biblio_price.ID = "edit_biblio_price";
            edit_biblio_price.CssClass = "biblio_price";
            edit_holder.Controls.Add(edit_biblio_price);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 摘要
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            literal = new LiteralControl();
            literal.Text = this.GetString("摘要");    //
            edit_holder.Controls.Add(literal);

            edit_holder.Controls.Add(new LiteralControl("</td><td>"));

            TextBox edit_biblio_summary = new TextBox();
            edit_biblio_summary.Text = "";
            edit_biblio_summary.ID = "edit_biblio_summary";
            edit_biblio_summary.CssClass = "biblio_summary";
            edit_biblio_summary.TextMode = TextBoxMode.MultiLine;
            edit_holder.Controls.Add(edit_biblio_summary);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // 提示文字
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='description' colspan='2'>"));

            LiteralControl description = new LiteralControl();
            description.ID = "edit_description";
            description.Text = this.GetString("还可详细阐述您的推荐意见如下(可选)") + ":";
            edit_holder.Controls.Add(description);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            // CommentControl
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='comment' colspan='2'>"));

            CommentControl commentcontrol = new CommentControl();
            commentcontrol.ID = "commentcontrol";
            edit_holder.Controls.Add(commentcontrol);
            commentcontrol.EditAction = "new";
            commentcontrol.ButtonSubmit.Visible = false;
            commentcontrol.ButtonCancel.Visible = false;
            commentcontrol.EditType = "订购征询";
            commentcontrol.OrderSuggestionHolder.Visible = false;
            commentcontrol.EditDescription.Visible = false;

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 提交
            edit_holder.Controls.Add(new LiteralControl("<tr><td colspan='2'>"));

            Button submit_button = new Button();
            submit_button.ID = "submit_button";
            submit_button.Text = this.GetString("提交");
            submit_button.Click += new EventHandler(submit_button_Click);
            edit_holder.Controls.Add(submit_button);

            edit_holder.Controls.Add(new LiteralControl("</td></tr></table>"));

            line.Controls.Add(new LiteralControl("</td></tr>"));
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new LiteralControl("<tr class='debugline'><td colspan='2'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            // 隐藏字段，用来转值
            HiddenField hidden = new HiddenField();
            hidden.ID = "resultcount";
            line.Controls.Add(hidden);

            HiddenField type = new HiddenField();
            type.ID = "edit_type";
            type.Value = "";
            line.Controls.Add(type);

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


        int BuildBiblioRecord(
            string strMarcSyntax,
            string strTitle,
            string strAuthor,
            string strPublisher,
            string strIsbnIssn,
            string strPrice,
            string strSummary,
            string strOperator,
            out string strMARC,
            out string strError)
        {
            strError = "";

            // strMARC = "012345678901234567890123";
            strMARC = "00892nam0 2200277   45  ";

            string strField = "";
            int nRet = 0;

            if (strMarcSyntax.ToLower() == "unimarc")
            {
                // 010
                strField = "010  "
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strIsbnIssn
                    + new string(MarcDocument.SUBFLD, 1) + "d"
                    + strPrice;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"010",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 010 error";
                    return -1;
                }

                // 200
                strField = "20010"
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strTitle
                    + new string(MarcDocument.SUBFLD, 1) + "f"
                    + strAuthor
                    ;

                nRet = MarcDocument.ReplaceField(
                ref strMARC,
                "200",
                0,
                strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 200 error";
                    return -1;
                }

                // 210
                strField = "210  "
                    + new string(MarcDocument.SUBFLD, 1) + "c"
                    + strPublisher;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"210",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 210 error";
                    return -1;
                }

                // 330
                strField = "330  "
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strSummary;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"330",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 330 error";
                    return -1;
                }


                // 701
                strField = "701  "
        + new string(MarcDocument.SUBFLD, 1) + "a"
        + strAuthor
        ;

                nRet = MarcDocument.ReplaceField(
                ref strMARC,
                "701",
                0,
                strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 701 error";
                    return -1;
                }
            }
            else if (strMarcSyntax.ToLower() == "usmarc")
            {
                // 020
                strField = "020  "
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strIsbnIssn
                    + new string(MarcDocument.SUBFLD, 1) + "c"
                    + strPrice;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"020",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 020 error";
                    return -1;
                }

                // 245
                strField = "24510"
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strTitle
                    + new string(MarcDocument.SUBFLD, 1) + "c"
                    + strAuthor
                    ;

                nRet = MarcDocument.ReplaceField(
                ref strMARC,
                "245",
                0,
                strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 245 error";
                    return -1;
                }

                // 260
                strField = "260  "
                    + new string(MarcDocument.SUBFLD, 1) + "b"
                    + strPublisher;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"260",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 260 error";
                    return -1;
                }

                // 520
                strField = "520  "
                    + new string(MarcDocument.SUBFLD, 1) + "a"
                    + strSummary;
                nRet = MarcDocument.ReplaceField(
ref strMARC,
"520",
0,
strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 520 error";
                    return -1;
                }


                // 701
                strField = "700  "
        + new string(MarcDocument.SUBFLD, 1) + "a"
        + strAuthor
        ;

                nRet = MarcDocument.ReplaceField(
                ref strMARC,
                "700",
                0,
                strField);
                if (nRet == -1)
                {
                    strError = "ReplaceField 700 error";
                    return -1;
                }
            }

            // 998
            strField = "998  "
                + new string(MarcDocument.SUBFLD, 1) + "s"
                + "订购征询,读者创建"
                + new string(MarcDocument.SUBFLD, 1) + "u"
                + DateTime.Now.ToString("u")
                + new string(MarcDocument.SUBFLD, 1) + "z"
                + strOperator
                ;

            nRet = MarcDocument.ReplaceField(
            ref strMARC,
            "998",
            0,
            strField);
            if (nRet == -1)
            {
                strError = "ReplaceField 998 error";
                return -1;
            }

            return 0;
        }

        void submit_button_Click(object sender, EventArgs e)
        {
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strCreator = sessioninfo.UserID;

            // 先创建一条书目记录
            TextBox biblio_title = (TextBox)this.FindControl("edit_biblio_title");
            TextBox biblio_author = (TextBox)this.FindControl("edit_biblio_author");
            TextBox biblio_publisher = (TextBox)this.FindControl("edit_biblio_publisher");
            TextBox biblio_isbn = (TextBox)this.FindControl("edit_biblio_isbn");
            TextBox biblio_price = (TextBox)this.FindControl("edit_biblio_price");
            TextBox biblio_summary = (TextBox)this.FindControl("edit_biblio_summary");

            // 检查必备字段
            if (String.IsNullOrEmpty(biblio_title.Text) == true)
            {
                strError = "尚未输入书名/刊名";
                goto ERROR1;
            }

            DropDownList store_dbname = (DropDownList)this.FindControl("store_dbname");

            string strBiblioDbName = store_dbname.SelectedValue;

            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "尚未选定目标库";
                goto ERROR1;
            }

            string strBiblioRecPath = "";
            string strMarcSyntax = "";
            string strMARC = "";

            // 得到目标书目库的MARC格式
            ItemDbCfg cfg = app.GetBiblioDbCfg(strBiblioDbName);
            if (cfg == null)
            {
                strError = "目标库 '" + strBiblioDbName + "' 不是系统定义的的书目库";
                goto ERROR1;
            }

            strMarcSyntax = cfg.BiblioDbSyntax;

            int nRet = BuildBiblioRecord(
                strMarcSyntax,
                biblio_title.Text,
                biblio_author.Text,
                biblio_publisher.Text,
                biblio_isbn.Text,
                biblio_price.Text,
                biblio_summary.Text,
                strCreator,
                out strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 保存
            // 将MARC格式转换为XML格式
            string strXml = "";
            nRet = MarcUtil.Marc2Xml(
strMARC,
strMarcSyntax,
out strXml,
out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            // 临时的SessionInfo对象
            SessionInfo temp_sessioninfo = new SessionInfo(app);

            // 模拟一个账户
            Account account = new Account();
            account.LoginName = "neworderbiblio";
            account.Password = "";
            account.Rights = "setbiblioinfo";

            account.Type = "";
            account.Barcode = "";
            account.Name = "neworderbiblio";
            account.UserID = "neworderbiblio";
            account.RmsUserName = app.ManagerUserName;
            account.RmsPassword = app.ManagerPassword;

            temp_sessioninfo.Account = account;
             * */
            SessionInfo temp_sessioninfo = new SessionInfo(app);
            temp_sessioninfo.UserID = app.ManagerUserName;
            temp_sessioninfo.Password = app.ManagerPassword;
            temp_sessioninfo.IsReader = false;

            string strOutputBiblioRecPath = "";

            try
            {

                strBiblioRecPath = strBiblioDbName + "/?";

                byte[] baOutputTimestamp = null;

                long lRet = temp_sessioninfo.Channel.SetBiblioInfo(
                    null,
                    "new",
                    strBiblioRecPath,
            "xml",
            strXml,
            null,
            "",
            out strOutputBiblioRecPath,
            out baOutputTimestamp,
            out strError);
                if (lRet == -1)
                {
                    strError = "创建书目记录发生错误: " + strError;
                    goto ERROR1;
                }
                /*
                LibraryServerResult result = app.SetBiblioInfo(
                    temp_sessioninfo,
                    "new",
                    strBiblioRecPath,
            "xml",
            strXml,
            null,
            out strOutputBiblioRecPath,
            out baOutputTimestamp);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
                 * */
            }
            finally
            {
                temp_sessioninfo.CloseSession();
                temp_sessioninfo = null;
            }

            // 清除每个输入域的内容
            biblio_title.Text = "";
            biblio_author.Text = "";
            biblio_publisher.Text = "";
            biblio_isbn.Text = "";
            biblio_summary.Text = "";
            biblio_price.Text = "";


            strBiblioRecPath = strOutputBiblioRecPath;

            CommentControl commentcontrol = (CommentControl)this.FindControl("commentcontrol");
            // 创建评注记录
            if (String.IsNullOrEmpty(commentcontrol.EditTitle) == false
    || String.IsNullOrEmpty(commentcontrol.EditContent) == false)
            {
                string strWarning = "";
                commentcontrol.BiblioRecPath = strBiblioRecPath;

                nRet = commentcontrol.DoSubmit(
           out strWarning,
           out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (String.IsNullOrEmpty(strWarning) == false)
                    this.SetDebugInfo("warninginfo", strWarning);
            }

            string strUrl = "./book.aspx?bibliorecpath="
                + HttpUtility.UrlEncode(strBiblioRecPath);
            string strText = "新的荐购书目记录创建成功。点击此处可查看：<a href='"
                + strUrl
                + "' target='_blank'>"
                + strUrl
                + "</a>";
            SetInfo(strText);
            this.SetDebugInfo("succeedinfo", strText);
            return;
        ERROR1:
            SetDebugInfo("errorinfo", strError);
        }

#if NO
        // 创建一个评注记录
        public int CreateCommentInfo(
            string strBiblioRecPath,
            string strCommentXml,
            out string strNewCommentRecPath,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            strNewCommentRecPath = "";
            strNewXml = "";
            baNewTimestamp = null;

            LoginState loginstate = Global.GetLoginState(this.Page);
            if (loginstate == LoginState.NotLogin)
            {
                strError = "尚未登录, 不能创建评注";
                return -1;
            }
            if (loginstate == LoginState.Public)
            {
                strError = "访客身份, 不能创建评注";
                return -1;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            Debug.Assert(String.IsNullOrEmpty(sessioninfo.UserID) == false, "");


            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = ResPath.GetRecordId(strBiblioRecPath);

            XmlDocument comment_dom = new XmlDocument();
            try
            {
                comment_dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(comment_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "new";
            info.NewRecPath = "";
            info.NewRecord = comment_dom.OuterXml;
            info.NewTimestamp = null;

            // 
            EntityInfo[] comments = new EntityInfo[1];
            comments[0] = info;

            EntityInfo[] errorinfos = null;

            long lRet = sessioninfo.Channel.SetComments(null,
                strBiblioRecPath,
                comments,
                out errorinfos,
                out strError);
            if (lRet == -1)
            {
                strError = "创建评注记录时发生错误: " + strError;
                return -1;
            }
            /*
            LibraryServerResult result = app.CommentItemDatabase.SetItems(sessioninfo,
                strBiblioRecPath,
                comments,
                out errorinfos);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                return -1;
            }
             * */

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        strNewCommentRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }
#endif

        // 布局控件
        protected override void CreateChildControls()
        {
            // 表格开头
            LiteralControl literal = new LiteralControl();
            literal.ID = "begin";
            if (this.Wrapper == true)
                literal.Text = this.GetPrefixString(
                    this.GetString("创建书目记录"),
                    "content_wrapper");
            literal.Text += "<table class='newrecommend'>";    //  width='100%' cellspacing='1' cellpadding='4'
            this.Controls.Add(literal);


            // 信息行
            PlaceHolder infoline = new PlaceHolder();
            infoline.ID = "infoline";
            this.Controls.Add(infoline);

            CreateInfoLine(infoline);

            // 编辑行
            PlaceHolder inputline = new PlaceHolder();
            inputline.ID = "inputline";
            this.Controls.Add(inputline);

            CreateInputLine(inputline);

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);

            // 表格结尾
            literal = new LiteralControl();
            literal.ID = "end";
            literal.Text = "</table>";
            if (this.Wrapper == true)
                literal.Text += this.GetPostfixString();

            this.Controls.Add(literal);
        }

        protected override void Render(HtmlTextWriter output)
        {
            //int nRet = 0;
            //string strError = "";

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);
            if (loginstate == LoginState.NotLogin)
            {
                SetInfo("尚未登录, 不能创建荐购书目记录。<a href='login.aspx?redirect=newrecommend.aspx'>登录</a>");
            }
            if (loginstate == LoginState.Public)
            {
                SetInfo("访客身份, 不能创建荐购书目记录。<a href='login.aspx?redirect=newrecommend.aspx'>登录</a>");
            }

            PlaceHolder edit_holder = (PlaceHolder)this.FindControl("edit_holder");
            if (loginstate == LoginState.Public || loginstate == LoginState.NotLogin)
            {
                edit_holder.Visible = false;
                base.Render(output);
                return;
            }
            else
            {
                edit_holder.Visible = true;
            }
#if NO

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (sessioninfo.Account != null)
            {
                TextBox edit_creator = (TextBox)this.FindControl("edit_creator");
                edit_creator.Text = sessioninfo.Account.UserID;
            }
#endif

            base.Render(output);
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
    }
}
