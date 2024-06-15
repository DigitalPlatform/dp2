using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using System.Diagnostics;
using System.IO;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.DTLP;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using dp2Catalog.DTLP;
using DigitalPlatform.CommonControl;
using System.Web;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Bibliography;

namespace dp2Catalog
{
    public partial class DtlpSearchForm : MyForm, ISearchForm
    {
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        public string BinDir = "";

#if NO
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;
#endif

        public DtlpChannelArray DtlpChannels = new DtlpChannelArray();
        public DtlpChannel DtlpChannel = null;	// 尽量使用一个通道
        Hashtable AccountTable = new Hashtable();

        // 当前检索参数
        string strCurrentTargetPath = "";
        string strCurrentQueryWord = "";

        const int WM_LOADSIZE = API.WM_USER + 201;

        // 当前缺省的编码方式
        Encoding CurrentEncoding = Encoding.GetEncoding("gb2312");

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventLoadFinish = new AutoResetEvent(false);

        public DtlpSearchForm()
        {
            InitializeComponent();

            DtlpChannels.Idle -= new DtlpIdleEventHandler(DtlpChannels_Idle);
            DtlpChannels.Idle += new DtlpIdleEventHandler(DtlpChannels_Idle);
        }

        void DtlpChannels_Idle(object sender, DtlpIdleEventArgs e)
        {
            e.bDoEvents = true;
        }

        #region ISearchForm 接口函数

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        public string CurrentProtocol
        {
            get
            {
                return "dtlp";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                return this.strCurrentTargetPath
                    + this.strCurrentQueryWord
                    + "/default";
            }
        }


        // 刷新一条MARC记录
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "尚未实现";

