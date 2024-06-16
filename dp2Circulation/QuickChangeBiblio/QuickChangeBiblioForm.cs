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
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Script;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改书目窗
    /// </summary>
    internal partial class QuickChangeBiblioForm : MyForm
    {
        public QuickChangeBiblioForm()
        {
            this.UseLooping = true; // 2022/11/3

            InitializeComponent();
        }

        private void QuickChangeBiblioForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif
        }

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

            /*
            this.EnableControls(false);
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("");
            _stop.BeginLoop();
            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "disableControl");

            try
            {
                return DoTextLines(looping.Progress,
                    channel);
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
            }
        }

        public void DoRecPathFile(string strFileName)
        {
            this.tabControl_input.SelectedTab = this.tabPage_recpathFile;
            this.textBox_recpathFile.Text = strFileName;

            /*
            this.EnableControls(false);
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("");
            _stop.BeginLoop();
            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "disableControl");
            try
            {
                DoFileName(looping.Progress,
                    channel);
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
            }
        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            /*
            this.EnableControls(false);
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("");
            _stop.BeginLoop();
            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "disableControl");
            try
            {
                if (this.tabControl_input.SelectedTab == this.tabPage_paths)
                {
                    DoTextLines(looping.Progress, channel);
                }
                else if (this.tabControl_input.SelectedTab == this.tabPage_recpathFile)
                {
                    DoFileName(looping.Progress, channel);
                }

            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
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
        int DoTextLines(Stop stop,
            LibraryChannel channel)
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
                bool temp = false;
                DialogResult result = MessageDlg.Show(this,
"即将进行下述修改动作：\r\n---" + strInfo + "\r\n\r\n开始处理?\r\n[注: 修改会自动兑现保存]",
"dp2Circulation",
MessageBoxButtons.OKCancel,
MessageBoxDefaultButton.Button1,
            ref temp);
                /*
                DialogResult result = MessageBox.Show(this,
"即将进行下述修改动作：\r\n---" + strInfo + "\r\n\r\n开始处理?\r\n[注: 修改会自动兑现保存]",
"dp2Circulation",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                */
                if (result == DialogResult.Cancel)
                    return 0;
            }

            _hideAdd210Dialog = false;
            _add210_dialog_result = DialogResult.Yes;

            int nCount = 0; // 总共处理多少条
            int nChangedCount = 0;  // 发生修改的有多少条

            DateTime now = DateTime.Now;

            stop?.SetProgressRange(0, this.textBox_paths.Lines.Length);

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始批处理修改书目记录</div>");
            for (int i = 0; i < this.textBox_paths.Lines.Length; i++)
            {
                Application.DoEvents();
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断1";
                    goto ERROR1;
                }

                string strLine = this.textBox_paths.Lines[i].Trim();
                nRet = strLine.IndexOfAny(new char[] { ' ', '\t' });
                if (nRet != -1)
                {
                    strLine = strLine.Substring(0, nRet).Trim();
                }

                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                nRet = ChangeOneRecord(
                    stop,
                    channel,
                    strLine,
                    now,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nCount++;
                if (nRet == 1)
                    nChangedCount++;
                stop?.SetProgressValue(i + 1);
            }

            Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束批处理修改书目记录</div>");

            MessageBox.Show(this, "处理完毕。共处理记录 " + nCount.ToString() + " 条，实际发生修改 " + nChangedCount.ToString() + " 条");
            return 1;
        ERROR1:
            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"修改书目记录出错: {strError}") + "</div>");
            MessageBox.Show(this, strError);
            return -1;
        }

        void DoFileName(Stop stop,
            LibraryChannel channel)
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

            _hideAdd210Dialog = false;
            _add210_dialog_result = DialogResult.Yes;

            int nCount = 0; // 总共处理多少条
            int nChangedCount = 0;  // 发生修改的有多少条

            using (StreamReader sr = new StreamReader(this.textBox_recpathFile.Text))
            {
                DateTime now = DateTime.Now;

                stop?.SetProgressRange(0, sr.BaseStream.Length);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始批处理修改书目记录</div>");

                for (; ; )
                {
                    Application.DoEvents();
                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断2";
                        goto ERROR1;
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
                    nRet = ChangeOneRecord(
                        stop,
                        channel,
                        strLine,
                        now,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    nCount++;
                    if (nRet == 1)
                        nChangedCount++;
                    stop?.SetProgressValue(sr.BaseStream.Position);
                }
            }

            Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束批处理修改书目记录</div>");

            MessageBox.Show(this, "处理完毕。共处理记录 " + nCount.ToString() + " 条，实际发生修改 " + nChangedCount.ToString() + " 条");
            return;
        ERROR1:
            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"修改书目记录出错: {strError}") + "</div>");
            MessageBox.Show(this, strError);
        }

        public override void UpdateEnable(bool bEnable)
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
        int ChangeOneRecord(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            DateTime now,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            stop?.SetMessage("正在处理 " + strBiblioRecPath + " ...");

            Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"=== {strBiblioRecPath} ===") + "</div>");

            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            byte[] timestamp = null;
            long lRet = channel.GetBiblioInfos(
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
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                "", // strMarcSyntax,
                out string strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            string strMarcSyntax = strOutMarcSyntax;

            bool changed = false;
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
            if (nRet == 1)
                changed = true;
            /*
            if (nRet == 0)
                return 0;
            */

            if (ChangeBiblioActionDialog.NeedAdd102)
            {
                nRet = Add102(
                    strMarcSyntax,
                    ref strMARC,
                    out strError);
                if (nRet == -1)
                {
                    strError = $"为书目记录 {strBiblioRecPath} 添加 102 字段时出错: {strError}";
                    return -1;
                }
                if (nRet == 1)
                    changed = true;
                else if (string.IsNullOrEmpty(strError) == false)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug info'>&nbsp;" + HttpUtility.HtmlEncode($"{strError}") + "</div>");
                }
            }

            if (ChangeBiblioActionDialog.NeedAddPublisher)
            {
                nRet = AddPublisher(
                    strMarcSyntax,
                    ref strMARC,
                    out strError);
                if (nRet == -1)
                {
                    strError = $"为书目记录 {strBiblioRecPath} 添加出版者子字段时出错: {strError}";
                    return -1;
                }
                if (nRet == 1)
                    changed = true;
                else if (string.IsNullOrEmpty(strError) == false)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>&nbsp;" + HttpUtility.HtmlEncode($"{strError}") + "</div>");
                }
            }

            if (ChangeBiblioActionDialog.NeedAddPinyin
                && strMarcSyntax == "unimarc")
            {
                nRet = AddPinyin(
                    ref strMARC,
                    ChangeBiblioActionDialog.PinyinCfgs,
                    ChangeBiblioActionDialog.PinyinStyle,
                    "",
                    ChangeBiblioActionDialog.PinyinAutoSel,
                    out strError);
                if (nRet == -1)
                {
                    strError = $"为书目记录 {strBiblioRecPath} 添加拼音子字段时出错: {strError}";
                    return -1;
                }
                if (nRet == 1)
                    changed = true;
                else if (string.IsNullOrEmpty(strError) == false)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug info'>&nbsp;" + HttpUtility.HtmlEncode($"{strError}") + "</div>");
                }
            }

            if (changed == false)
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
                XmlElement new_node = domMarc.CreateElement("dprms",
                    "file",
                    DpNs.dprms);
                domMarc.DocumentElement.AppendChild(new_node);
                DomUtil.SetElementOuterXml(new_node, nodes[i].OuterXml);
            }

            // 保存
            byte[] baNewTimestamp = null;
            string strOutputPath = "";
            lRet = channel.SetBiblioInfo(
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
            string strStateAction = Program.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                if (strStateAction == "<增、减>")
                {
                    string strAdd = Program.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state_add",
                "");
                    string strRemove = Program.MainForm.AppInfo.GetString(
            "change_biblio_param",
            "state_remove",
            "");
                    if (String.IsNullOrEmpty(strAdd) == false)
                    {
                        strResult += "\r\n在状态值(998$s)中添加 '" + strAdd + "'";
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
            string strTimeAction = Program.MainForm.AppInfo.GetString(
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
                    string strValue = Program.MainForm.AppInfo.GetString(
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
            string strBatchNoAction = Program.MainForm.AppInfo.GetString(
"change_biblio_param",
"batchNo",
"<不改变>");
            if (strBatchNoAction != "<不改变>")
            {
                strResult += "\r\n将批次号值(998$a)修改为 '" + strBatchNoAction + "'";
                nCount++;
            }

            if (ChangeBiblioActionDialog.NeedAdd102)
            {
                strResult += "\r\n添加 102 字段";
                nCount++;
            }

            if (ChangeBiblioActionDialog.NeedAddPublisher)
            {
                strResult += "\r\n添加出版者子字段";
                nCount++;
            }

            if (ChangeBiblioActionDialog.NeedAddPinyin)
            {
                strResult += "\r\n添加拼音子字段";
                nCount++;
            }

            return strResult;
        }

        int Add102(
            string strMarcSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";

            if (strMarcSyntax == "usmarc")
            {
                strError = "暂不能处理 USMARC 格式记录";
                return 0;   // 暂不能处理 USMARC 格式
            }

            MarcRecord record = new MarcRecord(strMARC);

            // 观察 102$a$b 子字段是否已经存在。如果已经存在，则放弃处理
            var s_102a = record.select("field[@name='102']/subfield[@name='a']").FirstContent?.Trim();
            var s_102b = record.select("field[@name='102']/subfield[@name='b']").FirstContent?.Trim();
            if (string.IsNullOrEmpty(s_102a) == false
                || string.IsNullOrEmpty(s_102b) == false)
            {
                // 102$a$b 子字段至少存在一个
                strError = "记录中已经存在 102$a$b 子字段(至少一个), 因此放弃添加 102$a$b";
                return 0;
            }

            var isbn = record.select("field[@name='010']/subfield[@name='a']").FirstContent?.Trim();
            if (string.IsNullOrEmpty(isbn))
            {
                // 010$a 不存在
                strError = "记录中不存在 010$a 子字段, 因此无法加 102$a$b";
                return 0;
            }

            // 切割出 出版社 代码部分
            int nRet = Program.MainForm.GetPublisherNumber(isbn,
                out string strPublisherNumber,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.Get102Info(strPublisherNumber,
    out string strValue,
    out strError);
            if (nRet == -1)
                return -1;

            bool new_entry = false;
        REDO_INPUT:
            if (nRet == 0 || string.IsNullOrEmpty(strValue))
            {
                // 创建新条目
                strValue = InputDlg.GetInput(
                    this,
                    null,
                    "请输入ISBN出版社号码 '" + isbn + "' 对应的UNIMARC 102$a$b参数(格式 国家代码[2位]:城市代码[6位]):",
                    "国家代码[2位]:城市代码[6位]",
                    Program.MainForm.DefaultFont);
                if (strValue == null)
                {
                    strError = "用户放弃处理";
                    return 0; // 放弃操作
                }

                nRet = this.Set102Info(strPublisherNumber,
                    strValue,
                    out strError);
                if (nRet == -1)
                    return -1;

                new_entry = true;
            }

            // MessageBox.Show(this.DetailForm, strValue);

            // 把全角冒号替换为半角的形态
            strValue = strValue.Replace("：", ":");

            string strCountryCode = "";
            string strCityCode = "";
            nRet = strValue.IndexOf(":");
            if (nRet == -1)
            {
                strCountryCode = strValue;

                if (strCountryCode.Length != 2)
                {
                    strError = "国家代码 '" + strCountryCode + "' 应当为2字符";
                    if (new_entry)
                        goto REDO_INPUT;
                    return -1;
                }
            }
            else
            {
                strCountryCode = strValue.Substring(0, nRet);
                strCityCode = strValue.Substring(nRet + 1);
                if (strCountryCode.Length != 2)
                {
                    strError = "冒号前面的国家代码部分 '" + strCountryCode + "' 应当为2字符";
                    if (new_entry)
                        goto REDO_INPUT;
                    return -1;
                }
                if (strCityCode.Length != 6)
                {
                    strError = "冒号后面的城市代码部分 '" + strCityCode + "' 应当为6字符";
                    if (new_entry)
                        goto REDO_INPUT;
                    return -1;
                }
            }

            record.setFirstSubfield("102", "a", strCountryCode);
            record.setFirstSubfield("102", "b", strCityCode);
            if (strMARC != record.Text)
            {
                strMARC = record.Text;
                return 1;
            }
            strError = "没有发生修改";
            return 0;
        }

        bool _hideAdd210Dialog = false;
        DialogResult _add210_dialog_result = DialogResult.Yes;

        // 加入出版地、出版者
        int AddPublisher(
            string strMarcSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";

            if (strMarcSyntax == "usmarc")
            {
                strError = "暂不能处理 USMARC 格式记录";
                return 0;   // 暂不能处理 USMARC 格式
            }

            MarcRecord record = new MarcRecord(strMARC);

            // 观察 210$a$c 子字段是否已经存在。如果已经存在，则放弃处理
            var s_210a = record.select("field[@name='210']/subfield[@name='a']").FirstContent?.Trim();
            var s_210c = record.select("field[@name='210']/subfield[@name='c']").FirstContent?.Trim();
            if (string.IsNullOrEmpty(s_210a) == false
                && string.IsNullOrEmpty(s_210c) == false)
            {
                // 210$a$c 子字段都存在
                strError = "记录中已经存在 210$a$c 子字段, 因此放弃添加 210$a$c 子字段";
                return 0;
            }

            var isbn = record.select("field[@name='010']/subfield[@name='a']").FirstContent?.Trim();
            if (string.IsNullOrEmpty(isbn))
            {
                // 010$a 不存在
                strError = "记录中不存在 010$a 子字段,因此无法加出版社子字段";
                return 0;
            }

            // 从 ISBN 切割出 出版社 代码部分
            int nRet = Program.MainForm.GetPublisherNumber(isbn,
                out string strPublisherNumber,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.GetPublisherInfo(strPublisherNumber,
                out string strValue,
                out strError);
            if (nRet == -1)
                return -1;

            string strName = "";
            string strCity = "";

        REDO_INPUT:
            if (nRet == 0 || string.IsNullOrEmpty(strValue))
            {
                // 创建新条目
                strValue = InputDlg.GetInput(
                    this,
                    null,
                    "请输入ISBN出版社号 '" + strPublisherNumber + "' 对应的出版社名称(格式 出版地:出版社名):",
                    "出版地:出版社名",
                    Program.MainForm.DefaultFont);
                if (strValue == null)
                {
                    strError = "用户放弃处理";
                    return 0; // 放弃操作
                }

                // 把全角冒号替换为半角的形态
                strValue = strValue.Replace("：", ":");

                ParsePublisherText(strValue,
out strName,
out strCity);
                if (strCity == "出版地" || strName == "出版社名")
                {
                    MessageBox.Show(this, $"输入的出版地、出版社内容 '{strValue}' 不正确。请重新输入");
                    nRet = 0;
                    goto REDO_INPUT;
                }

                nRet = this.SetPublisherInfo(strPublisherNumber,
                    strValue,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                // 把全角冒号替换为半角的形态
                strValue = strValue.Replace("：", ":");

                ParsePublisherText(strValue,
out strName,
out strCity);

                if (strCity == "出版地" || strName == "出版社名")
                {
                    MessageBox.Show(this, $"输入的出版地、出版社内容 '{strValue}' 不正确。请重新输入");
                    nRet = 0;
                    goto REDO_INPUT;
                }
            }
            // MessageBox.Show(this.DetailForm, strValue);

            /*
            nRet = strValue.IndexOf(":");
            if (nRet == -1)
            {
                strName = strValue;
            }
            else
            {
                strCity = strValue.Substring(0, nRet);
                strName = strValue.Substring(nRet + 1);
            }
            */


            // 2021/6/30
            if (string.IsNullOrEmpty(s_210a) == false
                && s_210a != strCity)
            {
                if (_hideAdd210Dialog == false)
                {
                    _add210_dialog_result = MessageDlg.Show(this,
            $"记录中已经存在的 210$a 子字段内容 '{s_210a}' 和拟添加的 '{strCity}:{strName}' 不一致。\r\n\r\n是否放弃添加?\r\n\r\n[不添加]放弃添加; [覆盖]用覆盖方式添加; [中断]中断批处理过程",
            "根据 010$a 自动添加 210$a$c",
            MessageBoxButtons.YesNoCancel,
            MessageBoxDefaultButton.Button1,
            ref _hideAdd210Dialog,
            new string[] { "不添加", "覆盖", "中断" },
            "以后不再显示本对话框");
                }

                if (_add210_dialog_result == DialogResult.Yes)
                {
                    strError = $"记录中已经存在的 210$a 子字段内容 '{s_210a}' 和拟添加的 '{strCity}:{strName}' 不一致, 加出版社子字段被放弃。请手动处理添加，或者先删除书目记录中的 210$a 以后再尝试自动添加";
                    return 0;
                }

                if (_add210_dialog_result == DialogResult.Cancel)
                {
                    strError = "用户中断处理";
                    return -1;
                }

                strError = $"记录中已经存在的 210$a 子字段内容 '{s_210a}' 和拟添加的 '{strCity}:{strName}' 不一致，原内容已被强行覆盖";
                Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"警告: {strError}") + "</div>");
            }

            // 2021/6/30
            if (string.IsNullOrEmpty(s_210c) == false
                && s_210c != strName)
            {
                if (_hideAdd210Dialog == false)
                {
                    _add210_dialog_result = MessageDlg.Show(this,
            $"记录中已经存在的 210$c 子字段内容 '{s_210c}' 和拟添加的 '{strCity}:{strName}' 不一致。\r\n\r\n是否放弃添加?\r\n\r\n[不添加]放弃添加; [覆盖]用覆盖方式添加; [中断]中断批处理过程",
            "根据 010$a 自动添加 210$a$c",
            MessageBoxButtons.YesNoCancel,
            MessageBoxDefaultButton.Button1,
            ref _hideAdd210Dialog,
            new string[] { "不添加", "覆盖", "中断" },
            "以后不再显示本对话框");
                }

                if (_add210_dialog_result == DialogResult.Yes)
                {
                    strError = $"记录中已经存在的 210$c 子字段内容 '{s_210c}' 和拟添加的 '{strCity}:{strName}' 不一致, 加出版社子字段被放弃。请手动处理添加，或者先删除数据记录中的 210$c 以后再尝试自动添加";
                    return 0;
                }

                if (_add210_dialog_result == DialogResult.Cancel)
                {
                    strError = "用户中断处理";
                    return -1;
                }

                strError = $"记录中已经存在的 210$c 子字段内容 '{s_210c}' 和拟添加的 '{strCity}:{strName}' 不一致，原内容已被强行覆盖";
                Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"警告: {strError}") + "</div>");
            }

            record.setFirstSubfield("210", "a", strCity);
            record.setFirstSubfield("210", "c", strName);
            if (strMARC != record.Text)
            {
                strMARC = record.Text;
                return 1;
            }

            strError = "没有发生修改";
            return 0;
        }

        static void ParsePublisherText(string strValue,
            out string strName,
            out string strCity)
        {
            strName = "";
            strCity = "";
            int nRet = strValue.IndexOf(":");
            if (nRet == -1)
            {
                strName = strValue;
            }
            else
            {
                strCity = strValue.Substring(0, nRet);
                strName = strValue.Substring(nRet + 1);
            }
        }

        public int AddPinyin(
            ref string strMARC,
            string strCfgXml,
            // bool bUseCache = true,
            PinyinStyle style,
            string strPrefix,
            bool bAutoSel,
            out string strError)
        {
            strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml 装载到 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            //Hashtable old_selected = new Hashtable();   // (bUseCache == true) ? this.DetailForm.GetSelectedPinyin() : new Hashtable();
            //Hashtable new_selected = new Hashtable();

            MarcRecord record = new MarcRecord(strMARC);
            // PinyinStyle style = PinyinStyle.None;	// 在这里修改拼音大小写风格
            foreach (MarcField field in record.Fields)
            // for (int i = 0; i < DetailForm.MarcEditor.Record.Fields.Count; i++)
            {
                // Field field = DetailForm.MarcEditor.Record.Fields[i];

                List<PinyinCfgItem> cfg_items = null;
                int nRet = DetailHost.GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strHanzi = "";
                // string strNextSubfieldName = "";

                string strField = field.Text;

                string strFieldPrefix = "";

                // 2012/11/5
                // 观察字段内容前面的 {} 部分
                {
                    string strCmd = StringUtil.GetLeadingCommand(field.Content);
                    if (string.IsNullOrEmpty(strRuleParam) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strCurRule = strCmd.Substring(3);
                        if (strCurRule != strRuleParam)
                            continue;
                    }
                    else if (string.IsNullOrEmpty(strCmd) == false)
                    {
                        strFieldPrefix = "{" + strCmd + "}";
                    }
                }

                // 2012/11/5
                // 观察 $* 子字段
                {
                    var nodes = field.select("subfield[@name='*']");
                    MarcSubfield subfield = null;
                    if (nodes.count > 0)
                        subfield = nodes[0] as MarcSubfield;

                    /*
                    //
                    string strSubfield = "";
                    string strNextSubfieldName1 = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = MarcUtil.GetSubfield(strField,
                        ItemType.Field,
                        "*",    // "*",
                        0,
                        out strSubfield,
                        out strNextSubfieldName1);
                    if (nRet == 1)
                    {
                        string strCurStyle = strSubfield.Substring(1);
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                        else if (string.IsNullOrEmpty(strCurStyle) == false)
                        {
                            strFieldPrefix = "{cr:" + strCurStyle + "}";
                        }
                    }
                    */
                    if (subfield != null)
                    {
                        string strCurStyle = subfield.Content;
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && strCurStyle != strRuleParam)
                            continue;
                        else if (string.IsNullOrEmpty(strCurStyle) == false)
                        {
                            strFieldPrefix = "{cr:" + strCurStyle + "}";
                        }
                    }
                }

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.From.Length; k++)
                    {
                        if (item.From.Length != item.To.Length)
                        {
                            strError = "配置事项 fieldname='" + item.FieldName + "' from='" + item.From + "' to='" + item.To + "' 其中from和to参数值的字符数不等";
                            return -1;
                        }

                        string from = new string(item.From[k], 1);
                        string to = new string(item.To[k], 1);
                        for (int j = 0; ; j++)
                        {
                            // return:
                            //		-1	error
                            //		0	not found
                            //		1	found

                            nRet = MarcUtil.GetSubfield(strField,
                                ItemType.Field,
                                from,
                                j,
                                out strHanzi,
                                out string strNextSubfieldName);
                            if (nRet != 1)
                                break;

                            if (strHanzi.Length <= 1)
                                break;

                            strHanzi = strHanzi.Substring(1);

                            // 2013/6/13
                            if (DetailHost.ContainHanzi(strHanzi) == false)
                                continue;

                            string strSubfieldPrefix = "";  // 当前子字段内容本来具有的前缀

                            // 检查内容前部可能出现的 {} 符号
                            string strCmd = StringUtil.GetLeadingCommand(strHanzi);
                            if (string.IsNullOrEmpty(strRuleParam) == false
                                && string.IsNullOrEmpty(strCmd) == false
                                && StringUtil.HasHead(strCmd, "cr:") == true)
                            {
                                string strCurRule = strCmd.Substring(3);
                                if (strCurRule != strRuleParam)
                                    continue;   // 当前子字段属于和strPrefix表示的不同的编目规则，需要跳过，不给加拼音
                                strHanzi = strHanzi.Substring(strPrefix.Length); // 去掉 {} 部分
                            }
                            else if (string.IsNullOrEmpty(strCmd) == false)
                            {
                                strHanzi = strHanzi.Substring(strCmd.Length + 2); // 去掉 {} 部分
                                strSubfieldPrefix = "{" + strCmd + "}";
                            }

                            string strPinyin = "";

                            // strPinyin = (string)old_selected[strHanzi];
                            if (string.IsNullOrEmpty(strPinyin) == true)
                            {
                                nRet = Program.MainForm.GetPinyin(
                                    this,
                                    strHanzi,
                                    style,
                                    bAutoSel,
                                    out strPinyin,
                                    out strError);
                                if (nRet == -1)
                                {
                                    //new_selected = null;
                                    return -1;
                                }
                                if (nRet == 0)
                                {
                                    //new_selected = null;
                                    strError = "用户中断。拼音子字段内容可能不完整。";
                                    return -1;
                                }
                            }

                            //if (new_selected != null && nRet != 2)
                            //    new_selected[strHanzi] = strPinyin;

                            nRet = MarcUtil.DeleteSubfield(
                                ref strField,
                                to,
                                j);

                            string strContent = strPinyin;

                            if (string.IsNullOrEmpty(strPrefix) == false)
                                strContent = strPrefix + strPinyin;
                            else if (string.IsNullOrEmpty(strSubfieldPrefix) == false)
                                strContent = strSubfieldPrefix + strPinyin;

                            nRet = MarcUtil.InsertSubfield(
                                ref strField,
                                from,
                                j,
                                new string(MarcUtil.SUBFLD, 1) + to + strContent,
                                1);
                            field.Text = strField;
                        }
                    }
                }
            }
            if (strMARC != record.Text)
            {
                strMARC = record.Text;
                return 1;
            }
            strError = "没有发生修改";
            return 0;
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
            string strStateAction = Program.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state",
                "<不改变>");
            if (strStateAction != "<不改变>")
            {
                string strState = MarcUtil.GetSubfieldContent(strField998,
    "s");

                if (strStateAction == "<增、减>")
                {
                    string strAdd = Program.MainForm.AppInfo.GetString(
                "change_biblio_param",
                "state_add",
                "");
                    string strRemove = Program.MainForm.AppInfo.GetString(
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
            string strTimeAction = Program.MainForm.AppInfo.GetString(
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
                    string strValue = Program.MainForm.AppInfo.GetString(
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
            string strBatchNoAction = Program.MainForm.AppInfo.GetString(
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
            // dlg.MainForm = Program.MainForm;

            Program.MainForm.AppInfo.LinkFormState(dlg, "ChangeBiblioActionDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

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
            // Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

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