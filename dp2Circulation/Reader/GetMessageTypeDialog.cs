using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace dp2Circulation
{
    public partial class GetMessageTypeDialog : Form
    {
        public GetMessageTypeDialog()
        {
            InitializeComponent();
        }

        public string TypeList
        {
            get
            {
                List<string> results = new List<string>();
                if (this.checkBox_dpmail.Checked)
                    results.Add("dpmail");
                if (this.checkBox_email.Checked)
                    results.Add("email");
                if (this.checkBox_mq.Checked)
                    results.Add("mq");
                if (this.checkBox_sms.Checked)
                    results.Add("sms");
                return StringUtil.MakePathList(results);
            }
            set
            {
                var list = StringUtil.SplitList(value);
                this.checkBox_dpmail.Checked = list.IndexOf("dpmail") != -1;
                this.checkBox_email.Checked = list.IndexOf("email") != -1;
                this.checkBox_mq.Checked = list.IndexOf("mq") != -1;
                this.checkBox_sms.Checked = list.IndexOf("sms") != -1;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.TypeList))
            {
                MessageBox.Show(this, "请选定至少一种消息类型");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
