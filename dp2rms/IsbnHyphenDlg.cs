using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2rms
{
    public partial class IsbnHyphenDlg : Form
    {
        public MainForm MainForm = null;

        public IsbnHyphenDlg()
        {
            InitializeComponent();
        }

        private void button_addHyphen_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.textBox_isbn.Text) == true)
            {
                strError = "尚未输入待加工的ISBN号";
                goto ERROR1;
            }

            int nRet = MainForm.LoadIsbnSplitter(true, out strError);
            if (nRet == -1)
                goto ERROR1;

            string strResult = "";

            nRet = MainForm.IsbnSplitter.IsbnInsertHyphen(this.textBox_isbn.Text,
                "auto",
                out strResult,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_isbn.Text = strResult;

            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}