using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace dp2LibraryApiTester
{
    public static class DataModel
    {
        public static MainForm MainForm
        {
            get
            {
                return FormClientInfo.MainForm as MainForm;
            }
        }

        public static void SetMessage(string text, string style = "")
        {
            MainForm.AppendString(text + "\r\n", style);
        }

        public static void SetHtml(string text)
        {
            MainForm.AppendHtml(text);
        }

        static string EncryptKey = "dp2libraryapitester_key";

        internal static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        internal static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }


        public static string dp2libraryServerUrl
        {
            get
            {
                return ClientInfo.Config.Get("dp2library", "serverUrl", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "serverUrl", value);
            }
        }

        public static string dp2libraryUserName
        {
            get
            {
                return ClientInfo.Config.Get("dp2library", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "userName", value);
            }
        }

        public static string dp2libraryPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("dp2library", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("dp2library", "password", password);
            }
        }

        public static string dp2libraryLocation
        {
            get
            {
                return ClientInfo.Config.Get("dp2library", "location", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "location", value);
            }
        }

        // 主要的通道池，用于当前服务器
        public static LibraryChannelPool _channelPool = new LibraryChannelPool();

        public static NormalResult Initial()
        {
            Free();

            _channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            if (string.IsNullOrEmpty(DataModel.dp2libraryServerUrl))
                return new NormalResult { Value = 0 };

            return new NormalResult { Value = 1 };
        }

        public static void Free()
        {
            _channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
        }

        internal static void Channel_BeforeLogin(object sender,
DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            if (e.FirstTry == true)
            {
                if (e.UserName != DataModel.dp2libraryUserName
                    && string.IsNullOrEmpty(e.UserName) == false)
                    throw new ArgumentException($"{e.UserName} 和默认账户不吻合。请改用 .NewChanel() 函数申请 LibraryChannel");
                
                e.UserName = DataModel.dp2libraryUserName;

                e.Password = DataModel.dp2libraryPassword;

                bool bIsReader = false;

                e.Parameters = "location=" + DataModel.dp2libraryLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                e.Parameters += ",client=dp2Inventory|" + ClientInfo.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
                else
                {
                    e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
                    e.Cancel = true;
                }
            }

            // e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
            e.Cancel = true;
        }
        static string _currentUserName = "";

        // public static string ServerUID = "";

        internal static void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;

            // _currentUserRights = channel.Rights;
            //_currentLibraryCodeList = channel.LibraryCodeList;
        }

        static object _syncRoot_channelList = new object();
        static List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public static void AbortAllChannel()
        {
            lock (_syncRoot_channelList)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }

        // parameters:
        //      style   如果包含 reader，表示以读者身份登录
        public static LibraryChannel NewChannel(string userName, 
            string password,
            string style = "")
        {
            LibraryChannel channel = new LibraryChannel();
            channel.Url = DataModel.dp2libraryServerUrl;

            var parameters = "client=dp2LibraryApiTester|0.01";
            if (StringUtil.IsInList("reader", style))
                parameters += ",type=reader";

            long lRet = channel.Login(userName,
                password,
                parameters,
                out string strError);
            if (lRet != 1)
                throw new Exception(strError);

            return channel;
        }

        public static void DeleteChannel(LibraryChannel channel)
        {
            channel.Logout(out string strError);
            channel.Dispose();
        }

        public static int MaxPoolChannelCount
        {
            get
            {
                return _channelPool.MaxCount;
            }
            set
            {
                _channelPool.MaxCount = value;
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public static LibraryChannel GetChannel(string strUserName = "")
        {
            if (string.IsNullOrEmpty(strUserName) == false)
                throw new ArgumentException($"GetChannel() 函数目前不允许使用非空的用户名 '{strUserName}'。请改用 NewChannel() 函数(和 DeleteChannel() 函数配套)");

            string strServerUrl = DataModel.dp2libraryServerUrl;

            if (string.IsNullOrEmpty(strUserName))
                strUserName = DataModel.dp2libraryUserName;

            LibraryChannel channel = _channelPool.GetChannel(strServerUrl, strUserName);
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }
            // TODO: 检查数组是否溢出
            return channel;
        }

        public static void ReturnChannel(LibraryChannel channel)
        {
            _channelPool.ReturnChannel(channel);
            lock (_syncRoot_channelList)
            {
                _channelList.Remove(channel);
            }
        }

        public static void Clear()
        {
            _channelPool.Clear();
        }
    }
}
