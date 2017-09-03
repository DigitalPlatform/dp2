using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Configuration.Install;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;

using DigitalPlatform.Install;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Interfaces;
//using DigitalPlatform.CirculationClient;

namespace DigitalPlatform.LibraryServer
{
    public partial class InstanceDialog : Form
    {
        public event CopyFilesEventHandler CopyFiles = null;

        public bool UninstallMode = false;

        public string SourceDir = "";   // 安装的程序文件目录。使用 SourceDir 的时候，这个目录下的 temp 子目录中应该是完整的数据文件目录

        public bool Changed = false;

        const int COLUMN_NAME = 0;
        const int COLUMN_ERRORINFO = 1;
        const int COLUMN_DATADIR = 2;
        const int COLUMN_BINDINGS = 3;

        private MessageBalloon m_firstUseBalloon = null;

        // string strCertificatSN = "";

        public InstanceDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 调试信息。过程信息
        /// </summary>
        public string DebugInfo
        {
            get;
            set;
        }

        private void InstanceDialog_Load(object sender, EventArgs e)
        {
            // Debug.Assert(false, "");

            // 卸载状态
            if (UninstallMode == true)
            {
                this.button_OK.Text = "卸载";
                this.button_newInstance.Visible = false;
                this.button_deleteInstance.Visible = false;
                this.button_modifyInstance.Visible = false;
                // this.button_certificate.Visible = false;
            }


            string strError = "";
            int nRet = FillInstanceList(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                // 安装状态
                if (UninstallMode == false
                    && this.listView_instance.Items.Count == 0)
                {
                    // 提示创建第一个实例
                    ShowMessageTip();
                }
            }

            /*
            this.strCertificatSN = InstallHelper.GetProductString(
                "dp2Library",
                "cert_sn");
             * */

            listView_instance_SelectedIndexChanged(null, null);

            this.BeginInvoke(new Action(RefreshInstanceState));
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.button_newInstance;
            m_firstUseBalloon.Title = "安装 dp2Library 图书馆应用服务器";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "请按此按钮创建第一个实例";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();
        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.Enabled = false;
            try
            {
                // HideMessageTip();

                // 全部卸载
                if (this.UninstallMode == true)
                {
                    DialogResult result = MessageBox.Show(this,
    "确实要卸载 dp2Library? \r\n\r\n(*** 警告：卸载后数据将全部丢失，并无法恢复 ***)",
    "卸载 dp2Library",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                        return;   // cancelled

                    //      -1  出错
                    //      0   放弃卸载
                    //      1   卸载成功
                    nRet = this.DeleteAllInstanceAndDataDir(out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                    if (nRet == 0)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }

                    this.Changed = false;

                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                    return;
                }

#if NO
                // 进行检查
                // return:
                //      -1  发现错误
                //      0   放弃整个保存操作
                //      1   一切顺利
                nRet = DoVerify(out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                nRet = DoModify(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif

                /*
                InstallHelper.SetProductString(
        "dp2Library",
        "cert_sn",
        this.strCertificatSN);
                 * */

                if (this.Changed == true)
                {
                    if (AfterChanged(out strError) == -1)
                        goto ERROR1;

                    this.Changed = false;
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            finally
            {
                this.Enabled = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // HideMessageTip();

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
#endif

        public string Comment
        {
            get
            {
                return this.textBox_Comment.Text;
            }
            set
            {
                this.textBox_Comment.Text = value;
            }
        }

        // 获得一个目前尚未被使用过的instancename值
        string GetNewInstanceName(int nStart)
        {
        REDO:
            string strResult = "instance" + nStart.ToString();
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                if (string.Compare(strResult, strInstanceName, true) == 0)
                {
                    nStart++;
                    goto REDO;
                }
            }

            return strResult;
        }

        private void button_newInstance_Click(object sender, EventArgs e)
        {
            string strError = "";

            HideMessageTip();

            OneInstanceDialog new_instance_dlg = new OneInstanceDialog();
            GuiUtil.AutoSetDefaultFont(new_instance_dlg);
            new_instance_dlg.Text = "创建一个新实例";
            new_instance_dlg.IsNew = true;
            if (this.listView_instance.Items.Count == 0)
            {
            }
            else
            {
                new_instance_dlg.InstanceName = GetNewInstanceName(this.listView_instance.Items.Count + 1);
            }

            new_instance_dlg.VerifyInstanceName += new VerifyEventHandler(new_instance_dlg_VerifyInstanceName);
            new_instance_dlg.VerifyDataDir += new VerifyEventHandler(new_instance_dlg_VerifyDataDir);
            new_instance_dlg.VerifyBindings += new VerifyEventHandler(new_instance_dlg_VerifyBindings);
            new_instance_dlg.LoadXmlFileInfo += new LoadXmlFileInfoEventHandler(new_instance_dlg_LoadXmlFileInfo);

            new_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
            if (new_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.Enabled = false;
            try
            {

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, new_instance_dlg.InstanceName);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, new_instance_dlg.DataDir);
                ListViewUtil.ChangeItemText(item, COLUMN_BINDINGS, new_instance_dlg.Bindings.Replace("\r\n", ";"));
                this.listView_instance.Items.Add(item);

                new_instance_dlg.LineInfo.Changed = true;
                item.Tag = new_instance_dlg.LineInfo;

                ListViewUtil.SelectLine(item, true);

                this.Changed = true;
                // 不要忘记整理注册表事项
                if (AfterChanged(out strError) == -1)
                    goto ERROR1;

                if (IsDp2libraryRunning())
                    StartOrStopOneInstance(new_instance_dlg.InstanceName, "start");
                return;
            }
            finally
            {
                this.Enabled = true;
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void new_instance_dlg_LoadXmlFileInfo(object sender, LoadXmlFileInfoEventArgs e)
        {
            Debug.Assert(String.IsNullOrEmpty(e.DataDir) == false, "");

            string strError = "";
            LineInfo info = new LineInfo();
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            int nRet = info.Build(e.DataDir,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            Debug.Assert(nRet == 1, "");

            e.LineInfo = info;
        }

        void new_instance_dlg_VerifyBindings(object sender, VerifyEventArgs e)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   不重
            //      1    重复
            int nRet = IsBindingDup(e.Value,
                (ListViewItem)null,
                out strError);
            if (nRet != 0)
            {
                e.ErrorInfo = strError;
                return;
            }

            nRet = InstallHelper.IsGlobalBindingDup(e.Value,
                "dp2Library",
                out strError);
            if (nRet != 0)
            {
                e.ErrorInfo = strError;
                return;
            }
        }

        void new_instance_dlg_VerifyDataDir(object sender, VerifyEventArgs e)
        {
            bool bRet = IsDataDirDup(e.Value,
                (ListViewItem)null);
            if (bRet == true)
                e.ErrorInfo = "数据目录 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        void new_instance_dlg_VerifyInstanceName(object sender, VerifyEventArgs e)
        {
            bool bRet = IsInstanceNameDup(e.Value,
                (ListViewItem)null);
            if (bRet == true)
                e.ErrorInfo = "实例名 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        ListViewItem m_currentEditItem = null;

        private void button_modifyInstance_Click(object sender, EventArgs e)
        {
            string strError = "";

            HideMessageTip();

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_instance.SelectedItems[0];
            this.m_currentEditItem = item;

            bool bStopped = false;
            string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
            if (item.ImageIndex == IMAGEINDEX_RUNNING)
            {
                // 只对正在 running 状态的实例做停止处理
                StartOrStopOneInstance(strInstanceName,
                "stop");
                bStopped = true;
            }
            try
            {
                OneInstanceDialog modify_instance_dlg = new OneInstanceDialog();
                GuiUtil.AutoSetDefaultFont(modify_instance_dlg);
                modify_instance_dlg.Text = "修改一个实例";
                modify_instance_dlg.InstanceName = strInstanceName;
                modify_instance_dlg.DataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                modify_instance_dlg.Bindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS).Replace(";", "\r\n");
                modify_instance_dlg.LineInfo = (LineInfo)item.Tag;

                modify_instance_dlg.VerifyInstanceName += new VerifyEventHandler(modify_instance_dlg_VerifyInstanceName);
                modify_instance_dlg.VerifyDataDir += new VerifyEventHandler(modify_instance_dlg_VerifyDataDir);
                modify_instance_dlg.VerifyBindings += new VerifyEventHandler(modify_instance_dlg_VerifyBindings);
                modify_instance_dlg.LoadXmlFileInfo += new LoadXmlFileInfoEventHandler(modify_instance_dlg_LoadXmlFileInfo);

                modify_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
                if (modify_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                    return;

                strInstanceName = modify_instance_dlg.InstanceName;
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, modify_instance_dlg.InstanceName);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, modify_instance_dlg.DataDir);
                ListViewUtil.ChangeItemText(item, COLUMN_BINDINGS, modify_instance_dlg.Bindings.Replace("\r\n", ";"));
                modify_instance_dlg.LineInfo.Changed = true;
                item.Tag = modify_instance_dlg.LineInfo;

                ListViewUtil.SelectLine(item, true);

                this.Changed = true;
                // 不要忘记整理注册表事项
                if (AfterChanged(out strError) == -1)
                    goto ERROR1;
            }
            finally
            {
                if (bStopped)
                    StartOrStopOneInstance(strInstanceName,
    "start");
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        void modify_instance_dlg_LoadXmlFileInfo(object sender, LoadXmlFileInfoEventArgs e)
        {
            Debug.Assert(String.IsNullOrEmpty(e.DataDir) == false, "");

            string strError = "";
            LineInfo info = new LineInfo();
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            int nRet = info.Build(e.DataDir,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            Debug.Assert(nRet == 1, "");

            e.LineInfo = info;
        }

        void modify_instance_dlg_VerifyBindings(object sender, VerifyEventArgs e)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   不重
            //      1    重复
            int nRet = IsBindingDup(e.Value,
            this.m_currentEditItem,
            out strError);
            if (nRet != 0)
            {
                e.ErrorInfo = strError;
                return;
            }

            nRet = InstallHelper.IsGlobalBindingDup(e.Value,
                "dp2Library",
                out strError);
            if (nRet != 0)
            {
                e.ErrorInfo = strError;
                return;
            }
        }

        void modify_instance_dlg_VerifyDataDir(object sender, VerifyEventArgs e)
        {
            bool bRet = IsDataDirDup(e.Value,
                this.m_currentEditItem);
            if (bRet == true)
                e.ErrorInfo = "数据目录 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        void modify_instance_dlg_VerifyInstanceName(object sender, VerifyEventArgs e)
        {
            bool bRet = IsInstanceNameDup(e.Value,
                this.m_currentEditItem);
            if (bRet == true)
                e.ErrorInfo = "实例名 '" + e.Value + "' 和已存在的其他实例发生了重复";
        }

        // return:
        //      false   不重
        //      true    重复
        bool IsInstanceNameDup(string strInstanceName,
            ListViewItem exclude_item)
        {
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;
                string strCurrent = ListViewUtil.GetItemText(item, COLUMN_NAME);
                if (String.Compare(strInstanceName, strCurrent, true) == 0)
                    return true;
            }

            return false;
        }

        // return:
        //      false   不重
        //      true    重复
        bool IsDataDirDup(string strDataDir,
            ListViewItem exclude_item)
        {
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;
                string strCurrent = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                if (String.IsNullOrEmpty(strCurrent) == true)
                    continue;

                if (PathUtil.IsEqual(strDataDir, strCurrent) == true)
                    return true;
            }

            return false;
        }


        // return:
        //      -1  出错
        //      0   不重
        //      1    重复
        int IsBindingDup(string strBindings,
            ListViewItem exclude_item,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strBindings) == true)
                return 0;

            string[] bindings = strBindings.Replace("\r\n", ";").Split(new char[] { ';' });
            if (bindings.Length == 0)
                return 0;


            // 先检查strBinding里面是不是内部有重复
            if (bindings.Length > 1)
            {
                for (int i = 0; i < bindings.Length; i++)
                {
                    string strStart = bindings[i];
                    // 抽掉自己
                    List<string> temps = StringUtil.FromStringArray(bindings);
                    temps.RemoveAt(i);
                    // 检查数组中的哪个url和strOneBinding端口、地址冲突
                    // return:
                    //      -2  不冲突
                    //      -1  出错
                    //      >=0 发生冲突的url在数组中的下标
                    nRet = InstallHelper.IsBindingDup(strStart,
                        StringUtil.FromListString(temps),
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet >= 0)
                    {
                        strError = "当前绑定集合 '" + strBindings + "' 中内部事项之间发生了冲突: " + strError;
                        return 1;
                    }
                }
            }

            // 对照其他事项的bindings检查是不是重复了
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;
                string strCurrentBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);
                if (String.IsNullOrEmpty(strCurrentBindings) == true)
                    continue;

                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                string[] current_bindings = strCurrentBindings.Split(new char[] { ';' });

                for (int i = 0; i < bindings.Length; i++)
                {
                    string strStart = bindings[i];

                    // 检查数组中的哪个url和strOneBinding端口、地址冲突
                    // return:
                    //      -2  不冲突
                    //      -1  出错
                    //      >=0 发生冲突的url在数组中的下标
                    nRet = InstallHelper.IsBindingDup(strStart,
                        current_bindings,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet >= 0)
                    {
                        strError = "当前绑定集合和已存在的实例 '" + strInstanceName + "' 的绑定集合之间发生了冲突: " + strError;
                        return 1;
                    }
                }
            }

            return 0;
        }

        // 删除一个实例
        // TODO: 将来可增加一个菜单命令，仅仅删除一个实例在注册表中的信息，而保留实例的数据目录，也不删除其使用的 dp2Kernel 数据库。以便日后可以迅速恢复创建这个实例
        private void button_deleteInstance_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            HideMessageTip();

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的实例";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
    "确实要删除所选择的 " + this.listView_instance.SelectedItems.Count.ToString() + " 个实例?\r\n\r\n(*** 警告: 实例删除后相关配置和数据库信息会全部丢失，并且无法恢复 ***)",
    "dp2Library 实例管理",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // 删除操作中，被停止过的实例的实例名
            List<string> stopped_instance_names = new List<string>();

            this.Enabled = false;
            try
            {
                bool bRunning = IsDp2libraryRunning();

                // List<string> datadirs = new List<string>();
                foreach (ListViewItem item in this.listView_instance.SelectedItems)
                {
                    string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                    string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                    if (String.IsNullOrEmpty(strDataDir) == true)
                        continue;

                    if (Directory.Exists(strDataDir) == false)
                        continue;

                    // 停止即将被删除的实例
                    if (bRunning)
                    {
                        StartOrStopOneInstance(strInstanceName, "stop");
                        stopped_instance_names.Add(strInstanceName);
                    }

                    // 要求操作者用 supervisor 账号登录一次。以便后续进行各种重要操作。
                    // 只需要 library.xml 即可，不需要 dp2library 在运行中。
                    // return:
                    //      -2  实例没有找到
                    //      -1  出错
                    //      0   放弃验证
                    //      1   成功
                    nRet = LibraryInstallHelper.LibrarySupervisorLogin(this,
                        strInstanceName,
                        "删除实例 '" + strInstanceName + "' 前，需要验证您的 dp2library 管理员身份",
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError + "\r\n\r\n实例 '" + strInstanceName + "' 无法删除");
                        continue;
                    }
                    if (nRet == 0)
                    {
                        MessageBox.Show(this, "实例 '" + strInstanceName + "' 放弃删除");
                        item.Selected = false;
                        continue;
                    }
                    if (nRet == -2)
                        continue;

                    string strFilename = Path.Combine(strDataDir, "library.xml");
                    if (File.Exists(strFilename) == true)
                    {
                        // 删除应用服务器在dp2Kernel内核中创建的数据库
                        // return:
                        //      -1  出错
                        //      0   用户放弃删除
                        //      1   已经删除
                        nRet = LibraryInstallHelper.DeleteKernelDatabases(
                            this,
                            strInstanceName,
                            strFilename,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);
                    }

                    // return:
                    //      -1  出错。包括出错后重试然后放弃
                    //      0   成功
                    nRet = InstallHelper.DeleteDataDir(strDataDir,
        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    // datadirs.Add(strDataDir);

                    stopped_instance_names.Remove(strInstanceName);
                }

                ListViewUtil.DeleteSelectedItems(this.listView_instance);

                this.Changed = true;
                // 不要忘记整理注册表事项
                if (AfterChanged(out strError) == -1)
                    goto ERROR1;

                // 重新启动那些被放弃删除的实例
                foreach (string strInstanceName in stopped_instance_names)
                {
                    StartOrStopOneInstance(strInstanceName, "start");
                }
            }
            finally
            {
                this.Enabled = true;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 根据已有的配置，填充InstanceList
        int FillInstanceList(out string strError)
        {
            strError = "";

            this.listView_instance.Items.Clear();

            int nErrorCount = 0;
            for (int i = 0; ; i++)
            {
                string strInstanceName = "";
                string strDataDir = "";
                string strCertificatSN = "";
                string[] existing_urls = null;
                string strSerialNumber = "";
                bool bRet = InstallHelper.GetInstanceInfo("dp2Library",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertificatSN,
                    out strSerialNumber);
                if (bRet == false)
                    break;

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, strInstanceName);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, strDataDir);
                ListViewUtil.ChangeItemText(item, COLUMN_BINDINGS, string.Join(";", existing_urls));
                this.listView_instance.Items.Add(item);
                LineInfo info = new LineInfo();
                item.Tag = info;

                info.CertificateSN = strCertificatSN;
                info.SerialNumber = strSerialNumber;

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                int nRet = info.Build(strDataDir,
                    out strError);
                if (nRet == -1)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);
                    item.BackColor = Color.Red;
                    item.ForeColor = Color.White;

                    nErrorCount++;
                }
            }

            if (nErrorCount > 0)
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 200;
            else
                this.listView_instance.Columns[COLUMN_ERRORINFO].Width = 0;

            return 0;
        }

        static bool HasDataDirDup(string strDataDir,
            List<string> dirs)
        {
            foreach (string strDir in dirs)
            {
                if (PathUtil.IsEqual(strDir, strDataDir) == true)
                    return true;
            }

            return false;
        }

        // 进行检查
        // return:
        //      -1  发现错误
        //      0   放弃整个保存操作
        //      1   一切顺利
        int DoVerify(out string strError)
        {
            strError = "";

            List<string> instance_names = new List<string>();
            List<string> data_dirs = new List<string>();

            // 检查实例名、数据目录是否重复
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);

                if (HasDataDirDup(strDataDir, data_dirs) == true)
                {
                    strError = "行 " + (i + 1).ToString() + " 的数据目录 '" + strDataDir + "' 和前面某行的数据目录发生了重复";
                    return -1;
                }

                if (instance_names.IndexOf(strInstanceName) != -1)
                {
                    strError = "行 " + (i + 1).ToString() + " 的实例名 '" + strInstanceName + "' 和前面某行的实例名发生了重复";
                    return -1;
                }

                data_dirs.Add(strDataDir);
                instance_names.Add(strInstanceName);

                if (String.IsNullOrEmpty(strDataDir) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的数据目录尚未设置";
                    return -1;
                }

                if (String.IsNullOrEmpty(strBindings) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的协议绑定尚未设置";
                    return -1;
                }
            }

