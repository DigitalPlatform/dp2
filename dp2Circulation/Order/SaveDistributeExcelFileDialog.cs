using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace dp2Circulation.Order
{
    public partial class SaveDistributeExcelFileDialog : Form
    {
        public SaveDistributeExcelFileDialog()
        {
            InitializeComponent();
        }

        private void button_getOutputFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的去向分配表 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.textBox_outputFileName.Text;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputFileName.Text = dlg.FileName;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_outputFileName.Text) == true)
            {
                strError = "尚未指定 Excel 文件名";
                goto ERROR1;
            }


            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string OutputFileName
        {
            get
            {
                return this.textBox_outputFileName.Text;
            }
            set
            {
                this.textBox_outputFileName.Text = value;
            }
        }

        public string Seller
        {
            get
            {
                if (comboBox_seller.Text == "<空>")
                    return "";

                if (string.IsNullOrEmpty(comboBox_seller.Text) == true)
                    return "*";
                return this.comboBox_seller.Text.Replace("<全部>", "*");
            }
            set
            {
                if (value == "<空>")
                    this.comboBox_seller.Text = "";
                else if (string.IsNullOrEmpty(value) == true)
                    this.comboBox_seller.Text = "<全部>";
                else
                    this.comboBox_seller.Text = value.Replace("*", "<全部>");
            }
        }

        private void comboBox_seller_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_seller.Items.Count > 0)
                return;

            this.comboBox_seller.Items.Add("<全部>");

            {
                string strError = "";
                string[] values = null;
                int nRet = Program.MainForm.GetValueTable("orderSeller",
                    "",
                    out values,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                this.comboBox_seller.Items.AddRange(values);
            }

            this.comboBox_seller.Items.Add("<空>");
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_seller);
                controls.Add(this.textBox_outputFileName);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_seller);
                controls.Add(this.textBox_outputFileName);
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
