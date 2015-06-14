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
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改书目窗
    /// </summary>
    internal partial class QuickChangeBiblioForm : MyForm
    {
#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
#endif


        public QuickChangeBiblioForm()
        {
            InitializeComponent();
        }

        private void QuickChangeBiblioForm_Load(object sender, EventArgs e)
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
        }

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        private void QuickChangeBiblioForm_FormClosing(object sender, FormClosingEventArgs e)
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
        }

        private void QuickChangeBiblioForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif


            // 长操作在进行中？
        }

        // return:
        //      -1  出错
        //      0   放弃处理
        //      1   正常结束
        public int DoRecPathLines()
        {
            this.tabControl_input.SelectedTab = this.tabPage_paths;

            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("");
            stop.BeginLoop();
            this.Update();
            this.MainForm.Update();

            try
            {
                return DoTextLines();
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

        }

        public void DoRecPathFile(string strFileName)
        {
            this.tabControl_input.SelectedTab = this.tabPage_recpathFile;
            this.textBox_recpathFile.Text = strFileName;

            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("");
            stop.BeginLoop();
            this.Update();
            this.MainForm.Update();
            try
            {
                DoFileName();
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }
        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("");
            stop.BeginLoop();
            this.Update();
            this.MainForm.Update();

            try
            {
                if (this.tabControl_input.SelectedTab == this.tabPage_paths)
                {
                    DoTextLines();
                }
                else if (this.tabControl_input.SelectedTab == this.tabPage_recpathFile)
                {
                    DoFileName();
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }
        }

        public string RecPathLines
        {
            get
            {
                return this.textBox_paths.Text;
            }
            set
            {
                this.textBox_paths.Text = value;
            }
        }

        public string RecPathFileName
        {
            get
            {
                return this.textBox_recpathFile.Text;
            }
            set
            {
                this.textBox_recpathFile.Text = value;
            }
        }

        // return:
        //      -1  出错
        //      0   放弃处理
        //      1   正常结束
        int DoTextLines()
        {
            string strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(this.textBox_paths.Text) == true)
            {
                strError = "尚未指定任何路径";
                goto ERROR1;
            }

            // TODO: MessageBox提示将要进行的修改动作。并警告不能复原

            // TODO: 检查修改动作，警告那种什么都不修改的情况
            string strInfo = GetSummary();
            if (String.IsNullOrEmpty(strInfo) == true)
            {
                DialogResult result = MessageBox.Show(this,
    "当前没有任何修改动作。确实要启动处理?",
    "dp2Circulation",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return 0;
            }
            else
            {
                DialogResult result = MessageBox.Show(this,
"即将进行下述修改动作：\r\n---"+strInfo+"\r\n\r\n开始处理?",
"dp2Circulation",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return 0;
            }

            int nCount = 0; // 总共处理多少条
            int nChangedCount = 0;  // 发生修改的有多少条

            DateTime now = DateTime.Now;

            stop.SetProgressRange(0, this.textBox_paths.Lines.Length);

            for (int i = 0; i < this.textBox_paths.Lines.Length; i++)
            {
                Application.DoEvents();
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }
                }

                string strLine = this.textBox_paths.Lines[i].Trim();
                nRet = strLine.IndexOfAny(new char[] {' ','\t'});
                if (nRet != -1)
                {
                    strLine = strLine.Substring(0, nRet).Trim();
                }

                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                nRet = ChangeOneRecord(strLine,
                    now,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nCount++;
                if (nRet == 1)
                    nChangedCount++;
                stop.SetProgressValue(i + 1);
            }

            MessageBox.Show(this, "处理完毕。共处理记录 " + nCount.ToString() + " 条，实际发生修改 " + nChangedCount.ToString() + " 条");
            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        void DoFileName()
        {
            string strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(this.textBox_recpathFile.Text) == true)
            {
                strError = "尚未指定记录路径文件名";
                goto ERROR1;
            }

            // TODO: MessageBox提示将要进行的修改动作。并警告不能复原

            // TODO: 检查修改动作，警告那种什么都不修改的情况
            string strInfo = GetSummary();
            if (String.IsNullOrEmpty(strInfo) == true)
            {
                DialogResult result = MessageBox.Show(this,
    "当前没有任何修改动作。确实要启动处理?",
    "dp2Circulation",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return;
            }
            else
            {
                DialogResult result = MessageBox.Show(this,
"即将进行下述修改动作：\r\n---" + strInfo + "\r\n\r\n开始处理?",
"dp2Circulation",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
            }

            int nCount = 0; // 总共处理多少条
            int nChangedCount = 0;  // 发生修改的有多少条

            using (StreamReader sr = new StreamReader(this.textBox_recpathFile.Text))
            {

                DateTime now = DateTime.Now;

                stop.SetProgressRange(0, sr.BaseStream.Length);

                for (; ; )
                {
                    Application.DoEvents();
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断2";
                            goto ERROR1;
                        }
                    }

                    string strLine = "";
                    strLine = sr.ReadLine();
                    if (strLine == null)
                        break;

                    strLine = strLine.Trim();
                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;

                    nRet = strLine.IndexOfAny(new char[] { ' ', '\t' });
                    if (nRet != -1)
                    {
                        strLine = strLine.Substring(0, nRet).Trim();
                    }

                    if (String.IsNullOrEmpty(strLine) == true)
                        continue;
                    // return:
                    //      -1  出错
                    //      0   未发生改变
                    //      1   发生了改变
                    nRet = ChangeOneRecord(strLine,
                        now,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nCount++;
                    if (nRet == 1)
                        nChangedCount++;
                    stop.SetProgressValue(sr.BaseStream.Position);
                }
            }

            MessageBox.Show(this, "处理完毕。共处理记录 "+nCount.ToString()+" 条，实际发生修改 "+nChangedCount.ToString()+" 条");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_paths.Enabled = bEnable;
            this.textBox_recpathFile.Enabled = bEnable;

            this.button_begin.Enabled = bEnable;
            this.button_changeParam.Enabled = bEnable;
            this.button_file_getRecpathFilename.Enabled = bEnable;
        }

        // return:
        //      -1  出错
        //      0   未发生改变
        //      1   发生了改变
        int ChangeOneRecord(string strBiblioRecPath,
            DateTime now,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            stop.SetMessage("正在处理 " + strBiblioRecPath + " ...");

            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            byte[] timestamp = null;
            long lRet = Channel.GetBiblioInfos(
                stop,
                strBiblioRecPath,
                    "",
                formats,
                out results,
                out timestamp,
                out strError);
            if (lRet == 0)
            {
                return 0;   // not found
            }
            if (lRet == -1)
                return -1;
            if (results.Length == 0)
            {
                strError = "results length error";
                return -1;
            }
            string strXml = results[0];

            XmlDocument domOrigin = new XmlDocument();

            try
            {
                domOrigin.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM时发生错误: " + ex.Message;
                return -1;
            }


            string strMARC = "";
            string strMarcSyntax = "";
            string strOutMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                strMarcSyntax,
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            // 修改
            // return:
            //      -1  出错
            //      0   未发生改变
            //      1   发生了改变
            nRet = ModifyField998(ref strMARC,
                now,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            // 转换回xml格式
            XmlDocument domMarc = null;
            nRet = MarcUtil.Marc2Xml(strMARC,
                strOutMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 合并<dprms:file>元素
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = domOrigin.DocumentElement.SelectNodes("//dprms:file", nsmgr);

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode new_node = domMarc.CreateElement("dprms",
                    "file",
                    DpNs.dprms);
                domMarc.DocumentElement.AppendChild(new_node);
                DomUtil.SetElementOuterXml(new_node, nodes[i].OuterXml);
            }

            // 保存
            byte[] baNewTimestamp = null;
            string strOutputPath = "";
            lRet = Channel.SetBiblioInfo(
    stop,
    "change",
    strBiblioRecPath,
    "xml",
    domMarc.DocumentElement.OuterXml,
    timestamp,
    "",
    out strOutputPath,
    out baNewTimestamp,
    out strError);
            if (lRet == -1)
            {
                strError = "保存书目记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                return -1;
            }

            return 1;
        }

        // 获得描述修改情况的文字
        string GetSummary()
        {
            string strResult = "";
            int nCount = 0;

            // state
            string strStateAction = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                if (strStateAction == "<增、减>")
                {
                    string strAdd = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state_add",
                "");
                    string strRemove = this.MainForm.AppInfo.GetString(
            "change_biblio_param",
            "state_remove",
            "");
                    if (String.IsNullOrEmpty(strAdd) == false)
                    {
                        strResult += "\r\n在状态值(998$s)中添加 '"+strAdd+"'";
                        nCount++;
                    }
                    if (String.IsNullOrEmpty(strRemove) == false)
                    {
                        strResult += "\r\n在状态值(998$s)中去除 '" + strAdd + "'";
                        nCount++;
                    }

                }
                else
                {
                    strResult += "\r\n将状态值(998$s)改为 '" + strStateAction + "'";
                    nCount++;
                }

            }

            // time
            string strTimeAction = this.MainForm.AppInfo.GetString(
    "change_biblio_param",
    "opertime",
    "<不改变>");
            if (strTimeAction != "<不改变>")
            {
                if (strTimeAction == "<当前时间>")
                {
                    strResult += "\r\n将时间值(998$u)设置为当前时间";
                    nCount++;
                }
                else if (strTimeAction == "<清除>")
                {
                    strResult += "\r\n将时间值(998$u)清空";
                    nCount++;
                }
                else if (strTimeAction == "<指定时间>")
                {
                    string strValue = this.MainForm.AppInfo.GetString(
                        "change_biblio_param",
                        "opertime_value",
                        "");
                    strResult += "\r\n将时间值(998$u)修改为 '" + strValue + "'";
                    nCount++;
                }
                else
                {
                }

            }

            // batchno
            string strBatchNoAction = this.MainForm.AppInfo.GetString(
"change_biblio_param",
"batchNo",
"<不改变>");
            if (strBatchNoAction != "<不改变>")
            {
                strResult += "\r\n将批次号值(998$a)修改为 '" + strBatchNoAction + "'";
                nCount++;
            }

            return strResult;
        }

        // TODO: 预先将AppInfo中值取出，加快速度
        // return:
        //      -1  出错
        //      0   未发生改变
        //      1   发生了改变
        int ModifyField998(ref string strMARC,
            DateTime now,
            out string strError)
        {
            strError = "";
            // int nRet = 0;
            bool bChanged = false;

            string strField998 = MarcUtil.GetField(strMARC,
                "998");
            if (strField998 == null)
            {
                strError = "GetField() 998 error";
                return -1;
            }

            if (String.IsNullOrEmpty(strField998) == true
                || strField998.Length < 5)
                strField998 = "998  ";


            // state
            string strStateAction = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = MarcUtil.GetSubfieldContent(strField998,
    "s");

                if (strStateAction == "<增、减>")
                {
                    string strAdd = this.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state_add",
                "");
                    string strRemove = this.MainForm.AppInfo.GetString(
            "change_biblio_param",
            "state_remove",
            "");

                    string strOldState = strState;

                    if (String.IsNullOrEmpty(strAdd) == false)
                        StringUtil.SetInList(ref strState, strAdd, true);
                    if (String.IsNullOrEmpty(strRemove) == false)
                        StringUtil.SetInList(ref strState, strRemove, false);

                    if (strOldState != strState)
                    {
                        MarcUtil.ReplaceSubfieldContent(ref strField998,
                            "s", strState);
                        bChanged = true;
                    }
                }
                else
                {
                    if (strStateAction != strState)
                    {
                        MarcUtil.ReplaceSubfieldContent(ref strField998,
                            "s", strStateAction);
                        bChanged = true;
                    }
                }

            }


            // time
            string strTimeAction = this.MainForm.AppInfo.GetString(
    "change_biblio_param",
    "opertime",
    "<不改变>");
            if (strTimeAction != "<不改变>")
            {
                string strTime = MarcUtil.GetSubfieldContent(strField998,
    "u");
                DateTime time = new DateTime(0);
                if (strTimeAction == "<当前时间>")
                {
                    time = now;
                }
                else if (strTimeAction == "<清除>")
                {

                }
                else if (strTimeAction == "<指定时间>")
                {
                    string strValue = this.MainForm.AppInfo.GetString(
                        "change_biblio_param",
                        "opertime_value",
                        "");
                    if (String.IsNullOrEmpty(strValue) == true)
                    {
                        strError = "当进行 <指定时间> 方式的修改时，所指定的时间值不能为空";
                        return -1;
                    }
                    try
                    {
                        time = DateTime.Parse(strValue);
                    }
                    catch (Exception ex)
                    {
                        strError = "无法解析时间字符串 '" + strValue + "' :" + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    // 不支持
                    strError = "不支持的时间动作 '" + strTimeAction + "'";
                    return -1;
                }

                string strOldTime = strTime;

                if (strTimeAction == "<清除>")
                    strTime = "";
                else
                    strTime = time.ToString("u");

                if (strOldTime != strTime)
                {
                    MarcUtil.ReplaceSubfieldContent(ref strField998,
    "u", strTime);
                    bChanged = true;
                }
            }


            // batchno
            string strBatchNoAction = this.MainForm.AppInfo.GetString(
"change_biblio_param",
"batchNo",
"<不改变>");
            if (strBatchNoAction != "<不改变>")
            {
                string strBatchNo = MarcUtil.GetSubfieldContent(strField998,
                    "a");

                if (strBatchNo != strBatchNoAction)
                {
                    MarcUtil.ReplaceSubfieldContent(ref strField998,
                        "a", strBatchNoAction);
                    bChanged = true;
                }
            }

            if (bChanged == false)
                return 0;

            // 
            MarcUtil.ReplaceField(ref strMARC,
                "998",
                0,
                strField998);

            return 1;
        }

        // 设置动作参数
        // return:
        //      false   放弃
        //      true    确认
        public bool SetChangeParameters()
        {
            ChangeBiblioActionDialog dlg = new ChangeBiblioActionDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.MainForm = this.MainForm;

            this.MainForm.AppInfo.LinkFormState(dlg, "ChangeBiblioActionDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.OK)
                return true;

            return false;
        }

        private void button_changeParam_Click(object sender, EventArgs e)
        {
            SetChangeParameters();
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
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

        private void QuickChangeBiblioForm_Activated(object sender, EventArgs e)
        {
            // this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

        }

        private void button_file_getRecpathFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的(书目库)记录路径文件名";
            dlg.FileName = this.textBox_recpathFile.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_recpathFile.Text = dlg.FileName;
        }




    }
}