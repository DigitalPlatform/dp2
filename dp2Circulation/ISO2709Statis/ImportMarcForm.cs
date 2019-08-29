using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

            if (string.IsNullOrEmpty(this.InputEncodingName) == true)
            {
                return new NormalResult(-1, "尚未选定 ISO2709 文件的编码方式");
            }

            if (StringUtil.IsNumber(this.InputEncodingName) == true)
                encoding = Encoding.GetEncoding(Convert.ToInt32(this.InputEncodingName));
            else
                encoding = Encoding.GetEncoding(this.InputEncodingName);

            ClearErrorInfoForm();

            string strInputFileName = "";

            strInputFileName = this.InputFileName;

            string strBiblioSyntax = Program.MainForm.GetBiblioSyntax(strTargetDbName);
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

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取ISO2709记录 ...");
            stop.BeginLoop();

            bool dont_display_dialog = false;

            LibraryChannel channel = this.GetChannel();

            EnableControls(false);

            try
            {
                int nCount = 0;

                DialogResult retry_result = DialogResult.Yes;

                for (int i = 0; ; i++)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (stop != null && stop.State != 0)
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

                        stop.Continue(); // 继续循环
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
                            "读入MARC记录(" + nCount.ToString() + ")出错: " + strError + "\r\n\r\n确实要中断当前批处理操作?",
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
                            strError = "读入MARC记录(" + nCount.ToString() + ")出错: " + strError;
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
                        stop.SetMessage("跳过第 " + (i + 1).ToString() + " 个 ISO2709 记录");
                        continue;
                    }
                    else
                        stop.SetMessage("正在获取第 " + (i + 1).ToString() + " 个 ISO2709 记录");

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

                    // 处理
                    string strBiblioRecPath = strTargetDbName + "/?";

                    nRet = MarcUtil.Marc2Xml(strMARC,
                        strBiblioSyntax,
                        out XmlDocument domMarc,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    REDO:
                    long lRet = channel.SetBiblioInfo(
                        stop,
    "new",
    strBiblioRecPath,
    "xml",
    domMarc.DocumentElement.OuterXml,
    null,   // timestamp
    "",
    out string strOutputBiblioRecPath,
    out byte[] baNewTimestamp,
    out strError);
                    if (lRet == -1)
                    {
#if NO
                        strError = "创建书目记录 '" + strBiblioRecPath + "' 时出错: " + strError + "\r\n";
                        goto ERROR1;
#endif

                        // string strText = strError;
                        if (dont_display_dialog == false)
                        {
                            retry_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                            {
                            // return AutoCloseMessageBox.Show(this, strText + "\r\n\r\n(点右上角关闭按钮可以中断批处理)", 5000);
                            return MessageDlg.Show(this,
    strError + ", 是否重试？\r\n---\r\n\r\n[重试]重试; [跳过]跳过本条继续后面批处理; [中断]中断批处理",
    "ImportMarcForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button1,
    ref dont_display_dialog,
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

                    nCount++;
                }

                return new NormalResult();
            }
            finally
            {
                EnableControls(true);

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (file != null)
                    file.Close();
            }
            ERROR1:
            return new NormalResult(-1, strError);
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

                if (String.IsNullOrEmpty(strTargetDbName) == true)
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
                }
                finally
                {
                    this.Running = false;
                }

                this.tabControl_main.SelectedTab = this.tabPage_runImport;
                this.ShowMessageBox("导入完成。");
                return;
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
            this.Invoke((Action)(() =>
            {
                this._openMarcFileDialog.MainPanel.Enabled = bEnable;

                this.comboBox_targetDbName.Enabled = bEnable;

                this.button_next.Enabled = bEnable;
            }));
        }

        private void ImportMarcForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);
        }
    }
}