            return -2;
        }

        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            return 0;
        }

        public int GetOneRecord(
            string strStyle,
            int nTest,
            string strPathParam,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strMARC,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.OldZ3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strMARC = "";
            record = null;
            strError = "";
            currrentEncoding = this.CurrentEncoding;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "marc";
            logininfo = new LoginInfo();
            lVersion = 0;

            if (strStyle != "marc")
            {
                strError = "DtlpSearchForm只支持获取MARC格式记录";
                return -1;
            }

            int nRet = 0;

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            if (index == -1)
            {
                string strOutputPath = "";
                nRet = InternalGetOneRecord(
                    strStyle,
                    strPath,
                    strDirection,
                    out strMARC,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out _,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }


            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);

            // int nRet = 0;

            // int nStyle = DtlpChannel.XX_STYLE; // 获得详细记录

            if (index >= this.listView_browse.Items.Count)
            {
                // 如果检索曾经中断过，这里可以触发继续检索

                strError = "越过结果集尾部";
                return -1;
            }

            ListViewItem curItem = this.listView_browse.Items[index];

            if (bHilightBrowseLine == true)
            {
                // 修改listview中事项的选定状态
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    this.listView_browse.SelectedItems[i].Selected = false;
                }

                curItem.Selected = true;
                curItem.EnsureVisible();
            }

            strPath = curItem.Text;

            // 将路径转换为内核可以接受的正规形态
            strPath = DigitalPlatform.DTLP.Global.ModifyDtlpRecPath(strPath,
                "ctlno");

            strSavePath = this.CurrentProtocol + ":" + strPath;

            /*

            byte[] baPackage;
            nRet = this.DtlpChannel.Search(strPath,
                nStyle,
                out baPackage);
            if (nRet == -1)
            {
                int errorcode = this.DtlpChannel.GetLastErrno();
                strError = "检索出错:\r\n"
                    + "检索式: " + strPath + "\r\n"
                    + "错误码: " + errorcode + "\r\n"
                    + "错误信息: " + this.DtlpChannel.GetErrorString(errorcode) + "\r\n";
                goto ERROR1;
            }

            Encoding encoding = this.DtlpChannel.GetPathEncoding(strPath);
            Package package = new Package();
            package.LoadPackage(baPackage,
                encoding);
            nRet = package.Parse(PackageFormat.Binary);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            byte[] content = null;
            nRet = package.GetFirstBin(out content);
            if (nRet == -1)
            {
                strError = "Package::GetFirstBin() error";
                goto ERROR1;
            }

            if (content == null
                || content.Length < 9)
            {
                strError = "content length < 9";
                goto ERROR1;
            }

            baTimestamp = new byte[9];
            Array.Copy(content, baTimestamp, 9);

            byte[] marc = new byte[content.Length - 9];
            Array.Copy(content, 
                9,
                marc,
                0,
                content.Length - 9);

            // strMARC = this.CurrentEncoding.GetString(marc);
            strMARC = encoding.GetString(marc);

            // 去掉最后若干连续的29字符或者0字符
            // 2008/3/11
            int nDelta = 0;
            for (int i = strMARC.Length - 1; i > 24; i--)
            {
                char ch = strMARC[i];
                if (ch == 0 || ch == 29)
                    nDelta++;
                else
                    break;
            }

            if (nDelta > 0)
                strMARC = strMARC.Substring(0, strMARC.Length - nDelta);

            // 自动识别MARC格式
            string strOutMarcSyntax = "";
            // 探测记录的MARC格式 unimarc / usmarc / reader
            nRet = MarcUtil.DetectMarcSyntax(strMARC,
                out strOutMarcSyntax);
            if (strOutMarcSyntax == "")
                strOutMarcSyntax = "unimarc";

            record = new DigitalPlatform.Z3950.Record();
            if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                record.m_strSyntaxOID = "1.2.840.10003.5.1";
            else if (strOutMarcSyntax == "usmarc")
                record.m_strSyntaxOID = "1.2.840.10003.5.10";
            else if (strOutMarcSyntax == "dt1000reader")
                record.m_strSyntaxOID = "1.2.840.10003.5.dt1000reader";
            else
            {
                // TODO: 可以出现菜单选择
            }
             * */
            {
                string strOutputPath = "";

                nRet = InternalGetOneRecord(
                    strStyle,
                    strPath,
                    "current",
                    out strMARC,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out _,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }
        ERROR1:
            return -1;
        }

        #endregion

        void DoNewStop(object sender, StopEventArgs e)
        {
            Stop current_stop = (Stop)sender;
            if (current_stop == null)
                return;
            DtlpChannel channel = (DtlpChannel)current_stop.Tag;
            if (channel == null)
                return;

            channel.Cancel();
        }

        // 获得一条MARC记录
        // 注：如果this.DtlpChannel被占用，启动启用新的通道
        // TODO: 尚未处理启用新通道时启用新Stop的课题
        // parameters:
        //      strPath 记录路径。格式为"localhost/中文图书/ctlno/1"
        //      strDirection    方向。为 prev/next/current之一。current可以缺省。
        //      strOutputPath   [out]返回的实际路径。格式和strPath相同。
        // return:
        //      -1  error 包括not found
        //      0   found
        //      1   为诊断记录
        int InternalGetOneRecord(
            string strStyle,
            string strPath,
            string strDirection,
            out string strMARC,
            out string strOutputPath,
            out string strOutStyle,
            out byte[] baTimestamp,
            out DigitalPlatform.OldZ3950.Record record,
            out Encoding currrentEncoding,
            out int errorcode,
            out string strError)
        {
            strMARC = "";
            record = null;
            strError = "";
            currrentEncoding = this.CurrentEncoding;
            baTimestamp = null;
            strOutStyle = "marc";
            strOutputPath = ""; // TODO: 需要参考dp1batch看获得outputpath的方法
            errorcode = 0;

            if (strStyle != "marc")
            {
                strError = "DtlpSearchForm只支持获取MARC格式记录";
                return -1;
            }

            int nRet = 0;

            int nStyle = DtlpChannel.XX_STYLE; // 获得详细记录

            if (strDirection == "prev")
                nStyle |= DtlpChannel.PREV_RECORD;
            else if (strDirection == "next")
                nStyle |= DtlpChannel.NEXT_RECORD;

            /*
            // 将路径转换为内核可以接受的正规形态
            string strPath = DigitalPlatform.DTLP.Global.ModifyDtlpRecPath(strPath,
                "ctlno");
             * */
            /*
            Stop temp_stop = this.stop;
            */
            var looping = BeginLoop(this.DoStop,
                $"正在获取记录 {strPath} ...");

            DtlpChannel channel = null;

            bool bNewChannel = false;
            if (this.m_nInSearching == 0)
                channel = this.DtlpChannel;
            else
            {
                looping.Progress.SetMessage($"正在为获取记录 {strPath} 准备通道 ...");

                channel = this.DtlpChannels.CreateChannel(0);
                bNewChannel = true;

                looping.Progress.SetMessage($"正在获取记录 {strPath} ...");

                /*
                temp_stop = new Stop();
                temp_stop.Tag = channel;
                temp_stop.Register(MainForm.stopManager, true);	// 和容器关联

                temp_stop.OnStop += new StopEventHandler(this.DoNewStop);
                temp_stop.Initial("正在初始化浏览器组件 ...");
                temp_stop.BeginLoop();
                */
            }

            byte[] baPackage = null;
            Encoding encoding = null;
            try
            {
                nRet = channel.Search(strPath,
                    nStyle,
                    out baPackage);
                if (nRet == -1)
                {
                    errorcode = channel.GetLastErrno();

                    if (errorcode == DtlpChannel.GL_NOTEXIST
                        && (strDirection == "prev" || strDirection == "next"))
                    {
                        if (strDirection == "prev")
                            strError = "到头";
                        else if (strDirection == "next")
                            strError = "到尾";
                        goto ERROR1;
                    }
                    strError = "检索出错:\r\n"
                        + "检索式: " + strPath + "\r\n"
                        + "错误码: " + errorcode + "\r\n"
                        + "错误信息: " + DtlpChannel.GetErrorString(errorcode) + "\r\n";
                    goto ERROR1;
                }

                encoding = channel.GetPathEncoding(strPath);
            }
            finally
            {
                if (bNewChannel == true)
                {
                    /*
                    temp_stop.EndLoop();
                    temp_stop.OnStop -= new StopEventHandler(this.DoNewStop);
                    temp_stop.Initial("");
                    */
                    this.DtlpChannels.DestroyChannel(channel);
                    channel = null;

                    /*
                    temp_stop.Unregister();	// 和容器关联
                    temp_stop = null;
                    */
                }
                EndLoop(looping);
            }
            Package package = new Package();
            package.LoadPackage(baPackage,
                encoding);
            nRet = package.Parse(PackageFormat.Binary);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            strOutputPath = package.GetFirstPath();

            nRet = package.GetFirstBin(out byte[] content);
            if (nRet == -1)
            {
                strError = "Package::GetFirstBin() error";
                goto ERROR1;
            }

            if (content == null
                || content.Length < 9)
            {
                strError = "content length < 9";
                goto ERROR1;
            }

            baTimestamp = new byte[9];
            Array.Copy(content, baTimestamp, 9);

            byte[] marc = new byte[content.Length - 9];
            Array.Copy(content,
                9,
                marc,
                0,
                content.Length - 9);

            // strMARC = this.CurrentEncoding.GetString(marc);
            strMARC = encoding.GetString(marc);

            // 去掉最后若干连续的29字符或者0字符
            // 2008/3/11
            int nDelta = 0;
            for (int i = strMARC.Length - 1; i > 24; i--)
            {
                char ch = strMARC[i];
                if (ch == 0 || ch == 29)
                    nDelta++;
                else
                    break;
            }

            if (nDelta > 0)
                strMARC = strMARC.Substring(0, strMARC.Length - nDelta);

            // 自动识别MARC格式
            string strOutMarcSyntax = "";
            // 探测记录的MARC格式 unimarc / usmarc / dt1000reader
            // return:
            //      0   没有探测出来。strMarcSyntax为空
            //      1   探测出来了
            nRet = MarcUtil.DetectMarcSyntax(strMARC,
                out strOutMarcSyntax);
            if (strOutMarcSyntax == "")
                strOutMarcSyntax = "unimarc";

            record = new DigitalPlatform.OldZ3950.Record();
            if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                record.m_strSyntaxOID = "1.2.840.10003.5.1";
            else if (strOutMarcSyntax == "usmarc")
                record.m_strSyntaxOID = "1.2.840.10003.5.10";
            else if (strOutMarcSyntax == "dt1000reader")
                record.m_strSyntaxOID = "1.2.840.10003.5.dt1000reader";
            else
            {
                /*
                strError = "未知的MARC syntax '" + strOutMarcSyntax + "'";
                goto ERROR1;
                 * */
                // TODO: 可以出现菜单选择
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int GetAccessPoint(
            string strPath,
            string strMARC,
            out List<string> results,
            out string strError)
        {
            strError = "";
            // 获得一条记录的检索点
            return this.DtlpChannel.GetAccessPoint(strPath,
                strMARC,
                out results,
                out strError);
        }

        private void DtlpSearchForm_Load(object sender, EventArgs e)
        {
            EventLoadFinish.Reset();

            this.BinDir = Environment.CurrentDirectory;

#if NO
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // 初始化ChannelArray
            DtlpChannels.appInfo = MainForm.AppInfo;
            DtlpChannels.AskAccountInfo += new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);
            /*
            channelArray.procAskAccountInfo = new Delegate_AskAccountInfo(
                this.AskAccountInfo);
             * */

            // 准备唯一的通道
            if (this.DtlpChannel == null)
            {
                this.DtlpChannel = DtlpChannels.CreateChannel(0);
            }

            this.dtlpResDirControl1.channelarray = DtlpChannels;
            dtlpResDirControl1.Channel = this.DtlpChannel;
            dtlpResDirControl1.Stop = null; // this.stop;

            /*
            dtlpResDirControl1.procItemSelected = new Delegate_ItemSelected(
                this.ItemSelected);
            dtlpResDirControl1.procItemText = new Delegate_ItemText(
                this.ItemText);
             * */
            dtlpResDirControl1.FillSub(null);

            /*
             * 需要异步执行，避免窗口长时间打不开
            string strLastTargetPath = MainForm.applicationInfo.GetString(
                "dtlpsearchform",
                "last_targetpath",
                "");
            if (String.IsNullOrEmpty(strLastTargetPath) == false)
            {
                this.dtlpResDirControl1.SelectedPath = strLastTargetPath;
            }*/

            // 按照上次保存的路径展开resdircontrol树
            string strResDirPath = this.MainForm.AppInfo.GetString(
                "dtlpsearchform",
                "last_targetpath",
                "");
            if (String.IsNullOrEmpty(strResDirPath) == false)
            {
                object[] pList = { strResDirPath };

                this.BeginInvoke(new Delegate_ExpandResDir(ExpandResDir),
                    pList);
            }
            else
            {
                this.EventLoadFinish.Set();
            }

            string strQueryWord = MainForm.AppInfo.GetString(
                "dtlpsearchform",
                "query_content",
                "");
            this.textBox_queryWord.Text = strQueryWord;

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        /// <summary>
        /// 等待装载结束
        /// </summary>
        public void WaitLoadFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventLoadFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        public delegate void Delegate_ExpandResDir(string strResDirPath);

        void ExpandResDir(string strLastTargetPath)
        {
            this.Update();

            this.EnableControls(false);

            // 展开到指定的节点
            if (String.IsNullOrEmpty(strLastTargetPath) == false)
            {
                Debug.Assert(this.dtlpResDirControl1.PathSeparator == "\\", "");
                this.dtlpResDirControl1.SelectedPath1 = strLastTargetPath;
            }

            this.EnableControls(true);

            this.EventLoadFinish.Set();
        }

        void channelArray_AskAccountInfo(object sender,
            AskDtlpAccountInfoEventArgs e)
        {
            e.Owner = null;
            e.UserName = "";
            e.Password = "";

            LoginDlg dlg = new LoginDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            AccountItem item = (AccountItem)AccountTable[e.Path];
            if (item == null)
            {
                item = new AccountItem();
                AccountTable.Add(e.Path, item);

                // 从配置文件中得到缺省账户
                item.UserName = MainForm.AppInfo.GetString(
                    "preference",
                    "defaultUserName",
                    "public");
                item.Password = MainForm.AppInfo.GetString(
                    "preference",
                    "defaultPassword",
                    "");
            }

            dlg.textBox_serverAddr.Text = e.Path;
            dlg.textBox_userName.Text = item.UserName;
            dlg.textBox_password.Text = item.Password;

            // 先登录一次再说
            {
                byte[] baResult = null;
                int nRet = e.Channel.API_ChDir(dlg.textBox_userName.Text,
                    dlg.textBox_password.Text,
                    e.Path,
                    out baResult);

                // 登录成功
                if (nRet > 0)
                {
                    e.Result = 2;
                    return;
                }
            }


            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.OK)
            {
                item.UserName = dlg.textBox_userName.Text;
                item.Password = dlg.textBox_password.Text;

                e.UserName = dlg.textBox_userName.Text;
                e.Password = dlg.textBox_password.Text;
                e.Owner = this;
                e.Result = 1;
                return;
            }

            e.Result = 0;
            return;
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 设置窗口尺寸状态
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                SizeStyle.All,
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);

                // 获得splitContainer_main的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "dtlpsearchform",
                    "splitContainer_main");

                // 获得splitContainer_up的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_up,
                    "dtlpsearchform",
                    "splitContainer_up");

                string strWidths = this.MainForm.AppInfo.GetString(
"dtlpsearchform",
"record_list_column_width",
"");
                if (String.IsNullOrEmpty(strWidths) == false)
                {
                    ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                        strWidths,
                        true);
                }
            }

