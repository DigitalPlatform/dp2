using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 开馆日历 属性页
    /// </summary>
    public partial class ManagerForm
    {
        // 日历编辑界面中的日历名版本
        int m_nCalendarVersion = 0;

        // 图标
        /// <summary>
        /// 普通
        /// </summary>
        const int ITEMTYPE_NORMAL = 0;
        /// <summary>
        /// 新增
        /// </summary>
        const int ITEMTYPE_NEW = 1;
        /// <summary>
        /// 发生过修改
        /// </summary>
        const int ITEMTYPE_CHANGED = 2;
        /// <summary>
        /// 被删除
        /// </summary>
        const int ITEMTYPE_DELETED = 3;

        // 栏目
        const int COLUMN_CALENDAR_NAME = 0;
        const int COLUMN_CALENDAR_RANGE = 1;
        const int COLUMN_CALENDAR_COMMENT = 2;
        const int COLUMN_CALENDAR_CONTENT = 3;

        bool m_bCalendarDefChanged = false;

        /// <summary>
        /// 日历定义是否被修改
        /// </summary>
        public bool CalendarDefChanged
        {
            get
            {
                return this.m_bCalendarDefChanged;
            }
            set
            {
                this.m_bCalendarDefChanged = value;
                if (value == true)
                    this.toolStripButton_calendar_save.Enabled = true;
                else
                    this.toolStripButton_calendar_save.Enabled = false;
            }
        }

        // 列出所有日历
        // 需要在 NewListRightsTables() 以前调用
        int ListCalendars(out string strError)
        {
            strError = "";

            if (this.CalendarDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内日历定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            this.listView_calendar.Items.Clear();
            //this.SortColumns.Clear();
            //SortColumns.ClearColumnSortDisplay(this.listView_calendar.Columns);

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得全部日历 ...");
            stop.BeginLoop();

            try
            {
                int nStart = 0;
                int nCount = 100;

                while (true)
                {
                    CalenderInfo[] infos = null;

                    long lRet = Channel.GetCalendar(
                        stop,
                        (StringUtil.CompareVersion(this.MainForm.ServerVersion, "2.29") < 0 ? "list" : "get"), // "list",
                        "",
                        nStart,
                        nCount,
                        out infos,
                        out strError);
                    if (lRet == -1)
                        return -1;
                    if (lRet == 0)
                        break;

                    // 
                    foreach (CalenderInfo info in infos)
                    {
                        ListViewItem item = new ListViewItem();
                        SetCalendarItemState(item, ITEMTYPE_NORMAL);

                        ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_NAME, info.Name);
                        ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_RANGE, info.Range);
                        ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_COMMENT, info.Comment);
                        ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_CONTENT, info.Content);
                        this.listView_calendar.Items.Add(item);
                    }

                    nStart += infos.Length;
                    if (nStart >= lRet)
                        break;
                }

#if NO
                names.Sort(new CalencarNameComparer());
                foreach (string s in names)
                {
                    calendar_names.Add(s);
                }
#endif
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }


            this.CalendarDefChanged = false;

            // dp2Library 版本 2.29 以后，才允许 get 获得全部事项内容，此处界面才允许修改日历定义
            if (StringUtil.CompareVersion(this.MainForm.ServerVersion,"2.29") < 0)
                this.toolStrip_calendar.Enabled = false;

            // 缺省按照第一列排序
            if (this.listView_calendar.ListViewItemSorter == null)
                listView_calendar_ColumnClick(this, new ColumnClickEventArgs(0));

            listView_calendar_SelectedIndexChanged(this, null);
            return 1;
        }

        // 获得当前的全部日历名
        int GetCalendarNames(out List<string> calendar_names,
            out string strError)
        {
            strError = "";
            calendar_names = new List<string>();

            foreach (ListViewItem item in this.listView_calendar.Items)
            {
                if (item.ImageIndex == ITEMTYPE_DELETED)
                    continue;

                string strName = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_NAME);
                calendar_names.Add(strName);
            }

            return 1;
        }

#if NO
        // 装入全部日历名
        int GetCalendarNames(out List<string> calendar_names,
            out string strError)
        {
            strError = "";
            calendar_names = new List<string>();

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得全部日历名 ...");
            stop.BeginLoop();

            try
            {
                int nStart = 0;
                int nCount = 100;
                List<string> names = new List<string>();

                while (true)
                {
                    CalenderInfo[] infos = null;

                    long lRet = Channel.GetCalendar(
                        stop,
                        "list",
                        "",
                        nStart,
                        nCount,
                        out infos,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    if (lRet == 0)
                        break;

                    // 
                    for (int i = 0; i < infos.Length; i++)
                    {
                        names.Add(infos[i].Name);
                    }

                    nStart += infos.Length;
                    if (nStart >= lRet)
                        break;
                }

                names.Sort(new CalencarNameComparer());
                foreach (string s in names)
                {
                    calendar_names.Add(s);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 1;
        ERROR1:
            return -1;
        }
#endif

        static void SetCalendarItemState(ListViewItem item, int nImageIndex)
        {
            item.ImageIndex = nImageIndex;
            if (nImageIndex == ITEMTYPE_NORMAL)
            {
                item.BackColor = Color.White;
                item.ForeColor = Color.Black;
            }
            else if (nImageIndex == ITEMTYPE_NEW)
            {
                item.BackColor = Color.Yellow;
                item.ForeColor = Color.Black;
            }
            else if (nImageIndex == ITEMTYPE_CHANGED)
            {
                item.BackColor = Color.Green;
                item.ForeColor = Color.White;
            }
            else if (nImageIndex == ITEMTYPE_DELETED)
            {
                item.BackColor = Color.White;
                item.ForeColor = Color.Gray;
            }

        }

        int SaveCalendars(out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存日历 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                int nErrorCount = 0;
                List<ListViewItem> delete_items = new List<ListViewItem>();

                foreach (ListViewItem item in this.listView_calendar.Items)
                {
                    if (item.ImageIndex == ITEMTYPE_NORMAL)
                        continue;

                    CalenderInfo info = new CalenderInfo();
                    info.Name = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_NAME);
                    info.Range = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_RANGE);
                    info.Comment = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_COMMENT);
                    info.Content = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_CONTENT);

                    string strAction = "";
                    if (item.ImageIndex == ITEMTYPE_NEW)
                        strAction = "new";
                    else if (item.ImageIndex == ITEMTYPE_CHANGED)
                        strAction = "change";
                    else if (item.ImageIndex == ITEMTYPE_DELETED)
                        strAction = "delete";
                    else
                    {
                        strError = "未知的 item.ImageIndex ["+item.ImageIndex.ToString()+"]";
                        return -1;
                    }

                    long lRet = Channel.SetCalendar(
                        stop,
                        strAction,
                        info,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
    "针对日历 " + info.Name + " 进行 "+strAction+" 操作出错："+strError+"\r\n\r\n请问是否继续进行余下的保存操作?",
    "ManagerForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                        if (result == System.Windows.Forms.DialogResult.No)
                            return -1;
                        nErrorCount++;
                        continue;
                    }

                    if (item.ImageIndex == ITEMTYPE_DELETED)
                        delete_items.Add(item);
                    else
                        SetCalendarItemState(item, ITEMTYPE_NORMAL);
                }

                foreach (ListViewItem item in delete_items)
                {
                    this.listView_calendar.Items.Remove(item);
                }

                if (nErrorCount == 0)
                    this.CalendarDefChanged = false;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 0;
        }

    }
}
