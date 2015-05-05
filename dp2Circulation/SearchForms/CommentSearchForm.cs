using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 评注查询窗
    /// </summary>
    public class CommentSearchForm: ItemSearchForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentSearchForm()
            : base()
        {
            this.DbType = "comment";
        }
    }
}
