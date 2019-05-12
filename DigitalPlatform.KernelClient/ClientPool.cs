using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.KernelClient.KernelServiceReference;

namespace DigitalPlatform.KernelClient
{
    // 
    /// <summary>
    /// 通道池
    /// </summary>
    public class ClientPool : List<ClientWrapper>
    {
        /// <summary>
        /// 登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;

        /// <summary>
        /// 登录后事件
        /// </summary>
        public event AfterLoginEventHandle AfterLogin;

        /// <summary>
        /// 最多通道数
        /// </summary>
        public int MaxCount = 50;

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        /// <summary>
        /// 征用一个通道
        /// </summary>
        /// <param name="strUrl">服务器 URL</param>
        /// <param name="strUserName">用户名</param>
        /// <param name="strLang">语言代码。如果为空，表示不在意通道的语言代码</param>
        /// <returns>返回通道对象</returns>
        public Client GetChannel(string strUrl,
            string strUserName,
            string strLang = null,
            string strClientIP = null)
        {
            ClientWrapper wrapper = null;

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                wrapper = this._findChannel(strUrl, strUserName, strLang, true);

                if (wrapper != null)
                    return wrapper.Channel;

                if (this.Count >= MaxCount)
                {
                    // 清理不用的通道
                    int nDeleteCount = _cleanChannel(false);
                    if (nDeleteCount == 0)
                    {
                        // 全部都在使用
                        throw new Exception("通道池已满，请稍后重试获取通道");
                    }
                }

                // 如果没有找到
                Client inner_channel = new Client();
                inner_channel.Url = strUrl;
                inner_channel.UserName = strUserName;
                if (strLang != null)
                    inner_channel.Lang = strLang;
                if (strClientIP != null)
                    inner_channel.ClientIP = strClientIP;

                // test
                // inner_channel.ClientIP = "test:127.0.0.1";


                inner_channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                inner_channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                inner_channel.AfterLogin -= inner_channel_AfterLogin;
                inner_channel.AfterLogin += inner_channel_AfterLogin;

                wrapper = new ClientWrapper();
                wrapper.Channel = inner_channel;
                wrapper.InUsing = true;

                this.Add(wrapper);
                return inner_channel;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        void inner_channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            if (this.AfterLogin != null)
                this.AfterLogin(sender, e);
        }

        void channel_BeforeLogin(object sender,
    BeforeLoginEventArgs e)
        {
            if (this.BeforeLogin != null)
                this.BeforeLogin(sender, e);
        }

        // 查找指定URL的LibraryChannel对象
        ClientWrapper _findChannel(string strUrl,
            string strUserName,
            string strLang,
            bool bAutoSetUsing)
        {
            foreach (ClientWrapper wrapper in this)
            {
                if (wrapper.InUsing == false
                    && wrapper.Channel.Url == strUrl
                    && (string.IsNullOrEmpty(wrapper.Channel.UserName) == true
                    || wrapper.Channel.UserName == strUserName)
                    && (string.IsNullOrEmpty(strLang) == true
                    || wrapper.Channel.Lang == strLang)
                    )
                {
                    if (bAutoSetUsing == true)
                        wrapper.InUsing = true;
                    return wrapper;
                }
            }

            return null;
        }

        // 查找指定URL的LibraryChannel对象
        ClientWrapper _findChannel(Client inner_channel)
        {
            foreach (ClientWrapper channel in this)
            {
                if (channel.Channel == inner_channel)
                {
                    return channel;
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 归还一个通道
        /// </summary>
        /// <param name="channel">通道对象</param>
        public void ReturnChannel(Client channel)
        {
            ClientWrapper wrapper = null;
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                wrapper = _findChannel(channel);
                if (wrapper != null)
                    wrapper.InUsing = false;
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        public int CleanChannel(string strUserName = "")
        {
            return _cleanChannel(true, strUserName);
        }

        // 清理处在未使用状态的通道
        // parameters:
        //      strUserName 希望清除用户名为此值的全部通道。如果本参数值为空，则表示清除全部通道
        // return:
        //      清理掉的通道数目
        int _cleanChannel(bool bLock, string strUserName = "")
        {
            List<ClientWrapper> deletes = new List<ClientWrapper>();

            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    ClientWrapper wrapper = this[i];
                    if (wrapper.InUsing == false
                        && (string.IsNullOrEmpty(strUserName) == true || wrapper.Channel.UserName == strUserName)
                        )
                    {
                        this.RemoveAt(i);
                        i--;
                        deletes.Add(wrapper);
                    }
                }
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ExitWriteLock();
            }

            foreach (ClientWrapper wrapper in deletes)
            {
                wrapper.Channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                wrapper.Channel.AfterLogin -= inner_channel_AfterLogin;
                wrapper.Channel.Close();
            }

            return deletes.Count;
        }

        /// <summary>
        /// 关闭所有通道，清除集合
        /// </summary>
        public new void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                foreach (ClientWrapper wrapper in this)
                {
                    wrapper.Channel.Close();
                }

                base.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        public void Close()
        {
            this.Clear();
        }
    }

    /// <summary>
    /// 通道包装对象
    /// </summary>
    public class ClientWrapper
    {
        /// <summary>
        /// 通道是否正在使用中
        /// </summary>
        public bool InUsing = false;

        /// <summary>
        /// 通道对象
        /// </summary>
        public Client Channel = null;
    }
}
