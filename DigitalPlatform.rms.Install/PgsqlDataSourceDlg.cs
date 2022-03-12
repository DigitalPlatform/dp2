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
using Npgsql;

namespace DigitalPlatform.rms
{
    public partial class PgsqlDataSourceDlg : Form
    {
        public PgsqlDataSourceDlg()
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
                strError = "尚未指定 PostgreSQL 服务器";
                goto ERROR1;
            }

            if (string.Compare(this.textBox_sqlServerName.Text.Trim(), "~sqlite") == 0)
            {
                strError = "PostgreSQL 服务器名不能为 '~sqlite'，因为这个名字保留给了 SQLite 内置数据库类型";
                goto ERROR1;
            }


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

            this.button_OK.Enabled = false;
            try
            {
                // 创建 Pgsql 的数据库。这里数据库实际上是一个实例内的公共空间，不是 MS SQL Server 那种数据库概念
                nRet = CreateDatabase(
                    this.SqlServerName,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    this.textBox_adminDatabaseName.Text,
                    this.textBox_instanceName.Text,
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
    "在自动创建数据库的过程中发生错误: \r\n\r\n"
    + strError
    + "\r\n\r\n是否依然采用这些参数继续完成安装?",
    "PgsqlServerDataSourceDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        MessageBox.Show(this, "请修改服务器参数");
                        return;
                    }
                }

