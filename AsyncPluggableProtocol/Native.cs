using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// https://github.com/mganss/AsyncPluggableProtocol
namespace AsyncPluggableProtocol
{
    /// <summary>
    /// Contains the security descriptor of an object and specifies whether the handle retrieved by specifying this structure is inheritable.
    /// </summary>
    [ComConversionLoss]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SECURITY_ATTRIBUTES
    {
        /// <summary>
        /// The size, in bytes, of this structure. Set this value to the size of this structure.
        /// </summary>
        public uint nLength;
        /// <summary>
        /// A pointer to a SECURITY_DESCRIPTOR structure that controls access to the object. If the value of this member is null, the object is assigned the default security descriptor associated with the access token of the calling process. This is not the same as granting access to everyone by assigning a null discretionary access control list (DACL). The default DACL in the access token of a process allows access only to the user represented by the access token.
        /// </summary>
        [ComConversionLoss]
        public IntPtr lpSecurityDescriptor;
        /// <summary>
        /// A boolean value that specifies whether the returned handle is inherited when a new process is created. If this field is true, the new process inherits the handle.
        /// </summary>
        public int bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct STGMEDIUM
    {
        public uint tymed;
        public IntPtr unionmember;
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnkForRelease;
    }

    /// <summary>
    /// Contains additional information on the requested binding operation.  The meaning of this structure is specific to the type of asynchronous moniker.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BINDINFO
    {
        /// <summary>
        /// Indicates the size of the structure in bytes.
        /// </summary>
        public uint cbSize;
        /// <summary>
        /// The behavior of this field is moniker-specific.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szExtraInfo;
        /// <summary>
        /// Defines the data to be used in a PUT or POST operation specified by <see cref="F:Microsoft.VisualStudio.OLE.Interop.BINDINFO.dwBindVerb"/>.
        /// </summary>
        public STGMEDIUM stgmedData;
        /// <summary>
        /// Indicates the flag from the <see cref="F:Microsoft.VisualStudio.OLE.Interop.BINDINFOF"/> enumeration that determines the use of URL encoding during the binding operation.  This member is specific to URL monikers.
        /// </summary>
        public uint grfBindInfoF;
        /// <summary>
        /// Indicates the value from the <see cref="T:Microsoft.VisualStudio.OLE.Interop.BINDVERB"/> enumeration specifying an action to be performed during the bind operation.
        /// </summary>
        public uint dwBindVerb;
        /// <summary>
        /// Represents the BSTR specifying a protocol-specific custom action to be performed during the bind operation (only if <see cref="F:Microsoft.VisualStudio.OLE.Interop.BINDINFO.dwBindVerb"/> is set to BINDVERB_CUSTOM).
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szCustomVerb;
        /// <summary>
        /// Indicates the size of the data provided in the <see cref="F:Microsoft.VisualStudio.OLE.Interop.BINDINFO.stgmedData"/> member.
        /// </summary>
        public uint cbstgmedData;
        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwOptions;
        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwOptionsFlags;
        /// <summary>
        /// Represents an unsigned long integer value that contains the code page used to perform the conversion.
        /// </summary>
        public uint dwCodePage;
        /// <summary>
        /// Represents the <see cref="F:Microsoft.VisualStudio.OLE.Interop.SECUTIRY_ATTRIBUTES"/> structure that contains the descriptor for the object being bound to and indicates whether the handle retrieved by specifying this structure is inheritable.
        /// </summary>
        public SECURITY_ATTRIBUTES securityAttributes;
        /// <summary>
        /// Indicates the interface identifier of the IUnknown interface referred to by <see cref="F:Microsoft.VisualStudio.OLE.Interop.BINDINFO.pUnk"/>.
        /// </summary>
        public Guid iid;
        /// <summary>
        /// Point to the IUnknown (COM) interface.
        /// </summary>
        [MarshalAs(UnmanagedType.IUnknown)]
        public object punk;
        /// <summary>
        /// Reserved. Must be set to 0.
        /// </summary>
        public uint dwReserved;
    }

    [ComImport]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        [PreserveSig]
        int CreateInstance([In] IntPtr pUnkOuter, [In] ref Guid riid, [Out] out IntPtr ppvObject);
        void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
    }

