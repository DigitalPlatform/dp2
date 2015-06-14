using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Text;


namespace DigitalPlatform.CirculationClient
{
    [Serializable()]
    public class dp2Server
    {
        public string Name = "";    // 服务器名

        public string Url = "";	// 服务器URL,应是webservice endpoint

        public string DefaultUserName = "";

        // [NonSerialized]
        string StorageDefaultPassword = Cryptography.Encrypt("", "dp2rms");

        public bool SavePassword = false;

        [NonSerialized]
        public bool Verified = false;   // 是验证过序列号?

        public dp2Server()
        {
        }

        // 拷贝构造函数
        public dp2Server(dp2Server refServer)
        {
            this.Name = refServer.Name;
            this.Url = refServer.Url;
            this.DefaultUserName = refServer.DefaultUserName;
            this.StorageDefaultPassword = refServer.StorageDefaultPassword;
            this.SavePassword = refServer.SavePassword;
            this.Verified = refServer.Verified;
        }

        public string DefaultPassword
        {
            get
            {
                if (SavePassword == false)
                    return "";
                return Cryptography.Decrypt(StorageDefaultPassword, "dp2rms");
            }
            set
            {
                StorageDefaultPassword = Cryptography.Encrypt(value, "dp2rms");
            }
        }
    }


    /// <summary>
    /// 储存服务器信息的容器类
    /// 打算取代HostList
    /// </summary>
    [Serializable()]
    public class dp2ServerCollection : ArrayList
    {
        [NonSerialized]
        string m_strFileName = "";

        [NonSerialized]
        bool m_bChanged = false;

        [NonSerialized]
        public IWin32Window ownerForm = null;

        public event dp2ServerChangedEventHandle ServerChanged = null;


        public dp2ServerCollection()
        {
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;
            }
        }

        public string FileName
        {
            get
            {
                return m_strFileName;
            }
            set
            {
                this.m_strFileName = value;
            }
        }

        // 索引器 字符串作索引
        public dp2Server this[string strUrl]
        {
            get
            {
                return this.GetServer(strUrl);
            }

        }

        /// <summary>
        /// 根据 URL 获得 dp2Server 对象
        /// </summary>
        /// <param name="strUrl">服务器 URL</param>
        /// <returns>dp2Server 对象</returns>
        public dp2Server GetServer(string strUrl)
        {
            strUrl = StringUtil.CanonicalizeHostUrl(strUrl);

            dp2Server server = null;
            for (int i = 0; i < this.Count; i++)
            {
                server = (dp2Server)this[i];
                string strCurrentUrl = StringUtil.CanonicalizeHostUrl(server.Url);
                if (String.Compare(strCurrentUrl, strUrl, true) == 0)
                    return server;
            }

            return null;
        }

        // 2007/9/14
        public dp2Server GetServerByName(string strServerName)
        {
            dp2Server server = null;
            for (int i = 0; i < this.Count; i++)
            {
                server = (dp2Server)this[i];
                if (server.Name == strServerName)
                    return server;
            }

            return null;
        }

        // 克隆。
        // 新数组中的对象完全是新创建的。
        public dp2ServerCollection Dup()
        {
            dp2ServerCollection newServers = new dp2ServerCollection();

            for (int i = 0; i < this.Count; i++)
            {
                dp2Server newServer = new dp2Server((dp2Server)this[i]);
                newServers.Add(newServer);
            }

            newServers.m_strFileName = this.m_strFileName;
            newServers.m_bChanged = this.m_bChanged;
            newServers.ownerForm = this.ownerForm;

            return newServers;
        }

        // 将另一对象的数组内容灌入本对象
        public void Import(dp2ServerCollection servers)
        {
            this.Clear();
            this.AddRange(servers);
            this.m_bChanged = true;

            // 新增加的动作
            dp2ServerChangedEventArgs e = new dp2ServerChangedEventArgs();
            e.Url = "";
            e.ServerChangeAction = dp2ServerChangeAction.Import;
            OnServerChanged(this, e);

        }

        // 创建一个新的Server对象
        // return:
        //		-1	出错
        //		0	加入了
        //		1	发现重复，没有加入
        public int NewServer(
            string strName,
            string strUrl,
            int nInsertPos)
        {
            dp2Server server = null;
            // 暂时不去重

            server = new dp2Server();
            server.Url = strUrl;
            server.Name = strName;

            if (nInsertPos == -1)
                this.Add(server);
            else
                this.Insert(nInsertPos, server);

            m_bChanged = true;

            dp2ServerChangedEventArgs e = new dp2ServerChangedEventArgs();
            e.Url = strUrl;
            e.ServerChangeAction = dp2ServerChangeAction.Add;
            OnServerChanged(this, e);

            return 0;
        }


        public void OnServerChanged(object sender, dp2ServerChangedEventArgs e)
        {
            if (this.ServerChanged != null)
            {
                this.ServerChanged(sender, e);
            }

        }

        // 创建一个新的Server对象
        // return:
        public dp2Server NewServer(int nInsertPos)
        {
            dp2Server server = null;
            server = new dp2Server();

            if (nInsertPos == -1)
                this.Add(server);
            else
                this.Insert(nInsertPos, server);

            m_bChanged = true;

            return server;
        }

