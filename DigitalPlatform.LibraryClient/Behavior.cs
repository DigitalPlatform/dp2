using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace DigitalPlatform.LibraryClient
{
    public delegate IDictionary<string, string> delegate_getHeaders();

    // http://blogs.msmvps.com/paulomorgado/2007/04/26/wcf-building-an-http-user-agent-message-inspector/
    public class HttpUserAgentMessageInspector : IClientMessageInspector
    {
#if NO
        private const string USER_AGENT_HTTP_HEADER = "user-agent";
        private const string TIMEOUT_HTTP_HEADER = "_timeout";

        private string m_userAgent;
        private string m_timeout;

        public HttpUserAgentMessageInspector(string userAgent, string timeout)
        {
            this.m_userAgent = userAgent;
            this.m_timeout = timeout;
        }
#endif
        private delegate_getHeaders _proc = null;

        public HttpUserAgentMessageInspector(delegate_getHeaders proc)
        {
            _proc = proc;
        }

        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }

#if NO
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[USER_AGENT_HTTP_HEADER]))
                    httpRequestMessage.Headers[USER_AGENT_HTTP_HEADER] = this.m_userAgent;
                if (this.m_timeout != null)
                {
                    if (string.IsNullOrEmpty(httpRequestMessage.Headers[TIMEOUT_HTTP_HEADER]))
                        httpRequestMessage.Headers[TIMEOUT_HTTP_HEADER] = this.m_timeout;
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(USER_AGENT_HTTP_HEADER, this.m_userAgent);
                if (this.m_timeout != null)
                    httpRequestMessage.Headers.Add(TIMEOUT_HTTP_HEADER, this.m_timeout);
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
#endif

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
        {
            if (_proc == null)
                return null;

            IDictionary<string, string> headers = _proc();

            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                foreach(string key in headers.Keys)
                {
                    httpRequestMessage.Headers.Set(key, headers[key]);
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                foreach (string key in headers.Keys)
                {
                    httpRequestMessage.Headers.Set(key, headers[key]);
                }
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }

        #endregion
    }

    public class HttpUserAgentEndpointBehavior : IEndpointBehavior
    {
#if NO
        private string m_userAgent;
        private string m_timeout;

        public HttpUserAgentEndpointBehavior(string userAgent, string timeout)
        {
            this.m_userAgent = userAgent;
            // this.m_timeout = timeout;
        }

        public string Timeout
        {
            get
            {
                if (m_timeout == null)
                    return "";
                return m_timeout.ToString();
            }
            set
            {
                // m_timeout = value;
            }
        }
#endif
        private delegate_getHeaders _proc = null;

        public HttpUserAgentEndpointBehavior(delegate_getHeaders proc)
        {
            this._proc = proc;
            // this.m_timeout = timeout;
        }

        #region IEndpointBehavior Members

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            HttpUserAgentMessageInspector inspector = new HttpUserAgentMessageInspector(_proc);
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
