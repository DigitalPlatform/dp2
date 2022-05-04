using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace sipApiTester
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

        static string EncryptKey = "sipapitester_key";

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


        public static string sipServerAddr
        {
            get
            {
                return ClientInfo.Config.Get("sip", "serverAddr", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "serverAddr", value);
            }
        }

        public static string sipServerPort
        {
            get
            {
                return ClientInfo.Config.Get("sip", "serverPort", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "serverPort", value);
            }
        }

        public static string sipUserName
        {
            get
            {
                return ClientInfo.Config.Get("sip", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "userName", value);
            }
        }

        public static string sipPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("sip", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("sip", "password", password);
            }
        }

        public static NormalResult Initial()
        {
            return new NormalResult();
        }

        public static void Free()
        {

        }

        public static void Clear()
        {
            // Channels.Clear();
        }
    }

}
