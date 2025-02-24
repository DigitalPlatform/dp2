using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Interfaces;

namespace dp2SSL.SampleSipMessageFilter
{
    /// <summary>
    /// 示范 ISipMessageFilter 接口的用法
    /// </summary>
    public class SampleSipMessageFilter : ISipMessageFilter
    {
        public string TriggerScript(string type,
            ref string message, 
            ScriptContext context)
        {
            // 在 context 中持久存储一个计数器变量
            var counter_value = Convert.ToInt32(context.Get("counter", 0));
            counter_value++;
            context.Set("counter", counter_value);

            // 改变 message 消息
            message += $"||| 追加一段文字 type={type} counter={counter_value}";

            return null;    // 表示成功
        }
    }
}
