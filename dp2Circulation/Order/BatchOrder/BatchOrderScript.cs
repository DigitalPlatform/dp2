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

        public void onSelectionChanged()
        {
            this.BatchOrderForm.OnSelectionChanged();
        }

        // 在内存中创建一条新的订购记录
        // return:
        //      需要在 Web 页面显示出来的代表订购记录的 HTML 字符串
        public string newOrder(string strBiblioRecPath, string xml)
        {
            return this.BatchOrderForm.NewOrder(strBiblioRecPath, xml);
        }

        // 标记删除一条订购记录
        public void deleteOrder(string strBiblioRecPath, string refid)
        {
            this.BatchOrderForm.DeleteOrder(strBiblioRecPath, refid);
        }

        // 在内存中修改一条订购记录
        //      xml 描述如何修改订购字段的 XML 记录
        // return:
        //      需要在 Web 页面显示出来的代表修改后订购记录的 HTML 字符串
        public string changeOrder(string strBiblioRecPath, string refid, string xml)
        {
            return this.BatchOrderForm.ChangeOrder(strBiblioRecPath, refid, xml);
        }

        // 验证一条书目记录下属的若干订购记录
        public System.Reflection.IReflect verifyOrders(string strBiblioRecPath)
        {
            return this.BatchOrderForm.VerifyOrders(strBiblioRecPath);
        }

        public void loadBiblio(string strBiblioRecPath)
        {
            this.BatchOrderForm.LoadBiblio(strBiblioRecPath);
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

        // 出现对话框编辑修改去向字段内容。
        // return:
        //      null    放弃修改
        //      其他  修改后的去向字段内容。内存已经被修改了，注意用此返回值更新 Web 页面显示
        public string editDistribute(string strBiblioRecPath,
            string strOrderRefID)
        {
            return this.BatchOrderForm.EditDistribute(strBiblioRecPath, strOrderRefID);
        }

        // 当修改 copy 字段后，对去向字段和复本数之间的关系进行校验。
        // return:
        //      null    正确
        //      其他  错误原因
        public string verifyDistribute(string strBiblioRecPath,
    string strOrderRefID)
        {
            return this.BatchOrderForm.VerifyDistribute(strBiblioRecPath, strOrderRefID);
        }

        public string getOrderTitleLine(string strBiblioRecPath)
        {
            return BatchOrderForm.GetOrderTitleLine(strBiblioRecPath);
        }

        // 出现对话框编辑修改时间范围字段内容。
        // return:
        //      null    放弃修改
        //      其他  修改后的时间范围字段内容。内存已经被修改了，注意用此返回值更新 Web 页面显示
        public string editRange(string strBiblioRecPath,
            string strOrderRefID)
        {
            return this.BatchOrderForm.EditRange(strBiblioRecPath, strOrderRefID);
        }

        // 获知一个书目记录下的订购记录数量
        public int getOrderCount(string strBiblioRecPath)
        {
            return this.BatchOrderForm.GetOrderCount(strBiblioRecPath);
        }

        // 获知一个书目记录下，在内存中发生过修改的订购记录数量
        public int getOrderChangedCount(string strBiblioRecPath)
        {
            return this.BatchOrderForm.GetOrderChangedCount(strBiblioRecPath);
        }

        // 移除一条书目记录。只是从内存移走，并不删除数据库中已经存在的书目记录和下属订购记录
        public void removeBiblio(string strBiblioRecPath)
        {
            this.BatchOrderForm.RemoveBiblio(strBiblioRecPath);
        }
    }
}
