using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;
using System.Data.SqlClient;
using System.IO;
using System.Xml;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 用于设定 MS SQL Server 的 Wizard 窗口
    /// </summary>
    public partial class MsSqlServerDataSourceWizard : Form
    {
        MsSqlServerInfo _sqlServerInfo = null;

        public MsSqlServerDataSourceWizard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 调试信息。选择 SQL Server 和创建登录名的过程信息
        /// </summary>
        public string DebugInfo
        {
            get;
            set;
        }

        public string SqlServerName
        {
            get
            {
                return this.textBox_sqlServerName.Text;
            }
            set
            {
                this.textBox_sqlServerName.Text = value;
            }
        }

        public string KernelLoginName
        {
            get
            {
                return this.textBox_loginName.Text;
            }
            set
            {
                this.textBox_loginName.Text = value;
            }
        }

        public string KernelLoginPassword
        {
            get
            {
                return this.textBox_loginPassword.Text;
            }
            set
            {
                this.textBox_loginPassword.Text = value;
                this.textBox_confirmLoginPassword.Text = value;
            }
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

        public string Comment
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        string _originTitle = "";

        private void MsSqlServerDataSourceWizard_Load(object sender, EventArgs e)
        {
            this._originTitle = this.Text;

            SetTitle();
            SetButtonState();
        }

        private void button_prev_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex > 0)
            {
                this.tabControl_main.SelectedIndex--;
                SetTitle();
                SetButtonState();
            }
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex < this.tabControl_main.TabPages.Count - 1)
            {
                this.tabControl_main.SelectedIndex++;
                SetTitle();
                SetButtonState();
            }
        }

        void SetTitle()
        {
            this.Text = this._originTitle + " - " + this.tabControl_main.SelectedTab.Text;
        }

        void SetButtonState()
        {
            if (this.tabControl_main.SelectedIndex == 0)
                this.button_prev.Enabled = false;
            else
                this.button_prev.Enabled = true;

            if (this.tabControl_main.SelectedIndex >= this.tabControl_main.TabPages.Count - 1)
                this.button_next.Enabled = false;
            else
            {
                this.button_next.Enabled = true;
            }

            if (this.tabControl_main.SelectedIndex == this.tabControl_main.TabPages.Count - 1)
                this.button_finish.Enabled = true;
            else
                this.button_finish.Enabled = false;
        }

        private void button_finish_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 如果必要，创建登录名
            if (this._sqlServerInfo == null)
            {
                strError = "尚未登录 SQL Server ...";
                goto ERROR1;
            }

            if (this._sqlServerInfo.IntegratedSecurityOnlyMode == false)
            {
                if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                {
                    strError = "尚未指定 dp2Kernel 登录名";
                    goto ERROR1;
                }

                if (this.textBox_loginPassword.Text != this.textBox_confirmLoginPassword.Text)
                {
                    strError = "dp2Kernel 登录名的密码和确认密码不一致";
                    goto ERROR1;
                }

                // 创建dp2Kernel登录名

                // 创建一个适合于dpKernel的SQL Server login
                // return:
                //      -1  出错
                //      0   成功
                //      1   原来已经存在，且不允许删除
                nRet = CreateLogin(
                    this._sqlServerInfo,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    out strError);
                if (nRet == -1 || nRet == 1)
                {
                    goto ERROR1;
                }


                this.DebugInfo += DateTime.Now.ToString() + " 已创建登录名: " + this.textBox_loginName.Text + "\r\n";
            }

            if (_sqlServerInfo.IntegratedSecurityOnlyMode == true)
            {
                nRet = AddSystemDbCreatorRole(this._sqlServerInfo,
        out strError);
                if (nRet == -1)
                {
                    strError = "为 登录名 'NT AUTHORITY\\SYSTEM' 添加角色 'dbcreator' 时出错: " + strError;
                    goto ERROR1;
                }

                this.DebugInfo += DateTime.Now.ToString() + " 为 登录名 'NT AUTHORITY\\SYSTEM' 添加了角色 'dbcreator'\r\n";

                this.textBox_loginName.Text = "";
                this.textBox_loginPassword.Text = "";
                this.textBox_confirmLoginPassword.Text = "";
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_getSqlServerName_Click(object sender, EventArgs e)
        {
            GetSqlServerDlg dlg = new GetSqlServerDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_sqlServerName.Text = dlg.SqlServerName;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_createLogin)
            {

                MsSqlServerInfo info = null;

                string strError = "";
                // 获得 SQL Server 信息
                // return:
                //      -1  出错
                //      0   放弃
                //      1   成功
                int nRet = GetSqlServerInfo(
                    this.textBox_sqlServerName.Text,
                    out info,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                if (nRet == 0)
                    return;

                this._sqlServerInfo = info;
                this.button_copySqlServerInfo.Enabled = true;

                if (info.IntegratedSecurityOnlyMode == false)
                {
                    if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                        this.textBox_loginName.Text = this.textBox_instanceName.Text;
                    if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                        this.textBox_loginName.Text = "dp2kernel";  // 缺省的名字
                }

                if (info.IntegratedSecurityOnlyMode == true)
                {
                    this.textBox_loginName.Text = "";
                    this.textBox_loginPassword.Text = "";
                    this.textBox_confirmLoginPassword.Text = "";

                    this.groupBox_login.Enabled = false;
                }

                this.DebugInfo = DateTime.Now.ToString() + "\r\n" + info.GetSummary() + "\r\n\r\n";
            }
        }

        class MsSqlServerInfo
        {
            public string ServerName = "";

            // 安装时用于访问 SQL Server 的登录信息
            public string SqlUserName = "";
            public string SqlUserPassword = "";
            public bool SSPI = false;

            public string Version = "";
            public bool IntegratedSecurityOnlyMode = false;

            // 获得摘要文字
            public string GetSummary()
            {
                return "SQL Server 名: " + this.ServerName + "\r\n"
                    + "SQL 用户名: " + this.SqlUserName + "\r\n"
                    + "SSPI: " + this.SSPI.ToString() + "\r\n"
                    + "SQL Server 版本: " + this.Version + "\r\n"
                    + "IntegratedSecurityOnlyMode: " + this.IntegratedSecurityOnlyMode.ToString();
            }
        }

        // 获得 SQL Server 信息
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        int GetSqlServerInfo(
            string strSqlServerName,
            out MsSqlServerInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            info = new MsSqlServerInfo();

            REDO_INPUT:
            SaLoginDialog dlg = new SaLoginDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.SqlServerName = strSqlServerName;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            info.ServerName = strSqlServerName;
            info.SqlUserName = dlg.SqlUserName;
            info.SqlUserPassword = dlg.SqlPassword;
            info.SSPI = dlg.SSPI;

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (info.SSPI == true)
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + strSqlServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }

            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                strError = "建立连接时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接SQL服务器出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                MessageBox.Show(this, strError);
                goto REDO_INPUT;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL服务器出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            /* http://support.microsoft.com/kb/321185
             * 10 -- SQL Server 2008
             * 9 -- SQL Server 2005
             * 8 -- SQL 2000
             * 7 -- SQL 7.0
             * */

            try
            {
                string strVersion = "7";
                string strCommand = "";
                SqlCommand command = null;

                // Debug.Assert(false, "");

                strCommand = "SELECT SERVERPROPERTY('productversion')";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    strVersion = (string)command.ExecuteScalar();
                    // 去掉次要版本号
                    nRet = strVersion.IndexOf(".");
                    if (nRet != -1)
                        strVersion = strVersion.Substring(0, nRet);
                }
                catch (Exception /*ex*/)
                {
                    // strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    // return -1;
                    strVersion = "7";
                }

                info.Version = strVersion;

                strCommand = "SELECT SERVERPROPERTY('IsIntegratedSecurityOnly')";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    nRet = (Int32)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    //strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    //return -1;
                    nRet = 1;
                }

                if (nRet == 1)
                    info.IntegratedSecurityOnlyMode = true;
                else
                    info.IntegratedSecurityOnlyMode = false;

            }
            finally
            {
                connection.Close();
            }

            return 1;
        }

        int AddSystemDbCreatorRole(MsSqlServerInfo info,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + info.ServerName + ";"
                + "Connect Timeout=30";

            if (info.SSPI == true)
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + info.ServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }

            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接SQL服务器出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL服务器出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            /* http://support.microsoft.com/kb/321185
             * 10 -- SQL Server 2008
             * 9 -- SQL Server 2005
             * 8 -- SQL 2000
             * 7 -- SQL 7.0
             * */

            try
            {
                string strCommand = "";
                SqlCommand command = null;

                strCommand = "EXEC sp_addsrvrolemember @loginame = 'NT AUTHORITY\\SYSTEM', @rolename = 'dbcreator'";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }
            }
            finally
            {
                connection.Close();
            }

            return 0;
        }


        // 创建一个适合于dp2Kernel的SQL Server login
        // return:
        //      -1  出错
        //      0   成功
        //      1   原来已经存在，且不允许删除
        int CreateLogin(
            MsSqlServerInfo info,
            string strLoginName,
            string strLoginPassword,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + info.ServerName + ";"
                + "Connect Timeout=30";

            if (info.SSPI == true)
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + info.ServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }


            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接 SQL 服务器时出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接 SQL 服务器时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            /* http://support.microsoft.com/kb/321185
             * 10 -- SQL Server 2008
             * 9 -- SQL Server 2005
             * 8 -- SQL 2000
             * 7 -- SQL 7.0
             * */

                // 先删除同名的login
                // strCommand = "IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'" + strLoginName + "') DROP LOGIN [" + strLoginName + "]";  // sql 2005

            try
            {
                string strCommand = "";
                SqlCommand command = null;

                if (info.Version == "10")
                    strCommand = "DROP LOGIN [" + strLoginName + "]";
                else
                    strCommand = "EXEC sp_droplogin @loginame = '" + strLoginName + "'";    // sql 2000
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // 需要判断SqlException的具体类型，针对性地报错或者不报错
                    if (IsSqlErrorNo(ex, 15007) == true)
                    {
                        // 15007    The login '%s' does not exist.
                    }
                    else if (IsSqlErrorNo(ex, 15151) == true)
                    {
                        // ms-help://MS.SQLCC.v10/MS.SQLSVR.v10.zh-CHS/s10de_5techref/html/e9f7b86b-891e-4abb-938e-c39c707f5a5f.htm
                        // 15151    无法对 %S_MSG '%.*ls' 执行 %S_MSG，因为它不存在，或者您没有所需的权限。
                    }
                    else if (IsSqlErrorNo(ex, 15174) == true)
                    {
                        //    15174   16   登录   ''%1!''   拥有一个或多个数据库。请更改下列数据库的所有者后再除去该登录：   
                        strError = "警告：执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                        return 1;
                    }
                    else
                    {
                        strError = "警告：执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                        MessageBox.Show(this, strError);
                    }
                }
                catch (Exception ex)
                {
                    strError = "警告：执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    MessageBox.Show(this, strError);
                }

                if (info.Version == "10")
                    strCommand = "CREATE LOGIN [" + strLoginName + "] WITH PASSWORD=N'" + strLoginPassword + "', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF";
                else
                    strCommand = "EXEC sp_addlogin @loginame='" + strLoginName + "',   @passwd='" + strLoginPassword + "', @defdb='master'";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }

                strCommand = "EXEC sp_addsrvrolemember @loginame = '" + strLoginName + "', @rolename = 'dbcreator'";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }
            }
            finally
            {
                connection.Close();
            }

            return 0;
        }

        // 是否包含了指定的错误号?
        // 错误码表：ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.SQL.v2000.en/trblsql/tr_syserrors2_93i1.htm
        // 15007    The login '%s' does not exist.
        static bool IsSqlErrorNo(SqlException ex,
            int nNumber)
        {
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                SqlError error = ex.Errors[i];

                if (error.Number == nNumber)
                    return true;
            }
            return false;
        }


        private void tabPage_sqlServerName_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_sqlServerName.Text) == true)
            {
                MessageBox.Show(this, "请指定 SQL 服务器名");
                e.Cancel = true;
            }
        }

        private void button_copySqlServerInfo_Click(object sender, EventArgs e)
        {
            if (this._sqlServerInfo != null)
            {
                Clipboard.SetDataObject(this._sqlServerInfo.GetSummary(), true);
            }
        }

        private void textBox_sqlServerName_TextChanged(object sender, EventArgs e)
        {
            this._sqlServerInfo = null;
            this.button_copySqlServerInfo.Enabled = false;
        }


    }
}
