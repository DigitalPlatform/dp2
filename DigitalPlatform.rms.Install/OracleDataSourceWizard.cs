using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// 用于设定 Oracle database 的 Wizard 窗口
    /// </summary>
    public partial class OracleDataSourceWizard : Form
    {
        FloatingMessageForm _floatingMessage = null;

        OracleSqlServerInfo _sqlServerInfo = null;

        public OracleDataSourceWizard()
        {
            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }
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

        bool _createMode = true;

        List<Control> _freeControls = new List<Control>();

        void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void OracleDataSourceWizard_Load(object sender, EventArgs e)
        {
            this._originTitle = this.Text;

            SetTitle();
            SetButtonState();

            if (string.IsNullOrEmpty(this.textBox_sqlServerName.Text) == true)
            {
                BuildServerName();
                this.textBox_sqlServerName.ReadOnly = true;
            }
            else
            {
                this._createMode = false;   // 修改模式。不使用模板构造 sql server name
                this.textBox_sqlServerName.ReadOnly = false;

                this.textBox_port.Enabled = false;
                this.textBox_hostName.Enabled = false;
                this.textBox_serviceName.Enabled = false;
                this.tabControl_serverName.TabPages.Remove(this.tabPage_template);
                ControlExtention.AddFreeControl(_freeControls, this.tabPage_template);  // 2015/11/7
            }
        }

        private void OracleDataSourceWizard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_floatingMessage != null)
                _floatingMessage.Close();
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
                strError = "尚未登录 Oracle 数据库服务器 ...";
                goto ERROR1;
            }

            this.Enabled = false;
            try
            {
                if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                {
                    strError = "尚未指定 dp2Kernel 用户";
                    goto ERROR1;
                }

                if (this.textBox_loginPassword.Text != this.textBox_confirmLoginPassword.Text)
                {
                    strError = "dp2Kernel 用户的密码和确认密码不一致";
                    goto ERROR1;
                }

                // 创建dp2Kernel用户

                // 创建一个适合于dpKernel的 Oracle 数据库用户
                // return:
                //      -1  出错
                //      0   成功
                //      1   原来已经存在，且不允许删除
                nRet = CreateLogin(
                    this._sqlServerInfo,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    this.textBox_tableSpaceFile.Text,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                if (nRet == 1)
                {
                    // return:
                    //      -1  出错
                    //      0   密码不正确
                    //      1   正确
                    nRet = VerifyUserNameAndPassword(
                        this._sqlServerInfo.ServerName,
                        this.textBox_loginName.Text,
                        this.textBox_loginPassword.Text,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        strError = "用户 '" + this.textBox_loginName.Text + "' 已经存在，但验证其密码的时候出错: " + strError;
                        goto ERROR1;
                    }
                }

                this.DebugInfo += DateTime.Now.ToString() + " 已创建登录名: " + this.textBox_loginName.Text + "\r\n";
            }
            finally
            {
                this.Enabled = true;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_createLogin)
            {
                this._floatingMessage.Text = "正在访问 Oracle 数据库服务器 ...";
                try
                {
                    OracleSqlServerInfo info = null;

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

                    textBox_loginName_TextChanged(this, new EventArgs());

                    this.button_copySqlServerInfo.Enabled = true;

                    {
                        if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                            this.textBox_loginName.Text = this.textBox_instanceName.Text;
                        if (string.IsNullOrEmpty(this.textBox_loginName.Text) == true)
                            this.textBox_loginName.Text = "dp2kernel";  // 缺省的名字

                        // 设置缺省的表空间文件名
                        if (string.IsNullOrEmpty(this.textBox_tableSpaceFile.Text) == true)
                        {
                            string strFileName = this.textBox_instanceName.Text.Replace(" ", "");
                            if (string.IsNullOrEmpty(strFileName) == true)
                                strFileName = "dp2kernel";
                            this.textBox_tableSpaceFile.Text = "c:\\oracle_data\\" + strFileName + ".dbf";
                        }
                    }

                    this.DebugInfo = DateTime.Now.ToString() + "\r\n" + info.GetSummary() + "\r\n\r\n";
                }
                finally
                {
                    this._floatingMessage.Text = "";
                }
            }

        }

        class DbaUser
        {
            public string Name = "";
            public string TableSpace = "";
        }

        class OracleSqlServerInfo
        {
            public string ServerName = "";

            // 安装时用于访问 SQL Server 的登录信息
            // 通常是 sys 或者 system 账户
            public string SqlUserName = "";
            public string SqlUserPassword = "";

            public string Version = "";

            public List<DbaUser> DbaUsers = new List<DbaUser>();

            // 获得摘要文字
            public string GetSummary()
            {
                StringBuilder text = new StringBuilder();
                foreach (DbaUser user in this.DbaUsers)
                {
                    text.Append("username=" + user.Name + ";default_tablespace=" + user.TableSpace+"\r\n");
                }
                return "数据库服务器名: " + this.ServerName + "\r\n"
                    + "用户名: " + this.SqlUserName + "\r\n"
                    + "当前 dba 用户: \r\n" + text.ToString() + "\r\n";
            }

            public DbaUser FindUser(string strUserName)
            {
                foreach(DbaUser user in this.DbaUsers)
                {
                    if (user.Name.ToUpper() == strUserName.ToUpper())
                        return user;
                }

                return null;
            }
        }

        // 获得 SQL Server 信息
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        int GetSqlServerInfo(
            string strSqlServerName,
            out OracleSqlServerInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            info = new OracleSqlServerInfo();

            SystemLoginDialog dlg = new SystemLoginDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.SqlServerName = strSqlServerName;
            dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            info.ServerName = strSqlServerName;
            info.SqlUserName = dlg.SqlUserName;
            info.SqlUserPassword = dlg.SqlPassword;

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            OracleConnection connection = null;
            try
            {
                connection = new OracleConnection(strConnection);
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
            catch (OracleException sqlEx)
            {
                // ex.Number == 12154
                // ORA-12154: TNS: 无法解析指定的连接标识符
                strError = "连接SQL服务器出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                MessageBox.Show(this, strError);
                dlg.Comment = "登录错误: " + strError + "\r\n请重新登录";
                goto REDO_INPUT;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL服务器出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                string strCommand = "";
                OracleCommand command = null;

                strCommand = "select username,default_tablespace from dba_users";
                command = new OracleCommand(strCommand,
                    connection);
                try
                {
                    OracleDataReader reader = command.ExecuteReader();
                    while (reader.Read() == true)
                    {
                        DbaUser user = new DbaUser();
                        user.Name = reader.GetString(0);
                        user.TableSpace = reader.GetString(1);
                        info.DbaUsers.Add(user);
                    }
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

            return 1;
        }



        private void textBox_hostName_TextChanged(object sender, EventArgs e)
        {
            BuildServerName();
        }

        private void textBox_port_TextChanged(object sender, EventArgs e)
        {
            BuildServerName();
        }

        private void textBox_serviceName_TextChanged(object sender, EventArgs e)
        {
            BuildServerName();
        }

        private void textBox_serverNameTemplate_TextChanged(object sender, EventArgs e)
        {
            BuildServerName();
        }

        // 根据参数构建 ServerName 字符串
        void BuildServerName()
        {
            Hashtable table = new Hashtable();
            table["%host%"] = this.textBox_hostName.Text;
            table["%port%"] = this.textBox_port.Text;
            table["%servicename%"] = this.textBox_serviceName.Text;

            this.textBox_sqlServerName.Text = StringUtil.MacroString(table,
                this.textBox_serverNameTemplate.Text);
        }


        // 创建一个适合于dpKernel的 Oracle 数据库用户
        // return:
        //      -1  出错
        //      0   成功
        //      1   原来已经存在，且不允许删除
        static int CreateLogin(
            OracleSqlServerInfo info,
            string strLoginName,
            string strLoginPassword,
            string strTableSpaceFileName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + info.ServerName + ";"
                + "Connect Timeout=30";

            try
            {
                using (OracleConnection connection = new OracleConnection(strConnection))
                {
                    connection.Open();


                    // 检查用户是否已经存在
                    {
                        string strCommand = "";
                        DbaUser user = null;

                        strCommand = "select username,default_tablespace from dba_users where username='" + strLoginName.ToUpper() + "'";
                        try
                        {
                            using (OracleCommand command = new OracleCommand(strCommand, connection))
                            {
                                OracleDataReader reader = command.ExecuteReader();
                                while (reader.Read() == true)
                                {
                                    user = new DbaUser();
                                    user.Name = reader.GetString(0);
                                    user.TableSpace = reader.GetString(1);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                            return -1;
                        }

                        if (user != null)
                        {
                            // 用户已经存在
                            // 需要验证密码是否正确?

                            // 检查表空间名，不能是 SYSTEM 或者 SYSAUX
                            if (user.TableSpace.ToUpper() == "SYSTEM"
                                || user.TableSpace.ToUpper() == "SYSAUX")
                            {
                                strError = "用户 '"+user.Name+"' 的表空间为 "+user.TableSpace+"，不适合用作 dp2kernel 用户。请重新指定或创建一个用户";
                                return -1;
                            }
                            return 1;
                        }
                    }

                    // 创建用户
                    {
                        // 先确保表空间物理文件不会和已有文件冲突
                        if (string.IsNullOrEmpty(strTableSpaceFileName) == true)
                        {
                            strError = "尚未指定表空间物理文件名";
                            return -1;
                        }

                        if (File.Exists(strTableSpaceFileName) == true)
                        {
                            strError = "指定的表空间物理文件 '" + strTableSpaceFileName + "' 已经存在。创建用户失败。请重新指定一个表空间文件名";
                            return -1;
                        }

                        // 确保表空间文件名中用到的路径已经创建好目录
                        PathUtil.TryCreateDir(Path.GetDirectoryName(strTableSpaceFileName));

                        string strCommand = "";

                        string strTableSpaceName = "ts_" + strLoginName;

                        strCommand = "create tablespace " + strTableSpaceName + " datafile '" + strTableSpaceFileName + "' size 100m autoextend on ; "
                            + "create user " + strLoginName + " identified by " + strLoginPassword + " default tablespace " + strTableSpaceName + "; "
                            + "grant dba, connect to " + strLoginName;
                        string[] lines = strCommand.Split(new char[] { ';' });
                        foreach (string line in lines)
                        {
                            string strLine = line.Trim();
                            if (string.IsNullOrEmpty(strLine) == true)
                                continue;

                            try
                            {
                                using (OracleCommand command = new OracleCommand(strLine, connection))
                                {
                                    nRet = command.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                strError = "执行命令 " + strLine + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                                return -1;
                            }
                        }
                    }
                }
            }
            catch (OracleException sqlEx)
            {
                strError = "出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            return 0;
        }

        // 根据表空间名字获得表空间的物理文件名
        // return:
        //      -1  出错
        //      0   成功。此时 strTableSpaceFileName 依然可能为空
        static int GetTableSpaceFileName(
    OracleSqlServerInfo info,
    string strTableSpaceName,
    out string strTableSpaceFileName,
    out string strError)
        {
            strError = "";
            strTableSpaceFileName = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + info.SqlUserName + ";"    //帐户和密码
                + "Password=" + info.SqlUserPassword + ";"
                + "Data Source=" + info.ServerName + ";"
                + "Connect Timeout=30";

            try
            {
                using (OracleConnection connection = new OracleConnection(strConnection))
                {
                    connection.Open();

                    /*
 * SQL> select tablespace_name, file_name from dba_data_files where tablespace_name
= 'TS_DP2KERNEL_ORACLE';
                     * */

                    string strCommand = "";

                    strCommand = "select file_name from dba_data_files where tablespace_name = '" + strTableSpaceName.ToUpper() + "'";
                    try
                    {
                        using (OracleCommand command = new OracleCommand(strCommand, connection))
                        {
                            OracleDataReader reader = command.ExecuteReader();
                            while (reader.Read() == true)
                            {
                                strTableSpaceFileName = reader.GetString(0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "执行命令 " + strCommand + " 出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                        return -1;
                    }

                }
            }
            catch (OracleException sqlEx)
            {
                strError = "出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            return 0;
        }

        private void button_copySqlServerInfo_Click(object sender, EventArgs e)
        {
            if (this._sqlServerInfo != null)
            {
                Clipboard.SetDataObject(this._sqlServerInfo.GetSummary(), true);
            }
        }

        private void textBox_loginName_TextChanged(object sender, EventArgs e)
        {
            // loginName 是否已经创建？
            if (this._sqlServerInfo != null
                && string.IsNullOrEmpty(this.textBox_loginName.Text) == false)
            {
                DbaUser user = this._sqlServerInfo.FindUser(this.textBox_loginName.Text);
                if (user != null)
                {
                    this.textBox_tableSpaceName.Text = user.TableSpace;

                    string strTableSpaceFileName = "";

                    string strError = "";
                    int nRet = GetTableSpaceFileName(
                        this._sqlServerInfo,
                        user.TableSpace,
                        out strTableSpaceFileName,
                        out strError);
                    if (nRet == -1)
                    {
                        // MessageBox.Show(this, strError);
                        // 最好是浮动显示出来
                    }
                    else
                        this.textBox_tableSpaceFile.Text = strTableSpaceFileName;
                }
            }
        }

        private void button_getServiceName_Click(object sender, EventArgs e)
        {
            // https://docs.oracle.com/database/121/ODPNT/OracleDataSourceEnumeratorClass.htm
            List<string> lines = new List<string>();

            OracleDataSourceEnumerator test = new OracleDataSourceEnumerator();

            DataTable dt = test.GetDataSources();

            string strLine = "";
            foreach (DataColumn column in dt.Columns)
            {
                strLine += column.ColumnName + ",";
            }
            lines.Add(strLine);

            foreach (DataRow row in dt.Rows)
            {
                strLine = "";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    strLine += row[i] += ",";
                }
                lines.Add(strLine);
            }

            return;
        }

        // return:
        //      -1  出错
        //      0   密码不正确
        //      1   正确
        public static int VerifyUserNameAndPassword(
    string strSqlServerName,
    string strSqlUserName,
    string strSqlUserPassword,
    out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strSqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            try
            {
                using (OracleConnection connection = new OracleConnection(strConnection))
                {
                    connection.Open();

                }
            }
            catch (OracleException sqlEx)
            {
                // {"ORA-01017: invalid username/password; logon denied"}
                if (sqlEx.Number == 1017)
                {
                    strError = "用户名或密码不正确";
                    return 0;
                }
                strError = "验证用户名和密码时出错： " + sqlEx.Message;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "验证用户名和密码时出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            return 1;
        }


    }
}
