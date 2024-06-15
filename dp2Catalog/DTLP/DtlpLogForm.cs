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
        DtlpChannel Channel = null;	// ����ʹ��һ��ͨ��
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
            stop.Register(MainForm.stopManager, true);	// ����������


            // ��ʼ��ChannelArray
            channelArray.appInfo = MainForm.AppInfo;
            channelArray.AskAccountInfo += new AskDtlpAccountInfoEventHandle(channelArray_AskAccountInfo);
            /*
            channelArray.procAskAccountInfo = new Delegate_AskAccountInfo(
                this.AskAccountInfo);
             * */

            // ׼��Ψһ��ͨ��
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

                // �������ļ��еõ�ȱʡ�˻�
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

            // �ȵ�¼һ����˵
            {
                byte[] baResult = null;
                int nRet = e.Channel.API_ChDir(dlg.textBox_userName.Text,
                    dlg.textBox_password.Text,
                    e.Path,
                    out baResult);

                // ��¼�ɹ�
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
            if (stop != null) // �������
            {
                stop.Style = StopStyle.None;    // ��Ҫǿ���ж�
                stop.DoStop();

                stop.Unregister();	// ����������
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
        // ���ȱʡ�ʻ���Ϣ���ص�����������resdircontrol
        // return:
        //		2	already login succeed
        //		1	dialog return OK
        //		0	dialog return Cancel
        //		-1	other error
        public int AskAccountInfo(DtlpChannel channel,
            string strPath,
            out IWin32Window owner,	// �����Ҫ���ֶԻ������ﷵ�ضԻ��������Form
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

                // �������ļ��еõ�ȱʡ�˻�
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

            // �ȵ�¼һ����˵
            {
                byte[] baResult = null;
                int nRet = channel.API_ChDir(dlg.textBox_userName.Text,
                    dlg.textBox_password.Text,
                    strPath,
                    out baResult);

                // ��¼�ɹ�
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

        // װ��
        private void button_load_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_serverAddr.Text == "")
            {
                strError = "��δָ����������ַ";
                goto ERROR1;
            }

            if (this.textBox_logFileName.Text == "")
            {
                strError = "��δָ����־�ļ���";
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

        // �����־��¼
        // return:
        //      -1  ����
        //      0   ��־�ļ�������
        //      1   ��־�ļ�����
        int GetLogRecords(string strServerAddr,
            string strLogFileName,
            out string strError)
        {
            strError = "";
            int nStartIndex = 0;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("���ڴӷ����������־��¼ ...");
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
                    Application.DoEvents();	// ���ý������Ȩ

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "�û��ж�";
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

                    stop.SetMessage("���ڻ����־��¼ " + strPath);

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

                        // ������ͨ��
                        if (errorcode == DtlpChannel.GL_INTR
                            || errorcode == DtlpChannel.GL_SEND
                            || errorcode == DtlpChannel.GL_RECV)
                        {
                            this.Channel = channelArray.CreateChannel(0);
                        }

                        strError = "��ȡ��־��¼:\r\n"
                            + "·��: " + strPath + "\r\n"
                            + "������: " + errorcode + "\r\n"
                            + "������Ϣ: " + DtlpChannel.GetErrorString(errorcode) + "\r\n";
                        return -1;
                    }


                    // ��������¼
                    Package package = new Package();
                    package.LoadPackage(baPackage,
                        encoding);
                    package.Parse(PackageFormat.Binary);

                    // �����һ·��
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
                            strError = "�ļ� " + strLogFileName + "������";
                            return 0;
                        }
                        // strError = "���� '" + strPath + "' ��Ӧ����·�����ֲ����� ...";
                        // return -1;
                        break;
                    }

                    // ��ü�¼����
                    byte[] baContent = null;
                    nRet = package.GetFirstBin(out baContent);
                    if (nRet != 1)
                    {
                        baContent = null;	// ����Ϊ�հ�
                    }



                    // �����¼


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

                    // ����־��¼·������Ϊ���ڡ���š�ƫ��
                    // һ����־��¼·��������Ϊ:
                    // /ip/log/19991231/0@1234~5678
                    // parameters:
                    //		strLogPath		����������־��¼·��
                    //		strDate			������������
                    //		nRecID			�������ļ�¼��
                    //		strOffset		�������ļ�¼ƫ�ƣ�����1234~5678
                    // return:
                    //		-1		����
                    //		0		��ȷ
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


                return 1;   // ��־�ļ����ڣ��ѻ���˼�¼

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }



        // �� a~b�ĵ�һ�����滻Ϊ0
        static void ModiOffset(ref string strOffset)
        {
            int nRet = strOffset.IndexOf("~");
            if (nRet == -1)
                return;

            strOffset = "0~" + strOffset.Substring(nRet + 1);

            return;
        }

        // ����'a~b'�ĺ��沿��
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
                            this.textBox_description.Text = "����: " + strOperComment + "\r\n���ݿ���: " + strOperPath;
                        else
                            this.textBox_description.Text = "����: " + strOperComment + "\r\n·��: " + strOperPath;
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