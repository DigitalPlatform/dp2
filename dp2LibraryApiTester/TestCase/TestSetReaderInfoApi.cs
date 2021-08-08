using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester
{
    // 测试 SetReaderInfo() API 正确性
    public static class TestSetReaderInfoApi
    {
        public static NormalResult TestAll()
        {
            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            result = TestDeleteReaderRecord("limitFields4");
            if (result.Value == -1) return result;

            result = TestDeleteReaderRecord("limitFields3");
            if (result.Value == -1) return result;

            result = TestDeleteReaderRecord("limitFields2");
            if (result.Value == -1) return result;

            result = TestDeleteReaderRecord("limitFields1");
            if (result.Value == -1) return result;

            result = TestDeleteReaderRecord("");
            if (result.Value == -1) return result;

            result = TestChangeReaderRecord_limitFields("dataFields");
            if (result.Value == -1) return result;

            result = TestChangeReaderRecord_limitFields("importantFields2");
            if (result.Value == -1) return result;

            result = TestChangeReaderRecord_limitFields("importantFields1");
            if (result.Value == -1) return result;

            result = TestChangeReaderRecord_limitFields("");
            if (result.Value == -1) return result;

            result = TestCreateReaderRecord_limitFields("dataFields");
            if (result.Value == -1) return result;

            result = TestCreateReaderRecord_limitFields("importantFields2");
            if (result.Value == -1) return result;

            result = TestCreateReaderRecord_limitFields("importantFields1");
            if (result.Value == -1) return result;

            result = TestCreateReaderRecord_limitFields("");
            if (result.Value == -1) return result;


            // 创建
            result = TestCreateReaderRecord("test_normal");
            if (result.Value == -1) return result;

            // 创建 空条码号
            result = TestCreateReaderRecord_blankBarcode("test_normal");
            if (result.Value == -1) return result;

            // 修改 改成空条码号
            result = TestChangeReaderRecord_blankBarcode("test_normal");
            if (result.Value == -1) return result;

            // 创建 有条码号
            result = TestCreateReaderRecord_barcode("test_normal");
            if (result.Value == -1) return result;

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

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
                            Rights = "setsystemparameter,getreaderinfo,setreaderinfo",
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

                DataModel.SetMessage("正确", "green");
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

        // 创建读者记录
        public static NormalResult TestCreateReaderRecord(string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
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
                    out string saved_recpath,
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

                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    saved_recpath,
    "", // _globalPatronXml,
    null,
    new_timestamp,
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


        // 测试创建读者记录，空条码号情况
        public static NormalResult TestCreateReaderRecord_blankBarcode(string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份创建读者记录(空证条码号) ...");
                long lRet = 0;

                // 允许空证条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?AcceptBlankReaderBarcode",
                    "true",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                string path = _globalPatronDbName + "/?";

                string _xml = @"<root>
<barcode></barcode>
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
                    out string saved_recpath,
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

                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    saved_recpath,
    "", // _globalPatronXml,
    null,
    new_timestamp,
    out string _,
    out string _,
    out string _,
    out byte[] _,
    out ErrorCodeValue _,
    out strError);
                if (lRet == -1)
                    goto ERROR1;


                // 不允许空证条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?AcceptBlankReaderBarcode",
                    "false",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                {
                    path = _globalPatronDbName + "/?";

                    _xml = @"<root>
<barcode></barcode>
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
                        out string _,
                        out string _,
                        out string _,
                        out byte[] _,
                        out ErrorCodeValue _,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.InvalidReaderBarcode)
                            DataModel.SetMessage($"期待中的报错 {strError} errorCode={channel.ErrorCode}");
                        else
                            DataModel.SetMessage($"*** 意外的报错 {strError} errorCode={channel.ErrorCode}", "warning");
                    }
                    else
                    {
                        strError = $"理应创建时报错，但却没有报错";
                        goto ERROR1;
                    }
                }

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
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord_blankBarcode() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord_blankBarcode() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 测试创建读者记录，有条码号情况
        // 有条码号规则
        // (验证条码号规则校验，条码号查重)
        public static NormalResult TestCreateReaderRecord_barcode(string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份创建读者记录(空证条码号) ...");
                long lRet = 0;

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

                // *** 创建第一条读者记录
                DataModel.SetMessage($"正在创建读者记录 {path}");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    _xml,
                    null,
                    null,
                    out string existing_xml,
                    out string saved_xml,
                    out string saved_recpath,
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

                // *** 条码号重复，期待报错
                DataModel.SetMessage($"正在创建重复 barcode 的另一条读者记录 {path}");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    _xml,
                    null,
                    null,
                    out string _,
                    out string _,
                    out string _,
                    out byte[] _,
                    out ErrorCodeValue _,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.ReaderBarcodeDup)
                        DataModel.SetMessage($"期待的报错 {strError} errorCode:{channel.ErrorCode}");
                    else
                        DataModel.SetMessage($"意外的报错 {strError} errorCode:{channel.ErrorCode}", "warning");
                }
                else
                {
                    strError = $"*** 本来期待报错，但却没有报错";
                    goto ERROR1;
                }

                // *** 条码号不符合规则，期待报错
                string _invalid_barcode_xml = @"<root>
<barcode>K0000001</barcode>
<name>李四</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                DataModel.SetMessage($"正在创建 barcode 不符合规则的另一条读者记录 {path}");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    _invalid_barcode_xml,
                    null,
                    null,
                    out string _,
                    out string _,
                    out string _,
                    out byte[] _,
                    out ErrorCodeValue _,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.InvalidReaderBarcode)
                        DataModel.SetMessage($"期待的报错 {strError} errorCode:{channel.ErrorCode}");
                    else
                        DataModel.SetMessage($"意外的报错 {strError} errorCode:{channel.ErrorCode}", "warning");
                }
                else
                {
                    strError = $"*** 本来期待报错，但却没有报错";
                    goto ERROR1;
                }


                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    saved_recpath,
    "", // _globalPatronXml,
    null,
    new_timestamp,
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
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord_barcode() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord_barcode() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        // 创建读者记录，用户只有部分字段权限
        // parameters:
        //      test_case   空
        //                  importantFields1 -- 失败
        //                  importantFields2 -- 成功
        //                  dataFields -- 失败。因为 action 为 new 时，不允许使用 dataFields 属性
        public static NormalResult TestCreateReaderRecord_limitFields(string test_case)
        {
            string strError = "";

            // *** 第一步，创建测试用的账户 test_level
            {
                LibraryChannel manage_channel = DataModel.GetChannel();
                try
                {
                    var user_names = new List<string>() { "test_level" };
                    DataModel.SetMessage($"正在删除可能存在的用户 {StringUtil.MakePathList(user_names, ",")} ...");
                    int nRet = Utility.DeleteUsers(manage_channel,
                        user_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage("正在创建用户 test_level ...");
                    long lRet = manage_channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            UserName = "test_level",
                            Rights = "setreaderinfo:name|readerType,getreaderinfo",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    // 要校验条码号
                    lRet = manage_channel.SetSystemParameter(null,
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
                }
                finally
                {
                    DataModel.ReturnChannel(manage_channel);
                }
            }

            {
                string userName = "test_level";
                LibraryChannel channel = DataModel.NewChannel(userName, "");
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromMinutes(10);

                try
                {
                    DataModel.SetMessage($"正在以用户 {userName} 身份创建读者记录 ...");
                    long lRet = 0;

                    string path = _globalPatronDbName + "/?";

                    string _xml = "";
                    if (string.IsNullOrEmpty(test_case))
                        _xml = @"<root>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                    else if (test_case == "importantFields1")
                        _xml = @"<root importantFields='barcode,name'>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                    else if (test_case == "importantFields2")
                        _xml = @"<root importantFields='name,readerType'>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                    else if (test_case == "dataFields")
                        _xml = @"<root dataFields='barcode,name,readerType'>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";
                    else
                        throw new Exception($"未知的 test_case '{test_case}'");

                    DataModel.SetMessage($"正在创建读者记录 {path}");
                    lRet = channel.SetReaderInfo(null,
                        "new",
                        path,
                        _xml,
                        null,
                        null,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue kernel_errorcode,
                        out strError);
                    if (string.IsNullOrEmpty(test_case))
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (test_case == "importantFields1")
                    {
                        if (lRet == -1)
                            DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{channel.ErrorCode}");
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }
                    else if (test_case == "importantFields2")
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (test_case == "dataFields")
                    {
                        if (lRet == -1)
                            DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{channel.ErrorCode}");
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }

                    if (string.IsNullOrEmpty(test_case)
                        || test_case == "importantFields2")
                    {
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

                        LibraryChannel manage_channel = DataModel.GetChannel();
                        try
                        {
                            // 删除读者记录
                            lRet = manage_channel.SetReaderInfo(null,
            "delete",
            saved_recpath,
            "", // _globalPatronXml,
            null,
            new_timestamp,
            out string _,
            out string _,
            out string _,
            out byte[] _,
            out ErrorCodeValue _,
            out strError);
                            if (lRet == -1)
                                goto ERROR1;
                        }
                        finally
                        {
                            DataModel.ReturnChannel(manage_channel);
                        }
                    }

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
                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    strError = "TestCreateReaderRecord_limitFields() Exception: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    DataModel.DeleteChannel(channel);
                }

            }
        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord_limitFields() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        // 测试修改读者记录，空册条码号情况
        public static NormalResult TestChangeReaderRecord_blankBarcode(string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份修改读者记录(空证条码号) ...");
                long lRet = 0;

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
                DataModel.SetMessage($"正在创建读者记录 {path}。这是为后面修改做准备");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    _xml,
                    null,
                    null,
                    out string existing_xml,
                    out string saved_xml,
                    out string saved_recpath,
                    out byte[] new_timestamp,
                    out ErrorCodeValue kernel_errorcode,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 允许空证条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?AcceptBlankReaderBarcode",
                    "true",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 制造一个具有空条码号的记录
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(saved_xml);

                DomUtil.DeleteElement(dom.DocumentElement, "barcode");

                DataModel.SetMessage($"正在修改读者记录 {saved_recpath}。修改后的记录没有条码号字段");
                lRet = channel.SetReaderInfo(null,
                    "change",
                    saved_recpath,
                    dom.OuterXml,
                    null,
                    new_timestamp,
                    out string _,
                    out saved_xml,
                    out string _,
                    out new_timestamp,
                    out ErrorCodeValue _,
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

                // 不允许空证条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?AcceptBlankReaderBarcode",
                    "false",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                {
                    DataModel.SetMessage($"正在修改读者记录 {saved_recpath}。修改后的读者记录没有证条码号字段");
                    lRet = channel.SetReaderInfo(null,
                        "change",
                        saved_recpath,
                        dom.OuterXml,
                        null,
                        new_timestamp,
                        out string _,
                        out string _,
                        out string _,
                        out byte[] _,
                        out ErrorCodeValue _,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.InvalidReaderBarcode)
                            DataModel.SetMessage($"期待中的报错 {strError} errorCode={channel.ErrorCode}");
                        else
                            DataModel.SetMessage($"*** 意外的报错 {strError} errorCode={channel.ErrorCode}", "warning");
                    }
                    else
                    {
                        strError = $"理应修改时报错，但却没有报错";
                        goto ERROR1;
                    }
                }

                // 删除读者记录
                lRet = channel.SetReaderInfo(null,
    "delete",
    saved_recpath,
    "", // _globalPatronXml,
    null,
    new_timestamp,
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
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord_blankBarcode() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord_blankBarcode() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 测试修改读者记录，限制字段权限情况
        public static NormalResult TestChangeReaderRecord_limitFields(string test_case)
        {
            string strError = "";

            // *** 第一步，创建测试用的账户 test_level
            LibraryChannel manage_channel = DataModel.GetChannel();
            try
            {
                var user_names = new List<string>() { "test_level" };
                DataModel.SetMessage($"正在删除可能存在的用户 {StringUtil.MakePathList(user_names, ",")} ...");
                int nRet = Utility.DeleteUsers(manage_channel,
                    user_names,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string rights = "";
                if (string.IsNullOrEmpty(test_case))
                {
                    rights = "setreaderinfo:barcode|name|readerType,getreaderinfo";
                }
                else if (test_case == "importantFields1")
                {
                    // 注: 缺少 barcode 元素
                    rights = "setreaderinfo:name|readerType,getreaderinfo";
                }
                else if (test_case == "importantFields2")
                {
                    // 注: 缺少 barcode 元素
                    rights = "setreaderinfo:name|readerType,getreaderinfo";
                }
                else if (test_case == "dataFields")
                {
                    // 注: 包含 barcode 元素
                    rights = "setreaderinfo:barcode|name|readerType,getreaderinfo";
                }
                else
                    throw new Exception($"未知的 test_case '{test_case}'");

                DataModel.SetMessage("正在创建用户 test_level ...");
                long lRet = manage_channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_level",
                        Rights = rights,
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 要校验条码号
                lRet = manage_channel.SetSystemParameter(null,
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


                string userName = "test_level";
                LibraryChannel test_channel = DataModel.NewChannel(userName, "");
                TimeSpan old_timeout = test_channel.Timeout;
                test_channel.Timeout = TimeSpan.FromMinutes(10);

                try
                {
                    DataModel.SetMessage($"正在以用户 {userName} 身份修改读者记录(空证条码号) ...");

                    string path = _globalPatronDbName + "/?";
                    string _xml = @"<root>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";

                    DataModel.SetMessage($"正在创建读者记录 {path}。这是为后面修改做准备");
                    lRet = manage_channel.SetReaderInfo(null,
                        "new",
                        path,
                        _xml,
                        null,
                        null,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue kernel_errorcode,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    // 制造一个改变了条码号的记录
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(saved_xml);

                    DomUtil.SetElementText(dom.DocumentElement, "barcode", "G0000002");

                    if (string.IsNullOrEmpty(test_case))
                    {
                    }
                    else if (test_case == "importantFields1")
                    {
                        dom.DocumentElement.SetAttribute("importantFields", "barcode");
                    }
                    else if (test_case == "importantFields2")
                    {
                        dom.DocumentElement.SetAttribute("importantFields", "name");
                    }
                    else if (test_case == "dataFields")
                    {
                        // 故意在请求的读者记录里面少 barcode 元素
                        DomUtil.DeleteElement(dom.DocumentElement, "barcode");

                        // 申明 barcode 元素并不在传递的元素之列。这样避免了后面修改保存时 barcode 元素被删除
                        dom.DocumentElement.SetAttribute("dataFields", "name,readerType");
                    }

                    DataModel.SetMessage($"正在修改读者记录 {saved_recpath}。记录中条码号字段被修改");
                    lRet = test_channel.SetReaderInfo(null,
                        "change",
                        saved_recpath,
                        dom.OuterXml,
                        null,
                        new_timestamp,
                        out string _,
                        out saved_xml,
                        out string _,
                        out new_timestamp,
                        out ErrorCodeValue _,
                        out strError);
                    if (string.IsNullOrEmpty(test_case))
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (test_case == "importantFields1")
                    {
                        if (lRet == -1)
                            DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{test_channel.ErrorCode}");
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }
                    else if (test_case == "importantFields2")
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }


                    if (string.IsNullOrEmpty(test_case)
    || test_case == "importantFields2"
    || test_case == "dataFields")
                    {
                        // 验证读者记录是否修改成功
                        lRet = manage_channel.GetReaderInfo(null,
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

                            XmlDocument changed_dom = new XmlDocument();
                            changed_dom.LoadXml(xml);

                            if (string.IsNullOrEmpty(test_case))
                            {
                                string barcode = DomUtil.GetElementText(changed_dom.DocumentElement, "barcode");
                                if (barcode == "G0000002")
                                    DataModel.SetMessage("修改后的记录验证正确");
                                else
                                {
                                    strError = $"修改后的读者记录，条码号期待为 'G0000002' 但却为 '{barcode}'";
                                    goto ERROR1;
                                }
                            }
                            else if (test_case == "importantFields2")
                            {
                                string barcode = DomUtil.GetElementText(changed_dom.DocumentElement, "barcode");
                                if (barcode == "G0000001")
                                    DataModel.SetMessage("修改后的记录验证正确(也就是说条码号没有变化)");
                                else
                                {
                                    strError = $"修改后的读者记录，条码号期待为 'G0000001'(也就是说条码号没有变化) 但实际上却为 '{barcode}'";
                                    goto ERROR1;
                                }
                            }
                            else if (test_case == "dataFields")
                            {
                                string barcode = DomUtil.GetElementText(changed_dom.DocumentElement, "barcode");
                                if (barcode == "G0000001")
                                    DataModel.SetMessage("修改后的记录验证正确(也就是说条码号没有变化)");
                                else
                                {
                                    strError = $"修改后的读者记录，条码号期待为 'G0000001'(也就是说条码号没有变化) 但实际上却为 '{barcode}'";
                                    goto ERROR1;
                                }
                            }
                        }
                    }

                    // 删除读者记录
                    lRet = manage_channel.SetReaderInfo(null,
        "delete",
        saved_recpath,
        "", // _globalPatronXml,
        null,
        new_timestamp,
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
                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    strError = "TestCreateReaderRecord_limitFields() Exception: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }
                finally
                {
                    test_channel.Timeout = old_timeout;
                    DataModel.DeleteChannel(test_channel);
                }
            }
            finally
            {
                DataModel.ReturnChannel(manage_channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord_limitFields() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 测试删除读者记录，特别是限制字段权限情况
        public static NormalResult TestDeleteReaderRecord(string test_case)
        {
            string strError = "";

            // *** 第一步，创建测试用的账户 test_level
            LibraryChannel manage_channel = DataModel.GetChannel();
            try
            {
                var user_names = new List<string>() { "test_level" };
                DataModel.SetMessage($"正在删除可能存在的用户 {StringUtil.MakePathList(user_names, ",")} ...");
                int nRet = Utility.DeleteUsers(manage_channel,
                    user_names,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string rights = "";
                if (string.IsNullOrEmpty(test_case))
                {
                    // 完整的 setreaderinfo 权限
                    rights = "setreaderinfo";
                }
                else if (test_case == "limitFields1")
                {
                    // 注: 缺少 barcode 元素
                    rights = "setreaderinfo:name|readerType";
                }
                else if (test_case == "limitFields2")
                {
                    // 注: 包含数据中的全部元素，和 r_delete
                    rights = "setreaderinfo:barcode|name|readerType|department|r_delete";
                }
                else if (test_case == "limitFields3")
                {
                    // 注: 包含数据中的全部元素，但缺乏 r_delete
                    rights = "setreaderinfo:barcode|name|readerType|department";
                }
                else if (test_case == "limitFields4")
                {
                    // 注: 包含数据中的部分元素，和 r_delete
                    rights = "setreaderinfo:barcode|name|readerType|r_delete";
                }
                else
                    throw new Exception($"未知的 test_case '{test_case}'");

                DataModel.SetMessage("正在创建用户 test_level ...");
                long lRet = manage_channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_level",
                        Rights = rights,
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 要校验条码号
                lRet = manage_channel.SetSystemParameter(null,
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


                string userName = "test_level";
                LibraryChannel test_channel = DataModel.NewChannel(userName, "");
                TimeSpan old_timeout = test_channel.Timeout;
                test_channel.Timeout = TimeSpan.FromMinutes(10);

                try
                {
                    DataModel.SetMessage($"正在以用户 {userName} 身份修改读者记录(空证条码号) ...");

                    string path = _globalPatronDbName + "/?";
                    string _xml = @"<root>
<barcode>G0000001</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";

                    DataModel.SetMessage($"正在创建读者记录 {path}。这是为后面删除做准备");
                    lRet = manage_channel.SetReaderInfo(null,
                        "new",
                        path,
                        _xml,
                        null,
                        null,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue kernel_errorcode,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage($"正在删除读者记录 {saved_recpath}");
                    lRet = test_channel.SetReaderInfo(null,
                        "delete",
                        saved_recpath,
                        null,
                        saved_xml,
                        new_timestamp,
                        out string _,
                        out saved_xml,
                        out string _,
                        out new_timestamp,
                        out ErrorCodeValue _,
                        out strError);
                    if (string.IsNullOrEmpty(test_case))
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (test_case == "limitFields1")
                    {
                        if (lRet == -1)
                        {
                            if (test_channel.ErrorCode == ErrorCode.AccessDenied)
                                DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{test_channel.ErrorCode}");
                            else
                            {
                                strError = ($"不是期待中的错误码 AccessDenied。{strError} errorCode:{test_channel.ErrorCode}");
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }
                    else if (test_case == "limitFields2")
                    {
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    else if (test_case == "limitFields3")
                    {
                        if (lRet == -1)
                        {
                            if (test_channel.ErrorCode == ErrorCode.AccessDenied)
                                DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{test_channel.ErrorCode}");
                            else
                            {
                                strError = ($"不是期待中的错误码 AccessDenied。{strError} errorCode:{test_channel.ErrorCode}");
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }
                    else if (test_case == "limitFields4")
                    {
                        if (lRet == -1)
                        {
                            if (test_channel.ErrorCode == ErrorCode.AccessDenied)
                                DataModel.SetMessage($"期待中的返回出错。{strError} errorCode:{test_channel.ErrorCode}");
                            else
                            {
                                strError = ($"不是期待中的错误码 AccessDenied。{strError} errorCode:{test_channel.ErrorCode}");
                                goto ERROR1;
                            }
                        }
                        else
                        {
                            strError = $"期待 SetReaderInfo() 返回出错，但返回了成功";
                            goto ERROR1;
                        }
                    }

                    if (string.IsNullOrEmpty(test_case)
    || test_case == "limitFields1"
    || test_case == "limitFields2"
    || test_case == "limitFields3"
    || test_case == "limitFields4")
                    {
                        // 验证读者记录是否删除成功
                        lRet = manage_channel.GetReaderInfo(null,
                        "@path:" + saved_recpath,
                        "xml",
                        out string[] results,
                        out strError);
                        if (lRet == -1)
                        {
                            strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                            goto ERROR1;
                        }
                        if (string.IsNullOrEmpty(test_case))
                        {
                            if (lRet == 0)
                                DataModel.SetMessage("删除成功");
                            else
                            {
                                strError = "删除不成功，读者记录依然存在";
                                goto ERROR1;
                            }
                        }
                        else if (test_case == "limitFields1")
                        {
                            if (lRet == -1 || lRet == 0)
                            {
                                strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                                goto ERROR1;
                            }

                            // TODO: 验证字段是否完好无损
                        }
                        else if (test_case == "limitFields2")
                        {
                            if (lRet == 0)
                                DataModel.SetMessage("删除成功");
                            else
                            {
                                strError = "删除不成功，读者记录依然存在";
                                goto ERROR1;
                            }
                        }
                        else if (test_case == "limitFields3")
                        {
                            if (lRet == -1 || lRet == 0)
                            {
                                strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                                goto ERROR1;
                            }

                            // TODO: 验证字段是否完好无损
                        }
                        else if (test_case == "limitFields4")
                        {
                            if (lRet == -1 || lRet == 0)
                            {
                                strError = $"读者记录 '{saved_recpath}' 验证获取时出错: {strError}";
                                goto ERROR1;
                            }

                            // TODO: 验证字段是否完好无损
                        }
                    }

                    // 最后补一次删除读者记录
                    lRet = manage_channel.SetReaderInfo(null,
        "delete",
        saved_recpath,
        "", // _globalPatronXml,
        null,
        new_timestamp,
        out string _,
        out string _,
        out string _,
        out byte[] _,
        out ErrorCodeValue _,
        out strError);
                    if (lRet == -1
                        && !(manage_channel.ErrorCode == ErrorCode.NotFound || manage_channel.ErrorCode == ErrorCode.ReaderBarcodeNotFound))
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
                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    strError = "TestDeleteReaderRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                    goto ERROR1;
                }
                finally
                {
                    test_channel.Timeout = old_timeout;
                    DataModel.DeleteChannel(test_channel);
                }
            }
            finally
            {
                DataModel.ReturnChannel(manage_channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestDeleteReaderRecord() error: {strError}", "error");
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

    }
}
