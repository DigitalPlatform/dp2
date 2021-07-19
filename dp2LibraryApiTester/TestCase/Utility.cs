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
            DataModel.SetMessage("**********************************");
            foreach (string error in errors)
            {
                DataModel.SetMessage($"!!! {error} !!!");
            }
            DataModel.SetMessage("**********************************");
        }

        public static int DeleteUsers(LibraryChannel channel,
    List<string> user_names,
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

    }
}