            // TODO: 检查绑定之间的端口是否冲突
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);

                // return:
                //      -1  出错
                //      0   不重
                //      1    重复
                int nRet = IsBindingDup(strBindings,
            item,
            out strError);
                if (nRet != 0)
                {
                    strError = "实例名为 '" + strInstanceName + "' (第 " + (i + 1).ToString() + " 行)的协议绑定发生错误或者冲突: " + strError;
                    return -1;
                }

                nRet = InstallHelper.IsGlobalBindingDup(strBindings,
                    "dp2Library",
                    out strError);
                if (nRet != 0)
                {
                    strError = "实例名为 '" + strInstanceName + "' (第 " + (i + 1).ToString() + " 行)的协议绑定发生错误或者冲突: " + strError;
                    return -1;
                }
            }

            // 警告XML文件格式不正确、XML文件未找到的错误
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);

                if (info.XmlFileNotFound == true)
                {
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中没有找到 library.xml 文件。\r\n\r\n要对这个数据目录进行全新安装么?\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "dp2Library 实例管理",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        info.Changed = false;
                    }
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        info.XmlFileNotFound = false;
                        info.Changed = true;
                    }
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃全部保存操作";
                        return 0;
                    }
                }

                if (info.XmlFileContentError == true)
                {
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中已经存在的 library.xml 文件(XML)格式不正确。程序无法对它进行读取操作\r\n\r\n要对这个数据目录进行全新安装么? 这将刷新整个目录(包括database.xml文件)到最初状态\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "dp2Library 实例管理",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        info.Changed = false;
                    }
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        info.Changed = true;
                        info.XmlFileNotFound = false;
                        info.XmlFileContentError = false;
                        // TODO: 是否要进行备份?
                        File.Delete(PathUtil.MergePath(strDataDir, "library.xml"));
                    }
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        strError = "放弃全部保存操作";
                        return 0;
                    }
                }

            }

            return 1;
        }

        // 2015/4/30
        // 本次新创建的实例名
        public List<string> NewInstanceNames = new List<string>();

        // 兑现修改。
        // 创建数据目录。创建或者修改library.xml文件
        int DoModify(out string strError)
        {
            strError = "";
            int nRet = 0;

            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                string strBindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS);

                if (String.IsNullOrEmpty(strDataDir) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的数据目录尚未设置";
                    return -1;
                }

                if (String.IsNullOrEmpty(strBindings) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的协议绑定尚未设置";
                    return -1;
                }

                if (info.Changed == false && info.Upgrade == false
                    && info.UpdateCfgsDir == false)
                    goto CONTINUE;

                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                nRet = LibraryInstallHelper.DetectDataDir(strDataDir,
            out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 2)
                {
                    // 进行升级检查

                    // 检查xml文件的版本。看看是否有必要提示升级
                    // return:
                    //      -1  error
                    //      0   没有version，即为V1格式
                    //      1   < 2.0
                    //      2   == 2.0
                    //      3   > 2.0
                    nRet = DetectXmlFileVersion(strDataDir,
            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet < 2)
                    {
                        // 提示升级安装
                        // 从以前的 rmsws 数据目录升级
                        string strText = "数据目录 '" + strDataDir + "' 中已经存在以前的 V1 图书馆应用服务器版本遗留下来的数据文件。\r\n\r\n确实要利用这个数据目录来进行升级安装么?\r\n(注意：如果利用以前dp2libraryws的数据目录来进行升级安装，则必须先行卸载dp2libraryws，以避免它和(正在安装的)dp2Library同时运行引起冲突)\r\n\r\n(是)继续进行升级安装 (否)暂停安装，以重新指定数据目录";
                        DialogResult result = MessageBox.Show(
                            this,
            strText,
        "dp2Library 实例管理",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);
                        /*
                        if (result == DialogResult.Cancel)
                        {
                            strError = "用户放弃指定数据目录。安装未完成。";
                            return -1;
                        }
                         * */
                        if (result == DialogResult.No)
                        {
                            strError = "请利用“修改”按钮重新指定实例 '" + item.Text + "' 的数据目录";
                            return -1;
                        }

                        // 刷新cfgs目录

                        info.Upgrade = true;
                    }

                    // 覆盖数据目录中的templates子目录
                    // parameters:
                    //      strRootDir  根目录
                    //      strDataDir    数据目录
                    nRet = OverwriteTemplates(
                        strDataDir,
                        out strError);
                    if (nRet == -1)
                    {
                        // 报错，但是不停止安装
                        MessageBox.Show(this,
                            strError);
                    }

                }
                else
                {
                    // 需要进行最新安装
                    nRet = CreateNewDataDir(strDataDir,
    out strError);
                    if (nRet == -1)
                        return -1;

                    this.NewInstanceNames.Add(strInstanceName);
                }


                // 兑现修改
                if (info.Changed == true
                    || info.Upgrade == true)
                {
                    // 保存信息到library.xml文件中
                    // return:
                    //      -1  error
                    //      0   succeed
                    nRet = info.SaveToXml(strDataDir,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (info.Upgrade == true)
                    {
                        nRet = UpdateXmlFileVersion(strDataDir,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "刷新database.xml文件<version>元素的时候出错: " + strError;
                            return -1;
                        }


                        // 覆盖数据目录中的cfgs子目录

                        // 1) 先备份原来的cfgs子目录
                        string strSourceDir = PathUtil.MergePath(strDataDir, "cfgs");
                        string strTargetDir = PathUtil.MergePath(strDataDir, "v1_cfgs_backup");
                        if (Directory.Exists(strTargetDir) == false)
                        {
                            MessageBox.Show(this,
            "安装程序将升级位于数据目录 '" + strSourceDir + "' 中的配置文件。原有文件将自动备份在目录 '" + strTargetDir + "' 中。");
                            nRet = PathUtil.CopyDirectory(strSourceDir,
            strTargetDir,
            true,
            out strError);
                            if (nRet == -1)
                            {
                                strError = "备份目录 '" + strSourceDir + "' 到 '" + strTargetDir + "' 时发生错误：" + strError;
                                MessageBox.Show(this,
                                    strError);
                            }
                        }

                        if (string.IsNullOrEmpty(this.SourceDir) == false)
                        {
                            // 兼容以前的 sourceDir 用法
                            Debug.Assert(String.IsNullOrEmpty(this.SourceDir) == false, "");
                            string strTempDataDir = PathUtil.MergePath(this.SourceDir, "temp");

                            strSourceDir = PathUtil.MergePath(strTempDataDir, "cfgs");
                            strTargetDir = PathUtil.MergePath(strDataDir, "cfgs");
                        REDO:
                            try
                            {
                                nRet = PathUtil.CopyDirectory(strSourceDir,
                    strTargetDir,
                    true,
                    out strError);
                            }
                            catch (Exception ex)
                            {
                                strError = "拷贝目录 '" + strSourceDir + "' 到配置文件目录 '" + strTargetDir + "' 发生错误：" + ex.Message;
                                DialogResult temp_result = MessageBox.Show(this, // ForegroundWindow.Instance,
    strError + "\r\n\r\n是否重试?",
    "dp2Library 实例管理",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                                if (temp_result == DialogResult.Retry)
                                    goto REDO;
                                throw new InstallException(strError);
                            }

                            if (nRet == -1)
                            {
                                strError = "拷贝目录 '" + strSourceDir + "' 到配置文件目录 '" + strTargetDir + "' 发生错误：" + strError;
                                throw new InstallException(strError);
                            }
                        }
                        else
                        {
                            // 新的方法，直接从 .zip 文件展开
                            Debug.Assert(this.CopyFiles != null, "");

                            strTargetDir = PathUtil.MergePath(strDataDir, "cfgs");
                            CopyFilesEventArgs e = new CopyFilesEventArgs();
                            e.Action = "cfgs";
                            e.DataDir = strDataDir;
                            this.CopyFiles(this, e);
                            if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                            {
                                strError = "拷贝目录 cfgs 到配置文件目录 '" + strTargetDir + "' 时发生错误：" + strError;
                                throw new InstallException(strError);
                            }
                        }

                        info.UpdateCfgsDir = false; // 避免重复做
                    }

                    /*
                    // 在注册表中写入instance信息
                    InstallHelper.SetInstanceInfo(
                        "dp2Library",
                        i,
                        strInstanceName,
                        strDataDir,
                        strBindings.Split(new char []{';'}),
                        info.CertificateSN);
                     * */

                    info.Changed = false;
                    info.Upgrade = false;
                }

                // 注：因为有了 dp2Installer 的自动升级功能，所以这个功能似乎没有存在的必要了
                // 2011/7/3
                if (info.UpdateCfgsDir == true)
                {
                    // 覆盖数据目录中的cfgs子目录

                    if (string.IsNullOrEmpty(this.SourceDir) == false)
                    {
                        // 兼容以前的 this.SourceDir 用法

                        Debug.Assert(String.IsNullOrEmpty(this.SourceDir) == false, "");
                        string strTempDataDir = PathUtil.MergePath(this.SourceDir, "temp");

                        string strSourceDir = PathUtil.MergePath(strTempDataDir, "cfgs");
                        string strTargetDir = PathUtil.MergePath(strDataDir, "cfgs");
                    REDO:
                        try
                        {
                            nRet = PathUtil.CopyDirectory(strSourceDir,
        strTargetDir,
        true,
        out strError);
                        }
                        catch (Exception ex)
                        {
                            strError = "拷贝目录 '" + strSourceDir + "' 到配置文件目录 '" + strTargetDir + "' 发生错误：" + ex.Message;
                            DialogResult temp_result = MessageBox.Show(this, //ForegroundWindow.Instance,
    strError + "\r\n\r\n是否重试?",
    "dp2Library 实例管理",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (temp_result == DialogResult.Retry)
                                goto REDO;
                            throw new InstallException(strError);
                        }

                        if (nRet == -1)
                        {
                            strError = "拷贝目录 '" + strSourceDir + "' 到配置文件目录 '" + strTargetDir + "' 发生错误：" + strError;
                            throw new InstallException(strError);
                        }
                    }
                    else
                    {
                        // 新的用法，直接从 .zip 文件中展开
                        Debug.Assert(this.CopyFiles != null, "");

                        string strTargetDir = PathUtil.MergePath(strDataDir, "cfgs");
                        CopyFilesEventArgs e = new CopyFilesEventArgs();
                        e.Action = "cfgs";
                        e.DataDir = strDataDir;
                        this.CopyFiles(this, e);
                        if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                        {
                            strError = "拷贝目录 cfgs 到配置文件目录 '" + strTargetDir + "' 时发生错误：" + strError;
                            throw new InstallException(strError);
                        }
                    }
                }

            CONTINUE:
                // 在注册表中写入instance信息
                // 因为可能插入或者删除任意实例，那么注册表事项需要全部重写
                InstallHelper.SetInstanceInfo(
                    "dp2Library",
                    i,
                    strInstanceName,
                    strDataDir,
                    strBindings.Split(new char[] { ';' }),
                    info.CertificateSN,
                    info.SerialNumber);
            }

            // 删除注册表中多余的instance信息
            for (int i = this.listView_instance.Items.Count; ; i++)
            {
                // 删除Instance信息
                // return:
                //      false   instance没有找到
                //      true    找到，并已经删除
                bool bRet = InstallHelper.DeleteInstanceInfo(
                    "dp2Library",
                    i);
                if (bRet == false)
                    break;
            }

            return 0;
        }

        // 覆盖数据目录中的templates子目录
        // parameters:
        //      strRootDir  根目录
        //      strDataDir    数据目录
        public int OverwriteTemplates(string strDataDir,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            if (string.IsNullOrEmpty(this.SourceDir) == false)
            {
                // 兼容以前的 this.SourceDir 用法

                Debug.Assert(String.IsNullOrEmpty(this.SourceDir) == false, "");

                string strTemplatesSourceDir = PathUtil.MergePath(this.SourceDir, "temp\\templates");
                string strTemplatesTargetDir = PathUtil.MergePath(strDataDir, "templates");

                PathUtil.TryCreateDir(strTemplatesTargetDir);

                nRet = PathUtil.CopyDirectory(strTemplatesSourceDir,
                    strTemplatesTargetDir,
                    false,  // 拷贝前不删除原来的目录
                    out strError);
                if (nRet == -1)
                {
                    strError = "拷贝临时模板目录 '" + strTemplatesSourceDir + "' 到数据目录之模板目录 '" + strTemplatesTargetDir + "' 时发生错误：" + strError;
                    // throw new InstallException(strError);
                    return -1;
                }
            }
            else
            {
                // 新的用法
                Debug.Assert(this.CopyFiles != null, "");

                string strTargetDir = PathUtil.MergePath(strDataDir, "templates");
                CopyFilesEventArgs e = new CopyFilesEventArgs();
                e.Action = "templates";
                e.DataDir = strDataDir;
                this.CopyFiles(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "拷贝目录 templates 到配置文件目录 '" + strTargetDir + "' 时发生错误：" + strError;
                    return -1;
                }
            }

            return 0;
        }

        // 创建数据目录，并复制进基本内容
        int CreateNewDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            PathUtil.TryCreateDir(strDataDir);

            if (string.IsNullOrEmpty(this.SourceDir) == false)
            {
                // 兼容以前的 this.SourceDir 用法

                // 要求在temp内准备要安装的数据文件(初次安装而不是升级安装)
                Debug.Assert(String.IsNullOrEmpty(this.SourceDir) == false, "");
                string strTempDataDir = PathUtil.MergePath(this.SourceDir, "temp");

                int nRet = PathUtil.CopyDirectory(strTempDataDir,
        strDataDir,
        true,
        out strError);
                if (nRet == -1)
                {
                    strError = "拷贝临时目录 '" + strTempDataDir + "' 到数据目录 '" + strDataDir + "' 时发生错误：" + strError;
                    return -1;
                }
            }
            else
            {
                // 新的用法
                Debug.Assert(this.CopyFiles != null, "");

                CopyFilesEventArgs e = new CopyFilesEventArgs();
                e.Action = "cfgs,templates,other";
                e.DataDir = strDataDir;
                this.CopyFiles(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "拷贝到配置文件目录 '" + strDataDir + "' 时发生错误：" + strError;
                    return -1;
                }
            }

            return 0;
        }

        // 检查xml文件的版本。看看是否有必要提示升级
        // return:
        //      -1  error
        //      0   没有version，即为V1格式
        //      1   < 2.0
        //      2   == 2.0
        //      3   > 2.0
        static int DetectXmlFileVersion(string strDataDir,
            out string strError)
        {
            strError = "";

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException ex)
            {
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            XmlNode nodeVersion = dom.DocumentElement.SelectSingleNode("version");
            if (nodeVersion == null)
            {
                strError = "文件 " + strFilename + " 为V1格式";
                return 0;
            }

            string strVersion = nodeVersion.InnerText;
            if (String.IsNullOrEmpty(strVersion) == true)
            {
                return 0;
            }

            double version = 0;
            try
            {
                version = Convert.ToDouble(strVersion);
            }
            catch (Exception)
            {
                strError = "文件 " + strFilename + " 中<version>元素内容 '" + strVersion + "' 不合法";
                return -1;
            }

            if (version < 2.0)
            {
                return 1;
            }

            if (version == 2.0)
                return 2;
            Debug.Assert(version > 2.0, "");

            return 3;
        }

        static int UpdateXmlFileVersion(string strDataDir,
    out string strError)
        {
            strError = "";

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException ex)
            {
                strError = ex.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strVersion = DomUtil.GetElementText(dom.DocumentElement,
                "version");
            bool bUpdate = false;
            if (string.IsNullOrEmpty(strVersion) == false)
            {
                double version = 0;
                try
                {
                    version = Convert.ToDouble(strVersion);
                }
                catch (Exception)
                {
                    strError = "文件 " + strFilename + " 中<version>元素内容 '" + strVersion + "' 不合法";
                    return -1;
                }
                if (version < 2.0)
                {
                    bUpdate = true;
                }
            }
            else
                bUpdate = true;

            if (bUpdate == true)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "version",
                    "2.0");
            }

            dom.Save(strFilename);

            return 3;
        }

