using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.IO;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.DTLP;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.GUI;

namespace dp2Catalog
{
    public partial class DtlpLogForm : Form
    {
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        DtlpChannelArray channelArray = new DtlpChannelArray();
        DtlpChannel Channel = null;	// 尽量使用一个通道
        Hashtable AccountTable = new Hashtable();

        public DtlpLogForm()
        {
            InitializeComponent();

            channelArray.Idle -= new DtlpIdleEventHandler(channelArray_Idle);
            channelArray.Idle += new DtlpIdleEventHandler(channelArray_Idle);
        }

        void channelArray_Idle(object sender, DtlpIdleEventArgs e)
        {
            e.bDoEvents = true;
        }

        private void DtlpLogForm_Load(object sender, EventArgs e)
        {
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联


            // 初始化ChannelArray
            channelArray.appInfo = MainForm.AppInfo;
            channelArray.AskAccountInfo += new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);
            /*
            channelArray.procAskAccountInfo = new Delegate_AskAccountInfo(
                this.AskAccountInfo);
             * */

            // 准备唯一的通道
            if (this.Channel == null)
            {
                this.Channel = channelArray.CreateChannel(0);
            }

            this.textBox_serverAddr.Text = MainForm.AppInfo.GetString(
    "dtlplogform",
    "serveraddr",
    ""); 

            this.textBox_logFileName.Text = MainForm.AppInfo.GetString(
    "dtlplogform",
    "logfilename",
    "");

            this.marcEditor_record.FieldNameCaptionWidth = 0;

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

        private void DtlpLogForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void DtlpLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器关联
                stop = null;
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SetString(
        "dtlplogform",
        "serveraddr",
        this.textBox_serverAddr.Text);

                MainForm.AppInfo.SetString(
    "dtlplogform",
    "logfilename",
    this.textBox_logFileName.Text);
            }

