using System;
using System.Collections;
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

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient;
using System.Web.UI.HtmlControls;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 显示书目信息的控件
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BiblioControl runat=server></{0}:BiblioControl>")]
    public class BiblioControl : WebControl, INamingContainer
    {
        public bool AutoSetPageTitle = false;    // 是否自动根据书名创建 HTML 页面的 <title> 元素
        public bool DisableAjax = false;    // 是否临时取消 Ajax? 这个参数如果 == true，可以覆盖 webui.xml中 <biblioControl>元素的ajsx属性
        // public bool Active = true;
        public event WantFocusEventHandler WantFocus = null;

        public string RecPath = "";

        public bool Wrapper = false;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BiblioControl.cs",
                typeof(BiblioControl).Module.Assembly);

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

        protected override void CreateChildControls()
        {
            this.Controls.Add(new AutoIndentLiteral("<%begin%><div class='"));

            LiteralControl outer_class = new LiteralControl();
            outer_class.ID = "outer_class";
            this.Controls.Add(outer_class);

            this.Controls.Add(new AutoIndentLiteral("'>"));

            // 隐藏字段
            HiddenField editaction = new HiddenField();
            editaction.ID = "editaction";
            this.Controls.Add(editaction);

            HiddenField active = new HiddenField();
            active.ID = "active";
            active.Value = "1";
            this.Controls.Add(active);

            // 包裹开始
            if (this.Wrapper == true)
                this.Controls.Add(new LiteralControl(this.GetPrefixString("书目信息", "content_wrapper")
                    + "<div class='biblio_wrapper'>"));

            // 种
            LiteralControl literal = new LiteralControl();
            literal.ID = "biblio";
            literal.Text = "";
            this.Controls.Add(literal);

            // 编辑行
            PlaceHolder inputline = new PlaceHolder();
            inputline.ID = "inputline";
            this.Controls.Add(inputline);

            CreateEditArea(inputline);

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "cmdline";
            this.Controls.Add(cmdline);

            CreateCmdLine(cmdline);

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            debugline.Visible = false;
            this.Controls.Add(debugline);

            CreateDebugLine(debugline);


            // 包裹结束
            if (this.Wrapper == true)
                this.Controls.Add(new LiteralControl(
                    "</div>" + this.GetPostfixString()
                    ));

            this.Controls.Add(new AutoIndentLiteral("<%end%></div>"));
        }

        void CreateCmdLine(PlaceHolder line)
        {
            line.Controls.Clear();

            line.Controls.Add(new LiteralControl("<div class='cmdline' onmouseover='HilightCommentCmdline(this); return false;'>"));

            /*
            // change
            Button change_button = new Button();
            change_button.Text = this.GetString("编辑");
            change_button.ID = "changebutton";
            change_button.CssClass = "edit";
            change_button.Click += new EventHandler(change_button_Click);
            line.Controls.Add(change_button);
             * */

            // state
            Button state_button = new Button();
            state_button.Text = this.GetString("状态");
            state_button.ID = "statebutton";
            state_button.CssClass = "state";
            state_button.Click += new EventHandler(state_button_Click);
            line.Controls.Add(state_button);

            /*
            // delete
            Button delete_button = new Button();
            delete_button.Text = this.GetString("删除");
            delete_button.ID = "deletebutton";
            delete_button.CssClass = "delete";
            delete_button.Click += new EventHandler(delete_button_Click);
            line.Controls.Add(delete_button);

            string strConfirmText = this.GetString("确实要删除这条书目记录?");
            delete_button.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");
            */


            line.Controls.Add(new LiteralControl("</div>"));
        }

        void state_button_Click(object sender, EventArgs e)
        {
            this.EditAction = "changestate";

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                this.WantFocus(this, e1);
            }
        }

        public string EditAction
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("editaction");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("editaction");
                s.Value = value;
            }
        }

        void CreateEditArea(PlaceHolder line)
        {
            line.Controls.Clear();

            PlaceHolder edit_holder = new PlaceHolder();
            edit_holder.ID = "edit_holder";
            line.Controls.Add(edit_holder);

            HiddenField timestamp = new HiddenField();
            timestamp.ID = "edit_timestamp";
            timestamp.Value = "";
            edit_holder.Controls.Add(timestamp);

            HiddenField bibliorecpath = new HiddenField();
            bibliorecpath.ID = "edit_bibliorecpath";
            bibliorecpath.Value = "";
            edit_holder.Controls.Add(bibliorecpath);

            edit_holder.Controls.Add(new LiteralControl("<table class='edit_biblio'>"));


            // 注释行。
            {
                PlaceHolder commentline = new PlaceHolder();
                commentline.ID = "commentline";
                commentline.Visible = false;
                edit_holder.Controls.Add(commentline);

                commentline.Controls.Add(new LiteralControl("<tr><td class='comment' colspan='2'>"));

                LiteralControl comment = new LiteralControl();
                comment.ID = "comment";
                commentline.Controls.Add(comment);

                commentline.Controls.Add(new LiteralControl("</td></tr>"));
            }



            // 状态
            PlaceHolder state_holder = new PlaceHolder();
            state_holder.ID = "state_holder";
            state_holder.Visible = true;
            edit_holder.Controls.Add(state_holder);

            state_holder.Controls.Add(new LiteralControl("<tr><td class='left'>"));

            LiteralControl literal = new LiteralControl();
            literal.Text = this.GetString("书目状态");    //
            state_holder.Controls.Add(literal);

            state_holder.Controls.Add(new LiteralControl("</td><td>"));

            // 屏蔽
            CheckBox screened = new CheckBox();
            screened.ID = "edit_screened";
            screened.Text = this.GetString("屏蔽");
            screened.CssClass = "screened";
            state_holder.Controls.Add(screened);

            // 审查
            CheckBox edit_censor = new CheckBox();
            edit_censor.ID = "edit_censor";
            edit_censor.Text = this.GetString("审查");
            edit_censor.CssClass = "censor";
            state_holder.Controls.Add(edit_censor);

            // 锁定
            CheckBox locked = new CheckBox();
            locked.ID = "edit_locked";
            locked.Text = this.GetString("锁定");
            locked.CssClass = "locked";
            state_holder.Controls.Add(locked);

            // 精品
            CheckBox valuable = new CheckBox();
            valuable.ID = "edit_valuable";
            valuable.Text = this.GetString("精品");
            valuable.CssClass = "valuable";
            state_holder.Controls.Add(valuable);

            state_holder.Controls.Add(new LiteralControl("</td></tr>"));


            // 提交
            edit_holder.Controls.Add(new LiteralControl("<tr><td class='submit' colspan='2'>"));

            Button submit_button = new Button();
            submit_button.ID = "submit_button";
            submit_button.CssClass = "submit";
            submit_button.Text = this.GetString("提交修改");
            submit_button.Click += new EventHandler(submit_button_Click);
            edit_holder.Controls.Add(submit_button);

            Button cancel_button = new Button();
            cancel_button.ID = "cancel_button";
            cancel_button.CssClass = "cancel";
            cancel_button.Text = this.GetString("取消");
            cancel_button.Click += new EventHandler(cancel_button_Click);
            string strConfirmText = this.GetString("确实要取消对书目状态的修改?");
            cancel_button.Attributes.Add("onclick", "return myConfirm('" + strConfirmText + "');");
            edit_holder.Controls.Add(cancel_button);

            LiteralControl edit_errorinfo = new LiteralControl();
            edit_errorinfo.ID = "edit_errorinfo";
            edit_errorinfo.Visible = false;
            edit_holder.Controls.Add(edit_errorinfo);

            edit_holder.Controls.Add(new LiteralControl("</td></tr>"));

            edit_holder.Controls.Add(new LiteralControl("</table>"));
        }

        string BiblioRecPath
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_bibliorecpath");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_bibliorecpath");
                s.Value = value;
            }
        }

        string Timestamp
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_timestamp");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("edit_timestamp");
                s.Value = value;
            }
        }

        void submit_button_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            long lRet = 0;
            string strError = "";
            // string strOutputPath = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            if (bManager == false)
            {
                strError = "当前帐户不具备 managecomment 权限， 不能进行修改书目状态的操作";
                goto ERROR1;
            }


            string strBiblioRecPath = this.BiblioRecPath;

            byte[] timestamp = null;
            string strBiblioXml = "";
            /*
            string strStyle = LibraryChannel.GETRES_ALL_STYLE;
            // 先获得XML记录体，然后和时间戳进行比较
            string strMetaData = "";
            lRet = sessioninfo.Channel.GetRes(
                null,
                strBiblioRecPath,
                strStyle,
                out strBiblioXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                goto ERROR1;
            }
             * */
            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            lRet = sessioninfo.Channel.GetBiblioInfos(
                null,
                strBiblioRecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == -1)
            {
                strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                goto ERROR1;
            }
            if (results == null || results.Length < 1)
            {
                strError = "results error ";
                goto ERROR1;
            }
            strBiblioXml = results[0];

            byte[] old_timestamp = ByteArray.GetTimeStampByteArray(this.Timestamp);
            if (ByteArray.Compare(timestamp, old_timestamp) != 0)
            {
                strError = "修改被拒绝。因为记录 '" + strBiblioRecPath + "' 在保存前已经被其他人修改过。请重新装载";
                goto ERROR1;
            }


            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                "", // this.CurMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBiblioState = MarcDocument.GetFirstSubfield(strMarc,
                    "998",
                    "s");   // 状态

            // 修改998字段
            string strOldBiblioState = strBiblioState;

            this.GetStateValueFromControls(ref strBiblioState);

            if (strOldBiblioState == strBiblioState)
            {
                // 也退出编辑态
                cancel_button_Click(this, new EventArgs());
                strError = "状态没有发生变化，放弃保存书目记录";
                goto ERROR1;
            }

            MarcUtil.SetFirstSubfield(ref strMarc,
                "998",
                "s",
                strBiblioState);

            // 保存
            // 将MARC格式转换为XML格式
            string strXml = "";
            nRet = MarcUtil.Marc2Xml(
strMarc,
strOutMarcSyntax,
out strXml,
out strError);
            if (nRet == -1)
                goto ERROR1;

            string strOutputBiblioRecPath = "";
            byte[] baOutputTimestamp = null;

            lRet = sessioninfo.Channel.SetBiblioInfo(
                null,
                "change",
                strBiblioRecPath,
        "xml",
        strXml,
        timestamp,
        "",
        out strOutputBiblioRecPath,
        out baOutputTimestamp,
        out strError);
            if (lRet == -1)
                goto ERROR1;

            /*
            LibraryServerResult result = app.SetBiblioInfo(
                sessioninfo,
                "change",
                strBiblioRecPath,
        "xml",
        strXml,
        timestamp,
        out strOutputBiblioRecPath,
        out baOutputTimestamp);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }
             * */
            this.Timestamp = ByteArray.GetHexTimeStampString(baOutputTimestamp);

            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        public void Clear()
        {
            this.ClearEdit();
        }

        void ClearEdit()
        {
            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_censor");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_locked");
            checkbox.Checked = false;

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            checkbox.Checked = false;
        }

        void cancel_button_Click(object sender, EventArgs e)
        {
            this.EditAction = "";
            this.ClearEdit();

            if (this.WantFocus != null)
            {
                WantFocusEventArgs e1 = new WantFocusEventArgs();
                e1.Focus = false;   // 不再独占Focus
                this.WantFocus(this, e1);
            }
        }

        void CreateDebugLine(PlaceHolder line)
        {
            line.Controls.Add(new AutoIndentLiteral("<%begin%><div class='debugline'>"));

            LiteralControl literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            line.Controls.Add(literal);

            line.Controls.Add(new AutoIndentLiteral("<%end%></div>"));
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


        void SetStateValueToControls(string strState)
        {
            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            checkbox.Checked = (StringUtil.IsInList("屏蔽", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_censor");
            checkbox.Checked = (StringUtil.IsInList("审查", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_locked");
            checkbox.Checked = (StringUtil.IsInList("锁定", strState) == true);

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            checkbox.Checked = (StringUtil.IsInList("精品", strState) == true);
        }

        void GetStateValueFromControls(ref string strState)
        {
            CheckBox checkbox = (CheckBox)this.FindControl("edit_screened");
            StringUtil.SetInList(ref strState, "屏蔽", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_censor");
            StringUtil.SetInList(ref strState, "审查", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_locked");
            StringUtil.SetInList(ref strState, "锁定", checkbox.Checked);

            checkbox = (CheckBox)this.FindControl("edit_valuable");
            StringUtil.SetInList(ref strState, "精品", checkbox.Checked);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string strError = "";
            int nRet = PrepareBiblioRecord(out strError);
            if (nRet == -1)
            {
                // throw new Exception(strError);
                this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
                this.Page.Response.End();
            }
        }

        string m_strXml = "";
        string m_strMARC = "";
        string m_strOpacBiblio = "";

        int PrepareBiblioRecord(
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;
            // string strOutputPath = "";

            if (string.IsNullOrEmpty(this.RecPath) == true)
                return 0;   // 此时无法进行初始化

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strBiblioXml = "";
            string strBiblioState = "";

            byte[] timestamp = null;
            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            lRet = sessioninfo.Channel.GetBiblioInfos(
                null,
                this.RecPath,
                "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == -1)
            {
                strError = "获得种记录 '" + this.RecPath + "' 时出错: " + strError;
                goto ERROR1;
            }
            if (results == null || results.Length < 1)
            {
                strError = "results error ";
                goto ERROR1;
            }

            if (app.SearchLog != null)
            {
                SearchLogItem log = new SearchLogItem();
                log.IP = this.Page.Request.UserHostAddress.ToString();
                log.Query = "";
                log.Time = DateTime.UtcNow;
                log.HitCount = 1;
                log.Format = "biblio";
                log.RecPath = this.RecPath;
                app.SearchLog.AddLogItem(log);
            }

            strBiblioXml = results[0];
            this.m_strXml = strBiblioXml;

            this.Timestamp = ByteArray.GetHexTimeStampString(timestamp);
            this.BiblioRecPath = this.RecPath;

            string strMarc = "";

            // 转换为MARC
            {
                string strOutMarcSyntax = "";

                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.m_strMARC = strMarc;
            }

            bool bAjax = true;
            if (this.DisableAjax == true)
                bAjax = false;
            else
            {
                if (app != null
                    && app.WebUiDom != null
                    && app.WebUiDom.DocumentElement != null)
                {
                    XmlNode nodeBiblioControl = app.WebUiDom.DocumentElement.SelectSingleNode(
            "biblioControl");
                    if (nodeBiblioControl != null)
                    {
                        DomUtil.GetBooleanParam(nodeBiblioControl,
                            "ajax",
                            true,
                            out bAjax,
                            out strError);
                    }
                }
            }

            if (bAjax == false)
            {
                string strBiblio = "";
                string strBiblioDbName = ResPath.GetDbName(this.RecPath);

                // 需要从内核映射过来文件
                string strLocalPath = "";
                nRet = app.MapKernelScriptFile(
                    null,   // sessioninfo,
                    strBiblioDbName,
                    "./cfgs/opac_biblio.fltx",  // OPAC查询固定认这个角色的配置文件，作为公共查询书目格式创建的脚本。而流通前端，创建书目格式的时候，找的是loan_biblio.fltx配置文件
                    out strLocalPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;


                // 将种记录数据从XML格式转换为HTML格式
                KeyValueCollection result_params = null;

                // 2006/11/28 changed
                string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                nRet = app.ConvertBiblioXmlToHtml(
                        strFilterFileName,
                        strBiblioXml,
                        this.RecPath,
                        out strBiblio,
                        out result_params,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;

                // TODO: Render的时候设置，已经晚了半拍
                // 要想办法在全部Render前得到题名和进行设置
                if (this.AutoSetPageTitle == true
                    && result_params != null && result_params.Count > 0)
                {
                    string strTitle = result_params["title"].Value;
                    if (string.IsNullOrEmpty(strTitle) == false)
                        this.Page.Title = strTitle;

                    bool bHasDC = false;
                    // 探测一下，是否有至少一个DC.开头的 key ?
                    foreach (KeyValue item in result_params)
                    {
                        if (StringUtil.HasHead(item.Key, "DC.") == true)
                        {
                            bHasDC = true;
                            break;
                        }
                    }

                    if (bHasDC == true)
                    {
                        // <header profile="http://dublincore.org/documents/2008/08/04/dc-html/">
                        this.Page.Header.Attributes.Add("profile", "http://dublincore.org/documents/2008/08/04/dc-html/");

                        // DC rel
                        // 
                        HtmlLink link = new HtmlLink();
                        link.Href = "http://purl.org/dc/elements/1.1/";
                        link.Attributes.Add("rel", "schema.DC");
                        this.Page.Header.Controls.Add(link);

                        // <link rel="schema.DCTERMS" href="http://purl.org/dc/terms/" >
                        link = new HtmlLink();
                        link.Href = "http://purl.org/dc/terms/";
                        link.Attributes.Add("rel", "schema.DCTERMS");
                        this.Page.Header.Controls.Add(link);

                        foreach (KeyValue item in result_params)
                        {
                            if (StringUtil.HasHead(item.Key, "DC.") == false
                                && StringUtil.HasHead(item.Key, "DCTERMS.") == false)
                                continue;
                            HtmlMeta meta = new HtmlMeta();
                            meta.Name = item.Key;
                            meta.Content = item.Value;
                            if (StringUtil.HasHead(item.Value, "urn:") == true
                                || StringUtil.HasHead(item.Value, "http:") == true
                                || StringUtil.HasHead(item.Value, "info:") == true
                                )
                                meta.Attributes.Add("scheme", "DCTERMS.URI");

                            this.Page.Header.Controls.Add(meta);
                        }
                    }
                }

                /*
                string strPrefix = "";
                if (this.Wrapper == true)
                    strPrefix = this.GetPrefixString("书目信息", "content_wrapper")
                        + "<div class='biblio_wrapper'>";

                string strPostfix = "";
                if (this.Wrapper == true)
                    strPostfix = "</div>" + this.GetPostfixString();
                 * */


                /*
                LiteralControl literal = (LiteralControl)FindControl("biblio");
                literal.Text = strPrefix + strBiblio + strPostfix;
                 * */

                // strBiblio = strPrefix + strBiblio + strPostfix;

                this.m_strOpacBiblio = strBiblio;
            }


            return 0;
        ERROR1:
            return -1;
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (this.RecPath == "")
            {
                output.Write("尚未指定记录路径");
                return;
            }

            int nRet = 0;
            long lRet = 0;
            string strError = "";
            // string strOutputPath = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            bool bManager = false;
            if (string.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                bManager = false;
            else
                bManager = true;

            PlaceHolder inputline = (PlaceHolder)this.FindControl("inputline");
            PlaceHolder cmdline = (PlaceHolder)this.FindControl("cmdline");


            if (bManager == false)
                cmdline.Visible = false;

            string strBiblioState = "";

#if NO
            string strBiblioXml = "";

            // 获得书目XML
            {
                byte[] timestamp = null;

                string[] formats = new string[1];
                formats[0] = "xml";

                string[] results = null;
                lRet = sessioninfo.Channel.GetBiblioInfos(
                    null,
                    this.RecPath,
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获得种记录 '" + this.RecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }
                if (results == null || results.Length < 1)
                {
                    strError = "results error ";
                    goto ERROR1;
                }
                strBiblioXml = results[0];

                this.Timestamp = ByteArray.GetHexTimeStampString(timestamp);
                this.BiblioRecPath = this.RecPath;
            }

            string strMarc = "";

            // 转换为MARC
            {
                string strOutMarcSyntax = "";

                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

#endif


            bool bAjax = true;
            if (this.DisableAjax == true)
                bAjax = false;
            else
            {
                if (app != null
                    && app.WebUiDom != null
                    && app.WebUiDom.DocumentElement != null)
                {
                    XmlNode nodeBiblioControl = app.WebUiDom.DocumentElement.SelectSingleNode(
            "biblioControl");
                    if (nodeBiblioControl != null)
                    {
                        DomUtil.GetBooleanParam(nodeBiblioControl,
                            "ajax",
                            true,
                            out bAjax,
                            out strError);
                    }
                }
            }

            string strMarc = "";
            if (string.IsNullOrEmpty(this.m_strMARC) == true)
            {
                nRet = PrepareBiblioRecord(out strError);
                if (nRet == -1)
                    goto ERROR1;
                strMarc = this.m_strMARC;
            }

            strBiblioState = MarcDocument.GetFirstSubfield(strMarc,
                "998",
                "s");   // 状态
            string strOriginCreator = MarcDocument.GetFirstSubfield(strMarc,
                    "998",
                    "z");

            bool bReaderCreate = false;
            if (StringUtil.IsInList("读者创建", strBiblioState) == true)
                bReaderCreate = true;

            // 不是读者创建的记录，就不让在这里修改状态
            if (bReaderCreate == false)
                cmdline.Visible = false;


            bool bDisplayOriginContent = false;
            if (StringUtil.IsInList("审查", strBiblioState) == false
                && StringUtil.IsInList("屏蔽", strBiblioState) == false)
                bDisplayOriginContent = true;

            // 管理员和作者本人都能看到被屏蔽的内容
            if (bManager == true || strOriginCreator == sessioninfo.UserID)
                bDisplayOriginContent = true;



            string strBiblio = "";
            if (bDisplayOriginContent == true)
            {
                if (bAjax == true)
                {
                    strBiblio = "<div class='pending'>biblio_html:" + HttpUtility.HtmlEncode(this.RecPath) + "</div>";
                }
                else
                {
                    strBiblio = this.m_strOpacBiblio;
#if NO
                    string strBiblioDbName = ResPath.GetDbName(this.RecPath);

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    nRet = app.MapKernelScriptFile(
                        null,   // sessioninfo,
                        strBiblioDbName,
                        "./cfgs/opac_biblio.fltx",  // OPAC查询固定认这个角色的配置文件，作为公共查询书目格式创建的脚本。而流通前端，创建书目格式的时候，找的是loan_biblio.fltx配置文件
                        out strLocalPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    // 将种记录数据从XML格式转换为HTML格式
                    Hashtable result_params = null;

                    // 2006/11/28 changed
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                    nRet = app.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strBiblioXml,
                            this.RecPath,
                            out strBiblio,
                            out result_params,
                            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // TODO: Render的时候设置，已经晚了半拍
                    // 要想办法在全部Render前得到题名和进行设置
                    if (this.AutoSetPageTitle == true
                        && result_params != null)
                    {
                        string strTitle = (string)result_params["title"];
                        if (string.IsNullOrEmpty(strTitle) == false)
                            this.Page.Title = strTitle;
                    }

                    /*
                    string strPrefix = "";
                    if (this.Wrapper == true)
                        strPrefix = this.GetPrefixString("书目信息", "content_wrapper")
                            + "<div class='biblio_wrapper'>";

                    string strPostfix = "";
                    if (this.Wrapper == true)
                        strPostfix = "</div>" + this.GetPostfixString();
                     * */


                    /*
                    LiteralControl literal = (LiteralControl)FindControl("biblio");
                    literal.Text = strPrefix + strBiblio + strPostfix;
                     * */

                    // strBiblio = strPrefix + strBiblio + strPostfix;
#endif

                }
            }


            // 屏蔽等状态显示
            string strResult = "";
            string strImage = "";
            if (StringUtil.IsInList("精品", strBiblioState) == true)
            {
                strImage = "<img src='" + MyWebPage.GetStylePath(app, "valuable.gif") + "'/>";
                strResult += "<div class='valuable' title='精品'>"
                    + strImage
+ this.GetString("精品")
+ "</div>";
            }
            if (StringUtil.IsInList("锁定", strBiblioState) == true)
            {
                strImage = "<img src='" + MyWebPage.GetStylePath(app, "locked.gif") + "'/>";
                strResult += "<div class='locked' title='锁定'>"
                    + strImage
+ this.GetString("锁定")
+ "</div>";
            }

            string strDisplayState = "";
            if (StringUtil.IsInList("审查", strBiblioState) == true)
            {
                strImage = "<img src='" + MyWebPage.GetStylePath(app, "censor.gif") + "'/>";
                strDisplayState = this.GetString("审查");
            }
            if (StringUtil.IsInList("屏蔽", strBiblioState) == true)
            {
                strImage = "<img src='" + MyWebPage.GetStylePath(app, "disable.gif") + "'/>";
                if (String.IsNullOrEmpty(strDisplayState) == false)
                    strDisplayState += ",";
                strDisplayState += this.GetString("屏蔽");
            }

            if (String.IsNullOrEmpty(strDisplayState) == false)
            {
                strResult += "<div class='forbidden' title='" + this.GetString("屏蔽原因") + "'>"
    + strImage
    + string.Format(this.GetString("本书目记录目前为X状态"), strDisplayState)
    + (strOriginCreator == sessioninfo.UserID ? "，" + this.GetString("其他人(非管理员)看不到下列内容") : "")
    + "</div>";
            }

            if (String.IsNullOrEmpty(strResult) == false)
            {
                strResult = "<div class='biblio_state'>" + strResult + "</div>";
            }

            if (this.EditAction == "changestate")
            {
                inputline.Visible = true;
                SetStateValueToControls(strBiblioState);
                cmdline.Visible = false;    // 在编辑态，命令条就不要出现了
            }
            else
            {
                inputline.Visible = false;
            }

            if (this.Active == false)
            {
                cmdline.Visible = false;
                inputline.Visible = false;  // 即便在编辑态也要加以压抑
            }


            string strClass = "biblio";
            if ((String.IsNullOrEmpty(strResult) == false
                || inputline.Visible == true)
                && this.Wrapper == false)
                strClass += " frame-border";

            LiteralControl outer_class = (LiteralControl)this.FindControl("outer_class");
            outer_class.Text = strClass;

            // output.Write(strBiblio);
            LiteralControl literal = (LiteralControl)FindControl("biblio");
            // literal.Text = "<div class='"+strClass+"'>" + strResult + strBiblio + "</biblio>";
            literal.Text = strResult + strBiblio;


            base.Render(output);
            return;

        ERROR1:
            output.Write(strError);

        }

        // 通过id值在儿子中找到控件
        public static Control FindControl(WebControl parent,
            string strID)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.ID == strID)
                    return control;
            }

            return null;
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
    }
}