#if REMOVED
            // 获得splitContainer_main的状态
            int nValue = MainForm.AppInfo.GetInt(
            "dtlpsearchform",
            "splitContainer_main",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_main.SplitterDistance = nValue;
                }
                catch
                {
                }
            }

            // 获得splitContainer_up的状态
            nValue = MainForm.AppInfo.GetInt(
            "dtlpsearchform",
            "splitContainer_up",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_up.SplitterDistance = nValue;
                }
                catch
                {
                }
            }
#endif

            // 2008/3/24
            if (this.dtlpResDirControl1.SelectedNode != null)
                this.dtlpResDirControl1.SelectedNode.EnsureVisible();
        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

                // 保存splitContainer_main的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "dtlpsearchform",
                    "splitContainer_main");

                this.MainForm.SaveSplitterPos(
                    this.splitContainer_up,
                    "dtlpsearchform",
                    "splitContainer_up");

                // 保存 listview 的各列宽度
                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.MainForm.AppInfo.SetString(
                    "dtlpsearchform",
                    "record_list_column_width",
                    strWidths);
#if REMOVED
                // 保存splitContainer_main的状态
                MainForm.AppInfo.SetInt(
                    "dtlpsearchform",
                    "splitContainer_main",
                    this.splitContainer_main.SplitterDistance);
                // 保存splitContainer_up的状态
                MainForm.AppInfo.SetInt(
                    "dtlpsearchform",
                    "splitContainer_up",
                    this.splitContainer_up.SplitterDistance);
