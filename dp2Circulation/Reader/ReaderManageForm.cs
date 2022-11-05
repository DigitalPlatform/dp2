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
    /// 停借窗
    /// </summary>
    public partial class ReaderManageForm : MyForm
    {
        Commander commander = null;

        const int WM_LOAD_RECORD = API.WM_USER + 200;
        const int WM_SAVE_RECORD = API.WM_USER + 201;

        WebExternalHost m_webExternalHost = new WebExternalHost();

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        bool m_bChanged = false;

        string RecPath = "";    // 读者记录路径
        byte [] Timestamp = null;
        string OldRecord = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderManageForm()
        {
            this.UseLooping = true; // 2022/11/3

            InitializeComponent();
        }

        private void ReaderManageForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            this.GetValueTable += new GetValueTableEventHandler(ReaderManageForm_GetValueTable);

            // webbrowser
            this.m_webExternalHost.Initial(// Program.MainForm, 
                this.webBrowser_normalInfo);
            this.webBrowser_normalInfo.ObjectForScripting = this.m_webExternalHost;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost.ChannelInUse;
        }

        void ReaderManageForm_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out string[] values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void ReaderManageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "ReaderManageForm",
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

        private void ReaderManageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();

            this.GetValueTable -= new GetValueTableEventHandler(ReaderManageForm_GetValueTable);
        }

        /// <summary>
        /// 当前读者证条码号
        /// </summary>
        public string ReaderBarcode
        {
            get
            {
                return this.textBox_readerBarcode.Text;
            }
            set
            {
                this.textBox_readerBarcode.Text = value;
            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        // 防止重入 2009/7/19
        int m_nInDropDown = 0;

        private void comboBox_operation_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/7/19
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {

                ComboBox combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    if (String.IsNullOrEmpty(this.RecPath) == false)
                        e1.DbName = Global.GetDbName(this.RecPath);

                    e1.TableName = "readerState";

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            if (this.textBox_readerBarcode.Text == "")
            {
                MessageBox.Show(this, "尚未指定读者证条码号");
                return;
            }

            this.button_load.Enabled = false;

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_normalInfo.Stop();

            this.commander.AddMessage(WM_LOAD_RECORD);
        }

        /// <summary>
        /// 根据读者证条码号，装入读者记录
        /// </summary>
        /// <param name="strBarcode">读者证条码号</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadRecord(string strBarcode)
        {
            int nRet = this.LoadRecord(ref strBarcode);
            if (this.ReaderBarcode != strBarcode)
                this.ReaderBarcode = strBarcode;
            return nRet;
        }

        // 根据读者证条码号，装入读者记录
        // return:
        //      0   cancelled
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strBarcode">读者证条码号</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadRecord(ref string strBarcode)
        {
            string strError = "";
            int nRet = 0;

            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据证条码号重新装载内容? ",
"ReaderManageForm",
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
                null,
                "disableControl");

            this.ClearOperationInfo();
            try
            {
                byte[] baTimestamp = null;
                string strRecPath = "";

                int nRedoCount = 0;
            REDO:
                looping.stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                long lRet = channel.GetReaderInfo(
                    looping.stop,
                    strBarcode,
                    "xml,html",
                    out string[] results,
                    out strRecPath,
                    out baTimestamp,
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
                    Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderManageForm_SelectPatronDialog_state");
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

                this.ReaderBarcode = strBarcode;

                this.RecPath = strRecPath;

                this.Timestamp = baTimestamp;

                if (results == null || results.Length < 2)
                {
                    strError = "返回的results不正常。";
                    goto ERROR1;
                }
                string strXml = "";
                string strHtml = "";

                strXml = results[0];
                strHtml = results[1];

                // 保存刚获得的记录
                this.OldRecord = strXml;

                Global.SetXmlToWebbrowser(this.webBrowser_xml,
                    Program.MainForm.DataDir,
                    "xml",
                    strXml);

                this.m_webExternalHost.SetHtmlString(strHtml,
                    "readermanageform_reader");

                nRet = LoadOperationInfo(out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.Changed = false;
                return 1;
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
                this.textBox_readerBarcode.Enabled = bEnable;
                this.textBox_operator.Enabled = bEnable;
                this.textBox_comment.Enabled = bEnable;

                this.comboBox_operation.Enabled = bEnable;
                this.tabControl_readerInfo.Enabled = bEnable;

                // 2008/10/28
                this.button_save.Enabled = bEnable;
                this.button_load.Enabled = bEnable;
            }));
        }

        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_load;
            Program.MainForm.EnterPatronIdEdit(InputType.PQR);
        }

        private void textBox_readerBarcode_Leave(object sender, EventArgs e)
        {
            Program.MainForm.LeavePatronIdEdit();
        }

        private void textBox_comment_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_save;
        }

        // 从XML记录中读出操作信息
        int LoadOperationInfo(out string strError)
        {
            strError = "";
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(this.OldRecord);
            }
            catch (Exception ex)
            {
                strError = "装载XML进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            this.comboBox_operation.Text = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            this.textBox_comment.Text = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            this.textBox_operator.Text = Program.MainForm.AppInfo.GetString(
                    "default_account",
                    "username",
                    "");
            return 0;
        }

        int BuildXml(out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(this.OldRecord);
            }
            catch (Exception ex)
            {
                strError = "装载XML进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode node = DomUtil.SetElementText(dom.DocumentElement,
                "state",
                this.comboBox_operation.Text);

            DomUtil.SetAttr(node, "operator", this.textBox_operator.Text);
            DomUtil.SetAttr(node, "time", DateTimeUtil.Rfc1123DateTimeString(DateTime.UtcNow));

            DomUtil.SetElementText(dom.DocumentElement,
                "comment",
                this.textBox_comment.Text);

            strXml = dom.OuterXml;

            return 0;
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            this.button_save.Enabled = false;

            this.m_webExternalHost.StopPrevious();

            this.commander.AddMessage(WM_SAVE_RECORD);
        }

        void SaveRecord()
        {
            string strError = "";

            if (this.ReaderBarcode == "")
            {
                strError = "尚未装载读者记录，缺乏证条码号";
                goto ERROR1;
            }

            string strXml = "";
            int nRet = BuildXml(out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存读者记录 " + this.ReaderBarcode + " ...");
            _stop.BeginLoop();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在保存读者记录 " + this.ReaderBarcode + " ...",
                "disableControl");
            try
            {
                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                // changestate操作需要"setreaderinfo"和"changereaderstate"之一权限。
                long lRet = channel.SetReaderInfo(
                    looping.stop,
                    "changestate",   // "change",
                    this.RecPath,
                    strXml,
                    this.OldRecord,
                    this.Timestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out ErrorCodeValue kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            //Program.MainForm,
                            this.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strXml,
                            this.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            this.OldRecord = dlg.UnsavedXml;
                            this.RecPath = dlg.RecPath;
                            this.Timestamp = dlg.UnsavedTimestamp;

                            nRet = LoadOperationInfo(out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意重新保存记录");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;

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
                    /*
                    this.OldRecord = strSavedXml;
                    this.RecPath = strSavedPath;
                    this.Timestamp = baNewTimestamp;

                    int nRet = LoadOperationInfo(out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strSavedXml);
                     * */

                }

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

            MessageBox.Show(this, "保存成功");
            this.Changed = false;

            // 重新装载记录到编辑器
            string strReaderBarcode = this.ReaderBarcode;
            this.LoadRecord(ref strReaderBarcode);
            if (this.ReaderBarcode != strReaderBarcode)
                this.ReaderBarcode = strReaderBarcode;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void ClearOperationInfo()
        {
            this.textBox_readerBarcode.Text = "";
            this.comboBox_operation.Text = "";
            this.textBox_comment.Text = "";

            Global.ClearHtmlPage(this.webBrowser_normalInfo,
    Program.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_xml,
    Program.MainForm.DataDir);
            // this.webBrowser_xml.Navigate("about:blank");    // ??
        }

        private void ReaderManageForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOAD_RECORD:
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            string strReaderBarcode = this.textBox_readerBarcode.Text;
                            this.LoadRecord(ref strReaderBarcode);
                            if (this.textBox_readerBarcode.Text != strReaderBarcode)
                                this.textBox_readerBarcode.Text = strReaderBarcode;
                        }
                    return;
                case WM_SAVE_RECORD:
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            SaveRecord();
                        }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void ReaderManageForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void ReaderManageForm_DragDrop(object sender, DragEventArgs e)
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
                strError = "停借窗只允许拖入一个记录";
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
                    this.textBox_readerBarcode.Text = strReaderBarcode;
                    this.button_load_Click(this, null);
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

        private void comboBox_operation_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_operation.Invalidate();
        }
    }
}