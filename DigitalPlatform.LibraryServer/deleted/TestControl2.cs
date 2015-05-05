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
    [ToolboxData("<{0}:TestControl2 runat=server></{0}:TestControl2>")]
    public class TestControl2 : WebControl, INamingContainer
    {
        public event EventHandler Change;

        public int Value
        {
            get
            {
                return Int32.Parse(((TextBox)Controls[1]).Text);
            }
            set
            {
                ((TextBox)Controls[1]).Text = value.ToString();
            }

        }

        protected override void CreateChildControls()
        {
            // base.CreateChildControls();
            // Add Literal Controls
            this.Controls.Add(new LiteralControl("<h3>Value: "));

            // Add Textbox
            TextBox box = new TextBox();
            box.Text = "0";
            box.TextChanged +=new EventHandler(this.TextBox_Changed);
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
            subtractButton.Click += new EventHandler(this.SubtractBtn_Click);
            this.Controls.Add(subtractButton);

        }

        protected void OnChange(EventArgs e)
        {
            Change(this, e);
        }

        private void TextBox_Changed(object sender, EventArgs e)
        {
            OnChange(EventArgs.Empty);
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            this.Value++;
            OnChange(EventArgs.Empty);
        }

        private void SubtractBtn_Click(object sender, EventArgs e)
        {
            this.Value--;
            OnChange(EventArgs.Empty);
        }
    }
}
