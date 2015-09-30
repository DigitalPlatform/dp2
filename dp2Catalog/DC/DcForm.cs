using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Web;   // HttpUtility
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Script;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.GUI;

namespace dp2Catalog
{
    public partial class DcForm : Form
    {
        public LoginInfo LoginInfo = new LoginInfo();

        int m_nTimerCount = 0;

        const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_ENABLE_UPDATE = API.WM_USER + 202;

        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        public ISearchForm LinkedSearchForm = null;

        /*
        ISearchForm m_linkedSearchForm = null;

        public ISearchForm LinkedSearchForm
        {
            get
            {
                return this.m_linkedSearchForm;
            }
            set
            {
                this.m_linkedSearchForm = value;

                // 修改相关的按钮状态
                if (this.m_linkedSearchForm != null)
                {
                    MainForm.toolButton_prev.Enabled = true;
                    MainForm.toolButton_next.Enabled = true;
                }
                else
                {
                    MainForm.toolButton_prev.Enabled = false;
                    MainForm.toolButton_next.Enabled = false;
                }
            }
        }
         * */

        DigitalPlatform.Z3950.Record CurrentRecord = null;

        Encoding CurrentEncoding = Encoding.GetEncoding(936);

        public string AutoDetectedMarcSyntaxOID = "";

        byte[] CurrentTimestamp = null;

        string DcCfgFilename = "";

        // public LibraryChannel Channel = new LibraryChannel();


        // 用于保存记录的路径
        public string SavePath
        {
            get
            {
                return this.textBox_savePath.Text;
            }
            set
            {
                this.textBox_savePath.Text = value;
            }

        }

        public DcForm()
        {
            InitializeComponent();
        }

        private void DcForm_Load(object sender, EventArgs e)
        {
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            /*
            Global.FillEncodingList(this.comboBox_originDataEncoding,
                false);
             * */



            // 初始化DC编辑器
            string strCfgFilename = this.MainForm.DataDir + "\\dc_define.xml";

            string strError = "";
            int nRet = this.DcEditor.LoadCfg(strCfgFilename,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                DcCfgFilename = strCfgFilename;
            }

            this.LoadFontToDcEditor();

            this.binaryResControl1.ContentChanged -= new ContentChangedEventHandler(binaryResControl1_ContentChanged);
            this.binaryResControl1.ContentChanged += new ContentChangedEventHandler(binaryResControl1_ContentChanged);

            this.binaryResControl1.Channel = null;
            this.binaryResControl1.Stop = this.stop;

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void DcForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {

                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "DcForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void DcForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            SaveSize();
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
                    /*
                case WM_ENABLE_UPDATE:
                    MessageBox.Show(this, "end");
                    this.DcEditor.EnableUpdate();
                    return;
                     * */
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);


            // 获得splitContainer_main的状态
            int nValue = MainForm.AppInfo.GetInt(
            "dcform",
            "splitContainer_main",
            -1);
            if (nValue != -1)
                this.splitContainer_main.SplitterDistance = nValue;

        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

                // 保存splitContainer_main的状态
                MainForm.AppInfo.SetInt(
                "dcform",
                "splitContainer_main",
                    this.splitContainer_main.SplitterDistance);
            }
        }

