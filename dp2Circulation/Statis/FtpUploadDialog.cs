using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Collections;
using System.IO;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class FtpUploadDialog : Form
    {
        public MainForm MainForm = null;

        public FtpUploadDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_ftpServerUrl);
                controls.Add(this.textBox_targetDir);
                controls.Add(this.textBox_userName);
                SavePassword save = new SavePassword(this.textBox_password, this.checkBox_savePassword);
                controls.Add(save);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_ftpServerUrl);
                controls.Add(this.textBox_targetDir);
                controls.Add(this.textBox_userName);
                SavePassword save = new SavePassword(this.textBox_password, this.checkBox_savePassword);
                controls.Add(save);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_ftpServerUrl.Text) == true)
            {
                strError = "尚未输入 FTP 服务器 URL";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_userName.Text) == true)
            {
                strError = "尚未输入用户名";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void FtpUploadDialog_Load(object sender, EventArgs e)
        {
#if NO
            this.textBox_ftpServerUrl.Text = this.MainForm.AppInfo.GetString(
                "ftp_upload_report",
                "ftp_server_url",
                "");

            this.textBox_targetDir.Text = this.MainForm.AppInfo.GetString(
                "ftp_upload_report",
                "target_dir",
                "");

            this.textBox_userName.Text = this.MainForm.AppInfo.GetString(
                "ftp_upload_report",
                "username",
                "");

            string strPassword = this.MainForm.AppInfo.GetString(
                "ftp_upload_report",
                "password",
                "");
            if (string.IsNullOrEmpty(strPassword) == false)
                strPassword = this.MainForm.DecryptPasssword(strPassword);

            this.textBox_password.Text = strPassword;

            this.checkBox_savePassword.Checked = this.MainForm.AppInfo.GetBoolean(
                "ftp_upload_report",
                "save_password",
                false);
#endif
        }

        private void FtpUploadDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            this.MainForm.AppInfo.SetString(
    "ftp_upload_report",
    "ftp_server_url",
    this.textBox_ftpServerUrl.Text);

            this.MainForm.AppInfo.SetString(
                "ftp_upload_report",
                "target_dir",
                this.textBox_targetDir.Text);

            this.MainForm.AppInfo.SetString(
                "ftp_upload_report",
                "username",
                this.textBox_userName.Text);

            string strPassword = "";

            if (this.checkBox_savePassword.Checked == true)
                strPassword = this.MainForm.EncryptPassword(this.textBox_password.Text);
                
                this.MainForm.AppInfo.SetString(
                "ftp_upload_report",
                "password",
                strPassword);

            this.MainForm.AppInfo.SetBoolean(
                "ftp_upload_report",
                "save_password",
                this.checkBox_savePassword.Checked);
#endif
        }

        public string FtpServerUrl
        {
            get
            {
                return this.textBox_ftpServerUrl.Text;
            }
            set
            {
                this.textBox_ftpServerUrl.Text = value;
            }
        }

        public string TargetDir
        {
            get
            {
                return this.textBox_targetDir.Text;
            }
            set
            {
                this.textBox_targetDir.Text = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
            set
            {
                this.textBox_password.Text = value;
            }
        }

        // 上传文件
        // 自动创建所需的目录
        // 不会抛出异常
        public static int SafeUploadFile(ref Hashtable dir_table,
            string strLocalFileName,
            string strFtpServerUrl,
            string strServerFilePath,
            string strUserName,
            string strPassword,
            out string strError)
        {
            try
            {
                if (string.IsNullOrEmpty(strServerFilePath) == true)
                {
                    strError = "服务器文件路径不能为空";
                    return -1;
                }
                if (string.IsNullOrEmpty(strLocalFileName) == true)
                {
                    strError = "本地文件路径不能为空";
                    return -1;
                }

                string strDirectory = Path.GetDirectoryName(strServerFilePath);
                int nRedoCount = 0;
            REDO:

                FtpCreateDir(
                    ref dir_table,
                    strFtpServerUrl,
                    strDirectory,
                    strUserName,
                    strPassword);

                int nRet = FtpUploadDialog.FtpUploadFile(strLocalFileName,
                    strFtpServerUrl,
                    strServerFilePath,
                    strUserName,
                    strPassword,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    if (nRedoCount == 0)
                    {
                        ClearDirTable(ref dir_table,
                            strFtpServerUrl,
                            strDirectory);
                        nRedoCount++;
                        goto REDO;
                    }
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                string strServerPath = strFtpServerUrl;
                if (string.IsNullOrEmpty(strServerFilePath) == false)
                    strServerPath += "/" + strServerFilePath.Replace("\\", "/");
                strError = "上传文件 " + strLocalFileName + " 到 " + strServerPath + " 的时候出错: " + ex.Message;
                return -1;
            }
        }

        // return:
        //      -1  出错
        //      0   可能是部分目录不存在。需要先逐级创建目录
        //      1   上传成功
        public static int FtpUploadFile(
            string strLocalFileName,
            string strFtpServerUrl,
            string strServerFilePath,
            string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            string strServerPath = strFtpServerUrl;
            if (string.IsNullOrEmpty(strServerFilePath) == false)
                strServerPath += "/" + strServerFilePath.Replace("\\", "/");

            FtpWebRequest ftpClient = (FtpWebRequest)FtpWebRequest.Create(strServerPath);
            ftpClient.Credentials = new System.Net.NetworkCredential(strUserName.Normalize(), strPassword.Normalize());
            ftpClient.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            ftpClient.UseBinary = true;
            ftpClient.KeepAlive = true;

            System.IO.FileInfo fi = new System.IO.FileInfo(strLocalFileName);
            ftpClient.ContentLength = fi.Length;

            byte[] buffer = new byte[4096];
            long total_bytes = fi.Length;
            try
            {
                using (System.IO.Stream rs = ftpClient.GetRequestStream())
                {
                    using (System.IO.FileStream fs = fi.OpenRead())
                    {
                        while (total_bytes > 0)
                        {
                            int bytes = 0;
                            bytes = fs.Read(buffer, 0, buffer.Length);
                            rs.Write(buffer, 0, bytes);
                            total_bytes = total_bytes - bytes;
                        }
                        //fs.Flush();
                    };
                }
                FtpWebResponse uploadResponse = (FtpWebResponse)ftpClient.GetResponse();
                string strStatus = uploadResponse.StatusDescription;
                uploadResponse.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = ex.Response as FtpWebResponse;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // 可能是文件不存在
                    strError = "上传文件 " + strLocalFileName + " 到 " + strServerPath + " 的时候出错: " + ex.Message;
                    return 0;
                }
                else
                {
                    strError = "上传文件 " + strLocalFileName + " 到 " + strServerPath + " 的时候出错: " + ex.Message;
                    return -1;
                }
            }

            return 1;
        }

        // parameters:
        //      dir_table   存储先前曾经创建过的目录。避免重复操作
        public static void FtpCreateDir(
            ref Hashtable dir_table,
            string strFtpServerUrl,
            string pathToCreate,
            string login,
            string password)
        {
            if (string.IsNullOrEmpty(pathToCreate) == true)
                return;

            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;

            pathToCreate = pathToCreate.Replace("\\", "/");

            string[] subDirs = pathToCreate.Split('/');

            string currentDir = strFtpServerUrl;

            foreach (string subDir in subDirs)
            {
                try
                {
                    currentDir = currentDir + "/" + subDir;

                    if (dir_table != null)
                    {
                        if (dir_table.ContainsKey(currentDir) == true)
                            continue;
                    }

                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(currentDir);
                    reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                    reqFTP.UseBinary = true;
                    reqFTP.Credentials = new NetworkCredential(login.Normalize(), password.Normalize());
                    FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                    ftpStream = response.GetResponseStream();
                    ftpStream.Close();
                    response.Close();
                }
                catch (WebException ex)
                {
                    FtpWebResponse response = ex.Response as FtpWebResponse;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                    }
                    else
                        throw ex;
                }

                if (dir_table != null)
                    dir_table[currentDir] = true;
            }
        }

        // 清除一个路径的相关缓存事项
        // parameters:
        //      dir_table   存储先前曾经创建过的目录。避免重复操作
        public static void ClearDirTable(
            ref Hashtable dir_table,
            string strFtpServerUrl,
            string pathToCreate)
        {
            pathToCreate = pathToCreate.Replace("\\", "/");

            string[] subDirs = pathToCreate.Split('/');

            string currentDir = strFtpServerUrl;

            foreach (string subDir in subDirs)
            {
                currentDir = currentDir + "/" + subDir;

                if (dir_table.ContainsKey(currentDir) == true)
                    dir_table.Remove(currentDir);
            }
        }
    }
}
