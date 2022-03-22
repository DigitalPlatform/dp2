using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;

namespace dp2LibraryApiTester
{
    // 测试 Login() API
    public static class TestLoginApi
    {
        public static NormalResult TestAll(CancellationToken token)
        {
            var result = TestChannelLeakLogin(token);
            if (result.Value == -1)
                return result;

            return new NormalResult();
        }

        public static NormalResult TestLogin(CancellationToken token)
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;

            try
            {
                int loops = 20 * 10000;
                string strParameters = "location=#test,type=worker,client=dp2libraryapitester|0.01";
                for (int i = 0; i < loops; i++)
                {
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断",
                            ErrorCode = "Canceled"
                        };
                    if ((i % 100) == 0)
                        DataModel.SetMessage($"正在进行 Login() API 测试 ({i}/{loops})");

                    var ret = channel.Login(DataModel.dp2libraryUserName,
                        DataModel.dp2libraryPassword,
                        strParameters,
                        out strError);
                    if (ret == -1)
                        goto ERROR1;
                    if (ret == 0)
                        goto ERROR1;
                }

                DataModel.SetMessage($"Login() 测试成功");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestLogin() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestLogin() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult TestChannelLeakLogin(CancellationToken token)
        {
            string strError = "";

            try
            { 
                int loops = 20 * 10000;
                string strParameters = "location=#test,type=worker,client=dp2libraryapitester|0.01";
                for (int i = 0; i < loops; i++)
                {
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断",
                            ErrorCode = "Canceled"
                        };
                    // if ((i % 100) == 0)
                        DataModel.SetMessage($"正在进行 Login() API 测试 ({i}/{loops})");

                    LibraryChannel channel = DataModel.GetChannel();

                    var ret = channel.Login(DataModel.dp2libraryUserName,
                        DataModel.dp2libraryPassword,
                        strParameters,
                        out strError);
                    if (ret == -1)
                        goto ERROR1;
                    if (ret == 0)
                        goto ERROR1;
                }

                DataModel.SetMessage($"Login() 测试成功");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestChannelLeakLogin() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }

        ERROR1:
            DataModel.SetMessage($"TestChannelLeakLogin() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }
    }
}
