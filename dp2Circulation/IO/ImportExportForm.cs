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
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using System.Collections;

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

            ProcessInfo info = new ProcessInfo();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));

            try
            {
                string strSourceFileName = (string)this.Invoke(new Func<string>(() =>
                {
                    return this.textBox_source_fileName.Text;
                }));

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
                                return;
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        if (bRet == false)
                            return;	// 结束

                        DoRecord(reader, info);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                strError = "导入过程出现异常" + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));
            }

        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
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
            info.BiblioRecPath = "";

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

        /*
        <dprms:biblio path="net.pipe://localhost/dp2library/xe?中文图书/10" timestamp="c95606aac8ecd2080000000000000000">
            <unimarc:record xmlns:dprms="http://dp2003.com/dprms" xmlns:unimarc="http://dp2003.com/UNIMARC">
                <unimarc:leader>00827nam0 2200229   45  </unimarc:leader>
                <unimarc:controlfield tag="001">0192000006</unimarc:controlfield>
         * */
        void DoBiblio(XmlTextReader reader, ProcessInfo info)
        {
            info.ItemRefIDTable.Clear();

            string strPath = reader.GetAttribute("path");
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
            if (info.OverwriteBiblio == true)
                strAction = "change";
            else
                strAction = "new";

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
                    return;

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    Debug.Assert(reader.Name == strRootElementName, "");
                    return;
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

        void DoItems(List<string> item_xmls, ProcessInfo info1)
        {
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

                EntityInfo info = new EntityInfo();

                string strRefID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");

                if (strRootElementName == "item"
                    && String.IsNullOrEmpty(strRefID) == false)
                {
                    // 参考 ID 要替换
                    string strNewRefID = Guid.NewGuid().ToString();
                    info1.ItemRefIDTable[strRefID] = strNewRefID;
                    strRefID = strNewRefID;
                }

                info.RefID = strRefID;

                DomUtil.SetElementText(item_dom.DocumentElement,
                    "parent", Global.GetRecordID(info1.BiblioRecPath));

                string strXml = item_dom.DocumentElement.OuterXml;

                info.Action = "new";

                info.NewRecord = strXml;
                info.NewTimestamp = ByteArray.GetTimeStampByteArray(strTimestamp);

                info.OldRecord = "";
                info.OldTimestamp = null;

                entityArray.Add(info);
            }

            EntityInfo[] errorinfos = null;

            string strError = "";
            long lRet = 0;

            if (strRootElementName == "item")
                lRet = info1.Channel.SetEntities(
                     info1.stop,
                     info1.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "order")
                lRet = info1.Channel.SetOrders(
                     info1.stop,
                     info1.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "issue")
                lRet = info1.Channel.SetIssues(
                     info1.stop,
                     info1.BiblioRecPath,
                     entityArray.ToArray(),
                     out errorinfos,
                     out strError);
            else if (strRootElementName == "comment")
                lRet = info1.Channel.SetComments(
                     info1.stop,
                     info1.BiblioRecPath,
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
            }

            if (text.Length > 0)
                throw new Exception(text.ToString());
        }

        class ProcessInfo
        {
            // 是否覆盖书目记录。false 表示为追加
            public bool OverwriteBiblio = false;

            public LibraryChannel Channel = null;
            public Stop stop = null;

            public string BiblioRecPath = "";   // 当前已经创建或者修改的书目记录路径

            public Hashtable ItemRefIDTable = new Hashtable();  // 册记录 refID 替换情况表。旧 refID --> 新 refID 
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
