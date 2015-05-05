using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 出纳窗公共接口
    /// </summary>
    public interface IChargingForm
    {
        /// <summary>
        /// 显示快速操作对话框
        /// </summary>
        /// <param name="color">信息颜色</param>
        /// <param name="strCaption">对话框标题文字</param>
        /// <param name="strMessage">消息内容文字</param>
        /// <param name="nTarget">对话框关闭后要切换去的位置。为 READER_BARCODE READER_PASSWORD ITEM_BARCODE 之一</param>
        void FastMessageBox(InfoColor color,
            string strCaption,
            string strMessage,
            int nTarget);
    }
}
