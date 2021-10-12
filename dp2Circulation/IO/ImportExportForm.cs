using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.Web;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;

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
            if (Control.ModifierKeys == Keys.Control)
                this.SpecialState = true;
            else
                this.SpecialState = false;

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

            checkBox_subRecords_object_CheckedChanged(sender, e);
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
                info.RandomItemRegisterNo = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return checkBox_target_randomItemRegisterNo.Checked;
                }));
                info.AddBiblioToItem = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_convert_addBiblioToItem.Checked;
                }));
                info.AddBiblioToItemOnMerging = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_convert_addBiblioToItemOnMerging.Checked;
                }));

                info.OverwriteBiblio = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_biblioRestoreOldID.Checked;
                }));



                info.DontChangeOperations = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_dontChangeOperations.Checked;
                }));
                info.SuppressOperLog = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_suppressOperLog.Checked;
                }));

                // 2021/8/27
                info.RestoreMode = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.comboBox_target_restore.Text;
                }));
                //2021/9/13
                if (string.IsNullOrEmpty(info.RestoreMode))
                    info.RestoreMode = "[不适用]";

                // 2021/8/31
                // 修正 OverwriteBiblio
                // 注：当只导入下级记录的时候，书目记录路径只能用原来的，无法用追加形态
                if (info.RestoreMode == "下级记录")
                    info.OverwriteBiblio = true;

                info.OverwriteSubrecord = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_subrecordRestoreOldID.Checked;
                }));

                info.DontSearchDup = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_dontSearchDup.Checked;
                }));
                info.AutoPostfix = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_target_autoPostfix.Checked;
                }));
                info.NewRefID = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_convert_refreshRefID.Checked;
                }));

                info.RecordRange = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_source_range.Text;
                }));

                info.ItemBatchNo = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_convert_itemBatchNo.Text;
                }));

                //
                info.IncludeSubItems = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_subRecords_entity.Checked;
                }));
                info.IncludeSubOrders = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_subRecords_order.Checked;
                }));
                info.IncludeSubIssues = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_subRecords_issue.Checked;
                }));
                info.IncludeSubComments = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_subRecords_comment.Checked;
                }));
                info.IncludeSubObjects = (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_subRecords_object.Checked;
                }));
                info.ObjectDirectoryName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_objectDirectoryName.Text;
                }));

                string strDbNameList = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_target_dbNameList.Text;
                }));
                info.AutoMergeRegistry = new MergeRegistry();
                info.AutoMergeRegistry.DbNames = StringUtil.SplitList(strDbNameList.Replace("\r\n", "\r"), '\r');
                // 这里验证一下书目库名的有效性
                foreach (string dbName in info.AutoMergeRegistry.DbNames)
                {
                    if (Program.MainForm.IsBiblioDbName(dbName) == false)
                    {
                        strError = "“自动选择目标数据库顺序”参数中的名字 '" + dbName + "' 不是当前服务器合法的书目库名";
                        goto ERROR1;
                    }
                }

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
            this._itemRegisterNoTable.Clear();

            this.ClearHtml();

            // 自动切换到信息显示页
            if (info.Collect == false)
            {
                this.Invoke((Action)(() =>
        this.tabControl_main.SelectedTab = this.tabPage_run
        ));
            }

            this.Invoke((Action)(() =>
Program.MainForm.ActivateFixPage("history")
    ));

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));

            string strText = "正在从书目转储文件导入数据 ...";
            if (info.Simulate)
                strText = ("正在从书目转储文件模拟导入数据 ...");
            else if (info.Collect)
                strText = ("正在从书目转储文件搜集信息 ...");

            WriteText(strText + "\r\n");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strText);
            stop.BeginLoop();

            TimeSpan old_timeout = info.Channel.Timeout;
            info.Channel.Timeout = new TimeSpan(0, 2, 0);

            // int nBiblioRecordCount = 0;

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
                    Program.MainForm.ActivateFixPage("history")
                        ));
                    goto ERROR1;
                }
                this.ShowMessage(strOperName + "完成", "green", true);
                return;
            }
            catch (InterruptException ex)
            {
                strError = ex.Message;
                goto ERROR1;
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
                    PrecessItemXmls(info);
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
                            DoItemCollection(reader, info); // 搜集 item xmls
                    }
                    else
                    {
                        throw new Exception("无法识别的 dprms:record 下级元素名 '" + reader.Name + "'");
                    }
                }
            }


        }

        void PrecessItemXmls(ProcessInfo info)
        {
            // 处理 item xmls
            if (info.ItemCollectionTable.Count > 0)
            {
                // 2017/6/5
                // 按照特定顺序处理
                {
                    List<string> item_xmls = (List<string>)info.ItemCollectionTable["item"];
                    if (item_xmls != null)
                    {
                        // 先把包含 binding 元素的册记录放在最后
                        item_xmls = AdjustBindingItems(item_xmls);

                        DoItems(item_xmls, info);
                    }
                }

                {
                    List<string> item_xmls = (List<string>)info.ItemCollectionTable["order"];
                    if (item_xmls != null)
                        DoItems(item_xmls, info);
                }

                {
                    List<string> item_xmls = (List<string>)info.ItemCollectionTable["issue"];
                    if (item_xmls != null)
                        DoItems(item_xmls, info);
                }

                {
                    List<string> item_xmls = (List<string>)info.ItemCollectionTable["comment"];
                    if (item_xmls != null)
                        DoItems(item_xmls, info);
                }

                info.ItemCollectionTable.Clear();
            }
        }

        static List<string> AdjustBindingItems(List<string> item_xmls)
        {
            List<string> normal_list = new List<string>();  // 普通 XML
            List<string> binding_list = new List<string>(); // 包含 binding/item 元素的 XML

            foreach (string xml in item_xmls)
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(xml);

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("binding/item");
                if (nodes.Count > 0)
                    binding_list.Add(xml);
                else
                    normal_list.Add(xml);
            }

            normal_list.AddRange(binding_list);
            return normal_list;
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
            if (this.SpecialState)
            {
                if (info.DontChangeOperations)
                    styles.Add("nooperations");
                if (info.DontSearchDup)
                    styles.Add("nocheckdup");
                if (info.SuppressOperLog)
                    styles.Add("noeventlog");
                /*
                if (info.RestoreMode.Contains("书目"))
                    styles.Add("force");
                */
            }
            string strStyle = StringUtil.MakePathList(styles);

            // 2021/8/27
            string restoreMode = info.RestoreMode;
            bool writeBiblio = restoreMode == "[不适用]" || (string.IsNullOrEmpty(restoreMode) == false && restoreMode.Contains("书目"));

            if (info.Collect == true
                || writeBiblio == false)
            {
                string strMessage = "采集数据 '" + strOldPath + "'";

                // 2021/8/27
                if (writeBiblio == false && info.Collect == false)
                {
                    info.BiblioRecPath = strPath;
                    strMessage = "越过写入 '" + strPath + "' (不影响写入下级记录)";
                }

                this.ShowMessage(strMessage);
                this.OutputText(strMessage, 0);
            }
            else
            {
                int nRedoCount = 0;
            REDO:
                // 创建或者覆盖书目记录
                string strError = "";
                long lRet = info.Channel.SetBiblioInfo(
        info.stop,
        info.Simulate ? "simulate_" + strAction : strAction,
        strPath,
        "xml",
        info.BiblioXml,
        ByteArray.GetTimeStampByteArray(strTimestamp),
        "",
        strStyle,
        out string strOutputPath,
        out byte[] baNewTimestamp,
        out strError);
                if (lRet == -1)
                {
                    if (info.Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch)
                    {
                        // 提示是否强行覆盖?
                    }

                    StringBuilder dup_message = new StringBuilder();
                    // info.MergeAction = "";
                    if (info.Channel.ErrorCode == ErrorCode.BiblioDup)
                    {
                        string strDialogAction = "";
                        string strTargetRecPath = "";
                        byte[] baTargetTimestamp = null;

                        this.Invoke((Action)(() =>
                        {
                            using (BiblioDupDialog dup_dialog = new BiblioDupDialog())
                            {
                                MainForm.SetControlFont(dup_dialog, this.Font, false);
                                dup_dialog.MergeStyle = info.UsedMergeStyle;    // 这个值会被持久存储

                                dup_dialog.Action = info.MergeAction;
                                dup_dialog.AutoMergeRegistry = info.AutoMergeRegistry;
                                dup_dialog.AutoSelectMode = info.AutoSelectMode;
                                dup_dialog.TempDir = Program.MainForm.UserTempDir;
                                dup_dialog.MarcHtmlHead = Program.MainForm.GetMarcHtmlHeadString();
                                dup_dialog.OriginXml = info.BiblioXml;
                                dup_dialog.DupBiblioRecPathList = strOutputPath;

                                Program.MainForm.AppInfo.LinkFormState(dup_dialog, "biblioDupDialog_state");
                                dup_dialog.UiState = Program.MainForm.AppInfo.GetString("ImportExportForm", "BiblioDupDialog_uiState", "");
                                dup_dialog.ShowDialog(this);
                                Program.MainForm.AppInfo.SetString("ImportExportForm", "BiblioDupDialog_uiState", dup_dialog.UiState);

                                info.AutoSelectMode = dup_dialog.AutoSelectMode;    // 记忆
                                info.UsedMergeStyle = dup_dialog.MergeStyle;
                                info.MergeAction = dup_dialog.Action;

                                strDialogAction = dup_dialog.Action;
                                strTargetRecPath = dup_dialog.SelectedRecPath;
                                baTargetTimestamp = dup_dialog.SelectedTimestamp;

                                dup_message.Append($"尝试导入书目记录({strOldPath}-->{strPath})时出现重复，然后经对话框选择合并策略:\r\n动作={strDialogAction}\r\n如何合并={info.UsedMergeStyle}\r\n目标记录路径={strTargetRecPath}\r\n自动选择={info.AutoSelectMode}");
                                this.OutputText(dup_message.ToString(), 1);
                            }
                        }));

                        if (string.IsNullOrEmpty(strDialogAction) == true
                            || strDialogAction == "stop")
                        {
                            throw new InterruptException("中断处理过程");
                        }
                        if (strDialogAction == "createNew")
                        {
                            // TODO: 最好出现一个 MARC 编辑器让操作者修改 MARC 记录
                            // 为 200$a 增加一个随机字符串部分，以保证再次提交保存不会遇到重复情况
                            string strBiblioXml = info.BiblioXml;
                            int nRet = ModifyTitle(ref strBiblioXml,
            out strError);
                            if (nRet == -1)
                                throw new Exception(strError);
                            info.BiblioXml = strBiblioXml;
                            goto REDO;
                        }

                        if (strDialogAction == "skip")
                        {
                            string strMessage = (info.BiblioRecCount + 1).ToString() + ":" + strOldPath + " 被跳过";
                            this.OutputText(strMessage, 0);
                            return false;
                        }
                        if (strDialogAction == "mergeTo")
                        {
                            // 合并源文件中的册到目标位置
                            Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");
                            strOutputPath = strTargetRecPath;
                            info.MergeAction = strDialogAction;
                            goto CONTINUE;
                        }
                        if (strDialogAction == "mergeToUseSourceBiblio")
                        {
                            // 合并源文件的册到目标位置，同时用源书目记录覆盖目标位置的书目记录
                            Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");

                            // 按照确定位置覆盖书目记录
                            // return:
                            //      -1  出错
                            //      0   跳过本条处理
                            //      1   成功。后面继续处理
                            int nRet = OverwriteBiblio(info,
                                // strAction,
                                strTargetRecPath,
                                strStyle,
                                baTargetTimestamp,
                                out strError);
#if NO
                            lRet = info.Channel.SetBiblioInfo(
info.stop,
info.Simulate ? "simulate_" + strAction : strAction,
strTargetRecPath,
"xml",
info.BiblioXml,
baTargetTimestamp,
"",
strStyle,
out strOutputPath,
out baNewTimestamp,
out strError);
                            if (lRet == -1)
                            {
                                // TODO: 如果这里报错了也应该有机会重做
                                throw new Exception(strError);
                            }
#endif
                            if (nRet == -1)
                                throw new Exception(strError);
                            if (nRet == 0)
                                return false;

                            info.MergeAction = strDialogAction;
                            goto CONTINUE;
                        }
                    }

                    strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;

                    if (info.HideBiblioMessageBox == false || nRedoCount > 10)
                    {
                        DialogResult result = System.Windows.Forms.DialogResult.Yes;
                        this.Invoke((Action)(() =>
                        {
                            result = MessageDialog.Show(this,
                        strError + "\r\n\r\n(重试) 重试操作;(跳过) 跳过本条继续处理后面的书目记录; (中断) 中断处理",
                        MessageBoxButtons.YesNoCancel,
                        info.LastBiblioDialogResult == DialogResult.Yes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2,
                        "此后不再出现本对话框",
                        ref info.HideBiblioMessageBox,
                        new string[] { "重试", "跳过", "中断" });
                        }));

                        info.LastBiblioDialogResult = result;
                        if (result == DialogResult.Yes)
                        {
                            // 为重试保存做准备。TODO: 理论上要显示对比，避免不经意覆盖了别人的修改
                            if (baNewTimestamp != null)
                                strTimestamp = ByteArray.GetHexTimeStampString(baNewTimestamp);

                            info.HideBiblioMessageBox = false;
                            nRedoCount++;
                            goto REDO;
                        }
                        if (result == DialogResult.No)
                            return false;
                        throw new InterruptException(strError);
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

            CONTINUE:
                info.BiblioRecPath = strOutputPath;

                {
                    string strMessage = (info.BiblioRecCount + 1).ToString() + ":" + strOldPath + "-->" + info.BiblioRecPath;
                    if (info.Simulate)
                        strMessage = "模拟导入 " + (info.BiblioRecCount + 1).ToString() + ":" + strOldPath + "-->" + info.BiblioRecPath;

                    this.ShowMessage(strMessage);
                    this.OutputText(strMessage, 0);
                }
            }

            // 上传书目记录的数字对象
            if (info.IncludeSubObjects
                && info.Collect == false
                && writeBiblio == true)
                UploadObjects(info, info.BiblioRecPath, info.BiblioXml);

            return true;
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
                info.Simulate ? "simulate" : "",
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
        "ImportMarcForm",
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

        // 修改书目记录的题名，增加一个随机字符串部分
        static int ModifyTitle(ref string strBiblioXml,
            out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(strBiblioXml) == true)
                return 0;

            string strMarcSyntax = "";
            string strMarc = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strBiblioXml) == false)
            {
                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	== true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                nRet = MarcUtil.Xml2Marc(strBiblioXml,
                    true,
                    "", // this.CurMarcSyntax,
                    out strMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (string.IsNullOrEmpty(strMarcSyntax) == true)
                return 0;   // 不是 MARC 格式

            bool bChanged = false;

            MarcRecord record = new MarcRecord(strMarc);
            if (strMarcSyntax == "unimarc")
            {
                string strValue = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                strValue += "_" + NewGuid().ToString();
                record.setFirstSubfield("200", "a", strValue);
                bChanged = true;
            }
            else if (strMarcSyntax == "usmarc")
            {
                // TODO: 其实需要把子字段内容最末一个符号字符后移
                string strValue = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                strValue += "_" + NewGuid().ToString();
                record.setFirstSubfield("245", "a", strValue);
                bChanged = true;
            }

            if (bChanged == true)
            {
                nRet = MarcUtil.Marc2XmlEx(record.Text,
                    strMarcSyntax,
                    ref strBiblioXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 1;
            }

            return 0;
        }

        // 按照确定位置覆盖书目记录
        // 函数内处理了报错和重试的情况
        // return:
        //      -1  出错
        //      0   跳过本条处理
        //      1   成功。后面继续处理
        int OverwriteBiblio(ProcessInfo info,
            // string strAction,
            string strTargetRecPath,
            string strStyle,
            byte[] baTargetTimestamp,
            out string strError)
        {
            strError = "";

            string strAction = "change";

            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            int nRedoCount = 0;
        REDO:
            long lRet = info.Channel.SetBiblioInfo(
info.stop,
info.Simulate ? "simulate_" + strAction : strAction,
strTargetRecPath,
"xml",
info.BiblioXml,
baTargetTimestamp,
"",
strStyle + ",bibliotoitem",
out strOutputPath,
out baNewTimestamp,
out strError);
            if (lRet == -1)
            {
                strError = "保存书目记录 '" + strTargetRecPath + "' 时出错: " + strError;

                if (info.HideBiblioMessageBox == false || nRedoCount > 10)
                {
                    DialogResult result = System.Windows.Forms.DialogResult.Yes;
                    string strText = strError;
                    this.Invoke((Action)(() =>
{
    result = MessageDialog.Show(this,
strText + "\r\n\r\n(重试) 重试操作;(跳过) 跳过本条继续处理后面的书目记录; (中断) 中断处理",
MessageBoxButtons.YesNoCancel,
info.LastBiblioDialogResult == DialogResult.Yes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2,
"此后不再出现本对话框",
ref info.HideBiblioMessageBox,
new string[] { "重试", "跳过", "中断" });
}));
                    info.LastBiblioDialogResult = result;
                    if (result == DialogResult.Yes)
                    {
                        // 为重试保存做准备。TODO: 理论上要显示对比，避免不经意覆盖了别人的修改
                        if (baNewTimestamp != null)
                            baTargetTimestamp = baNewTimestamp;

                        info.HideBiblioMessageBox = false;
                        nRedoCount++;
                        goto REDO;
                    }
                    if (result == DialogResult.No)
                        return 0;
                    return -1;
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
                        return 0;
                    return -1;
                }
            }

            return 1;
        }

#if NO
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
#endif

        void DoItemCollection(XmlTextReader reader, ProcessInfo info)
        {
            string strRootElementName = reader.Name;
            string strSubElementName = reader.LocalName.Replace("Collection", "");

            List<string> item_xmls = new List<string>();

            if (reader.IsEmptyElement == false) // 防范 <dprms:itemCollection /> 这种情况
            {
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
                        }
                        else
                        {
                            // 越过不认识的当前元素
                            if (reader.IsEmptyElement == false)
                                reader.ReadEndElement();
                        }
                    }
                }
            }

            List<string> existing = (List<string>)info.ItemCollectionTable[strSubElementName];
            if (existing == null)
                info.ItemCollectionTable[strSubElementName] = item_xmls;
            else
                existing.AddRange(item_xmls);
        }

        // return:
        //      false   没有发生填充
        //      true    发生了填充
        static bool FillEmptyRefID(ref string strRefID)
        {
            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strRefID = NewRefID().ToString();
                return true;
            }

            return false;
        }

        static string NewGuid()
        {
            return ShortGuid.NewGuid();
        }

        static string NewRefID()
        {
            return Guid.NewGuid().ToString();
        }

        static void RefreshRefID(Hashtable table, ref string strRefID)
        {
#if NO
            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strRefID = NewGuid().ToString();
                return;
            }
