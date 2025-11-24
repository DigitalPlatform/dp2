using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;

namespace dp2Circulation
{
    // 日志统计窗 扩展的 内置统计方案
    partial class OperLogStatisForm
    {
        public Task<NormalResult> XxxAsync(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                return _xxx(token);
            },
token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // return:
        //      -1  出错
        //      0   成功
        //      1   用户中断
        NormalResult _xxx(CancellationToken token)
        {
            return new NormalResult();
        }
    }
}