#endif
            }
        }

        private void DtlpSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void DtlpSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                Debug.Assert(this.dtlpResDirControl1.PathSeparator == "\\", "");
                string strLastTargetPath = TreeViewUtil.GetPath(this.dtlpResDirControl1.SelectedNode,
                    '\\');
                MainForm.AppInfo.SetString(
                    "dtlpsearchform",
                    "last_targetpath",
                    strLastTargetPath);

                MainForm.AppInfo.SetString(
                    "dtlpsearchform",
                    "query_content",
                    this.textBox_queryWord.Text);
            }

            DtlpChannels.AskAccountInfo -= new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);

            SaveSize();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.DtlpChannel != null)
                this.DtlpChannel.Cancel();
        }

        // 获得浏览记录内容
        // 注：使用现有的this.DplpChannel
        int GetOneBrowseRecord(
            string strPath,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = null;

            int nRet = 0;

            int nStyle = DtlpChannel.JH_STYLE; // 获得简化记录

            byte[] baPackage;
            nRet = this.DtlpChannel.Search(strPath,
                nStyle,
                out baPackage);
            if (nRet == -1)
            {
                strError = "Search() path '" + strPath + "' 时发生错误: " + this.DtlpChannel.GetErrorString();
                goto ERROR1;
            }

            Package package = new Package();
            package.LoadPackage(baPackage,
                this.DtlpChannel.GetPathEncoding(strPath));
            nRet = package.Parse(PackageFormat.String);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            string strContent = package.GetFirstContent();

            if (String.IsNullOrEmpty(strContent) == false)
            {
                cols = strContent.Split(new char[] { '\t' });
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 继续检索的路径
        string _queryPath = null;
        // 继续检索的附加部分
        byte[] _queryNext = null;

        void ClearContinueSearchParam()
        {
            _queryPath = null;
            _queryNext = null;
        }

        bool IsContinueSearchParamNull()
        {
            return _queryPath == null;
        }

        int m_nInSearching = 0; // 表示this.DtlpChannel是否被占用

        /*
发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“System.Net.Sockets.NetworkStream”。
Stack:
在 System.Net.Sockets.NetworkStream.EndRead(IAsyncResult asyncResult)
在 DigitalPlatform.DTLP.HostEntry.RecvTcpPackage(Byte[]& baPackage, Int32& nLen, Int32& nErrorNo)
在 DigitalPlatform.DTLP.DtlpChannel.API_Search(String strPath, Int32 lStyle, Byte[]& baResult)
在 DigitalPlatform.DTLP.DtlpChannel.Search(String strPath, Int32 lStyle, Byte[]& baResult)
在 dp2Catalog.DtlpSearchForm.GetOneBrowseRecord(String strPath, String[]& cols, String& strError)
在 dp2Catalog.DtlpSearchForm.FillBrowseList(Package package, String& strError)
在 dp2Catalog.DtlpSearchForm.DoSearch()
在 dp2Catalog.MainForm.toolButton_search_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

         * */
        // 检索
        // parameters:
        //      continueSearch  本次是否为继续检索
        public int DoSearch(bool continueSearch = false)
        {
            string strError = "";
            int nRet = 0;

            this._processing++;
            try
            {
                if (continueSearch == false)
                    ClearContinueSearchParam();
                else
                {
                    if (IsContinueSearchParamNull())
                    {
                        strError = "当前“继续检索”参数为空，无法以继续检索方式进行检索";
                        goto ERROR1;
                    }
                }

                byte[] baNext = null;
                int nStyle = DtlpChannel.CTRLNO_STYLE;

                // nStyle |=  Channel.JH_STYLE;    // 获得简化记录

                {
                    if ((this.dtlpResDirControl1.SelectedMask & DtlpChannel.TypeStdbase) != 0)
                    {
                        this.strCurrentTargetPath = this.textBox_resPath.Text + "//";
                    }
                    else
                    {
                        this.strCurrentTargetPath = this.textBox_resPath.Text + "/";
                    }
                    this.strCurrentQueryWord = this.textBox_queryWord.Text;
                }

                string strPath = "";
                if (continueSearch)
                {
                    strPath = _queryPath;
                    baNext = _queryNext;
                    nStyle |= DtlpChannel.CONT_RECORD;
                }
                else
                {
                    strPath = this.strCurrentTargetPath + this.strCurrentQueryWord;
                    baNext = null;

                    this.listView_browse.Items.Clear();
                }

                EnableControls(false);

                /*
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.SetMessage("开始检索 ...");
                stop.BeginLoop();

                this.Update();
                this.MainForm.Update();
                */
                var looping = BeginLoop(this.DoStop, "开始检索 ...");

                this.m_nInSearching++;
                /*
                this.listView_browse.ListViewItemSorter = null; // 暂时屏蔽排序能力
                 * */

                try
                {
                    int nDupCount = 0;
                    this.listView_browse.Focus();   // 便于Excape中断

                    bool bFirst = true;       // 第一次检索
                    while (true)
                    {
                        Application.DoEvents(); // 出让界面控制权

                        if (looping.Stopped)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        Encoding encoding = this.DtlpChannel.GetPathEncoding(strPath);

                        this.CurrentEncoding = encoding;    // 记忆下来

                        byte[] baPackage;
                        if (bFirst == true && continueSearch == false)
                        {
                            looping.Progress.SetMessage(listView_browse.Items.Count.ToString() + " 去重:" + nDupCount.ToString() + " " + "正在检索 " + strPath);
                            nRet = this.DtlpChannel.Search(strPath,
                                nStyle,
                                out baPackage);
                        }
                        else
                        {
                            looping.Progress.SetMessage(listView_browse.Items.Count.ToString() + " 去重:" + nDupCount.ToString() + " " + "正在检索 " + strPath + " " + encoding.GetString(baNext));
                            nRet = this.DtlpChannel.Search(strPath,
                                baNext,
                                nStyle,
                                out baPackage);
                        }
                        if (nRet == -1)
                        {
                            int errorcode = this.DtlpChannel.GetLastErrno();
                            if (errorcode == DtlpChannel.GL_NOTEXIST)
                            {
                                /*
                                if (bFirst == true)
                                    break;
                                 * */
                                break;
                            }
                            strError = "检索出错:\r\n"
                                + "检索式: " + strPath + "\r\n"
                                + "错误码: " + errorcode + "\r\n"
                                + "错误信息: " + DtlpChannel.GetErrorString(errorcode) + "\r\n";
                            goto ERROR1;
                        }

                        ClearContinueSearchParam();

                        bFirst = false;

                        Package package = new Package();
                        package.LoadPackage(baPackage,
                            encoding/*this.Channel.GetPathEncoding(strPath)*/);
                        // nRet = package.Parse(PackageFormat.String);

                        nRet = package.Parse(PackageFormat.String);
                        if (nRet == -1)
                        {
                            strError = "Package::Parse() error";
                            goto ERROR1;
                        }

                        ///
                        nRet = FillBrowseList(
                            looping,
                            package,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        nDupCount += nRet;

                        if (package.ContinueString != "")
                        {
                            nStyle |= DtlpChannel.CONT_RECORD;
                            baNext = package.ContinueBytes;

                            // 记忆为了中断后继续检索用
                            {
                                _queryPath = strPath;
                                _queryNext = baNext;
                            }
                        }
                        else
                        {
                            ClearContinueSearchParam();
                            break;
                        }
                    }

                    this.textBox_resultInfo.Text = "命中记录 " + this.listView_browse.Items.Count.ToString() + " 条";
                }
                finally
                {
                    this.m_nInSearching--;
                    try
                    {
                        /*
                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                        */
                        EndLoop(looping);

                        EnableControls(true);

                        /*
                        // 提供排序能力
                        this.listView_browse.ListViewItemSorter = new ListViewBrowseItemComparer();
                         * */

                    }
                    catch { }

                }

                if (this.listView_browse.Items.Count > 0)
                    this.listView_browse.Focus();
                else
                    this.textBox_queryWord.Focus();

                return 0;
            }
            finally
            {
                this._processing--;
            }
        ERROR1:
            try // 防止最后退出时报错
            {
                this.textBox_resultInfo.Text = "命中记录 " + this.listView_browse.Items.Count.ToString() + " 条";
                this.textBox_resultInfo.Text += "\r\n" + strError;

                MessageBox.Show(this, strError);

                this.textBox_queryWord.Focus();
            }
            catch
            {
            }
            return -1;
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_queryWord.Enabled = bEnable;
            this.dtlpResDirControl1.Enabled = bEnable;
            // this.listView_browse.Enabled = bEnable;
            this.textBox_resPath.Enabled = bEnable;
            this.textBox_resultInfo.Enabled = bEnable;
        }

        // return:
        //      本次重复的记录数
        int FillBrowseList(
            Looping looping,
            Package package,
            out string strError)
        {
            strError = "";

            int nDupCount = 0;

            // 处理每条记录
            for (int i = 0; i < package.Count; i++)
            {
                Application.DoEvents(); // 出让界面控制权

                if (looping.Stopped)
                {
                    strError = "用户中断";
                    return -1;
                }

                Cell cell = (Cell)package[i];

                // 查重
                string strPath = DigitalPlatform.DTLP.Global.ModifyDtlpRecPath(cell.Path,
                    "");
                if (DetectDup(strPath) == true)
                {
                    nDupCount++;
                    continue;
                }

                ListViewItem item = new ListViewItem();
                item.Text = strPath;

                int nRet = GetOneBrowseRecord(
                    cell.Path,
                    out string[] cols,
                    out strError);
                if (nRet == -1)
                {
                    item.SubItems.Add(strError);
                    goto CONTINUE;
                }
                if (cols != null)
                {
                    // 确保列标题数量足够
                    ListViewUtil.EnsureColumns(this.listView_browse,
                        cols.Length,
                        200);
                    for (int j = 0; j < cols.Length; j++)
                    {
                        item.SubItems.Add(cols[j]);
                    }
                }

            CONTINUE:

                this.listView_browse.Items.Add(item);
                // this.listView_browse.UpdateItem(this.listView_browse.Items.Count - 1);

            }

            return nDupCount;
        }

        // 检查是否重了
        // 如果可能，用Hashtable来提高速度
        // return:
        //      true    重了
        //      false   没有重
        bool DetectDup(string strPath)
        {
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                if (this.listView_browse.Items[i].Text == strPath)
                    return true;
            }

            return false;
        }

        private void DtlpSearchForm_Activated(object sender, EventArgs e)
        {
            /*
            if (stop != null)
                MainForm.stopManager.Active(this.stop);
            */
            MainForm.stopManager.Active(this.TopLooping?.Progress);

            MainForm.SetMenuItemState();

            // 菜单
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            }
            else
            {
                MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                MainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;
            }

            MainForm.MenuItem_font.Enabled = false;



            // 工具条按钮
            if (this.listView_browse.SelectedItems.Count == 0)
            {
                MainForm.toolButton_saveTo.Enabled = false;
            }
            else
            {
                MainForm.toolButton_saveTo.Enabled = true;
            }
            MainForm.toolButton_save.Enabled = false;
            MainForm.toolButton_search.Enabled = true;
            MainForm.toolButton_prev.Enabled = false;
            MainForm.toolButton_next.Enabled = false;
            MainForm.toolButton_nextBatch.Enabled = false;

            MainForm.toolButton_getAllRecords.Enabled = false;

            MainForm.toolButton_delete.Enabled = false;
        }

        // 按照点击的栏目排序
        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            /*
            int nClickColumn = e.Column;

            // 排序
            this.listView_browse.ListViewItemSorter = new ListViewBrowseItemComparer(nClickColumn);

            this.listView_browse.ListViewItemSorter = null;
             * */
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_browse.ListViewItemSorter = null;

        }
        // 何时去重?

        // 浏览窗上双击鼠标
        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;
            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
            }

            LoadDetail(nIndex);
        }

        void LoadDetail(int index)
        {
            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this.MainForm;
            form.MainForm = this.MainForm;

            // MARC Syntax OID
            // 需要建立数据库配置参数，从中得到MARC格式
            ////form.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";   // UNIMARC

            form.Show();

            form.LoadRecord(this, index);
        }

        // 保存记录
        public int SaveMarcRecord(
            string strPath,
            string strMARC,
            byte[] baTimestamp,
            out string strOutputPath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;
            strOutputPath = "";

            int nRet = 0;

            if (baTimestamp == null)
                baTimestamp = new byte[9];

            // 如果路径表明为追加，这个风格是否要对应设置？

            int nWriteStyle = DtlpChannel.REPLACE_WRITE;

            // 判断路径是否为追加?
            {
                string strOutPath = "";
                // 正规化保存路径
                // return:
                //      -1  error
                //      0   为覆盖方式的路径
                //      1   为追加方式的路径
                nRet = DtlpChannel.CanonicalizeWritePath(strPath,
                    out strOutPath,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                    nWriteStyle = DtlpChannel.APPEND_WRITE;

                strPath = strOutPath;
            }

            string strOutputRecord = "";
            nRet = this.DtlpChannel.WriteMarcRecord(strPath,
                nWriteStyle,
                strMARC,
                baTimestamp,
                out strOutputRecord,
                out strOutputPath,
                out baOutputTimestamp,
                out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }


        // 保存记录
        public int DeleteMarcRecord(
            string strPath,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (baTimestamp == null)
                baTimestamp = new byte[9];

            nRet = this.DtlpChannel.DeleteMarcRecord(strPath,
                baTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        protected override bool ProcessDialogKey(
            Keys keyData)
        {

            if (keyData == Keys.Enter || keyData == Keys.LineFeed)
            {
                if (this.textBox_queryWord.Focused == true)
                {
                    DoSearch();
                }
                else if (this.listView_browse.Focused == true)
                {
                    listView_browse_DoubleClick(this, null);
                }

                return true;
            }
            if (keyData == Keys.Escape)
            {
                MainForm.stopManager.DoStopActive();
                return true;
            }

            return base.ProcessDialogKey(keyData);
            // return false;
        }

        private void dtlpResDirControl1_ItemSelected(object sender, ItemSelectedEventArgs e)
        {
            if ((e.Mask & DtlpChannel.TypeStdbase) != 0
    || (e.Mask & DtlpChannel.TypeFrom) != 0)
                this.textBox_resPath.Text = e.Path;
            else
                this.textBox_resPath.Text = "";

        }

        private void dtlpResDirControl1_GetItemTextStyle(object sender, GetItemTextStyleEventArgs e)
        {
            e.FontFace = "";
            e.FontSize = 0;
            e.FontStyle = FontStyle.Regular;

            if ((e.Mask & DtlpChannel.TypeStdbase) != 0
                || (e.Mask & DtlpChannel.TypeFrom) != 0
                || (e.Mask & DtlpChannel.TypeKernel) != 0)
            {
                e.Result = 0;
            }
            else
            {
                e.ForeColor = ControlPaint.LightLight(ForeColor);
                e.Result = 1;
            }
        }

        private void listView_browse_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;

            int nSelectedCount = 0;
            nSelectedCount = this.listView_browse.SelectedItems.Count;

            /*
            // ---
            var sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);
            */

            menuItem = new ToolStripMenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            menuItem = new ToolStripMenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("复制单列(&S)");
            if (this.listView_browse.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            for (int i = 0; i < this.listView_browse.Columns.Count; i++)
            {
                ToolStripMenuItem subMenuItem = new ToolStripMenuItem("复制列 '" + this.listView_browse.Columns[i].Text + "'");
                subMenuItem.Tag = i;
                subMenuItem.Click += new System.EventHandler(this.menu_copySingleColumnToClipboard_Click);
                menuItem.DropDownItems.Add(subMenuItem);
            }

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(string)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

#if REMOVED
            menuItem = new ToolStripMenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertBefore_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("粘贴[后插](&V)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertAfter_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);
#endif

            // ---
            var sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("移除所选择的 " + nSelectedCount.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("test");
            menuItem.Click += new System.EventHandler(this.menu_testAddItems_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("导出所选择的 " + nSelectedCount.ToString() + " 个事项到批控文件(&L) ...");
            menuItem.Click += new System.EventHandler(this.menu_exportSelectedItemsToBatchFile_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 保存原始记录到ISO2709文件
            menuItem = new ToolStripMenuItem($"保存到 MARC 文件 [{nSelectedCount}] [带操作历史](&S) ...");
            if (nSelectedCount > 0 /*&& this.m_bInSearching == false*/)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_history_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem($"保存到 MARC 文件 [{nSelectedCount}] [带日志文件](&L) ...");
            if (nSelectedCount > 0 /*&& this.m_bInSearching == false*/)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_log_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            menuItem = new ToolStripMenuItem("发生批控文件(&G) ...");
            menuItem.Click += new System.EventHandler(this.menu_generateToBatchFile_Click);
            contextMenu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("按照索引号段装载(&L) ...");
            menuItem.Click += new System.EventHandler(this.menu_generateFill_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.listView_browse, e.Location);
        }

        void menu_generateFill_Click(object sender, EventArgs e)
        {
            var shift = (Control.ModifierKeys == Keys.Shift);

            var batch_dlg = new GenerateBatchFileDialog();
            batch_dlg.Text = "按照索引号段装载";
            batch_dlg.StartPathLabel = "开始号码(7位数字)";
            batch_dlg.EndPathLabel = "结束号码(7位数字)";
            batch_dlg.ShowDialog();
            if (batch_dlg.DialogResult == DialogResult.Cancel)
                return;

            string strError = "";

            // 提取出两个号码
            int start_value = 0;
            int end_value = 0;
            {
                string start_number = batch_dlg.StartPath;
                if (start_number.Length != 7)
                {
                    strError = $"起始路径 '{start_number.Length}' 字符数应当为 7";
                    goto ERROR1;
                }
                if (StringUtil.IsNumber(start_number) == false)
                {
                    strError = $"起始 ID 号码 '{start_number}' 格式不合法：应当为纯数字";
                    goto ERROR1;
                }
                if (Int32.TryParse(start_number, out start_value) == false)
                {
                    strError = $"起始 ID 号码 '{start_number}' 解析出错";
                    goto ERROR1;
                }

                string end_number = batch_dlg.EndPath;
                if (end_number.Length != 7)
                {
                    strError = $"结束路径 '{end_number.Length}' 字符数应当为 7";
                    goto ERROR1;
                }
                if (StringUtil.IsNumber(end_number) == false)
                {
                    strError = $"结束 ID 号码 '{end_number}' 格式不合法：应当为纯数字";
                    goto ERROR1;
                }
                if (Int32.TryParse(end_number, out end_value) == false)
                {
                    strError = $"结束 ID 号码 '{end_number}' 解析出错";
                    goto ERROR1;
                }
            }

            // 获得树状目录上当前选择节点的路径
            string prefix = "";
            {
                if ((this.dtlpResDirControl1.SelectedMask & DtlpChannel.TypeStdbase) != 0)
                {
                    prefix = this.textBox_resPath.Text + "/ctlno/";
                }
                else
                {
                    strError = "请先选择一个数据库节点";
                    goto ERROR1;
                }
            }

            var looping = BeginLoop(this.DoStop, "正在根据号码范围装载记录 ...");

            EnableControls(false);
            this.listView_browse.BeginUpdate();
            try
            {
                this.listView_browse.Items.Clear();

                int count = 0;
                int succeed_count = 0;
                bool current_shift = false;

                looping.Progress.SetProgressRange(0, end_value - start_value + 1);

                for (int value = start_value; value <= end_value; value++)
                {
                    Application.DoEvents();

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if ((count % 1000) == 0)
                    {
                        current_shift = (Control.ModifierKeys == Keys.Shift);
                        if (shift == true || current_shift == true)
                        {
                            // shift 按下的情况下，中途始终不刷新显示
                        }
                        else
                        {
                            // 每隔 1000 行刷新一次显示
                            this.listView_browse.EndUpdate();
                            Application.DoEvents();
                            this.listView_browse.BeginUpdate();
                        }
                    }

                    var number = value.ToString().PadLeft(7, '0');
                    string strPath = prefix + number;
                    ListViewItem item = new ListViewItem();
                    item.Text = strPath;

                    if (shift == false && current_shift == false)
                    {
                        int nRet = GetOneBrowseRecord(
                            strPath,    // cell.Path,
                            out string[] cols,
                            out strError);
                        if (nRet == -1)
                        {
                            item.SubItems.Add(strError);
                            goto CONTINUE;
                        }
                        succeed_count++;

                        if (cols != null)
                        {
                            // 确保列标题数量足够
                            ListViewUtil.EnsureColumns(this.listView_browse,
                                cols.Length,
                                200);
                            for (int j = 0; j < cols.Length; j++)
                            {
                                item.SubItems.Add(cols[j]);
                            }
                        }
                    }

                CONTINUE:
                    this.listView_browse.Items.Add(item);

                    count++;
                    looping.Progress.SetProgressValue(count);
                }

                MessageBox.Show(this, $"按照指定号码范围装载完成，共创建 {count} 个记录路径，装载成功 {succeed_count} 个");
                return;
            }
            finally
            {
                this.listView_browse.EndUpdate();
                EnableControls(true);
                EndLoop(looping);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 发生批控文件。
        // 根据输入的起止记录路径，创建一个全新的批控文件
        void menu_generateToBatchFile_Click(object sender, EventArgs e)
        {
            var batch_dlg = new GenerateBatchFileDialog();
            batch_dlg.ShowDialog();
            if (batch_dlg.DialogResult == DialogResult.Cancel)
                return;

            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的批控文件名";
            dlg.OverwritePrompt = true;
            // dlg.FileName = strLocalPath == "" ? strID + ".res" : strLocalPath;
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "批控文件 (*.ctl)|*.ctl|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string strError = "";

            // 提取出前缀，和两个号码
            string prefix = "";
            int start_value = 0;
            int end_value = 0;
            {
                string startPath = batch_dlg.StartPath;
                int index = startPath.LastIndexOf("/");
                if (index == -1)
                {
                    strError = $"起始路径 '{startPath}' 中没有找到字符 '/'，解析失败";
                    goto ERROR1;
                }

                prefix = startPath.Substring(0, index + 1);
                string start_number = startPath.Substring(index + 1);
                if (start_number.Length != 7)
                {
                    strError = $"起始路径 '{startPath.Length}' 中的 ID '{start_number}' 其字符数应当为 7";
                    goto ERROR1;
                }
                if (StringUtil.IsNumber(start_number) == false)
                {
                    strError = $"起始 ID 号码 '{start_number}' 格式不合法：应当为纯数字";
                    goto ERROR1;
                }
                if (Int32.TryParse(start_number, out start_value) == false)
                {
                    strError = $"起始 ID 号码 '{start_number}' 解析出错";
                    goto ERROR1;
                }

                string endPath = batch_dlg.EndPath;
                if (endPath.Length != startPath.Length)
                {
                    strError = $"起始路径 '{startPath.Length}' 和结束路径 '{endPath}' 的字符数应当相等";
                    goto ERROR1;
                }

                if (endPath.StartsWith(prefix) == false)
                {
                    strError = $"起始路径 '{startPath}' 和结束路径 '{endPath}' 的前缀部分(最后一个斜杠字符以左的部分)应当完全一致";
                    goto ERROR1;
                }

                string end_number = endPath.Substring(index + 1);
                if (StringUtil.IsNumber(end_number) == false)
                {
                    strError = $"结束 ID 号码 '{end_number}' 格式不合法：应当为纯数字";
                    goto ERROR1;
                }
                if (Int32.TryParse(end_number, out end_value) == false)
                {
                    strError = $"结束 ID 号码 '{end_number}' 解析出错";
                    goto ERROR1;
                }
            }

            var looping = BeginLoop(this.DoStop, "正在发生号码内容 ...");

            this.listView_browse.Enabled = false;
            try
            {
                int count = 0;
                using (var stream = File.Create(dlg.FileName))
                {
                    looping.Progress.SetProgressRange(0, end_value - start_value + 1);

                    for (int value = start_value; value <= end_value; value++)
                    {
                        Application.DoEvents();

                        if (looping.Stopped)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        var text = prefix + value.ToString().PadLeft(7, '0');
                        var buffer = Encoding.GetEncoding("gb2312").GetBytes(text);
                        stream.Write(buffer, 0, buffer.Length);
                        var tail = new byte[1];
                        tail[0] = 0;
                        stream.Write(tail, 0, tail.Length);

                        count++;
                        looping.Progress.SetProgressValue(count);
                    }
                }

                MessageBox.Show(this, $"发生路径到批控文件 {dlg.FileName} 完成，共创建 {count} 个记录路径");
                return;
            }
            finally
            {
                this.listView_browse.Enabled = true;
                EndLoop(looping);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_exportSelectedItemsToBatchFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的批控文件名";
            dlg.OverwritePrompt = true;
            // dlg.FileName = strLocalPath == "" ? strID + ".res" : strLocalPath;
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "批控文件 (*.ctl)|*.ctl|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var prefix = InputDlg.GetInput(this,
                "请指定每个路径需要添加的前缀",
                "路径前缀:",
                "/TCPIP网络/x.x.x.x",
                this.Font);
            if (prefix == null)
                return;

            var looping = BeginLoop(this.DoStop, "开始导出批控文件 ...");

            this.listView_browse.Enabled = false;
            try
            {
                int count = 0;
                using (var stream = File.Create(dlg.FileName))
                {
                    looping.Progress.SetProgressRange(0, this.listView_browse.SelectedItems.Count);

                    foreach (ListViewItem item in this.listView_browse.SelectedItems)
                    {
                        Application.DoEvents();

                        if (looping.Stopped)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        string path = ListViewUtil.GetItemText(item, 0);
                        // 变成外部可以理解的状态
                        path = DigitalPlatform.DTLP.Global.ModifyDtlpRecPath(path, "ctlno");

                        var text = prefix + RemoveFirstLevel(path); // .Replace("//", "/ctlno/")
                        // string strCompletePath = ;
                        var buffer = Encoding.GetEncoding("gb2312").GetBytes(text);
                        stream.Write(buffer, 0, buffer.Length);
                        var tail = new byte[1];
                        tail[0] = 0;
                        stream.Write(tail, 0, tail.Length);

                        count++;
                        looping.Progress.SetProgressValue(count);
                    }
                }

                MessageBox.Show(this, $"导出到文件 {dlg.FileName} 完成，共导出 {count} 个记录路径");
                return;
            }
            finally
            {
                this.listView_browse.Enabled = true;
                EndLoop(looping);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static string RemoveFirstLevel(string text)
        {
            int index = text.IndexOf("/");
            if (index == -1)
                return text;
            return text.Substring(index);
        }

        void menu_cutToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this,
                "dp2SearchForm",
                this.listView_browse,
                true);
        }

        void menu_copyToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this,
                "dp2SearchForm",
                this.listView_browse,
                false);
        }

        void menu_copySingleColumnToClipboard_Click(object sender, EventArgs e)
        {
            int nColumn = (int)((ToolStripMenuItem)sender).Tag;

            Global.CopyLinesToClipboard(this, nColumn, this.listView_browse, false);
        }

#if REMOVED
        void menu_pasteFromClipboard_insertBefore_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "dp2SearchForm,AmazonSearchForm",
                this.listView_browse,
                true);

            ConvertPastedLines();
        }

        void menu_pasteFromClipboard_insertAfter_Click(object sender, EventArgs e)
        {
            Global.PasteLinesFromClipboard(this,
                "dp2SearchForm,AmazonSearchForm",
                this.listView_browse,
                false);

            ConvertPastedLines();
        }
#endif

        void menuItem_selectAll_Click(object sender,
    EventArgs e)
        {
            this.listView_browse.BeginUpdate();
            ListViewUtil.SelectAllItems(this.listView_browse);
            /*
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                this.listView_browse.Items[i].Selected = true;
            }
            */
            this.listView_browse.EndUpdate();
        }

        void menu_testAddItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.listView_browse.BeginUpdate();
            for (int i = 0; i < 100 * 1000; i++)
            {
                this.listView_browse.Items.Add(new ListViewItem($"{i + 1}"));
            }
            this.listView_browse.EndUpdate();
            this.Cursor = oldCursor;
        }

        // 从窗口中移走所选择的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            ListViewUtil.DeleteSelectedItems(this.listView_browse);
            /*
            for (int i = this.listView_browse.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_browse.Items.RemoveAt(this.listView_browse.SelectedIndices[i]);
            }
            */
            this.Cursor = oldCursor;
        }

        void menuItem_saveOriginRecordToIso2709_log_Click(object sender, EventArgs e)
        {
            this.SaveOriginRecordToIso2709("write_log_file");
        }

        void menuItem_saveOriginRecordToIso2709_history_Click(object sender, EventArgs e)
        {
            this.SaveOriginRecordToIso2709("write_oper_history");
        }

        public void SaveOriginRecordToIso2709(string style)
        {
            string strError = "";
            int nRet = 0;

            bool bControl = Control.ModifierKeys == Keys.Control;

            if (this.listView_browse.SelectedItems.Count == 0)
            {
                strError = "尚未选定要保存的记录";
                goto ERROR1;
            }

            bool write_log_file = StringUtil.IsInList("write_log_file", style);
            bool write_oper_history = StringUtil.IsInList("write_oper_history", style);

            Encoding preferredEncoding = this.CurrentEncoding;

            string strPreferedMarcSyntax = "unimarc";

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.FileName = MainForm.LastIso2709FileName;
            // dlg.CrLf = MainForm.LastCrLfIso2709;
            dlg.CrLfVisible = false;   // 2020/3/9
            dlg.RemoveField998 = MainForm.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            dlg.EncodingName =
                (String.IsNullOrEmpty(MainForm.LastEncodingName) == true ? GetEncodingForm.GetEncodingName(preferredEncoding) : MainForm.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);

            if (string.IsNullOrEmpty(strPreferedMarcSyntax) == false)
                dlg.MarcSyntax = strPreferedMarcSyntax;
            else
                dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;

            if (bControl == false)
                dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool unimarc_modify_100 = dlg.UnimarcModify100;

            Encoding targetEncoding = null;

            if (dlg.EncodingName == "MARC-8"
                && preferredEncoding.Equals(this.MainForm.Marc8Encoding) == false)
            {
                strError = "保存操作无法进行。只有在记录的原始编码方式为 MARC-8 时，才能使用这个编码方式保存记录。";
                goto ERROR1;
            }

            nRet = this.MainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            string strLastFileName = MainForm.LastIso2709FileName;
            string strLastEncodingName = MainForm.LastEncodingName;

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "DtlpSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "DtlpSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }
                }
            }

            TextWriter log_writer = null;
            string log_filename = dlg.FileName + ".log";
            if (write_log_file)
                log_writer = new StreamWriter(log_filename,
                    false,
                    Encoding.GetEncoding("gb2312"));

            MainForm.LastIso2709FileName = dlg.FileName;
            MainForm.LastCrLfIso2709 = dlg.CrLf;
            MainForm.LastEncodingName = dlg.EncodingName;
            MainForm.LastRemoveField998 = dlg.RemoveField998;

            this.EnableControls(false);

            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString() + " 开始保存 MARC 文件 " + dlg.FileName) + "</div>");
            if (write_log_file)
                this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode("操作过程会自动写入日志文件 " + log_filename) + "</div>");

            var looping = BeginLoop(this.DoStop, "正在保存到 MARC 文件 ...");

            Stream s = null;
            try
            {
                s = File.Open(MainForm.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            bool bHideMessageBox = false;
            Hashtable silently_errorcode_table = new Hashtable();   // 记忆需要静默跳过的错误码
            DialogResult error_result = DialogResult.No;

            int nCount = 0;

            try
            {
                looping.Progress.SetProgressRange(0, this.listView_browse.SelectedItems.Count);
                bool bAsked = false;

                int i = 0;
                int count = this.listView_browse.SelectedItems.Count;

                foreach (ListViewItem item in this.listView_browse.SelectedItems)
                {
                    Application.DoEvents(); // 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strPath = item.Text;
                    // 2024/6/6
                    // 变成外部可以理解的状态
                    strPath = DigitalPlatform.DTLP.Global.ModifyDtlpRecPath(strPath, "ctlno");

                    byte[] baTarget = null;

                REDO_GET:
                    // 获得一条MARC/XML记录
                    // parameters:
                    //      strPath 记录路径。格式为"中文图书/1 @服务器名"
                    //      strDirection    方向。为 prev/next/current之一。current可以缺省。
                    //      strOutputPath   [out]返回的实际路径。格式和strPath相同。
                    // return:
                    //      -1  error 包括not found
                    //      0   found
                    //      1   为诊断记录
                    nRet = InternalGetOneRecord(
                        "marc",
                        strPath,
                        "current",
                        out string strRecord,
                        out string strOutputPath,
                        out string strOutStyle,
                        out byte[] baTimestamp,
                        out DigitalPlatform.OldZ3950.Record record,
                        out Encoding currrentEncoding,
                        out int errorcode,
                        out strError);
                    if (nRet == -1)
                    {
                        WriteLog($"*** {strPath} 获取时出错: {strError}");

                        bool dialog_opened = false;
                        if (bHideMessageBox == false
                            || silently_errorcode_table.ContainsKey(errorcode) == false)
                        {
                            Application.DoEvents();

                            error_result = MessageDialog.Show(this,
    $"获得书目记录 {strPath} 时发生错误: {strError}。\r\n\r\n请问是否重试获得记录?\r\n[重试]重试 [跳过]跳过本条继续后面的处理 [中断]中断整个批导出过程",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button1,
    "以后不再提示，按本次的选择处理",
    ref bHideMessageBox,
    new string[] { "重试", "跳过", "中断" });

                            dialog_opened = true;
                        }

                        if (error_result == DialogResult.Yes)
                        {
                            WriteLog($"对记录 {strPath} 及错误码 {String.Format("0X{0,8:X}", errorcode)} 选择重试获取 ...", false);
                            goto REDO_GET;
                        }
                        if (error_result == DialogResult.No)
                        {
                            if (bHideMessageBox && dialog_opened)
                            {
                                silently_errorcode_table[errorcode] = 1;
                                WriteLog($"{strPath} 对错误码 {String.Format("0X{0,8:X}", errorcode)} 首次选择静默跳过 ...", false);
                            }
                            else
                            {
                                WriteLog($"{strPath} 被跳过", false);
                            }

                            goto CONTINUE;
                        }

                        goto ERROR1;
                    }

                    WriteLog($"成功导出 {strPath}", false);

                    string strMarcSyntax = "";

                    if (dlg.MarcSyntax == "<自动>")
                    {
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        else if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";
                        else
                            strMarcSyntax = "unimarc";

                        if (strMarcSyntax == "unimarc"
                            && dlg.Mode880 == true
                            && bAsked == false)
                        {
                            DialogResult result = MessageBox.Show(this,
"书目记录 " + strPath + " 的 MARC 格式为 UNIMARC，在保存对话框选择“<自动>”的情况下，在保存前将不会被处理为 880 模式。如果确需在保存前处理为 880 模式，请终止当前操作，重新进行一次保存，注意在保存对话框中明确选择 “USMARC” 格式。\r\n\r\n请问是否继续处理? \r\n\r\n(Yes 继续处理，UNIMARC 格式记录不会处理为 880 模式；\r\nNo 中断整批保存操作)",
"DtlpSearchForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto END1;
                            bAsked = true;
                        }
                    }
                    else
                    {
                        strMarcSyntax = dlg.MarcSyntax;
                        // TODO: 检查常用字段名和所选定的 MARC 格式是否矛盾。如果矛盾给出警告
                    }

                    Debug.Assert(strMarcSyntax != "", "");

                    if (dlg.RemoveField998 == true)
                    {
                        MarcRecord temp = new MarcRecord(strRecord);
                        temp.select("field[@name='998']").detach();
                        temp.select("field[@name='997']").detach();
                        strRecord = temp.Text;
                    }

                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord temp = new MarcRecord(strRecord);
                        MarcQuery.To880(temp);
                        strRecord = temp.Text;
                    }

                    // 添加 -01 字段
                    {
                        MarcRecord temp = new MarcRecord(strRecord);
                        temp.select("field[@name='-01']").detach();
                        temp.setFirstField("-01", "", $"{strOutputPath}|{ByteArray.GetHexTimeStampString(baTimestamp)}");
                        strRecord = temp.Text;
                    }

                    // 将MARC机内格式转换为ISO2709格式
                    // parameters:
                    //      strSourceMARC   [in]机内格式MARC记录。
                    //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                    //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                    //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToISO2709(
                        strRecord,
                        strMarcSyntax,
                        targetEncoding,
                        unimarc_modify_100 ? "unimarc_100" : "",
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }

                CONTINUE:
                    nCount++;

                    looping.Progress.SetProgressValue(i + 1);
                    looping.Progress.SetMessage($"正在导出 {strPath}  {i + 1}/{count} ...");
                    i++;
                }
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();
                log_writer?.Close();

                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                */
                EndLoop(looping);

                this.EnableControls(true);

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString() + " 结束保存 MARC 文件 " + dlg.FileName) + "</div>");
            }

        END1:
            // 
            if (bAppend == true)
                MainForm.MessageText = nCount.ToString()
                    + "条记录成功追加到文件 " + MainForm.LastIso2709FileName + " 尾部";
            else
                MainForm.MessageText = nCount.ToString()
                    + "条记录成功保存到新文件 " + MainForm.LastIso2709FileName + " 尾部";

            return;
        ERROR1:
            this.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString() + $" 出错: {strError}") + "</div>");
            MessageBox.Show(this, strError);
            return;

            void WriteLog(string text, bool error = true)
            {
                if (write_log_file)
                    log_writer?.WriteLine($"{DateTime.Now.ToLongTimeString()} {text}");
                if (write_oper_history)
                {
                    if (error)
                        this.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(text) + "</div>");
                    else
                        this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(text) + "</div>");
                }
            }
        }


        /*
        protected override bool ProcessDialogChar(
            char charCode)
        {

            if (charCode == '\r')
            {
                if (this.textBox_queryWord.Focused == true)
                {
                    DoSearch();
                }
                else if (this.listView_browse.Focused == true)
                {
                    listView_browse_DoubleClick(this, null);
                }

                return true;
            }
            if (charCode == (char)((int)Keys.Escape))
            {
                MainForm.stopManager.DoStopActive();

                // this.DoStop(this, null);
                return true;
            }

            return false;
        }
         * */


