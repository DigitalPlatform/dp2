using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    internal partial class CellLineDialog : Form
    {
        public CellLineDialog()
        {
            InitializeComponent();
        }

        private void CellLineDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.comboBox_fieldName.Text) == true)
            {
                MessageBox.Show(this, "尚未指定字段名");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string FieldName
        {
            get
            {
                string strValue = this.comboBox_fieldName.Text;
                return GetLeftPart(strValue);
            }
            set
            {
                this.comboBox_fieldName.Text = value;
            }
        }

        public string Caption
        {
            get
            {
                string strValue = this.textBox_caption.Text;
                if (String.IsNullOrEmpty(strValue) == false)
                    return strValue;

                // 实在不行，找combobox右侧
                strValue = this.comboBox_fieldName.Text;
                return GetRightPart(strValue);
            }
            set
            {
                this.textBox_caption.Text = value;
            }
        }

        private void comboBox_fieldName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nRet = this.comboBox_fieldName.Text.IndexOf("--");
            if (nRet == -1)
                return;

            string strRight = this.comboBox_fieldName.Text.Substring(nRet + 2).Trim();
            this.textBox_caption.Text = strRight;
        }

        static string GetRightPart(string strText)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
                return "";

            return strText.Substring(nRet+2).Trim();
        }

        static string GetLeftPart(string strText)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
                return strText;

            return strText.Substring(0, nRet).Trim();
        }

        public string[] GroupFieldNames = new string[] {
"seller -- 订购渠道",
"source -- 经费来源",
"price -- 单册价格",
"range -- 时间范围",
"batchNo -- 批次号",

"state -- 订购状态",
"range -- 时间范围",
"issueCount -- 包含期数",
"orderTime -- 订购时间",
"orderID -- 订单ID",
"comment -- 注释",
"catalogNo -- 书目号",
"copy -- 复本数",
"distribute -- 馆藏分配",
"class -- 订购类目",
"totalPrice -- 总价格",
"sellerAddres -- 书商地址",
        };

        public void FillGroupFieldNameTable()
        {
            this.comboBox_fieldName.Items.Clear();
            this.comboBox_fieldName.Items.AddRange(this.GroupFieldNames);
        }
    }
}