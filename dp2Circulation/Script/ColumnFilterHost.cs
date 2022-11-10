using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.MarcDom;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 配合 ColumnFilterHost 的 FilterDocument 派生类(MARC 过滤器文档类)
    /// </summary>
    public class ColumnFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对象
        /// </summary>
        public ColumnFilterHost Host = null;
    }

    /// <summary>
    /// BiblioSearchForm (书目查询窗) 中 .fltx 脚本功能的宿主类
    /// </summary>
    public class ColumnFilterHost
    {
        /// <summary>
        /// 参数表
        /// </summary>
        public Hashtable ColumnTable { get; set; }  // 引用

        // 2022/11/10
        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string RecPath { get; set; }

        /// <summary>
        /// 宿主 MDI 窗口。例如 BiblioSearchForm
        /// </summary>
        public object HostForm { get; set; }

        /// <summary>
        /// 视觉事项对象。
        /// </summary>
        public object UiItem { get; set; }
    }
}
