using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using UpgradeUtil;

namespace dp2Circulation
{
    internal partial class TestJidaoForm : Form
    {
        public string MARC = "";
        public List<string> Xmls = new List<string>();

        public TestJidaoForm()
        {
            InitializeComponent();
        }

        private void TestJidaoForm_Load(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.MARC) == false)
            {
                nRet = this.jidaoControl1.SetData(
                    "920",
                    this.MARC,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripComboBox_dataSource_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox_dataSource_TextChanged(object sender, EventArgs e)
        {
            int nRet = 0;
            string strFieldName = this.toolStripComboBox_dataSource.Text;
            nRet = strFieldName.IndexOf(" ");
            if (nRet != -1)
                strFieldName = strFieldName.Substring(0, nRet).Trim();

            if (strFieldName.Length != 3)
                return;

            string strError = "";
            if (String.IsNullOrEmpty(toolStripComboBox_dataSource.Text) == true
                && String.IsNullOrEmpty(this.MARC) == true)
            {
                strError = "没有指定字段名";
                goto ERROR1;
            }


            if (String.IsNullOrEmpty(this.MARC) == false)
            {
                nRet = this.jidaoControl1.SetData(
                    strFieldName,
                    this.MARC,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripComboBox_dataSource_DropDown(object sender, EventArgs e)
        {
            if (this.toolStripComboBox_dataSource.Items.Count > 0)
                return;

            List<String> names = JidaoControl.GetMenuTexts(this.MARC);
            string [] items = new string[names.Count];
            names.CopyTo(items);
            this.toolStripComboBox_dataSource.Items.AddRange(items);
        }

        private void toolStripButton_upgrade_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> xmls = null;
            int nRet = this.jidaoControl1.Upgrade(this.MARC,
                "",
                out xmls,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            this.Xmls = xmls;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_check_Click(object sender, EventArgs e)
        {
            string strError = "";
            // List<string> xmls = null;
            int nRet = this.jidaoControl1.Check(this.MARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "经检查没有发现问题");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }
    }
}