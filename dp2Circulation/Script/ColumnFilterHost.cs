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
        public Hashtable ColumnTable = null;// 引用

        /// <summary>
        /// 视觉事项对象。
        /// </summary>
        public object UiItem = null;
    }
}
