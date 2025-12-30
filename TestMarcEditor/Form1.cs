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

using LibraryStudio.Forms;
using DigitalPlatform.Text;

namespace TestMarcEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // this.marcEditor1.BorderStyle = BorderStyle.None;
            //marcEditor1.Click += MarcEditor1_Click;
            marcEditor1.SelectedFieldChanged += MarcEditor1_SelectedFieldChanged;
        }

        private void MarcEditor1_SelectedFieldChanged(object sender, EventArgs e)
        {
            /*
            var index = marcEditor1.FocusedFieldIndex;
            var field = marcEditor1.FocusedField;
            */
        }

        private void MarcEditor1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "click");
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
            if (filename.Contains("#"))
            {
                StringUtil.ParseTwoPart(filename,
                    "#",
                    out filename,
                    out string strFragment);
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(filename);

            e.XmlDocument = dom;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.marcEditor1.Content = MarcRecord.BuildContent(
                @"012345678901234567890123
001ABCDEFG
2001 $aAAA$bBBB
801  $aCN$bBBB$cCCC"
);

            this.marcControl1.Content = "012345678901234567890123";
        }
    }
}
