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

// using System.Reflection;
//using DigitalPlatform.Script;

using DigitalPlatform;
using DigitalPlatform.DTLP;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
//using DigitalPlatform.IO;
using DigitalPlatform.GUI;

namespace dp2Catalog
{
    public partial class DtlpSearchForm : Form, ISearchForm
    {
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        public string BinDir = "";

        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

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
            out DigitalPlatform.Z3950.Record record,
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
            out DigitalPlatform.Z3950.Record record,
            out Encoding currrentEncoding,
            out string strError)
        {
            strMARC = "";
            record = null;
            strError = "";
            currrentEncoding = this.CurrentEncoding;
            baTimestamp = null;
            strOutStyle = "marc";
            strOutputPath = ""; // TODO: 需要参考dp1batch看获得outputpath的方法

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
            Stop temp_stop = this.stop;
            DtlpChannel channel = null;

            bool bNewChannel = false;
            if (this.m_nInSearching == 0)
                channel = this.DtlpChannel;
            else
            {
                channel = this.DtlpChannels.CreateChannel(0);
                bNewChannel = true;

                temp_stop = new Stop();
                temp_stop.Tag = channel;
                temp_stop.Register(MainForm.stopManager, true);	// 和容器关联

                temp_stop.OnStop += new StopEventHandler(this.DoNewStop);
                temp_stop.Initial("正在初始化浏览器组件 ...");
                temp_stop.BeginLoop();

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
                    int errorcode = channel.GetLastErrno();

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
                        + "错误信息: " + channel.GetErrorString(errorcode) + "\r\n";
                    goto ERROR1;
                }

                encoding = channel.GetPathEncoding(strPath);
            }
            finally
            {
                if (bNewChannel == true)
                {
                    temp_stop.EndLoop();
                    temp_stop.OnStop -= new StopEventHandler(this.DoNewStop);
                    temp_stop.Initial("");

                    this.DtlpChannels.DestroyChannel(channel);
                    channel = null;


                    temp_stop.Unregister();	// 和容器关联
                    temp_stop = null;
                }
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
            // return:
            //      0   没有探测出来。strMarcSyntax为空
            //      1   探测出来了
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

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联


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
            dtlpResDirControl1.Stop = this.stop;

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
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);


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
                MainForm.AppInfo.SetInt(
                    "dtlpsearchform",
                    "splitContainer_main",
                    this.splitContainer_main.SplitterDistance);
                // 保存splitContainer_up的状态
                MainForm.AppInfo.SetInt(
                    "dtlpsearchform",
                    "splitContainer_up",
                    this.splitContainer_up.SplitterDistance);
            }
        }


        private void DtlpSearchForm_FormClosing(object sender, FormClosingEventArgs e)
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
        }

        private void DtlpSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器脱离关联
                stop = null;
            }

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
                cols = strContent.Split(new char[] {'\t'});
            }

            return 0;
        ERROR1:
            return -1;
        }

        int m_nInSearching = 0; // 表示this.DtlpChannel是否被占用

        // 检索
        public int DoSearch()
        {
            string strError = "";
            int nRet = 0;

            byte[] baNext = null;
            int nStyle = DtlpChannel.CTRLNO_STYLE;

            // nStyle |=  Channel.JH_STYLE;    // 获得简化记录


            string strPath = "";

            if ((this.dtlpResDirControl1.SelectedMask & DtlpChannel.TypeStdbase) != 0)
            {
                this.strCurrentTargetPath = this.textBox_resPath.Text + "//";
            }
            else
            {
                this.strCurrentTargetPath = this.textBox_resPath.Text + "/";
            }
            this.strCurrentQueryWord = this.textBox_queryWord.Text;

            strPath = this.strCurrentTargetPath + this.strCurrentQueryWord;

            this.listView_browse.Items.Clear();
            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("开始检索 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    Encoding encoding = this.DtlpChannel.GetPathEncoding(strPath);

                    this.CurrentEncoding = encoding;    // 记忆下来

                    byte[] baPackage;
                    if (bFirst == true)
                    {
                        stop.SetMessage(listView_browse.Items.Count.ToString() + " 去重:" + nDupCount.ToString() + " " + "正在检索 " + strPath );
                        nRet = this.DtlpChannel.Search(strPath,
                            nStyle,
                            out baPackage);
                    }
                    else
                    {
                        stop.SetMessage(listView_browse.Items.Count.ToString() + " 去重:" + nDupCount.ToString() + " " + "正在检索 " + strPath + " " + encoding.GetString(baNext));
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
                            + "错误信息: " + this.DtlpChannel.GetErrorString(errorcode) + "\r\n";
                        goto ERROR1;
                    }

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
                    nRet = FillBrowseList(package,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nDupCount += nRet;

                    if (package.ContinueString != "")
                    {
                        nStyle |= DtlpChannel.CONT_RECORD;
                        baNext = package.ContinueBytes;
                    }
                    else
                    {
                        break;
                    }

                }

                this.textBox_resultInfo.Text = "命中记录 "+this.listView_browse.Items.Count.ToString()+" 条";
            }

            finally
            {
                this.m_nInSearching--;
                try
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

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
        int FillBrowseList(Package package,
            out string strError)
        {
            strError = "";

            int nDupCount = 0;

            // 处理每条记录
            for (int i = 0; i < package.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
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


                string[] cols = null;
                int nRet = GetOneBrowseRecord(
                    cell.Path,
                    out cols,
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
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

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

            if (keyData == Keys.Enter)
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

            base.ProcessDialogKey(keyData);
            return false;
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