using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    public partial class ImportPatronForm : MyForm
    {
        public ImportPatronForm()
        {
            InitializeComponent();
        }

        private void ImportPatronForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                this.UiState = Program.MainForm.AppInfo.GetString(
"ImportPatronForm",
"ui_state",
"");
            }

            checkBox_object_CheckedChanged(sender, e);
        }

        private void ImportPatronForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportPatronForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
    "ImportPatronForm",
    "ui_state",
    this.UiState);
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_patronXmlFileName);
                controls.Add(this.comboBox_appendMode);
                controls.Add(this.comboBox_targetDbName);
                controls.Add(this.checkBox_refreshRefID);
                controls.Add(this.checkBox_object);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.checkBox_autoPostfix);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_patronXmlFileName);
                controls.Add(this.comboBox_appendMode);
                controls.Add(this.comboBox_targetDbName);
                controls.Add(this.checkBox_refreshRefID);
                controls.Add(this.checkBox_object);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.checkBox_autoPostfix);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_getFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的读者 XML 文件名";
            dlg.FileName = this.textBox_patronXmlFileName.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "读者 XML 文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_patronXmlFileName.Text = dlg.FileName;
        }

        public override void EnableControls(bool bEnable)
        {
            this.textBox_patronXmlFileName.Enabled = bEnable;
            this.comboBox_appendMode.Enabled = bEnable;
            this.comboBox_targetDbName.Enabled = bEnable;

            this.checkBox_refreshRefID.Enabled = bEnable;
            this.checkBox_restoreMode.Enabled = bEnable;
            this.checkBox_autoPostfix.Enabled = bEnable;

            // 2021/12/19
            this.checkBox_object.Enabled = bEnable;
            this.textBox_objectDirectoryName.Enabled = bEnable;
            this.button_getObjectDirectoryName.Enabled = bEnable;

            this.button_begin.Enabled = bEnable;
            this.button_stop.Enabled = !bEnable;
        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.checkBox_object.Checked
    && string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text))
            {
                strError = "尚未指定对象目录";
                goto ERROR1;
            }

            ProcessInfo info = new ProcessInfo();
            {
                info.Channel = this.GetChannel();
                info.stop = stop;
                info.TargetDbName = this.comboBox_targetDbName.Text;
                info.AppendMode = this.comboBox_appendMode.Text;
                info.NewRefID = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_refreshRefID.Checked;
                }));
                info.AutoPostfix = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_autoPostfix.Checked;
                }));
                info.RestoreMode = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_restoreMode.Checked;
                }));
                info.IncludeSubObjects = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_object.Checked;
                }));
                info.ObjectDirectoryName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_objectDirectoryName.Text;
                }));

                if ((info.TargetDbName == "<覆盖到原有路径>"
                    && info.AppendMode != "覆盖到原有路径")
                    ||
                    (info.TargetDbName != "<覆盖到原有路径>"
                    && info.AppendMode == "覆盖到原有路径"))
                {
                    strError = "导入方式和目标库，只要其中一个为“覆盖到原有路径”，另一个也必须是“覆盖到原有路径”";
                    goto ERROR1;
                }
            }

            // TODO: 根据危险性出现提示
            // 如果是 覆盖到原有路径，最好先把所有路径统计一下，然后显示出来
            // 建议做一个把 XML 文件导入读者查询窗内存的功能，便于查看其中的每一条读者记录
            if (info.TargetDbName == "<覆盖到原有路径>")
            {
                DialogResult result = MessageBox.Show(this,
    "您现在采用的是“覆盖到原有路径”导入方式，将采用读者 XML 文件中每条记录记载的原有路径来导入，假设文件中包含了不同读者库的记录，那么会分别导入到这些不同的读者库中的原有 ID 位置。\r\n\r\n确实要进行导入?",
    "BinaryResControl",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            var strSourceFileName = this.textBox_patronXmlFileName.Text;

            this.Invoke((Action)(() =>
    EnableControls(false)
    ));

            OutputText($"{DateTime.Now.ToString()} 开始导入读者 XML 记录", 0);

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入读者 XML 记录");
            stop.BeginLoop();

            TimeSpan old_timeout = info.Channel.Timeout;
            info.Channel.Timeout = new TimeSpan(0, 2, 0);

            try
            {
                // 用 FileStream 方式打开，主要是为了能在中途观察进度
                using (FileStream file = File.Open(strSourceFileName,
    FileMode.Open,
    FileAccess.Read))
                using (XmlTextReader reader = new XmlTextReader(file))
                {
                    if (stop != null)
                        stop.SetProgressRange(0, file.Length);

                    bool bRet = false;

                    // 到根元素
                    while (true)
                    {
                        bRet = reader.Read();
                        if (bRet == false)
                        {
                            strError = "没有根元素";
                            goto ERROR1;
                        }
                        if (reader.NodeType == XmlNodeType.Element)
                            break;
                    }

                    for (; ; )
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        // 到下一个 record 元素
                        while (true)
                        {
                            bRet = reader.Read();
                            if (bRet == false)
                                break;
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        if (bRet == false)
                            break;  // 结束

                        DoRecord(reader, info);

                        if (stop != null)
                            stop.SetProgressValue(file.Position);

                        info.PatronRecCount++;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                MainForm.WriteErrorLog($"导入读者 XML 记录时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                info.Channel.Timeout = old_timeout;
                this.ReturnChannel(info.Channel);

                OutputText($"{DateTime.Now.ToString()} 结束导入读者 XML 记录", 0);

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        string CollectRecPath(string strSourceFileName)
        {
            this.Invoke((Action)(() =>
EnableControls(false)
));

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在分析读者记录路径 ...");
            stop.BeginLoop();
            try
            {
                // 用 FileStream 方式打开，主要是为了能在中途观察进度
                using (FileStream file = File.Open(strSourceFileName,
    FileMode.Open,
    FileAccess.Read))
                using (XmlTextReader reader = new XmlTextReader(file))
                {
                    if (stop != null)
                        stop.SetProgressRange(0, file.Length);

                    bool bRet = false;

                    // 到根元素
                    while (true)
                    {
                        bRet = reader.Read();
                        if (bRet == false)
                            throw new Exception("没有根元素");

                        if (reader.NodeType == XmlNodeType.Element)
                            break;
                    }

                    for (; ; )
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        // 到下一个 record 元素
                        while (true)
                        {
                            bRet = reader.Read();
                            if (bRet == false)
                                break;
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        if (bRet == false)
                            break;  // 结束

                        DoRecord(reader, info);

                        if (stop != null)
                            stop.SetProgressValue(file.Position);

                        info.PatronRecCount++;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                MainForm.WriteErrorLog($"导入读者 XML 记录时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
            }
        }
#endif

        private void button_stop_Click(object sender, EventArgs e)
        {

        }

        class ProcessInfo
        {
            public string TargetDbName { get; set; }
            public string AppendMode { get; set; }

            public bool NewRefID { get; set; }
            public bool RestoreMode { get; set; }
            public bool AutoPostfix { get; set; }

            public LibraryChannel Channel = null;
            public Stop stop = null;

            public long PatronRecCount { get; set; }

            public bool IncludeSubObjects = true;
            public string ObjectDirectoryName = "";

            // 2021/9/14
            // 最近一次读者保存出错后对话框选择的结果
            public DialogResult patron_retry_result = DialogResult.Yes;
            // 是否选择了不出现读者保存出错对话框(按照上次的选择结果自动处理)
            public bool patron_dont_display_retry_dialog = false;


            // 最近一次对象保存出错后对话框选择的结果
            public DialogResult object_retry_result = DialogResult.Yes;
            // 是否选择了不出现实体保存出错对话框(按照上次的选择结果自动处理)
            public bool object_dont_display_retry_dialog = false;
        }

        static string GetShortPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return "";
            int nRet = strPath.IndexOf("?");
            if (nRet == -1)
                return strPath;
            return strPath.Substring(nRet + 1);
        }

        // 处理一个 dprms:record 元素
        void DoRecord(
            XmlTextReader reader,
            ProcessInfo info)
        {
            /*
            info.ClearRecordVars();

            if (info.RangeList != null && info.RangeList.IsInRange(info.BiblioRecCount, true) == false)
            {
                reader.ReadOuterXml();
                return;
            }
            */

            XmlDocument dom = new XmlDocument();
            XmlElement root = dom.ReadNode(reader) as XmlElement;

            if (info.NewRefID)
                DomUtil.SetElementText(root, "refID", Guid.NewGuid().ToString());

            var barcode = DomUtil.GetElementText(root, "barcode");
            var name = DomUtil.GetElementText(root, "name");

            /*
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            */

            string strOldPath = root.GetAttribute("path", DpNs.dprms);
            string strTimestamp = root.GetAttribute("timestamp", DpNs.dprms);

            root.RemoveAttribute("path", DpNs.dprms);
            root.RemoveAttribute("timestamp", DpNs.dprms);

            string strPath = GetShortPath(strOldPath);

            string strAction = "";
            string strTargetRecPath = "";
            byte[] timestamp = null;
            if (info.AppendMode == "追加")
            {
                if (info.RestoreMode == true)
                    strAction = "forcenew";
                else
                    strAction = "new";
                strTargetRecPath = info.TargetDbName + "/?";
            }
            else
            {
                // 覆盖
                if (info.RestoreMode == true)
                    strAction = "forcechange";
                else
                    strAction = "change";

                if (info.TargetDbName == "<覆盖到原有路径>")
                {
                    Debug.Assert(info.AppendMode == "覆盖到原有路径");
                    strTargetRecPath = strPath;
                }
                else
                    strTargetRecPath = info.TargetDbName + "/" + Global.GetRecordID(strPath);
                timestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
            }

            OutputText($"{strPath}-->{strTargetRecPath}", 0);

        REDO:
            long lRet = info.Channel.SetReaderInfo(
    stop,
    strAction,
    strTargetRecPath,
    root.OuterXml,
    null,
    timestamp,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedPath,
    out byte[] baNewTimestamp,
    out ErrorCodeValue kernel_errorcode,
    out string strError);
            if (lRet == -1)
            {
                if (info.Channel.ErrorCode == ErrorCode.ReaderBarcodeDup
                    && info.AutoPostfix == true)
                {
                    // 自动加后缀
                    barcode = barcode + "_" + Guid.NewGuid().ToString();
                    DomUtil.SetElementText(root, "barcode", barcode);
                    OutputText($"册条码号发生重复，尝试添加后缀({barcode})并重新写入记录", 1);
                    goto REDO;
                }

                // 2021/9/14
                // 这里很有可能是通讯错误
                if (info.patron_dont_display_retry_dialog == false)
                {
                    string error = $"{strAction} 读者记录 {strTargetRecPath} (证条码号:{barcode},姓名:{name}) 时出错: {strError}";
                    info.patron_retry_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                    {
                        return MessageDlg.Show(this,
error + ", 是否重试？\r\n---\r\n\r\n[重试]重试; [跳过]跳过本条继续后面批处理; [中断]中断批处理",
"ImportPatronForm",
MessageBoxButtons.YesNoCancel,
MessageBoxDefaultButton.Button1,
ref info.patron_dont_display_retry_dialog,
new string[] { "重试", "跳过", "中断" },
"后面不再出现此对话框，按本次选择自动处理");
                    }));
                }

                if (info.patron_retry_result == System.Windows.Forms.DialogResult.Cancel)
                    throw new ChannelException(info.Channel.ErrorCode, strError);
                if (info.patron_retry_result == DialogResult.Yes)
                    goto REDO;

                OutputText($"{strAction} 读者记录 {strTargetRecPath} (证条码号:{barcode},姓名:{name}) 时出错: {strError}", 2);
                return;
            }

            // 上传书目记录的数字对象
            if (info.IncludeSubObjects)
                UploadObjects(info, strSavedPath, strSavedXml);
        }

        /*
    <dprms:file id="0" xmlns:dprms="http://dp2003.com/dprms" _timestamp="9d4c3d9950a9d4080000000000000002" _metadataFile="a0b54269-1f2f-4750-911e-1e213f71b238.met" _objectFile="a0b54269-1f2f-4750-911e-1e213f71b238.bin" />
 * */
        // 上传数字对象
        void UploadObjects(ProcessInfo info,
            string strRecPath,
            string strXml)
        {
            if (string.IsNullOrEmpty(strXml))
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);  // info.BiblioXml

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement node in nodes)
            {
                string strObjectFile = node.GetAttribute("_objectFile");
                if (string.IsNullOrEmpty(strObjectFile))
                    continue;
                string strMetadataFile = node.GetAttribute("_metadataFile");
                if (string.IsNullOrEmpty(strMetadataFile))
                    continue;
                string strTimestamp = node.GetAttribute("_timestamp");
                if (string.IsNullOrEmpty(strTimestamp))
                    continue;
                string strID = node.GetAttribute("id");
                if (string.IsNullOrEmpty(strID))
                    continue;

                string strClientFilePath = Path.Combine(info.ObjectDirectoryName, strObjectFile);
                string strServerFilePath = strRecPath + "/object/" + strID;

                string strMetadata = "";
                using (StreamReader sr = new StreamReader(Path.Combine(info.ObjectDirectoryName, strMetadataFile)))
                {
                    strMetadata = sr.ReadToEnd();
                }

            REDO:
                // if (info.Simulate == false)
                {
                    // 上传文件到到 dp2lbrary 服务器
                    // parameters:
                    //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
                    //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
                    // return:
                    //		-1	出错
                    //		0   上传文件成功
                    int nRet = info.Channel.UploadFile(
                info.stop,
                strClientFilePath,
                strServerFilePath,
                strMetadata,
                "", // info.Simulate ? "simulate" : "",
                ByteArray.GetTimeStampByteArray(strTimestamp),
                true,
                out byte[] temp_timestamp,
                out string strError);
                    if (nRet == -1)
                    {
                        // 2021/9/14
                        if (info.object_dont_display_retry_dialog == false)
                        {
                            string error = strError;
                            info.object_retry_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                            {
                                return MessageDlg.Show(this,
        error + ", 是否重试？\r\n---\r\n\r\n[重试]重试; [跳过]跳过本条继续后面批处理; [中断]中断批处理",
        "ImportPatronForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxDefaultButton.Button1,
        ref info.object_dont_display_retry_dialog,
        new string[] { "重试", "跳过", "中断" },
        "后面不再出现此对话框，按本次选择自动处理");
                            }));
                        }

                        if (info.object_retry_result == System.Windows.Forms.DialogResult.Cancel)
                            throw new ChannelException(info.Channel.ErrorCode, strError);
                        if (info.object_retry_result == DialogResult.Yes)
                            goto REDO;

                        continue;

                        // throw new Exception(strError);  // TODO: 空对象不存在怎么办?
                    }
                }
            }
        }


        private void comboBox_targetDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_targetDbName.Items.Count > 0)
                return;

            this.comboBox_targetDbName.Items.Add("<覆盖到原有路径>");

            if (Program.MainForm.ReaderDbNames != null)
            {
                foreach (var name in Program.MainForm.ReaderDbNames)
                {
                    this.comboBox_targetDbName.Items.Add(name);
                }
            }
        }

        private void button_getObjectDirectoryName_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定对象文件所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            dir_dlg.SelectedPath = this.textBox_objectDirectoryName.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_objectDirectoryName.Text = dir_dlg.SelectedPath;
        }

        private void checkBox_object_CheckedChanged(object sender, EventArgs e)
        {
            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            this.textBox_objectDirectoryName.ReadOnly = !control;

            if (this.checkBox_object.Checked)
            {
                this.textBox_objectDirectoryName.Enabled = true;
                this.button_getObjectDirectoryName.Enabled = true;

                this.label_objectDirectoryName.Enabled = true;

                AutoBuildObjectDirectoryName(true);
            }
            else
            {
                this.textBox_objectDirectoryName.Enabled = false;
                this.button_getObjectDirectoryName.Enabled = false;

                this.label_objectDirectoryName.Enabled = false;

                this.textBox_objectDirectoryName.Text = "";
            }
        }

        private void textBox_patronXmlFileName_TextChanged(object sender, EventArgs e)
        {
            /*
            if (string.IsNullOrEmpty(this.textBox_patronXmlFileName.Text) == false)
                this.textBox_objectDirectoryName.Text = this.textBox_patronXmlFileName.Text + ".object";
            else
                this.textBox_objectDirectoryName.Text = "";
            */
            AutoBuildObjectDirectoryName(true);
        }

        void AutoBuildObjectDirectoryName(bool bForce)
        {
            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text)
                || bForce)
            {
                if (string.IsNullOrEmpty(this.textBox_patronXmlFileName.Text) == false)
                    this.textBox_objectDirectoryName.Text = this.textBox_patronXmlFileName.Text + ".object";
            }
        }

        private void comboBox_targetDbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_targetDbName.Text == "<覆盖到原有路径>")
                this.comboBox_appendMode.Text = "覆盖到原有路径";
        }

        private void comboBox_appendMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_appendMode.Text == "覆盖到原有路径")
                this.comboBox_targetDbName.Text = "<覆盖到原有路径>";
        }

        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            base.OutputText(strText, nWarningLevel);
            /*
            if (nWarningLevel == 2)
                WriteHtml(HttpUtility.HtmlEncode(strText) + "\r\n");
            */
        }
    }
}
