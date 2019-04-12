using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DigitalPlatform.Z3950.UI
{
    public partial class ZServerListDialog : Form
    {
        public string XmlFileName { get; set; }

        XmlDocument _dom = new XmlDocument();

        public ZServerListDialog()
        {
            InitializeComponent();
        }

        private void ZServerListDialog_Load(object sender, EventArgs e)
        {

        }

        private void ZServerListDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ZServerListDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

        }

#if NO
        NormalResult FillList()
        {
            this.listView1.Items.Clear();

            if (string.IsNullOrEmpty(this.XmlFileName))
                return new NormalResult();

            XmlDocument _dom = new XmlDocument();
            try
            {
                _dom.Load(this.XmlFileName);
            }
            catch(FileNotFoundException)
            {
                _dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                return new NormalResult { Value = -1, ErrorInfo = $"装载配置文件 {this.XmlFileName} 出现异常: {ex.Message}" };
            }

            XmlNodeList servers = _dom.DocumentElement.SelectNodes("server");
        }
#endif
    }
}
