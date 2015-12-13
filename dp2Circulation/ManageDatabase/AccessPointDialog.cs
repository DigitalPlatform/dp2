using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    internal partial class AccessPointDialog : Form
    {
        public BiblioDbFromInfo[] DbFromInfos = null;   // 书目库检索路径信息


        public AccessPointDialog()
        {
            InitializeComponent();
        }

        private void AccessPointDialog_Load(object sender, EventArgs e)
        {
            if (this.DbFromInfos != null)
            {
                FillFromList(this.comboBox_fromName);
            }
        }

        void FillFromList(ComboBox comboBox_from)
        {
            comboBox_from.Items.Clear();

            comboBox_from.Items.Add("<全部>");

            if (this.DbFromInfos == null)
                return;

            Debug.Assert(this.DbFromInfos != null);

            string strFirstItem = "";
            // 装入检索途径
            for (int i = 0; i < this.DbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.DbFromInfos[i];

                comboBox_from.Items.Add(info.Caption/* + "(" + infos[i].Style+ ")"*/);

                if (i == 0)
                    strFirstItem = info.Caption;
            }

            comboBox_from.Text = strFirstItem;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.comboBox_fromName.Text == "")
            {
                strError = "尚未输入检索途径";
                goto ERROR1;
            }

            if (this.textBox_weight.Text == "")
            {
                strError = "尚未输入权值";
                goto ERROR1;
            }

            if (this.tabComboBox_searchStyle.Text == "")
            {
                strError = "尚未指定检索方式";
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string FromName
        {
            get
            {
                return this.comboBox_fromName.Text;
            }
            set
            {
                this.comboBox_fromName.Text = value;
            }
        }

        public string Weight
        {
            get
            {
                return this.textBox_weight.Text;
            }
            set
            {
                this.textBox_weight.Text = value;
            }
        }

        public string SearchStyle
        {
            get
            {
                string strText = this.tabComboBox_searchStyle.Text;
                return TabComboBox.GetLeftPart(strText);
            }
            set
            {
                this.tabComboBox_searchStyle.Text = value;
            }
        }

        private void textBox_weight_Validating(object sender, CancelEventArgs e)
        {
            if (StringUtil.IsPureNumber(this.textBox_weight.Text) == false)
            {
                string[] parts = this.textBox_weight.Text.Split(new char[] {','});
                for (int i = 0; i < parts.Length; i++)
                {
                    if (StringUtil.IsPureNumber(parts[i]) == false)
                    {
                        MessageBox.Show(this, "权值必须为纯数字，或者逗号间隔的纯数字");
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}