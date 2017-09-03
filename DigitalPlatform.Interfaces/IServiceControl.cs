using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Interfaces
{
    /// <summary>
    /// 控制 Windows Service 模块的命令接口
    /// </summary>
    public interface IServiceControl
    {
        // 启动一个 Instance
        ServiceControlResult StartInstance(string strInstanceName);

        // 停止一个 Instance
        ServiceControlResult StopInstance(string strInstanceName);

        // 获得一个 Instance 的信息
        ServiceControlResult GetInstanceInfo(string strInstanceName, out InstanceInfo info);

    }

    [Serializable()]
    public class ServiceControlResult
    {
        public string ErrorInfo { get; set; }
        public int Value { get; set; }
        public string ErrorCode { get; set; }
    }

    [Serializable()]
    public class InstanceInfo
    {
        public string InstanceName { get; set; }
        public string State { get; set; }   // running / stopped
    }
}
