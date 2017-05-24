using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class OrderListScript
    {
        public event EventHandler CallFunc = null;

        public dp2Circulation.OrderListViewerForm.Sheet Sheet = null;
        public BatchOrderForm BatchOrderForm = null;

        public void onSelectionChanged()
        {
            this.BatchOrderForm.OnSheetSelectionChanged(this.Sheet);
        }


    }
}
