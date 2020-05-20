using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using DigitalPlatform.Text;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 2020/5/20
    /// 新版 dp2 服务器集合类。取代 dp2ServerCollection
    /// </summary>
    public class dp2ServerCollectionNew
    {
        List<dp2Server> _servers = new List<dp2Server>();

        [NonSerialized]
        string m_strFileName = "";

        [NonSerialized]
        bool m_bChanged = false;

        /*
            [NonSerialized]
            public IWin32Window ownerForm = null;
            */

        public event dp2ServerChangedEventHandle ServerChanged = null;


        public dp2ServerCollectionNew()
        {
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [JsonIgnore]
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

        [JsonIgnore]
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

        public dp2Server this[int index]
        {
            get
            {
                return this._servers[index];
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

            foreach (var server in _servers)
            {
                string strCurrentUrl = StringUtil.CanonicalizeHostUrl(server.Url);
                if (String.Compare(strCurrentUrl, strUrl, true) == 0)
                    return server;
            }

            return null;
        }

        // 2007/9/14
        public dp2Server GetServerByName(string strServerName)
        {
            foreach (var server in _servers)
            {
                if (server.Name == strServerName)
                    return server;
            }

            return null;
        }

        // 根据 UID 查找
        public List<dp2Server> FindServerByUID(string uid)
        {
            List<dp2Server> results = new List<dp2Server>();
            foreach (dp2Server server in _servers)
            {
                if (server.UID == uid)
                    results.Add(server);
            }

            return results;
        }

        // 克隆。
        // 新数组中的对象完全是新创建的。
        public dp2ServerCollectionNew Dup()
        {
            dp2ServerCollectionNew newServers = new dp2ServerCollectionNew();

            foreach (var server in _servers)
            {
                dp2Server newServer = new dp2Server(server);
                newServers.Add(newServer);
            }

            newServers.FileName = this.FileName;
            newServers.Changed = this.Changed;
            // newServers.ownerForm = this.ownerForm;

            return newServers;
        }

        public void Clear()
        {
            _servers.Clear();
            this.m_bChanged = true;
        }

        public void Add(dp2Server server)
        {
            _servers.Add(server);
            this.m_bChanged = true;
        }

        public void Insert(int pos, dp2Server server)
        {
            _servers.Insert(pos, server);
            this.m_bChanged = true;
        }

        public void RemoveAt(int pos)
        {
            this._servers.RemoveAt(pos);
            this.m_bChanged = true;
        }

        [JsonIgnore]
        public int Count
        {
            get
            {
                return _servers.Count;
            }
        }

        public List<dp2Server> Servers
        {
            get
            {
                return _servers;
            }
        }

        // 将另一对象的数组内容灌入本对象
        public void Import(dp2ServerCollectionNew servers)
        {
            this.Clear();
            this._servers.AddRange(servers.Servers);
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
        public static dp2ServerCollectionNew Load(
            string strFileName,
            bool bIgnoreFileNotFound)
        {
            try
            {
                lock (_syncRoot_file)
                {
                    string value = File.ReadAllText(strFileName);
                    var servers = JsonConvert.DeserializeObject<dp2ServerCollectionNew>(value);
                    if (servers == null)
                        servers = new dp2ServerCollectionNew();
                    if (servers._servers == null)
                        servers._servers = new List<dp2Server>();

                    servers.m_strFileName = strFileName;
                    return servers;
                }
            }
            catch (FileNotFoundException ex)
            {
                if (bIgnoreFileNotFound == false)
                    throw ex;

                var servers = new dp2ServerCollectionNew();
                servers.m_strFileName = strFileName;

                // 让调主有一个新的空对象可用
                return servers;
            }
        }

        static object _syncRoot_file = new object();

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

            lock (_syncRoot_file)
            {
                string value = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(strFileName, value);
            }
        }

        public void SetAllVerified(bool bVerified)
        {
            foreach (var server in _servers)
            {
                server.Verified = bVerified;
            }
        }
    }
}