#if NO
        // 修改root用户记录文件
        // parameters:
        //      strUserName 如果为null，表示不修改用户名
        //      strPassword 如果为null，表示不修改密码
        //      strRights   如果为null，表示不修改权限
        static int ModifyRootUser(string strDataDir,
            string strUserName,
            string strPassword,
            string strRights,
            out string strError)
        {
            strError = "";

            if (strUserName == null
                && strPassword == null
                && strRights == null)
                return 0;

            string strFileName = PathUtil.MergePath(strDataDir, "userdb\\0000000001.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载root用户记录文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                return -1;
            }

            string strOldUserName = "";
            if (strUserName != null)
            {
                strOldUserName = DomUtil.GetElementText(dom.DocumentElement,
                    "name");
                DomUtil.SetElementText(dom.DocumentElement, 
                    "name", 
                    strUserName);
            }

            if (strPassword != null)
            {
                DomUtil.SetElementText(dom.DocumentElement, "password",
                    Cryptography.GetSHA1(strPassword));
            }

            if (strRights != null)
            {
                XmlNode nodeServer = dom.DocumentElement.SelectSingleNode("server");
                if (nodeServer == null)
                {
                    Debug.Assert(false, "不可能的情况");
                    strError = "root用户记录文件 " + strFileName + " 格式错误: 根元素下没有<server>元素";
                    return -1;
                }

                DomUtil.SetAttr(nodeServer, "rights", strRights);
            }

            dom.Save(strFileName);

            // 2011/3/29
            // 修改keys_name.xml文件
            if (strUserName != null
                && strUserName != strOldUserName)
            {
                strFileName = PathUtil.MergePath(strDataDir, "userdb\\keys_name.xml");

                dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    strError = "装载用户keys文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                    return -1;
                }

                XmlNode node = dom.DocumentElement.SelectSingleNode("key/keystring[text()='" + strOldUserName + "']");
                if (node == null)
                {
                    strError = "更新用户keys文件时出错：" + "根下 key/keystring 文本值为 '"+strOldUserName+"' 的元素没有找到";
                    return -1;
                }
                node.InnerText = strUserName;
                dom.Save(strFileName);
            }

            return 0;
        }

