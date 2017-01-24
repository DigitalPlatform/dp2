using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class BatchOrderScript
    {
        public event EventHandler CallFunc = null;

        public BatchOrderForm BatchOrderForm = null;

        // 订购字段发生改变
        public void onOrderChanged(string strBiblioRecPath,
            string strOrderRefID,
            string strFieldName,
            string strValue)
        {
            this.BatchOrderForm.OnOrderChanged(strBiblioRecPath,
    strOrderRefID,
    strFieldName,
    strValue);
        }

        public string newOrder(string strBiblioRecPath)
        {
            return this.BatchOrderForm.NewOrder(strBiblioRecPath);
        }

        public void deleteOrder(string strBiblioRecPath, string refid)
        {
            this.BatchOrderForm.DeleteOrder(strBiblioRecPath, refid);
        }

#if NO
        public int getArriveCount(string copy)
        {
            try
            {
                return Convert.ToInt32(OrderDesignControl.GetCopyFromCopyString(copy));
            }
            catch
            {
                return -1;  // error
            }
        }
#endif

        public string editDistribute(string strBiblioRecPath,
            string strOrderRefID)
        {
            return this.BatchOrderForm.EditDistribute(strBiblioRecPath, strOrderRefID);
        }
    }
}
