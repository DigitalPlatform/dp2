using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;
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
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_patronXmlFileName);
                controls.Add(this.comboBox_appendMode);
                controls.Add(this.comboBox_targetDbName);
                controls.Add(this.checkBox_refreshRefID);
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

            this.button_begin.Enabled = bEnable;
            this.button_stop.Enabled = !bEnable;
        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            ProcessInfo info = new ProcessInfo();
            info.Channel = this.GetChannel();
            info.stop = stop;
            info.TargetDbName = this.comboBox_targetDbName.Text;
            info.AppendMode = this.comboBox_appendMode.Text;
            info.NewRefID = (bool)this.Invoke(new Func<bool>(() =>
            {
                return this.checkBox_refreshRefID.Checked;
            }));
            info.RestoreMode = (bool)this.Invoke(new Func<bool>(() =>
            {
                return this.checkBox_restoreMode.Checked;
            }));

            var strSourceFileName = this.textBox_patronXmlFileName.Text;

            this.Invoke((Action)(() =>
    EnableControls(false)
    ));

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

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_stop_Click(object sender, EventArgs e)
        {

        }

        class ProcessInfo
        {
            public string TargetDbName { get; set; }
            public string AppendMode { get; set; }

            public bool NewRefID { get; set; }
            public bool RestoreMode { get; set; }

            public LibraryChannel Channel = null;
            public Stop stop = null;

            public long PatronRecCount { get; set; }
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

            /*
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            */

            string strOldPath = root.GetAttribute("path", DpNs.dprms);
            string strTimestamp = root.GetAttribute("timestamp", DpNs.dprms);

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
                if (info.RestoreMode == true)
                    strAction = "forcechange";
                else
                    strAction = "change";
                strTargetRecPath = info.TargetDbName + "/" + Global.GetRecordID(strPath);
                timestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
            }

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
                throw new Exception(strError);
        }

        private void comboBox_targetDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_targetDbName.Items.Count > 0)
                return;

            if (Program.MainForm.ReaderDbNames != null)
            {
                foreach (var name in Program.MainForm.ReaderDbNames)
                {
                    this.comboBox_targetDbName.Items.Add(name);
                }
            }
        }
    }
}