        // 从文件中装载创建一个ServerCollection对象
        // parameters:
        //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
        //							如果==true，函数直接返回一个新的空ServerCollection对象
        // Exception:
        //			FileNotFoundException	文件没找到
        //			SerializationException	版本迁移时容易出现
        public static dp2ServerCollection Load(
            string strFileName,
            bool bIgnorFileNotFound)
        {
            Stream stream = null;
            dp2ServerCollection servers = null;

            try
            {
                stream = File.Open(strFileName, FileMode.Open);
            }
            catch (FileNotFoundException ex)
            {
                if (bIgnorFileNotFound == false)
                    throw ex;

                servers = new dp2ServerCollection();
                servers.m_strFileName = strFileName;

                // 让调主有一个新的空对象可用
                return servers;
            }


            BinaryFormatter formatter = new BinaryFormatter();

            servers = (dp2ServerCollection)formatter.Deserialize(stream);
            stream.Close();
            servers.m_strFileName = strFileName;


            return servers;
        }

        // 保存到文件
        // parameters:
        //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
        public void Save(string strFileName)
        {
            if (m_bChanged == false)
                return;

            if (strFileName == null)
                strFileName = m_strFileName;

            if (strFileName == null)
            {
                throw (new Exception("ServerCollection.Save()没有指定保存文件名"));
            }

            Stream stream = File.Open(strFileName,
                FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, this);
            stream.Close();
        }

        public void SetAllVerified(bool bVerified)
        {
            for (int i = 0; i < this.Count; i++)
            {
                dp2Server server = (dp2Server)this[i];
                server.Verified = bVerified;
            }
        }

        /*
        // 获得缺省帐户信息
        // return:
        //		2	already login succeed
        //		1	dialog return OK
        //		0	dialog return Cancel
        //		-1	other error
        public void OnAskAccountInfo(object sender,
            AskAccountInfoEventArgs e)
        {
            bool bFirst = true;

            bool bAutoLogin = (e.LoginStyle & LoginStyle.AutoLogin) == LoginStyle.AutoLogin;
            bool bFillDefault = (e.LoginStyle & LoginStyle.FillDefaultInfo) == LoginStyle.FillDefaultInfo;

            e.Owner = this.ownerForm;
            e.UserName = "";
            e.Password = "";

            LoginDlg dlg = new LoginDlg();

            Server server = this[e.Url];

            dlg.textBox_serverAddr.Text = e.Url;
            if (bFillDefault == true)
            {
                if (server != null)
                {
                    dlg.textBox_userName.Text = (server.DefaultUserName == "" ? "public" : server.DefaultUserName);
                    dlg.textBox_password.Text = server.DefaultPassword;
                    dlg.checkBox_savePassword.Checked = server.SavePassword;
                }
                else
                {
                    dlg.textBox_userName.Text = "public";
                    dlg.textBox_password.Text = "";
                    dlg.checkBox_savePassword.Checked = false;
                }
            }

            if (e.Comment != null)
                dlg.textBox_comment.Text = e.Comment;

        DOLOGIN:
            if (e.Channels != null)
            {
                if (bAutoLogin == false && bFirst == true)
                    goto REDOINPUT;

                // 找到Channel
                RmsChannel channel = e.Channels.GetChannel(dlg.textBox_serverAddr.Text);

                Debug.Assert(channel != null, "Channels.GetChannel()异常...");


                string strError;
                // 登录
                int nRet = channel.Login(dlg.textBox_userName.Text,
                    dlg.textBox_password.Text,
                    out strError);

                if (nRet != 1)
                {
                    strError = "以用户名 '" + dlg.textBox_userName.Text + "' 登录到 '" + dlg.textBox_serverAddr.Text + "' 失败: " + strError;

                    if (this.ownerForm != null)
                    {
                        MessageBox.Show(this.ownerForm, strError);
                    }
                    else
                    {
                        e.ErrorInfo = strError;
                        e.Result = -1;
                    }

                    goto REDOINPUT;
                }
                else // 登录成功
                {
                    if (String.Compare(e.Url, dlg.textBox_serverAddr.Text, true) != 0)
                    {
                        // 创建一个新的Server对象
                        // return:
                        //		-1	出错
                        //		0	加入了
                        //		1	发现重复，没有加入
                        nRet = this.NewServer(dlg.textBox_serverAddr.Text, -1);
                        if (nRet == 0)
                            e.Url = channel.Url;
                    }

                    server = this[dlg.textBox_serverAddr.Text];

                    if (server == null) // 2006/8/19 add
                    {
                        // 创建一个新的Server对象
                        // return:
                        //		-1	出错
                        //		0	加入了
                        //		1	发现重复，没有加入
                        nRet = this.NewServer(dlg.textBox_serverAddr.Text, -1);
                        if (nRet == 0)
                            e.Url = channel.Url;

                        server = this[dlg.textBox_serverAddr.Text];

                    }

                    Debug.Assert(server != null, "此时server不可能为null");

                    server.DefaultUserName = dlg.textBox_userName.Text;
                    server.DefaultPassword = dlg.textBox_password.Text;
                    server.SavePassword = dlg.checkBox_savePassword.Checked;
                    this.m_bChanged = true;

                    e.Result = 2;
                    return;
                }
            }


        REDOINPUT:
            bFirst = false;

            dlg.ShowDialog(ownerForm);

            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Result = 0;
                return;
            }

            if (e.Channels == null)
            {
                e.UserName = dlg.textBox_userName.Text;
                e.Password = dlg.textBox_password.Text;

                e.Result = 1;
                return;
            }


            goto DOLOGIN;
        }
        */
    }

    // 事件: 增添或者删除了服务器
    public delegate void dp2ServerChangedEventHandle(object sender,
    dp2ServerChangedEventArgs e);

    public class dp2ServerChangedEventArgs : EventArgs
    {
        public string Url = ""; // 服务器URL
        public dp2ServerChangeAction ServerChangeAction = dp2ServerChangeAction.None; // 所发生的改变类型
    }

    public enum dp2ServerChangeAction
    {
        None = 0,
        Add = 1,
        Remove = 2,
        Import = 3,
    }
}
