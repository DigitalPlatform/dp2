using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
//using DigitalPlatform.CirculationClient;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:SearchFilterControl runat=server></{0}:SearchFilterControl>")]
    public class SearchFilterControl : WebControl, INamingContainer
    {
        public event TreeItemClickEventHandler TreeItemClick = null;

        public override void Dispose()
        {
            this.TreeItemClick = null;

            base.Dispose();
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.SearchFilterControl.cs",
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


        public string ResultSetName
        {
            get
            {
                this.EnsureChildControls();
                HiddenField resultsetname = (HiddenField)this.FindControl("resultsetname");
                if (resultsetname == null)
                    return "";
                return resultsetname.Value;
            }
            set
            {
                this.EnsureChildControls();
                HiddenField resultsetname = (HiddenField)this.FindControl("resultsetname");
                if (resultsetname == null)
                    return;
                resultsetname.Value = value;
            }
        }

        public string SelectedNodePath
        {
            get
            {
                this.EnsureChildControls();
                HiddenField selected_node = (HiddenField)this.FindControl("selected-data");
                return selected_node.Value;
            }
            set
            {
                this.EnsureChildControls();
                HiddenField selected_node = (HiddenField)this.FindControl("selected-data");
                selected_node.Value = value;
            }
        }

        protected override void CreateChildControls()
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            HiddenField resultsetname = new HiddenField();
            resultsetname.ID = "resultsetname";
            this.Controls.Add(resultsetname);

            Panel title = new Panel();
            title.ID = "title";
            this.Controls.Add(title);

            LiteralControl titletext = new LiteralControl();
            titletext.Text = this.GetString("分面导航");
            title.Controls.Add(titletext);

            Panel progressbar = new Panel();
            progressbar.ID = "progressbar";
            this.Controls.Add(progressbar);

            HiddenField selected_node = new HiddenField();
            selected_node.ID = "selected-data";
            this.Controls.Add(selected_node);

            Button button = new Button();
            button.ID = "button";
            button.CssClass = "treebutton hidden";
            button.Click += new EventHandler(button_Click);
            this.Controls.Add(button);

        }

        void button_Click(object sender, EventArgs e)
        {
            HiddenField value = (HiddenField)this.FindControl("selected-data");

            if (this.TreeItemClick != null)
            {
                TreeItemClickEventArgs e1 = new TreeItemClickEventArgs();
                e1.Url = value.Value;
                this.TreeItemClick(this, e1);
            }
        }

    }
}
