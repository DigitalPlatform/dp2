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
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class ImportExportForm : MyForm
    {
        public ImportExportForm()
        {
            InitializeComponent();
        }

        private void ImportExportForm_Load(object sender, EventArgs e)
        {
            FillBiblioDbNameList();
        }

        private void ImportExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportExportForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        /// <summary>
        /// 最近使用过的书目转储文件全路径
        /// </summary>
        public string BiblioDumpFilePath { get; set; }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.tabControl_main.Enabled = bEnable;

            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;
        }

        void SetNextButtonEnable()
        {
            // string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_target)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_run)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }
        }

        // 下一步
        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_source)
            {
                if (string.IsNullOrEmpty(this.textBox_source_fileName.Text))
                {
                    strError = "尚未指定书目转储文件名";
                    goto ERROR1;
                }

                if (this.checkBox_subRecords_object.Checked
                    && string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text))
                {
                    strError = "尚未指定对象目录";
                    goto ERROR1;
                }

                this.tabControl_main.SelectedTab = this.tabPage_target;
                this.button_next.Enabled = true;
                this.comboBox_target_targetBiblioDbName.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_target)
            {
                if (string.IsNullOrEmpty(this.comboBox_target_targetBiblioDbName.Text))
                {
                    strError = "尚未指定目标书目库";
                    goto ERROR1;
                }

                this.tabControl_main.SelectedTab = this.tabPage_run;
                this.button_next.Enabled = false;
                Task.Factory.StartNew(() => DoImport());
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_run)
            {
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

            this.SetNextButtonEnable();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /* 文件头部结构
<?xml version="1.0" encoding="utf-8"?>
<dprms:collection xmlns:dprms="http://dp2003.com/dprms">
    <dprms:record>
        <dprms:biblio path="net.pipe://localhost/dp2library/xe?中文图书/10" timestamp="c95606aac8ecd2080000000000000000">
            <unimarc:record xmlns:dprms="http://dp2003.com/dprms" xmlns:unimarc="http://dp2003.com/UNIMARC">
                <unimarc:leader>00827nam0 2200229   45  </unimarc:leader>
                <unimarc:controlfield tag="001">0192000006</unimarc:controlfield>
         ...
         * */
        // 在一个单独的线程中运行
        void DoImport()
        {
            string strError = "";
            bool bRet = false;


            this.Invoke((Action)(() =>
                EnableControls(false)
                ));

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在从书目转储文件导入数据 ...");
            stop.BeginLoop();

            ProcessInfo info = new ProcessInfo();
            info.Channel = this.GetChannel();
            info.stop = stop;
            info.OverwriteBiblio = false;
            info.TargetBiblioDbName = (string)this.Invoke(new Func<string>(() =>
            {
                return this.comboBox_target_targetBiblioDbName.Text;
            }));

            TimeSpan old_timeout = info.Channel.Timeout;
            info.Channel.Timeout = new TimeSpan(0, 2, 0);

            try
            {
                string strSourceFileName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_source_fileName.Text;
                }));

                // 用 FileStream 方式打开，主要是为了能在中途观察进度
                using (FileStream file = File.Open(strSourceFileName,
    FileMode.Open,
    FileAccess.Read))

                using (XmlTextReader reader = new XmlTextReader(file))
                {
                    if (stop != null)
                        stop.SetProgressRange(0, file.Length);

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
                            break;	// 结束

                        DoRecord(reader, info);
                    }
                }

                if (info.ItemErrorCount > 0)
                {
                    strError = "导入完成。共发生 " + info.ItemErrorCount + " 次错误。详情请见固定面板的操作历史属性页";
                    this.Invoke((Action)(() =>
                    this.MainForm.ActivateFixPage("history")
                        ));
                    goto ERROR1;
                }
                this.ShowMessage("导入完成", "green", true);
                return;
            }
            catch (Exception ex)
            {
                strError = "导入过程出现异常" + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                info.Channel.Timeout = old_timeout;
                this.ReturnChannel(info.Channel);

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
            }

        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
        }

        /* 结构
<dprms:record>
    <dprms:biblio path="net.pipe://localhost/dp2library/xe?中文图书/10" timestamp="c95606aac8ecd2080000000000000000">
 ...
    </dprms:biblio>
    <dprms:orderCollection>
            <dprms:order path="中文图书订购/1" timestamp="5bfd16621a18d3080000000000000003">
            ...
 * */
        // 处理一个 dprms:record 元素
        void DoRecord(XmlTextReader reader, ProcessInfo info)
        {
            info.Clear();

            // 对下级元素进行循环处理
            while (true)
            {
                bool bRet = reader.Read();
                if (bRet == false)
                    return;

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    Debug.Assert(reader.LocalName == "record" && reader.NamespaceURI == DpNs.dprms, "");
                    return;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    // biblio 元素
                    // 应当是同级元素中的第一个。因为后面写入册记录等需要知道书目记录的实际写入路径
                    if (reader.LocalName == "biblio")
                    {
                        DoBiblio(reader, info);
                    }
                    else if (reader.LocalName == "orderCollection"
                        || reader.LocalName == "itemCollection"
                        || reader.LocalName == "issueCollection"
                        || reader.LocalName == "commentCollection")
                    {
                        DoItemCollection(reader, info);
                    }
                    else
                    {
                        throw new Exception("无法识别的 dprms:record 下级元素名 '" + reader.Name + "'");
                    }
                }
            }
        }

        static string GetShortPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return "";
            int nRet = strPath.IndexOf("?");
            if (nRet == -1)
                return strPath;
            return strPath.Substring(nRet);
        }

        /*
        <dprms:biblio path="net.pipe://localhost/dp2library/xe?中文图书/10" timestamp="c95606aac8ecd2080000000000000000">
            <unimarc:record xmlns:dprms="http://dp2003.com/dprms" xmlns:unimarc="http://dp2003.com/UNIMARC">
                <unimarc:leader>00827nam0 2200229   45  </unimarc:leader>
                <unimarc:controlfield tag="001">0192000006</unimarc:controlfield>
         * */
        void DoBiblio(XmlTextReader reader, ProcessInfo info)
        {
            // info.ItemRefIDTable.Clear();
            string strOldPath = reader.GetAttribute("path");
            string strTimestamp = reader.GetAttribute("timestamp");

            // 到下级的 unimarc:record 元素
            while (true)
            {
                bool bRet = reader.Read();
                if (bRet == false)
                    return;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "record"
                        && (reader.NamespaceURI == DpNs.unimarcxml
                        || reader.NamespaceURI == Ns.usmarcxml))
                        break;
                }
            }

            string strBiblioXml = reader.ReadOuterXml();
            reader.ReadEndElement();    // 位置已经到了 dprms:biblio 后面的一个同级元素

            // TODO: 检查 MARC 格式是否和目标书目库吻合

            string strAction = "";
            string strPath = "";
            if (info.OverwriteBiblio == true)
            {
                strAction = "change";
                strPath = GetShortPath(strOldPath);
            }
            else
            {
                strAction = "new";
                if (info.TargetBiblioDbName == "<使用文件中的原书目库名>")
                {
                    // 注意 strPath 是长路径 "http://xxxx/dp2library?中文图书/1"
                    strPath = Global.GetDbName(GetShortPath(strOldPath)) + "/?";
                }
                else
                    strPath = info.TargetBiblioDbName + "/?";
            }

            string strMessage = strOldPath + "-->" + strPath;
            this.ShowMessage(strMessage);
            this.OutputText(strMessage, 0);

            // 创建或者覆盖书目记录
            string strError = "";
            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            long lRet = info.Channel.SetBiblioInfo(
    info.stop,
    strAction,
    strPath,
    "xml",
    strBiblioXml,
    ByteArray.GetTimeStampByteArray(strTimestamp),
    "",
    out strOutputPath,
    out baNewTimestamp,
    out strError);
            if (lRet == -1)
            {
                if (info.Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch)
                {
                    // 提示是否强行覆盖?
                }
                strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;
                throw new Exception(strError);
            }

            info.BiblioRecPath = strOutputPath;
        }

        void DoItemCollection(XmlTextReader reader, ProcessInfo info)
        {
            string strRootElementName = reader.Name;
            string strSubElementName = reader.LocalName.Replace("Collection", "");

            List<string> item_xmls = new List<string>();

            // 对下级元素进行循环处理
            while (true)
            {
                bool bRet = reader.Read();
                if (bRet == false)
                    break;

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    Debug.Assert(reader.Name == strRootElementName, "");
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    // xxx 元素
                    // 应当是同级元素中的第一个。因为后面写入册记录等需要知道书目记录的实际写入路径
                    if (reader.LocalName == strSubElementName
                        && reader.NamespaceURI == DpNs.dprms)
                    {
                        item_xmls.Add(reader.ReadOuterXml());
                        if (item_xmls.Count >= 10)
                        {
                            DoItems(item_xmls, info);
                            item_xmls.Clear();
                        }
                    }
                    else
                    {
                        // 越过不认识的当前元素
                        reader.ReadEndElement();
                    }
                }
            }

            // 最后一批
            if (item_xmls.Count > 0)
            {
                DoItems(item_xmls, info);
                item_xmls.Clear();
            }
        }

        static void RefreshRefID(Hashtable table, ref string strRefID)
        {
            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strRefID = Guid.NewGuid().ToString();
                return;
            }

            if (table == null)
                return;

            // 参考 ID 要替换
            string strNewRefID = (string)table[strRefID];
            if (string.IsNullOrEmpty(strNewRefID) == false)
            {
                strRefID = strNewRefID;
            }
            else
            {
                strNewRefID = Guid.NewGuid().ToString();
                table[strRefID] = strNewRefID;
                strRefID = strNewRefID;
            }
        }

        void DoItems(List<string> item_xmls, ProcessInfo info)
        {
            string strError = "";

            List<EntityInfo> entityArray = new List<EntityInfo>();
            string strRootElementName = "";

            foreach (string xml in item_xmls)
            {
                if (string.IsNullOrEmpty(xml))
                    continue;
                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(xml);

                strRootElementName = item_dom.DocumentElement.LocalName;

                string strPath = item_dom.DocumentElement.GetAttribute("path");
                string strTimestamp = item_dom.DocumentElement.GetAttribute("timestamp");

                EntityInfo item = new EntityInfo();

                string strRefID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");

                if (strRootElementName == "item")
                {
                    RefreshRefID(info.ItemRefIDTable, ref strRefID);
                }
                else if (strRootElementName == "order")
                {
                    RefreshRefID(info.OrderRefIDTable, ref strRefID);

                    // 记录中 distribute 元素中的 refid 要被替换
                    string strDistribute = DomUtil.GetElementText(item_dom.DocumentElement, "distribute");
                    if (string.IsNullOrEmpty(strDistribute) == false)
                    {
                        LocationCollection collection = new LocationCollection();
                        int nRet = collection.Build(strDistribute, out strError);
                        if (nRet != -1)
                        {
                            collection.RefreshRefIDs(ref info.ItemRefIDTable);
                        }
                        string strNewDistribute = collection.ToString();
                        if (strNewDistribute != strDistribute)
                        {
                            DomUtil.SetElementText(item_dom.DocumentElement, "distribute", strNewDistribute);
                        }
                    }
                }
                else
                {
                    RefreshRefID(null, ref strRefID);
                }

                item.RefID = strRefID;
                DomUtil.SetElementText(item_dom.DocumentElement, "refID", strRefID);

                DomUtil.SetElementText(item_dom.DocumentElement,
                    "parent", Global.GetRecordID(info.BiblioRecPath));

                string strXml = item_dom.DocumentElement.OuterXml;

                item.Action = "new";

                item.NewRecord = strXml;
                item.NewTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);

                item.OldRecord = "";
                item.OldTimestamp = null;

                entityArray.Add(item);
            }

            info.stop.SetMessage("正在为书目记录 '" + info.BiblioRecPath + "' 上传 "+info.UploadedSubItems+"+" + entityArray.Count + " 个下属 " + strRootElementName + " 记录 ...");

            info.UploadedSubItems += entityArray.Count;

            EntityInfo[] errorinfos = null;

            long lRet = 0;

            if (strRootElementName == "item")
                lRet = info.Channel.SetEntities(
                     info.stop,
                     info.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "order")
                lRet = info.Channel.SetOrders(
                     info.stop,
                     info.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "issue")
                lRet = info.Channel.SetIssues(
                     info.stop,
                     info.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "comment")
                lRet = info.Channel.SetComments(
                     info.stop,
                     info.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else
            {
                strError = "未知的 strRootElementName '" + strRootElementName + "'";
                throw new Exception(strError);
            }
            if (lRet == -1)
                throw new Exception(strError);

            if (errorinfos == null || errorinfos.Length == 0)
                return;

            StringBuilder text = new StringBuilder();
            foreach (EntityInfo error in errorinfos)
            {
                if (String.IsNullOrEmpty(error.RefID) == true)
                    throw new Exception("服务器返回的EntityInfo结构中RefID为空");

                // 正常信息处理
                if (error.ErrorCode == ErrorCodeValue.NoError)
                    continue;

                text.Append(error.RefID + "在提交保存过程中发生错误 -- " + error.ErrorInfo + "\r\n");
                info.ItemErrorCount++;
            }

            if (text.Length > 0)
            {
                strError = "在为书目记录 '" + info.BiblioRecPath + "' 导入下属 '" + strRootElementName + "' 记录的阶段出现错误:\r\n" + text.ToString();

                this.OutputText(strError, 2);

                // 询问是否忽略错误继续向后处理? 此后全部忽略?
                if (AskContinue(info, strError) == false)
                    throw new Exception(strError);
            }
        }

        // return:
        //      true    继续处理
        //      false   中断处理
        bool AskContinue(ProcessInfo info, string strText)
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<ProcessInfo, string, bool>(AskContinue), info, strText);
            }

            // 1) 继续处理。但遇到错误依然报错
            // 2) 继续处理，但遇到错误不再报错。此时要累积报错信息，最后统一报错。或者显示在一个浏览器控件窗口中。

            if (info.HideMessageBox == false)
            {
                // TODO: 按钮文字较长的时候，应该能自动适应
                DialogResult result = MessageDialog.Show(this,
strText + "\r\n\r\n(继续) 继续处理; (中断) 中断处理",
MessageBoxButtons.YesNo,
MessageBoxDefaultButton.Button2,
null,
ref info.HideMessageBox,
new string[] { "继续", "中断" });
                if (result == DialogResult.Yes)
                    return true;
                return false;
            }
            return true;
        }

        class ProcessInfo
        {
            // 是否覆盖书目记录。false 表示为追加
            public bool OverwriteBiblio = false;
            public string TargetBiblioDbName = "";  // 目标书目库名

            public bool HideMessageBox = false; // 是否隐藏报错对话框

            public int ItemErrorCount = 0;  // 总共发生过多少次下属记录导入错误

            public LibraryChannel Channel = null;
            public Stop stop = null;

            public string BiblioRecPath = "";   // 当前已经创建或者修改的书目记录路径

            public Hashtable ItemRefIDTable = new Hashtable();  // 册记录 refID 替换情况表。旧 refID --> 新 refID 
            public Hashtable OrderRefIDTable = new Hashtable();  // 订购记录 refID 替换情况表。旧 refID --> 新 refID 

            public int UploadedSubItems = 0;    // 当前书目记录累计已经上传的子记录个数

            // 每次要处理新的一条书目记录以前，进行清除
            public void Clear()
            {
                this.ItemRefIDTable.Clear();
                this.OrderRefIDTable.Clear();
                this.BiblioRecPath = "";
                this.UploadedSubItems = 0;
            }
        }

        private void button_source_findFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的书目转储文件名";
            dlg.FileName = this.textBox_source_fileName.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "书目转储文件 (*.bdf)|*.bdf|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_source_fileName.Text = dlg.FileName;

            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text))
                this.textBox_objectDirectoryName.Text = this.textBox_source_fileName.Text + ".object";

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

        private void checkBox_subRecords_object_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_subRecords_object.Checked)
            {
                this.textBox_objectDirectoryName.Enabled = true;
                this.button_getObjectDirectoryName.Enabled = true;
            }
            else
            {
                this.textBox_objectDirectoryName.Enabled = false;
                this.textBox_objectDirectoryName.Text = "";
                this.button_getObjectDirectoryName.Enabled = false;
            }
        }

        void FillBiblioDbNameList()
        {
            this.comboBox_target_targetBiblioDbName.Items.Clear();

            foreach (BiblioDbProperty prop in this.MainForm.BiblioDbProperties)
            {
                string strDbName = prop.DbName;
                if (string.IsNullOrEmpty(strDbName) == true)
                    continue;
                this.comboBox_target_targetBiblioDbName.Items.Add(strDbName);
            }

            this.comboBox_target_targetBiblioDbName.Items.Add("<使用文件中的原书目库名>");
        }
    }
}
