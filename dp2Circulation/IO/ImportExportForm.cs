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
using DigitalPlatform.Text;
using Newtonsoft.Json;
using DigitalPlatform.Range;

namespace dp2Circulation
{
    public partial class ImportExportForm : MyForm
    {
        StringMapDialog _mapDialog = new StringMapDialog();

        public ImportExportForm()
        {
            InitializeComponent();

            this.panel_map.Controls.Add(_mapDialog.ListView);
            _mapDialog.ListView.Dock = DockStyle.Fill;
        }

        private void ImportExportForm_Load(object sender, EventArgs e)
        {
            FillBiblioDbNameList();

            this.UiState = Program.MainForm.AppInfo.GetString(
        "ImportExportForm",
        "uiState",
        "");

            {
                string strStringTable = Program.MainForm.AppInfo.GetString(
            "ImportExportForm",
            "stringTable",
            "");
                this._mapDialog.StringTable = JsonConvert.DeserializeObject<List<TwoString>>(strStringTable);
            }

            this._mapDialog.Show();  // 有了此句对话框的 xxx_load 才能被执行
            this._mapDialog.Hide();  // 有了此句可避免主窗口背后显示一个空对话框窗口
        }

        private void ImportExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportExportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MainForm.AppInfo.SetString(
"ImportExportForm",
"uiState",
this.UiState);

            {
                string strStringTable = JsonConvert.SerializeObject(this._mapDialog.StringTable);

                Program.MainForm.AppInfo.SetString(
"ImportExportForm",
"stringTable",
strStringTable);
            }

            this._mapDialog.Close();
        }

        /// <summary>
        /// 最近使用过的书目转储文件全路径
        /// </summary>
        public string BiblioDumpFilePath { get; set; }

        public static void EnableTabPage(TabPage page, bool bEnable)
        {
            foreach (Control control in page.Controls)
            {
                control.Enabled = bEnable;
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // this.tabControl_main.Enabled = bEnable;
            EnableTabPage(this.tabPage_source, bEnable);
            EnableTabPage(this.tabPage_convert, bEnable);
            EnableTabPage(this.tabPage_target, bEnable);
            // tabPage_run 不要禁止

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
            else if (this.tabControl_main.SelectedTab == this.tabPage_convert)
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

                this.tabControl_main.SelectedTab = this.tabPage_convert;
                this.button_next.Enabled = true;
                this.comboBox_target_targetBiblioDbName.Focus();
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_convert)
            {
                this.tabControl_main.SelectedTab = this.tabPage_target;
                this.button_next.Enabled = true;
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
                Task.Factory.StartNew(() => DoImport(""));
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

        // 准备和判断范围有关的成员变量
        void PrepareRange(ProcessInfo info)
        {
            info.BiblioRecCount = 0;
            info.RangeList = null;
            info.StartBiblioRecPath = "";

            if (String.IsNullOrEmpty(info.RecordRange) == false)
            {
                if (info.RecordRange.IndexOf("/") != -1)
                {
                    info.StartBiblioRecPath = info.RecordRange;
                    info.Start = false;
                }
                else
                {
                    info.RangeList = new RangeList(info.RecordRange);
                    info.RangeList.Sort();
                    info.Start = true;
                }
            }
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
        // parameters:
        //      strStyle    空/simulate/collect 之一
        void DoImport(string strStyle)
        {
            string strError = "";
            bool bRet = false;

            ProcessInfo info = new ProcessInfo();
            {
                info.Channel = this.GetChannel();
                info.stop = stop;

                info.TargetBiblioDbName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.comboBox_target_targetBiblioDbName.Text;
                }));
                info.RandomItemBarcode = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return checkBox_target_randomItemBarcode.Checked;
                }));
                info.AddBiblioToItem = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_convert_addBiblioToItem.Checked;
                }));
                info.OverwriteBiblio = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_restoreOldID.Checked;
                }));

                info.DontChangeOperations = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_dontChangeOperations.Checked;
                }));
                info.SuppressOperLog = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_suppressOperLog.Checked;
                }));
                info.DontSearchDup = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_dontSearchDup.Checked;
                }));

                info.RecordRange = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_source_range.Text;
                }));
