using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2LibraryApiTester
{
    public static class Utility
    {

        public static void DisplayErrors(List<string> errors)
        {
            DataModel.SetMessage("**********************************", "error");
            foreach (string error in errors)
            {
                DataModel.SetMessage($"!!! {error} !!!");
            }
            DataModel.SetMessage("**********************************", "error");
        }

        public static int DeleteUsers(LibraryChannel channel,
    IEnumerable<string> user_names,
    out string strError)
        {
            strError = "";
            foreach (string user_name in user_names)
            {
                long lRet = channel.SetUser(null,
        "delete",
        new UserInfo
        {
            UserName = user_name,
        },
        out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    return -1;
            }
            return 0;
        }

        // 设置条码号校验规则
        public static int SetBarcodeValidation(
            string validation_innerxml,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = DataModel.GetChannel();

            try
            {
                long lRet = channel.SetSystemParameter(null,
    "circulation",
    "barcodeValidation",
    validation_innerxml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        // 设置借阅权限表
        public static int SetRightsTable(
            string innerxml,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = DataModel.GetChannel();

            try
            {
                long lRet = channel.SetSystemParameter(null,
    "circulation",
    "rightsTable",
    innerxml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

    }
}
