using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    internal partial class PlaceIdcardDialog : Form
    {
        /// <summary>
        /// 读卡
        /// </summary>
        public event ReadCardEventHandler ReadCard = null;

        public PlaceIdcardDialog()
        {
            InitializeComponent();
        }

        private void PlaceIdcardDialog_Load(object sender, EventArgs e)
        {
        }

        private void PlaceIdcardDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_retry_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.ReadCard != null)
            {
                ReadCardEventArgs e1 = new ReadCardEventArgs();
                Cursor old_cursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                this.ReadCard(this, e1);

                this.Cursor = old_cursor;

                if (e1.Done == true)
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }
        }

        public bool AutoRetry
        {
            get
            {
                return this.checkBox_autoRetry.Checked;
            }
            set
            {
                this.checkBox_autoRetry.Checked = value;
            }
        }

        private void checkBox_autoRetry_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_autoRetry.Checked == true)
                this.timer1.Enabled = true;
            else
                this.timer1.Enabled = false;
        }
    }

    /// <summary>
    /// 读卡事件
    /// </summary>
    /// <param name="sender">触发者</param>
    /// <param name="e">事件参数</param>
    public delegate void ReadCardEventHandler(object sender,
ReadCardEventArgs e);

    /// <summary>
    /// 读卡事件的参数
    /// </summary>
    public class ReadCardEventArgs : EventArgs
    {
        /// <summary>
        /// [out] 读卡完成
        /// </summary>
        public bool Done = false;
    }
}
