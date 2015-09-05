using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 日历窗。管理图书馆开馆日历
    /// </summary>
    public partial class CalendarForm : MyForm
    {
#if NO
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        DigitalPlatform.Stop stop = null;
#endif

        string m_strCurrentCalendarName = "";

        const int WM_LOAD_CALENDAR = API.WM_USER + 200;
        const int WM_DROPDOWN = API.WM_USER + 201;

        /// <summary>
        /// 当前日历名
        /// </summary>
        public string CurrentCalendarName
        {
            get
            {
                return this.m_strCurrentCalendarName;
            }
            set
            {
                this.m_strCurrentCalendarName = value;
                // 刷新窗口标题
                this.Text = "日历: " + this.m_strCurrentCalendarName;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CalendarForm()
        {
            InitializeComponent();
        }

        private void CalendarForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.comboBox_calendarName.Text = MainForm.AppInfo.GetString(
                "CalendarForm",
                "CalendarName",
                "");


        }

        private void CalendarForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif

            if (this.calenderControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "CalendarForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }


        private void CalendarForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SetString(
        "CalendarForm",
        "CalendarName",
        this.comboBox_calendarName.Text);
            }

        }

        // 装入全部日历名
        int FillCalendarNames(out string strError)
        {
            this.comboBox_calendarName.Items.Clear();

            EnableControls(false, true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得全部日历名 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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
                    this.comboBox_calendarName.Items.Add(s);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true, true);
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 获得日历内容
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetCalendarContent(string strName,
            out string strRange,
            out string strContent,
            out string strComment,
            out string strError)
        {
            strRange = "";
            strContent = "";
            strComment = "";
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得日历 '"+strName+"' 的内容 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                CalenderInfo[] infos = null;

                long lRet = Channel.GetCalendar(
                    stop,
                    "get",
                    strName,
                    0,
                    -1,
                    out infos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                {
                    strError = "日历 '" + strName + "' 不存在。";
                    return 0;   // not found
                }

                if (lRet > 1)
                {
                    strError = "名字为 '" + strName + "' 的日历居然有 " + lRet.ToString() + " 个。请系统管理员修改此错误。";
                    return -1;
                }

                Debug.Assert(infos != null, "");
                strContent = infos[0].Content;
                strRange = infos[0].Range;
                strComment = infos[0].Comment;

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

        // 保存、创建、删除日历
        // return:
        //      -1  出错
        //      0   成功
        int SetCalendarContent(
            string strAction,
            string strName,
            string strRange,
            string strContent,
            string strComment,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对日历 '" + strName + "' 进行 "+strAction+" 操作 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                CalenderInfo　info = new CalenderInfo();
                info.Name = strName;
                info.Range = strRange;
                info.Comment = strComment;
                info.Content = strContent;

                long lRet = Channel.SetCalendar(
                    stop,
                    strAction,
                    info,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);

            }

            return 0;
        ERROR1:
            return -1;
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            LoadCalendar(this.comboBox_calendarName.Text);
        }

        int LoadCalendar(string strName)
        {
            string strError = "";

            if (this.calenderControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时装载新的数据，现有未保存信息将丢失。\r\n\r\n确实要继续装载? ",
    "CalendarForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;   // 放弃
                }
            }


            string strRange = "";
            string strContent = "";
            string strComment = "";

            this.calenderControl1.Clear();

            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = GetCalendarContent(strName,
                out strRange,
                out strContent,
                out strComment,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                goto ERROR1;
            }

            this.textBox_timeRange.Text = strRange;
            this.textBox_comment.Text = strComment;

            nRet = this.calenderControl1.SetData(strRange,
                1,
                strContent,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.button_delete.Enabled = true;
            this.CurrentCalendarName = strName;   // 保存当前日历名字
            this.calenderControl1.Changed = false;
            return 1;   // 正常装入
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;  // 出错
        }

        private void comboBox_calendarName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_calendarName.Items.Count > 0)
                return;
            string strError = "";
            int nRet = FillCalendarNames(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else 
            {

                if (this.comboBox_calendarName.Text == ""
                    && this.comboBox_calendarName.Items.Count > 0)
                {
                    this.comboBox_calendarName.Text = (string)this.comboBox_calendarName.Items[0];
                    API.PostMessage(this.Handle, WM_DROPDOWN, 0, 0);
                }
            }

        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            EnableControls(bEnable, false);
        }

        void EnableControls(bool bEnable, bool bExcludeNameList)
        {
            if (bExcludeNameList == true)
                this.comboBox_calendarName.Enabled = bEnable;

            this.button_load.Enabled = bEnable;
            this.calenderControl1.Enabled = bEnable;

            if (bEnable == false)
                this.button_save.Enabled = bEnable;
            else
                this.button_save.Enabled = this.calenderControl1.Changed;
        }

        private void comboBox_calendarName_DropDownClosed(object sender, EventArgs e)
        {
        }

        private void comboBox_calendarName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            API.PostMessage(this.Handle, WM_LOAD_CALENDAR, 0, 0);

        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DROPDOWN:
                    {
                        this.comboBox_calendarName.Focus();
                        this.comboBox_calendarName.DroppedDown = true;
                    }
                    return;
                case WM_LOAD_CALENDAR:
                    {
                        if (this.comboBox_calendarName.Text != "")
                        {
                            // 需要一个地方记忆改换前的名字，并在放弃装入新的后，恢复旧名字。

                            int nRet = LoadCalendar(this.comboBox_calendarName.Text);

                            // 恢复原来的名字
                            if (nRet == -1 || nRet == 0)
                                this.comboBox_calendarName.Text = this.CurrentCalendarName;
                        }
                        return;
                    }
                    // break;

            }
            base.DefWndProc(ref m);
        }

        private void comboBox_calendarName_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_load;
        }

        private void calenderControl1_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_save;
        }

        private void calenderControl1_BoxStateChanged(object sender, EventArgs e)
        {
            this.button_save.Enabled = true;
        }

        private void button_create_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strData = "";

            int nRet = calenderControl1.GetDates(1,
                out strData,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_timeRange.Text = this.calenderControl1.GetRangeString();

            // 创建到服务器
            // return:
            //      -1  出错
            //      0   成功
            nRet = SetCalendarContent(
                "new",
                this.comboBox_calendarName.Text,
                this.textBox_timeRange.Text,
                strData,
                this.textBox_comment.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            this.calenderControl1.Changed = false;
            this.button_save.Enabled = false;

            this.comboBox_calendarName.Items.Clear();   // 促使重新获取


            MessageBox.Show(this, "创建成功");
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strData = "";

            int nRet = calenderControl1.GetDates(1,
                out strData,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_timeRange.Text = this.calenderControl1.GetRangeString();

            // 保存到服务器
            // return:
            //      -1  出错
            //      0   成功
            nRet = SetCalendarContent(
                "change",
                this.comboBox_calendarName.Text,
                this.textBox_timeRange.Text,
                strData,
                this.textBox_comment.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.calenderControl1.Changed = false;
            this.button_save.Enabled = false;
            MessageBox.Show(this, "保存成功");
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void textBox_comment_TextChanged(object sender, EventArgs e)
        {
            this.button_save.Enabled = true;
            this.button_delete.Enabled = true;
        }

        // 删除
        private void button_delete_Click(object sender, EventArgs e)
        {
            string strError = "";

            DialogResult result = MessageBox.Show(this,
"确实要删除日历 '"+this.comboBox_calendarName.Text+"' ? ",
"CalendarForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            // 从服务器删除
            // return:
            //      -1  出错
            //      0   成功
            int nRet = SetCalendarContent(
                "delete",
                this.comboBox_calendarName.Text,
                "", // range
                "", // content
                "", // conmment
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.calenderControl1.Changed = false;
            this.button_save.Enabled = true;
            this.button_delete.Enabled = false;

            this.comboBox_calendarName.Items.Clear();   // 促使重新获取

            MessageBox.Show(this, "删除成功");
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void CalendarForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }
 
    }

    // 日历名排序器
    /*public*/ class CalencarNameComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string x, string y)
        {
            string strLibraryCode1 = "";
            string strPureName1 = "";

            Global.ParseCalendarName(x,
        out strLibraryCode1,
        out strPureName1);

            string strLibraryCode2 = "";
            string strPureName2 = "";

            Global.ParseCalendarName(y,
        out strLibraryCode2,
        out strPureName2);

            // 馆代码部分都为空，就比较纯名字部分
            if (string.IsNullOrEmpty(strLibraryCode1) == true
                && string.IsNullOrEmpty(strLibraryCode2) == true)
                return string.Compare(strPureName1, strPureName2);

            // 馆代码部分足以区别
            if (string.IsNullOrEmpty(strLibraryCode1) == true
                && string.IsNullOrEmpty(strLibraryCode2) == false)
                return -1;

            if (string.IsNullOrEmpty(strLibraryCode1) == false
                && string.IsNullOrEmpty(strLibraryCode2) == true)
                return 1;

            // 全字符串比较
            return string.Compare(x, y);
        }
    }
}