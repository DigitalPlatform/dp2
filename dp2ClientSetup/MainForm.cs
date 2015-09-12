using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace dp2ClientSetup
{
    public partial class MainForm : Form
    {
        public string DataDir = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 每个 zip 文件都有一个同名的 .zip.xml 文件，里面配置了如何展开的信息
        int ExpandZipFile(out string strError)
        {
            strError = "";

            // 在数据目录中遍历所有 .zip 文件
            DirectoryInfo di = new DirectoryInfo(this.DataDir);
            FileInfo[] fis = di.GetFiles("*.zip");
            foreach(FileInfo fi in fis)
            {

            }

            return 0;
        }

        int ExpandOneZipFile(string strZipFileName,
            out string strError)
        {
            strError = "";

            string strXmlFileName = strZipFileName + ".xml";

            return 0;
        }
    }
}
