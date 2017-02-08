using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Circulation
{
    public partial class DistributeDialog : Form
    {
        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        int _versionTextBox = 0;
        int _versionControl = 0;

        public DistributeDialog()
        {
            InitializeComponent();

            this.locationEditControl1.ArriveMode = false;
        }

        public string DistributeString
        {
            get
            {
                Sync();
                return this.locationEditControl1.Value;
            }
            set
            {
                this.locationEditControl1.Value = value;
                this.textBox_text.Text = value;
                _versionControl = 0;
                _versionTextBox = 0;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.Sync();

            string strError = "";
            // 进行检查
            // return:
            //      -1  函数运行出错
            //      0   检查没有发现错误
            //      1   检查发现了错误
            int nRet = this.locationEditControl1.Check(out strError);
            if (nRet != 0)
                goto ERROR1;
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

        private void textBox_text_TextChanged(object sender, EventArgs e)
        {
            _versionTextBox++;
        }

        private void locationEditControl1_ContentChanged(object sender, DigitalPlatform.ContentChangedEventArgs e)
        {
            _versionControl++;
        }

        int _inAsync = 0;
        void Sync()
        {
            if (_inAsync > 0)
                return;
            Cursor old_cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            _inAsync++;
            try
            {
                if (_versionControl > _versionTextBox)
                    this.textBox_text.Text = this.locationEditControl1.Value;
                else if (_versionTextBox > _versionControl)
                    this.locationEditControl1.Value = this.textBox_text.Text;
                _versionControl = 0;
                _versionTextBox = 0;
            }
            finally
            {
                _inAsync--;
                this.Cursor = old_cursor;
            }
        }

        private void textBox_text_Leave(object sender, EventArgs e)
        {
            Sync();
        }

        private void locationEditControl1_Leave(object sender, EventArgs e)
        {
            Sync();
        }

        public int Count
        {
            get
            {
                return this.locationEditControl1.Count;
            }
            set
            {
                if (value != -1)
                    this.locationEditControl1.Count = value;
            }
        }

        private void locationEditControl1_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }
    }
}
