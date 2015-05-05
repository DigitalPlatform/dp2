using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 期查询窗
    /// </summary>
    public class IssueSearchForm : ItemSearchForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public IssueSearchForm()
            : base()
        {
            this.DbType = "issue";
        }
    }
}
