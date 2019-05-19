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
        // 极限通道数
        public int MAX_COUNT = 1000;    // 1000

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

                if (_idleChannels.Count + _usedChannels.Count > MAX_COUNT)
                    throw new Exception();

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

        public void ClearIdle(delegate_deleteChannel func_delete)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (T channel in _idleChannels)
                {
                    func_delete(channel);
                }
                _idleChannels.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public delegate void delegate_deleteChannel(T channel);

        public void Close(delegate_deleteChannel func_delete)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (T channel in _usedChannels)
                {
                    func_delete(channel);
                }
                _usedChannels.Clear();

                foreach (T channel in _idleChannels)
                {
                    func_delete(channel);
                }
                _idleChannels.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