                // 检查服务器参数
                nRet = VerifySqlServer(
                    this.SqlServerName,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    this.textBox_instanceName.Text,
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
    "在检查服务器参数的过程中发生错误: \r\n\r\n"
    + strError
    + "\r\n\r\n是否依然采用这些参数继续完成安装?",
    "PgsqlServerDataSourceDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        MessageBox.Show(this, "请修改服务器参数");
                        return;
                    }
                }
            }
            finally
            {
                this.button_OK.Enabled = true;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            // MessageBox.Show(this, "虽然刚才的创建登录名操作失败了，但您也可以在重新指定登录名和密码后，再次按“确定”按钮创建登录名，继续进行安装");
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public static int DeleteDatabase(
string strSqlServerName,
string strSqlUserName,
string strSqlUserPassword,
string strAdminDatabase,
string strDatabaseName,
out string strError)
        {
            strError = "";

            if (strSqlServerName.Contains("="))
            {
                strError = $"strSqlServerName 内容 '{strSqlServerName}' 中不允许包含等号";
                return -1;
            }
            string strConnection = $"Host={strSqlServerName};Username={strSqlUserName};Password={strSqlUserPassword};Database={strAdminDatabase};"; // Database={strDatabaseName}

            try
            {
                using (var connection = new NpgsqlConnection(strConnection))
                {
                    try
                    {
                        connection.Open();
                        string strCommand = $"DROP DATABASE \"{strDatabaseName}\"";
                        using (var command = new NpgsqlCommand(strCommand, connection))
                        {
                            var count = command.ExecuteNonQuery();
                        }
                    }
                    catch (PostgresException ex)
                    {
                        // https://www.postgresql.org/docs/current/errcodes-appendix.html
                        // 3D000	invalid_catalog_name
                        if (ex.SqlState == "3D000")
                        {
                            strError = $"数据库 '{strDatabaseName}' 不存在";
                            return -1;
                        }
                        strError = $"删除数据库 {strDatabaseName} 出错: { ex.Message }";
                        return -1;
                    }
                    catch (NpgsqlException sqlEx)
                    {
                        strError = $"删除数据库 {strDatabaseName} 出错: { sqlEx.Message }";
                        int nError = (int)sqlEx.ErrorCode;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = $"删除数据库 {strDatabaseName} 出错: { ex.Message }";
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = $"删除数据库 {strDatabaseName} 出错: { ex.Message }";
                return -1;
            }
            return 0;
        }


        public static int CreateDatabase(
string strSqlServerName,
string strSqlUserName,
string strSqlUserPassword,
string strAdminDatabase,
string strDatabaseName,
out string strError)
        {
            strError = "";

            if (strSqlServerName.Contains("="))
            {
                strError = $"strSqlServerName 内容 '{strSqlServerName}' 中不允许包含等号";
                return -1;
            }
            string strConnection = $"Host={strSqlServerName};Username={strSqlUserName};Password={strSqlUserPassword};Database={strAdminDatabase};"; // Database={strDatabaseName}

            try
            {
                using (var connection = new NpgsqlConnection(strConnection))
                {
                    try
                    {
                        connection.Open();
                        string strCommand = $"CREATE DATABASE \"{strDatabaseName}\"";
                        using(var command = new NpgsqlCommand(strCommand, connection))
                        {
                            var count = command.ExecuteNonQuery();
                        }
                    }
                    catch(PostgresException ex)
                    {
                        // https://www.postgresql.org/docs/current/errcodes-appendix.html
                        // 42P04	duplicate_database
                        if (ex.SqlState == "42P04")
                            return 0;
                        strError = $"创建数据库 {strDatabaseName} 出错: { ex.Message }";
                        return -1;
                    }
                    catch (NpgsqlException sqlEx)
                    {
                        strError = $"创建数据库 {strDatabaseName} 出错: { sqlEx.Message }";
                        int nError = (int)sqlEx.ErrorCode;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        // strError = "连接 SQL 数据库出错： " + ex.Message + " 类型:" + ex.GetType().ToString();
                        strError = $"创建数据库 {strDatabaseName} 出错: { ex.Message }";
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }
            return 0;
        }

        public static int VerifySqlServer(
string strSqlServerName,
string strSqlUserName,
string strSqlUserPassword,
string strDatabaseName,
out string strError)
        {
            strError = "";

            if (strSqlServerName.Contains("="))
            {
                strError = $"strSqlServerName 内容 '{strSqlServerName}' 中不允许包含等号";
                return -1;
            }
            // strSqlServerName 的内容一般为 "localhost;Database=postgres" 形态。等于包含了 database 参数
            string strConnection = $"Host={strSqlServerName};Username={strSqlUserName};Password={strSqlUserPassword};Database={strDatabaseName};"; // Database={strDatabaseName}

            try
            {
                using (var connection = new NpgsqlConnection(strConnection))
                {
                    try
                    {
                        connection.Open();
                        NpgsqlConnection.ClearPool(connection);
                    }
                    catch (NpgsqlException sqlEx)
                    {
                        strError = "连接 SQL 数据库出错： " + sqlEx.Message + "。";
                        int nError = (int)sqlEx.ErrorCode;
                        return -1;
                    }
                    catch (Exception ex)
                    {
                        strError = "连接 SQL 数据库出错： " + ex.Message + " 类型:" + ex.GetType().ToString();
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }
            return 0;
        }

        private void button_getSqlServerName_Click(object sender, EventArgs e)
        {
            /*
            GetSqlServerDlg dlg = new GetSqlServerDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_sqlServerName.Text = dlg.SqlServerName;
            */
        }

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

        private void textBox_loginName_TextChanged(object sender, EventArgs e)
        {
            this.textBox_adminDatabaseName.Text = this.textBox_loginName.Text;
        }

        private void checkBox_enableModifyAdminDatabaseName_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_adminDatabaseName.ReadOnly = !this.checkBox_enableModifyAdminDatabaseName.Checked;
        }

        private void button_deleteDatabase_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
$"确实要删除数据库 '{this.InstanceName}'?",
"PgsqlServerDataSourceDlg",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            this.button_OK.Enabled = false;
            try
            {
                // 删除 Pgsql 的数据库。这里数据库实际上是一个实例内的公共空间，不是 MS SQL Server 那种数据库概念
                int nRet = DeleteDatabase(
                    this.SqlServerName,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    this.textBox_adminDatabaseName.Text,
                    this.textBox_instanceName.Text,
                    out string strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this,
    $"在删除数据库 '{this.InstanceName}' 的过程中发生错误: \r\n\r\n"
    + strError);
                    return;
                }

                MessageBox.Show(this, $"数据库 '{this.InstanceName}' 删除成功");
            }
            finally
            {
                this.button_OK.Enabled = true;
            }
        }
    }
}