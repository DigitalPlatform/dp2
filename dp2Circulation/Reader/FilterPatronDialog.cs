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
    /// <summary>
    /// 指定筛选读者输出特性的对话框
    /// </summary>
    public partial class FilterPatronDialog : Form
    {
        public FilterPatronDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }


        /// <summary>
        /// “无在借册和违约金的”是否被勾选
        /// </summary>
        public bool NoBorrowAndOverdueItem
        {
            get
            {
                return this.checkBox_range_noBorrowAndOverdueItem.Checked;
            }
            set
            {
                this.checkBox_range_noBorrowAndOverdueItem.Checked = value;
            }

        }

        /// <summary>
        /// “有在借册的 / 已超期”是否被勾选
        /// </summary>
        public bool OutofPeriodItem
        {
            get
            {
                return this.checkBox_range_outofPeriod.Checked;
            }
            set
            {
                this.checkBox_range_outofPeriod.Checked = value;
            }

        }

        /// <summary>
        /// “有在借册的 / 未超期”是否被勾选
        /// </summary>
        public bool InPeriodItem
        {
            get
            {
                return this.checkBox_range_inPeriod.Checked;
            }
            set
            {
                this.checkBox_range_inPeriod.Checked = value;
            }

        }

        /// <summary>
        /// “有违约金的”是否被勾选
        /// </summary>
        public bool HasOverdueItem
        {
            get
            {
                return this.checkBox_range_hasOverdueItem.Checked;
            }
            set
            {
                this.checkBox_range_hasOverdueItem.Checked = value;
            }
        }

        private void checkBox_range_outofPeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_range_inPeriod.Checked == true
&& this.checkBox_range_outofPeriod.Checked == true)
            {
                this.checkBox_range_hasBorrowItem.Checked = true;
                this.checkBox_range_hasBorrowItem.CheckState = CheckState.Checked;
                return;
            }

            if (this.checkBox_range_inPeriod.Checked == true
                || this.checkBox_range_outofPeriod.Checked == true)
            {
                this.checkBox_range_hasBorrowItem.CheckState = CheckState.Indeterminate;
                this.checkBox_range_hasBorrowItem.Checked = true;
                return;
            }

            if (this.checkBox_range_inPeriod.Checked == false
                && this.checkBox_range_outofPeriod.Checked == false)
            {
                this.checkBox_range_hasBorrowItem.Checked = false;
                return;
            }

        }

        private void checkBox_range_inPeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_range_inPeriod.Checked == true
    && this.checkBox_range_outofPeriod.Checked == true)
            {
                this.checkBox_range_hasBorrowItem.Checked = true;
                this.checkBox_range_hasBorrowItem.CheckState = CheckState.Checked;
                return;
            }

            if (this.checkBox_range_inPeriod.Checked == true
                || this.checkBox_range_outofPeriod.Checked == true)
            {
                this.checkBox_range_hasBorrowItem.CheckState = CheckState.Indeterminate;
                this.checkBox_range_hasBorrowItem.Checked = true;
                return;
            }

            if (this.checkBox_range_inPeriod.Checked == false
                && this.checkBox_range_outofPeriod.Checked == false)
            {
                this.checkBox_range_hasBorrowItem.Checked = false;
                return;
            }

        }

        private void checkBox_range_hasBorrowItem_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_range_hasBorrowItem.Checked == true)
            {

                if (this.checkBox_range_hasBorrowItem.CheckState == CheckState.Indeterminate)
                {
                }
                else
                {
                    this.checkBox_range_outofPeriod.Checked = true;
                    this.checkBox_range_inPeriod.Checked = true;
                }
            }
            else
            {
                this.checkBox_range_outofPeriod.Checked = false;
                this.checkBox_range_inPeriod.Checked = false;
            }
        }

    }
}