#if NOOOOOOOOOOOOOOOOOO
                public void ItemSelected(string strPath, Int32 nMask)
        {
            if ((nMask & DtlpChannel.TypeStdbase) != 0
                || (nMask & DtlpChannel.TypeFrom) != 0)
                this.textBox_resPath.Text = strPath;
            else
                this.textBox_resPath.Text = "";

        }

        // 请求给出Item文字参数
        public int ItemText(string strPath,
            Int32 nMask,
            out string strFontFace,
            out int nFontSize,
            out FontStyle FontStyle,
            ref Color ForeColor)
        {
            strFontFace = "";
            nFontSize = 0;
            FontStyle = FontStyle.Regular;

            if ((nMask & DtlpChannel.TypeStdbase) != 0
                || (nMask & DtlpChannel.TypeFrom) != 0)
            {

                /*
                strFontFace = "宋体";
                nFontSize = 12;
                FontStyle = FontStyle.Bold;
                ForeColor = Color.Red;
                */

                return 0;
            }
            else
            {
                ForeColor = ControlPaint.LightLight(ForeColor);
                return 1;
            }

        }
#endif
    }

    public class AccountItem
    {
        public string UserName = "public";
        public string Password = "";
        public bool SavePassword = false;
    }

    // Implements the manual sorting of items by columns.
    class ListViewBrowseItemComparer : IComparer
    {
        private int col = 0;
        public ListViewBrowseItemComparer()
        {
            col = 0;
        }
        public ListViewBrowseItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
        }
    }
}