#endif

#if NO
        // 获得root用户信息
        // return:
        //      -1  error
        //      0   succeed
        static int GetRootUserInfo(string strDataDir,
            out string strUserName,
            out string strRights,
            out string strError)
        {
            strError = "";
            strUserName = "";
            strRights = "";

            string strFileName = PathUtil.MergePath(strDataDir, "userdb\\0000000001.xml");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载root用户记录文件 " + strFileName + " 到DOM时发生错误: " + ex.Message;
                return -1;
            }

            strUserName = DomUtil.GetElementText(dom.DocumentElement, "name");
            XmlNode nodeServer = dom.DocumentElement.SelectSingleNode("server");
            if (nodeServer == null)
            {
                Debug.Assert(false, "不可能的情况");
                strError = "root用户记录文件 " + strFileName + " 格式错误: 根元素下没有<server>元素";
                return -1;
            }

            strRights = DomUtil.GetAttr(nodeServer, "rights");
            return 0;
        }

#endif

        // return:
        //      -1  出错
        //      0   放弃卸载
        //      1   卸载成功
        int DeleteAllInstanceAndDataDir(out string strError)
        {
            strError = "";

            int nRet = 0;

            string strTotalError = "";

            // 预先验证一次全部实例的密码。这样一旦中间有一个放弃，就可放弃全部卸载
            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                // 要求操作者用 supervisor 账号登录一次。以便后续进行各种重要操作。
                // 只需要 library.xml 即可，不需要 dp2library 在运行中。
                // return:
                //      -2  实例没有找到
                //      -1  出错
                //      0   放弃验证
                //      1   成功
                nRet = LibraryInstallHelper.LibrarySupervisorLogin(this,
                    strInstanceName,
                    "删除实例 '" + strInstanceName + "' 前，需要验证您的 dp2library 管理员身份",
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + "\r\n\r\n因实例 '" + strInstanceName + "' 无法删除。放弃整个卸载";
                    return -1;
                }
                if (nRet == 0)
                {
                    strError = "放弃卸载";
                    return 0;
                }
                if (nRet == -2)
                    continue;
            }

            bool bRunning = IsDp2libraryRunning();

            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                if (bRunning)
                    StartOrStopOneInstance(strInstanceName, "start");

                string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
                // XmlDocument dom = new XmlDocument();
                try
                {
                    // dom.Load(strFilename);


                    // 删除应用服务器在dp2Kernel内核中创建的数据库
                    // return:
                    //      -1  出错
                    //      0   用户放弃删除
                    //      1   已经删除
                    nRet = LibraryInstallHelper.DeleteKernelDatabases(
                        this,
                        strInstanceName,
                        strFilename,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                    MessageBox.Show(this, strError);
                }

                if (string.IsNullOrEmpty(strDataDir) == false)
                {
                REDO_DELETE_DATADIR:
                    // 删除数据目录
                    try
                    {
                        Directory.Delete(strDataDir, true);
                    }
                    catch (Exception ex)
                    {
                        DialogResult temp_result = MessageBox.Show(this, // ForegroundWindow.Instance,
    "删除实例 '" + strInstanceName + "' 的数据目录 '" + strDataDir + "' 出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2Library",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_DELETE_DATADIR;

                        strTotalError += "删除实例 '" + strInstanceName + "' 的数据目录 '" + strDataDir + "' 时出错：" + ex.Message + "\r\n";
                    }
                }
            }

            // 注意: 注册表中实例信息的编号是连续的。如果仅仅删除中间某些实例信息，需要把后面编号修改小才行。或者彻底顺序重新写入一次。因此下面采取全部删除注册表信息的做法
            // 删除注册表中所有instance信息
            for (int i = 0; ; i++)
            {
                // 删除Instance信息
                // return:
                //      false   instance没有找到
                //      true    找到，并已经删除
                bool bRet = InstallHelper.DeleteInstanceInfo(
                    "dp2Library",
                    i);
                if (bRet == false)
                    break;
            }

            if (string.IsNullOrEmpty(strTotalError) == false)
                return -1;

            return 1;
        }

        private void listView_instance_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_instance.SelectedItems.Count == 0)
            {
                this.button_modifyInstance.Enabled = false;
                this.button_deleteInstance.Enabled = false;
            }
            else
            {
                this.button_modifyInstance.Enabled = true;
                this.button_deleteInstance.Enabled = true;
            }
        }

        private void listView_instance_DoubleClick(object sender, EventArgs e)
        {
            button_modifyInstance_Click(this, null);
        }

        private void InstanceDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            HideMessageTip();
        }

        private void InstanceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果删除了实例以后，点“取消”退出，则会忘记删除注册表事项
            // TODO: 改进删除实例的功能，让删除的当时就自动整理注册表事项
            if (this.Changed == true)
            {
#if NO
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有修改尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "InstanceDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
#endif
                if (this.Changed == true)
                {
                    string strError = "";
                    if (AfterChanged(out strError) == -1)
                        MessageBox.Show(this, strError);
                    else
                        this.Changed = false;
                }
            }
        }

        private void listView_instance_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            {
                menuItem = new MenuItem("启动实例 [" + this.listView_instance.SelectedItems.Count + "] (&S)");
                menuItem.Click += new System.EventHandler(this.menu_startInstance_Click);
                if (this.listView_instance.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }


            {
                menuItem = new MenuItem("停止实例 [" + this.listView_instance.SelectedItems.Count + "] (&T)");
                menuItem.Click += new System.EventHandler(this.menu_stopInstance_Click);
                if (this.listView_instance.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            {
                menuItem = new MenuItem("刷新状态(&R)");
                menuItem.Click += new System.EventHandler(this.menu_refreshInstanceState_Click);
                if (this.listView_instance.Items.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.Show(this.listView_instance, new Point(e.X, e.Y));
        }

        // 启动所选的实例
        void menu_startInstance_Click(object sender, EventArgs e)
        {
            StartOrStopInstance("start");
        }

        // 停止所选的实例
        void menu_stopInstance_Click(object sender, EventArgs e)
        {
            StartOrStopInstance("stop");
        }

        // 刷新全部事项的状态显示
        void menu_refreshInstanceState_Click(object sender, EventArgs e)
        {
            RefreshInstanceState();
        }

        void EnableControls(bool bEnable)
        {
            if (this.Enabled == false)
                return;

            this.listView_instance.Enabled = bEnable;
            // this.button_Cancel.Enabled = bEnable;
            this.button_OK.Enabled = bEnable;
            this.button_newInstance.Enabled = bEnable;
            this.button_modifyInstance.Enabled = bEnable;
            this.button_deleteInstance.Enabled = bEnable;
        }

        int AfterChanged(out string strError)
        {
            strError = "";

            // 进行检查
            // return:
            //      -1  发现错误
            //      0   放弃整个保存操作
            //      1   一切顺利
            int nRet = DoVerify(out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            nRet = DoModify(out strError);
            if (nRet == -1)
                return -1;

            this.Changed = false;
            return 0;
        }


        #region 实例运行状态

        void StartOrStopInstance(string strAction)
        {
            List<string> errors = new List<string>();
            this.EnableControls(false);
            try
            {
                string strError = "";

                foreach (ListViewItem item in this.listView_instance.SelectedItems)
                {
                    string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                    int nRet = dp2library_serviceControl(
        strAction,
        strInstanceName,
        out strError);
                    if (nRet == -1)
                        errors.Add(strError);
                    else
                        item.ImageIndex = strAction == "stop" ? IMAGEINDEX_STOPPED : IMAGEINDEX_RUNNING;
                }

            }
            finally
            {
                this.EnableControls(true);
            }

            if (errors.Count > 0)
                MessageBox.Show(this, StringUtil.MakePathList(errors, "; "));
        }

        void StartOrStopOneInstance(string strInstanceName,
            string strAction)
        {
            ListViewItem item = ListViewUtil.FindItem(this.listView_instance, strInstanceName, COLUMN_NAME);
            if (item == null)
            {
                MessageBox.Show(this, "名为 '" + strInstanceName + "' 实例在列表中没有找到");
                return;
            }
            List<string> errors = new List<string>();
            this.EnableControls(false);
            try
            {
                string strError = "";

                {
                    int nRet = dp2library_serviceControl(
        strAction,
        strInstanceName,
        out strError);
                    if (nRet == -1)
                        errors.Add(strError);
                    else
                        item.ImageIndex = strAction == "stop" ? IMAGEINDEX_STOPPED : IMAGEINDEX_RUNNING;
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            if (errors.Count > 0)
                MessageBox.Show(this, StringUtil.MakePathList(errors, "; "));
        }

        const int IMAGEINDEX_RUNNING = 0;
        const int IMAGEINDEX_STOPPED = 1;

        // 刷新实例状态显示
        void RefreshInstanceState()
        {
            bool bError = false;
            string strError = "";
            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (bError)
                {
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                    continue;
                }
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                int nRet = dp2library_serviceControl(
                    "getState",
                    strInstanceName,
                    out strError);
                if (nRet == -1)
                {
                    // 只要出错一次，后面就不再调用 dp2library_serviceControl()
                    bError = true;
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                }
                else if (nRet == 0 || strError == "stopped")
                {
                    item.ImageIndex = IMAGEINDEX_STOPPED;
                }
                else
                {
                    // nRet == 1
                    item.ImageIndex = IMAGEINDEX_RUNNING;
                }
            }
        }

        class IpcInfo
        {
            public IpcClientChannel Channel { get; set; }
            public IServiceControl Server { get; set; }
        }

        static IpcInfo BeginIpc()
        {
            IpcInfo info = new IpcInfo();

            string strUrl = "ipc://dp2library_ServiceControlChannel/dp2library_ServiceControlServer";
            info.Channel = new IpcClientChannel();

            ChannelServices.RegisterChannel(info.Channel, false);

            info.Server = (IServiceControl)Activator.GetObject(typeof(IServiceControl),
                strUrl);
            if (info.Server == null)
            {
                string strError = "无法连接到 remoting 服务器 " + strUrl;
                throw new Exception(strError);
            }

            return info;
        }

        static void EndIpc(IpcInfo info)
        {
            ChannelServices.UnregisterChannel(info.Channel);
        }

        // 检测 dp2library.exe 是否在运行状态
        static bool IsDp2libraryRunning()
        {
            try
            {
                IpcInfo ipc = BeginIpc();
                try
                {
                    ServiceControlResult result = null;
                    InstanceInfo info = null;
                    // 获得一个实例的信息
                    result = ipc.Server.GetInstanceInfo(".",
        out info);
                    if (result.Value == -1)
                        return false;
                    if (info != null)
                        return info.State == "running";
                    else
                        return true;
                }
                finally
                {
                    EndIpc(ipc);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // parameters:
        //      strCommand  start/stop/getState
        // return:
        //      -1  出错
        //      0/1 strCommand 为 "getState" 时分别表示实例 不在运行/在运行 状态
        static int dp2library_serviceControl(
    string strCommand,
    string strInstanceName,
    out string strError)
        {
            strError = "";

            try
            {
                IpcInfo ipc = BeginIpc();
                try
                {
                    ServiceControlResult result = null;
                    if (strCommand == "start")
                        result = ipc.Server.StartInstance(strInstanceName);
                    else if (strCommand == "stop")
                        result = ipc.Server.StopInstance(strInstanceName);
                    else if (strCommand == "getState")
                    {
                        InstanceInfo info = null;
                        // 获得一个实例的信息
                        result = ipc.Server.GetInstanceInfo(strInstanceName,
            out info);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }
                        else
                            strError = info.State;
                        return result.Value;
                    }
                    else
                    {
                        strError = "未知的命令 '" + strCommand + "'";
                        return -1;
                    }
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        return -1;
                    }
                    strError = result.ErrorInfo;
                    return 0;

                }
                finally
                {
                    EndIpc(ipc);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

#if NO
        // parameters:
        //      strCommand  start/stop/getState
        // return:
        //      -1  出错
        //      0/1 strCommand 为 "getState" 时分别表示实例 不在运行/在运行 状态
        static int dp2library_serviceControl(
    string strCommand,
    string strInstanceName,
    out string strError)
        {
            strError = "";

            string strUrl = "ipc://dp2library_ServiceControlChannel/dp2library_ServiceControlServer";
            IpcClientChannel channel = new IpcClientChannel();

            ChannelServices.RegisterChannel(channel, false);

            try
            {
                IServiceControl server = (IServiceControl)Activator.GetObject(typeof(IServiceControl),
                    strUrl);
                if (server == null)
                {
                    strError = "无法连接到 remoting 服务器 " + strUrl;
                    return -1;
                }

                try
                {
                    ServiceControlResult result = null;
                    if (strCommand == "start")
                        result = server.StartInstance(strInstanceName);
                    else if (strCommand == "stop")
                        result = server.StopInstance(strInstanceName);
                    else if (strCommand == "getState")
                    {
                        InstanceInfo info = null;
                        // 获得一个实例的信息
                        result = server.GetInstanceInfo(strInstanceName,
            out info);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }
                        else
                            strError = info.State;
                        return result.Value;
                    }
                    else
                    {
                        strError = "未知的命令 '" + strCommand + "'";
                        return -1;
                    }
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        return -1;
                    }
                    strError = result.ErrorInfo;
                    return 0;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }
            }
            finally
            {
                ChannelServices.UnregisterChannel(channel);
            }
        }

#endif

        #endregion

    }

    // ListView中每一行的隐藏信息
    public class LineInfo
    {
        public bool UpdateCfgsDir = false;  // 是否要刷新数据目录的cfgs子目录内容

        public string CertificateSN = "";

        public string SerialNumber = "";

        // *** dp2Kernel服务器信息
        // dp2Kernel URL
        public string KernelUrl = "";
        // username
        public string KernelUserName = "";
        // password
        public string KernelPassword = "";

        // *** supervisor 账户信息
        public string SupervisorUserName = null;
        public string SupervisorPassword = null;  // null表示不修改以前的密码
        public string SupervisorRights = null;

        //
        public string LibraryName = "";

        // 内容是否发生过修改
        public bool Changed = false;

        // XML文件没有找到
        public bool XmlFileNotFound = false;
        // XML文件内容格式错误
        public bool XmlFileContentError = false;
        // 是否从V1的数据目录升级上来
        public bool Upgrade = false;

        public void Clear()
        {
            this.KernelUrl = "";
            this.KernelUserName = "";
            this.KernelPassword = "";

            this.SupervisorUserName = "";
            this.SupervisorPassword = "";
            this.SupervisorRights = "";

            this.LibraryName = "";

            this.XmlFileNotFound = false;
            this.XmlFileContentError = false;
            this.Changed = false;
            this.Upgrade = false;
        }

        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        public int Build(string strDataDir,
            out string strError)
        {
            strError = "";

            this.Clear();

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                this.XmlFileNotFound = true;
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                this.XmlFileContentError = true;
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeRmsServer = dom.DocumentElement.SelectSingleNode("rmsserver");
            if (nodeRmsServer == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<rmsserver>元素不存在。";
                return -1;
            }

            // DomUtil.SetAttr(nodeDatasource, "mode", null);

            this.KernelUrl = DomUtil.GetAttr(nodeRmsServer, "url");
            this.KernelUserName = DomUtil.GetAttr(nodeRmsServer, "username");

            this.KernelPassword = DomUtil.GetAttr(nodeRmsServer, "password");
            try
            {
                this.KernelPassword = Cryptography.Decrypt(this.KernelPassword, "dp2circulationpassword");
            }
            catch
            {
                strError = "<rmsserver password='???' /> 中的密码不正确";
                return -1;
            }

            // supervisor
            XmlElement nodeSupervisor = dom.DocumentElement.SelectSingleNode("accounts/account[@type='']") as XmlElement;
            if (nodeSupervisor != null)
            {
                this.SupervisorUserName = DomUtil.GetAttr(nodeSupervisor, "name");
#if NO
                // library.xml 2.00 以前的做法
                this.SupervisorPassword = DomUtil.GetAttr(nodeSupervisor, "password");
                try
                {
                    this.SupervisorPassword = Cryptography.Decrypt(this.SupervisorPassword, "dp2circulationpassword");
                }
                catch
                {
                    strError = "<account password='???' /> 中的密码不正确";
                    return -1;
                }
#endif
                // 新的做法
                this.SupervisorPassword = null; // 表示得不到以前的密码，同时也不打算修改

                this.SupervisorRights = DomUtil.GetAttr(nodeSupervisor, "rights");
            }

            this.LibraryName = DomUtil.GetElementText(dom.DocumentElement,
                "libraryInfo/libraryName");

            return 1;
        }


        // return:
        //      -1  error
        //      0   succeed
        public int SaveToXml(string strDataDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFilename = PathUtil.MergePath(strDataDir, "library.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strFilename + " 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "加载文件 " + strFilename + " 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            XmlNode nodeRmsServer = dom.DocumentElement.SelectSingleNode("rmsserver");
            if (nodeRmsServer == null)
            {
                // strError = "文件 " + strFilename + " 内容不合法，根下的<rmsserver>元素不存在。";
                // return -1;
                nodeRmsServer = dom.CreateElement("rmsserver");
                dom.DocumentElement.AppendChild(nodeRmsServer);
            }

            DomUtil.SetAttr(nodeRmsServer,
                "url",
                this.KernelUrl);
            DomUtil.SetAttr(nodeRmsServer,
                 "username",
                 this.KernelUserName);

            string strPassword = Cryptography.Encrypt(this.KernelPassword, "dp2circulationpassword");
            DomUtil.SetAttr(nodeRmsServer,
                "password",
                strPassword);

            // 
            XmlNode nodeAccounts = dom.DocumentElement.SelectSingleNode("accounts");
            if (nodeAccounts == null)
            {
                nodeAccounts = dom.CreateElement("accounts");
                dom.DocumentElement.AppendChild(nodeAccounts);
            }
            XmlElement nodeSupervisor = nodeAccounts.SelectSingleNode("account[@type='']") as XmlElement;
            if (nodeSupervisor == null)
            {
                nodeSupervisor = dom.CreateElement("account");
                nodeAccounts.AppendChild(nodeSupervisor);
            }

            if (this.SupervisorUserName != null)
                nodeSupervisor.SetAttribute("name", this.SupervisorUserName);
            if (this.SupervisorPassword != null)
            {
                double version = LibraryServerUtil.GetLibraryXmlVersion(dom);

                if (version <= 2.0)
                {
                    nodeSupervisor.SetAttribute("password",
                        Cryptography.Encrypt(this.SupervisorPassword, "dp2circulationpassword")
                        );
                }
                else
                {
                    // 新的密码存储策略
                    string strHashed = "";
                    nRet = LibraryServerUtil.SetUserPassword(this.SupervisorPassword, out strHashed, out strError);
                    if (nRet == -1)
                    {
                        strError = "SetUserPassword() error: " + strError;
                        return -1;
                    }
                    nodeSupervisor.SetAttribute("password", strHashed);
                }
            }
            if (this.SupervisorRights != null)
                DomUtil.SetAttr(nodeSupervisor, "rights", this.SupervisorRights);

            if (this.LibraryName != null)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                        "libraryInfo/libraryName",
                        this.LibraryName);
            }

            dom.Save(strFilename);

            return 0;
        }
    }
}
