using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

// dynamic deserialize JSONG method:
// https://stackoverflow.com/questions/3142495/deserialize-json-into-c-sharp-dynamic-object
/***
Another way using Newtonsoft.Json:
dynamic stuff = Newtonsoft.Json.JsonConvert.DeserializeObject("{ color: 'red', value: 5 }");
string color = stuff.color;
int value = stuff.value;
***/

namespace GcatLite
{
    /// <summary>
    /// Restful 通讯的通道。在通讯过程中需要保持使用同一个对象，它自然能持久化 CookiesContainer，保证有状态通讯
    /// </summary>
    public class RestChannel : CookieAwareWebClient// , IDisposable
    {
        public string ServerUrl { get; set; }

        const string ContentTypeValue = "application/json; charset=utf-8";

        public RestChannel(string url) : base()
        {
            this.ServerUrl = url;
        }

        protected override void Dispose(bool disposing)
        {
            if (string.IsNullOrEmpty(this.ServerUrl) == false)
                this.BeginLogout();
        }

        public
#if DYNAMIC
            dynamic
#else
            LibraryServerResult
#endif
            Login(string userName,
string password,
string parameters)
        {
            if (string.IsNullOrEmpty(this.ServerUrl))
                throw new ArgumentException("ServerUrl 尚未设置");

            this.Headers["Content-type"] = ContentTypeValue;
            var request = new
            {
                strUserName = userName,
                strPassword = password,
                strParameters = parameters
            };
            byte[] baData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            byte[] result = this.UploadData(GetRestfulApiUrl(this.ServerUrl, "Login"),
                "POST",
                baData);
            string strResult = Encoding.UTF8.GetString(result);
#if DYNAMIC
            dynamic response = JsonConvert.DeserializeObject(strResult);
#else
            var response = JsonConvert.DeserializeObject<LoginResponse>(strResult);
#endif
            return response.LoginResult;
        }

        // 取著者号
        // result.Value:
        //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
        //		-3	需要回答问题
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public
#if DYNAMIC
            dynamic
#else
            LibraryServerResult
#endif
            GetAuthorNumber(
            string author,
            bool selectPinyin,
            bool selectEntry,
            bool outputDebugInfo,
#if DYNAMIC
            ref dynamic questions_param,
#else
            ref List<Question> questions_param,
#endif
            out string number,
            out string debugInfo)
        {
            number = "";
            debugInfo = "";

            if (string.IsNullOrEmpty(this.ServerUrl))
                throw new ArgumentException("ServerUrl 尚未设置");

            this.Headers["Content-type"] = ContentTypeValue;
            var request = new
            {
                strAuthor = author,
                bSelectPinyin = selectPinyin,
                bSelectEntry = selectEntry,
                bOutputDebugInfo = outputDebugInfo,
                questions = questions_param,
            };

            byte[] baData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            byte[] result = this.UploadData(GetRestfulApiUrl(this.ServerUrl, "GetAuthorNumber"),
                "POST",
                baData);

            string strResult = Encoding.UTF8.GetString(result);
#if DYNAMIC
            dynamic response = JsonConvert.DeserializeObject(strResult);
#else
            var response = JsonConvert.DeserializeObject<GetAuthorNumberResponse>(strResult);
#endif
            number = response.strNumber;
            debugInfo = response.strDebugInfo;
            questions_param = response.questions;

            return response.GetAuthorNumberResult;
        }

        // 登出。不等待返回值
        public void BeginLogout()
        {
            if (string.IsNullOrEmpty(this.ServerUrl))
                throw new ArgumentException("ServerUrl 尚未设置");

            this.Headers["Content-type"] = ContentTypeValue;
            var request = new
            {
            };
            byte[] baData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            Task<byte[]> task = this.UploadDataTaskAsync(GetRestfulApiUrl(this.ServerUrl, "Logout"),
                "POST",
                baData);
#if NO
            string strResult = Encoding.UTF8.GetString(result);
            var response = JsonConvert.DeserializeObject<LogoutResponse>(strResult);
            return response.LogoutResult;
#endif
        }

        // 登出
        public
#if DYNAMIC
            dynamic
#else
            LibraryServerResult
#endif
            Logout()
        {
            if (string.IsNullOrEmpty(this.ServerUrl))
                throw new ArgumentException("ServerUrl 尚未设置");

            this.Headers["Content-type"] = ContentTypeValue;
            var request = new
            {
            };
            byte[] baData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            byte[] result = this.UploadData(GetRestfulApiUrl(this.ServerUrl, "Logout"),
                "POST",
                baData);
            string strResult = Encoding.UTF8.GetString(result);
#if DYNAMIC
            dynamic response = JsonConvert.DeserializeObject(strResult);
#else
            var response = JsonConvert.DeserializeObject<LogoutResponse>(strResult);
#endif
            return response.LogoutResult;
        }

