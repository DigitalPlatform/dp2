using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

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

                strSummary = strSummary.Replace(". -- ", "\r\n");   // .Replace("/", "\r\n");

                return new GetEntityDataResult
                {
                    Value = 0,
                    ItemXml = item_xml,
                    ItemRecPath = item_recpath,
                    Title = strSummary
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
                    "xml",
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

    }
}
