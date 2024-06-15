using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// ѯ���Ƿ�ɾ���ֶεĶԻ���
    /// </summary>
    internal partial class DeleteFieldDlg : Form
    {
        public DeleteFieldDlg()
        {
            InitializeComponent();
        }

        private void DeleteFieldDlg_Load(object sender, EventArgs e)
        {
            // this.AcceptButton = this.button_no;
        }

        private void button_yes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button_no_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        public string Message
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        /// <summary>
        /// ����Ի����
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys ֵ֮һ������ʾҪ����ļ���</param>
        /// <returns>����ؼ�����ʹ�û�������Ϊ true������Ϊ false���������һ������</returns>
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 2006/11/14 changed
            if (keyData == Keys.Delete)
            {
                button_yes_Click(null, null);
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
    }
}