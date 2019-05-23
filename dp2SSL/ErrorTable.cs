using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
{
    // 分类报错机制
    public class ErrorTable
    {
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint
        Hashtable _globalErrorTable = new Hashtable();

        public delegate void delegate_setError(string text);

        delegate_setError _setError = null;

        public ErrorTable(delegate_setError func)
        {
            _setError = func;
        }

        // 设置全局区域错误字符串
        public void SetError(string type, string error)
        {
            if (string.IsNullOrEmpty(error) == false)
                error = error.Replace("\r\n", ";").TrimEnd(new char[] { ';', ' ' });

            _lock.EnterWriteLock();
            try
            {
                _globalErrorTable[type] = error;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            _setError?.Invoke(GetError());
        }

        // 合成全局区域错误字符串，用于刷新显示
        public string GetError()
        {
            List<string> errors = new List<string>();

            _lock.EnterReadLock();
            try
            {
                foreach (string type in _globalErrorTable.Keys)
                {
                    string error = _globalErrorTable[type] as string;
                    if (string.IsNullOrEmpty(error) == false)
                        errors.Add(error.Replace("\r\n", "\n").TrimEnd(new char[] { '\n', ' ' }));
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (errors.Count == 0)
                return null;
            if (errors.Count == 1)
                return errors[0];
            int i = 0;
            StringBuilder text = new StringBuilder();
            foreach (string error in errors)
            {
                if (text.Length > 0)
                    text.Append("\n");
                text.Append($"{i + 1}) {error}");
                i++;
            }
            return text.ToString();
        }
    }
}
