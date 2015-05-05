using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Text;

namespace dp2Installer
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

    /// <summary>
    /// 实做注册/注销 Windows Service 的类
    /// </summary>
    public class InstallServiceWork : MarshalByRefObject
    {
        public string InstallService(Parameters parameters)
        {
            // 准备 rootdir 参数，以便兼容以前的 Installer 模块功能
            string strRootDir = Path.GetDirectoryName(parameters.ExePath);
            string strRootDirParam = "/rootdir='" + strRootDir + "'";
            try
            {
                if (parameters.Install == true)
                    ManagedInstallerClass.InstallHelper(new[] {  strRootDirParam, parameters.ExePath });
                else
                    ManagedInstallerClass.InstallHelper(new[] { "/u", strRootDirParam, parameters.ExePath  });
            }
            catch (Exception ex)
            {
                // TODO: 要能够看到 InnerException
                if (parameters.Install == true)
                    return "注册 Windows Service 的过程发生错误: " + ex.Message + (ex.InnerException == null ? "" : " " + ex.InnerException.Message);
                else
                    return "注销 Windows Service 的过程发生错误: " + ex.Message + (ex.InnerException == null ? "" : " " + ex.InnerException.Message);
            }

            return null;
        }
    } 
 
}
