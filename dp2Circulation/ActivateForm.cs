using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 激活窗
    /// </summary>
    public partial class ActivateForm : MyForm
    {
        Commander commander = null;

        const int WM_LOAD_OLD_USERINFO = API.WM_USER + 200;
        const int WM_LOAD_NEW_USERINFO = API.WM_USER + 201;
        const int WM_SAVE_OLD_RECORD = API.WM_USER + 202;
        const int WM_SAVE_NEW_RECORD = API.WM_USER + 203;
        const int WM_DEVOLVE = API.WM_USER + 204;
        const int WM_ACTIVATE_TARGET = API.WM_USER + 205;



        WebExternalHost m_webExternalHost_new = new WebExternalHost();
        WebExternalHost m_webExternalHost_old = new WebExternalHost();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActivateForm()
        {
            this.UseLooping = true; // 2022/11/1

            InitializeComponent();
        }

        private void ActivateForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.readerEditControl_old.SetReadOnly("librarian");
            this.readerEditControl_old.GetValueTable += new GetValueTableEventHandler(readerEditControl1_GetValueTable);
            this.readerEditControl_old.Initializing = false;   // 如果没有此句，一开始在空模板上修改就不会变色

            this.readerEditControl_new.SetReadOnly("librarian");
            this.readerEditControl_new.GetValueTable += new GetValueTableEventHandler(readerEditControl1_GetValueTable);
            this.readerEditControl_new.Initializing = false;   // 如果没有此句，一开始在空模板上修改就不会变色


            // webbrowser
            this.m_webExternalHost_new.Initial(// Program.MainForm, 
                this.webBrowser_newReaderInfo);
            this.webBrowser_newReaderInfo.ObjectForScripting = this.m_webExternalHost_new;

            this.m_webExternalHost_old.Initial(// Program.MainForm, 
                this.webBrowser_oldReaderInfo);
            this.webBrowser_oldReaderInfo.ObjectForScripting = this.m_webExternalHost_old;

            // commander
            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            if (this.m_webExternalHost_old.ChannelInUse ||
                this.m_webExternalHost_new.ChannelInUse == true)
            {
                e.IsBusy = true;
            }
        }

        private void ActivateForm_FormClosing(object sender, FormClosingEventArgs e)
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

            if (this.readerEditControl_old.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前旧证有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "ActivateForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (this.readerEditControl_new.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前新证有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "ActivateForm",
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

        private void ActivateForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHost_new != null)
                this.m_webExternalHost_new.Destroy();

            if (this.m_webExternalHost_old != null)
                this.m_webExternalHost_old.Destroy();

            this.readerEditControl_old.GetValueTable -= new GetValueTableEventHandler(readerEditControl1_GetValueTable);
            this.readerEditControl_new.GetValueTable -= new GetValueTableEventHandler(readerEditControl1_GetValueTable);
        }

        void readerEditControl1_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void textBox_oldBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_loadOldUserInfo;
            Program.MainForm.EnterPatronIdEdit(InputType.PQR);
        }

        private void textBox_oldBarcode_Leave(object sender, EventArgs e)
        {
            Program.MainForm.LeavePatronIdEdit();

        }

        private void textBox_newBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_loadNewUserInfo;

            Program.MainForm.EnterPatronIdEdit(InputType.PQR);
        }

        private void textBox_newBarcode_Leave(object sender, EventArgs e)
        {
            Program.MainForm.LeavePatronIdEdit();
        }

        /// <summary>
        /// 装载旧记录
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int LoadOldRecord(string strReaderBarcode)
        {
            this.textBox_oldBarcode.Text = strReaderBarcode;

            int nRet = this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_old,
                this.m_webExternalHost_old,
                // this.webBrowser_oldReaderInfo,
                this.webBrowser_oldXml);
            if (this.textBox_oldBarcode.Text != strReaderBarcode)
                this.textBox_oldBarcode.Text = strReaderBarcode;
            return nRet;
        }

        /// <summary>
        /// 装载新记录
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int LoadNewRecord(string strReaderBarcode)
        {
            this.textBox_newBarcode.Text = strReaderBarcode;

            int nRet = this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_new,
                this.m_webExternalHost_new,
                // this.webBrowser_newReaderInfo,
                this.webBrowser_newXml);
            if (this.textBox_newBarcode.Text != strReaderBarcode)
                this.textBox_newBarcode.Text = strReaderBarcode;

            return nRet;
        }

        private void button_loadOldUserInfo_Click(object sender, EventArgs e)
        {
            if (this.textBox_oldBarcode.Text == "")
            {
                MessageBox.Show(this, "尚未指定旧读者证的证条码号");
                return;
            }

            this.button_loadOldUserInfo.Enabled = false;

            this.m_webExternalHost_old.StopPrevious();
            this.webBrowser_oldReaderInfo.Stop();

            this.commander.AddMessage(WM_LOAD_OLD_USERINFO);
        }

        private void button_loadNewUserInfo_Click(object sender, EventArgs e)
        {
            if (this.textBox_newBarcode.Text == "")
            {
                MessageBox.Show(this, "尚未指定新读者证的证条码号");
                return;
            }

            this.button_loadNewUserInfo.Enabled = false;

            this.m_webExternalHost_new.StopPrevious();
            this.webBrowser_newReaderInfo.Stop();

            this.commander.AddMessage(WM_LOAD_NEW_USERINFO);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOAD_OLD_USERINFO:
                        if (this.m_webExternalHost_old.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            string strReaderBarcode = this.textBox_oldBarcode.Text;
                            this.LoadRecord(ref strReaderBarcode,
                                this.readerEditControl_old,
                                this.m_webExternalHost_old,
                                // this.webBrowser_oldReaderInfo,
                                this.webBrowser_oldXml);
                            if (this.textBox_oldBarcode.Text != strReaderBarcode)
                                this.textBox_oldBarcode.Text = strReaderBarcode;
                        }
                    return;
                case WM_LOAD_NEW_USERINFO:
                    if (this.m_webExternalHost_new.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {
                        string strReaderBarcode = this.textBox_newBarcode.Text;
                        this.LoadRecord(ref strReaderBarcode,
                            this.readerEditControl_new,
                            this.m_webExternalHost_new,
                            // this.webBrowser_newReaderInfo,
                            this.webBrowser_newXml);
                        if (this.textBox_newBarcode.Text != strReaderBarcode)
                            this.textBox_newBarcode.Text = strReaderBarcode;
                    }
                    return;
                case WM_SAVE_OLD_RECORD:
                    if (this.m_webExternalHost_old.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {

                        this.SaveOldRecord();
                    }
                    return;
                case WM_SAVE_NEW_RECORD:
                    if (this.m_webExternalHost_new.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {

                        this.SaveNewRecord();
                    }
                    return;
                case WM_DEVOLVE:
                    if (this.m_webExternalHost_new.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {

                        this.Devolve();
                    }
                    return;
                case WM_ACTIVATE_TARGET:
                    if (this.m_webExternalHost_new.CanCallNew(
                        this.commander,
                        m.Msg) == true)
                    {

                        this.ActivateTarget();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }


        // 根据读者证条码号，装入读者记录
        // parameters:
        //      edit    读者编辑控件。可以==null
        //      webbHtml    用于显示HTML的WebBrowser控件。可以==null
        //      webbXml   用于显示XML的WebBrowser控件。可以==null
        // return:
        //      0   cancelled
        internal int LoadRecord(ref string strBarcode,
            ReaderEditControl edit,
            WebExternalHost external_html,
            // WebBrowser webbHtml,
            WebBrowser webbXml)
        {
            string strError = "";
            int nRet = 0;

            if (edit != null
                && edit.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据证条码号重新装载内容? ",
"ActivateForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return 0;   // cancelled

            }

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在初始化浏览器组件 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在初始化浏览器组件 ...",
                "disableControl");

            if (edit != null)
                edit.Clear();
#if NO
            if (webbHtml != null)
            {
                Global.ClearHtmlPage(webbHtml,
                    Program.MainForm.DataDir);
            }
#endif
            if (external_html != null)
            {
                external_html.ClearHtmlPage();
            }

            if (webbXml != null)
            {
                Global.ClearHtmlPage(webbXml,
                    Program.MainForm.DataDir);
            }

            try
            {
                int nRedoCount = 0;
            REDO:
                looping.stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                long lRet = channel.GetReaderInfo(
                    looping.stop,
                    strBarcode,
                    "xml,html",
                    out string[] results,
                    out string strRecPath,
                    out byte[] baTimestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                    goto ERROR1;

                if (lRet > 1)
                {
                    // 如果重试后依然发生重复
                    if (nRedoCount > 0)
                    {
                        strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        goto ERROR1;    // 当出错处理
                    }
                    SelectPatronDialog dlg = new SelectPatronDialog();

                    dlg.Overflow = StringUtil.SplitList(strRecPath).Count < lRet;
                    nRet = dlg.Initial(
                        // Program.MainForm,
                        StringUtil.SplitList(strRecPath),
                        "请选择一个读者记录",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // TODO: 保存窗口内的尺寸状态
                    Program.MainForm.AppInfo.LinkFormState(dlg, "ActivateForm_SelectPatronDialog_state");
                    dlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃选择";
                        return 0;
                    }

                    // strBarcode = dlg.SelectedBarcode;
                    strBarcode = "@path:" + dlg.SelectedRecPath;   // 2015/11/16

                    nRedoCount++;
                    goto REDO;
                }

                if (results == null || results.Length < 2)
                {
                    strError = "返回的results不正常。";
                    goto ERROR1;
                }

                string strXml = "";
                strXml = results[0];
                string strHtml = results[1];

                if (edit != null)
                {
                    nRet = edit.SetData(
                        strXml,
                        strRecPath,
                        baTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                if (webbXml != null)
                {
                    /*
                    SetXmlToWebbrowser(webbXml,
                        strXml);
                     * */
                    Global.SetXmlToWebbrowser(webbXml,
                        Program.MainForm.DataDir,
                        "xml",
                        strXml);
                }

                // this.m_strSetAction = "change";

#if NO
                if (webbHtml != null)
                {
                    Global.SetHtmlString(webbHtml,
                            strHtml,
                            Program.MainForm.DataDir,
                            "activateform_html");
                }
#endif

                if (external_html != null)
                    external_html.SetHtmlString(strHtml, "activateform_html");
            }
            finally
            {
                looping.Dispose();
                /*
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }

            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this.textBox_oldBarcode.Enabled = bEnable;
                this.textBox_newBarcode.Enabled = bEnable;

                this.tabControl_old.Enabled = bEnable;
                this.tabControl_new.Enabled = bEnable;

                this.button_loadOldUserInfo.Enabled = bEnable;
                this.button_loadNewUserInfo.Enabled = bEnable;

                this.button_devolve.Enabled = bEnable;
                this.button_activate.Enabled = bEnable;

                this.toolStrip_new.Enabled = bEnable;
                this.toolStrip_old.Enabled = bEnable;
            }));
        }

        // 转移并激活目标证
        private void button_activate_Click(object sender, EventArgs e)
        {
            this.button_activate.Enabled = false;

            this.m_webExternalHost_new.StopPrevious();
            this.webBrowser_newReaderInfo.Stop();

            this.m_webExternalHost_old.StopPrevious();
            this.webBrowser_oldReaderInfo.Stop();

            this.commander.AddMessage(WM_ACTIVATE_TARGET);
        }

        // 转移并激活目标证
        void ActivateTarget()
        {
            string strError = "";
            int nRet = 0;

            // 把旧证的借阅信息转入新证
            nRet = DevolveReaderInfo(this.textBox_oldBarcode.Text,
                this.textBox_newBarcode.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新
            string strReaderBarcode = this.textBox_oldBarcode.Text;
            this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_old,
                this.m_webExternalHost_old,
                // this.webBrowser_oldReaderInfo,
                this.webBrowser_oldXml);
            strReaderBarcode = this.textBox_newBarcode.Text;
            this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_new,
                this.m_webExternalHost_new,
                // this.webBrowser_newReaderInfo,
                this.webBrowser_newXml);

            bool bZhuxiao = false;

            // 把旧证的状态修改为“注销”
            if (this.readerEditControl_old.State != "注销")
            {
                this.readerEditControl_old.State = "注销";

                // return:
                //      -1  error
                //      0   成功
                //      1   服务器端记录发生改变，未保存。注意重新保存记录
                nRet = SaveReaderInfo(this.readerEditControl_old,
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + "\r\n\r\n目标证没有来得及激活。";
                    goto ERROR1;
                }
                if (nRet == 1)
                {
                    // 发现服务器端记录发生改变，记录因此未保存。
                    // 建议为左右两个记录各增加一个保存按钮。单独可以保存。
                    MessageBox.Show(this, "源证信息修改后尚未保存，请按保存按钮保存之。");
                }
                else
                {
                    bZhuxiao = true;
                    // 刷新
                    string strTempReaderBarcode = this.readerEditControl_old.Barcode;
                    this.LoadRecord(ref strTempReaderBarcode,
                        null,
                        this.m_webExternalHost_old,
                        // this.webBrowser_oldReaderInfo,
                        this.webBrowser_oldXml);
                }

            }

            // 把新证的状态修改为可用
            if (this.readerEditControl_new.State != "")
            {
                this.readerEditControl_new.State = "";
                // return:
                //      -1  error
                //      0   成功
                //      1   服务器端记录发生改变，未保存。注意重新保存记录
                nRet = SaveReaderInfo(this.readerEditControl_new,
                    out strError);
                if (nRet == -1)
                {
                    if (bZhuxiao == true)
                        strError = strError + "\r\n\r\n源证已经注销。";
                    goto ERROR1;
                }
                if (nRet == 1)
                {
                    MessageBox.Show(this, "目标证信息修改后尚未保存，请按保存按钮保存之。");
                }
                else
                {
                    string strTempReaderBarcode = this.readerEditControl_new.Barcode;
                    this.LoadRecord(ref strTempReaderBarcode,
                        null,
                        this.m_webExternalHost_new,
                        // this.webBrowser_newReaderInfo,
                        this.webBrowser_newXml);
                }

            }


            MessageBox.Show(this, "转移并激活目标证操作完成");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存
        // return:
        //      -1  error
        //      0   成功
        //      1   服务器端记录发生改变，未保存。注意重新保存记录
        int SaveReaderInfo(ReaderEditControl edit,
            out string strError)
        {
            strError = "";

            if (edit.Barcode == "")
            {
                strError = "尚未输入证条码号";
                return -1;
            }

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存读者记录 " + edit.Barcode + " ...");
            _stop.BeginLoop();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在保存读者记录 " + edit.Barcode + " ...",
                "disableControl");

            try
            {
                string strNewXml = "";
                int nRet = edit.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = channel.SetReaderInfo(
                    looping.stop,
                    "change",
                    edit.RecPath,
                    strNewXml,
                    edit.OldRecord,
                    edit.Timestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            //Program.MainForm,
                            edit.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            edit.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = edit.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                return -1;
                            }
                            strError = "请注意重新保存记录";
                            return 1;
                        }
                    }

                    return -1;
                }

                /*
                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;
                 */

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    MessageBox.Show(this, strError);

                    if (channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        // 提醒重新装载?
                        MessageBox.Show(this, "请重新装载记录, 检查哪些字段内容修改被拒绝。");
                    }
                }
                else
                {
                    // 重新装载记录到编辑器
                    nRet = edit.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }


                strError = "保存成功";
                return 0;
            }
            finally
            {
                looping.Dispose();
                /*
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }
        }


        int DevolveReaderInfo(string strSourceReaderBarcode,
            string strTargetReaderBarcode,
            out string strError)
        {
            strError = "";

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在转移读者借阅信息 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在转移读者借阅信息 ...",
                "disableControl");

            try
            {
                long lRet = channel.DevolveReaderInfo(
                    looping.stop,
                    strSourceReaderBarcode,
                    strTargetReaderBarcode,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                this.EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }
        }

        // 转移
        private void button_devolve_Click(object sender, EventArgs e)
        {
            this.button_devolve.Enabled = false;

            this.m_webExternalHost_new.StopPrevious();
            this.webBrowser_newReaderInfo.Stop();

            this.m_webExternalHost_old.StopPrevious();
            this.webBrowser_oldReaderInfo.Stop();

            this.commander.AddMessage(WM_DEVOLVE);
        }

        void Devolve()
        {
            string strError = "";
            int nRet = 0;

            // 把源证的借阅信息转入目标证
            nRet = DevolveReaderInfo(this.textBox_oldBarcode.Text,
                this.textBox_newBarcode.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新
            string strReaderBarcode = this.textBox_oldBarcode.Text;
            this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_old,
                this.m_webExternalHost_old,
                // this.webBrowser_oldReaderInfo,
                this.webBrowser_oldXml);
            strReaderBarcode = this.textBox_newBarcode.Text;
            this.LoadRecord(ref strReaderBarcode,
                this.readerEditControl_new,
                this.m_webExternalHost_new,
                // this.webBrowser_newReaderInfo,
                this.webBrowser_newXml);

            MessageBox.Show(this, "转移完成");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        void SetXmlToWebbrowser(WebBrowser webbrowser,
            string strXml)
        {
            string strTargetFileName = MainForm.DataDir + "\\xml.xml";

            StreamWriter sw = new StreamWriter(strTargetFileName,
                false,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strXml);
            sw.Close();

            webbrowser.Navigate(strTargetFileName);
        }
#endif

        private void button_saveOld_Click(object sender, EventArgs e)
        {
        }

        void SaveOldRecord()
        {
            string strError = "";
            int nRet = 0;

            nRet = SaveReaderInfo(this.readerEditControl_old,
    out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                // 发现服务器端记录发生改变，记录因此未保存。
                goto ERROR1;
            }

            MessageBox.Show(this, "保存成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void button_saveNew_Click(object sender, EventArgs e)
        {
        }

        void SaveNewRecord()
        {
            string strError = "";
            int nRet = 0;

            nRet = SaveReaderInfo(this.readerEditControl_new,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                // 发现服务器端记录发生改变，记录因此未保存。
                goto ERROR1;
            }

            MessageBox.Show(this, "保存成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void readerEditControl_old_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            this.toolStripButton_old_save.Enabled = e.CurrentChanged;
        }

        private void readerEditControl_new_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            this.toolStripButton_new_save.Enabled = e.CurrentChanged;
        }

        private void ActivateForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        // 从源记录中复制除了条码号和记录路径以外的其他全部内容。
        private void button_copyFromOld_Click(object sender, EventArgs e)
        {

        }

        private void panel_old_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void panel_old_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strWhole = (String)e.Data.GetData("Text");

            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                strError = "连一行也不存在";
                goto ERROR1;
            }

            if (lines.Length > 1)
            {
                strError = "激活窗一次只允许拖入一个记录";
                goto ERROR1;
            }

            string strFirstLine = lines[0].Trim();

            // 取得recpath
            string strRecPath = "";
            int nRet = strFirstLine.IndexOf("\t");
            if (nRet == -1)
                strRecPath = strFirstLine;
            else
                strRecPath = strFirstLine.Substring(0, nRet).Trim();

            // 判断它是不是读者记录路径
            string strDbName = Global.GetDbName(strRecPath);

            if (Program.MainForm.IsReaderDbName(strDbName) == true)
            {
                string[] parts = strFirstLine.Split(new char[] { '\t' });
                string strReaderBarcode = "";
                if (parts.Length >= 2)
                    strReaderBarcode = parts[1].Trim();

                if (String.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    this.textBox_oldBarcode.Text = strReaderBarcode;
                    this.button_loadOldUserInfo_Click(this, null);
                }
            }
            else
            {
                strError = "记录路径 '" + strRecPath + "' 中的数据库名不是读者库名...";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void panel_new_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void panel_new_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strWhole = (String)e.Data.GetData("Text");

            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                strError = "连一行也不存在";
                goto ERROR1;
            }

            if (lines.Length > 1)
            {
                strError = "激活窗一次只允许拖入一个记录";
                goto ERROR1;
            }

            string strFirstLine = lines[0].Trim();

            // 取得recpath
            string strRecPath = "";
            int nRet = strFirstLine.IndexOf("\t");
            if (nRet == -1)
                strRecPath = strFirstLine;
            else
                strRecPath = strFirstLine.Substring(0, nRet).Trim();

            // 判断它是不是读者记录路径
            string strDbName = Global.GetDbName(strRecPath);

            if (Program.MainForm.IsReaderDbName(strDbName) == true)
            {
                string[] parts = strFirstLine.Split(new char[] { '\t' });
                string strReaderBarcode = "";
                if (parts.Length >= 2)
                    strReaderBarcode = parts[1].Trim();

                if (String.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    this.textBox_newBarcode.Text = strReaderBarcode;
                    this.button_loadNewUserInfo_Click(this, null);
                }
            }
            else
            {
                strError = "记录路径 '" + strRecPath + "' 中的数据库名不是读者库名...";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_old_save_Click(object sender, EventArgs e)
        {
            this.toolStrip_old.Enabled = false;

            this.m_webExternalHost_old.StopPrevious();
            this.webBrowser_oldReaderInfo.Stop();

            this.commander.AddMessage(WM_SAVE_OLD_RECORD);
        }

        private void toolStripButton_new_save_Click(object sender, EventArgs e)
        {
            this.toolStrip_new.Enabled = false;

            this.m_webExternalHost_new.StopPrevious();
            this.webBrowser_newReaderInfo.Stop();

            this.commander.AddMessage(WM_SAVE_NEW_RECORD);
        }

        private void toolStripButton_new_copyFromOld_Click(object sender, EventArgs e)
        {

            // TODO: 需要增加新的域
            this.readerEditControl_new.NameString = this.readerEditControl_old.NameString;
            this.readerEditControl_new.State = this.readerEditControl_old.State;
            this.readerEditControl_new.Comment = this.readerEditControl_old.Comment;
            this.readerEditControl_new.ReaderType = this.readerEditControl_old.ReaderType;
            this.readerEditControl_new.CreateDate = this.readerEditControl_old.CreateDate;
            this.readerEditControl_new.ExpireDate = this.readerEditControl_old.ExpireDate;
            this.readerEditControl_new.DateOfBirth = this.readerEditControl_old.DateOfBirth;
            this.readerEditControl_new.Gender = this.readerEditControl_old.Gender;
            this.readerEditControl_new.IdCardNumber = this.readerEditControl_old.IdCardNumber;
            this.readerEditControl_new.Department = this.readerEditControl_old.Department;
            this.readerEditControl_new.Address = this.readerEditControl_old.Address;
            this.readerEditControl_new.Tel = this.readerEditControl_old.Tel;
            this.readerEditControl_new.Email = this.readerEditControl_old.Email;

        }

        private void readerEditControl_old_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = Program.MainForm.GetReaderDbLibraryCode(e.DbName);
        }

        private void readerEditControl_new_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = Program.MainForm.GetReaderDbLibraryCode(e.DbName);
        }

    }
}