        static string GetRestfulApiUrl(string url, string strMethod)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("url 不应为空");

            if (string.IsNullOrEmpty(url) == true)
                return strMethod;

            if (url[url.Length - 1] == '/')
                return url + strMethod;

            return url + "/" + strMethod;
        }
    }

    /// <summary>
    /// 可以设置 CookieContainer 的 WebClient 类
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        /// 保持通道的恒定身份，是靠 HTTP 通讯的 Cookies 机制
        public CookieContainer CookieContainer { get; set; }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        { }
        public CookieAwareWebClient(CookieContainer c)
        {
            this.CookieContainer = c;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }
            return request;
        }
    }

    #region 数据结构

#if !DYNAMIC
    // API GetAuthorNumber() 的响应包结构
    public class GetAuthorNumberResponse
    {
        public LibraryServerResult GetAuthorNumberResult { get; set; }
        public List<Question> questions { get; set; }
        public string strNumber { get; set; }
        public string strDebugInfo { get; set; }
    }

    // API Login() 的响应包结构
    public class LoginResponse
    {
        public LibraryServerResult LoginResult { get; set; }
        public string strOutputUserName { get; set; }
        public string strRights { get; set; }

        public string strLibraryCode { get; set; }
    }

    // API Logout() 的响应包结构
    public class LogoutResponse
    {
        public LibraryServerResult LogoutResult { get; set; }
    }

    // 需要前端回答的问题
    public class Question
    {
        public string Text { get; set; }   // 问题正文
        public string Answer { get; set; }  // 问题答案
    }

    // API 函数返回值
    public class LibraryServerResult
    {
        public long Value { get; set; }
        public string ErrorInfo { get; set; }
        public ErrorCode ErrorCode { get; set; }
    }

    // dp2Library API错误码
    public enum ErrorCode
    {
        NoError = 0,
        SystemError = 1,    // 系统错误。指application启动时的严重错误。
        NotFound = 2,   // 没有找到
        ReaderBarcodeNotFound = 3,  // 读者证条码号不存在
        ItemBarcodeNotFound = 4,  // 册条码号不存在
        Overdue = 5,    // 还书过程发现有超期情况（已经按还书处理完毕，并且已经将超期信息记载到读者记录中，但是需要提醒读者及时履行超期违约金等手续）
        NotLogin = 6,   // 尚未登录
        DupItemBarcode = 7, // 预约中本次提交的某些册条码号被本读者先前曾预约过
        InvalidParameter = 8,   // 不合法的参数
        ReturnReservation = 9,    // 还书操作成功, 因属于被预约图书, 请放入预约保留架
        BorrowReservationDenied = 10,    // 借书操作失败, 因属于被预约(到书)保留的图书, 非当前预约者不能借阅
        RenewReservationDenied = 11,    // 续借操作失败, 因属于被预约的图书
        AccessDenied = 12,  // 存取被拒绝
        ChangePartDenied = 13,    // 部分修改被拒绝
        ItemBarcodeDup = 14,    // 册条码号重复
        Hangup = 15,    // 系统挂起
        ReaderBarcodeDup = 16,  // 读者证条码号重复
        HasCirculationInfo = 17,    // 包含流通信息(不能删除)
        SourceReaderBarcodeNotFound = 18,  // 源读者证条码号不存在
        TargetReaderBarcodeNotFound = 19,  // 目标读者证条码号不存在
        FromNotFound = 20,  // 检索途径(from caption或者style)没有找到
        ItemDbNotDef = 21,  // 实体库没有定义
        IdcardNumberDup = 22,   // 身份证号检索点命中读者记录不唯一。因为无法用它借书还书。但是可以用证条码号来进行
        IdcardNumberNotFound = 23,  // 身份证号不存在

        // 以下为兼容内核错误码而设立的同名错误码
        AlreadyExist = 100, // 兼容
        AlreadyExistOtherType = 101,
        ApplicationStartError = 102,
        EmptyRecord = 103,
        // None = 104, 采用了NoError
        NotFoundSubRes = 105,
        NotHasEnoughRights = 106,
        OtherError = 107,
        PartNotFound = 108,
        RequestCanceled = 109,
        RequestCanceledByEventClose = 110,
        RequestError = 111,
        RequestTimeOut = 112,
        TimestampMismatch = 113,
    }
#endif

#endregion
}
