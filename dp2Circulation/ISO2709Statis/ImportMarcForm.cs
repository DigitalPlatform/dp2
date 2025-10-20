using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Core;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DocumentFormat.OpenXml.EMMA;
using dp2Circulation.Script;

namespace dp2Circulation
{
    public partial class ImportMarcForm : MyScriptForm
    {
        OpenMarcFileDlg _openMarcFileDialog = null;

        public ImportMarcForm()
        {
            this.UseLooping = true; // 2022/11/5

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

            PrepareScriptDirectory();

            this._openMarcFileDialog.IsOutput = false;  // 2024/6/4
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

            this.UiState = Program.MainForm.AppInfo.GetString(
"ImportMarcForm",
"uiState",
"");

            BeginListProjectNames();
            tabComboBox_dupProject_SelectedIndexChanged(null, null);
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

                Program.MainForm.AppInfo.SetString(
"ImportMarcForm",
"uiState",
this.UiState);
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.comboBox_targetDbName,
                    this.checkBox_lockTargetDbName,
                    this.checkBox_overwriteByG01,
                    this.textBox_importRange,
                    this.textBox_batchNo,
                    this.tabComboBox_dupProject,
                    this.checkBox_dontImportDupRecords,
                    //this.textBox_source,
                    this.textBox_operator,
                    this.textBox_biblio_filterScriptFileName,
                    /*
                    new ControlWrapper(this.checkBox_cfg_autoChangePassword, true),
                    new ControlWrapper(this.checkBox_forceCreate, false),
                    */
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.comboBox_targetDbName,
                    this.checkBox_lockTargetDbName,
                    this.checkBox_overwriteByG01,
                    this.textBox_importRange,
                    this.textBox_batchNo,
                    this.tabComboBox_dupProject,
                    this.checkBox_dontImportDupRecords,
                    //this.textBox_source,
                    this.textBox_operator,
                    this.textBox_biblio_filterScriptFileName,
                    /*
                    new ControlWrapper(this.checkBox_cfg_autoChangePassword, true),
                    new ControlWrapper(this.checkBox_forceCreate, false),
                    */
                };
                _inSetUiState++;
                try
                {
                    GuiState.SetUiState(controls, value);
                }
                finally
                {
                    _inSetUiState--;
                }
            }
        }

        int _inSetUiState = 0;

        // 检查列表。如果 .Text 中的值没有在列表中找到，则清除 .Text
        bool ClearProjectNameIfNeed()
        {
            var name = this.tabComboBox_dupProject.Text;
            if (string.IsNullOrEmpty(name))
                return false;
            foreach (string s in this.tabComboBox_dupProject.Items)
            {
                if (name == s)
                    return false;
            }

            this.tabComboBox_dupProject.Text = "[不查重]";
            return true;
        }

        void BeginListProjectNames()
        {
            this.TryInvoke(() =>
            {
                this.tabComboBox_dupProject.Items.Clear();
                this.tabComboBox_dupProject.Items.Add("[不查重]");
            });

            var dbName = this.TryGet(() =>
            {
                return this.comboBox_targetDbName.Text;
            });

            dbName = TabComboBox.GetLeftPart(dbName);

            if (string.IsNullOrEmpty(dbName))
            {
                this.TryInvoke(() =>
                {
                    ClearProjectNameIfNeed();
                });
                return;
            }

            // -1: 出错; >=0: 成功
            int nRet = ListProjectNames(dbName + "/?",
                out string[] projectnames,
                out string strError);
            if (nRet == -1)
            {
                this.MessageBoxShow(strError);
                return;
            }

            this.TryInvoke(() =>
            {
                this.tabComboBox_dupProject.Items.AddRange(projectnames);
                ClearProjectNameIfNeed();
            });
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
            return await Task<NormalResult>.Run(async () =>
            {
                return await _doImportAsync(strTargetDbName);
            });
        }

        public string InputFileName
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this._openMarcFileDialog.FileName;
                });
            }
        }

        public string InputEncodingName
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this._openMarcFileDialog.EncodingName;
                });
            }
        }

        public string InputMarcSyntax
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this._openMarcFileDialog.MarcSyntax;
                });
            }
        }

        public bool InputMode880
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this._openMarcFileDialog.Mode880;
                });
            }
        }

        public bool TryGetScriptCode(out string code)
        {
            try
            {
                string temp = this.TryGet(() =>
                {
                    return this.ScriptCode;
                });
                code = temp;
                return true;

            }
            catch (FileNotFoundException)
            {
                code = "";
                return false;
            }
        }

        public string ScriptCode
        {
            get
            {
                if (string.IsNullOrEmpty(ScriptFileName))
                    return "";

                var scriptFileName = Path.Combine(FilterScriptDirectory, ScriptFileName);

                if (File.Exists(scriptFileName))
                    return File.ReadAllText(scriptFileName);
                throw new FileNotFoundException($"脚本文件 '{scriptFileName}' 不存在 ...");
            }
        }

        public string ScriptFileName
        {
            get
            {
                string value = this.TryGet(() =>
                {
                    return this.textBox_biblio_filterScriptFileName.Text;
                });
                if (value == ScriptDialog.DO_NOT_USE_SCRIPT)
                    return "";

                return value;
            }
        }

        public static string BuildCacheKey(string pure_fileName)
        {
            return "import:" + pure_fileName;
        }

        // return:
        //      0   普通返回
        //      1   要全部中断
        async Task<NormalResult> _doImportAsync(string strTargetDbName)
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

            DupForm dup_form = null;

            var dup_project_name = this.TryGet(() =>
            {
                return this.tabComboBox_dupProject.Text;
            });
            var dont_import_dup = this.TryGet(() =>
            {
                return this.checkBox_dontImportDupRecords.Checked;
            });


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
                try
                {
                    range = new RangeList(strRange);
                    range.Sort();
                }
                catch (Exception ex)
                {
                    strError = $"导入记录范围 '{strRange}' 不合法: {ex.Message}";
                    goto ERROR1;
                }
            }

            // 编目批次号
            string batchNo = this.TryGet(() =>
            {
                return this.textBox_batchNo.Text;
            });
            /*
            string source = this.TryGet(() =>
            {
                return this.textBox_source.Text;
            });
            */
            string operator_string = this.TryGet(() =>
            {
                return this.textBox_operator.Text;
            });



            var scriptFileName = this.TryGet(() =>
            {
                return this.ScriptFileName;
            });

            if (TryGetScriptCode(out string filterCode) == false)
            {
                strError = $"脚本代码文件 {scriptFileName} 不存在";
                goto ERROR1;
            }

            var cacheKey = ExportMarcHoldingDialog.BuildCacheKey(scriptFileName);


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

            bool dont_display_retry_dialog = false;   //  不再出现重试对话框
            bool dont_display_compare_dialog = false;  // 不再出现两条书目记录对比的对话框
            int overwrite_count = 0;
            int append_count = 0;
            int skip_count = 0;

            VerifyHost host = null;

            GenerateData genData = new GenerateData(this, null);

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取ISO2709记录 ...");
            _stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取ISO2709记录 ...",
                "disableControl");
            try
            {
                if (string.IsNullOrEmpty(dup_project_name)
    || dup_project_name == "[不查重]")
                {

                }
                else
                {
                    this.TryInvoke(() =>
                    {
                        dup_form = Program.MainForm.EnsureDupForm();
                    });
                }

                // 读入 ISO2709 记录的索引
                int nIndex = 0;

                DialogResult retry_result = DialogResult.Yes;

                for (int i = 0; ; i++)
                {
                    // Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        DialogResult result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
                            "准备中断。\r\n\r\n确实要中断全部操作? (Yes 全部中断；No 中断循环，但是继续收尾处理；Cancel 放弃中断，继续操作)",
                            "ImportMarcForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button3);
                        });

                        if (result == DialogResult.Yes)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                        if (result == DialogResult.No)
                            return new NormalResult();   // 假装loop正常结束

                        looping.Progress.Continue(); // 继续循环
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
                        DialogResult result = this.TryGet(() =>
                        {
                            return MessageBox.Show(this,
                            "读入MARC记录(" + nIndex.ToString() + ")出错: " + strError + "\r\n\r\n确实要中断当前批处理操作?",
                            "ImportMarcForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        });
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
                        looping.Progress.SetMessage("跳过第 " + (i + 1).ToString() + " 个 ISO2709 记录");
                        continue;
                    }
                    else
                        looping.Progress.SetMessage("正在获取第 " + (i + 1).ToString() + " 个 ISO2709 记录");

                    // 跳过太短的记录
                    if (string.IsNullOrEmpty(strMARC) == true
                        || strMARC.Length <= 24)
                        continue;


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
        looping.Progress,
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

                    string summary = "";
                    {
                        List<NameValueLine> results = null;

                        if (strBiblioSyntax == "usmarc")
                            nRet = MarcTable.ScriptMarc21("",
                                strMARC,
                                "title_area",
                                null,
                                out results,
                                out strError);
                        else if (strBiblioSyntax == "unimarc")
                            nRet = MarcTable.ScriptUnimarc("",
                                strMARC,
                                "title_area",
                                null,
                                out results,
                                out strError);
                        else
                            throw new Exception($"未知的 MARC 格式 '{strBiblioSyntax}'");

                        if (results != null && results.Count > 0)
                            summary = results[0].Value;
                    }


                    nRet = MarcUtil.Xml2Marc(new_xml,    // info.OldXml,
    true,
    null,
    out string strMarcSyntax,
    out strMARC,
    out strError);
                    if (nRet == -1)
                    {
                        strError = "XML转换到MARC记录时出错: " + strError;
                        goto ERROR1;
                    }
                    MarcRecord record = new MarcRecord(strMARC);
                    var record_changed = false;
                    // 2024/7/26
                    // 执行保存前的预处理操作
                    {
                        nRet = genData.InitialAutogenAssembly(strBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (genData.DetailHostObj != null)
                        {
                            BeforeSaveRecordEventArgs e = new BeforeSaveRecordEventArgs();
                            e.SaveAction = "save";
                            e.SourceRecPath = "";
                            e.TargetRecPath = strBiblioRecPath;
                            this.TryInvoke(() =>
                            {
                                genData.DetailHostObj.Invoke("BeforeSaveRecord",
                                    record,
                                    e);
                            });
                            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                            {
                                strError = e.ErrorInfo;
                                goto ERROR1;
                            }
                            if (e.Changed)
                                record_changed = true;
                        }
                    }

                    // 2024/8/6
                    if (string.IsNullOrEmpty(filterCode) == false)
                    {
                        string old_marc = record.Text;
                        var ret = ScriptDialog.FilterRecord(
    cacheKey,
    filterCode,
    "",
    record,
    (o) =>
    {
        o.ClearParameter();
        o.Table = new Hashtable();
    },
    ref host,
    out string error);
                        if (ret < 0)
                        {
                            strError = $"脚本处理 {strBiblioRecPath} 过程中出错: {error}";
                            goto ERROR1;
                        }
                        if (host != null
    && host.VerifyResult != null
    && host.VerifyResult.Errors != null
    && host.VerifyResult.Errors.Count > 0
    && VerifyError.GetErrorCount(host.VerifyResult.Errors) > 0)
                        {
                            strError = VerifyError.BuildTextLines(host.VerifyResult.Errors);
                            strError = $"脚本处理 {strBiblioRecPath} 过程中出错: {strError}";
                            goto ERROR1;
                        }

                        bool skip_import = false;
                        if (host != null
    && host.Table != null
    && host.Table.ContainsKey("skipImport")
    )
                        {
                            skip_import = (bool)host.Table["skipExport"];
                        }

                        if (skip_import == true)
                            continue;

                        if (old_marc != record.Text)
                            record_changed = true;
                    }


                    if (this.InputMode880 == true
                        && (this.InputMarcSyntax == "usmarc" || this.InputMarcSyntax == "<自动>"))
                    {
                        MarcQuery.ToParallel(record);
                        record_changed = true;
                    }

                    // 2022/10/25
                    if (string.IsNullOrEmpty(batchNo) == false)
                    {
                        if (batchNo == "[清除]")
                            record.select("field[@name='998']/subfield[@name='a']").detach();
                        else
                            record.setFirstSubfield("998", "a", batchNo);
                        record_changed = true;
                    }

                    /*
                    // 2024/7/27
                    if (string.IsNullOrEmpty(source) == false)
                    {
                        if (source == "[清除]")
                            record.select("field[@name='998']/subfield[@name='f']").detach();
                        else
                            record.setFirstSubfield("998", "f", source);
                        record_changed = true;
                    }
                    */

                    // 2024/7/31
                    if (string.IsNullOrEmpty(operator_string) == false)
                    {
                        if (operator_string == "[清除]")
                        {
                            record.select("field[@name='998']/subfield[@name='z']").detach();
                            // 把操作时间也连带清除
                            record.select("field[@name='998']/subfield[@name='u']").detach();
                        }
                        else
                            record.setFirstSubfield("998", "z", operator_string);
                        record_changed = true;
                    }

                    if (record_changed)
                    {
                        nRet = MarcUtil.Marc2XmlEx(record.Text,
        strMarcSyntax,
        ref new_xml,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    if (dup_form != null
                        && overwrite == false)
                    {
                        // form.Activate();
                        var result = await dup_form.DoSearchAsync(dup_project_name,
                            strBiblioRecPath,
                            new_xml);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            goto ERROR1;
                        }

                        if (dup_form.GetDupCount() > 0)
                        {
                            if (dont_import_dup)
                            {
                                // TODO: 增加题名与责任者显示
                                strError = $"记录 {(i + 1)}) {summary} 因为重复而没有导入";
                                Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strError) + "</div>");
                                GetErrorInfoForm().WriteHtml(strError + "\r\n");
                                continue;
                            }

                            // 为 998$s 写入重复状态
                            {
                                MarcRecord temp = new MarcRecord(strMARC);
                                var old_value = temp.select($"field[@name='998']/subfield[@name='s']").FirstContent;
                                StringUtil.SetInList(ref old_value, "重", true);
                                temp.setFirstSubfield("998", "s", old_value);
                                nRet = MarcUtil.Marc2Xml(temp.Text,
                strBiblioSyntax,
                out new_xml,
                out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }
                        }
                    }

                    //

                    lRet = channel.SetBiblioInfo(
                        looping.Progress,
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

                if (dup_form != null)
                {
                    this.TryInvoke(() =>
                    {
                        dup_form.Close();
                        dup_form = null;
                    });
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
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ExceptionUtil.GetDebugText(ex)
                };
            }
            finally
            {
                looping.Dispose();
                /*
                EnableControls(true);

                this.ReturnChannel(channel);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
    + $" 结束导入 MARC 文件 {strInputFileName}</div>");

                if (file != null)
                    file.Close();

                if (dup_form != null)
                {
                    this.TryInvoke(() =>
                    {
                        dup_form.Close();
                        dup_form = null;
                    });
                }

                genData.Dispose();
                host?.Dispose();
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
                if (string.IsNullOrEmpty(this.InputFileName) == true)
                {
                    strError = "尚未指定输入的 ISO2709 文件名";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(this.InputEncodingName) == true)
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

        public override void UpdateEnable(bool bEnable)
        {
            this._openMarcFileDialog.MainPanel.Enabled = bEnable;

            if (bEnable == false)
                this.comboBox_targetDbName.Enabled = false;
            else
                ChangeTargetDbNameEnabled();

            // this.comboBox_targetDbName.Enabled = this.checkBox_overwriteByG01.Checked == true ? false : bEnable;

            this.button_next.Enabled = bEnable;
        }

        private void ImportMarcForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */
        }

        private void checkBox_overwriteByG01_CheckedChanged(object sender, EventArgs e)
        {
            ChangeTargetDbNameEnabled();
        }

        private void comboBox_targetDbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            BeginListProjectNames();
        }

        private void tabComboBox_dupProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = this.tabComboBox_dupProject.Text;
            if (string.IsNullOrEmpty(name)
                || name == "[不查重]")
                this.checkBox_dontImportDupRecords.Visible = false;
            else
                this.checkBox_dontImportDupRecords.Visible = true;
        }

        private void checkBox_lockTargetDbName_CheckedChanged(object sender, EventArgs e)
        {
            ChangeTargetDbNameEnabled();
        }

        void ChangeTargetDbNameEnabled()
        {
            if (this.checkBox_overwriteByG01.Checked
                || this.checkBox_lockTargetDbName.Checked)
                this.comboBox_targetDbName.Enabled = false;
            else
                this.comboBox_targetDbName.Enabled = true;
        }

        // 存放过滤脚本 .cs 文件的子目录
        public string FilterScriptDirectory { get; set; }

        public void PrepareScriptDirectory()
        {
            this.FilterScriptDirectory = Path.Combine(Program.MainForm.UserDir, "import_marc_scripts");
            PathUtil.CreateDirIfNeed(this.FilterScriptDirectory);
        }

        private void button_biblio_findFilterScriptFileName_Click(object sender, EventArgs e)
        {
            using (ScriptDialog dlg = new ScriptDialog())
            {
                dlg.Font = this.Font;
                dlg.FilterScriptDirectory = this.FilterScriptDirectory;
                dlg.ScriptFileName = this.textBox_biblio_filterScriptFileName.Text;
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.LinkFormState(dlg, "impor_marc_script_dialog");
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;
                this.textBox_biblio_filterScriptFileName.Text = dlg.ScriptFileName;
            }
        }
    }
}
