using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Interfaces
{
    public interface ISipMessageFilter
    {
        // parameters:
        //      type    消息的类型。"request" "response" 之一
        string TriggerScript(string type,
    ref string message,
    ScriptContext context);

    }


    public class ScriptContext : Hashtable
    {
        public ScriptContext() : base()
        {

        }

        public object Get(object key, object default_value)
        {
            if (this.ContainsKey(key) == false)
                return default_value;
            return this[key];
        }

        public object Get(object key)
        {
            return this[key];
        }

        public void Set(object key, object value)
        {
            this[key] = value;
        }

        // 注: Hashtable 本来有 Remove(key) 函数
    }

}
