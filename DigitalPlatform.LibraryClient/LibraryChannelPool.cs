using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DigitalPlatform.LibraryClient
{
    // 
    /// <summary>
    /// 通道池
    /// </summary>
    public class LibraryChannelPool : List<LibraryChannelWrapper>
    {
        /// <summary>
        /// 登录前事件
        /// </summary>
        public event BeforeLoginEventHandle BeforeLogin;

        /// <summary>
        /// 登录后事件
        /// </summary>
        public event AfterLoginEventHandle AfterLogin;

        // 2021/11/6
        /// <summary>
        /// 请求重设密码事件
        /// </summary>
        public event RequestPasswordEventHandler RequestPasswordEvent = null;

        // 2021/11/6
        /// <summary>
        /// 密码变化通知事件
        /// </summary>
        public event PasswordChangedEventHandler PasswordChangedEvent = null;


        /// <summary>
        /// 最多通道数
        /// </summary>
        public int MaxCount = 50;   // 50

        internal ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        // 
        /// <summary>
        /// 征用一个通道
        /// </summary>
        /// <param name="strUrl">服务器 URL</param>
        /// <param name="strUserName">用户名</param>
        /// <param name="strLang">语言代码。如果为空，表示不在意通道的语言代码</param>
        /// <returns>返回通道对象</returns>
        public LibraryChannel GetChannel(string strUrl,
            string strUserName,
            string strLang = null,
            string strClientIP = null)
        {
            LibraryChannelWrapper wrapper = null;

            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                wrapper = this._findChannel(strUrl, strUserName, strLang, true);
                if (wrapper != null)
                    return wrapper.Channel;

                // 如果没有找到
                LibraryChannel inner_channel = new LibraryChannel();
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

                // 2021/11/6
                inner_channel.RequestPasswordEvent -= Inner_channel_RequestPasswordEvent;
                inner_channel.RequestPasswordEvent += Inner_channel_RequestPasswordEvent;

                inner_channel.PasswordChangedEvent -= Inner_channel_PasswordChangedEvent;
                inner_channel.PasswordChangedEvent += Inner_channel_PasswordChangedEvent;

                wrapper = new LibraryChannelWrapper();
                wrapper.Channel = inner_channel;
                wrapper.InUsing = true;

                if (this.Count >= MaxCount)
                {
                    // TODO: 这一个部分放在写锁定范围内，是否会导致其他请求排队超时？
                    // 清理不用的通道
                    int nDeleteCount = _cleanChannel(false,
                        (channel) => { return true; });
                    if (nDeleteCount == 0)
                    {
                        inner_channel?.Dispose();
                        // 全部都在使用
                        throw new Exception("通道池已满，请稍后重试获取通道");
                    }
                }

                this.Add(wrapper);
                return inner_channel;
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        private void Inner_channel_PasswordChangedEvent(object sender, PasswordChangedEventArgs e)
        {
            PasswordChangedEvent?.Invoke(sender, e);
        }

        private void Inner_channel_RequestPasswordEvent(object sender, RequestPasswordEventArgs e)
        {
            RequestPasswordEvent?.Invoke(sender, e);
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
        LibraryChannelWrapper _findChannel(string strUrl,
            string strUserName,
            string strLang,
            bool bAutoSetUsing)
        {
            foreach (LibraryChannelWrapper wrapper in this)
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
        LibraryChannelWrapper _findChannel(LibraryChannel inner_channel)
        {
            foreach (LibraryChannelWrapper channel in this)
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
        public void ReturnChannel(LibraryChannel channel)
        {
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                LibraryChannelWrapper wrapper = _findChannel(channel);
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
            // return _cleanChannel(true, strUserName);
            return _cleanChannel(true, (channel) => string.IsNullOrEmpty(strUserName) == true || channel.UserName == strUserName);
        }

        public delegate bool Delegate_needClean(LibraryChannel channel);

        public int CleanChannel(Delegate_needClean func_needClean)
        {
            return _cleanChannel(true, func_needClean);
        }

#if NO
        // 清理处在未使用状态的通道
        // parameters:
        //      strUserName 希望清除用户名为此值的全部通道。如果本参数值为空，则表示清除全部通道
        // return:
        //      清理掉的通道数目
        int _cleanChannel(bool bLock, string strUserName = "")
        {
            List<LibraryChannelWrapper> deletes = new List<LibraryChannelWrapper>();

            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    LibraryChannelWrapper wrapper = this[i];
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

            foreach (LibraryChannelWrapper wrapper in deletes)
            {
                wrapper.Channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                wrapper.Channel.AfterLogin -= inner_channel_AfterLogin;
                wrapper.Channel.Close();
            }

            return deletes.Count;
        }
#endif


        // return:
        //      清理掉的通道数目
        int _cleanChannel(bool bLock, Delegate_needClean func_needClean)
        {
            List<LibraryChannelWrapper> deletes = new List<LibraryChannelWrapper>();

            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    LibraryChannelWrapper wrapper = this[i];
                    if (wrapper.InUsing == false
                        && (func_needClean == null || func_needClean(wrapper.Channel) == true))
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

            foreach (LibraryChannelWrapper wrapper in deletes)
            {
                wrapper.Channel.BeforeLogin -= new BeforeLoginEventHandle(channel_BeforeLogin);
                wrapper.Channel.AfterLogin -= inner_channel_AfterLogin;
                wrapper.Channel.RequestPasswordEvent -= Inner_channel_RequestPasswordEvent;
                wrapper.Channel.PasswordChangedEvent -= Inner_channel_PasswordChangedEvent;
                wrapper.Channel.Close();
            }

            return deletes.Count;
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
内容 发生未捕获的界面线程异常: 
Type: System.Threading.LockRecursionException
Message: 此模式不下允许以递归方式获取写入锁定。
Stack:
在 System.Threading.ReaderWriterLockSlim.TryEnterWriteLockCore(TimeoutTracker timeout)
在 System.Threading.ReaderWriterLockSlim.TryEnterWriteLock(TimeoutTracker timeout)
在 System.Threading.ReaderWriterLockSlim.TryEnterWriteLock(Int32 millisecondsTimeout)
在 DigitalPlatform.LibraryClient.LibraryChannelPool.Clear()
在 DigitalPlatform.LibraryClient.LibraryChannelPool.Close()
在 dp2Circulation.MainForm.MainForm_FormClosed(Object sender, FormClosedEventArgs e)
在 System.Windows.Forms.Form.OnFormClosed(FormClosedEventArgs e)
在 System.Windows.Forms.Form.WmClose(Message& m)
在 dp2Circulation.MainForm.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.30.6550.17227, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.2.9200.0
操作时间 2017/12/10 10:24:59 (Sun, 10 Dec 2017 10:24:59 +0800) 
         * */
        /// <summary>
        /// 关闭所有通道，清除集合
        /// </summary>
        public new void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                foreach (LibraryChannelWrapper wrapper in this)
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
    public class LibraryChannelWrapper
    {
        // 
        /// <summary>
        /// 通道是否正在使用中
        /// </summary>
        public bool InUsing = false;
        /// <summary>
        /// 通道对象
        /// </summary>
        public LibraryChannel Channel = null;
    }

    /// <summary>
    /// 获取通道的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetChannelEventHandler(object sender,
    GetChannelEventArgs e);

    /// <summary>
    /// 获取通道事件的参数
    /// </summary>
    public class GetChannelEventArgs : EventArgs
    {
        // public bool BeginLoop = false;  // [in]
        public string Style = null; // [in] beginLoop 表示需要调用 BeginLoop，并返回 Looping
        public LibraryChannel Channel = null;   // [out]
        public Looping Looping = null;  // [out]
        public string ErrorInfo = "";   // [out]
    }

    /// <summary>
    /// 归还通道的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ReturnChannelEventHandler(object sender,
    ReturnChannelEventArgs e);

    /// <summary>
    /// 归还通道事件的参数
    /// </summary>
    public class ReturnChannelEventArgs : EventArgs
    {
        public LibraryChannel Channel = null;   // [in]
        public Looping Looping = null;  // [in]
        // public bool EndLoop = false;    // [in]
    }

}
