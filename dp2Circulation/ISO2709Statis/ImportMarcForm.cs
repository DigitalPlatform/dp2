using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using static dp2Circulation.MainForm;

namespace dp2Circulation
{
    public partial class ImportMarcForm : MyScriptForm
    {
        OpenMarcFileDlg _openMarcFileDialog = null;

        public ImportMarcForm()
        {
            InitializeComponent();

            _openMarcFileDialog = new OpenMarcFileDlg();
            _openMarcFileDialog.IsOutput = false;
            this.tabPage_source.Padding = new Padding(4, 4, 4, 4);
            this.tabPage_source.Controls.Add(_openMarcFileDialog.MainPanel);
            _openMarcFileDialog.MainPanel.Dock = DockStyle.Fill;
        }

        private void ImportMarcForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            // 输入的ISO2709文件名
            this._openMarcFileDialog.FileName = Program.MainForm.AppInfo.GetString(
                "ImportMarcForm",
                "input_iso2709_filename",
                "");

            // 编码方式
            this._openMarcFileDialog.EncodingName = Program.MainForm.AppInfo.GetString(
    "ImportMarcForm",
    "input_iso2709_file_encoding",
    "");

            this._openMarcFileDialog.MarcSyntax = Program.MainForm.AppInfo.GetString(
    "ImportMarcForm",
    "input_marc_syntax",
    "unimarc");

            this._openMarcFileDialog.Mode880 = Program.MainForm.AppInfo.GetBoolean(
    "ImportMarcForm",
    "input_mode880",
    false);

            ScriptManager.CfgFilePath = Path.Combine(
Program.MainForm.UserDir,
"import_marc_projects.xml");

            this.checkBox_overwriteByG01.Checked = Program.MainForm.AppInfo.GetBoolean(
    "ImportMarcForm",
    "overwrite_by_g01",
    false);
        }

        private void ImportMarcForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportMarcForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                // 输入的ISO2709文件名
                Program.MainForm.AppInfo.SetString(
                    "ImportMarcForm",
                    "input_iso2709_filename",
                    this._openMarcFileDialog.FileName);

                // 编码方式
                Program.MainForm.AppInfo.SetString(
        "ImportMarcForm",
        "input_iso2709_file_encoding",
        this._openMarcFileDialog.EncodingName);

                Program.MainForm.AppInfo.SetString(
    "ImportMarcForm",
    "input_marc_syntax",
    this._openMarcFileDialog.MarcSyntax);

                Program.MainForm.AppInfo.SetBoolean(
    "ImportMarcForm",
    "input_mode880",
    this._openMarcFileDialog.Mode880);

