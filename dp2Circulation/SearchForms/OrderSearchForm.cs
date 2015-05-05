using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 订购查询窗
    /// </summary>
    public class OrderSearchForm : ItemSearchForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderSearchForm() : base()
        {
            this.DbType = "order";
        }
    }
}
