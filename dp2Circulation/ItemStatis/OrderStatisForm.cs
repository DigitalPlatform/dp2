using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 订购统计窗
    /// </summary>
    public class OrderStatisForm : ItemStatisForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderStatisForm()
        {
            this.DbType = "order";
        }
    }
}
