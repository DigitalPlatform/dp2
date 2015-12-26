using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Install
{
    /// <summary>
    /// 在新的 AppDomain 中执行代码
    /// http://www.superstarcoders.com/blogs/posts/executing-code-in-a-separate-application-domain-using-c-sharp.aspx
    /// Executing Code in a Separate Application Domain Using C#
    /// </summary>
    /// <typeparam name="T">要包裹的工作类型</typeparam>
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;
        private T _value;

        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
               null, AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);

                _domain = null;
            }
        }
    }

    /// <summary>
    /// 参数
    /// </summary>
    [Serializable]
    public class Parameters
    {
        /// <summary>
        /// exe 文件的全路径
        /// </summary>
        public string ExePath = "";

        /// <summary>
        /// 是否为注册。false 表示注销
        /// </summary>
        public bool Install = true;
    }
}
