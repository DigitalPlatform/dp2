using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;
using System.Resources;
using System.Globalization;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:TabControl runat=server></{0}:TabControl>")]
    public class TabControl : WebControl, INamingContainer
    {
        public string Description = "";

        public List<TabColumn> Columns = new List<TabColumn>();

        public string ResFilename = "";

        static ResourceManager GetRm(string strResFilename)
        {
            return new ResourceManager(strResFilename,
                typeof(TabControl).Module.Assembly);
        }

        static string GetString(
            string strResFilename,
            string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name/*"en-US"*/);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm(strResFilename).GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 的 '" + strResFilename + "' 中没有找到对应的资源。";
            }
        }


        // 取消最外面的tag
        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        public string ActiveTab
        {
            get
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active_tab");
                return s.Value;
            }
            set
            {
                this.EnsureChildControls();

                HiddenField s = (HiddenField)this.FindControl("active_tab");
                s.Value = value;
            }
        }

        protected override void CreateChildControls()
        {
            // 总表格
            this.Controls.Add(new AutoIndentLiteral("<table class='tab'>"));

            HiddenField active_tab = new HiddenField();
            active_tab.ID = "active_tab";
            this.Controls.Add(active_tab);

            // tab栏目
            this.Controls.Add(new AutoIndentLiteral("<tr>"));

            // 左边空白
            this.Controls.Add(new AutoIndentLiteral("<td class='leftblank'>"
                + (String.IsNullOrEmpty(this.Description) == true ? "&nbsp;" : this.Description)
                + "</td>"));


            for (int i = 0; i < this.Columns.Count; i++)
            {
                TabColumn column = this.Columns[i];

                if (String.IsNullOrEmpty(this.ResFilename) == false)
                {
                    CreateOneColumn(column.ID,
                        GetString(this.ResFilename, column.Name));
                }
                else
                {
                    CreateOneColumn(column.ID,
                        column.Name);
                }
            }


            // 右边空白
            this.Controls.Add(new AutoIndentLiteral("<td class='rightblank'>&nbsp;</td>"));


            this.Controls.Add(new AutoIndentLiteral("</tr>"));

            this.Controls.Add(new AutoIndentLiteral("<tr class='bottom_line'><td colspan='10'>&nbsp;</td></tr>"));

            this.Controls.Add(new AutoIndentLiteral("</table/>"));

        }

        void CreateOneColumn(string strColumnID,
    string strColumnCaption)
        {
            if (String.IsNullOrEmpty(strColumnID) == true)
                strColumnID = strColumnCaption;

            LiteralControl literal = new LiteralControl();
            literal.Text = "<td class='";
            this.Controls.Add(literal);

            // 可以替换的class值
            literal = new LiteralControl();
            literal.ID = strColumnID + "_class";
            literal.Text = "";    // 缺省值
            this.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "'>";
            this.Controls.Add(literal);


            LinkButton barcode_button = new LinkButton();
            barcode_button.ID = strColumnID;
            barcode_button.Text = strColumnCaption;
            barcode_button.Click += new EventHandler(barcode_button_Click);
            this.Controls.Add(barcode_button);

            literal = new LiteralControl();
            literal.Text = "</td>";
            this.Controls.Add(literal);
        }


        void barcode_button_Click(object sender, EventArgs e)
        {
            LinkButton button = (LinkButton)sender;

            this.ActiveTab = button.ID;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            for (int i = 0; i < this.Columns.Count; i++)
            {
                TabColumn column = this.Columns[i];

                string strColumnID = column.ID;
                if (String.IsNullOrEmpty(strColumnID) == true)
                    strColumnID = column.Name;

                LiteralControl column_class = (LiteralControl)this.FindControl(strColumnID + "_class");
                if (strColumnID == this.ActiveTab)
                    column_class.Text = strColumnID + " active";
                else
                    column_class.Text = strColumnID + " normal";
            }

            base.Render(writer);
        }
    }

    public class Caption
    {
        public string Lang = "";
        public string Text = "";
    }

    public class TabColumn
    {
        // public List<Caption> Captions = null;
        public string Name = "";    // 语言中立的名字
        public string ID = "";  // 也被当作 css class
    }
}
