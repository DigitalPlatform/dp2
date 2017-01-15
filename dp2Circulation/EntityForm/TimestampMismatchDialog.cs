using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 保存书目记录时发现时间戳不匹配，对比显示两条书目记录的对话框
    /// </summary>
    public class TimestampMismatchDialog : PartialDeniedDialog
    {
        public TimestampMismatchDialog()
        {
            _rightTitle = "数据库中的记录";

            this.Text = "保存的时候发现时间戳不匹配";
            this.button_Cancel.Text = "取消";
            this.button_loadSaved.Text = "强行覆盖保存";
            this.button_compareEdit.Visible = true;
            this.AcceptButton = this.button_compareEdit;

            this.button_loadSaved.Click += button_loadSaved_Click;
            this.button_compareEdit.Click += button_compareEdit_Click;
        }

        void button_loadSaved_Click(object sender, EventArgs e)
        {
            this.Action = "retrySave";
            this.Close();
        }

        void button_compareEdit_Click(object sender, EventArgs e)
        {
            this.Action = "compareEdit";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