#if NO
            info.Simulate = (bool)this.Invoke(new Func<bool>(() =>
            {
                return this.checkBox_target_simulate.Checked;
            }));
#endif
                if (strStyle == "collect")
                {
                    info.Collect = true;

                    info.LocationMapTable = new List<TwoString>();  // 不适用转换表
                }
                else
                {
                    if (strStyle == "simulate")
                        info.Simulate = true;

                    this.Invoke((Action)(() =>
                    info.LocationMapTable = this._mapDialog.StringTable
                    ));
                }

            }

            this._locationTable.Clear();
            this._itemBarcodeTable.Clear();

            this.ClearHtml();

            // 自动切换到信息显示页
            if (info.Collect == false)
            {
                this.Invoke((Action)(() =>
        this.tabControl_main.SelectedTab = this.tabPage_run
        ));
            }

            this.Invoke((Action)(() =>
this.MainForm.ActivateFixPage("history")
    ));

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));

            string strText = "正在从书目转储文件导入数据 ...";
            if (info.Simulate)
                strText = ("正在从书目转储文件模拟导入数据 ...");
            else if (info.Collect)
                strText = ("正在从书目转储文件搜集信息 ...");

            WriteHtml(strText + "\r\n");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strText);
            stop.BeginLoop();

            TimeSpan old_timeout = info.Channel.Timeout;
            info.Channel.Timeout = new TimeSpan(0, 2, 0);

            int nBiblioRecordCount = 0;

            PrepareRange(info);

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
                            break;	// 结束

                        DoRecord(reader, info);

                        if (stop != null)
                            stop.SetProgressValue(file.Position);

                        info.BiblioRecCount++;
                    }
                }

                string strOperName = "导入";
                if (info.Collect)
                {
                    strOperName = "搜集数据";
                    // GetLocationList(this._locationTable);
                }
                else if (info.Simulate)
                    strOperName = "模拟导入";

                if (info.ItemErrorCount > 0)
                {
                    strError = strOperName + "完成。共发生 " + info.ItemErrorCount + " 次错误。详情请见固定面板的操作历史属性页";
                    this.Invoke((Action)(() =>
                    this.MainForm.ActivateFixPage("history")
                        ));
                    goto ERROR1;
                }
                this.ShowMessage(strOperName + "完成", "green", true);
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
                stop.Style = StopStyle.None;

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
        void DoRecord(XmlTextReader reader,
            ProcessInfo info)
        {
            info.ClearRecordVars();

            if (info.RangeList != null && info.RangeList.IsInRange(info.BiblioRecCount, true) == false)
            {
                reader.ReadOuterXml();
                return;
            }

            bool bSkip = false;

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
                        // return:
                        //      true    书目记录已经处理。需要继续处理后面的下级记录
                        //      false   书目记录不需要处理，应跳过后面的下级记录
                        bSkip = !DoBiblio(reader, info);
                    }
                    else if (reader.LocalName == "orderCollection"
                        || reader.LocalName == "itemCollection"
                        || reader.LocalName == "issueCollection"
                        || reader.LocalName == "commentCollection")
                    {
                        if (bSkip)
                            reader.ReadOuterXml();
                        else
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
            return strPath.Substring(nRet + 1);
        }

        bool NeedSkip(XmlElement biblio_root, ProcessInfo info)
        {
            if (info.Start == false
                && string.IsNullOrEmpty(info.StartBiblioRecPath) == false)
            {
                string strOldPath = biblio_root.GetAttribute("path");

                // 定位第一条书目记录，并进入开始处理状态
                if (info.Start == false && GetShortPath(strOldPath) == info.StartBiblioRecPath)
                {
                    info.Start = true;
                    return false;
                }

                if (info.Start == false)
                    return true;
            }

            return false;
        }

        /*
        <dprms:biblio path="net.pipe://localhost/dp2library/xe?中文图书/10" timestamp="c95606aac8ecd2080000000000000000">
            <unimarc:record xmlns:dprms="http://dp2003.com/dprms" xmlns:unimarc="http://dp2003.com/UNIMARC">
                <unimarc:leader>00827nam0 2200229   45  </unimarc:leader>
                <unimarc:controlfield tag="001">0192000006</unimarc:controlfield>
         * */
        // return:
        //      true    书目记录已经处理。需要继续处理后面的下级记录
        //      false   书目记录不需要处理，应跳过后面的下级记录
        bool DoBiblio(XmlTextReader reader,
            ProcessInfo info)
        {
#if NO
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
#endif
            XmlDocument dom = new XmlDocument();
            XmlElement root = dom.ReadNode(reader) as XmlElement;

            if (NeedSkip(root, info) == true)
                return false;

            string strOldPath = root.GetAttribute("path");

            string strTimestamp = root.GetAttribute("timestamp");

            info.BiblioXml = root.InnerXml;

            info.SourceBiblioRecPath = strOldPath;

            // TODO: 检查 MARC 格式是否和目标书目库吻合

            string strAction = "";
            string strPath = "";

            if (info.OverwriteBiblio == true)
            {
                strAction = "new"; //  "change";
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

#if NO
            string strMessage = strOldPath + "-->" + strPath;
            if (info.Collect)
                strMessage = strOldPath;
            if (info.Simulate)
                strMessage = "模拟导入 " + strOldPath + "-->" + strPath;
#endif

            // WriteHtml(strMessage + "\r\n");

            // 2016/12/22
            List<string> styles = new List<string>();
            if (info.DontChangeOperations)
                styles.Add("nooperations");
            if (info.DontSearchDup)
                styles.Add("nocheckdup");
            if (info.SuppressOperLog)
                styles.Add("noeventlog");
            string strStyle = StringUtil.MakePathList(styles);

            if (info.Collect == true)
            {
                string strMessage = "采集数据 '" + strOldPath + "'";

                this.ShowMessage(strMessage);
                this.OutputText(strMessage, 0);
            }
            else
            {
                int nRedoCount = 0;
            REDO:
                // 创建或者覆盖书目记录
                string strError = "";
                string strOutputPath = "";
                byte[] baNewTimestamp = null;
                long lRet = info.Channel.SetBiblioInfo(
        info.stop,
        info.Simulate ? "simulate_" + strAction : strAction,
        strPath,
        "xml",
        info.BiblioXml,
        ByteArray.GetTimeStampByteArray(strTimestamp),
        "",
        strStyle,
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

                    if (info.HideBiblioMessageBox == false || nRedoCount > 10)
                    {
                        DialogResult result = MessageDialog.Show(this,
        strError + "\r\n\r\n(重试) 重试操作;(跳过) 跳过本条继续处理后面的书目记录; (中断) 中断处理",
        MessageBoxButtons.YesNoCancel,
        info.LastBiblioDialogResult == DialogResult.Yes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2,
        "此后不再出现本对话框",
        ref info.HideBiblioMessageBox,
        new string[] { "重试", "跳过", "中断" });
                        info.LastBiblioDialogResult = result;
                        if (result == DialogResult.Yes)
                        {
#if NO
                            // 为重试保存做准备。TODO: 理论上要显示对比，避免不经意覆盖了别人的修改
                            if (baNewTimestamp != null)
                                strTimestamp = ByteArray.GetHexTimeStampString(baNewTimestamp);
#endif
                            info.HideBiblioMessageBox = false;
                            nRedoCount++;
                            goto REDO;
                        }
                        if (result == DialogResult.No)
                            return false;
                        throw new Exception(strError);
                    }
                    else
                    {
                        if (info.LastBiblioDialogResult == DialogResult.Yes)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        // 跳过
                        if (info.LastBiblioDialogResult == DialogResult.No)
                            return false;
                        throw new Exception(strError);
                    }
                }

                info.BiblioRecPath = strOutputPath;

                string strMessage = strOldPath + "-->" + info.BiblioRecPath;
                if (info.Simulate)
                    strMessage = "模拟导入 " + strOldPath + "-->" + info.BiblioRecPath;

                this.ShowMessage(strMessage);
                this.OutputText(strMessage, 0);
            }

            return true;
        }

        static int ITEM_BATCH_SIZE = 10;

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
                        if (item_xmls.Count >= ITEM_BATCH_SIZE)
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

        static void RandomItemBarcode(XmlDocument item_dom)
        {
            string strItemBarcode = DomUtil.GetElementText(item_dom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(strItemBarcode) == false)
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "barcode",
                    strItemBarcode + "_" + Guid.NewGuid().ToString());
        }

        static bool AddBiblioToItem(XmlDocument item_dom, string strBiblioXml)
        {
            if (string.IsNullOrEmpty(strBiblioXml))
                throw new ArgumentException("strBiblioXml 值不应为空", "strBiblioXml");

            XmlElement biblio = item_dom.DocumentElement.SelectSingleNode("biblio") as XmlElement;
            if (biblio != null)
                return false;
            biblio = item_dom.CreateElement("biblio");
            item_dom.DocumentElement.AppendChild(biblio);
            biblio.InnerXml = strBiblioXml;
            return true;
        }

        Hashtable _locationTable = new Hashtable(); // location string --> count

        void DoItems(List<string> item_xmls, ProcessInfo info)
        {
            string strError = "";

            StringBuilder dupInfo = new StringBuilder();

            List<EntityInfo> entityArray = new List<EntityInfo>();
            string strRootElementName = "";

            // 2016/12/22
            List<string> styles = new List<string>();
            if (info.DontChangeOperations)
                styles.Add("nooperations");
            if (info.DontSearchDup)
                styles.Add("nocheckdup");
            if (info.SuppressOperLog)
                styles.Add("noeventlog");
            string strStyle = StringUtil.MakePathList(styles);

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

                    if (info.RandomItemBarcode)
                        RandomItemBarcode(item_dom);
                    else
                    {
                        // 针对册条码号进行文件空间内查重
                        SearchDupOnFileScope(item_dom, dupInfo);
                    }

                    string strLocation1 = DomUtil.GetElementText(item_dom.DocumentElement, "location");

                    string strPureLocation = StringUtil.GetPureLocation(strLocation1);

                    if (info.Collect)
                    {
                        object o = _locationTable[strPureLocation];
                        if (o == null)
                            _locationTable[strPureLocation] = 1;
                        else
                        {
                            int v = (int)o;
                            v++;
                            _locationTable[strPureLocation] = v;
                        }
                    }

                    if (info.Collect == false)
                    {
                        bool bRet = ConvertLocation(info.LocationMapTable, ref strPureLocation);
                        if (bRet == true)
                        {
                            DomUtil.SetElementText(item_dom.DocumentElement,
                                "location",
                                StringUtil.SetLocationString(strLocation1, strPureLocation));
                        }
                    }

                    if (info.AddBiblioToItem)
                        AddBiblioToItem(item_dom, info.BiblioXml);

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

                if (info.Simulate)
                    item.Style = "simulate";

                item.NewRecord = strXml;
                item.NewTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);

                item.OldRecord = "";
                item.OldTimestamp = null;

                item.Style = strStyle;

                entityArray.Add(item);
            }

            if (info.Collect)
                info.stop.SetMessage("正在为书目记录 '" + info.SourceBiblioRecPath + "' 搜集 " + info.UploadedSubItems + "+" + entityArray.Count + " 个下属 " + strRootElementName + " 记录的信息 ...");
            else
                info.stop.SetMessage("正在为书目记录 '" + info.BiblioRecPath + "' " +
                    (info.Simulate ? "模拟" : "") +
                    "上传 " + info.UploadedSubItems + "+" + entityArray.Count + " 个下属 " + strRootElementName + " 记录 ...");

            info.UploadedSubItems += entityArray.Count;

            if (info.Collect == false)
            {
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

            if (dupInfo.Length > 0)
            {
                strError = info.SourceBiblioRecPath + ":\r\n" + dupInfo.ToString();
                this.OutputText(strError, 2);

                if (AskContinue(info, strError) == false)
                    throw new Exception(strError);
            }
        }

        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            base.OutputText(strText, nWarningLevel);
            if (nWarningLevel == 2)
                WriteHtml(strText + "\r\n");
        }

        Hashtable _itemBarcodeTable = new Hashtable();  // itemBarcode --> count

        void SearchDupOnFileScope(XmlDocument item_dom, StringBuilder errorInfo)
        {
            string strItemBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(strItemBarcode) == true)
                return;
            object o = _itemBarcodeTable[strItemBarcode];
            if (o == null)
            {
                _itemBarcodeTable[strItemBarcode] = 1;
                return;
            }

            int v = (int)o;
            v++;
            _itemBarcodeTable[strItemBarcode] = v;

            errorInfo.Append("册条码号 '" + strItemBarcode + "' 在源文件中发生重复(出现 " + v + " 次)");
        }

        static bool ConvertLocation(List<TwoString> table, ref string location)
        {
            foreach (TwoString s in table)
            {
                if (s.Source == location)
                {
                    location = s.Target;
                    return true;
                }
            }

            return false;
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

            if (info.HideItemsMessageBox == false)
            {
                // TODO: 按钮文字较长的时候，应该能自动适应
                DialogResult result = MessageDialog.Show(this,
strText + "\r\n\r\n(继续) 继续处理; (中断) 中断处理",
MessageBoxButtons.YesNo,
MessageBoxDefaultButton.Button2,
"此后不再出现本对话框",
ref info.HideItemsMessageBox,
new string[] { "继续", "中断" });
                if (result == DialogResult.Yes)
                    return true;
                return false;
            }
            return true;
        }

        class ProcessInfo
        {
            // 是否为模拟导入
            public bool Simulate = false;

            // 是否为搜集数据
            public bool Collect = false;    // 搜集数据时，不向 dp2library 发出请求

            // 是否为册条码号加上随机的后缀字符串
            public bool RandomItemBarcode = false;

            // 是否为册记录自动添加书目元素。(注：如果册记录中本来有了这个元素就不添加了)
            public bool AddBiblioToItem = false;

            // 是否覆盖书目记录(恢复到原始 ID)。false 表示为追加
            public bool OverwriteBiblio = false;
            public string TargetBiblioDbName = "";  // 目标书目库名

            public bool DontChangeOperations = false;
            public bool SuppressOperLog = false;
            public bool DontSearchDup = false;

            public string RecordRange = ""; // 导入源文件中的书目记录范围

            public int ItemErrorCount = 0;  // 总共发生过多少次下属记录导入错误

            public LibraryChannel Channel = null;
            public Stop stop = null;

            // *** 以下成员都是在运行中动态设定和变化的
            public bool Start = true;   // 是否进入开始处理状态
            public string StartBiblioRecPath = "";  // 定位源文件中需开始处理的一条记录的路径
            public RangeList RangeList = null;
            public int BiblioRecCount = 0;  // 已经处理的书目记录数

            public string SourceBiblioRecPath = ""; // 数据中的书目记录路径

            public string BiblioRecPath = "";   // 当前已经创建或者修改的书目记录路径

            public string BiblioXml { get; set; }   // 书目记录 XML

            public Hashtable ItemRefIDTable = new Hashtable();  // 册记录 refID 替换情况表。旧 refID --> 新 refID 
            public Hashtable OrderRefIDTable = new Hashtable();  // 订购记录 refID 替换情况表。旧 refID --> 新 refID 

            public int UploadedSubItems = 0;    // 当前书目记录累计已经上传的子记录个数

            public bool HideItemsMessageBox = false; // 是否隐藏报错对话框
            public bool HideBiblioMessageBox = false; // 是否隐藏报错对话框
            public DialogResult LastBiblioDialogResult = DialogResult.No;   // 最近一次书目保存出错以后显示的对话框的选择结果

            // *** 以下变量在整个处理过程中持久
            public List<TwoString> LocationMapTable = new List<TwoString>(); // 馆藏地转换表

            // 每次要处理新的一条书目记录以前，进行清除
            public void ClearRecordVars()
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

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_source_fileName);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.comboBox_target_targetBiblioDbName);
                controls.Add(this.checkBox_target_randomItemBarcode);
                controls.Add(this.checkBox_convert_addBiblioToItem);
                controls.Add(this.checkBox_target_restoreOldID);

                controls.Add(this.checkBox_target_dontSearchDup);
                controls.Add(this.checkBox_target_suppressOperLog);
                controls.Add(this.checkBox_target_dontChangeOperations);
                controls.Add(this.textBox_source_range);

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_source_fileName);
                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.comboBox_target_targetBiblioDbName);
                controls.Add(this.checkBox_target_randomItemBarcode);
                controls.Add(this.checkBox_convert_addBiblioToItem);
                controls.Add(this.checkBox_target_restoreOldID);

                controls.Add(this.checkBox_target_dontSearchDup);
                controls.Add(this.checkBox_target_suppressOperLog);
                controls.Add(this.checkBox_target_dontChangeOperations);
                controls.Add(this.textBox_source_range);

                GuiState.SetUiState(controls, value);
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetNextButtonEnable();
        }

        private void button_target_simulateImport_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => DoImport("simulate"));
        }

