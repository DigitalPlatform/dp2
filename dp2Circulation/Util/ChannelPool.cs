using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace dp2Circulation
{
    /// <summary>
    /// 通用的通道复用管理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelPool<T>
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        List<T> _idleChannels = new List<T>();
        List<T> _usedChannels = new List<T>();

        public delegate T delegate_newChannel();

        public T GetChannel(delegate_newChannel func_new)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_idleChannels.Count > 0)
                {
                    var result = _idleChannels[0];
                    _idleChannels.RemoveAt(0);
                    _usedChannels.Add(result);
                    return result;
                }

                {
                    var new_channel = func_new();
                    _usedChannels.Add(new_channel);
                    return new_channel;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ReturnChannel(T channel)
        {
            _lock.EnterWriteLock();
            try
            {
                _usedChannels.Remove(channel);
                _idleChannels.Add(channel);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ClearIdle()
        {
            _lock.EnterWriteLock();
            try
            {
                _idleChannels.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Close()
        {
            _lock.EnterWriteLock();
            try
            {
                _usedChannels.Clear();
                _idleChannels.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
