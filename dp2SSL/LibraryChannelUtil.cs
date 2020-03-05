using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    public static class LibraryChannelUtil
    {
        public class GetEntityDataResult : NormalResult
        {
            public string Title { get; set; }
            public string ItemXml { get; set; }
            public string ItemRecPath { get; set; }
        }

        // 获得一个册的题名字符串
        // .Value
        //      0   没有找到
        //      1   找到
        public static GetEntityDataResult GetEntityData(string pii)
        {
            /*
            title = "";
            item_xml = "";
            item_recpath = "";
            */

            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                long lRet = channel.GetItemInfo(null,
                    "item",
                    pii,
                    "",
                    "xml",
                    out string item_xml,
                    out string item_recpath,
                    out byte[] item_timestamp,
                    "",
                    out string biblio_xml,
                    out string biblio_recpath,
                    out string strError);
                if (lRet == -1)
                    return new GetEntityDataResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                lRet = channel.GetBiblioSummary(
    null,
    pii,
    "", // strConfirmItemRecPath,
    null,
    out string strBiblioRecPath,
    out string strSummary,
    out strError);
                if (lRet == -1)
                    return new GetEntityDataResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                strSummary = strSummary?.Replace(". -- ", "\r\n");   // .Replace("/", "\r\n");

                return new GetEntityDataResult
                {
                    Value = (int)lRet,
                    ItemXml = item_xml,
                    ItemRecPath = item_recpath,
                    Title = strSummary,
                    ErrorInfo = strError,
                    ErrorCode = channel.ErrorCode.ToString()
                };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class SetReaderInfoResult : NormalResult
        {
            public byte[] NewTimestamp { get; set; }
        }

        public static Task<SetReaderInfoResult> SetReaderInfo(string recpath,
            string xml,
            string old_xml,
            byte[] timestamp)
        {
            return Task<SetReaderInfoResult>.Run(() =>
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                try
                {
                    long lRet = channel.SetReaderInfo(null,
                        "change",
                        recpath,
                        xml,
                        old_xml,
                        timestamp,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue error_code,
                        out string strError);
                    if (lRet == -1)
                        return new SetReaderInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    if (lRet == 0)
                        return new SetReaderInfoResult
                        {
                            Value = 0,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    return new SetReaderInfoResult
                    {
                        Value = 1,
                        NewTimestamp = new_timestamp
                    };
                }
                finally
                {
                    App.CurrentApp.ReturnChannel(channel);
                }
            });
        }

        public class GetReaderInfoResult : NormalResult
        {
            public string RecPath { get; set; }
            public string ReaderXml { get; set; }
            public byte[] Timestamp { get; set; }
        }

        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfo(string pii)
        {
            /*
            reader_xml = "";
            recpath = "";
            timestamp = null;
            */
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法获得读者信息"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                long lRet = channel.GetReaderInfo(null,
                    pii,
                    "advancexml", // "xml",
                    out string[] results,
                    out string recpath,
                    out byte[] timestamp,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                    return new GetReaderInfoResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                        RecPath = recpath,
                        Timestamp = timestamp
                    };

                // 2019/12/19
                // 命中读者记录多于一条
                if (lRet > 1)
                {
                    return new GetReaderInfoResult
                    {
                        Value = -1,
                        ErrorInfo = $"装载读者记录失败：'{pii}' 检索命中读者记录 {lRet} 条"
                    };
                }

                string reader_xml = "";
                if (results != null && results.Length > 0)
                    reader_xml = results[0];
                return new GetReaderInfoResult
                {
                    Value = 1,
                    RecPath = recpath,
                    Timestamp = timestamp,
                    ReaderXml = reader_xml
                };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class LoginResult : NormalResult
        {
            public string OutputUserName { get; set; }
            public string Rights { get; set; }
            public string LibraryCode { get; set; }
        }

        // result.Value
        //      -1:   出错
        //      0:    登录未成功
        //      1:    登录成功
        public static LoginResult WorkerLogin(string userName, string password)
        {
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法进行工作人员登录"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel(userName);
            try
            {
                // -1:   出错
                // 0:    登录未成功
                // 1:    登录成功
                long lRet = channel.Login(userName,
                    password,
                    "type=worker,client=dp2ssl|" + WpfClientInfo.ClientVersion,
                    out string strOutputUserName,
                    out string strRights,
                    out string strLibraryCode,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                    return new LoginResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                    };

                // testing
                // channel.Logout(out strError);

                return new LoginResult
                {
                    Value = 1,
                    OutputUserName = strOutputUserName,
                    Rights = strRights,
                    LibraryCode = strLibraryCode,
                };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class GetLocationListResult : NormalResult
        {
            public List<string> List { get; set; }
        }

        // 获得馆藏地列表
        public static GetLocationListResult GetLocationList()
        {
            string strOutputInfo = "";
            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                long lRet = channel.GetSystemParameter(
null,
"circulation",
"locationTypes",
out strOutputInfo,
out string strError);
                if (lRet == -1)
                    return new GetLocationListResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                return new GetLocationListResult
                {
                    Value = -1,
                    ErrorInfo = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message
                };
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
<locationTypes>
    <item canborrow="yes" itembarcodeNullable="yes">流通库</item>
    <item>阅览室</item>
    <library code="分馆1">
        <item canborrow="yes">流通库</item>
        <item>阅览室</item>
    </library>
</locationTypes>
*/

            List<string> results = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement node in nodes)
            {
                string strText = node.InnerText;

                // 
                string strLibraryCode = "";
                XmlElement parent = node.ParentNode as XmlElement;
                if (parent.Name == "library")
                {
                    strLibraryCode = parent.GetAttribute("code");
                }

                results.Add(string.IsNullOrEmpty(strLibraryCode) ? strText : strLibraryCode + "/" + strText);
            }

            return new GetLocationListResult { List = results };
        }

    }
}