        void binaryResControl1_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            // SetSaveAllButtonState(true);
        }

        // 对象信息是否被改变
        public bool ObjectChanged
        {
            get
            {
                if (this.binaryResControl1 != null)
                    return this.binaryResControl1.Changed;

                return false;
            }
            set
            {
                if (this.binaryResControl1 != null)
                    this.binaryResControl1.Changed = value;
            }
        }

        // 书目信息是否被改变
        public bool BiblioChanged
        {
            get
            {
                if (this.DcEditor != null)
                {
                    // 如果object id有所改变，那么即便MARC没有改变，那最后的合成XML也发生了改变
                    if (this.binaryResControl1 != null)
                    {
                        if (this.binaryResControl1.IsIdUsageChanged() == true)
                            return true;
                    }

                    return this.DcEditor.Changed;
                }

                return false;
            }
            set
            {
                if (this.DcEditor != null)
                    this.DcEditor.Changed = value;
            }
        }

        // 获得当前有修改标志的部分的名称
        string GetCurrentChangedPartName()
        {
            string strPart = "";

            if (this.BiblioChanged == true)
                strPart += "书目信息";

            if (this.ObjectChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "对象信息";
            }

            return strPart;
        }

        public void Reload()
        {
            LoadRecordByPath("current");
        }

        // 根据数据库索引号位置装载记录
        public int LoadRecordByPath(string strDirection)
        {
            string strError = "";
            int nRet = 0;

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                    "DcForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            dp2SearchForm dp2_searchform = null;

            if (this.LinkedSearchForm == null
                || !(this.LinkedSearchForm is dp2SearchForm))
            {
                dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行LoadRecord()";
                    goto ERROR1;
                }
            }
            else
            {
                dp2_searchform = (dp2SearchForm)this.LinkedSearchForm;
            }

            if (strDirection == "prev")
            {
            }
            else if (strDirection == "next")
            {
            }
            else if (strDirection == "current")
            {
            }
            else
            {
                strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "记录路径为空，无法进行定位";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strProtocol != "dp2library")
            {
                strError = "不能处理协议" + strProtocol;
                goto ERROR1;
            }

            return LoadDp2Record(dp2_searchform,
                strPath,
                strDirection,
                true);
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 从结果集中装载记录
        public int LoadRecord(string strDirection)
        {
            string strError = "";

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                    "DcForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            ISearchForm searchform = null;

            if (this.LinkedSearchForm == null)
            {
                strError = "没有关联的检索窗，无法从检索结果集中装载记录";
                goto ERROR1;

                /*
                searchform = this.GetDp2SearchForm();

                if (searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行LoadRecord()";
                    goto ERROR1;
                }*/
            }
            else
            {
                searchform = this.LinkedSearchForm;
            }

            string strPath = this.textBox_tempRecPath.Text;
            if (String.IsNullOrEmpty(strPath) == true)
            {
                strError = "textBox_tempRecPath中路径为空";
                goto ERROR1;
            }

            // 分离出各个部分
            string strProtocol = "";
            string strResultsetName = "";
            string strIndex = "";

            int nRet = MarcDetailForm.ParsePath(strPath,
                out strProtocol,
                out strResultsetName,
                out strIndex,
                out strError);
            if (nRet == -1)
            {
                strError = "解析路径 '" + strPath + "' 字符串过程中发生错误: " + strError;
                goto ERROR1;
            }


            if (strProtocol != searchform.CurrentProtocol)
            {
                strError = "检索窗的协议已经发生改变";
                goto ERROR1;
            }

            if (strResultsetName != searchform.CurrentResultsetPath)
            {
                strError = "结果集已经发生改变";
                goto ERROR1;
            }

            int index = 0;

            index = Convert.ToInt32(strIndex) - 1;


            if (strDirection == "prev")
            {
                index--;
                if (index < 0)
                {
                    strError = "到头";
                    goto ERROR1;
                }
            }
            else if (strDirection == "next")
            {
                index++;
            }
            else if (strDirection == "current")
            {
                // index不变
            }
            else
            {
                strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                goto ERROR1;
            }

            return LoadRecord(searchform, index);
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装载XML记录，根据结果集中位置
        public int LoadRecord(ISearchForm searchform,
            int index)
        {
            string strError = "";
            string strRecordXml = "";

            this.LinkedSearchForm = searchform;
            this.SavePath = "";

            DigitalPlatform.Z3950.Record record = null;
            Encoding currentEncoding = null;

            this.CurrentRecord = null;


            byte[] baTimestamp = null;
            string strSavePath = "";
            string strOutStyle = "";
            LoginInfo logininfo = null;
            long lVersion = 0;
            string strXmlFragment = "";

            int nRet = searchform.GetOneRecord(
                "xml",
                index,  // 即将废止
                "index:" + index.ToString(),
                "hilight_browse_line", // true,
                out strSavePath,
                out strRecordXml,
                out strXmlFragment,
                out strOutStyle,
                out baTimestamp,
                out lVersion,
                out record,
                out currentEncoding,
                out logininfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.LoginInfo = logininfo;

            this.CurrentTimestamp = baTimestamp;
            this.SavePath = strSavePath;
            this.CurrentEncoding = currentEncoding;

            // dp2library协议
            if (searchform.CurrentProtocol == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                    goto ERROR1;
                }

                string strProtocol = "";
                string strPath = "";
                nRet = Global.ParsePath(strSavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                // 获得cfgs\dcdef
                string strCfgFileName = "dcdef";

                string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                // 和以前的不同，才有必要重新载入
                if (this.DcCfgFilename != strCfgPath)
                {
                    string strCode = "";
                    byte[] baCfgOutputTimestamp = null;
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = dp2_searchform.GetCfgFile(strCfgPath,
                        out strCode,
                        out baCfgOutputTimestamp,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;

                    nRet = this.DcEditor.LoadCfgCode(strCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.DcCfgFilename = strCfgPath;
                }

                // 接着装入对象资源
                {
                    EnableStateCollection save = this.MainForm.DisableToolButtons();
                    try
                    {
                        this.binaryResControl1.Channel = dp2_searchform.GetChannel(dp2_searchform.GetServerUrl(strServerName));
                        nRet = this.binaryResControl1.LoadObject(strLocalPath,
                            strRecordXml,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                            return -1;
                        }
                    }
                    finally
                    {
                        save.RestoreAll();
                    }
                }
            }

            /*
            // 替换单个0x0a
            strMARC = strMARC.Replace("\r", "");
            strMARC = strMARC.Replace("\n", "\r\n");
             * */

            // TODO: 再次装入的时候有问题
            // 装入DC编辑器
            this.DcEditor.Xml = strRecordXml;


            /*
            // 装入XML只读Web控件
            {
                string strTempFileName = MainForm.DataDir + "\\xml.xml";

                // SUTRS
                if (record != null)
                {
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
                        strTempFileName = MainForm.DataDir + "\\xml.txt";
                }

                Stream stream = File.Create(strTempFileName);

                // 写入xml内容
                byte[] buffer = Encoding.UTF8.GetBytes(strRecordXml);

                stream.Write(buffer, 0, buffer.Length);

                stream.Close();

                this.webBrowser_xml.Navigate(strTempFileName);
            }
             * */


            this.CurrentRecord = record;

            /*
            if (this.CurrentRecord != null)
            {
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    this.CurrentRecord.m_baRecord);

                // 装入原始文本
                nRet = this.SetOriginText(this.CurrentRecord.m_baRecord,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }

                // 数据库名
                this.textBox_originDatabaseName.Text = this.CurrentRecord.m_strDBName;

                // record syntax OID
                this.textBox_originMarcSyntaxOID.Text = this.CurrentRecord.m_strSyntaxOID;
            }*/


            // 构造结果集路径
            string strFullPath = searchform.CurrentProtocol + ":"
                + searchform.CurrentResultsetPath
                + "/" + (index + 1).ToString();

            this.textBox_tempRecPath.Text = strFullPath;


            this.BiblioChanged = false;

            this.DcEditor.Focus();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装载XML记录，根据记录路径
        // parameters:
        //      strPath 路径。例如 "图书总库/1@本地服务器"
        public int LoadDp2Record(dp2SearchForm dp2_searchform,
            string strPath,
            string strDirection,
            bool bLoadResObject)
        {
            string strError = "";
            string strRecordXml = "";

            if (dp2_searchform == null)
            {
                strError = "dp2_searchform参数不能为空";
                goto ERROR1;
            }

            if (dp2_searchform.CurrentProtocol != "dp2library")
            {
                strError = "所提供的检索窗不是dp2library协议";
                goto ERROR1;
            }

            DigitalPlatform.Z3950.Record record = null;
            Encoding currentEncoding = null;

            this.CurrentRecord = null;

            byte[] baTimestamp = null;
            string strOutStyle = "";

            string strSavePath = "";

            long lVersion = 0;
            LoginInfo logininfo = null;
            string strXmlFragment = "";

            int nRet = dp2_searchform.GetOneRecord(
                // true,
                "xml",
                // strPath,
                // strDirection,
                0,   // test
                "path:" + strPath + ",direction:" + strDirection,
                "",
                out strSavePath,
                out strRecordXml,
                out strXmlFragment,

                out strOutStyle,
                out baTimestamp,
                out lVersion,
                out record,
                out currentEncoding,
                out logininfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.CurrentTimestamp = baTimestamp;
            // this.SavePath = dp2_searchform.CurrentProtocol + ":" + strOutputPath;
            this.SavePath = strSavePath;
            this.CurrentEncoding = currentEncoding;

            string strServerName = "";
            string strLocalPath = "";

            strPath = strSavePath;

            // 解析记录路径。
            // 记录路径为如下形态 "中文图书/1 @服务器"
            dp2SearchForm.ParseRecPath(strPath,
                out strServerName,
                out strLocalPath);

            string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

            // 获得cfgs\dcdef
            string strCfgFileName = "dcdef";

            string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

            // 和以前的不同，才有必要重新载入
            if (this.DcCfgFilename != strCfgPath)
            {
                string strCode = "";
                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                nRet = this.DcEditor.LoadCfgCode(strCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.DcCfgFilename = strCfgPath;
            }

            // 接着装入对象资源
            if (bLoadResObject == true)
            {
                this.binaryResControl1.Channel = dp2_searchform.GetChannel(dp2_searchform.GetServerUrl(strServerName));
                nRet = this.binaryResControl1.LoadObject(strLocalPath,
                    strRecordXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return -1;
                }
            }




            // TODO: 再次装入的时候有问题
            // 装入DC编辑器
            this.DcEditor.Xml = strRecordXml;


            this.CurrentRecord = record;

            this.BiblioChanged = false;

            this.DcEditor.Focus();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装入XML只读Web控件
        void SetXmlCodeDisplay(string strRecordXml)
        {
            string strTempFileName = MainForm.DataDir + "\\xml.xml";

            Stream stream = File.Create(strTempFileName);

            // 写入xml内容
            byte[] buffer = Encoding.UTF8.GetBytes(strRecordXml);

            stream.Write(buffer, 0, buffer.Length);

            stream.Close();

            this.webBrowser_xml.Navigate(strTempFileName);
        }

        /*
        int SetOriginText(byte[] baOrigin,
    Encoding encoding,
    out string strError)
        {
            strError = "";

            if (encoding == null)
            {
                int nRet = this.MainForm.GetEncoding(this.comboBox_originDataEncoding.Text,
                    out encoding,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                this.comboBox_originDataEncoding.Text = GetEncodingForm.GetEncodingName(this.CurrentEncoding);

            }

            this.textBox_originData.Text = encoding.GetString(baOrigin);

            return 0;
        }*/

        private void toolStripButton_dispXmlText_Click(object sender, EventArgs e)
        {
            string strXmlBody = "";

            try
            {
                strXmlBody = this.DcEditor.Xml;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            XmlViewerForm dlg = new XmlViewerForm();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "XML编码的当前DC记录";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strXmlBody;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dlg,
                "dc_xml_dialog_state");

            dlg.ShowDialog(this);

            this.MainForm.AppInfo.UnlinkFormState(dlg);



            // dlg.ShowDialog();
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_tempRecPath.Enabled = bEnable;
            this.DcEditor.Enabled = bEnable;
        }

        public int MergeResourceIds(ref string strXml,
            out string strError)
        {
            strError = "";

            if (this.binaryResControl1 == null)
                return 0;

            XmlDocument domDc = new XmlDocument();
            try
            {
                domDc.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装入DOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // 先删除已经有的<dprms:file>元素
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = domDc.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.ParentNode != null)
                    node.ParentNode.RemoveChild(node);
            }

#if NO
            // 然后增加本次的id们
            List<string> ids = this.binaryResControl1.GetIds();

            for (int i = 0; i < ids.Count; i++)
            {
                string strID = ids[i];
                if (String.IsNullOrEmpty(strID) == true)
                    continue;

                XmlNode node = domDc.CreateElement("dprms",
                    "file",
                    DpNs.dprms);
                domDc.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "id", strID);
            }
#endif
            int nRet = this.binaryResControl1.AddFileFragments(ref domDc, out strError);
            if (nRet == -1)
                return -1;

            strXml = domDc.OuterXml;
            return 1;
        ERROR1:
            return -1;

        }

        // 查重
        // parameters:
        //      strSender   触发命令的来源 "toolbar" "ctrl_d"
        public int SearchDup(string strSender)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(strSender == "toolbar" || strSender == "ctrl_d", "");

            string strStartPath = this.SavePath;

            // 检查当前通讯协议
            string strProtocol = "";
            string strPath = "";

            if (String.IsNullOrEmpty(strStartPath) == false)
            {
                nRet = Global.ParsePath(strStartPath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (strProtocol != "dp2library")
                    strStartPath = "";  // 迫使重新选择起点路径
            }

            if (String.IsNullOrEmpty(strStartPath) == true)
            {
                /*
                strError = "当前记录路径为空，无法进行查重";
                goto ERROR1;
                 * */
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行查重";
                    goto ERROR1;
                }

                string strDefaultStartPath = this.MainForm.DefaultSearchDupStartPath;

                // 如果缺省起点路径定义为空，或者按下Control键强制要求出现对话框
                if (String.IsNullOrEmpty(strDefaultStartPath) == true
                    || (Control.ModifierKeys == Keys.Control && strSender == "toolbar"))
                {
                    // 变为正装形态
                    if (String.IsNullOrEmpty(strDefaultStartPath) == false)
                        strDefaultStartPath = Global.GetForwardStyleDp2Path(strDefaultStartPath);

                    // 临时指定一个dp2library服务器和数据库
                    GetDp2ResDlg dlg = new GetDp2ResDlg();
                    GuiUtil.SetControlFont(dlg, this.Font);

                    dlg.Text = "请指定一个dp2library数据库，以作为模拟的查重起点";
                    dlg.dp2Channels = dp2_searchform.Channels;
                    dlg.Servers = this.MainForm.Servers;
                    dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
                    dlg.Path = strDefaultStartPath;  // 采用遗留的上次用过的路径

                    this.MainForm.AppInfo.LinkFormState(dlg,
                        "searchdup_selectstartpath_dialog_state");

                    dlg.ShowDialog(this);

                    this.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult != DialogResult.OK)
                        return 0;

                    strDefaultStartPath = Global.GetBackStyleDp2Path(dlg.Path + "/?");

                    // 重新设置到系统参数中
                    this.MainForm.DefaultSearchDupStartPath = strDefaultStartPath;
                }

                strProtocol = "dp2library";
                strPath = strDefaultStartPath;
            }



            this.EnableControls(false);
            try
            {

                // dtlp协议的记录保存
                if (strProtocol.ToLower() == "dtlp")
                {
                    strError = "目前暂不支持DTLP协议的查重操作";
                    goto ERROR1;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行查重";
                        goto ERROR1;
                    }

                    // 将strPath解析为server url和local path两个部分
                    string strServerName = "";
                    string strPurePath = "";
                    dp2SearchForm.ParseRecPath(strPath,
                        out strServerName,
                        out strPurePath);

                    string strDbName = dp2SearchForm.GetDbName(strPurePath);
                    string strSyntax = "";


                    // 获得一个数据库的数据syntax
                    // parameters:
                    //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                    //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = dp2_searchform.GetDbSyntax(null, // this.stop, BUG!!!
                        strServerName,
                        strDbName,
                        out strSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strSyntax) == true)
                        strSyntax = "unimarc";

                    // 获得书目记录的XML格式
                    string strXml = "";

                    try
                    {
                        strXml = this.DcEditor.Xml;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        goto ERROR1;
                    }


                    // 打开查重窗口
                    dp2DupForm form = new dp2DupForm();

                    form.MainForm = this.MainForm;
                    form.MdiParent = this.MainForm;

                    form.LibraryServerName = strServerName;
                    form.ProjectName = "<默认>";
                    form.XmlRecord = strXml;
                    form.RecordPath = strPurePath;

                    form.AutoBeginSearch = true;

                    form.Show();

                    return 0;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }


            }
            finally
            {
                this.EnableControls(true);
            }

        ERROR1:
            MessageBox.Show(this, strError);
            return -1;

        }

        // 保存记录
        public int SaveRecord()
        {
            string strError = "";
            int nRet = 0;

            string strLastSavePath = MainForm.LastSavePath;
            if (String.IsNullOrEmpty(strLastSavePath) == false)
            {
                string strOutputPath = "";
                nRet = MarcDetailForm.ChangePathToAppendStyle(strLastSavePath,
                    out strOutputPath,
                    out strError);
                if (nRet == -1)
                {
                    MainForm.LastSavePath = ""; // 避免下次继续出错 2011/3/4
                    goto ERROR1;
                }
                strLastSavePath = strOutputPath;
            }

            SaveRecordDlg dlg = new SaveRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.MainForm = this.MainForm;
            dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
            dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
            // dlg.RecPath = this.SavePath == "" ? MainForm.LastSavePath : this.SavePath;
            dlg.RecPath = this.SavePath == "" ? strLastSavePath : this.SavePath;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ActiveProtocol = "dp2library";

            this.MainForm.AppInfo.LinkFormState(dlg, "SaveRecordDlg_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("DcForm", "SaveRecordDlg_uiState", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("DcForm", "SaveRecordDlg_uiState", dlg.UiState);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            MainForm.LastSavePath = dlg.RecPath;


            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(dlg.RecPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.EnableControls(false);
            try
            {

                // dp2library协议的记录保存
                if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    string strXml = "";

                    try
                    {
                        strXml = this.DcEditor.Xml;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        goto ERROR1;
                    }

                    // 合成<dprms:file>元素
                    nRet = MergeResourceIds(ref strXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    /*
                    if (this.binaryResControl1 != null)
                    {
                        XmlDocument domDc = new XmlDocument();
                        try
                        {
                            domDc.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "XML数据装入DOM时出错: " + ex.Message;
                            goto ERROR1;
                        }

                        // 先删除已经有的<dprms:file>元素

                        List<string> ids = this.binaryResControl1.GetIds();

                        for (int i = 0; i < ids.Count; i++)
                        {
                            string strID = ids[i];
                            if (String.IsNullOrEmpty(strID) == true)
                                continue;

                            XmlNode node = domDc.CreateElement("dprms",
                                "file",
                                DpNs.dprms);
                            domDc.DocumentElement.AppendChild(node);
                            DomUtil.SetAttr(node, "id", strID);
                        }

                        strXml = domDc.OuterXml;
                    }
                     * */

                    string strOutputPath = "";
                    byte[] baOutputTimestamp = null;
                    nRet = dp2_searchform.SaveXmlRecord(
                        strPath,
                        strXml,
                        this.CurrentTimestamp,
                        out strOutputPath,
                        out baOutputTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.SavePath = strProtocol + ":" + strOutputPath;
                    this.CurrentTimestamp = baOutputTimestamp;

                    /*
                    // 结果集路径为空
                    this.textBox_tempRecPath.Text = "";
                     * */


                    // 如果资源控件还没有设置path，或者为追加型，则补上
                    // TODO: 是不是干脆都作一次？
                    if (String.IsNullOrEmpty(this.binaryResControl1.BiblioRecPath) == true
                        || dp2SearchForm.IsAppendRecPath(this.binaryResControl1.BiblioRecPath) == true)
                    {
                        string strServerName = "";
                        string strLocalPath = "";
                        // 解析记录路径。
                        // 记录路径为如下形态 "中文图书/1 @服务器"
                        dp2SearchForm.ParseRecPath(strOutputPath,
                            out strServerName,
                            out strLocalPath);
                        this.binaryResControl1.BiblioRecPath = strLocalPath;
                    }

                    // 提交对象保存请求
                    // return:
                    //		-1	error
                    //		>=0 实际上载的资源对象数
                    nRet = this.binaryResControl1.Save(out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                    else
                    {
                        MessageBox.Show(this, "保存成功");
                    }

                    if (nRet >= 1)
                    {
                        /*
                        bObjectSaved = true;
                        if (strText != "")
                            strText += " ";
                        strText += "对象信息";
                         * */

                        // 刷新书目记录的时间戳
                        // LoadRecord("current");
                        LoadDp2Record(dp2_searchform,
                            strOutputPath,
                            "current",
                            false);
                    }

                    this.BiblioChanged = false;
                    return 0;
                }
                else if (strProtocol.ToLower() == "dtlp")
                {
                    strError = "目前DC窗暂不支持DTLP协议的保存操作";
                    goto ERROR1;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前DC窗暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                this.EnableControls(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm is DtlpSearchForm)
            {
                dtlp_searchform = (DtlpSearchForm)this.LinkedSearchForm;
            }
            else
            {
                dtlp_searchform = this.MainForm.TopDtlpSearchForm;

                if (dtlp_searchform == null)
                {
                    // 新开一个dtlp检索窗
                    dtlp_searchform = new DtlpSearchForm();
                    dtlp_searchform.MainForm = this.MainForm;
                    dtlp_searchform.MdiParent = this.MainForm;
                    dtlp_searchform.WindowState = FormWindowState.Minimized;
                    dtlp_searchform.Show();
                }
            }

            return dtlp_searchform;
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm.IsValid() == true   // 2008/3/17
                && this.LinkedSearchForm is dp2SearchForm)
            {
                dp2_searchform = (dp2SearchForm)this.LinkedSearchForm;
            }
            else
            {
                dp2_searchform = this.MainForm.TopDp2SearchForm;

                if (dp2_searchform == null)
                {
                    // 新开一个dp2检索窗
                    FormWindowState old_state = this.WindowState;
                    dp2_searchform = new dp2SearchForm();
                    dp2_searchform.MainForm = this.MainForm;
                    dp2_searchform.MdiParent = this.MainForm;
                    dp2_searchform.WindowState = FormWindowState.Minimized;
                    dp2_searchform.Show();

                    // 2008/3/17
                    this.WindowState = old_state;
                    this.Activate();

                    // 需要等待初始化操作彻底完成
                    dp2_searchform.WaitLoadFinish();
                }
            }

            return dp2_searchform;
        }

        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            e.dp2Channels = dp2_searchform.Channels;
            e.MainForm = this.MainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        private void DcForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 菜单
            MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
            MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            MainForm.MenuItem_font.Enabled = true;
            MainForm.MenuItem_saveToTemplate.Enabled = true;

            // 工具条按钮
            MainForm.toolButton_search.Enabled = false;

            MainForm.toolButton_prev.Enabled = true;
            MainForm.toolButton_next.Enabled = true;
            /*
            if (this.LinkedSearchForm != null)
            {
                MainForm.toolButton_prev.Enabled = true;
                MainForm.toolButton_next.Enabled = true;
            }
            else
            {
                MainForm.toolButton_prev.Enabled = false;
                MainForm.toolButton_next.Enabled = false;
            }*/

            MainForm.toolButton_nextBatch.Enabled = false;

            MainForm.toolButton_getAllRecords.Enabled = false;
            MainForm.toolButton_saveTo.Enabled = false;
            MainForm.toolButton_save.Enabled = true;
            MainForm.toolButton_delete.Enabled = true;

            MainForm.toolButton_loadTemplate.Enabled = true;

            MainForm.toolButton_dup.Enabled = true;
            MainForm.toolButton_verify.Enabled = true;

            MainForm.toolButton_refresh.Enabled = true;
            MainForm.toolButton_loadFullRecord.Enabled = false;
        }


        // 删除记录
        public int DeleteRecord()
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "缺乏保存路径，无法进行删除";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strChangedWarning = "";

            if (this.ObjectChanged == true
                || this.BiblioChanged == true)
            {
                strChangedWarning = "当前有 "
                    + GetCurrentChangedPartName()
                    // strChangedWarning
                + " 被修改过。\r\n\r\n";
            }

            string strText = strChangedWarning;

            strText += "确实要删除书目记录 " + strPath + " ";

            int nObjectCount = this.binaryResControl1.ObjectCount;
            if (nObjectCount != 0)
                strText += "和从属的 " + nObjectCount.ToString() + " 个对象";

            strText += " ?";

            // 警告删除
            DialogResult result = MessageBox.Show(this,
                strText,
                "DcForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return 0;
            }



            this.EnableControls(false);
            try
            {

                // dp2library协议的记录保存
                if (strProtocol.ToLower() == "dp2library")
                {

                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                        goto ERROR1;
                    }



                    // string strOutputPath = "";
                    byte[] baOutputTimestamp = null;
                    // 删除一条MARC/XML记录
                    // parameters:
                    //      strSavePath 内容为"中文图书/1@本地服务器"。没有协议名部分。
                    // return:
                    //      -1  error
                    //      0   suceed
                    nRet = dp2_searchform.DeleteOneRecord(
                        strPath,
                        this.CurrentTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    this.CurrentTimestamp = baOutputTimestamp;  // 即便发生错误，也要更新时间戳，以便后面继续删除
                    if (nRet == -1)
                        goto ERROR1;

                    this.binaryResControl1.Clear(); // 2008/3/18 清除残余的内容，避免保存回去的时候形成空对象资源

                    this.ObjectChanged = false;
                    this.BiblioChanged = false;

                    MessageBox.Show(this, "删除成功");
                    return 1;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的删除操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                this.EnableControls(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装载模板
        public int LoadTemplate()
        {
            string strError = "";
            int nRet = 0;

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                    "DcForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }


            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "没有连接的或者打开的dp2检索窗，无法装载模板";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";

            string strServerName = "";
            string strLocalPath = "";

            string strBiblioDbName = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }

                if (strProtocol == "dp2library")
                {
                    // 解析记录路径。
                    // 记录路径为如下形态 "中文图书/1 @服务器"
                    dp2SearchForm.ParseRecPath(strPath,
                        out strServerName,
                        out strLocalPath);

                    strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);
                }
                else
                {
                    strProtocol = "dp2library";
                    strPath = "";
                }
            }
            else
            {
                strProtocol = "dp2library";
            }

            /*
            if (this.LinkedSearchForm != null
                && strProtocol != this.LinkedSearchForm.CurrentProtocol)
            {
                strError = "检索窗的协议已经发生改变";
                goto ERROR1;
            }*/

            string strStartPath = "";

            if (String.IsNullOrEmpty(strServerName) == false
                && String.IsNullOrEmpty(strBiblioDbName) == false)
                strStartPath = strServerName + "/" + strBiblioDbName;
            else if (String.IsNullOrEmpty(strServerName) == false)
                strStartPath = strServerName;

            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请选择目标数据库";
            dlg.dp2Channels = dp2_searchform.Channels;
            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
            dlg.Path = strStartPath;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            // 将目标路径拆分为两个部分
            nRet = dlg.Path.IndexOf("/");
            if (nRet == -1)
            {
                Debug.Assert(false, "");
                strServerName = dlg.Path;
                strBiblioDbName = "";
                strError = "所选择目标(数据库)路径 '" + dlg.Path + "' 格式不正确";
                goto ERROR1;
            }
            else
            {
                strServerName = dlg.Path.Substring(0, nRet);
                strBiblioDbName = dlg.Path.Substring(nRet + 1);

                // 检查所选数据库的syntax，必须为dc

                string strSyntax = "";
                // 获得一个数据库的数据syntax
                // parameters:
                //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetDbSyntax(
                    null,
                    strServerName,
                    strBiblioDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取书目库 '" +strBiblioDbName+ "的数据格式时发生错误: " + strError;
                    goto ERROR1;
                }

                if (strSyntax != "dc")
                {
                    strError = "所选书目库 '" + strBiblioDbName + "' 不是DC格式的数据库";
                    goto ERROR1;
                }
            }


            // 然后获得cfgs/template配置文件
            string strCfgFilePath = strBiblioDbName + "/cfgs/template" + "@" + strServerName;

            string strCode = "";
            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = dp2_searchform.GetCfgFile(strCfgFilePath,
                out strCode,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
            GuiUtil.SetControlFont(tempdlg, this.Font);
            nRet = tempdlg.Initial(false,
                strCode, 
                out strError);
            if (nRet == -1)
            {
                strError = "装载配置文件 '" + strCfgFilePath + "' 发生错误: " + strError;
                goto ERROR1;
            }

            tempdlg.ap = this.MainForm.AppInfo;
            tempdlg.ApCfgTitle = "dcform_selecttemplatedlg";
            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return 0;

            // 获得cfgs\dcdef
            string strCfgFileName = "dcdef";

            string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

            // 和以前的不同，才有必要重新载入
            if (this.DcCfgFilename != strCfgPath)
            {
                strCode = "";
                // byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                nRet = this.DcEditor.LoadCfgCode(strCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.DcCfgFilename = strCfgPath;
            }

            // 接着装入对象资源
            {
                this.binaryResControl1.Clear();
                this.binaryResControl1.Channel = dp2_searchform.GetChannel(dp2_searchform.GetServerUrl(strServerName));
                this.binaryResControl1.BiblioRecPath = strBiblioDbName + "/?";
            }



            this.DcEditor.Xml = tempdlg.SelectedRecordXml;
            this.CurrentTimestamp = null;   // baCfgOutputTimestamp;

            this.SavePath = strProtocol + ":" + strBiblioDbName + "/?" + "@" + strServerName;

            this.ObjectChanged = false;
            this.BiblioChanged = false;

            this.LinkedSearchForm = null;  // 切断和原来关联的检索窗的联系。这样就没法前后翻页了
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }


        // 保存到模板
        public int SaveToTemplate()
        {
            string strError = "";
            int nRet = 0;

            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "没有连接的或者打开的dp2检索窗，无法保存当前内容到模板";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";

            string strServerName = "";
            string strLocalPath = "";

            string strBiblioDbName = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }

                if (strProtocol == "dp2library")
                {
                    // 解析记录路径。
                    // 记录路径为如下形态 "中文图书/1 @服务器"
                    dp2SearchForm.ParseRecPath(strPath,
                        out strServerName,
                        out strLocalPath);

                    strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);
                }
                else
                {
                    strProtocol = "dp2library";
                    strPath = "";
                }
            }


            string strStartPath = "";

            if (String.IsNullOrEmpty(strServerName) == false
                && String.IsNullOrEmpty(strBiblioDbName) == false)
                strStartPath = strServerName + "/" + strBiblioDbName;
            else if (String.IsNullOrEmpty(strServerName) == false)
                strStartPath = strServerName;

            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请选择目标数据库";
            dlg.dp2Channels = dp2_searchform.Channels;
            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
            dlg.Path = strStartPath;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            nRet = dlg.Path.IndexOf("/");
            if (nRet == -1)
                strServerName = dlg.Path;
            else
            {
                strServerName = dlg.Path.Substring(0, nRet);
                strBiblioDbName = dlg.Path.Substring(nRet + 1);

                // 检查所选数据库的syntax，必须为dc

                string strSyntax = "";
                // 获得一个数据库的数据syntax
                // parameters:
                //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetDbSyntax(
                    null,
                    strServerName,
                    strBiblioDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                    goto ERROR1;
                }

                if (strSyntax != "dc")
                {
                    strError = "所选书目库 '" + strBiblioDbName + "' 不是DC格式的数据库";
                    goto ERROR1;
                }
            }


            // 然后获得cfgs/template配置文件
            string strCfgFilePath = strBiblioDbName + "/cfgs/template" + "@" + strServerName;

            string strCode = "";
            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = dp2_searchform.GetCfgFile(strCfgFilePath,
                out strCode,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
            GuiUtil.SetControlFont(tempdlg, this.Font);
            nRet = tempdlg.Initial(true,
                strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            tempdlg.Text = "请选择要修改的模板记录";
			tempdlg.CheckNameExist = false;	// 按OK按钮时不警告"名字不存在",这样允许新建一个模板
			tempdlg.ap = this.MainForm.AppInfo;
			tempdlg.ApCfgTitle = "dcform_selecttemplatedlg";
			tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return 0;

			// 修改配置文件内容
			if (tempdlg.textBox_name.Text != "")
			{
                string strXml = "";

                try {
                strXml = this.DcEditor.Xml;
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

				// 替换或者追加一个记录
				nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
					strXml,
					out strError);
				if (nRet == -1) 
				{
					goto ERROR1;
				}
			}

			if (tempdlg.Changed == false)	// 没有必要保存回去
				return 0;

   			string strOutputXml = tempdlg.OutputXml;

            nRet = dp2_searchform.SaveCfgFile(
                strCfgFilePath,
                strOutputXml,
                baCfgOutputTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "修改模板成功");
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        private void tabControl_main_Selected(object sender, TabControlEventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_xmlDisplay)
            {
                string strXml = "";
                try
                {
                    strXml = this.DcEditor.Xml;
                }
                catch (Exception ex)
                {
                    strXml = "<error>" + HttpUtility.HtmlEncode(ex.Message) + "</error>";
                }

                SetXmlCodeDisplay(strXml);
            }
        }

        // 接管Ctrl+各种键
        protected override bool ProcessDialogKey(
            Keys keyData)
        {

            // Ctrl + A 自动录入功能
            if (keyData == (Keys.A | Keys.Control))
            {
                // MessageBox.Show(this, "CTRL+A");
                this.AutoGenerate();
                return true;
            }

            // Ctrl + D 查重
            if (keyData == (Keys.D | Keys.Control))
            {
                this.SearchDup("ctrl_d");
                return true;
            }

            return false;
        }

        // 自动加工数据
        public void AutoGenerate()
        {
            string strError = "";
            string strCode = "";
            string strRef = "";


            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "缺乏保存路径";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // dtlp协议的自动创建数据
            if (strProtocol.ToLower() == "dtlp")
            {
                strError = "暂不支持来自DTLP协议的数据自动创建功能";
                goto ERROR1;
            }

            // dp2library协议的自动创建数据
            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                string strCfgFileName = "dp2catalog_dc_autogen.cs";

                string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                strCfgFileName = "dp2catalog_dc_autogen.cs.ref";

                strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strRef,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

            }

            try
            {
                // 执行代码
                nRet = RunScript(strCode,
                    strRef,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "执行脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int RunScript(string strCode,
            string strRef,
            out string strError)
        {
            strError = "";
            string[] saRef = null;
            int nRet;
            // string strWarning = "";

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            // 2007/12/4
            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",

									//Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2catalog.exe"
								};

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            /*
            Assembly assembly = ScriptManager.CreateAssembly(
                strCode,
                saRef,
                null,	// strLibPaths,
                null,	// strOutputFile,
                out strError,
                out strWarning);
            if (assembly == null)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strError;
                return -1;
            }*/
            Assembly assembly = null;
            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out assembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Catalog.DcDetailHost");
            if (entryClassType == null)
            {
                strError = "dp2Catalog.DcDetailHost派生类没有找到";
                return -1;
            }

            // new一个DcDetailHost派生对象
            DcDetailHost hostObj = (DcDetailHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (hostObj == null)
            {
                strError = "new Host派生类对象失败";
                return -1;
            }

            // 为Host派生类设置参数
            hostObj.DetailForm = this;
            hostObj.Assembly = assembly;

            HostEventArgs e = new HostEventArgs();

            /*
            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;
             * */


            hostObj.Main(null, e);

            /*
            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;
             * */

            return 0;
        }

        // 获得出版社相关信息
        public int GetPublisherInfo(
            string strPublisherNumber,
            out string str210,
            out string strError)
        {
            strError = "";
            str210 = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用GetPublisherInfo()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.GetPublisherInfo(
                    strServerName,
                    strPublisherNumber,
                    out str210,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 设置出版社相关信息
        public int SetPublisherInfo(
            string strPublisherNumber,
            string str210,
            out string strError)
        {
            strError = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用SetPublisherInfo()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.SetPublisherInfo(
                    strServerName,
                    strPublisherNumber,
                    str210,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 获得102相关信息
        public int Get102Info(
            string strPublisherNumber,
            out string str102,
            out string strError)
        {
            strError = "";
            str102 = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用Get102Info()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.Get102Info(
                    strServerName,
                    strPublisherNumber,
                    out str102,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 设置102相关信息
        public int Set102Info(
            string strPublisherNumber,
            string str102,
            out string strError)
        {
            strError = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用Set102Info()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.Set102Info(
                    strServerName,
                    strPublisherNumber,
                    str102,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 为了兼容以前的 API
        public int HanziTextToPinyin(
    bool bLocal,
    string strText,
    PinyinStyle style,
    out string strPinyin,
    out string strError)
        {
            return this.MainForm.HanziTextToPinyin(
                this,
                bLocal,
                strText,
                style,
                "",
                out strPinyin,
                out strError);
        }

#if NO
        // 根据汉字得到拼音字符串
        // parameters:
        //      bLocal  是否从本地获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int HanziTextToPinyin(
            bool bLocal,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";


            string strHanzi;
            int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字


            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                strHanzi = "";

                if (ch >= 0 && ch <= 128)
                {
                    if (nStatus == 2)
                        strPinyin += " ";

                    strPinyin += ch;

                    if (ch == ' ')
                        nStatus = 1;
                    else
                        nStatus = 0;

                    continue;
                }
                else
                {	// 汉字
                    strHanzi += ch;
                }

                // 汉字前面出现了英文或者汉字，中间间隔空格
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";


                // 看看是否特殊符号
                if (strSpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// 放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }


                // 获得拼音
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.MainForm.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.MainForm.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取拼音");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceld
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    // GuiUtil.SetControlFont(dlg, this.Font);
                    float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    GuiUtil.SetControlFont(dlg, this.Font, false);
                    // 维持字体的原有大小比例关系
                    dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);

                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    MainForm.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                    dlg.ShowDialog(this);

                    MainForm.AppInfo.UnlinkFormState(dlg);

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
                    }
                    else if (dlg.DialogResult == DialogResult.OK)
                    {
                        strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                            dlg.ResultPinyin,
                            style);
                    }
                    else if (dlg.DialogResult == DialogResult.Abort)
                    {
                        return 0;   // 用户希望整个中断
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                    }
                }
                else
                {
                    // 单个拼音

                    strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            return 1;   // 正常结束
        }

#endif

        void LoadFontToDcEditor()
        {
            string strFontString = MainForm.AppInfo.GetString("dceditor",
                "fontstring",
                "");  // "Arial Unicode MS, 9pt"

            if (String.IsNullOrEmpty(strFontString) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                this.DcEditor.Font = (Font)converter.ConvertFromString(strFontString);
            }

            string strFontColor = MainForm.AppInfo.GetString("dceditor",
                "fontcolor",
                "");

            if (String.IsNullOrEmpty(strFontColor) == false)
            {
                // Create the ColorConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                this.DcEditor.ForeColor = (Color)converter.ConvertFromString(strFontColor);
            }
        }

        void SaveFontForDcEditor()
        {
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                string strFontString = converter.ConvertToString(this.DcEditor.Font);

                MainForm.AppInfo.SetString("dceditor",
                    "fontstring",
                    strFontString);
            }

            {
                // Create the ColorConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                string strFontColor = converter.ConvertToString(this.DcEditor.ForeColor);

                MainForm.AppInfo.SetString("dceditor",
                    "fontcolor",
                    strFontColor);
            }

        }

        // 设置字体
        public void SetFont()
        {
            FontDialog dlg = new FontDialog();

            dlg.ShowColor = true;
            dlg.Color = this.DcEditor.ForeColor;
            dlg.Font = this.DcEditor.Font;
            dlg.ShowApply = true;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            dlg.Apply -= new EventHandler(dlgFont_Apply);
            dlg.Apply += new EventHandler(dlgFont_Apply);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            this.DcEditor.DisableUpdate();

            this.DcEditor.Font = dlg.Font;
            this.DcEditor.ForeColor = dlg.Color;

            this.DcEditor.EnableUpdate();

            // 保存到配置文件
            SaveFontForDcEditor();
        }

        void dlgFont_Apply(object sender, EventArgs e)
        {
            FontDialog dlg = (FontDialog)sender;

            this.DcEditor.DisableUpdate();

            this.DcEditor.Font = dlg.Font;
            this.DcEditor.ForeColor = dlg.Color;

            this.DcEditor.EnableUpdate();

            // 保存到配置文件
            SaveFontForDcEditor();
        }

        // 新增元素
        private void toolStripButton_newElement_Click(object sender, EventArgs e)
        {
            this.DcEditor.NewElement();
        }

        // 删除元素
        private void toolStripButton_deleteEelement_Click(object sender, EventArgs e)
        {
            this.DcEditor.DeleteSelectedElements();
        }

        private void DcEditor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.DcEditor.SelectedIndices.Count == 0)
                this.toolStripButton_deleteEelement.Enabled = false;
            else
                this.toolStripButton_deleteEelement.Enabled = true;
        }

#if NOOOOOOOOOOOO
        protected override void OnSizeChanged(EventArgs e)
        {
            // MessageBox.Show(this, "begin");
            this.DcEditor.DisableDrawCell();
            try
            {
                base.OnSizeChanged(e);
            }
            finally
            {
                this.DcEditor.EnableDrawCell();
                // MessageBox.Show(this, "end");
            }
        }
#endif

    }
}