#if NO
        List<TwoString> _map = null;

        private async void button_target_mapString_Click(object sender, EventArgs e)
        {
            // 按住 Ctrl 键可迫使重新创建对照表
            if (_map == null || (Control.ModifierKeys == Keys.Control))
            {
                await Task.Factory.StartNew(() => DoImport("collect"));

                _map = GetStringMap(this._locationTable);
            }

            StringMapDialog dlg = new StringMapDialog();
            dlg.StringTable = _map;
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this._map = dlg.StringTable;
        }
#endif

        // 从数据中获取数据，创建馆藏地转换表
        private async void button_convert_initialMapString_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() => DoImport("collect"));

            this._mapDialog.StringTable = GetStringMap(this._locationTable);
        }

        static List<string> GetLocationList(Hashtable locationTable)
        {
            List<string> lines = new List<string>();
            foreach (string key in locationTable.Keys)
            {
                lines.Add(key);
            }
            lines.Sort();
            return lines;
        }

        static List<TwoString> GetStringMap(Hashtable locationTable)
        {
            List<TwoString> results = new List<TwoString>();
            foreach (string key in locationTable.Keys)
            {
                TwoString item = new TwoString();
                item.Source = key;
                item.Target = key;  // 让源和目标一致，是预防用户使用空的目标造成意料之外的变换效果
                results.Add(item);
            }

            return results;
        }

        /// <summary>
        /// 写入 HTML 字符串
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void WriteHtml(string strHtml)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser1,
                    strHtml);
                webBrowser1.ScrollToEnd();
            }));
        }

        public void ClearHtml()
        {
            this.Invoke((Action)(() =>
            {
                Global.ClearForPureTextOutputing(this.webBrowser1);
            }));
        }

#if NO
        private void button_target_collect_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => DoImport("collect"));
        }
#endif
    }
}
