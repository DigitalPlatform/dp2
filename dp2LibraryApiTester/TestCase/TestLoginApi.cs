using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;

namespace dp2LibraryApiTester
{
    // 测试 Login() API
    public static class TestLoginApi
    {
        public static NormalResult TestAll(CancellationToken token)
        {
            NormalResult result = null;
            /*
            result = TestChannelLeakLogin(token);
            if (result.Value == -1)
                return result;

            result = TestLoopLogin(token);
            if (result.Value == -1)
                return result;
            */

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            result = TestReaderLogin("password_expired");
            if (result.Value == -1) return result;

            result = TestReaderLogin("");
            if (result.Value == -1) return result;

#if REMOVED
            {
                DataModel.SetMessage("=== 开始 读者 token 登录测试 ===");


                result = TestCreateReaderRecord("test_normal",
        "password",
        out string new_recpath,
        out byte[] timestamp);
                if (result.Value == -1) return result;

                /*
                // 以读者身份普通密码登录
                {
                    result = TestLogin("G0000001",
                        "20250913",
                        "G0000001",
                        null);
                    if (result.Value == -1) return result;

                    DataModel.SetMessage("读者身份 普通密码 登录正确", "green");
                }
                */

                // 以读者身份 token 方式登录
                {
                    result = TestLoginGetToken("test_normal",
                        "password",
                        "G0000001",
                        out string login_token);
                    if (result.Value == -1) return result;

                    if (string.IsNullOrEmpty(login_token))
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "获得的 login token 不应为空"
                        };
                    }

                    {
                        result = TestLogin("test_normal",
                            "password",
                            "G0000001",
                            login_token);
                        if (result.Value == -1) return result;

                        DataModel.SetMessage("读者身份 token 登录正确", "green");
                    }
                }

                result = TestDeleteReaderRecord("test_normal",
                    "password",
                    new_recpath,
                    timestamp);
                if (result.Value == -1) return result;

                DataModel.SetMessage("=== 结束 读者 token 登录测试 ===");

            }
#endif
            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

        static NormalResult TestReaderLogin(string style)
        {
            var password_expired = StringUtil.IsInList("password_expired", style);

            NormalResult result = new NormalResult();

            DataModel.SetMessage($"=== 开始 读者 token 登录测试 {style} ===", "begin");


            result = TestCreateReaderRecord("test_normal",
    "password",
    style,
    out string new_recpath,
    out byte[] timestamp);
            if (result.Value == -1) return result;

            // 以读者身份普通密码登录
            {
                result = TestLogin("G0000001",
                    "20250913",
                    "G0000001",
                    null);
                if (password_expired)
                {
                    if (result.ErrorCode != "PasswordExpired")
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"TestLogin() 本应 expire 失败才行，但现在返回 {result.ToString()}"
                        };
                }
                else
                {
                    if (result.Value == -1) return result;
                }

                DataModel.SetMessage("读者身份 普通密码 登录符合预期", "green");
            }

