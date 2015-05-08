using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Install;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform;

#if NO

namespace dp2Kernel
{
    public partial class OneInstanceDialog : Form
    {
        public bool IsNew = false;   // 是否为新创建实体
        public LineInfo LineInfo = null;

        public event VerifyEventHandler VerifyInstanceName = null;
        public event VerifyEventHandler VerifyDataDir = null;
        public event VerifyEventHandler VerifyBindings = null;

        public event LoadXmlFileInfoEventHandler LoadXmlFileInfo = null;    // 临时获取特定的数据目录内的相关信息

        public string LoadedDataDir = "";  // 已经特别装载过的数据目录。防止Leave时重复装载同一个目录
        bool m_bDataDirExist = false;   // 数据目录是否已经存在


        bool m_bDataDirKeyEdited = false;    // 数据目录textbox是否被键盘修改过

        private MessageBalloon m_firstUseBalloon = null;

        const int WM_CHECK_DATADIR = API.WM_USER + 201;

        public OneInstanceDialog()
        {
            InitializeComponent();
        }

        private void OneInstanceDialog_Load(object sender, EventArgs e)
        {
            // Debug.Assert(false, "");

            // 填充sql定义
            RefreshSqlDef();
            RefreshRootUserInfo();

            if (IsNew == true && String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                SetDefaultDataDirValue();   // 让数据目录textbox有缺省值
            }

            if (IsNew == false)
            {
                if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
                    && Directory.Exists(this.textBox_dataDir.Text) == true)
                    this.LoadedDataDir = this.textBox_dataDir.Text;
            }