            channelArray.AskAccountInfo -= new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);
        }

        /*
        // 获得缺省帐户信息。回调函数，用于resdircontrol
        // return:
        //		2	already login succeed
        //		1	dialog return OK
        //		0	dialog return Cancel
        //		-1	other error
        public int AskAccountInfo(DtlpChannel channel,
            string strPath,
            out IWin32Window owner,	// 如果需要出现对话框，这里返回对话框的宿主Form
            out string strUserName,
            out string strPassword)
        {
            owner = null;
            strUserName = "";
            strPassword = "";

            LoginDlg dlg = new LoginDlg();

            AccountItem item = (AccountItem)AccountTable[strPath];
            if (item == null)
            {
                item = new AccountItem();
                AccountTable.Add(strPath, item);

                // 从配置文件中得到缺省账户
                item.UserName = MainForm.applicationInfo.GetString(
                    "preference",
                    "defaultUserName",
                    "public");
                item.Password = MainForm.applicationInfo.GetString(
                    "preference",
                    "defaultPassword",
                    "");


            }

            dlg.textBox_serverAddr.Text = strPath;
            dlg.textBox_userName.Text = item.UserName;
            dlg.textBox_password.Text = item.Password;

            // 先登录一次再说
            {
                byte[] baResult = null;
                int nRet = channel.API_ChDir(dlg.textBox_userName.Text,
                    dlg.textBox_password.Text,
                    strPath,
                    out baResult);

                // 登录成功
                if (nRet > 0)
                    return 2;
            }




            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.OK)
            {
                item.UserName = dlg.textBox_userName.Text;
                item.Password = dlg.textBox_password.Text;

                strUserName = dlg.textBox_userName.Text;
                strPassword = dlg.textBox_password.Text;
                owner = this;
                return 1;
            }

            return 0;
        }
         * */

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Cancel();
        }

        // 装载
        private void button_load_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_serverAddr.Text == "")
            {
                strError = "尚未指定服务器地址";
                goto ERROR1;
            }

            if (this.textBox_logFileName.Text == "")
            {
                strError = "尚未指定日志文件名";
                goto ERROR1;
            }

            this.listView_records.Items.Clear();

            int nRet = GetLogRecords(this.textBox_serverAddr.Text,
                    this.textBox_logFileName.Text,
                    out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得日志记录
        // return:
        //      -1  出错
        //      0   日志文件不存在
        //      1   日志文件存在
        int GetLogRecords(string strServerAddr,
            string strLogFileName,
            out string strError)
        {
            strError = "";
            int nStartIndex = 0;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在从服务器获得日志记录 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {

                string strPath = strServerAddr + "/log/" + strLogFileName + "/" + nStartIndex.ToString();

                bool bFirst = true;

                string strDate = "";
                int nRecID = -1;
                string strOffset = "";

                int nStyle = 0;

                for (int i = nStartIndex; ; i++)
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

                    byte[] baPackage = null;

                    if (bFirst == true)
                    {
                    }
                    else
                    {
                        strPath = strServerAddr + "/log/" + strDate/*strLogFileName*/ + "/" + nRecID.ToString() + "@" + strOffset;
                    }

                    Encoding encoding = this.Channel.GetPathEncoding(strPath);

                    stop.SetMessage("正在获得日志记录 " + strPath);

                    int nRet = this.Channel.Search(strPath,
                        DtlpChannel.RIZHI_STYLE | nStyle,
                        out baPackage);
                    if (nRet == -1)
                    {
                        int errorcode = this.Channel.GetLastErrno();
                        if (errorcode == DtlpChannel.GL_NOTEXIST)
                        {
                            if (bFirst == true)
                                break;
                        }

                        // 更换新通道
                        if (errorcode == DtlpChannel.GL_INTR
                            || errorcode == DtlpChannel.GL_SEND
                            || errorcode == DtlpChannel.GL_RECV)
                        {
                            this.Channel = channelArray.CreateChannel(0);
                        }

                        strError = "获取日志记录:\r\n"
                            + "路径: " + strPath + "\r\n"
                            + "错误码: " + errorcode + "\r\n"
                            + "错误信息: " + DtlpChannel.GetErrorString(errorcode) + "\r\n";
                        return -1;
                    }


                    // 解析出记录
                    Package package = new Package();
                    package.LoadPackage(baPackage,
                        encoding);
                    package.Parse(PackageFormat.Binary);

                    // 获得下一路径
                    string strNextPath = "";
                    strNextPath = package.GetFirstPath();
                    if (String.IsNullOrEmpty(strNextPath) == true)
                    {
                        if (this.checkBox_loop.Checked == true)
                        {
                            i--;
                            continue;
                        }

                        if (bFirst == true)
                        {
                            strError = "文件 " + strLogFileName + "不存在";
                            return 0;
                        }
                        // strError = "检索 '" + strPath + "' 响应包中路径部分不存在 ...";
                        // return -1;
                        break;
                    }

                    // 获得记录内容
                    byte[] baContent = null;
                    nRet = package.GetFirstBin(out baContent);
                    if (nRet != 1)
                    {
                        baContent = null;	// 但是为空包
                    }



                    // 处理记录


                    string strMARC = DtlpChannel.GetDt1000LogRecord(baContent, encoding);

                    string strOperCode = "";
                    string strOperComment = "";
                    string strOperPath = "";

                    nRet = DtlpChannel.ParseDt1000LogRecord(strMARC,
                        out strOperCode,
                        out strOperComment,
                        out strOperPath,
                        out strError);
                    if (nRet == -1)
                    {
                        strOperComment = strError;
                    }

                    LogItemInfo info = new LogItemInfo();
                    info.Index = i;
                    info.Offset = GetStartOffs(strOffset);
                    info.OriginData = baContent;
                    info.Encoding = encoding;

                    ListViewItem item = new ListViewItem();
                    item.Text = i.ToString();
                    item.SubItems.Add(info.Offset);
                    item.SubItems.Add(strOperComment);
                    item.SubItems.Add(strOperPath);
                    item.Tag = info;

                    this.listView_records.Items.Add(item);

                    // 将日志记录路径解析为日期、序号、偏移
                    // 一个日志记录路径的例子为:
                    // /ip/log/19991231/0@1234~5678
                    // parameters:
                    //		strLogPath		待解析的日志记录路径
                    //		strDate			解析出的日期
                    //		nRecID			解析出的记录号
                    //		strOffset		解析出的记录偏移，例如1234~5678
                    // return:
                    //		-1		出错
                    //		0		正确
                    nRet = DtlpChannel.ParseLogPath(strNextPath,
                        out strDate,
                        out nRecID,
                        out strOffset,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // ModiOffset(ref strOffset);

                    bFirst = false;
                }


                return 1;   // 日志文件存在，已获得了记录

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }



        // 把 a~b的第一部分替换为0
        static void ModiOffset(ref string strOffset)
        {
            int nRet = strOffset.IndexOf("~");
            if (nRet == -1)
                return;

            strOffset = "0~" + strOffset.Substring(nRet + 1);

            return;
        }

        // 返回'a~b'的后面部分
        static string GetStartOffs(string strOffs)
        {
            int nRet = strOffs.IndexOf("~");
            if (nRet == -1)
                return strOffs;

            return strOffs.Substring(nRet + 1);
        }

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                this.textBox_worksheet.Text = "";
                this.marcEditor_record.Marc = "";
                this.textBox_description.Text = "";
            }
            else
            {
                LogItemInfo info = (LogItemInfo)this.listView_records.SelectedItems[0].Tag;
                if (info != null)
                {
                    this.textBox_worksheet.Text = info.Encoding.GetString(info.OriginData).Replace(MarcUtil.SUBFLD, '$');
                    string strMARC = DtlpChannel.GetDt1000LogRecord(info.OriginData, info.Encoding);
                    this.marcEditor_record.Marc = strMARC;
                    this.marcEditor_record.DocumentOrgX = 0;
                    this.marcEditor_record.DocumentOrgY = 0;

                    string strOperCode = "";
                    string strOperComment = "";
                    string strOperPath = "";
                    string strError = "";

                    int nRet = DtlpChannel.ParseDt1000LogRecord(strMARC,
                        out strOperCode,
                        out strOperComment,
                        out strOperPath,
                        out strError);
                    if (nRet == -1)
                        this.textBox_description.Text = strError;
                    else
                    {
                        if (strOperCode == "12")
                            this.textBox_description.Text = "操作: " + strOperComment + "\r\n数据库名: " + strOperPath;
                        else
                            this.textBox_description.Text = "操作: " + strOperComment + "\r\n路径: " + strOperPath;
                    }
                        
                }
                else
                {
                    this.textBox_worksheet.Text = "(no origin data)";
                    this.marcEditor_record.Marc = "";
                    this.textBox_description.Text = "";
                }
            }
        }



    }

    public class LogItemInfo
    {
        public int Index = -1;
        public string Offset = "";
        public byte [] OriginData = null;
        public Encoding Encoding = null;
    }
}