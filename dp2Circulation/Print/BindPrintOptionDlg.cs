using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 打印装订单 的 打印选项对话框
    /// </summary>
    internal class BindPrintOptionDlg : PrintOptionDlg
    {
        // 合订册的栏目定义
        public List<Column> BindColumns
        {
            get
            {
                return GetColumns(this._dlg.ListView);
            }
            set
            {
                LoadColumns(value, this._dlg.ListView);
            }
        }

        string[] _bindColumnItems = null;

        public string[] BindColumnItems
        {
            get
            {
                return _bindColumnItems;
            }
            set
            {
                _bindColumnItems = value;
                if (_dlg != null)
                    _dlg.ColumnItems = value;
            }
        }

        PrintOptionDlg _dlg = null;

        public BindPrintOptionDlg() : base()
        {
            _dlg = new PrintOptionDlg();
            _dlg.ColumnItems = BindColumnItems;
            TabPage new_page = _dlg.PageColumns;

            new_page.Text = "合订册栏目定义";
            this.TabControl.TabPages.Add(new_page);

            this.PageColumns.Text = "成员册栏目定义";
        }
    }
}
