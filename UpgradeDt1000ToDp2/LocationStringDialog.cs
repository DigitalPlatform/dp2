using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 编辑修改从数据中直接提取的馆藏事项列表
    /// </summary>
    public partial class LocationStringDialog : Form
    {
        public List<string> Locations = new List<string>();

        public LocationStringDialog()
        {
            InitializeComponent();
        }

        private void LocationStringDialog_Load(object sender, EventArgs e)
        {
            if (this.Locations != null)
            {
                string strText = "";
                for (int i = 0; i < this.Locations.Count; i++)
                {
                    if (String.IsNullOrEmpty(strText) == false)
                        strText += "\r\n";

                    strText += this.Locations[i];
                }

                this.textBox_lines.Text = strText;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Locations = BuildStringList();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        List<string> BuildStringList()
        {
            List<string> results = new List<string>(); ;
            for (int i = 0; i < this.textBox_lines.Lines.Length; i++)
            {
                string strLine = this.textBox_lines.Lines[i].Trim();

                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                results.Add(strLine);
            }

            return results;
        }
    }
}