            if (IsNew && String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
            {
                ShowMessageTip();
            }

            API.PostMessage(this.Handle, WM_CHECK_DATADIR, 0, 0);
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CHECK_DATADIR:
                    HideMessageTip();
                    textBox_dataDir_Leave(this, null);  // 促使检查数据目录是否碰巧为已经存在的目录

                    if (IsNew && String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
                    {
                        ShowMessageTip();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.textBox_instanceName;
            m_firstUseBalloon.Title = "创建第一个实例";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "对于第一个实例(或者唯一的实例)来说，可以让实例名保持为空，这样，缺省的数据目录名和协议路径都可以简短一些";

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

            // 检查

            // 数据目录
            if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.comboBox_sqlServerType.Text) == true)
            {
                strError = "尚未指定 SQL 服务器类型";
                goto ERROR1;
            }

            // sql服务器信息
            if (String.IsNullOrEmpty(this.textBox_sqlDef.Text) == true)
            {
                strError = "尚未指定 SQL 服务器信息";
                goto ERROR1;
            }

            // 协议绑定
            if (String.IsNullOrEmpty(this.textBox_bindings.Text) == true)
            {
                strError = "尚未指定协议绑定信息";
                goto ERROR1;
            }


            if (this.IsNew == true
                && this.m_bDataDirExist == false)
            {
                // root用户信息
                if (this.LineInfo.RootUserName == null
        || this.LineInfo.RootPassword == null
        || this.LineInfo.RootUserRights == null)
                {
                    strError = "尚未设定root账户的用户名、密码、权限";
                    goto ERROR1;
                }
            }

            // 如果修改时，需要创建新的数据目录
            if (this.IsNew == false)
            {
                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                int nRet = InstanceDialog.DetectDataDir(this.textBox_dataDir.Text,
            out strError);
                if (nRet == -1)
                {
                    strError = "探测数据目录 '" + this.textBox_dataDir.Text + "' 是否存在时，出现错误: " + strError;
                    goto ERROR1;
                }

                if (nRet == 0 || nRet == 1)
                {

                    // root用户信息
                    if (this.LineInfo.RootUserName == null
            || this.LineInfo.RootPassword == null
            || this.LineInfo.RootUserRights == null)
                    {
                        strError = "尚未设定root账户的用户名、密码、权限";
                        goto ERROR1;
                    }
                }
            }

            // TODO: 如果在编辑状态，需要排除和listview中自己重的情况

            if (this.VerifyInstanceName != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_instanceName.Text;
                this.VerifyInstanceName(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = e1.ErrorInfo;
                    goto ERROR1;
                }
            }

            if (this.VerifyDataDir != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_dataDir.Text;
                this.VerifyDataDir(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = e1.ErrorInfo;
                    goto ERROR1;
                }
            }

            if (this.VerifyBindings != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_bindings.Text;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = e1.ErrorInfo;
                    goto ERROR1;
                }
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // 要把修改还原
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_editSqlDef_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.comboBox_sqlServerType.Text == "SQLite")
            {
                SqliteDataSourceDlg datasource_dlg = new SqliteDataSourceDlg();
                GuiUtil.AutoSetDefaultFont(datasource_dlg);

                datasource_dlg.Comment = "dp2Kernel 内核的数据库功能可以基于内置的 SQLite 实现，不再需要任何其他数据库底层软件。请设置下列 SQLite 相关参数。";
                if (this.LineInfo != null)
                {
                    datasource_dlg.InstanceName = this.LineInfo.DatabaseInstanceName;
                }
                else
                {
                    datasource_dlg.InstanceName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;    // 应当没有空格和特殊字符
                }

                datasource_dlg.StartPosition = FormStartPosition.CenterScreen;
                datasource_dlg.ShowDialog(this);
                if (datasource_dlg.DialogResult != DialogResult.OK)
                    return;

                if (this.LineInfo == null)
                    this.LineInfo = new LineInfo();

                this.LineInfo.SqlServerType = "SQLite";
                this.LineInfo.SqlServerName = "~sqlite";
                this.LineInfo.DatabaseInstanceName = datasource_dlg.InstanceName;
                this.LineInfo.DatabaseLoginName = "";
                this.LineInfo.DatabaseLoginPassword = "";
                RefreshSqlDef();

            }
            else if (this.comboBox_sqlServerType.Text == "MS SQL Server")
            {
                // MS SQL Server

                // 调对话框得到数据源配置参数
                MsSqlServerDataSourceDlg datasource_dlg = new MsSqlServerDataSourceDlg();
                GuiUtil.AutoSetDefaultFont(datasource_dlg);

                datasource_dlg.Comment = "dp2Kernel 内核的数据库功能可以基于 MS SQL Server 2000 以上版本实现。请设置下列 SQL Server 相关参数。";
                if (this.LineInfo != null)
                {
                    datasource_dlg.SqlServerName = this.LineInfo.SqlServerName;
                    datasource_dlg.InstanceName = this.LineInfo.DatabaseInstanceName;
                    datasource_dlg.KernelLoginName = this.LineInfo.DatabaseLoginName;
                    datasource_dlg.KernelLoginPassword = this.LineInfo.DatabaseLoginPassword;
                }
                else
                {
                    datasource_dlg.SqlServerName = "";
                    datasource_dlg.InstanceName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;    // 应当没有空格和特殊字符
                    datasource_dlg.KernelLoginName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;
                    datasource_dlg.KernelLoginPassword = "";
                }

                datasource_dlg.StartPosition = FormStartPosition.CenterScreen;
                datasource_dlg.ShowDialog(this);
                if (datasource_dlg.DialogResult != DialogResult.OK)
                    return;

                if (this.LineInfo == null)
                    this.LineInfo = new LineInfo();

                this.LineInfo.SqlServerType = "MS SQL Server";
                this.LineInfo.SqlServerName = datasource_dlg.SqlServerName;
                this.LineInfo.DatabaseInstanceName = datasource_dlg.InstanceName;
                this.LineInfo.DatabaseLoginName = datasource_dlg.KernelLoginName;
                this.LineInfo.DatabaseLoginPassword = datasource_dlg.KernelLoginPassword;
                RefreshSqlDef();
            }
            else if (this.comboBox_sqlServerType.Text == "MySQL Server")
            {
                // MySQL Server

                // 调对话框得到数据源配置参数
                MySqlDataSourceDlg datasource_dlg = new MySqlDataSourceDlg();
                GuiUtil.AutoSetDefaultFont(datasource_dlg);

                datasource_dlg.Comment = "dp2Kernel 内核的数据库功能可以基于 MySQL Server 5.5 以上版本实现。请设置下列 MySQL Server 相关参数。";
                if (this.LineInfo != null)
                {
                    datasource_dlg.SqlServerName = this.LineInfo.SqlServerName;
                    datasource_dlg.InstanceName = this.LineInfo.DatabaseInstanceName;
                    datasource_dlg.KernelLoginName = this.LineInfo.DatabaseLoginName;
                    datasource_dlg.KernelLoginPassword = this.LineInfo.DatabaseLoginPassword;
                }
                else
                {
                    datasource_dlg.SqlServerName = "";
                    datasource_dlg.InstanceName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;    // 应当没有空格和特殊字符
                    datasource_dlg.KernelLoginName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;
                    datasource_dlg.KernelLoginPassword = "";
                }

                datasource_dlg.StartPosition = FormStartPosition.CenterScreen;
                datasource_dlg.ShowDialog(this);
                if (datasource_dlg.DialogResult != DialogResult.OK)
                    return;

                if (this.LineInfo == null)
                    this.LineInfo = new LineInfo();

                this.LineInfo.SqlServerType = "MySQL Server";
                this.LineInfo.SqlServerName = datasource_dlg.SqlServerName;
                this.LineInfo.DatabaseInstanceName = datasource_dlg.InstanceName;
                this.LineInfo.DatabaseLoginName = datasource_dlg.KernelLoginName;
                this.LineInfo.DatabaseLoginPassword = datasource_dlg.KernelLoginPassword;
                RefreshSqlDef();
            }
            else if (this.comboBox_sqlServerType.Text == "Oracle")
            {
                // Oracle

                // 调对话框得到数据源配置参数
                OracleDataSourceDlg datasource_dlg = new OracleDataSourceDlg();
                GuiUtil.AutoSetDefaultFont(datasource_dlg);

                datasource_dlg.Comment = "dp2Kernel 内核的数据库功能可以基于 Oracle 11g 以上版本实现。请设置下列 Oracle 相关参数。";
                if (this.LineInfo != null)
                {
                    datasource_dlg.SqlServerName = this.LineInfo.SqlServerName;
                    // datasource_dlg.InstanceName = this.LineInfo.DatabaseInstanceName;
                    datasource_dlg.KernelLoginName = this.LineInfo.DatabaseLoginName;
                    datasource_dlg.KernelLoginPassword = this.LineInfo.DatabaseLoginPassword;
                }
                else
                {
                    datasource_dlg.SqlServerName = "";
                    /*
                    datasource_dlg.InstanceName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;    // 应当没有空格和特殊字符
                     * */
                    datasource_dlg.KernelLoginName = "dp2kernel"
                        + (String.IsNullOrEmpty(this.InstanceName) == false ? "_" : "")
                        + this.InstanceName;
                    datasource_dlg.KernelLoginPassword = "";
                }

                datasource_dlg.StartPosition = FormStartPosition.CenterScreen;
                datasource_dlg.ShowDialog(this);
                if (datasource_dlg.DialogResult != DialogResult.OK)
                    return;

                if (this.LineInfo == null)
                    this.LineInfo = new LineInfo();

                this.LineInfo.SqlServerType = "Oracle";
                this.LineInfo.SqlServerName = datasource_dlg.SqlServerName;
                this.LineInfo.DatabaseInstanceName = datasource_dlg.InstanceName;
                this.LineInfo.DatabaseLoginName = datasource_dlg.KernelLoginName;
                this.LineInfo.DatabaseLoginPassword = datasource_dlg.KernelLoginPassword;
                RefreshSqlDef();
            }
            else
            {
                strError = "未知的 SQL 服务器类型 '" + this.comboBox_sqlServerType.Text + "'";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void button_editRootUserInfo_Click(object sender, EventArgs e)
        {
            RootUserDlg dlg = new RootUserDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            if (this.LineInfo == null
                ||
                (this.LineInfo.RootUserName == null
                && this.LineInfo.RootPassword == null
                && this.LineInfo.RootUserRights == null)
                )
            {
                dlg.UserName = "root";
                dlg.Password = "";
                dlg.Rights = "this:management;children_database:management;children_directory:management;children_leaf:management;descendant_directory:management;descendant_record:management;descendant_leaf:management";
            }
            else
            {
                Debug.Assert(this.LineInfo != null, "");
                dlg.UserName = this.LineInfo.RootUserName;
                dlg.Password = this.LineInfo.RootPassword;
                dlg.Rights = this.LineInfo.RootUserRights;
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (this.LineInfo == null)
            {
                this.LineInfo = new dp2Kernel.LineInfo();
            }

            this.LineInfo.RootUserName = dlg.UserName;
            this.LineInfo.RootPassword = dlg.Password;
            this.LineInfo.RootUserRights = dlg.Rights;

            RefreshRootUserInfo();
        }

        void RefreshSqlDef()
        {
            if (this.LineInfo == null)
            {
                this.textBox_sqlDef.Text = "";
                return;
            }

            if (this.LineInfo.SqlServerType == "MS SQL Server"
|| string.IsNullOrEmpty(this.LineInfo.SqlServerType) == true)
            {
                this.textBox_sqlDef.Text = "SQL Server Name = " + this.LineInfo.SqlServerName
                    + "; Database Prefix = " + this.LineInfo.DatabaseInstanceName
                    + "; SQL Login Name = " + this.LineInfo.DatabaseLoginName
                    + "; SQL Login Password = " + new string('*', this.LineInfo.DatabaseLoginPassword.Length);
                if (this.comboBox_sqlServerType.Text != "MS SQL Server")
                {
                    this.m_nDisableTextChange++;
                    this.comboBox_sqlServerType.Text = "MS SQL Server";
                    this.m_nDisableTextChange--;
                }
            }
            else if (this.LineInfo.SqlServerType == "SQLite"
                || string.Compare(this.LineInfo.SqlServerName, "~sqlite", true) == 0)
            {
                this.textBox_sqlDef.Text = "SQL Server Name = " + this.LineInfo.SqlServerName
        + "; Database Prefix = " + this.LineInfo.DatabaseInstanceName;
                if (this.comboBox_sqlServerType.Text != "SQLite")
                {
                    this.m_nDisableTextChange++;
                    this.comboBox_sqlServerType.Text = "SQLite";
                    this.m_nDisableTextChange--;
                }
            }
            else if (this.LineInfo.SqlServerType == "MySQL Server")
            {
                this.textBox_sqlDef.Text = "SQL Server Name = " + this.LineInfo.SqlServerName
                    + "; Database Prefix = " + this.LineInfo.DatabaseInstanceName
                    + "; SQL Login Name = " + this.LineInfo.DatabaseLoginName
                    + "; SQL Login Password = " + new string('*', this.LineInfo.DatabaseLoginPassword.Length);
                if (this.comboBox_sqlServerType.Text != "MySQL Server")
                {
                    this.m_nDisableTextChange++;
                    this.comboBox_sqlServerType.Text = "MySQL Server";
                    this.m_nDisableTextChange--;
                }
            }
            else if (this.LineInfo.SqlServerType == "Oracle")
            {
                this.textBox_sqlDef.Text = "SQL Server Name = " + this.LineInfo.SqlServerName
                    + "; Database Prefix = " + this.LineInfo.DatabaseInstanceName
                    + "; SQL Login Name = " + this.LineInfo.DatabaseLoginName
                    + "; SQL Login Password = " + new string('*', this.LineInfo.DatabaseLoginPassword.Length);
                if (this.comboBox_sqlServerType.Text != "Oracle")
                {
                    this.m_nDisableTextChange++;
                    this.comboBox_sqlServerType.Text = "Oracle";
                    this.m_nDisableTextChange--;
                }
            }
            else
            {
                this.m_nDisableTextChange++;
                this.comboBox_sqlServerType.Text = "MS SQL Server";
                this.m_nDisableTextChange--;
            }
        }

        void RefreshRootUserInfo()
        {
            if (this.LineInfo == null)
            {
                this.textBox_rootUserInfo.Text = "";
                return;
            }
            if (this.LineInfo.RootUserName != null)
                this.textBox_rootUserInfo.Text = "UserName = " + this.LineInfo.RootUserName;
            else
                this.textBox_rootUserInfo.Text = "UserName = <不修改>";

            if (this.LineInfo.RootPassword != null)
                this.textBox_rootUserInfo.Text += "; Password = " + new string('*', this.LineInfo.RootPassword.Length);
            else
                this.textBox_rootUserInfo.Text += "; Password = <不修改>";

            if (this.LineInfo.RootUserRights != null)
                this.textBox_rootUserInfo.Text += "; Rights = " + this.LineInfo.RootUserRights;
            else
                this.textBox_rootUserInfo.Text += "; Rights = <不修改>";
        }

        // 检查                // 绑定协议
        // 内容是否适合利用对话框进行编辑
        // return:
        //      -1  出错
        //      0   适合编辑
        //      1   不适合编辑
        int CheckBindingsEditable(out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(this.textBox_bindings.Text) == true)
                return 0;
            string[] bindings = this.textBox_bindings.Text.Replace("\r\n", ";").Split(new char[] { ';' });

            int nTcpCount = 0;
            int nHttpCount = 0;
            int nPipeCount = 0;
            for (int i = 0; i < bindings.Length; i++)
            {
                string strOneBinding = bindings[i].Trim();
                if (String.IsNullOrEmpty(strOneBinding) == true)
                    continue;

                Uri one_uri = new Uri(strOneBinding);
                if (one_uri.Scheme.ToLower() == "net.tcp")
                {
                    nTcpCount++;
                }
                else if (one_uri.Scheme.ToLower() == "net.pipe")
                {
                    nPipeCount++;
                }
                else if (one_uri.Scheme.ToLower() == "http")
                {
                    nHttpCount++;
                }
            }

            if (nTcpCount > 1)
            {
                strError = "net.tcp 协议绑定数超过一个";
                return 1;
            }
            if (nPipeCount > 1)
            {
                strError = "net.pipe 协议绑定数超过一个";
                return 1;
            }
            if (nHttpCount > 1)
            {
                strError = "http 协议绑定数超过一个";
                return 1;
            }
            return 0;
        }

        // 准备可选的缺省绑定内容
        int PrepareDefaultBindings(string strTail,
            out string [] default_urls,
            out string strError)
        {
            default_urls = null;
            strError = "";

            // 无法检查
            if (this.VerifyBindings == null)
            {
                default_urls = new string[] {
                    "net.tcp://localhost:8002/dp2kernel/" + strTail,
                    "net.pipe://localhost/dp2kernel/" + strTail,
                    "http://localhost:8001/dp2kernel/" + strTail
                };
                return 0;
            }

            string strTcpUrl = "";
            for (int nPort = 8002; ; nPort++)
            {
                strTcpUrl = "net.tcp://localhost:" + nPort.ToString() + "/dp2kernel/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strTcpUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strPipeUrl = "";
            for (int nNumber = 0; ; nNumber++)
            {
                strPipeUrl = "net.pipe://localhost/dp2kernel/" + strTail;
                if (nNumber > 0)
                    strPipeUrl += nNumber.ToString() + "/";

                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strPipeUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strHttpUrl = "";
            for (int nPort = 8001; ; nPort++)
            {
                strHttpUrl = "http://localhost:" + nPort.ToString() + "/dp2kernel/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strHttpUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            default_urls = new string[] {
                    strTcpUrl,
                    strPipeUrl,
                    strHttpUrl
                };
            return 0;
        }

        private void button_editBindings_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 检查                // 绑定协议
            // 内容是否适合利用对话框进行编辑
            // return:
            //      -1  出错
            //      0   适合编辑
            //      1   不适合编辑
            int nRet = CheckBindingsEditable(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                strError = "当前绑定内容因为 " + strError + " 而无法被专用对话框进行编辑，但可以直接在文本框中进行修改";
                goto ERROR1;
            }

            // TODO: 要找到一个没有被使用过的tcp端口号
            string strTail =  this.InstanceName + (String.IsNullOrEmpty(this.InstanceName) == false ? "/" : "");

            string [] default_urls = null;
            nRet = PrepareDefaultBindings(strTail,
            out default_urls,
            out strError);
            if (nRet == -1)
            {
                strError = "准备缺省班定值时发生错误: " + strError;
                goto ERROR1;
            }
            /*
            string[] default_urls = new string[] {
                    "net.tcp://localhost:8002/dp2kernel/" + strTail,
                    "net.pipe://localhost/dp2kernel/" + strTail,
                    "http://localhost:8001/dp2kernel/" + strTail
                };
             * */

            WcfBindingDlg dlg = new WcfBindingDlg();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.RestEnabled = false;    // 目前不允许 REST.HTTP 协议绑定
            dlg.BasicEnabled = false;    // 目前不允许 BASIC.HTTP 协议绑定
            if (this.IsNew && String.IsNullOrEmpty(this.textBox_bindings.Text) == true)
                dlg.Urls = default_urls;
            else
                dlg.Urls = this.textBox_bindings.Text.Replace("\r\n", ";").Split(new char[] {';'});
            dlg.DefaultUrls = default_urls;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_bindings.Text = string.Join("\r\n", dlg.Urls);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        public string InstanceName
        {
            get
            {
                return this.textBox_instanceName.Text;
            }
            set
            {
                this.textBox_instanceName.Text = value;
            }
        }

        public string DataDir
        {
            get
            {
                return this.textBox_dataDir.Text;
            }
            set
            {
                this.textBox_dataDir.Text = value;
            }
        }

        public string Bindings
        {
            get
            {
                return this.textBox_bindings.Text;
            }
            set
            {
                this.textBox_bindings.Text = value;
            }
        }

        private void textBox_instanceName_TextChanged(object sender, EventArgs e)
        {
            if (IsNew == true)
            {
                // 数据目录textbox要跟从实体名进行变化
                if (m_bDataDirKeyEdited == false)
                {
                    SetDefaultDataDirValue();
                }
            }
        }

        public void SetDefaultDataDirValue()
        {
            this.textBox_dataDir.Text = "c:\\kernel_data"
    + (String.IsNullOrEmpty(this.textBox_instanceName.Text) == false ? "_" : "")
    + this.textBox_instanceName.Text;
        }

        private void textBox_dataDir_KeyPress(object sender, KeyPressEventArgs e)
        {
            m_bDataDirKeyEdited = true;
        }

        private void textBox_instanceName_Leave(object sender, EventArgs e)
        {
            HideMessageTip();

            // 2012/3/1
            // 检测数据目录是否已经存在
            textBox_dataDir_Leave(null, null);
        }

        bool IsMouseOnCancelButton()
        {
            return GuiUtil.PtInRect(Control.MousePosition.X,
                Control.MousePosition.Y,
                this.RectangleToScreen(this.button_Cancel.Bounds));
        }

        // 检测目录是否已经存在
        private void textBox_dataDir_Leave(object sender, EventArgs e)
        {
            // 已经准备Cancel，就不检查了
            if (IsMouseOnCancelButton() == true)
                return;

            // 新建时
            if (IsNew == true 
                && String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
                && this.LoadedDataDir != this.textBox_dataDir.Text)
            {
                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = this.textBox_dataDir.Text;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        MessageBox.Show(this, e1.ErrorInfo);
                        return;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "databases.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 database.xml 文件。\r\n\r\n是否要直接利用此其中(指xml文件和目录中有关帐户文件)的配置信息?",
"安装 dp2Kernel",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            MessageBox.Show(this, e1.ErrorInfo);
                            return;
                        }

                        // refresh
                        this.LineInfo = e1.LineInfo;
                        RefreshSqlDef();
                        RefreshRootUserInfo();
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }

                return;
            }

            // 修改时
            if (IsNew == false
    && String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
    && this.LoadedDataDir != this.textBox_dataDir.Text)
            {
                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = this.textBox_dataDir.Text;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        MessageBox.Show(this, e1.ErrorInfo);
                        return;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "databases.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 database.xml 文件。\r\n\r\n是否要直接利用其中(指xml文件和目录中有关帐户文件)的配置信息？\r\n\r\n是：直接利用其中的信息，也即将其中的配置信息装入当前对话框\r\n否：利用这个数据目录，但其中xml文件和帐户文件的相关信息即将被当前对话框中的值覆盖\r\n\r\n(提示：无论您选“是”“否”，原有目录 '" + this.LoadedDataDir + "' 都会被闲置)",
"安装 dp2Kernel",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            MessageBox.Show(this, e1.ErrorInfo);
                            return;
                        }

                        // refresh
                        this.LineInfo = e1.LineInfo;
                        RefreshSqlDef();
                        RefreshRootUserInfo();
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }
                else if (String.IsNullOrEmpty(this.LoadedDataDir) == false)
                {
                    // 修改目录名

                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"要将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '"+this.textBox_dataDir.Text+"' 么?\r\n\r\n(如果选择“否”，则安装程序在稍后将新创建一个数据目录，并复制进初始内容)",
"安装 dp2Kernel",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.m_bDataDirExist = false;
                        return;
                    }

                    try
                    {
                        // TODO: 需要测试当数据目录中内容尺寸太大，而目标盘无妨容纳时的报错情况
                        Directory.Move(this.LoadedDataDir, this.textBox_dataDir.Text);

                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '"+this.textBox_dataDir.Text+"' 时发生错误: " + ex.Message);
                    }

                }

                return;
            }

        }

        private void button_certificate_Click(object sender, EventArgs e)
        {
            CertificateDialog dlg = new CertificateDialog();

            dlg.SN = LineInfo.CertificateSN;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            LineInfo.CertificateSN = dlg.SN;
        }

        int m_nDisableTextChange = 0;

        private void comboBox_sqlServerType_TextChanged(object sender, EventArgs e)
        {
            if (m_nDisableTextChange == 0)
                this.textBox_sqlDef.Text = "";
        }

    }

    public delegate void VerifyEventHandler(object sender,
        VerifyEventArgs e);

    public class VerifyEventArgs : EventArgs
    {
        public string Value = "";   // [in] 要校验的值
        public string ErrorInfo = "";   // [out]出错信息
    }

    //
    public delegate void LoadXmlFileInfoEventHandler(object sender,
    LoadXmlFileInfoEventArgs e);

    public class LoadXmlFileInfoEventArgs : EventArgs
    {
        public string DataDir = "";   // [in] 数据目录

        public LineInfo LineInfo = null;    // out
        public string ErrorInfo = "";   // [out]出错信息
    }
}

#endif