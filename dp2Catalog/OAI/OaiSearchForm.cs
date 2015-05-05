using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
                MessageBox.Show(this, strError);

        }

        private void OaiSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void OaiSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}