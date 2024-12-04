using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Circulation
{
    /// <summary>
    /// 对比显示两条书目记录的对话框
    /// </summary>
    public class MarcRecordComparerDialog : PartialDeniedDialog
    {
        public MarcRecordComparerDialog()
        {
            this.button_Cancel.Visible = false;
            this.button_loadSaved.Visible = false;
            this.button_compareEdit.Visible = false;
            this.AcceptButton = this.button_compareEdit;
        }
    }

}