    [ComImport]
    [Guid("79EAC9E1-BAF9-11CE-8C82-00AA004BA90B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternetBindInfo
    {
        void GetBindInfo(out uint grfBINDF, [In, Out] ref BINDINFO pbindinfo);
        void GetBindString(uint ulStringType, [MarshalAs(UnmanagedType.LPWStr)] ref string ppwzStr, uint cEl, ref uint pcElFetched);
    }

    /// <summary>
    /// Indicates the type of data that is available when passed to the client in IBindStatusCallback::OnDataAvailable.
    /// </summary>
    public enum BSCF
    {
        BSCF_FIRSTDATANOTIFICATION = 1,
        BSCF_INTERMEDIATEDATANOTIFICATION = 2,
        BSCF_LASTDATANOTIFICATION = 4,
        BSCF_DATAFULLYAVAILABLE = 8,
        BSCF_AVAILABLEDATASIZEUNKNOWN = 16,
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("79EAC9E5-BAF9-11CE-8C82-00AA004BA90B")]
    public interface IInternetProtocolSink
    {
        void Switch(ref PROTOCOLDATA pProtocolData);
        void ReportProgress(uint ulStatusCode, [MarshalAs(UnmanagedType.LPWStr)] string szStatusText);
        void ReportData(BSCF grfBSCF, uint ulProgress, uint ulProgressMax);
        void ReportResult(int hrResult, uint dwError, [MarshalAs(UnmanagedType.LPWStr)] string szResult);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROTOCOLDATA
    {
        uint grfFlags;
        uint dwState;
        IntPtr pData;
        ulong cbData;
    }

    /// <summary>
    /// Represents a 64-bit signed integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct LARGE_INTEGER
    {
        /// <summary>
        /// Represents a 64-bit signed integer.
        /// </summary>
        public long QuadPart;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ULARGE_INTEGER
    {
        public ulong QuadPart;
    }

    [Flags]
    public enum PI_FLAGS
    {
        PI_PARSE_URL = 1,
        PI_FILTER_MODE = 2,
        PI_FORCE_ASYNC = 4,
        PI_USE_WORKERTHREAD = 8,
        PI_MIMEVERIFICATION = 16,
        PI_CLSIDLOOKUP = 32,
        PI_DATAPROGRESS = 64,
        PI_SYNCHRONOUS = 128,
        PI_APARTMENTTHREADED = 256,
        PI_CLASSINSTALL = 512,
        PI_PASSONBINDCTX = 8192,
        PI_NOMIMEHANDLER = 32768,
        PI_LOADAPPDIRECT = 16384,
        PD_FORCE_SWITCH = 65536,
        PI_PREFERDEFAULTHANDLER = 131072,
    }

    /// <summary>This interface is used to control the operation of an asynchronous pluggable protocol handler. </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9e3-baf9-11ce-8c82-00aa004ba90b")]
    public interface IInternetProtocolRoot
    {
        /// <summary>Starts the operation. </summary>
        /// <remarks>URL Moniker Error Codes can be returned only by a pluggable namespace handler or MIME filter. Only a single, permanently registered asynchronous pluggable protocol handler can be assigned to a particular scheme (such as FTP), so there are no other handlers to default to.</remarks>
        /// <param name="szUrl">
        /// Address of a string value that contains the URL. For a pluggable MIME filter, this parameter contains the MIME type.</param>
        /// <param name="pOIProtSink">
        /// Address of the protocol sink provided by the client.</param>
        /// <param name="pOIBindInfo">
        /// Address of the IInternetBindInfo interface from which the protocol gets download-specific information.</param>
        /// <param name="grfPI">
        /// Unsigned long integer value that contains the flags that determine if the method only parses or if it parses and downloads the URL. This can be one of the PI_FLAGS values.</param>
        /// <param name="dwReserved">
        /// For pluggable MIME filters, contains the address of a PROTOCOLFILTERDATA structure. Otherwise, it is reserved and must be set to NULL.</param>
        void Start(
            [MarshalAs(UnmanagedType.LPWStr)]
                [In] string szUrl,
                [MarshalAs(UnmanagedType.Interface)]
                [In] IInternetProtocolSink pOIProtSink,
                [MarshalAs(UnmanagedType.Interface)]
                [In] IInternetBindInfo pOIBindInfo,
                [In] PI_FLAGS grfPI,
                [In] int dwReserved);

        /// <summary>Allows the pluggable protocol handler to continue processing data on the apartment thread. </summary>
        /// <remarks>This method is called in response to a call to the IInternetProtocolSink::Switch method. </remarks>
        /// <param name="pProtocolData">
        /// Address of the PROTOCOLDATA structure data passed to IInternetProtocolSink::Switch.</param>
        void Continue(
            [In] ref PROTOCOLDATA pProtocolData);

        /// <summary>Cancels an operation that is in progress. </summary>
        /// <param name="hrReason">
        /// HRESULT value that contains the reason for canceling the operation. This is the HRESULT that is reported by the pluggable protocol if it successfully canceled the binding. The pluggable protocol passes this HRESULT to urlmon.dll using the IInternetProtocolSink::ReportResult method. Urlmon.dll then passes this HRESULT to the host using IBindStatusCallback::OnStopBinding.</param>
        /// <param name="dwOptions">
        /// Reserved. Must be set to 0.</param>
        void Abort(
            [In] int hrReason,
            [In] int dwOptions);

        /// <summary>Releases the resources used by the pluggable protocol handler. </summary>
        /// <remarks>Note to implementers
        /// Urlmon.dll will not call this method until your asynchronous pluggable protocol handler calls the Urlmon.dll IInternetProtocolSink::ReportResult method. When your IInternetProtocolRoot::Terminate method is called, your asynchronous pluggable protocol handler should free all resources it has allocated.
        /// Note to callers
        /// This method should be called after receiving a call to your IInternetProtocolSink::ReportResult method and after the protocol handler's IInternetProtocol::LockRequest method has been called. </remarks>
        /// <param name="dwOptions">
        /// Reserved. Must be set to 0.</param>
        void Terminate(
            [In] int dwOptions);

        /// <summary>Not currently implemented.</summary>
        void Suspend();

        /// <summary>Not currently implemented. </summary>
        void Resume();
    }

    /// <summary>This is the main interface exposed by an asynchronous pluggable protocol. This interface and the IInternetProtocolSink interface communicate with each other very closely during download operations. </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9e4-baf9-11ce-8c82-00aa004ba90b")]
    public interface IInternetProtocol
    {
        /// <summary>Starts the operation. </summary>
        /// <remarks>URL Moniker Error Codes can be returned only by a pluggable namespace handler or MIME filter. Only a single, permanently registered asynchronous pluggable protocol handler can be assigned to a particular scheme (such as FTP), so there are no other handlers to default to.</remarks>
        /// <param name="szUrl">
        /// Address of a string value that contains the URL. For a pluggable MIME filter, this parameter contains the MIME type.</param>
        /// <param name="pOIProtSink">
        /// Address of the protocol sink provided by the client.</param>
        /// <param name="pOIBindInfo">
        /// Address of the IInternetBindInfo interface from which the protocol gets download-specific information.</param>
        /// <param name="grfPI">
        /// Unsigned long integer value that contains the flags that determine if the method only parses or if it parses and downloads the URL. This can be one of the PI_FLAGS values.</param>
        /// <param name="dwReserved">
        /// For pluggable MIME filters, contains the address of a PROTOCOLFILTERDATA structure. Otherwise, it is reserved and must be set to NULL.</param>
        void Start(
            [MarshalAs(UnmanagedType.LPWStr)]
                [In] string szUrl,
                [MarshalAs(UnmanagedType.Interface)]
                [In] IInternetProtocolSink pOIProtSink,
                [MarshalAs(UnmanagedType.Interface)]
                [In] IInternetBindInfo pOIBindInfo,
                [In] PI_FLAGS grfPI,
                [In] int dwReserved);

        /// <summary>Allows the pluggable protocol handler to continue processing data on the apartment thread. </summary>
        /// <remarks>This method is called in response to a call to the IInternetProtocolSink::Switch method. </remarks>
        /// <param name="pProtocolData">
        /// Address of the PROTOCOLDATA structure data passed to IInternetProtocolSink::Switch.</param>
        void Continue(
            [In] ref PROTOCOLDATA pProtocolData);

        /// <summary>Cancels an operation that is in progress. </summary>
        /// <param name="hrReason">
        /// HRESULT value that contains the reason for canceling the operation. This is the HRESULT that is reported by the pluggable protocol if it successfully canceled the binding. The pluggable protocol passes this HRESULT to urlmon.dll using the IInternetProtocolSink::ReportResult method. Urlmon.dll then passes this HRESULT to the host using IBindStatusCallback::OnStopBinding.</param>
        /// <param name="dwOptions">
        /// Reserved. Must be set to 0.</param>
        void Abort(
            [In] int hrReason,
            [In] int dwOptions);

        /// <summary>Releases the resources used by the pluggable protocol handler. </summary>
        /// <remarks>Note to implementers
        /// Urlmon.dll will not call this method until your asynchronous pluggable protocol handler calls the Urlmon.dll IInternetProtocolSink::ReportResult method. When your IInternetProtocolRoot::Terminate method is called, your asynchronous pluggable protocol handler should free all resources it has allocated.
        /// Note to callers
        /// This method should be called after receiving a call to your IInternetProtocolSink::ReportResult method and after the protocol handler's IInternetProtocol::LockRequest method has been called. </remarks>
        /// <param name="dwOptions">
        /// Reserved. Must be set to 0.</param>
        void Terminate(
            [In] int dwOptions);

        /// <summary>Not currently implemented.</summary>
        void Suspend();

        /// <summary>Not currently implemented.</summary>
        void Resume();

        /// <summary>Reads data retrieved by the pluggable protocol handler. </summary>
        /// <remarks>Developers who are implementing an asynchronous pluggable protocol must be prepared to have their implementation of IInternetProtocol::Read continue to be called a few extra times after it has returned S_FALSE. </remarks>
        /// <param name="pv">
        /// Address of the buffer where the information will be stored.</param>
        /// <param name="cb">
        /// Value that indicates the size of the buffer.</param>
        /// <param name="pcbRead">
        /// Address of a value that indicates the amount of data stored in the buffer.</param>
        [PreserveSig]
        int Read(
            [In, Out] IntPtr pv,
            [In] int cb,
            [Out] out int pcbRead);

        /// <summary>Moves the current seek offset.</summary>
        /// <param name="dlibMove">
        /// Large integer value that indicates how far to move the offset.</param>
        /// <param name="dwOrigin">
        /// DWORD value that indicates where the move should begin.
        /// FILE_BEGIN : Starting point is zero or the beginning of the file. If FILE_BEGIN is specified, dlibMove is interpreted as an unsigned location for the new file pointer.
        /// FILE_CURRENT : Current value of the file pointer is the starting point.
        /// FILE_END : Current end-of-file position is the starting point. This method fails if the content length is unknown.</param>
        /// <param name="plibNewPosition">
        /// Address of an unsigned long integer value that indicates the new offset.</param>
        void Seek(
            [In] long dlibMove,
            [In] int dwOrigin,
            [Out] out long plibNewPosition);

        /// <summary>Locks the requested resource so that the IInternetProtocolRoot::Terminate method can be called and the remaining data can be read. </summary>
        /// <remarks>For asynchronous pluggable protocols that do not need to lock a request, the method should return S_OK.</remarks>
        /// <param name="dwOptions">
        /// Reserved. Must be set to 0.</param>
        void LockRequest(
            [In] int dwOptions);

        /// <summary>Frees any resources associated with a lock. </summary>
        /// <remarks>This method is called only if IInternetProtocol::LockRequest was called. </remarks>
        void UnlockRequest();
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9e7-baf9-11ce-8c82-00aa004ba90b")]
    public interface IInternetSession
    {
        void RegisterNameSpace(
            [MarshalAs(UnmanagedType.Interface)]
                IClassFactory pCF,
            //IntPtr pCF,
            [In] ref Guid rclsid,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzProtocol,
            int cPatterns,
            [In, MarshalAs(UnmanagedType.LPWStr)] ref string ppwzPatterns,
            int dwReserved);

        void UnregisterNameSpace(
            IClassFactory pCF,
            //IntPtr pCF,
            [MarshalAs(UnmanagedType.LPWStr)] string pszProtocol);

        void RegisterMimeFilter(
            IntPtr pCF,
            [In] ref Guid rclsid,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzType);

        void UnregisterMimeFilter(
            IntPtr pCF,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzType);

        void CreateBinding(
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPWStr)] string szUrl,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppunk,
            [Out] out IInternetProtocol ppOInetProt,
            int dwOption);

        void SetSessionOption(
            int dwOption,
            [MarshalAs(UnmanagedType.I4)] IntPtr pBuffer,
            int dwBufferLength,
            int dwReserved);

        void GetSessionOption(
            int dwOption,
            [MarshalAs(UnmanagedType.I4)] IntPtr pBuffer,
            [In, Out] ref int pdwBufferLength,
            int dwReserved);
    }

    [ComImport]
    [Guid("79eac9ec-baf9-11ce-8c82-00aa004ba90b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInternetProtocolInfo
    {
        [PreserveSig]
        int ParseUrl(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [In] PARSEACTION ParseAction,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwParseFlags,
            [In] IntPtr pwzResult,
            [In] [MarshalAs(UnmanagedType.U4)] uint cchResult,
            [Out] [MarshalAs(UnmanagedType.U4)] out uint pcchResult,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwReserved);

        [PreserveSig]
        int CombineUrl(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzBaseUrl,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzRelativeUrl,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwCombineFlags,
            [In] IntPtr pwzResult,
            [In] [MarshalAs(UnmanagedType.U4)] uint cchResult,
            [Out] [MarshalAs(UnmanagedType.U4)] out uint pcchResult,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwReserved);

        [PreserveSig]
        int CompareUrl(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl1,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl2,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwCompareFlags);

        [PreserveSig]
        int QueryInfo(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [In] QUERYOPTION OueryOption,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwQueryFlags,
            [In] IntPtr pBuffer,
            [In] [MarshalAs(UnmanagedType.U4)] uint cbBuffer,
            [In, Out] [MarshalAs(UnmanagedType.U4)] ref uint pcbBuf,
            [In] [MarshalAs(UnmanagedType.U4)] uint dwReserved);
    }

    public enum PARSEACTION
    {
        PARSE_CANONICALIZE = 1,
        PARSE_FRIENDLY,
        PARSE_SECURITY_URL,
        PARSE_ROOTDOCUMENT,
        PARSE_DOCUMENT,
        PARSE_ANCHOR,
        PARSE_ENCODE,
        PARSE_DECODE,
        PARSE_PATH_FROM_URL,
        PARSE_URL_FROM_PATH,
        PARSE_MIME,
        PARSE_SERVER,
        PARSE_SCHEMA,
        PARSE_SITE,
        PARSE_DOMAIN,
        PARSE_LOCATION,
        PARSE_SECURITY_DOMAIN,
        PARSE_ESCAPE,
        PARSE_UNESCAPE
    }

    public enum QUERYOPTION
    {
        QUERY_EXPIRATION_DATE = 1,
        QUERY_TIME_OF_LAST_CHANGE,
        QUERY_CONTENT_ENCODING,
        QUERY_CONTENT_TYPE,
        QUERY_REFRESH,
        QUERY_RECOMBINE,
        QUERY_CAN_NAVIGATE,
        QUERY_USES_NETWORK,
        QUERY_IS_CACHED,
        QUERY_IS_INSTALLEDENTRY,
        QUERY_IS_CACHED_OR_MAPPED,
        QUERY_USES_CACHE,
        QUERY_IS_SECURE,
        QUERY_IS_SAFE
    }

    public static class NativeMethods
    {
        [DllImport("urlmon.dll")]
        internal static extern int CoInternetGetSession(
            int dwSessionMode,
            out IInternetSession ppIInternetSession,
            int dwReserved);
    }

    public static class NativeConstants
    {
        public static readonly Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IDispatchEx = new Guid("{a6ef9860-c720-11d0-9337-00a0c90dcaa9}");
        public static readonly Guid IID_IPersistStorage = new Guid("{0000010A-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IPersistStream = new Guid("{00000109-0000-0000-C000-000000000046}");
        public static readonly Guid IID_IPersistPropertyBag = new Guid("{37D84F60-42CB-11CE-8135-00AA004BB851}");

        public const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        public const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;

        public const int INET_E_DEFAULT_ACTION = unchecked((int)0x800C0011);
        public const int INET_E_INVALID_URL = unchecked((int)0x800C0002);
        public const int INET_E_DATA_NOT_AVAILABLE = unchecked((int)0x800C0007);

        public const int STG_E_FILENOTFOUND = unchecked((int)0x80030002);

        public const int S_OK = 0;
        public const int S_FALSE = 1;

        public const int E_PENDING = unchecked((int)0x8000000A);
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    }
}

