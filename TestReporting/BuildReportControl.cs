using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

namespace TestReporting
{
    public partial class BuildReportControl : UserControl
    {
        public BuildReportControl()
        {
            InitializeComponent();
        }

        const int LINE_HEIGHT = 20;
        const int LINE_SEP = 2;
        const int LABEL_WIDTH = 120;
        const int TEXTBOX_WIDTH = 120;

        // 参数和控件关系对照表
        // 参数名 --> TextBox 对象
        Hashtable _control_table = new Hashtable();

        // 创建子控件
        public void CreateChildren(XmlElement parameters)
        {
            this.Controls.Clear();
            _control_table.Clear();

            int x = 0;
            int y = 0;
            XmlNodeList nodes = parameters.SelectNodes("parameter");
            foreach (XmlElement parameter in nodes)
            {
                string name = parameter.GetAttribute("name");

                Label label = new Label();
                label.Text = parameter.GetAttribute("comment");
                label.Location = new Point(x, y);
                label.Size = new Size(LABEL_WIDTH, LINE_HEIGHT);
                this.Controls.Add(label);

                TextBox textbox = new TextBox();
                textbox.Location = new Point(x + LABEL_WIDTH, y);
                textbox.Size = new Size(TEXTBOX_WIDTH, LINE_HEIGHT);
                this.Controls.Add(textbox);

                _control_table[name] = textbox;

                y += LINE_HEIGHT + LINE_SEP;
            }
        }

        public void SetValue(Hashtable param_table)
        {
            foreach (string name in param_table.Keys)
            {
                Control control = _control_table[name] as Control;
                if (control != null)
                    control.Text = param_table[name] as string;
            }
        }

        public void ClearValue()
        {
            foreach (string name in _control_table.Keys)
            {
                (_control_table[name] as Control).Text = "";
            }
        }

        public Hashtable GetValue()
        {
            Hashtable param_table = new Hashtable();
            foreach(string name in _control_table.Keys)
            {
                string value = (_control_table[name] as Control).Text;
                param_table[name] = value;
            }

            return param_table;
        }
    }
}
