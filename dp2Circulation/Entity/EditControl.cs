using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Drawing.Drawing2D;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 本类已经废弃
    /// 编辑区域类的基类。实现了下拉列表的一些功能
    /// </summary>
    internal class EditControl1 : UserControl
    {
        delegate void Delegate_filterValue(Control control);

        // 不安全版本
        // 过滤掉 {} 包围的部分
        void __FilterValue(Control control)
        {
            string strText = Global.GetPureSeletedValue(control.Text);
            if (control.Text != strText)
                control.Text = strText;
        }

        // 安全版本
        public void FilterValue(Control control)
        {
            if (this.InvokeRequired == true)
            {
                Delegate_filterValue d = new Delegate_filterValue(__FilterValue);
                this.BeginInvoke(d, new object[] { control });
            }
            else
            {
                __FilterValue((Control)control);
            }
        }

        // 不安全版本
        // 过滤掉 {} 包围的部分
        // 还有列表值去重的功能
        void __FilterValueList(Control control)
        {
            List<string> results = StringUtil.FromListString(Global.GetPureSeletedValue(control.Text));
            StringUtil.RemoveDupNoSort(ref results);
            string strText = StringUtil.MakePathList(results);
            if (control.Text != strText)
                control.Text = strText;
        }

        // 安全版本
        public void FilterValueList(Control control)
        {
            if (this.InvokeRequired == true)
            {
                Delegate_filterValue d = new Delegate_filterValue(__FilterValueList);
                this.BeginInvoke(d, new object[] { control });
            }
            else
            {
                __FilterValueList((Control)control);
            }
        }
    }
}
