using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
{
    public class DelayAction
    {
        public Task Task { get; set; }
        public CancellationTokenSource Cancel { get; set; }

        public delegate void Delegate_clear();
        public delegate void Delegate_heartBeat(int leftSeconds);

        public static DelayAction Start(
            int leftSeconds,
            Delegate_clear func_clear,
            Delegate_heartBeat func_heartBeat)
        {
            DelayAction result = new DelayAction();
            result.Cancel = new CancellationTokenSource();
            result.Task = Task.Run(() =>
            {
                try
                {
                    var token = result.Cancel.Token;
                    while (token.IsCancellationRequested == false
                    && leftSeconds > 0)
                    {
                        Task.Delay(TimeSpan.FromSeconds(1), token).Wait();
                        func_heartBeat?.Invoke(leftSeconds--);
                    }
                    token.ThrowIfCancellationRequested();
                    // Task.Delay(TimeSpan.FromSeconds(10), token).Wait();
                    func_clear?.Invoke();
                }
                catch
                {
                    return;
                }
                finally
                {
                    func_heartBeat?.Invoke(-1); // 让按钮文字中的倒计时数字消失
                }
            });
            return result;
        }
    }

}
