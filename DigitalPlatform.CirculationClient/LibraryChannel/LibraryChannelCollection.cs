using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// dp2Library 的前端实用库。目前被 dp2Circulatio / dp2Catalog /dp2OPAC 所调用
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // LibraryChannel对象集合
    // URL是对象的唯一性标志
    /// <summary>
    /// 通讯通道集合。也就是 LibraryChannel 对象的集合
    /// </summary>
    public class LibraryChannelCollection : List<LibraryChannel>, IDisposable
    {
        /// <summary>
        /// 登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;

        /// <summary>
        /// 登录后事件
        /// </summary>
        public event AfterLoginEventHandle AfterLogin;

        public void Dispose()
        {
            this.Close();
            BeforeLogin = null;
            AfterLogin = null;
        }

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        // 创建一个新的LibraryChannel对象，而不管以前有没有相同URL的对象。
        /// <summary>
        /// 创建一个新的 LibraryChannel 对象，不管当前集合中以前有没有相同 URL 的对象。
        /// </summary>
        /// <param name="strUrl">dp2Library 服务器的 URL</param>
        /// <returns>新创建的 LibraryChannel 对象</returns>
        public LibraryChannel NewChannel(string strUrl)
        {
            LibraryChannel channel = new LibraryChannel();
            channel.Url = strUrl;
            channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
            channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

            channel.AfterLogin -= new AfterLoginEventHandle(channel_AfterLogin);
            channel.AfterLogin += new AfterLoginEventHandle(channel_AfterLogin);

            this.m_lock.EnterWriteLock();
            try
            {
                this.Add(channel);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            return channel;
        }

        // 2015/1/1
        void channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            // 直接转发给容器
            if (this.AfterLogin != null)
                this.AfterLogin(sender, e);
        }

        // 删除一个channel
        /// <summary>
        /// 删除当前集合中的一个 LibraryChannel 对象
        /// </summary>
        /// <param name="channel">要删除的 LibraryChannel 对象</param>
        public void RemoveChannel(LibraryChannel channel)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                int index = this.IndexOf(channel);
                if (index == -1)
                    return;

                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.AfterLogin -= new AfterLoginEventHandle(channel_AfterLogin);
                this.Remove(channel);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 获得和URL相关的一个LibraryChannel对象。
        // 如果对象不存在，则自动创建一个
        /// <summary>
        /// 获得和指定 URL 相关的一个 LibraryChannel 对象。如果对象不存在，则自动创建一个，并加入集合
        /// </summary>
        /// <param name="strUrl">dp2Library 服务器的 URL</param>
        /// <returns>LibraryChannel 对象</returns>
        public LibraryChannel GetChannel(string strUrl)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                LibraryChannel channel = this._findChannel(strUrl);

                if (channel != null)
                    return channel;

                // 如果没有找到
                channel = new LibraryChannel();
                channel.Url = strUrl;
                channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                channel.BeforeLogin += new BeforeLoginEventHandle(channel_BeforeLogin);

                channel.AfterLogin -= new AfterLoginEventHandle(channel_AfterLogin);
                channel.AfterLogin += new AfterLoginEventHandle(channel_AfterLogin);

                this.Add(channel);
                return channel;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        void channel_BeforeLogin(object sender,
            BeforeLoginEventArgs e)
        {
            // 直接转发给容器
            if (this.BeforeLogin != null)
                this.BeforeLogin(sender, e);
        }

        // 查找指定URL的LibraryChannel对象
        /*public*/ LibraryChannel _findChannel(string strUrl)
        {
            foreach (LibraryChannel channel in this)
            {
                if (channel.Url == strUrl)
                    return channel;
            }

            return null;
        }

        /// <summary>
        /// 查找指定 URL 的 LibraryChannel 对象
        /// </summary>
        /// <param name="strUrl">dp2Library 服务器的 URL</param>
        /// <returns>LibraryChannel 对象</returns>
        public LibraryChannel FindChannel(string strUrl)
        {
            this.m_lock.EnterReadLock();
            try
            {
                return _findChannel(strUrl);
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 从当前集合中关闭所有通讯通道，删除所有通讯通道对象
        /// </summary>
        public void Close()
        {
            this.Clear(true);
        }

        /// <summary>
        /// 清除全部通道对象
        /// </summary>
        /// <param name="bClose">清除前是否关闭通道对象</param>
        public void Clear(bool bClose = true)
        {
            this.m_lock.EnterWriteLock();
            try
            {
                if (bClose == true)
                {
                    foreach (LibraryChannel channel in this)
                    {
                        channel.Close();
                    }
                }
                base.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }
    }
}
