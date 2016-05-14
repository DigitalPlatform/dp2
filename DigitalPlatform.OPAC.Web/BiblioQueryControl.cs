using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:BiblioQueryControl runat=server></{0}:BiblioQueryControl>")]
    public class BiblioQueryControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BiblioQueryControl.cs",
                typeof(BiblioSearchControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
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

        public string Lang
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        string m_strDefaultFormatName = "simple";

        // 缺省的面板格式
        public string DefaultFormatName
        {
            get
            {
                return this.m_strDefaultFormatName;
            }
            set
            {
                this.m_strDefaultFormatName = value;
            }
        }

        protected override void CreateChildControls()
        {
            CreatePrifix(String.IsNullOrEmpty(this.Title) == true ? this.GetString("命中结果") : this.Title,
                "content_wrapper");
            this.Controls.Add(new LiteralControl("<table class='query'>"));

            // tabcontrol
            this.Controls.Add(new LiteralControl(
    "<tr class='format'><td colspan='4'>"
));
            TabControl format_control = new TabControl();
            format_control.ID = "format_control";
            this.Controls.Add(format_control);
            this.Controls.Add(new LiteralControl(
     "</td></tr>"
 ));
            format_control.Description = this.GetString("面板格式");
            FillTabControl(format_control);

            this.Controls.Add(new LiteralControl(
                // "</table></div>"
               "</table>" + this.GetPostfixString()
               ));

        }

        void FillTabControl(TabControl tabcontrol)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];

            string strDefaultFormatName = this.DefaultFormatName;

            List<string> format_ids = new List<string> {
                "simple",
                "simplest",
                "logic" };
            List<string> format_names = new List<string> { 
                this.GetString("simple"),
                this.GetString("simplest"),
                this.GetString("logic") };

            tabcontrol.Columns.Clear();
            for (int i = 0; i < format_ids.Count; i++)
            {
                TabColumn column = new TabColumn();
                column.Name = format_names[i];
                column.ID = format_ids[i];
                tabcontrol.Columns.Add(column);

                if (this.DefaultFormatName == format_ids[i])
                    tabcontrol.ActiveTab = format_names[i];
            }
        }

        void CreatePrifix(string strTitle,
string strWrapperClass)
        {
            LiteralControl literal = new LiteralControl("<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>");
            this.Controls.Add(literal);

            literal = new LiteralControl(strTitle);
            literal.ID = "wrapper_title";
            this.Controls.Add(literal);

            literal = new LiteralControl("</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>");
            this.Controls.Add(literal);
        }

        public string GetPostfixString()
        {
            return "</div>";
        }

        void SetTitle(string strTitle)
        {
            LiteralControl literal = (LiteralControl)this.FindControl("wrapper_title");
            literal.Text = strTitle;
        }

        public string Title
        {
            get
            {
                String s = (String)ViewState[this.ID + "Title"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState[this.ID + "Title"] = value;
            }
        }
    }
}
