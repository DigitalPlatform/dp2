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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改册窗
    /// </summary>
    internal partial class QuickChangeEntityForm : MyForm
    {
        WebExternalHost m_webExternalHost_biblio = new WebExternalHost();

        string m_strRefID_1 = "";
        // public string m_strRefID_2 = "";
#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
#endif

        /// <summary>
        /// 当前正在处理的一条册记录所从属的书目记录路径
        /// </summary>
        public string BiblioRecPath = "";

        LoadActionType m_loadActionType = LoadActionType.LoadAndAutoChange;

        const int WM_SWITCH_FOCUS = API.WM_USER + 200;

        // 消息WM_SWITCH_FOCUS的wparam参数值
        const int ITEM_BARCODE = 0;
        const int CONTROL_STATE = 1;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QuickChangeEntityForm()
        {
            InitializeComponent();
        }

        string RefID_1
        {
            get
            {
                if (String.IsNullOrEmpty(this.m_strRefID_1) == true)
                    this.m_strRefID_1 = Guid.NewGuid().ToString();

                return this.m_strRefID_1;
            }
        }

        /*
        public string RefID_2
        {
            get
            {
                if (String.IsNullOrEmpty(this.m_strRefID_2) == true)
                    this.m_strRefID_2 = Guid.NewGuid().ToString();

                return this.m_strRefID_2;
            }
        }*/

        private void QuickChangeEntityForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.m_webExternalHost_biblio.Initial(this.MainForm, this.webBrowser_biblio);
            this.webBrowser_biblio.ObjectForScripting = this.m_webExternalHost_biblio;

            this.AcceptButton = this.button_loadBarcode;

            this.entityEditControl1.GetValueTable += new GetValueTableEventHandler(entityEditControl1_GetValueTable);
        }

        void entityEditControl1_GetValueTable(object sender, GetValueTableEventArgs e)
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

        private void QuickChangeEntityForm_FormClosing(object sender, FormClosingEventArgs e)
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

            if (this.entityEditControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "QuickChangeEntityForm",
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

        private void QuickChangeEntityForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Destroy();

#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            this.entityEditControl1.GetValueTable -= new GetValueTableEventHandler(entityEditControl1_GetValueTable);
        }

        // 
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册条码号，装入册记录和书目记录
        /// </summary>
        /// <param name="bEnableControls">是否在处理过程中禁止界面元素</param>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有找到
        ///      1   找到
        /// </returns>
        public int LoadRecord(
            bool bEnableControls,
            string strBarcode,
            out string strError)
        {
            strError = "";

            if (bEnableControls == true)
            {
                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();


                this.Update();
                this.MainForm.Update();
            }

            this.entityEditControl1.Clear();

            this.m_webExternalHost_biblio.StopPrevious();
            this.webBrowser_biblio.Stop();

            Global.ClearHtmlPage(this.webBrowser_biblio,
                this.MainForm.DataDir);

            this.textBox_message.Text = "";

            stop.SetMessage("正在装入册记录 " + strBarcode + " ...");


            try
            {
                string strItemText = "";
                string strBiblioText = "";

                string strItemRecPath = "";
                string strBiblioRecPath = "";

                byte[] item_timestamp = null;

                long lRet = Channel.GetItemInfo(
                    stop,
                    strBarcode,
                    "xml",
                    out strItemText,
                    out strItemRecPath,
                    out item_timestamp,
                    "html",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                    return 0;

                if (lRet > 1)
                {
                    strError = "册条码号 " + strBarcode + " 发现被下列多个册记录所使用: \r\n" + strItemRecPath + "\r\n\r\n这是一个严重错误，请求助于系统管理员尽快排除。";
                    goto ERROR1;
                }

                this.BiblioRecPath = strBiblioRecPath;

                int nRet = this.entityEditControl1.SetData(strItemText,
                    strItemRecPath,
                    item_timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                Debug.Assert(this.entityEditControl1.Changed == false, "");

                this.entityEditControl1.SetReadOnly("librarian");

#if NO
                Global.SetHtmlString(this.webBrowser_biblio,
                    strBiblioText,
                    this.MainForm.DataDir,
                    "quickchangeentityform_biblio");
#endif
                this.m_webExternalHost_biblio.SetHtmlString(strBiblioText,
                    "quickchangeentityform_biblio");

                this.textBox_message.Text = "册记录路径: " + strItemRecPath + " ；其从属的种(书目)记录路径: " + strBiblioRecPath;

            }
            finally
            {
                if (bEnableControls == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);
                }
            }

            return 1;
        ERROR1:
            strError = "装载册条码号为 " + strBarcode + "的记录发生错误: " + strError;
            // MessageBox.Show(this, strError);
            return -1;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_barcode.Enabled = bEnable;
            this.textBox_message.Enabled = bEnable;

            this.textBox_barcodeFile.Enabled = bEnable;
            this.textBox_outputBarcodes.Enabled = bEnable;

            this.button_loadBarcode.Enabled = bEnable;
            this.entityEditControl1.Enabled = bEnable;

            this.button_beginByBarcodeFile.Enabled = bEnable;
            this.button_changeParam.Enabled = bEnable;
            this.button_file_getBarcodeFilename.Enabled = bEnable;
            this.button_saveCurrentRecord.Enabled = bEnable;
            this.button_saveToBarcodeFile.Enabled = bEnable;

            this.button_getRecPathFileName.Enabled = bEnable;
            this.textBox_recPathFile.Enabled = bEnable;
            this.button_beginByRecPathFile.Enabled = bEnable;
        }

        private void button_loadBarcode_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            // Debug.Assert(false, "");

            // 先保存前一条
            if (this.entityEditControl1.Changed == true)
            {
                if (this.LoadActionType == LoadActionType.LoadOnly)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
        "当前有信息被修改后尚未保存。若此时装载新的信息，现有未保存信息将丢失。\r\n\r\n是否保存后再装入新的信息? (Yes 保存后装入; No 不保存但装入; Cancel 不保存，也放弃装入)",
        "QuickChangeEntityForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button3);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.No)
                        goto DOLOAD;
                }
                else
                {
                    // 在装入并自动修改的状态下，不必询问，直接保存后装入
                }


                nRet = DoSave(true, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            DOLOAD:

            nRet = LoadRecord(true, this.textBox_barcode.Text,
                out strError);
            if (nRet != 1)
            {
                MessageBox.Show(this, strError);
                goto SETFOCUS;
            }

            if (this.LoadActionType == LoadActionType.LoadAndAutoChange)
            {
                // 自动修改
                AutoChangeData();
            }

            this.textBox_outputBarcodes.Text += this.textBox_barcode.Text + "\r\n";

            SETFOCUS:
            // 焦点定位
            string strFocusAction = this.MainForm.AppInfo.GetString(
"change_param",
"focusAction",
"册条码号，并全选");


            if (strFocusAction == "册条码号，并全选")
            {
                SwitchFocus(0);
            }
            else if (strFocusAction == "册信息编辑器")
            {
                SwitchFocus(1);
            }
        }

        // return:
        //      0   没有实质性改变
        //      1   有实质性改变
        int AutoChangeData()
        {
            bool bChanged = false;
            // 装载值
            /*
            string strState = this.MainForm.AppInfo.GetString(
                "change_param",
                "state",
                "<不改变>");

            if (strState != "<不改变>")
                this.entityEditControl1.State = strState;
            */


            // state
            string strStateAction = this.MainForm.AppInfo.GetString(
                "change_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = this.entityEditControl1.State;

                if (strStateAction == "<增、减>")
                {
                    string strAdd = this.MainForm.AppInfo.GetString(
                        "change_param",
                        "state_add",
                        "");
                    string strRemove = this.MainForm.AppInfo.GetString(
                        "change_param",
                        "state_remove",
                        "");

                    string strOldState = strState;

                    if (String.IsNullOrEmpty(strAdd) == false)
                        StringUtil.SetInList(ref strState, strAdd, true);
                    if (String.IsNullOrEmpty(strRemove) == false)
                        StringUtil.SetInList(ref strState, strRemove, false);

                    if (strOldState != strState)
                    {
                        this.entityEditControl1.State = strState;
                        bChanged = true;
                    }
                }
                else
                {
                    if (strStateAction != strState)
                    {
                        this.entityEditControl1.State = strStateAction;
                        bChanged = true;
                    }
                }
            }

            string strLocation = this.MainForm.AppInfo.GetString(
                "change_param",
                "location",
                "<不改变>");

            if (strLocation != "<不改变>")
            {
                if (this.entityEditControl1.LocationString != strLocation)
                {
                    this.entityEditControl1.LocationString = strLocation;
                    bChanged = true;
                }
            }


            string strBookType = this.MainForm.AppInfo.GetString(
                "change_param",
                "bookType",
                "<不改变>");

            if (strBookType != "<不改变>")
            {
                if (this.entityEditControl1.BookType != strBookType)
                {
                    this.entityEditControl1.BookType = strBookType;
                    bChanged = true;
                }
            }

            string strBatchNo = this.MainForm.AppInfo.GetString(
                "change_param",
                "batchNo",
                "<不改变>");
            if (strBatchNo != "<不改变>")
            {
                if (this.entityEditControl1.BatchNo != strBatchNo)
                {
                    this.entityEditControl1.BatchNo = strBatchNo;
                    bChanged = true;
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        /// <summary>
        /// 出现动作参数对话框，收集输入信息
        /// </summary>
        /// <returns>true: 确定; false: 放弃</returns>
        public bool SetChangeParameters()
        {
            ChangeEntityActionDialog dlg = new ChangeEntityActionDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.GetValueTable += new GetValueTableEventHandler(entityEditControl1_GetValueTable);
            if (String.IsNullOrEmpty(this.entityEditControl1.RecPath) == true)
            {
                dlg.RefDbName = "";
            }
            else
            {
                dlg.RefDbName = Global.GetDbName(this.entityEditControl1.RecPath);
            }
            dlg.MainForm = this.MainForm;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dlg, "quickchangeentityform_changeparamdialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.OK)
                return true;
            return false;
        }

        // 修改参数
        private void button_changeParam_Click(object sender, EventArgs e)
        {
            SetChangeParameters();
        }


        // 保存当前实体记录
        // 保存以后，红色的字符将变为黑色
        private void button_saveCurrentRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = DoSave(true, out strError);
            if (nRet != 1)
                MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        int DoSave(bool bEnableControls,
            out string strError)
        {
            strError = "";

            if (bEnableControls == true)
                EnableControls(false);

            try
            {
                if (this.entityEditControl1.Changed == false)
                {
                    strError = "没有修改过的信息需要保存";
                    goto ERROR1;
                }

                EntityInfo[] entities = null;
                EntityInfo[] errorinfos = null;

                // 构造需要提交的实体信息数组
                int nRet = BuildSaveEntities(
                    out entities,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (entities == null || entities.Length == 0)
                    return 0; // 没有必要保存


                nRet = SaveEntityRecords(
                    bEnableControls,
                    this.BiblioRecPath,
                    entities,
                    out errorinfos,
                    out strError);

                this.entityEditControl1.Changed = false;    // 2007/4/4

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                if (RefreshOperResult(errorinfos) == true)
                {
                    if (nRet != -1)
                        return -1;
                }

                if (nRet == -1)
                {
                    goto ERROR1;
                }

                return 1;
            ERROR1:
                strError = "保存条码为 "+this.entityEditControl1.Barcode+" 的册记录时出错: " + strError;
                return -1;
            }
            finally
            {
                if (bEnableControls == true)
                    EnableControls(true);
            }
        }

        // 构造用于保存的实体信息数组
        int BuildSaveEntities(
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            entities = new EntityInfo[1];

            EntityInfo info = new EntityInfo();

            string strXml = "";
            nRet = this.entityEditControl1.GetData(
                true,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;


            info.Action = "change";
            info.OldRecPath = this.entityEditControl1.RecPath;  //  2007/6/2
            info.NewRecPath = this.entityEditControl1.RecPath;

            info.NewRecord = strXml;
            info.NewTimestamp = null;

            info.OldRecord = this.entityEditControl1.OldRecord;
            info.OldTimestamp = this.entityEditControl1.Timestamp;

            // info.RefID = this.RefID_1; // 2008/3/3 // 这一句难以理解其意思，难道是反复使用同一个 refid?

            // 2013/6/23
            if (string.IsNullOrEmpty(info.RefID) == true)
                info.RefID = Guid.NewGuid().ToString();

            entities[0] = info;

            return 0;
        }

        // 保存实体记录
        // 不负责刷新界面和报错
        int SaveEntityRecords(
            bool bEnableControls,
            string strBiblioRecPath,
            EntityInfo[] entities,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            if (bEnableControls == true)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在保存册信息 ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();
            }


            try
            {
                long lRet = Channel.SetEntities(
                    stop,
                    strBiblioRecPath,
                    entities,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                if (bEnableControls == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”BookItem事项（内存和视觉上）
        // return:
        //      false   没有警告
        //      true    出现警告
        bool RefreshOperResult(EntityInfo[] errorinfos)
        {
            int nRet = 0;

            string strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            for (int i = 0; i < errorinfos.Length; i++)
            {
                XmlDocument dom = new XmlDocument();

                string strNewXml = errorinfos[i].NewRecord;
                string strOldXml = errorinfos[i].OldRecord;


                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                {

                    if (errorinfos[i].Action == "change")
                    {
                        string strError = "";
                        nRet = this.entityEditControl1.SetData(
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);

                        this.entityEditControl1.SetReadOnly("librarian");
                    }

                    continue;
                }

                // 报错处理

                // TimeStampMismatch报错的时候, 实际上OldRecord中返回了当前库中该位置的记录, OldTimeStamp中是对应的时间戳
                // 需要出现参考行, 便于操作者对比处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.TimestampMismatch)
                {
                    this.entityEditControl1.OldRecord = errorinfos[i].OldRecord;

                    // 是否需要用界面命令明确刷新一次, 才行?
                    // 因为不刷新, 可以阻止鲁莽地重新提交保存
                    this.entityEditControl1.Timestamp = errorinfos[i].OldTimestamp;    // 这样就使得再次保存, 没有了障碍
                }

                strWarning += "在提交保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改后重新提交保存";
                MessageBox.Show(this, strWarning);
                return true;
            }

            return false;
        }

        private void ToolStripMenuItem_loadOnly_Click(object sender, EventArgs e)
        {
            this.LoadActionType = LoadActionType.LoadOnly;
        }

        private void ToolStripMenuItem_loadAndAutoChange_Click(object sender, EventArgs e)
        {
            this.LoadActionType = LoadActionType.LoadAndAutoChange;
        }

        /// <summary>
        /// 装载类型
        /// </summary>
        public LoadActionType LoadActionType
        {
            get
            {
                return this.m_loadActionType;
            }
            set
            {
                this.m_loadActionType = value;

                this.ToolStripMenuItem_loadOnly.Checked = false;
                this.ToolStripMenuItem_loadAndAutoChange.Checked = false;

                if (m_loadActionType == LoadActionType.LoadOnly)
                {
                    this.button_loadBarcode.Text = "只装入(不修改)";
                    this.ToolStripMenuItem_loadOnly.Checked = true;
                }
                if (m_loadActionType == LoadActionType.LoadAndAutoChange)
                {
                    this.button_loadBarcode.Text = "装入并自动修改";
                    this.ToolStripMenuItem_loadAndAutoChange.Checked = true;
                }

            }
        }

        private void QuickChangeEntityForm_Activated(object sender, EventArgs e)
        {
            // this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        // 探测文本文件的行数
        // parameters:
        //      bIncludeBlankLine   是包括空行
        // return:
        //      -1  出错
        //      >=0 行数
        static long GetTextLineCount(string strFilename,
            bool bIncludeBlankLine)
        {
            try
            {
                long lCount = 0;
                StreamReader sr = null;
                sr = new StreamReader(strFilename);

                try
                {

                    // 逐行读入文件内容
                    for (; ; )
                    {
                        string strLine = "";
                        strLine = sr.ReadLine();
                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true
                            && bIncludeBlankLine == false)
                            continue;

                        lCount++;
                    }
                    return lCount;

                }
                finally
                {
                    sr.Close();
                }
            }
            catch
            {
                return -1;
            }
        }

        // return:
        //      -1  出错
        //      0   放弃处理
        //      >=1 处理的条数
        /// <summary>
        /// 根据册条码号文件进行处理
        /// </summary>
        /// <param name="strFilename">册条码号文件名</param>
        /// <returns>
        ///      -1  出错
        ///      0   放弃处理
        ///      >=1 处理的条数
        /// </returns>
        public int DoBarcodeFile(string strFilename)
        {
            string strError = "";

            this.tabControl_input.SelectedTab = this.tabPage_barcodeFile;

            this.textBox_barcodeFile.Text = strFilename;

            // parameters:
            //      strFileType barcode/recpath
            // return:
            //      -1  出错
            //      0   放弃
            //      >=1 处理的条数
            int nRet = ProcessFile(
                    "barcode",
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // return:
        //      -1  出错
        //      0   放弃处理
        //      >=1 处理的条数
        /// <summary>
        /// 根据记录路径文件进行处理
        /// </summary>
        /// <param name="strFilename">记录路径文件名</param>
        /// <returns>
        ///      -1  出错
        ///      0   放弃处理
        ///      >=1 处理的条数
        /// </returns>
        public int DoRecPathFile(string strFilename)
        {
            string strError = "";

            this.tabControl_input.SelectedTab = this.tabPage_recPathFile;

            this.textBox_recPathFile.Text = strFilename;

            // parameters:
            //      strFileType barcode/recpath
            // return:
            //      -1  出错
            //      0   放弃
            //      >=1 处理的条数
            int nRet = ProcessFile(
                    "recpath",
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // parameters:
        //      strFileType barcode/recpath
        // return:
        //      -1  出错
        //      0   放弃
        //      >=1 处理的条数
        int ProcessFile(
            string strFileType,
            out string strError)
        {
            strError = "";

            string strFilename = "";
            if (strFileType == "barcode")
            {
                if (string.IsNullOrEmpty(this.textBox_barcodeFile.Text) == true)
                {
                    OpenFileDialog dlg = new OpenFileDialog();

                    dlg.FileName = this.textBox_barcodeFile.Text;
                    dlg.Title = "请指定要打开的条码号文件名";
                    dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return 0;

                    this.textBox_barcodeFile.Text = dlg.FileName;
                }

                strFilename = this.textBox_barcodeFile.Text;
            }
            else if (strFileType == "recpath")
            {
                if (string.IsNullOrEmpty(this.textBox_recPathFile.Text) == true)
                {
                    OpenFileDialog dlg = new OpenFileDialog();

                    dlg.FileName = this.textBox_recPathFile.Text;
                    dlg.Title = "请指定要打开的记录路径文件名";
                    dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return 0;

                    this.textBox_recPathFile.Text = dlg.FileName;
                }

                strFilename = this.textBox_recPathFile.Text;
            }
            else
            {
                strError = "未知的 strFileType '" + strFilename + "'";
                return -1;
            }

            // 探测文本文件的行数
            // parameters:
            //      bIncludeBlankLine   是包括空行
            // return:
            //      -1  出错
            //      >=0 行数
            long lLineCount = GetTextLineCount(strFilename,
                false);

            if (lLineCount != -1)
                stop.SetProgressRange(0, lLineCount);

            int nCurrentLine = 0;
            StreamReader sr = null;
            try
            {
                this.textBox_outputBarcodes.Text = "";

                // 打开文件

                sr = new StreamReader(strFilename);

                EnableControls(false);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                this.MainForm.Update();

                try
                {
                    int nRet = 0;

                    // 逐行读入文件内容
                    for (; ; )
                    {
                        Application.DoEvents();
                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断1";
                                return -1;
                            }
                        }


                        string strLine = "";
                        strLine = sr.ReadLine();
                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strFileType == "barcode")
                            stop.SetMessage("正在处理册条码号 " + strLine + " 对应的记录...");
                        else
                            stop.SetMessage("正在处理记录路径 " + strLine + " 对应的记录...");
                        stop.SetProgressValue(nCurrentLine);

                        nCurrentLine++;

                        // 先保存前一条
                        if (this.entityEditControl1.Changed == true)
                        {
                            nRet = DoSave(false,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(this, strError);
                        }
                        // DOLOAD:

                        nRet = LoadRecord(false,
                            strFileType == "barcode" ? strLine : "@path:" + strLine,
                            out strError);
                        if (nRet != 1)
                        {
                            this.textBox_outputBarcodes.Text += "# " + strLine + " " + strError + "\r\n";
                            continue;
                        }

                        // 自动修改
                        // return:
                        //      0   没有实质性改变
                        //      1   有实质性改变
                        AutoChangeData();

                        if (this.entityEditControl1.Changed == true)
                        {
                            nRet = DoSave(false,
                                out strError);
                            if (nRet == -1)
                            {
                                this.textBox_outputBarcodes.Text += "# " + strLine + " " + strError + "\r\n";
                                continue;
                            }

                            if (nRet != -1)
                            {
                                this.textBox_outputBarcodes.Text += strLine + "\r\n";
                            }
                        }
                        else
                        {
                            this.textBox_outputBarcodes.Text += "# " + strLine + "\r\n";
                        }
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();

                    EnableControls(true);
                }

                return nCurrentLine;
            }
            catch (Exception ex)
            {
                strError = "QuickChangeEntityForm ProcessFile() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        // 根据文件自动进行修改
        private void button_beginByBarcodeFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // parameters:
            //      strFileType barcode/recpath
            // return:
            //      -1  出错
            //      0   放弃
            //      >=1 处理的条数
            int nRet = ProcessFile(
                    "barcode",
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "处理完成。共处理记录 " + nRet.ToString() + " 条");

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存已处理条码号到条码号文件
        private void button_saveToBarcodeFile_Click(object sender, EventArgs e)
        {
            // 询问文件全路径
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的(条码号或路径)输出文件名";
            dlg.OverwritePrompt = true;
            dlg.CreatePrompt = false;
            // dlg.FileName = this.LocalPath;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "输出文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (StreamWriter sw = new StreamWriter(dlg.FileName))
            {
                sw.Write(this.textBox_outputBarcodes.Text);
            }
        }

        void SwitchFocus(int target)
        {
            API.PostMessage(this.Handle, WM_SWITCH_FOCUS,
                target, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SWITCH_FOCUS:
                    {
                        if ((int)m.WParam == ITEM_BARCODE)
                        {
                            this.textBox_barcode.SelectAll();
                            this.textBox_barcode.Focus();
                        }

                        if ((int)m.WParam == CONTROL_STATE)
                        {
                            this.entityEditControl1.FocusState(false);
                        }

                        return;
                    }
                // break;
            }
            base.DefWndProc(ref m);
        }

        private void button_file_getBarcodeFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = this.textBox_barcodeFile.Text;
            dlg.Title = "请指定要打开的条码号文件名";
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_barcodeFile.Text = dlg.FileName;
        }

        private void button_getRecPathFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = this.textBox_recPathFile.Text;
            dlg.Title = "请指定要打开的记录路径文件名";
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_recPathFile.Text = dlg.FileName;
        }

        private void button_beginByRecPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // parameters:
            //      strFileType barcode/recpath
            // return:
            //      -1  出错
            //      0   放弃
            //      >=1 处理的条数
            int nRet = ProcessFile(
                    "recpath",
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "处理完成。共处理记录 " + nRet.ToString() + " 条");

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }
    }

    /// <summary>
    /// 装载类型
    /// </summary>
    public enum LoadActionType
    {
        /// <summary>
        /// 只装载(不修改)
        /// </summary>
        LoadOnly = 0,   // 只装载(不修改)
        /// <summary>
        /// 装载并且自动修改
        /// </summary>
        LoadAndAutoChange = 1, // 装载并且自动修改
    }
}