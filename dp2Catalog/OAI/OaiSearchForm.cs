using System;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Catalog
{
    public partial class OaiSearchForm : Form
    {
        public MainForm MainForm = null;

        public OaiSearchForm()
        {
            InitializeComponent();
        }

        private void OaiSearchForm_Load(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";
            nRet = this.oaiTargeControl1.Load(MainForm.DataDir + "\\oai_server.xml",
                out strError);
            if (nRet == -1)
                this.MessageBoxShow(strError);

        }

        private void OaiSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void OaiSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}