                Program.MainForm.AppInfo.SetBoolean(
"ImportMarcForm",
"overwrite_by_g01",
this.checkBox_overwriteByG01.Checked);
            }

        }

        private void comboBox_targetDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_targetDbName.Items.Count > 0)
                return;

            if (Program.MainForm?.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty property in Program.MainForm.BiblioDbProperties)
                {
                    this.comboBox_targetDbName.Items.Add(property.DbName + "\t书目");
                }
            }

            if (Program.MainForm?.AuthorityDbProperties != null)
            {
                foreach (BiblioDbProperty property in Program.MainForm.AuthorityDbProperties)
                {
                    this.comboBox_targetDbName.Items.Add(property.DbName + "\t规范");
                }
            }
        }

        async Task<NormalResult> ImportAsync(string strTargetDbName)
        {
            return await Task<NormalResult>.Run(() => DoImport(strTargetDbName));
        }

        public string InputFileName
        {
            get
            {
                string value = "";
                this.Invoke((Action)(() =>
                {
                    value = this._openMarcFileDialog.FileName;
                }));
                return value;
            }
        }

        public string InputEncodingName
        {
            get
            {
                string value = "";
                this.Invoke((Action)(() =>
                {
                    value = this._openMarcFileDialog.EncodingName;
                }));
                return value;
            }
        }

        public string InputMarcSyntax
        {
            get
            {
                string value = "";
                this.Invoke((Action)(() =>
                {
                    value = this._openMarcFileDialog.MarcSyntax;
                }));
                return value;
            }
        }

        // return:
        //      0   普通返回
        //      1   要全部中断
        NormalResult DoImport(string strTargetDbName)
        {
            string strError = "";
            Encoding encoding = null;
            long lRet = 0;

            if (string.IsNullOrEmpty(this.InputEncodingName) == true)
            {
                return new NormalResult(-1, "尚未选定 ISO2709 文件的编码方式");
            }

            if (StringUtil.IsNumber(this.InputEncodingName) == true)
                encoding = Encoding.GetEncoding(Convert.ToInt32(this.InputEncodingName));
            else
                encoding = Encoding.GetEncoding(this.InputEncodingName);

            ClearErrorInfoForm();

            // 2021/4/8
            bool overwrite = (bool)this.Invoke(new Func<bool>(() =>
            {
                return this.checkBox_overwriteByG01.Checked;
            }));

            string strInputFileName = "";

            strInputFileName = this.InputFileName;

            string strBiblioSyntax = "";
            if (overwrite == false)
            {
                strBiblioSyntax = Program.MainForm.GetBiblioSyntax(strTargetDbName);
                if (strBiblioSyntax == null)
                {
                    strBiblioSyntax = Program.MainForm.GetAuthoritySyntax(strTargetDbName);
                    if (strBiblioSyntax == null)
                    {
                        strError = "没有找到书目或规范库 '" + strTargetDbName + "' 的 MARC 格式信息";
                        goto ERROR1;
                    }
                }

                // 和第一个属性页的 MARC 格式进行对比，如果不符合，要报错
                if (strBiblioSyntax != this.InputMarcSyntax)
                {
                    strError = "您在 数据来源 属性页为文件 '" + strInputFileName + "' 选定的 MARC 格式 '" + this.InputMarcSyntax + "' 与数据库 '" + strTargetDbName + "' 的预定义 MARC 格式 '" + strBiblioSyntax + "' 不符合。导入被终止";
                    goto ERROR1;
                }
            }

            string strRange = (string)this.Invoke(new Func<string>(() =>
            {
                return this.textBox_importRange.Text;
            }));
            RangeList range = null;
            if (string.IsNullOrEmpty(strRange) == false)
            {
                range = new RangeList(strRange);
                range.Sort();
            }

            // 编目批次号
            string batchNo = (string)this.Invoke(new Func<string>(() =>
            {
                return this.textBox_batchNo.Text;
            }));

            Stream file = null;

            try
            {
                file = File.Open(strInputFileName,
                    FileMode.Open,
                    FileAccess.Read);
            }
            catch (Exception ex)
            {
                return new NormalResult(-1, "打开文件 " + strInputFileName + " 失败: " + ex.Message);
            }

            this.Invoke((Action)(() =>
            {
                this.progressBar_records.Minimum = 0;
                this.progressBar_records.Maximum = (int)file.Length;
                this.progressBar_records.Value = 0;
            }));

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ $" 开始导入 MARC 文件 {strInputFileName}</div>");

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取ISO2709记录 ...");
            _stop.BeginLoop();

            bool dont_display_retry_dialog = false;   //  不再出现重试对话框
            bool dont_display_compare_dialog = false;  // 不再出现两条书目记录对比的对话框
            int overwrite_count = 0;
            int append_count = 0;
            int skip_count = 0;

            LibraryChannel channel = this.GetChannel();

            EnableControls(false);

            try
            {
                // 读入 ISO2709 记录的索引
                int nIndex = 0;

                DialogResult retry_result = DialogResult.Yes;

                for (int i = 0; ; i++)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (_stop != null && _stop.State != 0)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "准备中断。\r\n\r\n确实要中断全部操作? (Yes 全部中断；No 中断循环，但是继续收尾处理；Cancel 放弃中断，继续操作)",
                            "ImportMarcForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button3);

                        if (result == DialogResult.Yes)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                        if (result == DialogResult.No)
                            return new NormalResult();   // 假装loop正常结束

                        _stop.Continue(); // 继续循环
                    }


                    // 从ISO2709文件中读入一条MARC记录
                    // return:
                    //	-2	MARC格式错
                    //	-1	出错
                    //	0	正确
                    //	1	结束(当前返回的记录有效)
                    //	2	结束(当前返回的记录无效)
                    int nRet = MarcUtil.ReadMarcRecord(file,
                        encoding,
                        true,   // bRemoveEndCrLf,
                        true,   // bForce,
                        out string strMARC,
                        out strError);
                    if (nRet == -2 || nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "读入MARC记录(" + nIndex.ToString() + ")出错: " + strError + "\r\n\r\n确实要中断当前批处理操作?",
                            "ImportMarcForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Yes)
                        {
                            break;
                        }
                        else
                        {
                            strError = "读入MARC记录(" + nIndex.ToString() + ")出错: " + strError;
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strError) + "</div>");
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }
                    }

                    if (nRet != 0 && nRet != 1)
                        return new NormalResult();   // 结束

                    this.Invoke((Action)(() =>
                    {
                        this.progressBar_records.Value = (int)file.Position;
                    }));

                    if (range != null && range.IsInRange(i, true) == false)
                    {
                        _stop.SetMessage("跳过第 " + (i + 1).ToString() + " 个 ISO2709 记录");
                        continue;
                    }
                    else
                        _stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个 ISO2709 记录");

                    // 跳过太短的记录
                    if (string.IsNullOrEmpty(strMARC) == true
                        || strMARC.Length <= 24)
                        continue;

                    if (this._openMarcFileDialog.Mode880 == true
                        && (this._openMarcFileDialog.MarcSyntax == "usmarc" || this._openMarcFileDialog.MarcSyntax == "<自动>"))
                    {
                        MarcRecord temp = new MarcRecord(strMARC);
                        MarcQuery.ToParallel(temp);
                        strMARC = temp.Text;
                    }

                    // 2022/10/25
                    if (string.IsNullOrEmpty(batchNo) == false)
                    {
                        MarcRecord temp = new MarcRecord(strMARC);
                        if (batchNo == "[清除]")
                        {
                            temp.select("field[@name='998']/subfield[@name='a']").detach();
                        }
                        else
                        {
                            temp.setFirstSubfield("998", "a", batchNo);
                        }
                        strMARC = temp.Text;
                    }

                    // 处理
                    string strBiblioRecPath = strTargetDbName + "/?";

                REDO:
                    string existing_xml = "";
                    byte[] exist_timestamp = null;
                    string new_xml = "";

                    if (overwrite)
                    {
                        var result = GetOverwriteTargetPath(ref strMARC);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            goto ERROR1;
                        }
                        strBiblioRecPath = result.ErrorInfo;

                        string dbname = Global.GetDbName(strBiblioRecPath);
                        strBiblioSyntax = Program.MainForm.GetBiblioSyntax(dbname);
                        if (strBiblioSyntax == null)
                        {
                            strBiblioSyntax = Program.MainForm.GetAuthoritySyntax(dbname);
                            if (strBiblioSyntax == null)
                            {
                                strError = "没有找到书目或规范库 '" + dbname + "' 的 MARC 格式信息";
                                goto ERROR1;
                            }
                        }

                        /*
                        nRet = MarcUtil.Marc2Xml(strMARC,
strBiblioSyntax,
out domNew,
out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        */

                        // TODO: 去除记录中的 -01 字段
                        // 从服务器检索 strBiblioRecPath 位置的书目记录
                        lRet = channel.GetBiblioInfos(
        _stop,
        strBiblioRecPath,
        "",
        new string[] { "xml" },   // formats
        out string[] results,
        out exist_timestamp,
        out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            strError = $"获取打算覆盖的原书目记录 {strBiblioRecPath} 时出错: {strError}";
                            goto ERROR1;
                        }
                        existing_xml = results[0];

                        // 用旧记录垫底，并入新记录。目的是为了保留里面的 file 元素等
                        new_xml = existing_xml;
                        nRet = MarcUtil.Marc2XmlEx(strMARC,
    strBiblioSyntax,
    ref new_xml,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 左右窗格显示书目记录，等待按下“保存”按钮
                        string action = "";
                        if (dont_display_compare_dialog == true
                            && IsDifferent(existing_xml, new_xml) == false)
                            action = "overwrite";
                        else
                            this.Invoke((Action)(() =>
                            {
                                using (OverwriteBiblioDialog dlg = new OverwriteBiblioDialog())
                                {
                                    dlg.TargetPosition = strBiblioRecPath;
                                    dlg.SourcePosition = $"{i + 1}/{strInputFileName}";
                                    dlg.ExistingXml = existing_xml;
                                    dlg.NewXml = new_xml;

                                    Program.MainForm.AppInfo.LinkFormState(dlg, "importMarcForm_OverwriteBiblioDialog");
                                    dlg.ShowDialog(this);
                                    action = dlg.Action;

                                    if (dlg.DontAsk == true)
                                        dont_display_compare_dialog = true;
                                }
                            }));

                        if (action == "cancel" || string.IsNullOrEmpty(action))
                        {
                            strError = "中断批处理";
                            goto ERROR1;
                        }
                        if (action != "overwrite")
                        {
                            skip_count++;
                            string error = $"已跳过覆盖记录 {strBiblioRecPath}";
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div>");
                            GetErrorInfoForm().WriteHtml(error + "\r\n");
                            continue;
                        }

                        Debug.Assert(action == "overwrite");
                    }
                    else
                    {
                        nRet = MarcUtil.Marc2Xml(strMARC,
        strBiblioSyntax,
        out new_xml,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    lRet = channel.SetBiblioInfo(
                        _stop,
                        overwrite ? "change" : "new",
                        strBiblioRecPath,
                        "xml",
                        new_xml,
                        exist_timestamp,   // timestamp
                        "",
                        out string strOutputBiblioRecPath,
                        out byte[] baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strError) + "</div>");
                        GetErrorInfoForm().WriteHtml(strError + "\r\n");

#if NO
                        strError = "创建书目记录 '" + strBiblioRecPath + "' 时出错: " + strError + "\r\n";
                        goto ERROR1;
#endif

                        // string strText = strError;
                        if (dont_display_retry_dialog == false)
                        {
                            retry_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                            {
                                // return AutoCloseMessageBox.Show(this, strText + "\r\n\r\n(点右上角关闭按钮可以中断批处理)", 5000);
                                return MessageDlg.Show(this,
        strError + ", 是否重试？\r\n---\r\n\r\n[重试]重试; [跳过]跳过本条继续后面批处理; [中断]中断批处理",
        "ImportMarcForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxDefaultButton.Button1,
        ref dont_display_retry_dialog,
        new string[] { "重试", "跳过", "中断" },
        "后面不再出现此对话框，按本次选择自动处理");
                            }));
                        }

                        if (retry_result == System.Windows.Forms.DialogResult.Cancel)
                            goto ERROR1;
                        if (retry_result == DialogResult.Yes)
                            goto REDO;

                        // 在操作历史中显示出错信息

                    }
                    else
                    {
                        string html = "";
                        /*
                        nRet = MarcUtil.Xml2Marc(new_xml, false, null, out _, out string marc, out strError);
                        if (nRet != -1)
                            html = MarcUtil.GetHtmlOfMarc(marc,
        "", // strNewFragmentXml,
        "",
        false);
                        */

                        if (overwrite)
                        {
                            // 因为覆盖操作比较重要，建议写入 dp2circulation 错误日志文件，包括被覆盖的记录和新记录详细内容
                            //MarcUtil.CvtJineiToWorksheet();
                            //MainForm.WriteErrorLog($"覆盖操作");

                            Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"覆盖书目记录 {strBiblioRecPath}") + "</div>" + html);
                            overwrite_count++;
                        }
                        else
                        {
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode($"追加书目记录 {strBiblioRecPath}") + "</div>" + html);
                            append_count++;
                        }
                    }

                    nIndex++;
                }

                string message = "";
                if (overwrite)
                    message = $"导入完成。覆盖记录 {overwrite} 条; 跳过记录 {skip_count} 条";
                else
                    message = $"导入完成。共追加记录 {append_count} 条；跳过记录 {skip_count} 条";

                return new NormalResult
                {
                    Value = 0,
                    ErrorInfo = message
                };
            }
            finally
            {
                EnableControls(true);

                this.ReturnChannel(channel);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ $" 结束导入 MARC 文件 {strInputFileName}</div>");

                if (file != null)
                    file.Close();
            }
        ERROR1:
            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strError) + "</div>");
            GetErrorInfoForm().WriteHtml(strError + "\r\n");
            return new NormalResult(-1, strError);
        }

        // 比较两条记录，是否差异过大
        static bool IsDifferent(string xml1,
            string xml2)
        {
            int nRet = MarcUtil.Xml2Marc(xml1,
                false,
                "",
                out string syntax1,
                out string marc1,
                out string strError);
            if (nRet == -1)
                return true;

            nRet = MarcUtil.Xml2Marc(xml2,
    false,
    "",
    out string syntax2,
    out string marc2,
    out strError);
            if (nRet == -1)
                return true;

            if (syntax1 != syntax2)
                return true;

            MarcRecord record1 = new MarcRecord(marc1);
            MarcRecord record2 = new MarcRecord(marc2);
            string title1, title2;
            if (syntax1 == "usmarc")
            {
                title1 = record1.select("field[@name='245']/subfield[@name='a']").FirstContent;
                title2 = record2.select("field[@name='245']/subfield[@name='a']").FirstContent;
            }
            else
            {
                title1 = record1.select("field[@name='200']/subfield[@name='a']").FirstContent;
                title2 = record2.select("field[@name='200']/subfield[@name='a']").FirstContent;
            }

            return title1 != title2;
        }

        // 从 -01 字段中获取目标记录路径
        static NormalResult GetOverwriteTargetPath(ref string strMARC)
        {
            MarcRecord record = new MarcRecord(strMARC);
            var fields = record.select("field[@name='-01']");
            if (fields.count == 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "MARC 记录中没有包含 -01 字段"
                };
            if (fields.count > 1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "MARC 记录中具有多个 -01 字段，格式错误"
                };
            string text = fields[0].Content;
            if (string.IsNullOrEmpty(text))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "MARC 记录中 -01 字段内容为空，不符合要求"
                };

            string verify = StringUtil.GetParameterByPrefix(text, "verify");
            if (string.IsNullOrEmpty(verify))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "MARC 记录中 -01 字段格式不正确，缺乏 verify 部分"
                };

            if (BiblioSearchForm.VerifyString(verify) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"MARC 记录中 -01 字段格式不正确，verify 部分 '{verify}' 已损坏"
                };

            // 只保留 , 前的部分，即路径部分
            var parts = StringUtil.ParseTwoPart(text, ",");
            text = parts[0];

            // TODO: 验证“校验字符串”看看小语种字符是否丢失
            // TODO: 校验路径合法性
            fields.detach();
            strMARC = record.Text;
            return new NormalResult
            {
                Value = 0,
                ErrorInfo = text
            };
        }

        private async void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (string.IsNullOrEmpty(this._openMarcFileDialog.FileName) == true)
                {
                    strError = "尚未指定输入的 ISO2709 文件名";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this._openMarcFileDialog.EncodingName) == true)
                {
                    strError = "尚未选定 ISO2709 文件的编码方式";
                    goto ERROR1;
                }

                this.tabControl_main.SelectedTab = this.tabPage_selectTarget;
                return;
            }


            if (this.tabControl_main.SelectedTab == this.tabPage_selectTarget)
            {
                string strTargetDbName = this.comboBox_targetDbName.Text;

                if (this.checkBox_overwriteByG01.Checked == false
                    && string.IsNullOrEmpty(strTargetDbName) == true)
                {
                    strError = "尚未指定目标数据库";
                    this.comboBox_targetDbName.Focus();
                    goto ERROR1;
                }

                // 切换到执行page
                this.tabControl_main.SelectedTab = this.tabPage_runImport;

                this.Running = true;
                try
                {

                    NormalResult result = await ImportAsync(strTargetDbName);
#if NO
                    Task<NormalResult> task = Task.Run(() =>
                    {
                        return DoImport();
                    });
#endif
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }

                    this.tabControl_main.SelectedTab = this.tabPage_runImport;
                    this.ShowMessageBox(string.IsNullOrEmpty(result.ErrorInfo) ? "导入完成。" : result.ErrorInfo);
                    return;
                }
                finally
                {
                    this.Running = false;
                }
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_runImport)
            {
                // 切换到...
                this.tabControl_main.SelectedTab = this.tabPage_print;

                this.button_next.Enabled = false;
            }

            return;
        ERROR1:
            this.ShowMessageBox(strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this._openMarcFileDialog.MainPanel.Enabled = bEnable;

                this.comboBox_targetDbName.Enabled = this.checkBox_overwriteByG01.Checked == true ? false : bEnable;

                this.button_next.Enabled = bEnable;
            }));
        }

        private void ImportMarcForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this._stop);
        }

        private void checkBox_overwriteByG01_CheckedChanged(object sender, EventArgs e)
        {
            this.comboBox_targetDbName.Enabled = !this.checkBox_overwriteByG01.Checked;
        }
    }
}
