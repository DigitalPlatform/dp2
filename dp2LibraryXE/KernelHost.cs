using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
// using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;

using System.Security.Cryptography.X509Certificates;
// using System.IdentityModel.Selectors;

using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;

using dp2Kernel;
using DigitalPlatform;

namespace dp2LibraryXE
{
    public class HostBase : IDisposable
    {
        internal ServiceHost _host = null;

        public string DataDir = "";
        public string ErrorInfo = "";
        internal bool _running = true;
        internal Thread _thread = null;
        internal AutoResetEvent _eventStarted = new AutoResetEvent(false);
        internal AutoResetEvent _eventClosed = new AutoResetEvent(false);

        public string MetadataUrl = "";

        public void Dispose()
        {
            CloseHosts();

            _eventClosed.Dispose();
            _eventStarted.Dispose();
        }

        public int Start(out string strError)
        {
            strError = "";

            Debug.Assert(_thread == null, "");
            _thread = new Thread(new ThreadStart(ThreadMethod));
            _thread.Start();

            // 等待，直到完全启动
            WaitHandle[] events = new WaitHandle[2];

            events[0] = _eventClosed;
            events[1] = _eventStarted;
            int index = WaitHandle.WaitAny(events, -1, false);
#if NO
            if (index == WaitHandle.WaitTimeout)
                return;
            if (index == 0)
                return;
#endif
            if (string.IsNullOrEmpty(this.ErrorInfo) == false)
            {
                strError = this.ErrorInfo;
                return -1;
            }

            return 0;
        }

        public virtual void CloseHosts()
        {
        }

        public virtual void Stop()
        {
            lock (this)
            {
                this._running = false;
            }

            // 2014/12/3
            // CloseHosts();

            // 等待 CloseHosts() 结束
            while (this._thread != null)
            {
                Thread.Sleep(100);
            }
        }

        public virtual void ThreadMethod()
        {
        }
    }

    public class KernelHost : HostBase
    {
        public override void ThreadMethod()
        {
            string strError = "";

            _running = true;

            int nRet = Start(this.DataDir, out strError);
            if (nRet == -1)
            {
                this.ErrorInfo = strError;
                this._host = null; 
            }

            this._eventStarted.Set();

            while (_running)
            {
                Thread.Sleep(100);
            }

            this.CloseHosts();
            this._thread = null;
            this._eventClosed.Set();
        }

        public static string ListenUrl = "net.pipe://localhost/dp2kernel/xe";

        int Start(string strDataDir,
            out string strError)
        {
            strError = "";

            CloseHosts();

            string strInstanceName = "";
            // string strUrl = "net.pipe://localhost/dp2kernel/xe";

            _host = new ServiceHost(typeof(KernelService));

            HostInfo info = new HostInfo();
            info.DataDir = strDataDir;
            _host.Extensions.Add(info);
            /// 

            // 绑定协议
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(ListenUrl);
                }
                catch (Exception ex)
                {
                    strError = "dp2Kernel OnStart() 警告：发现不正确的协议URL '" + ListenUrl + "' (异常信息: " + ex.Message + ")。该URL已被放弃绑定。";
                    return -1;
                }

                if (uri.Scheme.ToLower() == "net.pipe")
                {
                    _host.AddServiceEndpoint(typeof(IKernelService),
            CreateNamedpipeBinding0(),
            ListenUrl);
                }
                else
                {
                    // 警告不能支持的协议
                    strError = "dp2Kernel OnStart() 警告：发现不能支持的协议类型 '" + ListenUrl + "'";
                    return -1;
                }
            }

#if NO
            {
                string strMetadataUrl = "http://localhost:8001/dp2kernel/xe/";
                if (strMetadataUrl[strMetadataUrl.Length - 1] != '/')
                    strMetadataUrl += "/";
                strMetadataUrl += "metadata";

                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                behavior.HttpGetEnabled = true;
                behavior.HttpGetUrl = new Uri(strMetadataUrl);
                _host.Description.Behaviors.Add(behavior);

                this.MetadataUrl = strMetadataUrl;
            }
#endif

            if (_host.Description.Behaviors.Find<ServiceThrottlingBehavior>() == null)
            {
                ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior();
                behavior.MaxConcurrentCalls = 50;
                behavior.MaxConcurrentInstances = 1000;
                behavior.MaxConcurrentSessions = 1000;
                _host.Description.Behaviors.Add(behavior);
            }

            // IncludeExceptionDetailInFaults
            ServiceDebugBehavior debug_behavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debug_behavior == null)
            {
                _host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
            }
            else
            {
                if (debug_behavior.IncludeExceptionDetailInFaults == false)
                    debug_behavior.IncludeExceptionDetailInFaults = true;
            }

            _host.Opening += new EventHandler(host_Opening);
            _host.Closing += new EventHandler(m_host_Closing);

            try
            {
                _host.Open();
            }
            catch (Exception ex)
            {
                strError = "dp2Kernel OnStart() host.Open() 时发生错误: instancename=[" + strInstanceName + "]:" + ex.Message;
                return -1;
            }

            return 0;
        }

        // np0: namedpipe 
        System.ServiceModel.Channels.Binding CreateNamedpipeBinding0()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.Namespace = "http://dp2003.com/dp2kernel/";
            binding.Security.Mode = NetNamedPipeSecurityMode.None;

            binding.MaxReceivedMessageSize = 1024 * 1024;
            // binding.MessageEncoding = WSMessageEncoding.Mtom;
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxArrayLength = 1024 * 1024;
            quotas.MaxStringContentLength = 1024 * 1024;
            binding.ReaderQuotas = quotas;
            binding.SendTimeout = new TimeSpan(0, 20, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 20, 0);
            // binding.ReliableSession.Enabled = false;

            return binding;
        }

        void m_host_Closing(object sender, EventArgs e)
        {
            if (this._host != null)
            {
                HostInfo info = _host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    _host.Extensions.Remove(info);
                    info.Dispose();
                }
            }
        }

        void host_Opening(object sender, EventArgs e)
        {

        }

        public override void CloseHosts()
        {
            if (_host != null)
            {
                HostInfo info = _host.Extensions.Find<HostInfo>();
                if (info != null)
                {
                    _host.Extensions.Remove(info);
                    info.Dispose();
                }

                _host.Close();
                _host = null;
            }
        }
    }
}
