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
using System.Collections;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Data.SqlClient;

using Ionic.Zip;
//using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;

using DigitalPlatform.Install;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Interfaces;
using MySqlConnector;

namespace DigitalPlatform.rms
{
    public partial class InstanceDialog : Form
    {
        public bool UninstallMode = false;

        public string SourceDir = "";   // 安装的程序文件目录，兼容以前用法。如果 SourceDir 有内容，就优先用它，不用 DataZipFileName 了

        public string DataZipFileName = ""; // 数据目录初始内容压缩文件

        public bool Changed = false;

        const int COLUMN_NAME = 0;
        const int COLUMN_ERRORINFO = 1;
        const int COLUMN_DATADIR = 2;
        const int COLUMN_BINDINGS = 3;

        private MessageBalloon m_firstUseBalloon = null;

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
            // 卸载状态
            if (this.UninstallMode == true)
            {
                this.button_OK.Text = "卸载";
                this.button_newInstance.Visible = false;
                this.button_deleteInstance.Visible = false;
                this.button_modifyInstance.Visible = false;
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

            listView_instance_SelectedIndexChanged(null, null);

            this.BeginInvoke(new Action(RefreshInstanceState));
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.button_newInstance;
            m_firstUseBalloon.Title = "安装 dp2kernel 数据库内核";
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

            try
            {
                HideMessageTip();
            }
            catch
            {
            }

            // 全部卸载
            if (this.UninstallMode == true)
            {
                DialogResult result = MessageBox.Show(this,
"确实要卸载 dp2Kernel? \r\n\r\n(*** 警告：卸载后数据和数据库信息将全部丢失，并无法恢复 ***)",
"卸载 dp2Kernel",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;   // cancelled

                nRet = this.DeleteAllInstanceAndDataDir(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

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


            if (this.Changed == true)
            {
                if (AfterChanged(out strError) == -1)
                    goto ERROR1;

                this.Changed = false;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            try
            {
                HideMessageTip();
            }
            catch
            {
            }

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

            try
            {
                HideMessageTip();
            }
            catch
            {
            }


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
            new_instance_dlg.VerifyDatabases += new_instance_dlg_VerifyDatabases;

            new_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
            if (new_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

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

            if (IsDp2kernelRunning())
                StartOrStopOneInstance(new_instance_dlg.InstanceName, "start");

            // 记载到 DebugInfo 中
            if (string.IsNullOrEmpty(this.DebugInfo) == false)
                this.DebugInfo += "\r\n\r\n";
            this.DebugInfo += new_instance_dlg.DebugInfo;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void new_instance_dlg_VerifyDatabases(object sender, VerifyEventArgs e)
        {
            string strError = "";
            bool bRet = IsDatabasesDup(
                e.Value,
                e.Value1,
                (ListViewItem)null,
                out strError);
            if (bRet == true)
                e.ErrorInfo = "检查 databases.xml 文件过程中发现错误: " + strError;
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

            if (nRet == 1)
            {
                string strRootUserName = "";
                string strRootUserRights = "";
                // 获得root用户信息
                // return:
                //      -1  error
                //      0   succeed
                nRet = GetRootUserInfo(e.DataDir,
        out strRootUserName,
        out strRootUserRights,
        out strError);
                if (nRet == -1)
                {
                    e.ErrorInfo = strError;
                    return;
                }
                else
                {
                    info.RootUserName = strRootUserName;
                    info.RootUserRights = strRootUserRights;
                }
            }

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
    "dp2Kernel",
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

            try
            {
                HideMessageTip();
            }
            catch
            {
            }


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
                modify_instance_dlg.InstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);
                modify_instance_dlg.DataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                modify_instance_dlg.Bindings = ListViewUtil.GetItemText(item, COLUMN_BINDINGS).Replace(";", "\r\n");
                modify_instance_dlg.LineInfo = (LineInfo)item.Tag;

                modify_instance_dlg.VerifyInstanceName += new VerifyEventHandler(modify_instance_dlg_VerifyInstanceName);
                modify_instance_dlg.VerifyDataDir += new VerifyEventHandler(modify_instance_dlg_VerifyDataDir);
                modify_instance_dlg.VerifyBindings += new VerifyEventHandler(modify_instance_dlg_VerifyBindings);
                modify_instance_dlg.LoadXmlFileInfo += new LoadXmlFileInfoEventHandler(modify_instance_dlg_LoadXmlFileInfo);
                modify_instance_dlg.VerifyDatabases += modify_instance_dlg_VerifyDatabases;

                modify_instance_dlg.StartPosition = FormStartPosition.CenterScreen;
                if (modify_instance_dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                    return;

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

                // 记载到 DebugInfo 中
                if (string.IsNullOrEmpty(this.DebugInfo) == false)
                    this.DebugInfo += "\r\n\r\n";
                this.DebugInfo += modify_instance_dlg.DebugInfo;
            }
            finally
            {
                if (bStopped)
                    StartOrStopOneInstance(strInstanceName, "start");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        void modify_instance_dlg_VerifyDatabases(object sender, VerifyEventArgs e)
        {
            string strError = "";
            bool bRet = IsDatabasesDup(e.Value,
                e.Value1,
                this.m_currentEditItem,
                out strError);
            if (bRet == true)
                e.ErrorInfo = "检查 databases.xml 文件过程中发现错误: " + strError;
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

            if (nRet == 1)
            {
                string strRootUserName = "";
                string strRootUserRights = "";
                // 获得root用户信息
                // return:
                //      -1  error
                //      0   succeed
                nRet = GetRootUserInfo(e.DataDir,
        out strRootUserName,
        out strRootUserRights,
        out strError);
                if (nRet == -1)
                {
                    e.ErrorInfo = strError;
                    return;
                }
                else
                {
                    info.RootUserName = strRootUserName;
                    info.RootUserRights = strRootUserRights;
                }
            }

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
"dp2Kernel",
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
        //      false   不重
        //      true    重复
        bool IsDatabasesDup(
            string strDataDir,
            string strInstanceName,
            ListViewItem exclude_item,
            out string strError)
        {
            strError = "";

            Hashtable name_table = new Hashtable();     // sqldbname --> InstanceValue
            Hashtable prefix_table = new Hashtable();   // prefix --> InstanceValue

            List<string> instance_name_list = new List<string>();
            List<string> datadirs = new List<string>();

            // string strInstanceName = "新实例";

            foreach (ListViewItem item in this.listView_instance.Items)
            {
                if (item == exclude_item)
                    continue;

                string strCurrentInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                string strCurrentDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                if (String.IsNullOrEmpty(strCurrentDataDir) == true)
                    continue;

                if (PathUtil.IsEqual(strDataDir, strCurrentDataDir) == true)
                {
                    if (string.IsNullOrEmpty(strInstanceName))
                        strInstanceName = strCurrentInstanceName;
                    continue;
                }

                instance_name_list.Add(strCurrentInstanceName);
                datadirs.Add(strCurrentDataDir);
            }

            instance_name_list.Add(strInstanceName);
            datadirs.Add(strDataDir);

            for (int i = 0; i < datadirs.Count; i++)
            {
                string strCurrentInstanceName = instance_name_list[i];
                string strCurrentDataDir = datadirs[i];

                // 检查不同实例的 dp2kernel 中所用的 SQL 数据库名是否发生了重复和冲突
                // return:
                //      -1  检查过程出错
                //      0   没有冲突
                //      1   发生了冲突。报错信息在 strError 中
                int nRet = InstallHelper.CheckDatabasesXml(
                    strCurrentInstanceName,
                    strCurrentDataDir,
                    prefix_table,
                    name_table,
                    out strError);
                if (nRet == -1)
                    return true;    // -1
                if (nRet == 1)
                    return true;
            }

            return false;
        }

#if NO

        // 对两个URL字符串进行忽略最后一个'/'字符的比较
        static bool IsUrlEqual(string url1, string url2)
        {
            if (url1.Length > 0 && url1[url1.Length - 1] != '/')
                url1 += "/";
            if (url2.Length > 0 && url2[url2.Length - 1] != '/')
                url2 += "/";

            if (url1 == url2)
                return true;

            // 更严格地比较

            try
            {
                Uri uri1 = new Uri(url1);
                Uri uri2 = new Uri(url2);

                if (uri1.Equals(uri2) == true)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }



        // 检查数组中的哪个url和strOneBinding端口、地址冲突
        // return:
        //      -2  不冲突
        //      -1  出错
        //      >=0 发生冲突的url在数组中的下标
        static int IsBindingDup(string strOneBinding,
            string[] bindings,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strOneBinding) == true)
            {
                strError = "strOneBinding参数值不能为空";
                return -1;
            }

            Uri one_uri = new Uri(strOneBinding);

            for (int i = 0; i < bindings.Length; i++)
            {
                string strCurrentBinding = bindings[i];
                if (String.IsNullOrEmpty(strCurrentBinding) == true)
                    continue;

                Uri current_uri = new Uri(strCurrentBinding);

                if (one_uri.Scheme.ToLower() == "net.tcp")
                {
                    if (current_uri.Scheme.ToLower() == "net.tcp")
                    {
                        // 端口不能冲突
                        if (one_uri.Port == current_uri.Port)
                        {
                            strError = "'"+strOneBinding+"' 和 '"+strCurrentBinding+"' 之间端口号冲突了";
                            return i;
                        }
                    }
                    else if (current_uri.Scheme.ToLower() == "net.pipe")
                    {
                        // 不存在冲突的可能
                    }
                    else if (current_uri.Scheme.ToLower() == "http")
                    {
                        // 端口号不能冲突
                        if (one_uri.Port == current_uri.Port)
                        {
                            strError = "'" + strOneBinding + "' 和 '" + strCurrentBinding + "' 之间端口号冲突了";
                            return i;
                        }
                    }
                }
                else if (one_uri.Scheme.ToLower() == "net.pipe")
                {
                    if (current_uri.Scheme.ToLower() == "net.pipe")
                    {
                        // 不能全部相同
                        if (one_uri.Equals(current_uri) == true)
                        {
                            strError = "net.pipe类型的URL '" + strOneBinding + "' 有两项完全相同";
                            return i;
                        }

                        if (IsUrlEqual(one_uri.ToString(), current_uri.ToString()) == true)
                        {
                            strError = "net.pipe类型的URL '" + strOneBinding + "' 有两项实质上相同(末尾仅仅差异一个'/'字符)";
                            return i;
                        }
                    }
                }
                else if (one_uri.Scheme.ToLower() == "http")
                {
                    if (current_uri.Scheme.ToLower() == "net.tcp")
                    {
                        // 端口不能冲突
                        if (one_uri.Port == current_uri.Port)
                        {
                            strError = "'" + strOneBinding + "' 和 '" + strCurrentBinding + "' 之间端口号冲突了";
                            return i;
                        }
                    }
                    else if (current_uri.Scheme.ToLower() == "net.pipe")
                    {
                        // 不可能冲突
                    }
                    else if (current_uri.Scheme.ToLower() == "http")
                    {
                        // 端口号可以相同，但是不能全部相同
                        if (one_uri.Equals(current_uri) == true)
                        {
                            strError = "http类型的URL '"+strOneBinding+"' 有两项完全相同";
                            return i;
                        }

                        if (IsUrlEqual(one_uri.ToString(), current_uri.ToString()) == true)
                        {
                            strError = "http类型的URL '" + strOneBinding + "' 有两项实质上相同(末尾仅仅差异一个'/'字符)";
                            return i;
                        }

                    }
                }
            }

            return -2;
        }

#endif

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

        // 删除本实例创建过的全部 SQL 数据库
        // parameters:
        // return:
        //      -1  出错
        //      0   databases.xml 文件不存在; 或 databases.xml 中没有任何 SQL 数据库信息
        //      1   成功删除
        public int DeleteAllSqlDatabase(string strDataDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFileName = Path.Combine(strDataDir, "databases.xml");
            if (File.Exists(strFileName) == false)
                return 0;

            LineInfo info = new LineInfo();
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            nRet = info.Build(strDataDir, out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList databases = dom.DocumentElement.SelectNodes("//database/property/sqlserverdb");
            if (databases.Count == 0)
                return 0;

            if (info.SqlServerType == "SQLite")
                return 0;


            string strConnectionString = "";
            nRet = GetConnectionString(info,
                out strConnectionString,
                out strError);
            if (nRet == -1)
                return -1;

            if (info.SqlServerType == "MS SQL Server")
            {
                StringBuilder command = new StringBuilder();
                command.Append("use master " + "\n");
                foreach (XmlElement database in databases)
                {
                    string strSqlDbName = database.GetAttribute("name");
                    if (string.IsNullOrEmpty(strSqlDbName) == true)
                        continue;

                    command.Append(" if exists (select * from dbo.sysdatabases where name = N'" + strSqlDbName + "')" + "\n"
                        + " drop database " + strSqlDbName + "\n");
                }
                command.Append(" use master " + "\n");

                try
                {
                    using (SqlConnection connection = new SqlConnection(strConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand sql_command = new SqlCommand(command.ToString(),
                            connection))
                        {
                            nRet = sql_command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = "执行 SQL 命令 '" + command.ToString() + "' 时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                    return -1;
                }
            }

            if (info.SqlServerType == "PostgreSQL")
            {
                nRet = PgsqlDataSourceDlg.DeleteDatabase(
info.SqlServerName,
info.DatabaseInstanceName,
AskAdminUserName,
out strError);
                if (nRet == -1)
                {
                    strError = $"删除 Pgsql 实例数据库 '{info.DatabaseInstanceName}' 时出错: {strError}";
                    return -1;
                }

                nRet = PgsqlDataSourceDlg.DeleteUser(
                    info.SqlServerName,
                    info.DatabaseLoginName,
                    AskAdminUserName,
                    out strError);
                if (nRet == -1)
                {
                    strError = $"删除 Pgsql 用户 '{info.DatabaseLoginName}' 时出错: {strError}";
                    return -1;
                }
            }

            if (info.SqlServerType == "MySQL Server")
            {
                StringBuilder command = new StringBuilder();
                foreach (XmlElement database in databases)
                {
                    string strSqlDbName = database.GetAttribute("name");
                    if (string.IsNullOrEmpty(strSqlDbName) == true)
                        continue;

                    command.Append(" DROP DATABASE IF EXISTS `" + strSqlDbName + "`; \n");
                }

                if (command.Length == 0)
                    return 0;

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(strConnectionString))
                    {
                        connection.Open();
                        using (MySqlCommand sql_command = new MySqlCommand(command.ToString(),
                            connection))
                        {
                            nRet = sql_command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = "执行 SQL 命令 '" + command.ToString() + "' 时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                    return -1;
                }
            }

            if (info.SqlServerType == "Oracle")
            {
                List<string> sql_dbnames = new List<string>();
                foreach (XmlElement database in databases)
                {
                    string strSqlDbName = database.GetAttribute("name");
                    if (string.IsNullOrEmpty(strSqlDbName) == true)
                        continue;

                    sql_dbnames.Add(strSqlDbName);
                }

                if (sql_dbnames.Count > 0)
                {
                    // 删除所有的 table
                    nRet = DeleteOracleTables(strConnectionString,
        sql_dbnames,
        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 2022/7/1
                // 用 system 用户身份删除 dp2kernel_oracle 账户(和表空间)
                nRet = OracleDataSourceWizard.DeleteUser(
                    info.SqlServerName,
                    info.DatabaseLoginName,
                    AskAdminUserName,
                    out strError);
                if (nRet == -1)
                    return -1;
#if REMOVED
                string strCommand = " SELECT table_name FROM user_tables WHERE ";

                int i = 0;
                foreach (string strSqlDbName in sql_dbnames)
                {
                    string pattern = (strSqlDbName + "_%").Replace("_", "\\_");
                    if (i > 0)
                        strCommand += " or ";
                    strCommand += " table_name like '" + pattern.ToUpper() + "_%' ESCAPE '\\'";
                    i++;
                }

                try
                {
                    using (OracleConnection connection = new OracleConnection(strConnectionString))
                    {
                        connection.Open();

                        List<string> table_names = new List<string>();
                        using (OracleCommand command = new OracleCommand(strCommand, connection))
                        {
                            using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                            {
                                while (dr.Read())
                                {
                                    if (dr.IsDBNull(0) == false)
                                        table_names.Add(dr.GetString(0));
                                }
                            }

                            // 第二步，删除这些表
                            List<string> cmd_lines = new List<string>();
                            foreach (string strTableName in table_names)
                            {
                                cmd_lines.Add("DROP TABLE " + strTableName + " \n");
                            }

                            foreach (string strLine in cmd_lines)
                            {
                                command.CommandText = strLine;
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    strError = "删除所有表时出错：\r\n"
                                        + ex.Message + "\r\n"
                                        + "SQL命令:\r\n"
                                        + strLine;
                                    return -1;
                                }
                            }
                        } // end of using command
                    }
                }
                catch (Exception ex)
                {
                    strError = "执行 SQL 命令时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                    return -1;
                }
#endif
            }

            return 1;
        }

        static int DeleteOracleTables(string strConnectionString,
            List<string> sql_dbnames,
            out string strError)
        {
            strError = "";

            string strCommand = " SELECT table_name FROM user_tables WHERE ";

            int i = 0;
            foreach (string strSqlDbName in sql_dbnames)
            {
                string pattern = (strSqlDbName + "_%").Replace("_", "\\_");
                if (i > 0)
                    strCommand += " or ";
                strCommand += " table_name like '" + pattern.ToUpper() + "_%' ESCAPE '\\'";
                i++;
            }

            try
            {
                using (OracleConnection connection = new OracleConnection(strConnectionString))
                {
                    connection.Open();

                    List<string> table_names = new List<string>();
                    using (OracleCommand command = new OracleCommand(strCommand, connection))
                    {
                        using (OracleDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (dr.Read())
                            {
                                if (dr.IsDBNull(0) == false)
                                    table_names.Add(dr.GetString(0));
                            }
                        }

                        // 第二步，删除这些表
                        List<string> cmd_lines = new List<string>();
                        foreach (string strTableName in table_names)
                        {
                            cmd_lines.Add("DROP TABLE " + strTableName + " \n");
                        }

                        foreach (string strLine in cmd_lines)
                        {
                            command.CommandText = strLine;
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                strError = "删除所有表时出错：\r\n"
                                    + ex.Message + "\r\n"
                                    + "SQL命令:\r\n"
                                    + strLine;
                                return -1;
                            }
                        }

                        return 0;
                    } // end of using command
                }
            }
            catch (Exception ex)
            {
                strError = "执行 SQL 命令时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }
        }

        // TODO: 建议分数据库类型存储。比如存储在一个 hashtable 中
        string adminUserName = "";
        string adminPassword = "";

        void ClearCachedAdminUserName()
        {
            adminUserName = "";
            adminPassword = "";
        }

        // 询问超级用户名和密码
        string AskAdminUserName(
            string title,
            string defaultUserName,
            out string userName,
            out string password)
        {
            if (string.IsNullOrEmpty(adminUserName))
            {
                userName = "";
                password = "";

                using (LoginDlg dlg = new LoginDlg())
                {
                    dlg.Comment = title;
                    dlg.ServerUrl = " ";
                    dlg.UserName = defaultUserName; //  "postgres";
                    dlg.Password = "";
                    dlg.SavePassword = true;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                    {
                        return "放弃操作";
                    }

                    userName = dlg.UserName;
                    password = dlg.Password;

                    if (dlg.SavePassword == true)
                    {
                        adminUserName = dlg.UserName;
                        adminPassword = dlg.Password;
                    }
                    else
                        ClearCachedAdminUserName();
                }
            }
            else
            {
                userName = adminUserName;
                password = adminPassword;
            }

            return null;
        }


        /*
<datasource servername="XIETAO-THINKPAD" mode="SSPI" servertype="MS SQL Server" userid='' password=''/>
         * 
         * */
        // 根据 datasource 信息构造 SQL Connection 语句，支持四种数据库方式
        static int GetConnectionString(LineInfo info,
            out string strConnectionString,
            out string strError)
        {
            strConnectionString = "";
            strError = "";
            int nTimeout = 3600;

            if (info.SqlServerType == "MS SQL Server")
            {
                if (string.IsNullOrEmpty(info.DatabaseLoginName) == true) // "SSPI"
                {
                    strConnectionString = @"Persist Security Info=False;"
                        + "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + info.SqlServerName + ";"
                        + "Connect Timeout=" + nTimeout.ToString() + ";"; // 30秒
                    return 0;
                }

                strConnectionString = @"Persist Security Info=False;"
        + "User ID=" + info.DatabaseLoginName + ";"    //帐户和密码
        + "Password=" + info.DatabaseLoginPassword + ";"
        //+ "Integrated Security=SSPI; "      //信任连接
        + "Data Source=" + info.SqlServerName + ";"
        // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
        + "Connect Timeout=" + nTimeout.ToString() + ";";
                return 0;
            }

            if (info.SqlServerType == "MySQL Server")
            {
                strConnectionString = @"Persist Security Info=False;"
    + "User ID=" + info.DatabaseLoginName + ";"    //帐户和密码
    + "Password=" + info.DatabaseLoginPassword + ";"
    + "Data Source=" + info.SqlServerName + ";"
    // http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
    + "Connect Timeout=" + nTimeout.ToString() + ";"
                + (string.IsNullOrEmpty(info.SslMode) ? "" : "SslMode=" + info.SslMode + ";")  // 2018/9/22
                + "charset=utf8;";
                return 0;
            }

            if (info.SqlServerType == "Oracle")
            {
                if (string.IsNullOrEmpty(info.DatabaseLoginName) == true) // "SSPI"
                {
#if NO
                    strConnectionString = @"Persist Security Info=False;"
                        + "Integrated Security=SSPI; "      //信任连接
                        + "Data Source=" + info.SqlServerName + ";"
                        + "Connect Timeout=" + nTimeout.ToString() + ";"; // 30秒
                    return 0;
#endif
                    strError = "Oracle 数据库情形下， user id 不能为空";
                    return -1;
                }

                strConnectionString = @"Persist Security Info=False;"
+ "User ID=" + info.DatabaseLoginName + ";"    //帐户和密码
+ "Password=" + info.DatabaseLoginPassword + ";"
//+ "Integrated Security=SSPI; "      //信任连接
+ "Data Source=" + info.SqlServerName + ";"
// http://msdn2.microsoft.com/en-us/library/8xx3tyca(vs.71).aspx
+ "Connect Timeout=" + nTimeout.ToString() + ";";
                return 0;
            }

            if (info.SqlServerType == "SQLite")
            {
                strError = "SQLite 暂时不使用本函数";
                return -1;
            }

            if (info.SqlServerType == "PostgreSQL")
            {
                return 0;
            }

            strError = "未知的 SQL 服务器类型 '" + info.SqlServerType + "'";
            return -1;
        }

        // 删除一个实例
        private void button_deleteInstance_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            try
            {
                HideMessageTip();
            }
            catch
            {
            }

            if (this.listView_instance.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
    "确实要删除所选择的 " + this.listView_instance.SelectedItems.Count.ToString() + " 个实例?\r\n\r\n(*** 警告: 数据库和配置信息将全部丢失，并且无法恢复 ***)",
    "InstanceDialog",
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
                bool bRunning = IsDp2kernelRunning();

                // List<string> datadirs = new List<string>();
                foreach (ListViewItem item in this.listView_instance.SelectedItems)
                {
                    string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                    string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

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

                    // return:
                    //      -1  出错
                    //      0   databases.xml 文件不存在; 或 databases.xml 中没有任何 SQL 数据库信息
                    //      1   成功删除
                    nRet = DeleteAllSqlDatabase(strDataDir, out strError);
                    if (nRet == -1)
                    {
                        result = MessageBox.Show(this,
    "删除实例 '" + strInstanceName + "' 的全部 SQL 数据库时出错: " + strError + "。\r\n\r\n请问是否放弃删除此实例的数据目录?\r\n\r\n(数据目录删除后，请您用适当的 SQL 管理工具自行删除 SQL 数据库)\r\n\r\n(是: 不删除数据目录，实例得到完整保留; 否: 继续删除数据目录)",
    "删除 dp2Kernel 实例",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            item.Selected = false;
                            continue;
                        }
                        // 否则继续删除实例
                    }

                    // 删除数据目录
                    // return:
                    //      -1  出错。包括出错后重试然后放弃
                    //      0   成功
                    nRet = DeleteDataDir(strDataDir,
        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    // datadirs.Add(strDataDir);
                    this.Changed = true;

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

        // return:
        //      -1  出错。包括出错后重试然后放弃
        //      0   成功
        int DeleteDataDir(string strDataDir,
            out string strError)
        {
            strError = "";
        REDO_DELETE_DATADIR:
            try
            {
                MessageBar bar = new MessageBar();
                bar.MessageText = "正在删除目录 '" + strDataDir + "'，请等待 ...";
                bar.StartPosition = FormStartPosition.CenterScreen;
                bar.Show(this);
                bar.Update();
                try
                {
                    Directory.Delete(strDataDir, true);
                }
                finally
                {
                    bar.Close();
                }
                return 0;
            }
            catch (Exception ex)
            {
                strError = "删除数据目录 '" + strDataDir + "' 时出错: " + ex.Message;
            }

            DialogResult temp_result = MessageBox.Show(ForegroundWindow.Instance,
strError + "\r\n\r\n是否重试?",
"删除数据目录 '" + strDataDir + "'",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
            if (temp_result == DialogResult.Retry)
                goto REDO_DELETE_DATADIR;

            return -1;
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
                string strCertSN = "";
                string[] existing_urls = null;

                bool bRet = InstallHelper.GetInstanceInfo("dp2Kernel",
                    i,
                    out strInstanceName,
                    out strDataDir,
                    out existing_urls,
                    out strCertSN);
                if (bRet == false)
                    break;

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, strInstanceName);
                ListViewUtil.ChangeItemText(item, COLUMN_DATADIR, strDataDir);
                ListViewUtil.ChangeItemText(item, COLUMN_BINDINGS, string.Join(";", existing_urls));
                this.listView_instance.Items.Add(item);
                LineInfo info = new LineInfo();
                item.Tag = info;
                info.CertificateSN = strCertSN;
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

                if (nRet == 1)
                {
                    string strRootUserName = "";
                    string strRootUserRights = "";
                    // 获得root用户信息
                    // return:
                    //      -1  error
                    //      0   succeed
                    nRet = GetRootUserInfo(strDataDir,
            out strRootUserName,
            out strRootUserRights,
            out strError);
                    if (nRet == -1)
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, strError);
                        item.BackColor = Color.Red;
                        item.ForeColor = Color.White;
                        nErrorCount++;
                    }
                    else
                    {
                        info.RootUserName = strRootUserName;
                        info.RootUserRights = strRootUserRights;
                    }

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
                    "dp2Kernel",
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
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中没有找到 databases.xml 文件。\r\n\r\n要对这个数据目录进行全新安装么?\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "setup_dp2kernel",
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
                    string strText = "实例 '" + item.Text + "' 的数据目录 '" + strDataDir + "' 中已经存在的 databases.xml 文件(XML)格式不正确。程序无法对它进行读取操作\r\n\r\n要对这个数据目录进行全新安装么? 这将刷新整个目录(包括database.xml文件)到最初状态\r\n\r\n(是)进行全新安装 (否)不进行任何修改和安装 (取消)放弃全部保存操作";
                    DialogResult result = MessageBox.Show(
                        this,
        strText,
        "setup_dp2kernel",
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
                        File.Delete(PathUtil.MergePath(strDataDir, "databases.xml"));
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

        // 兑现修改。
        // 创建数据目录。创建或者修改databases.xml文件
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

                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                nRet = DetectDataDir(strDataDir,
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
                        string strText = "数据目录 '" + strDataDir + "' 中已经存在以前的 V1 数据库内核版本遗留下来的数据文件。\r\n\r\n确实要利用这个数据目录来进行升级安装么?\r\n(注意：如果利用以前 rmsws 的数据目录来进行升级安装，则必须先行卸载 rmsws，以避免它和(正在安装的) dp2Kernel 同时运行引起冲突)\r\n\r\n(是)继续进行升级安装 (否)暂停安装，以重新指定数据目录";
                        DialogResult result = MessageBox.Show(
                            this,
            strText,
            "setup_dp2kernel",
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
                        info.Upgrade = true;
                    }

                }
                else
                {
                    // 需要进行最新安装
                    nRet = CreateNewDataDir(strDataDir,
    out strError);
                    if (nRet == -1)
                        return -1;
                }


                // 兑现修改
                if (info.Changed == true
                    || info.Upgrade == true)
                {
                    // 保存信息到databases.xml文件中
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
                    }

                    // 修改root账户信息
                    if (info.RootUserName != null
                        || info.RootPassword != null
                        || info.RootUserRights != null)
                    {
                        // 修改root用户记录文件
                        // parameters:
                        //      strUserName 如果为null，表示不修改用户名
                        //      strPassword 如果为null，表示不修改密码
                        //      strRights   如果为null，表示不修改权限
                        nRet = ModifyRootUser(strDataDir,
                            info.RootUserName,
                            info.RootPassword,
                            info.RootUserRights,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    /*
                    // 在注册表中写入instance信息
                    InstallHelper.SetInstanceInfo(
                        "dp2Kernel",
                        i,
                        strInstanceName,
                        strDataDir,
                        strBindings.Split(new char []{';'}),
                        info.CertificateSN);
                     * */

                    info.Changed = false;
                    info.Upgrade = false;
                }

                // 在注册表中写入instance信息
                // 因为可能插入或者删除任意实例，那么注册表事项需要全部重写
                InstallHelper.SetInstanceInfo(
                    "dp2Kernel",
                    i,
                    strInstanceName,
                    strDataDir,
                    strBindings.Split(new char[] { ';' }),
                    info.CertificateSN);
            }

            // 删除注册表中多余的instance信息
            for (int i = this.listView_instance.Items.Count; ; i++)
            {
                // 删除Instance信息
                // return:
                //      false   instance没有找到
                //      true    找到，并已经删除
                bool bRet = InstallHelper.DeleteInstanceInfo(
                    "dp2Kernel",
                    i);
                if (bRet == false)
                    break;
            }

            return 0;
        }

        // 探测数据目录，是否已经存在数据，是不是属于升级情形
        // return:
        //      -1  error
        //      0   数据目录不存在
        //      1   数据目录存在，但是xml文件不存在
        //      2   xml文件已经存在
        public static int DetectDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            DirectoryInfo di = new DirectoryInfo(strDataDir);
            if (di.Exists == false)
                return 0;

            string strExistingDatabasesFileName = PathUtil.MergePath(strDataDir,
                "databases.xml");
            if (File.Exists(strExistingDatabasesFileName) == true)
                return 2;

            return 1;
        }

        // 创建数据目录，并复制进基本内容
        int CreateNewDataDir(string strDataDir,
            out string strError)
        {
            strError = "";

            try
            {
                PathUtil.TryCreateDir(strDataDir);
            }
            catch (Exception ex)
            {
                // 2018/1/27
                strError = ex.Message;
                return -1;
            }

            if (string.IsNullOrEmpty(this.SourceDir) == false)
            {
                // 旧的用法 this.SourceDir

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

                Debug.Assert(String.IsNullOrEmpty(this.DataZipFileName) == false, "");
                try
                {
                    using (ZipFile zip = ZipFile.Read(this.DataZipFileName))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            ZipEntry e = zip[i];

                            string strPart = GetSubPath(e.FileName);
                            string strFullPath = Path.Combine(strDataDir, strPart);

                            e.FileName = strPart;

                            e.Extract(strDataDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

            return 0;
        }

        // 去掉第一级路经
        static string GetSubPath(string strPath)
        {
            int nRet = strPath.IndexOfAny(new char[] { '/', '\\' }, 0);
            if (nRet == -1)
                return "";
            return strPath.Substring(nRet + 1);
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

            string strFilename = PathUtil.MergePath(strDataDir, "databases.xml");
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

            string strFilename = PathUtil.MergePath(strDataDir, "databases.xml");
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
                    strError = "更新用户keys文件时出错：" + "根下 key/keystring 文本值为 '" + strOldUserName + "' 的元素没有找到";
                    return -1;
                }
                node.InnerText = strUserName;
                dom.Save(strFileName);
            }

            return 0;
        }

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

        int DeleteAllInstanceAndDataDir(out string strError)
        {
            strError = "";
            int nRet = 0;

            for (int i = 0; i < this.listView_instance.Items.Count; i++)
            {
                ListViewItem item = this.listView_instance.Items[i];
                LineInfo info = (LineInfo)item.Tag;
                string strDataDir = ListViewUtil.GetItemText(item, COLUMN_DATADIR);
                string strInstanceName = ListViewUtil.GetItemText(item, COLUMN_NAME);

                // TODO: 删除全部 SQL 数据库
                // return:
                //      -1  出错
                //      0   databases.xml 文件不存在; 或 databases.xml 中没有任何 SQL 数据库信息
                //      1   成功删除
                nRet = DeleteAllSqlDatabase(strDataDir, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this,
"删除实例 '" + strInstanceName + "' 的全部 SQL 数据库时出错: " + strError + "。数据目录删除后，请您用适当的 SQL 管理工具自行删除 SQL 数据库"
                        );
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
                        DialogResult temp_result = MessageBox.Show(ForegroundWindow.Instance,
    "删除实例 '" + strInstanceName + "' 的数据目录'" + strDataDir + "'出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
    "卸载 dp2Kernel",
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_DELETE_DATADIR;

                        strError += "删除实例 '" + strInstanceName + "' 的数据目录 '" + strDataDir + "' 时出错：" + ex.Message + "\r\n";
                    }
                }
            }

            // 删除注册表中所有instance信息
            for (int i = 0; ; i++)
            {
                // 删除Instance信息
                // return:
                //      false   instance没有找到
                //      true    找到，并已经删除
                bool bRet = InstallHelper.DeleteInstanceInfo(
                    "dp2Kernel",
                    i);
                if (bRet == false)
                    break;
            }

            if (string.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
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

        private void InstanceDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果删除了实例以后，点“取消”退出，则会忘记删除注册表事项
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

        private void InstanceDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

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
                    int nRet = dp2kernel_serviceControl(
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
                    int nRet = dp2kernel_serviceControl(
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
                int nRet = dp2kernel_serviceControl(
                    "getState",
                    strInstanceName,
                    out strError);
                if (nRet == -1)
                {
                    // 2019/5/3
                    MessageBox.Show(this, strError);
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

            string strUrl = "ipc://dp2kernel_ServiceControlChannel/dp2kernel_ServiceControlServer";
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

        // 检测 dp2kernel.exe 是否在运行状态
        static bool IsDp2kernelRunning()
        {
            try
            {
                IpcInfo ipc = BeginIpc();
                try
                {
                    ServiceControlResult result = null;
                    // 获得一个实例的信息
                    result = ipc.Server.GetInstanceInfo(".",
        out InstanceInfo info);
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
        static int dp2kernel_serviceControl(
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
                        // 获得一个实例的信息
                        // 当 result.Value 返回值为 -1 或 0 时，info 可能返回 null
                        result = ipc.Server.GetInstanceInfo(strInstanceName,
            out InstanceInfo info);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            return -1;
                        }
                        else
                            strError = info?.State;
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

        #endregion

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

    }

    // ListView中每一行的隐藏信息
    public class LineInfo
    {
        public string CertificateSN = "";
        // *** SQL服务器信息
        // SQL服务器类型
        public string SqlServerType = "";
        // SQL服务器名
        public string SqlServerName = "";
        // SQL数据库前缀
        public string DatabaseInstanceName = "";
        // SQL Login Name
        public string DatabaseLoginName = "";
        // SQL Login Password
        public string DatabaseLoginPassword = "";

        // 2018/9/22
        // 值可以为空
        string _sslMode = "";
        public string SslMode
        {
            get
            {
                return _sslMode;
            }
            set
            {
                // TODO: 检查值的正确性

                if (value != null && value.IndexOf(":") != -1)
                    throw new ArgumentException("SslMode 值内不允许出现冒号");

                _sslMode = value;
            }
        }

        // *** root账户信息
        public string RootUserName = null;
        public string RootPassword = null;  // null表示不修改以前的密码
        public string RootUserRights = null;

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
            this.SqlServerType = "";
            this.SqlServerName = "";
            this.DatabaseInstanceName = "";
            this.DatabaseLoginName = "";
            this.DatabaseLoginPassword = "";
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

            string strFilename = PathUtil.MergePath(strDataDir, "databases.xml");
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

            XmlNode nodeDatasource = dom.DocumentElement.SelectSingleNode("datasource");
            if (nodeDatasource == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<datasource>元素不存在。";
                return -1;
            }

            // DomUtil.SetAttr(nodeDatasource, "mode", null);

            this.SqlServerType = DomUtil.GetAttr(nodeDatasource, "servertype");
            this.SqlServerName = DomUtil.GetAttr(nodeDatasource, "servername");
            this.DatabaseLoginName = DomUtil.GetAttr(nodeDatasource, "userid");

            this.DatabaseLoginPassword = DomUtil.GetAttr(nodeDatasource, "password");
            if (string.IsNullOrEmpty(this.DatabaseLoginPassword) == false)
                this.DatabaseLoginPassword = Cryptography.Decrypt(this.DatabaseLoginPassword, "dp2003");

            // 2015/5/1
            string strMode = DomUtil.GetAttr(nodeDatasource, "mode");
            if (strMode == "SSPI")
            {
                this.DatabaseLoginName = "";
                this.DatabaseLoginPassword = "";
            }

            // 2018/9/23
            if (strMode != null && strMode.StartsWith("SslMode:"))
                this.SslMode = strMode.Substring("SslMode:".Length);
            else if (this.SqlServerType == "MySQL Server")
                this.SslMode = "None";  // 兼容以前无 mode 属性时的情况，此情况下等于 SslMode:None

            var nodeDbs = dom.DocumentElement.SelectSingleNode("dbs") as XmlElement;
            if (nodeDbs == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<dbs>元素不存在。";
                return -1;
            }
            this.DatabaseInstanceName = nodeDbs.GetAttribute("instancename");
            return 1;
        }

        // return:
        //      -1  error
        //      0   succeed
        public int SaveToXml(string strDataDir,
            out string strError)
        {
            strError = "";

            string strFilename = PathUtil.MergePath(strDataDir, "databases.xml");
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

            XmlElement nodeDatasource = dom.DocumentElement.SelectSingleNode("datasource") as XmlElement;
            if (nodeDatasource == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<datasource>元素不存在。";
                return -1;
            }

            if (this.SqlServerType == "MySQL Server")
            {
                // 2018/9/22
                if (string.IsNullOrEmpty(this.SslMode) == false)
                    DomUtil.SetAttr(nodeDatasource, "mode", "SslMode:" + this.SslMode);
                else
                    nodeDatasource.RemoveAttribute("mode");
            }
            else
            {
                if (this.SqlServerType == "MS SQL Server"
                    && string.IsNullOrEmpty(this.DatabaseLoginName) == true)
                    DomUtil.SetAttr(nodeDatasource, "mode", "SSPI");    // 2015/4/23
                else
                    DomUtil.SetAttr(nodeDatasource, "mode", null);
            }

            DomUtil.SetAttr(nodeDatasource,
                "servertype",
                this.SqlServerType);
            DomUtil.SetAttr(nodeDatasource,
                "servername",
                this.SqlServerName);

            if (this.SqlServerType == "MS SQL Server"
                && string.IsNullOrEmpty(this.DatabaseLoginName) == true)
            {
                // 因为使用集成权限认证(SSPI)，所以不需要 login 了

                DomUtil.SetAttr(nodeDatasource,
                    "userid", null);

                DomUtil.SetAttr(nodeDatasource,
                    "password", null);
            }
            else
            {
                DomUtil.SetAttr(nodeDatasource,
                     "userid",
                     this.DatabaseLoginName);

                string strPassword = Cryptography.Encrypt(this.DatabaseLoginPassword, "dp2003");
                DomUtil.SetAttr(nodeDatasource,
                    "password",
                    strPassword);
            }

            XmlNode nodeDbs = dom.DocumentElement.SelectSingleNode("dbs");
            if (nodeDbs == null)
            {
                strError = "文件 " + strFilename + " 内容不合法，根下的<dbs>元素不存在。";
                return -1;
            }
            DomUtil.SetAttr(nodeDbs,
                 "instancename",
                 this.DatabaseInstanceName);

            dom.Save(strFilename);
            return 0;
        }
    }
}
