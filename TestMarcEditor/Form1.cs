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

namespace TestMarcEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void marcEditor1_GetConfigFile(object sender, DigitalPlatform.Marc.GetConfigFileEventArgs e)
        {
            // 这个事件暂时用不到
        }

        private void marcEditor1_GetConfigDom(object sender, DigitalPlatform.Marc.GetConfigDomEventArgs e)
        {
            // e.Path 中可能是 "marcdef" 或 "marcvaluelist"
            string filename = Path.Combine(Environment.CurrentDirectory,
                e.Path);
            XmlDocument dom = new XmlDocument();
            dom.Load(filename);

            e.XmlDocument = dom;
        }
    }
}
