using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalPlatform.LibraryServer
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:TestControl1 runat=server></{0}:TestControl1>")]
    public class TestControl1 : WebControl, INamingContainer
    {
        public int Value
        {
            get
            {
                return Int32.Parse( ((TextBox)Controls[1]).Text );
            }
            set
            {
                ((TextBox)Controls[1]).Text = value.ToString();
            }

        }

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
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(Text);
        }
         */

        protected override void CreateChildControls()
        {
            // base.CreateChildControls();
            // Add Literal Controls
            this.Controls.Add(new LiteralControl("<h3>Value: "));

            // Add Textbox
            TextBox box = new TextBox();
            box.Text = "0";
            this.Controls.Add(box);

            // Add Literal Controls
            this.Controls.Add(new LiteralControl("</h3>"));

            // Add "Add" Button
            Button addButton = new Button();
            addButton.Text = "Add";
            addButton.Click += new EventHandler(this.AddBtn_Click);
            this.Controls.Add(addButton);

            // Add Literal Controls
            this.Controls.Add(new LiteralControl(" | "));

            // Add "Subtract" Button
            Button subtractButton = new Button();
            subtractButton.Text = "Subtract";
            subtractButton.Click +=new EventHandler(this.SubtractBtn_Click);
            this.Controls.Add(subtractButton);

        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            this.Value++;
        }

        private void SubtractBtn_Click(object sender, EventArgs e)
        {
            this.Value--;
        }
    }
}
