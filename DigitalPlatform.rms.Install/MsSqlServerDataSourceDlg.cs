using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;

namespace DigitalPlatform.rms
{
    public partial class MsSqlServerDataSourceDlg : Form
    {
        public MsSqlServerDataSourceDlg()
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

        /*
        public bool SSPI
        {
            get
            {
                return this.radioButton_SSPI.Checked;
            }
            set
            {
                this.radioButton_SSPI.Checked = value;
            }
        }*/

        /*
        public string SqlUserName
        {
            get
            {
                return this.textBox_sqlUserName.Text;
            }
            set
            {
                this.textBox_sqlUserName.Text = value;
            }
        }

        public string SqlPassword
        {
            get
            {
                return this.textBox_sqlPassword.Text;
            }
            set
            {
                this.textBox_sqlPassword.Text = value;
            }
        }
         * */

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

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.textBox_sqlServerName.Text == "")
            {
                strError = "尚未指定 SQL 服务器";
                goto ERROR1;
            }

            if (string.Compare(this.textBox_sqlServerName.Text.Trim(), "~sqlite") == 0)
            {
                strError = "MS SQL 服务器名不能为 '~sqlite'，因为这个名字保留给了 SQLite 内置数据库类型";
                goto ERROR1;
            }

#if NO
                    // 获得 SQL Server 信息
            SqlServerInfo info = null;
            nRet = GetSqlServerInfo(
        this.SqlServerName,
        dlg.SqlUserName,
        dlg.SqlPassword,
        dlg.SSPI,
            out info,
            out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            nRet = GetIntegratedSecurityOnlyMode(this.textBox_sqlServerName.Text, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            bool bISOM = false;
            if (nRet == 1)
                bISOM = true;

            // 集成权限登录唯一方式的情况下，不要创建 登录名
            if (bISOM == true)
            {
                this.textBox_loginName.Text = "";
                this.textBox_loginPassword.Text = "";
                this.textBox_confirmLoginPassword.Text = "";
            }
            else
            {
                if (this.textBox_loginName.Text == "")
                {
                    strError = "尚未指定 dp2Kernel 登录名";
                    goto ERROR1;
                }

                if (this.textBox_loginPassword.Text != this.textBox_confirmLoginPassword.Text)
                {
                    strError = "dp2Kernel 登录名的密码和确认密码不一致";
                    goto ERROR1;
                }
            }

            /*
            if (this.SSPI == false && this.textBox_sqlUserName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL帐号。");
                return;
            }


            // 检测SQL帐户是否正确
            EnableControls(false);
            string strError = "";
            int nRet = this.detect(this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                radioButton_SSPI.Checked,
                out strError);
            EnableControls(true);
            if (nRet == -1)
            {
                strError = strError + "\r\n" + "请重新指定服务器信息。";
                MessageBox.Show(this, strError);
                return;
            }
             * */

            SaLoginDialog dlg = new SaLoginDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.SqlServerName = this.textBox_sqlServerName.Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (string.IsNullOrEmpty(this.textBox_loginName.Text) == false)
            {
                // 创建dp2Kernel登录名

                // 创建一个适合于dpKernel的SQL Server login
                // return:
                //      -1  出错
                //      0   成功
                //      1   原来已经存在，且不允许删除
                nRet = CreateLogin(
                    this.SqlServerName,
                    dlg.SqlUserName,
                    dlg.SqlPassword,
                    dlg.SSPI,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    out strError);
                if (nRet == -1 || nRet == 1)
                {
                    goto ERROR1;
                }
            }

            if (bISOM == true)
            {
                nRet = AddSystemDbCreatorRole(
        this.SqlServerName,
        dlg.SqlUserName,
        dlg.SqlPassword,
        dlg.SSPI,
        out strError);
                if (nRet == -1)
                {
                    strError = "为 登录名 'NT AUTHORITY\\SYSTEM' 添加 'dbcreator' 时出错: " + strError;
                    goto ERROR1;
                }
            }

            /*
            if (nRet == 1)
            {
                string strText = "登录名 '" + this.textBox_loginName.Text + "' 在SQL服务器 '" + this.SqlServerName + "' 中已经存在，但其密码不一定和当前指定的密码相同。\r\n\r\n是否继续使用这个登录名?\r\n(Yes)继续使用；(No)重新指定登录名和密码";
                DialogResult result = MessageBox.Show(this,
                    strText,
                    "setup_dp2Kernel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    this.textBox_loginPassword.Focus();
                    return;
                }
            }
             * */

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            MessageBox.Show(this, "虽然刚才的创建登录名操作失败了，但您也可以在重新指定登录名和密码后，再次按“确定”按钮创建登录名，继续进行安装");
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 创建一个适合于dp2Kernel的SQL Server login
        // return:
        //      -1  出错
        //      0   成功
        //      1   原来已经存在，且不允许删除
        public int CreateLogin(
            string strSqlServerName,
            string strSqlUserName,
            string strSqlUserPassword,
            bool bSSPI,
            string strLoginName,
            string strLoginPassword,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strSqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (bSSPI == true)
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
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接SQL数据库发生出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL数据库发生出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
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
                    int nRet = strVersion.IndexOf(".");
                    if (nRet != -1)
                        strVersion = strVersion.Substring(0, nRet);
                }
                catch (Exception /*ex*/)
                {
                    // strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    // return -1;
                    strVersion = "7";
                }


                // 先删除同名的login
                // strCommand = "IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'" + strLoginName + "') DROP LOGIN [" + strLoginName + "]";  // sql 2005

                if (strVersion == "10")
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
                
                if (strVersion == "10")
                    strCommand = "CREATE LOGIN [" + strLoginName + "] WITH PASSWORD=N'" + strLoginPassword + "', DEFAULT_DATABASE=[master], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF";
                else
                    strCommand = "EXEC sp_addlogin @loginame='"+strLoginName+"',   @passwd='"+strLoginPassword+"', @defdb='master'";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令 "+strCommand+" 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }

                strCommand = "EXEC sp_addsrvrolemember @loginame = '"+strLoginName+"', @rolename = 'dbcreator'";
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

        public int AddSystemDbCreatorRole(
string strSqlServerName,
string strSqlUserName,
string strSqlUserPassword,
bool bSSPI,
out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strSqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (bSSPI == true)
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
                    int nRet = strVersion.IndexOf(".");
                    if (nRet != -1)
                        strVersion = strVersion.Substring(0, nRet);
                }
                catch (Exception /*ex*/)
                {
                    // strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    // return -1;
                    strVersion = "7";
                }

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


        // 2015/4/24
        // 探测 SQL Server 是否为 Integrated Security Only 模式
        // return:
        //      -1  出错
        //      0   不是 Integrated Security Only 模式
        //      1   是 Integrated Security Only 模式
        public int GetIntegratedSecurityOnlyMode(
string strSqlServerName,
out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + strSqlServerName + ";"
                    + "Connect Timeout=30"; // 30秒

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

            try
            {
                int nRet = 1;
                string strCommand = "";
                SqlCommand command = null;

                strCommand = "SELECT SERVERPROPERTY('IsIntegratedSecurityOnly')";
                command = new SqlCommand(strCommand,
                    connection);
                try
                {
                    nRet = (Int32)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                    nRet = 1;
                }

                return nRet;
            }
            finally
            {
                connection.Close();
            }
        }

#if NO
        class SqlServerInfo
        {
            public string ServerName = "";

            // 安装时用于访问 SQL Server 的登录信息
            public string SqlUserName = "";
            public string SqlUserPassword = "";
            public bool SSPI = false;

            public string Version = "";
            public bool IntegratedSecurityOnlyMode = false;
        }

        // 获得 SQL Server 信息
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        public int GetSqlServerInfo(
            string strSqlServerName,
            out SqlServerInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            info = new SqlServerInfo();

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

            return 0;
        }

#endif

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

        private void button_getSqlServerName_Click(object sender, EventArgs e)
        {
            GetSqlServerDlg dlg = new GetSqlServerDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_sqlServerName.Text = dlg.SqlServerName;

#if NO
            if (string.IsNullOrEmpty(this.textBox_sqlServerName.Text) == false)
            {
                string strError = "";
                int nRet = GetIntegratedSecurityOnlyMode(this.textBox_sqlServerName.Text, out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                {
                    if (nRet == 0)
                        this.groupBox_login.Enabled = true;
                    else
                        this.groupBox_login.Enabled = false;
                }
            }
#endif
        }

        /*
        private void button_detect_Click(object sender, EventArgs e)
        {
            if (this.textBox_sqlServerName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL服务器名，无法检测。");
                return;
            }
            if (this.SSPI == false && this.textBox_sqlUserName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL用户，无法检测。");
                return;
            }

            EnableControls(false);
            string strError = "";
            int nRet = this.detect(this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                this.radioButton_SSPI.Checked,
                out strError);
            EnableControls(true);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "SQL服务器信息正确。");
            }
        }

        // return:
        //      -1  出错
        //      0   成功
        public int detect(string strSqlServerName,
            string strSqlUserName,
            string strPassword,
            bool bSSPI,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (bSSPI == true)
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
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接SQL数据库发生出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL数据库发生出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                string strCommand = "";
                SqlCommand command = null;
                strCommand = "use master " + "\n";


                command = new SqlCommand(strCommand,
                    connection);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }
            }
            finally
            {
                connection.Close();
            }

            return 0;
        }
         * */

        public void EnableControls(bool bEnable)
        {
            this.textBox_sqlServerName.Enabled = bEnable;

            /*
            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = bEnable;
                this.textBox_sqlPassword.Enabled = bEnable;
            }*/

            this.textBox_instanceName.Enabled = bEnable;

            this.button_getSqlServerName.Enabled = bEnable;

            // this.button_detect.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;            
        }

        /*
        private void radioButton_SSPI_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = true;
                this.textBox_sqlPassword.Enabled = true;
            }

        }

        private void radioButton_sqlAccount_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = true;
                this.textBox_sqlPassword.Enabled = true;
            }
        }*/

        private void DataSourceDlg_Load(object sender, EventArgs e)
        {
            // radioButton_SSPI_CheckedChanged(null, null);
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

        private void textBox_sqlServerName_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}