            // 以读者身份 token 方式登录
            {
                result = TestLoginGetToken("test_normal",
                    "password",
                    "G0000001",
                    out string login_token);
                if (result.Value == -1) return result;

                if (string.IsNullOrEmpty(login_token))
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "获得的 login token 不应为空"
                    };
                }

                {
                    result = TestLogin("test_normal",
                        "password",
                        "G0000001",
                        login_token);
                    if (result.Value == -1) return result;

                    DataModel.SetMessage("读者身份 token 登录符合预期", "green");
                }
            }

            result = TestDeleteReaderRecord("test_normal",
                "password",
                new_recpath,
                timestamp);
            if (result.Value == -1) return result;

            DataModel.SetMessage($"=== 结束 读者 token 登录测试 {style} ===", "end");

            return new NormalResult();
        }

        public static NormalResult TestLoopLogin(CancellationToken token)
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;

            try
            {
                int loops = 20 * 10;
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
                    if ((i % 10) == 0)
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
                strError = "TestLoopLogin() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestLoopLogin() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 模拟通道泄露
        public static NormalResult TestChannelLeakLogin(CancellationToken token)
        {
            string strError = "";

            DataModel.MaxPoolChannelCount = 300;

            List<LibraryChannel> channels = new List<LibraryChannel>();

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

                    // 注: 这里 GetChannel() 没有配套的 ReturnChannel()，所以会发生通道泄露
                    LibraryChannel channel = DataModel.GetChannel();
                    channels.Add(channel);

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
            finally
            {
                channels.ForEach((c) =>
                {
                    DataModel.ReturnChannel(c);
                });
                DataModel.Clear();
            }

        ERROR1:
            DataModel.SetMessage($"TestChannelLeakLogin() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        #region 读者记录相关

        static string _globalPatronDbName = "_总馆读者";
        static string _globalLibraryCode = "";

        static string _globalPatronXml = @"<root>
<barcode>R9999998</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";

        static string _haidianPatronDbName = "_海淀读者";
        static string _haidianLibraryCode = "海淀分馆";

        static string _haidianPatronXml = @"<root>
<barcode>R9999997</barcode>
<name>海淀读者</name>
<readerType>教授</readerType>
<department>物理系</department>
</root>";

        static string _xichengPatronDbName = "_西城读者";
        static string _xichengLibraryCode = "西城分馆";

        static string _xichengPatronXml = @"<root>
<barcode>R9999996</barcode>
<name>西城读者</name>
<readerType>博士生</readerType>
<department>化学系</department>
</root>";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            DataModel.SetMessage("开始", "begin");

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                // *** 创建三个读者库

                // 如果测试用的读者库以前就存在，要先删除。
                DataModel.SetMessage("正在删除测试用读者库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _globalPatronDbName + "," + _haidianPatronDbName + "," + _xichengPatronDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                {
                    DataModel.SetMessage($"正在创建测试用读者库 '{_globalPatronDbName}'...");
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    int nRet = ManageHelper.CreateReaderDatabase(
                        channel,
                        null,
                        _globalPatronDbName,
                        _globalLibraryCode,
                        true,
                        "",
                        out strError);
                    if (nRet != 1)
                        goto ERROR1;
                }

                {
                    DataModel.SetMessage($"正在创建测试用读者库 '{_haidianPatronDbName}'...");
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    int nRet = ManageHelper.CreateReaderDatabase(
                        channel,
                        null,
                        _haidianPatronDbName,
                        _haidianLibraryCode,
                        true,
                        "",
                        out strError);
                    if (nRet != 1)
                        goto ERROR1;
                }

                {
                    DataModel.SetMessage($"正在创建测试用读者库 '{_xichengPatronDbName}'...");
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    int nRet = ManageHelper.CreateReaderDatabase(
                        channel,
                        null,
                        _xichengPatronDbName,
                        _xichengLibraryCode,
                        true,
                        "",
                        out strError);
                    if (nRet != 1)
                        goto ERROR1;
                }

                // *** 创建总馆用户若干
                {
                    var user_names = new List<string>() { "test_cannot", "test_normal", "test_level1", "test_access1" };
                    DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                    int nRet = Utility.DeleteUsers(channel,
                        user_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 创建几个测试用工作人员账户

                    DataModel.SetMessage("正在创建用户 test_normal ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            UserName = "test_normal",
                            Password = "password",
                            SetPassword = true,
                            Rights = "setsystemparameter,getreaderinfo,setreaderinfo,simulatereader",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 test_cannot ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            UserName = "test_cannot",
                            Rights = "searchreader,search",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 test_level1 ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            UserName = "test_level1",
                            Rights = "searchreader,search,getreaderinfo:1",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // *** 创建海淀用户若干
                {
                    var user_names = new List<string>() { "_haidian_cannot", "_haidian_normal", "_haidian_level1", "_haidian_access1" };
                    DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                    int nRet = Utility.DeleteUsers(channel,
                        user_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 创建几个测试用工作人员账户

                    DataModel.SetMessage("正在创建用户 _haidian_normal ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _haidianLibraryCode,
                            UserName = "_haidian_normal",
                            Rights = "searchreader,search,getreaderinfo",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 _haidian_cannot ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _haidianLibraryCode,
                            UserName = "_haidian_cannot",
                            Rights = "searchreader,search",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 _haidian_level1 ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _haidianLibraryCode,
                            UserName = "_haidian_level1",
                            Rights = "searchreader,search,getreaderinfo:1",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // *** 创建西城用户若干
                {
                    var user_names = new List<string>() { "_xicheng_cannot", "_xicheng_normal", "_xicheng_level1", "_xicheng_access1" };
                    DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                    int nRet = Utility.DeleteUsers(channel,
                        user_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 创建几个测试用工作人员账户

                    DataModel.SetMessage("正在创建用户 _xicheng_normal ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _xichengLibraryCode,
                            UserName = "_xicheng_normal",
                            Rights = "searchreader,search,getreaderinfo",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 _xicheng_cannot ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _xichengLibraryCode,
                            UserName = "_xicheng_cannot",
                            Rights = "searchreader,search",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 _xicheng_level1 ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            LibraryCode = _xichengLibraryCode,
                            UserName = "_xicheng_level1",
                            Rights = "searchreader,search,getreaderinfo:1",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // DataModel.SetMessage("正确", "green");
                DataModel.SetMessage("环境已经准备好");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "PrepareEnvironment() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult Finish()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用读者库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _globalPatronDbName + "," + _haidianPatronDbName + "," + _xichengPatronDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                var user_names = new List<string>() { "test_cannot", "test_normal", "test_level1", "test_access1" };
                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                int nRet = Utility.DeleteUsers(channel,
                    user_names,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                DataModel.SetMessage("结束", "end");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "Finish() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 创建读者记录
        // parameters:
        //      style   如果包含 password_expired，则表示创建的读者密码是立即失效的
        public static NormalResult TestCreateReaderRecord(
            string userName,
            string password,
            string style,
            out string saved_recpath,
            out byte[] saved_timestamp)
        {
            saved_recpath = "";
            saved_timestamp = null;
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, password);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份创建读者记录 ...");
                long lRet = 0;

                // 要校验条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?VerifyBarcode",
                    "true",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                var password_expired = StringUtil.IsInList("password_expired", style);

                if (password_expired)
                {
                    // 读者账户密码启用临时失效
                    lRet = channel.SetSystemParameter(null,
        "login",
        "?patronPasswordExpireLength",
        "90day",
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                else
                {
                    // 读者密码不失效
                    lRet = channel.SetSystemParameter(null,
"login",
"?patronPasswordExpireLength",
"",
out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // 设置条码号校验规则
                int ret = Utility.SetBarcodeValidation(
                    @"<validator location=',流通库,测试库,智能书柜,阅览室,保存本库'>
        <patron>
            <CMIS />
            <range value='G0000001-G9999999' />
        </patron>
    </validator>
    <validator location='海淀分馆*' >
        <patron>
            <CMIS />
            <range value='H0000001-H9999999' />
        </patron>
    </validator>
    <validator location='西城分馆*' >
        <patron>
            <CMIS />
            <range value='X0000001-X9999999' />
        </patron>
    </validator>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;

                // 设置借还权限。这样读者类型和图书类型就都定下来了
                ret = Utility.SetRightsTable(
                    @"    <type reader='本科生'>
        <param name='可借总册数' value='10' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='2' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='硕士生'>
        <param name='可借总册数' value='15' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='3' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='博士生'>
        <param name='可借总册数' value='20' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='4' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='讲师'>
        <param name='可借总册数' value='20' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='教授'>
        <param name='可借总册数' value='30' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='6' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
        <item>硕士生</item>
        <item>博士生</item>
        <item>讲师</item>
        <item>教授</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
        <item>教材</item>
        <item>教学参考</item>
        <item>原版西文</item>
    </bookTypes>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;

                string path = _globalPatronDbName + "/?";

                // TODO: 如何让创建的读者记录的密码一开始就超过失效期?
                // 密码为生日 "20250913"
                string patron_xml = @"<root>
<barcode>G0000001</barcode>
<name>张三</name>
<dateOfBirth>Sat, 13 Sep 2025 18:41:23 +0800</dateOfBirth>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(patron_xml);
                    if (password_expired)
                        dom.DocumentElement.SetAttribute("passwordExpireLength", "00:00:00");
                    patron_xml = dom.DocumentElement.OuterXml;
                }

                DataModel.SetMessage($"正在创建读者记录 {path}");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    patron_xml,
                    null,
                    null,
                    out string existing_xml,
                    out string saved_xml,
                    out saved_recpath,
                    out saved_timestamp,
                    out ErrorCodeValue kernel_errorcode,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

#if REMOVED
                // 验证读者记录是否创建成功
                lRet = channel.GetReaderInfo(null,
                    "@path:" + saved_recpath,
                    "xml",
                    out string[] results,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                    goto ERROR1;
                }

                string xml = results[0];

                // 验证
                {
                    xml = DomUtil.GetIndentXml(xml);
                    DataModel.SetMessage($"path={saved_recpath}");
                    DataModel.SetMessage($"xml=\r\n{xml}");
                }

                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    saved_recpath,
    "", // _globalPatronXml,
    null,
    saved_timestamp,
    out string _,
    out string _,
    out string _,
    out byte[] _,
    out ErrorCodeValue _,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                /*
                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                */
                DataModel.SetMessage("正确", "green");
#endif
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult TestDeleteReaderRecord(string userName,
            string password,
            string recpath,
            byte[] timestamp)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, password);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份删除读者记录 ...");
                long lRet = 0;

#if REMOVED
                // 要校验条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?VerifyBarcode",
                    "true",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 设置条码号校验规则
                int ret = Utility.SetBarcodeValidation(
                    @"<validator location=',流通库,测试库,智能书柜,阅览室,保存本库'>
        <patron>
            <CMIS />
            <range value='G0000001-G9999999' />
        </patron>
    </validator>
    <validator location='海淀分馆*' >
        <patron>
            <CMIS />
            <range value='H0000001-H9999999' />
        </patron>
    </validator>
    <validator location='西城分馆*' >
        <patron>
            <CMIS />
            <range value='X0000001-X9999999' />
        </patron>
    </validator>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;

                // 设置借还权限。这样读者类型和图书类型就都定下来了
                ret = Utility.SetRightsTable(
                    @"    <type reader='本科生'>
        <param name='可借总册数' value='10' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='2' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='硕士生'>
        <param name='可借总册数' value='15' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='3' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='博士生'>
        <param name='可借总册数' value='20' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='4' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='讲师'>
        <param name='可借总册数' value='20' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <type reader='教授'>
        <param name='可借总册数' value='30' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教材'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='教学参考'>
            <param name='可借册数' value='10' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
        <type book='原版西文'>
            <param name='可借册数' value='6' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='3.0' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
        <item>硕士生</item>
        <item>博士生</item>
        <item>讲师</item>
        <item>教授</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
        <item>教材</item>
        <item>教学参考</item>
        <item>原版西文</item>
    </bookTypes>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;

                string path = _globalPatronDbName + "/?";

                string _xml = @"<root>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                DataModel.SetMessage($"正在创建读者记录 {path}");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    _xml,
                    null,
                    null,
                    out string existing_xml,
                    out string saved_xml,
                    out saved_recpath,
                    out byte[] new_timestamp,
                    out ErrorCodeValue kernel_errorcode,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 验证读者记录是否创建成功
                lRet = channel.GetReaderInfo(null,
                    "@path:" + saved_recpath,
                    "xml",
                    out string[] results,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                    goto ERROR1;
                }

                string xml = results[0];

                // 验证
                {
                    xml = DomUtil.GetIndentXml(xml);
                    DataModel.SetMessage($"path={saved_recpath}");
                    DataModel.SetMessage($"xml=\r\n{xml}");
                }

#endif

                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    recpath,
    "", // _globalPatronXml,
    null,
    timestamp,
    out string _,
    out string _,
    out string _,
    out byte[] _,
    out ErrorCodeValue _,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestDeleteReaderRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestDeleteReaderRecord() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 登录。工作人员登录，或者工作人员代理读者身份登录
        // parameters:
        //      patronBarcode 读者证条码号。如果为空，表示工作人员登录
        //      token 如果不为空，表示使用 token 登录。否则为普通密码登录
        public static NormalResult TestLogin(string userName,
            string password,
            string patronBarcode,
            string token)
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;

            try
            {
                string strUserName = "";
                string strPassword = "";
                string strParameters = "";
                if (string.IsNullOrEmpty(patronBarcode) == false
                    && string.IsNullOrEmpty(token) == false)
                {
                    // 读者被代理 token 登录
                    strUserName = patronBarcode;
                    strPassword = userName + "," + password + "|||token:" + token;
                    strParameters = "location=#test,type=reader,index=-1,client=dp2libraryapitester|0.01";
                    strParameters += ",simulate=yes";
                }
                else if (string.IsNullOrEmpty(patronBarcode) == false
    && string.IsNullOrEmpty(token) == true)
                {
                    // 读者普通密码登录
                    strUserName = patronBarcode;
                    strPassword = password;
                    strParameters = "location=#test,type=reader,index=-1,client=dp2libraryapitester|0.01";
                }
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(patronBarcode));
                    Debug.Assert(string.IsNullOrEmpty(userName) == false);

                    // 工作人员普通密码登录
                    strUserName = userName;
                    strPassword = password;
                    strParameters = "location=#test,type=worker,client=dp2libraryapitester|0.01";
                }

                {
                    var ret = channel.Login(patronBarcode,
                        strPassword,
                        strParameters,
                        out strError);
                    if (ret == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.PasswordExpired)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError,
                                ErrorCode = "PasswordExpired"
                            };
                        goto ERROR1;
                    }
                    if (ret == 0)
                        goto ERROR1;

                    ret = channel.Logout(out strError);
                    if (ret == -1)
                        goto ERROR1;
                }
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
            DataModel.SetMessage($"TestLogin() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // parameters:
        //      patronBarcode 读者证条码号。如果为空，表示工作人员登录
        public static NormalResult TestLoginGetToken(string userName,
    string password,
    string patronBarcode,
    out string token)
        {
            string strError = "";
            token = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;

            try
            {
                string strUserName = "";
                string strPassword = "";
                string strParameters = "location=#test,type=worker,gettoken=day,client=dp2libraryapitester|0.01";

                if (string.IsNullOrEmpty(patronBarcode) == false)
                {
                    strUserName = patronBarcode;
                    strPassword = userName + "," + password;
                    strParameters = "location=#test,type=reader,index=-1,gettoken=day,client=dp2libraryapitester|0.01";
                    strParameters += ",simulate=yes";
                }
                else
                {
                    strUserName = userName;
                    strPassword = password;
                    strParameters = "location=#test,type=worker,gettoken=day,client=dp2libraryapitester|0.01";
                }

                {
                    var ret = channel.Login(strUserName,
                        strPassword,
                        strParameters,
                        out string strOutputUserName,
                        out string strRights,
                        out string strLibraryCode,
                        out strError);
                    if (ret == -1)
                        goto ERROR1;
                    if (ret == 0)
                        goto ERROR1;

                    token = StringUtil.GetParameterByPrefix(strRights, "token");
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestLoginGetToken() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestLoginGetToken() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        #endregion
    }
}
