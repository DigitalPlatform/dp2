using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;

using DigitalPlatform.Interfaces;
using DigitalPlatform.Text;

namespace dp2Library
{
    public class ServiceControlServer : MarshalByRefObject, IServiceControl, IDisposable
    {
        // 启动一个 Instance
        public ServiceControlResult StartInstance(string strInstanceName)
        {
            ServiceControlResult result = new ServiceControlResult();

            try
            {
                List<string> errors = null;
                int nCount = ServerInfo.Host.OpenHosts(new List<string>() { strInstanceName },
                    out errors);
                if (errors != null && errors.Count > 0)
                {
                    result.ErrorInfo = StringUtil.MakePathList(errors);
                    result.Value = -1;
                }

                return result;
            }
            catch(Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "StartInstance() 出现异常: " + ex.Message;
                return result;
            }
        }

        // 停止一个 Instance
        public ServiceControlResult StopInstance(string strInstanceName)
        {
            ServiceControlResult result = new ServiceControlResult();

            try
            {
                bool bRet = ServerInfo.Host.CloseHost(strInstanceName);
                if (bRet == true)
                    result.Value = 1;   // 本次停止了它
                else
                {
                    result.Value = 0;   // 本来就是停止状态
                    //result.ErrorInfo = "实例名 '" + strInstanceName + "' 没有找到";
                    //result.Value = -1;
                }
                return result;
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "StopInstance() 出现异常: " + ex.Message;
                return result;
            }
        }

        // 获得一个实例的信息
        public ServiceControlResult GetInstanceInfo(string strInstanceName,
            out InstanceInfo info)
        {
            info = null;
            ServiceControlResult result = new ServiceControlResult();

            try
            {
                if (strInstanceName == ".")
                {
                    info = new InstanceInfo();
                    info.InstanceName = strInstanceName;
                    info.State = "running";
                    result.Value = 1;   // 表示 dp2library 正在运行状态
                    return result;
                }

                ServiceHost host = ServerInfo.Host.FindHost(strInstanceName);
                if (host == null)
                {
                    result.Value = 0;
                    return result;
                }

                HostInfo host_info = host.Extensions.Find<HostInfo>();
                if (host_info == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "host 中没有找到 HostInfo ";
                    return result;
                }

                info = new InstanceInfo();
                info.InstanceName = host_info.InstanceName;
                info.State = "running";
                result.Value = 1;
                return result;
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorInfo = "GetInstanceInfo() 出现异常: " + ex.Message;
                return result;
            }
        }

        public void Dispose()
        {

        }
    }
}
