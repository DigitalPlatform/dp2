using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AsyncPluggableProtocol
{
    public interface IProtocol
    {
        string Name { get; }
        Task<Stream> GetStreamAsync(string url);
    }

    public class ProtocolFactory : IClassFactory
    {
        private static IInternetSession GetSession()
        {
            IInternetSession session;
            int res = NativeMethods.CoInternetGetSession(0, out session, 0);

            if (res != NativeConstants.S_OK || session == null)
                throw new InvalidOperationException("CoInternetGetSession failed.");

            return session;
        }

        private Func<IProtocol> _factory;

        private ProtocolFactory(Func<IProtocol> factory)
        {
            _factory = factory;
        }

        public static void Register(string name, Func<IProtocol> factory)
        {
            string emptyStr = null;

            IInternetSession session = GetSession();
            try
            {
                Guid handlerGuid = typeof(Protocol).GUID;
                session.RegisterNameSpace(
                    new ProtocolFactory(factory),
                    ref handlerGuid,
                    name,
                    0,
                    ref emptyStr,
                    0);
            }
            finally
            {
                Marshal.ReleaseComObject(session);
                session = null;
            }
        }

        public void LockServer(bool Lock)
        {
        }

        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            if (pUnkOuter != IntPtr.Zero)
                return NativeConstants.CLASS_E_NOAGGREGATION;

            if (typeof(IInternetProtocol).GUID.Equals(riid)
                || typeof(IInternetProtocolRoot).GUID.Equals(riid)
                || typeof(IInternetProtocolInfo).GUID.Equals(riid))
            {
                object obj = new Protocol(_factory());
                IntPtr objPtr = Marshal.GetIUnknownForObject(obj);
                IntPtr resultPtr;
                Guid refIid = riid;
                Marshal.QueryInterface(objPtr, ref refIid, out resultPtr);
                ppvObject = resultPtr;
                return NativeConstants.S_OK;
            }

            return NativeConstants.E_NOINTERFACE;
        }
    }

    public class Protocol : IInternetProtocol, IInternetProtocolInfo
    {
        private IProtocol _protocol;

        public Protocol(IProtocol protocol)
        {
            _protocol = protocol;
        }

        private byte[] data;
        private int readPos;

        public async void Start(string szUrl, IInternetProtocolSink pOIProtSink, IInternetBindInfo pOIBindInfo, PI_FLAGS grfPI, int dwReserved)
        {
            var bytes = new byte[4096];

            try
            {
                using (var ms = new MemoryStream())
                using (var stream = await _protocol.GetStreamAsync(szUrl))
                {
                    while (true)
                    {
                        var n = await stream.ReadAsync(bytes, 0, bytes.Length);
                        if (n == 0)
                        {
                            data = ms.ToArray();
                            readPos = 0;
                            pOIProtSink.ReportData(BSCF.BSCF_DATAFULLYAVAILABLE, (uint)ms.Length, 0);
                            pOIProtSink.ReportResult(NativeConstants.S_OK, 0, "");
                            break;
                        }

                        ms.Write(bytes, 0, n);
                    }
                }
            }
            catch (Exception ex)
            {
                pOIProtSink.ReportResult(NativeConstants.E_FAIL, 0, ex.Message);
            }
        }

        public void Continue(ref PROTOCOLDATA pProtocolData)
        {
        }

        public void Abort(int hrReason, int dwOptions)
        {
        }

        public void Terminate(int dwOptions)
        {
        }

        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        public int Read(IntPtr pv, int cb, out int pcbRead)
        {
            if (readPos >= data.Length)
            {
                pcbRead = 0;
                return NativeConstants.S_FALSE;
            }

            var n = Math.Min(cb, data.Length - readPos);
            Marshal.Copy(data, readPos, pv, n);
            readPos += n;
            pcbRead = n;

            return NativeConstants.S_OK;
        }

        public void Seek(long dlibMove, int dwOrigin, out long plibNewPosition)
        {
            int origin = 0;

            switch (dwOrigin)
            {
                case 0:
                    origin = 0;
                    break;
                case 1:
                    origin = readPos;
                    break;
                case 2:
                    origin = data.Length;
                    break;
            }

            readPos = origin + (int)dlibMove;
            plibNewPosition = readPos;
        }

        public void LockRequest(int dwOptions)
        {
        }

        public void UnlockRequest()
        {
        }

        public int CombineUrl(string pwzBaseUrl, string pwzRelativeUrl, uint dwCombineFlags, IntPtr pwzResult, uint cchResult, out uint pcchResult, uint dwReserved)
        {
            pcchResult = (uint)pwzRelativeUrl.Length;

            if (pwzRelativeUrl.Length > cchResult)
                return NativeConstants.S_FALSE; // buffer too small

            WriteLPWStr(pwzRelativeUrl, pwzResult);
            return NativeConstants.S_OK;
        }

        public int CompareUrl(string pwzUrl1, string pwzUrl2, uint dwCompareFlags)
        {
            return pwzUrl1 == pwzUrl2 ? NativeConstants.S_OK : NativeConstants.S_FALSE;
        }

        public int ParseUrl(string pwzUrl, PARSEACTION ParseAction, uint dwParseFlags, IntPtr pwzResult, uint cchResult, out uint pcchResult, uint dwReserved)
        {
            string result = DoParseUrl(pwzUrl, ParseAction);
            if (result != null)
            {
                pcchResult = (uint)result.Length;

                if (result.Length > cchResult)
                    return NativeConstants.S_FALSE; // buffer too small

                WriteLPWStr(result, pwzResult);
                return NativeConstants.S_OK;
            }

            pcchResult = 0;
            return NativeConstants.INET_E_DEFAULT_ACTION;
        }

        private string DoParseUrl(string pwzUrl, PARSEACTION ParseAction)
        {
            switch (ParseAction)
            {
                case PARSEACTION.PARSE_CANONICALIZE:
                    return pwzUrl;
                case PARSEACTION.PARSE_SECURITY_URL:
                    return "http://localhost/";
                case PARSEACTION.PARSE_SECURITY_DOMAIN:
                    return "http://localhost/";
                default:
                    return null;
            }
        }

        public int QueryInfo(string pwzUrl, QUERYOPTION QueryOption, uint dwQueryFlags, IntPtr pBuffer, uint cbBuffer, ref uint pcbBuf, uint dwReserved)
        {
            return NativeConstants.INET_E_DEFAULT_ACTION;
        }

        public static void WriteLPWStr(string value, IntPtr targetPtr)
        {
            Marshal.Copy(value.ToCharArray(), 0, targetPtr, value.Length);
            Marshal.WriteInt16(targetPtr, value.Length * 2, 0);
        }
    }
}
