using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace dp2KernelApiTester
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

        public static void ShowProgressMessage(string id, string text)
        {
            MainForm.ShowProgressMessage(id, text);
        }

        static int _progressIdSeed = 0;

        public static string NewProgressID()
        {
            return _progressIdSeed++.ToString();
        }

        static string EncryptKey = "dp2kernelapitester_key";

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

        public static string [] Urls
        {
            get
            {
                var list = ClientInfo.Config.Get("dp2kernel", "urls", null);
                return StringUtil.SplitList(list).ToArray();
            }
            set
            {

                ClientInfo.Config.Set("dp2kernel", "urls", StringUtil.MakePathList(value));
            }
        }

        public static string dp2kernelServerUrl
        {
            get
            {
                return ClientInfo.Config.Get("dp2kernel", "serverUrl", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2kernel", "serverUrl", value);
            }
        }

        public static string dp2kernelUserName
        {
            get
            {
                return ClientInfo.Config.Get("dp2kernel", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2kernel", "userName", value);
            }
        }

        public static string dp2kernelPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("dp2kernel", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("dp2kernel", "password", password);
            }
        }

        public static RmsChannelCollection Channels = new RmsChannelCollection();


        public static NormalResult Initial()
        {
            Free();

            Channels.AskAccountInfo -= Channels_AskAccountInfo;
            Channels.AskAccountInfo += Channels_AskAccountInfo;

            if (string.IsNullOrEmpty(DataModel.dp2kernelServerUrl))
                return new NormalResult { Value = 0 };

            return new NormalResult { Value = 1 };
        }

        private static void Channels_AskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            e.Owner = null;

            ///
            e.UserName = DataModel.dp2kernelUserName;
            e.Password = DataModel.dp2kernelPassword;
            e.Result = 1;
        }

        public static void Free()
        {
            Channels.AskAccountInfo -= Channels_AskAccountInfo;
        }

        public static RmsChannel GetChannel()
        {
            return Channels.GetChannel(DataModel.dp2kernelServerUrl);
        }

        public static void ReturnChannel(RmsChannel channel)
        {

        }

        static string _currentUserName = "";

        public static void Clear()
        {
            Channels.Clear();
        }
    }

}