#endif
            if (FillEmptyRefID(ref strRefID) == true)
                return;

            if (table == null)
            {
                strRefID = NewRefID().ToString();    // 2018/12/1
                return;
            }

            // 参考 ID 要替换
            string strNewRefID = (string)table[strRefID];
            if (string.IsNullOrEmpty(strNewRefID) == false)
            {
                strRefID = strNewRefID;
            }
            else
            {
                strNewRefID = NewRefID().ToString();
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
                    strItemBarcode + "_" + NewGuid().ToString().ToUpper());
        }

        static void RandomItemRegisterNo(XmlDocument item_dom)
        {
            string strRegisterNo = DomUtil.GetElementText(item_dom.DocumentElement,
                "registerNo");
            if (string.IsNullOrEmpty(strRegisterNo) == false)
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "registerNo",
                    strRegisterNo + "_" + NewGuid().ToString().ToUpper());
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
            int nRet = 0;

            StringBuilder dupInfo = new StringBuilder();

            List<EntityInfo> entityArray = new List<EntityInfo>();
            string strRootElementName = "";

            // 2016/12/22
            List<string> styles = new List<string>();
            // 要在这些 checkbox 可见的情况下，才起作用
            if (this.SpecialState)
            {
                if (info.DontChangeOperations)
                    styles.Add("nooperations");
                if (info.DontSearchDup)
                    styles.Add("nocheckdup");
                if (info.SuppressOperLog)
                    styles.Add("noeventlog");
                if (info.RestoreMode != null
                    && info.RestoreMode.Contains("下级记录"))
                    styles.Add("force");
            }

            // 2017/1/4
            if (info.Simulate)
                styles.Add("simulate");

            // 2018/12/1
            if (info.AutoPostfix)
                styles.Add("autopostfix");

            string strStyle = StringUtil.MakePathList(styles);

            foreach (string xml in item_xmls)
            {
                if (string.IsNullOrEmpty(xml))
                    continue;
                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(xml);

                strRootElementName = item_dom.DocumentElement.LocalName;

                if (info.IncludeSubItems == false && strRootElementName == "item")
                    continue;
                if (info.IncludeSubOrders == false && strRootElementName == "order")
                    continue;
                if (info.IncludeSubIssues == false && strRootElementName == "issue")
                    continue;
                if (info.IncludeSubComments == false && strRootElementName == "comment")
                    continue;

                string strPath = item_dom.DocumentElement.GetAttribute("path");
                string strTimestamp = item_dom.DocumentElement.GetAttribute("timestamp");

                EntityInfo item = new EntityInfo();

                string strRefID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");

                string strNewPath = "";
                if (info.OverwriteSubrecord == true)
                {
                    string dbName = Global.GetDbName(GetShortPath(strPath));
                    if (string.IsNullOrEmpty(dbName))
                        throw new Exception($"下级记录在 bdf 文件中记载的原始路径为 '{strPath}'，无法获得库名部分");
                    string id = Global.GetRecordID(GetShortPath(strPath));
                    if (string.IsNullOrEmpty(id))
                        throw new Exception($"下级记录在 bdf 文件中记载的原始路径为 '{strPath}'，无法获得 ID 部分");

                    string current_dbName = Program.MainForm.GetItemDbName(Global.GetDbName(info.BiblioRecPath));
                    if (dbName != current_dbName)
                    {
                        throw new Exception($"下级记录在 bdf 文件中记载的原始路径为 '{strPath}'，其中的库名部分 '{dbName}' 和即将导入的库名 '{current_dbName}' 不吻合，因此无法实现覆盖回原始 ID 的效果 ");
                    }
                    strNewPath = current_dbName + "/" + id;
                }
                else
                {
                    /*
                    // 注意 strPath 是长路径 "http://xxxx/dp2library?中文图书实体/1"
                    strNewPath = Global.GetDbName(GetShortPath(strPath)) + "/?";
                    */
                }

                if (strRootElementName == "item")
                {
                    if (info.NewRefID)
                    {
                        RefreshRefID(info.ItemRefIDTable, ref strRefID);

                        // 更换<binding>元素内<item>元素的refID属性值
                        // TODO: 含有 binding 元素的册记录应该在最后统一替换其内部的 refid。不过，一般情况下，含有 binding 的册记录会自然排列在其他册记录的后面，这是由装订流程的特点造成的
                        {
                            XmlNodeList nodes = item_dom.DocumentElement.SelectNodes("binding");
                            foreach (XmlElement node in nodes)
                            {
                                nRet = BookItem.ReplaceBindingItemRefID(info.ItemRefIDTable,
                        node,
                        out strError);
                                if (nRet == -1)
                                    throw new Exception(strError);
                            }
                        }
                    }

                    if (info.RandomItemBarcode)
                        RandomItemBarcode(item_dom);
                    else
                    {
                        // 针对册条码号进行文件空间内查重
                        SearchItemBarcodeDupOnFileScope(item_dom, dupInfo);
                    }

                    if (info.RandomItemRegisterNo)
                        RandomItemRegisterNo(item_dom);
                    else
                    {
                        // 针对登录号进行文件空间内查重
                        SearchItemRegisterNoDupOnFileScope(item_dom, dupInfo);
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

                    if (info.AddBiblioToItem
                        || (info.AddBiblioToItemOnMerging == true && info.MergeAction.StartsWith("mergeTo"))
                        )
                        AddBiblioToItem(item_dom, info.BiblioXml);

                    if (string.IsNullOrEmpty(info.ItemBatchNo) == false)
                        DomUtil.SetElementText(item_dom.DocumentElement,
                            "batchNo",
                            info.ItemBatchNo);

                }
                else if (strRootElementName == "order")
                {
                    if (info.NewRefID)
                    {
                        RefreshRefID(info.OrderRefIDTable, ref strRefID);

                        // 记录中 distribute 元素中的 refid 要被替换
                        string strDistribute = DomUtil.GetElementText(item_dom.DocumentElement, "distribute");
                        if (string.IsNullOrEmpty(strDistribute) == false)
                        {
                            LocationCollection collection = new LocationCollection();
                            nRet = collection.Build(strDistribute, out strError);
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
                }
                else if (strRootElementName == "issue")
                {
                    if (info.NewRefID)
                    {
                        RefreshRefID(info.IssueRefIDTable, ref strRefID);

                        // TODO: 要确保册记录和订购记录先替换 refid
                        // orderInfo/root/distribute
                        XmlNodeList nodes = item_dom.DocumentElement.SelectNodes("orderInfo");
                        foreach (XmlElement node in nodes)
                        {
                            // 更换 orderInfo 元素里的 distribute 元素中的 refid 字符串
                            // return:
                            //      -1  出错
                            //      >=0 发生替换的个数
                            nRet = IssueItem.ReplaceOrderInfoItemRefID(info.ItemRefIDTable,
                                node,
                                out strError);
                            if (nRet == -1)
                                throw new Exception(strError);

                            // 更换 orderInfo 元素里的 refID 元素中的 参考 ID 字符串
                            // return:
                            //      -1  出错
                            //      >=0 发生替换的个数
                            nRet = IssueItem.ReplaceOrderInfoRefID(info.OrderRefIDTable,
                    node,
                    out strError);
                            if (nRet == -1)
                                throw new Exception(strError);
                        }
                    }
                }
                else
                {
                    if (info.NewRefID)
                        RefreshRefID(null, ref strRefID);
                }

                // 2017/6/13
                // 即便不要求替换 refid，也要确保当 refid 为空的时候发生它
                if (string.IsNullOrEmpty(strRefID))
                    FillEmptyRefID(ref strRefID);

                Debug.Assert(string.IsNullOrEmpty(strRefID) == false, "");

                item.RefID = strRefID;
                DomUtil.SetElementText(item_dom.DocumentElement, "refID", strRefID);

                // 2021/8/31
                var parent_id = Global.GetRecordID(info.BiblioRecPath);
                // 检查 ID 是否合法
                if (StringUtil.IsPureNumber(parent_id) == false)
                    throw new Exception($"书目记录 '{info.BiblioRecPath}' 中的 ID 部分 '{parent_id}' 不合法，应为纯数字");

                DomUtil.SetElementText(item_dom.DocumentElement,
                    "parent", parent_id);

                string strXml = item_dom.DocumentElement.OuterXml;

                item.Action = "new";

                item.NewRecPath = strNewPath;
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

            // 2021/8/27
            string restoreMode = info.RestoreMode;
            bool writeSubrecords = restoreMode == "[不适用]" || (string.IsNullOrEmpty(restoreMode) == false && restoreMode.Contains("下级记录"));

            if (info.Collect == false
                && entityArray.Count > 0
                && writeSubrecords == true)
            {
                WriteEntities(
    info,
    strRootElementName,
    entityArray,
    out strError);
            }

            if (dupInfo.Length > 0)
            {
                strError = info.SourceBiblioRecPath + ":\r\n" + dupInfo.ToString();
                this.OutputText(strError, 2);

                if (AskContinue(info, strError) == false)
                    throw new Exception(strError);
            }
        }

        internal static EntityInfo[] GetPart(List<EntityInfo> source,
int nStart,
int nCount)
        {
            EntityInfo[] result = new EntityInfo[nCount];
            for (int i = 0; i < nCount; i++)
            {
                result[i] = source[i + nStart];
            }
            return result;
        }

        void WriteEntities(
            ProcessInfo info,
            string strRootElementName,
            List<EntityInfo> entities,
            out string strError)
        {
            strError = "";

            // refid --> 记录路径
            Hashtable refid_table = new Hashtable();

            int nBatch = 100;
            for (int i = 0; i < (entities.Count / nBatch) + ((entities.Count % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, entities.Count - i * nBatch);
                EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                EntityInfo[] errorinfos = null;

                long lRet = 0;
            REDO:
                if (strRootElementName == "item")
                    lRet = info.Channel.SetEntities(
                         info.stop,
                         info.BiblioRecPath,
                         current.ToArray(),
                         out errorinfos,
                         out strError);
                else if (strRootElementName == "order")
                    lRet = info.Channel.SetOrders(
                         info.stop,
                         info.BiblioRecPath,
                         current.ToArray(),
                         out errorinfos,
                         out strError);
                else if (strRootElementName == "issue")
                    lRet = info.Channel.SetIssues(
                         info.stop,
                         info.BiblioRecPath,
                         current.ToArray(),
                         out errorinfos,
                         out strError);
                else if (strRootElementName == "comment")
                    lRet = info.Channel.SetComments(
                         info.stop,
                         info.BiblioRecPath,
                         current.ToArray(),
                         out errorinfos,
                         out strError);
                else
                {
                    strError = "未知的 strRootElementName '" + strRootElementName + "'";
                    throw new Exception(strError);
                }
                if (lRet == -1)
                {
                    // 2021/9/14
                    // 这里很有可能是通讯错误
                    if (info.item_dont_display_retry_dialog == false)
                    {
                        string error = strError;
                        info.item_retry_result = (DialogResult)this.Invoke((Func<DialogResult>)(() =>
                        {
                            return MessageDlg.Show(this,
    error + ", 是否重试？\r\n---\r\n\r\n[重试]重试; [跳过]跳过本条继续后面批处理; [中断]中断批处理",
    "ImportMarcForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button1,
    ref info.item_dont_display_retry_dialog,
    new string[] { "重试", "跳过", "中断" },
    "后面不再出现此对话框，按本次选择自动处理");
                        }));
                    }

                    if (info.item_retry_result == System.Windows.Forms.DialogResult.Cancel)
                        throw new ChannelException(info.Channel.ErrorCode, strError);
                    if (info.item_retry_result == DialogResult.Yes)
                        goto REDO;

                    // throw new ChannelException(info.Channel.ErrorCode, strError);
                    continue;
                }

                if (errorinfos == null || errorinfos.Length == 0)
                    continue;

                // TODO: 建立保存成功的记录的 参考 ID 和记录路径的对照表

                StringBuilder text = new StringBuilder();
                foreach (EntityInfo error in errorinfos)
                {
                    if (String.IsNullOrEmpty(error.RefID) == true)
                        throw new Exception("服务器返回的EntityInfo结构中RefID为空");

                    // 正常信息处理
                    if (error.ErrorCode == ErrorCodeValue.NoError)
                    {
                        if (string.IsNullOrEmpty(error.RefID) == false
                            && string.IsNullOrEmpty(error.NewRecPath) == false)
                            refid_table[error.RefID] = error.NewRecPath;
                        continue;
                    }

                    text.Append(error.RefID + "在提交保存过程中发生错误 -- " + error.ErrorInfo + "\r\n");
                    info.ItemErrorCount++;
                }

                // 这里是每一条册记录保存中的具体报错。肯定不是通讯出错
                // 这里只能选择继续(也就是跳过的意思)或者中断。不提供“重试”的选项
                if (text.Length > 0)
                {
                    strError = "在为书目记录 '" + info.BiblioRecPath + "' 导入下属 '" + strRootElementName + "' 记录的阶段出现错误:\r\n" + text.ToString();

                    this.OutputText(strError, 2);

                    // 询问是否忽略错误继续向后处理? 此后全部忽略?
                    if (AskContinue(info, strError) == false)
                        throw new Exception(strError);
                }
            }

            // 上载对象
            foreach (EntityInfo item in entities)
            {
                // 上传下属记录的数字对象
                if (info.IncludeSubObjects)
                {
                    string strRecPath = (string)refid_table[item.RefID];
                    UploadObjects(info, strRecPath, item.NewRecord);
                }
            }

        }

        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            base.OutputText(strText, nWarningLevel);
            if (nWarningLevel == 2)
                WriteHtml(HttpUtility.HtmlEncode(strText) + "\r\n");
        }

        Hashtable _itemBarcodeTable = new Hashtable();  // itemBarcode --> count

        void SearchItemBarcodeDupOnFileScope(XmlDocument item_dom, StringBuilder errorInfo)
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

        // 2018/11/16
        Hashtable _itemRegisterNoTable = new Hashtable();  // registerNo --> count

        void SearchItemRegisterNoDupOnFileScope(XmlDocument item_dom, StringBuilder errorInfo)
        {
            string strRegisterNo = DomUtil.GetElementText(item_dom.DocumentElement, "registerNo");
            if (string.IsNullOrEmpty(strRegisterNo) == true)
                return;
            object o = _itemRegisterNoTable[strRegisterNo];
            if (o == null)
            {
                _itemRegisterNoTable[strRegisterNo] = 1;
                return;
            }

            int v = (int)o;
            v++;
            _itemRegisterNoTable[strRegisterNo] = v;

            errorInfo.Append("登录号 '" + strRegisterNo + "' 在源文件中发生重复(出现 " + v + " 次)");
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

            // 2018/11/16
            // 是否为登录号加上随机的后缀字符串
            public bool RandomItemRegisterNo = false;

            // 是否为册记录自动添加书目元素。(注：如果册记录中本来有了这个元素就不添加了)
            public bool AddBiblioToItem = false;

            // 是否为书目发生合并时的册记录自动添加书目元素。
            public bool AddBiblioToItemOnMerging = false;

            // 是否覆盖书目记录(恢复到原始 ID)。false 表示为追加
            public bool OverwriteBiblio = false;
            public string TargetBiblioDbName = "";  // 目标书目库名

            // 2021/8/27
            // 下级记录恢复到原先的 ID
            public bool OverwriteSubrecord = false;

            public bool DontChangeOperations = false;
            public bool SuppressOperLog = false;
            public bool DontSearchDup = false;
            public bool AutoPostfix = false;

            public string RestoreMode = "";

            public bool IncludeSubItems = true;
            public bool IncludeSubOrders = true;
            public bool IncludeSubIssues = true;
            public bool IncludeSubComments = true;
            public bool IncludeSubObjects = true;
            public string ObjectDirectoryName = "";

            public bool NewRefID = true;

            public string ItemBatchNo = ""; // 设定给册记录的批次号。如果为空，表示不修改册记录中的批次号，否则会覆盖记录中的批次号

            public string RecordRange = ""; // 导入源文件中的书目记录范围

            public int ItemErrorCount = 0;  // 总共发生过多少次下属记录导入错误

            public LibraryChannel Channel = null;
            public Stop stop = null;

            public MergeRegistry AutoMergeRegistry = null;

            // *** 以下成员都是在运行中动态设定和变化的
            public string MergeAction = "";  // 书目记录合并策略
            public bool AutoSelectMode = false; // (发现书目重复时)是否自动选择目标
            public dp2Circulation.MergeStyle UsedMergeStyle = dp2Circulation.MergeStyle.None;
            public bool Start = true;   // 是否进入开始处理状态
            public string StartBiblioRecPath = "";  // 定位源文件中需开始处理的一条记录的路径
            public RangeList RangeList = null;
            public int BiblioRecCount = 0;  // 已经处理的书目记录数

            public string SourceBiblioRecPath = ""; // 数据中的书目记录路径

            public string BiblioRecPath = "";   // 当前已经创建或者修改的书目记录路径

            public string BiblioXml { get; set; }   // 书目记录 XML

            public Hashtable ItemRefIDTable = new Hashtable();  // 册记录 refID 替换情况表。旧 refID --> 新 refID 
            public Hashtable OrderRefIDTable = new Hashtable();  // 订购记录 refID 替换情况表。旧 refID --> 新 refID 
            public Hashtable IssueRefIDTable = new Hashtable();  // 期记录 refID 替换情况表。旧 refID --> 新 refID 

            public Hashtable ItemCollectionTable = new Hashtable(); // strRootElementName --> List<string> XML 字符串列表

            public int UploadedSubItems = 0;    // 当前书目记录累计已经上传的子记录个数

            public bool HideItemsMessageBox = false; // 是否隐藏报错对话框
            public bool HideBiblioMessageBox = false; // 是否隐藏报错对话框
            public DialogResult LastBiblioDialogResult = DialogResult.No;   // 最近一次书目保存出错以后显示的对话框的选择结果

            // 2021/9/14
            // 最近一次实体保存出错后对话框选择的结果
            public DialogResult item_retry_result = DialogResult.Yes;
            // 是否选择了不出现实体保存出错对话框(按照上次的选择结果自动处理)
            public bool item_dont_display_retry_dialog = false;

            // 最近一次对象保存出错后对话框选择的结果
            public DialogResult object_retry_result = DialogResult.Yes;
            // 是否选择了不出现实体保存出错对话框(按照上次的选择结果自动处理)
            public bool object_dont_display_retry_dialog = false;

            // *** 以下变量在整个处理过程中持久
            public List<TwoString> LocationMapTable = new List<TwoString>(); // 馆藏地转换表

            // 每次要处理新的一条书目记录以前，进行清除
            public void ClearRecordVars()
            {
                this.ItemRefIDTable.Clear();
                this.OrderRefIDTable.Clear();
                this.IssueRefIDTable.Clear();
                this.BiblioRecPath = "";
                this.UploadedSubItems = 0;
                // this.MergeAction = "";
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

            /*
            if (string.IsNullOrEmpty(this.textBox_objectDirectoryName.Text))
                this.textBox_objectDirectoryName.Text = this.textBox_source_fileName.Text + ".object";
            */
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
            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            this.textBox_objectDirectoryName.ReadOnly = !control;

            if (this.checkBox_subRecords_object.Checked)
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

        void FillBiblioDbNameList()
        {
            this.comboBox_target_targetBiblioDbName.Items.Clear();

            foreach (BiblioDbProperty prop in Program.MainForm.BiblioDbProperties)
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

                controls.Add(this.checkBox_subRecords_entity);
                controls.Add(this.checkBox_subRecords_issue);
                controls.Add(this.checkBox_subRecords_order);
                controls.Add(this.checkBox_subRecords_comment);
                controls.Add(this.checkBox_subRecords_object);

                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.textBox_source_range);

                controls.Add(this.checkBox_convert_addBiblioToItem);
                controls.Add(this.checkBox_convert_addBiblioToItemOnMerging);
                controls.Add(this.checkBox_convert_refreshRefID);
                controls.Add(this.textBox_convert_itemBatchNo);

                controls.Add(this.comboBox_target_targetBiblioDbName);
                controls.Add(this.checkBox_target_randomItemBarcode);
                controls.Add(this.checkBox_target_biblioRestoreOldID);

                controls.Add(this.checkBox_target_dontSearchDup);
                controls.Add(this.checkBox_target_suppressOperLog);
                controls.Add(this.checkBox_target_dontChangeOperations);

                controls.Add(this.textBox_target_dbNameList);
                controls.Add(this.checkBox_target_randomItemRegisterNo);

                controls.Add(this.checkBox_target_autoPostfix);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_source_fileName);

                controls.Add(this.checkBox_subRecords_entity);
                controls.Add(this.checkBox_subRecords_issue);
                controls.Add(this.checkBox_subRecords_order);
                controls.Add(this.checkBox_subRecords_comment);
                controls.Add(this.checkBox_subRecords_object);

                controls.Add(this.textBox_objectDirectoryName);
                controls.Add(this.textBox_source_range);

                controls.Add(this.checkBox_convert_addBiblioToItem);
                controls.Add(this.checkBox_convert_addBiblioToItemOnMerging);
                controls.Add(this.checkBox_convert_refreshRefID);
                controls.Add(this.textBox_convert_itemBatchNo);

                controls.Add(this.comboBox_target_targetBiblioDbName);
                controls.Add(this.checkBox_target_randomItemBarcode);
                controls.Add(this.checkBox_target_biblioRestoreOldID);

                controls.Add(this.checkBox_target_dontSearchDup);
                controls.Add(this.checkBox_target_suppressOperLog);
                controls.Add(this.checkBox_target_dontChangeOperations);

                controls.Add(this.textBox_target_dbNameList);
                controls.Add(this.checkBox_target_randomItemRegisterNo);

                controls.Add(this.checkBox_target_autoPostfix);
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

        void WriteText(string strText)
        {
            WriteHtml(HttpUtility.HtmlEncode(strText));
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
        bool _specialState = false; // 是否为特殊状态？特殊状态允许使用一些有危险的 checkbox

        public bool SpecialState
        {
            get
            {
                return _specialState;
            }
            set
            {
                _specialState = value;

                this.groupBox_danger.Visible = value;
                /*
                {
                    this.checkBox_target_suppressOperLog.Visible = value;
                    this.checkBox_target_dontSearchDup.Visible = value;
                    this.checkBox_target_dontChangeOperations.Visible = value;
                    this.comboBox_target_restore.Visible = value;
                    this.label_target_restore.Visible = value;
                }
                */
            }
        }

        private void comboBox_target_restore_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_target_restore.Text == "下级记录")
                this.checkBox_target_biblioRestoreOldID.Enabled = false;
            else
                this.checkBox_target_biblioRestoreOldID.Enabled = true;
        }

        private void textBox_source_fileName_TextChanged(object sender, EventArgs e)
        {
            /*
            if (string.IsNullOrEmpty(this.textBox_source_fileName.Text) == false)
                this.textBox_objectDirectoryName.Text = this.textBox_source_fileName.Text + ".object";
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
                if (string.IsNullOrEmpty(this.textBox_source_fileName.Text) == false)
                    this.textBox_objectDirectoryName.Text = this.textBox_source_fileName.Text + ".object";
            }
        }
    }
}
