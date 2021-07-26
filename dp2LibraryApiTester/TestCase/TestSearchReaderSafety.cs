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

// TODO: 创建一个具有 Access 存取定义的账户，限定读者库访问的那种，然后进行检索验证
// TODO: 为读者库准备好机构代码参数，然后测试验证返回的读者 XML 记录中是否有 oi 元素
namespace dp2LibraryApiTester
{
    // 测试 SearchReader() API 安全性
    // 测试 Search() API 针对读者库的安全性
    public static class TestSearchReaderSafety
    {
        public static NormalResult TestAll()
        {
            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            result = TestSearchReader("SearchReader", "test_cannot");
            if (result.Value == -1) return result;
            result = TestSearchReader("Search", "test_cannot");
            if (result.Value == -1) return result;

            result = TestSearchReader("SearchReader", "test_normal");
            if (result.Value == -1) return result;
            result = TestSearchReader("Search", "test_normal");
            if (result.Value == -1) return result;
            result = TestSearchReader("SearchReader", "test_level1");
            if (result.Value == -1) return result;
            result = TestSearchReader("Search", "test_level1");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_cannot");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_normal");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_level1");
            if (result.Value == -1) return result;

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

        public static NormalResult TestCross()
        {
            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            // 顺着
            result = TestCrossSearch("SearchReader", "_haidian_cannot", _haidianPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_cannot", _haidianPatronDbName);
            if (result.Value == -1) return result;


            result = TestCrossSearch("SearchReader", "_haidian_normal", _haidianPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_normal", _haidianPatronDbName);
            if (result.Value == -1) return result;

            result = TestCrossSearch("SearchReader", "_haidian_level1", _haidianPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_level1", _haidianPatronDbName);
            if (result.Value == -1) return result;

            // 交叉
            result = TestCrossSearch("SearchReader", "_haidian_cannot", _xichengPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_cannot", _xichengPatronDbName);
            if (result.Value == -1) return result;

            result = TestCrossSearch("SearchReader", "_haidian_normal", _xichengPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_normal", _xichengPatronDbName);
            if (result.Value == -1) return result;

            result = TestCrossSearch("SearchReader", "_haidian_level1", _xichengPatronDbName);
            if (result.Value == -1) return result;
            result = TestCrossSearch("Search", "_haidian_level1", _xichengPatronDbName);
            if (result.Value == -1) return result;

            // TODO: 两个分馆反着交叉

            // TODO: 总馆和分馆之间交叉

            result = TestGetBrowseRecords("test_cannot");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_normal");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_level1");
            if (result.Value == -1) return result;

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

        // 检查浏览列内容是否完整包含了关键列内容
        public static bool FullContains(string [] cols, string xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            if (Array.IndexOf(cols, readerType) == -1)
                return false;
            var department = DomUtil.GetElementText(dom.DocumentElement, "department");
            if (Array.IndexOf(cols, department) == -1)
                return false;
            var name = DomUtil.GetElementText(dom.DocumentElement, "name");
            if (Array.IndexOf(cols, name) == -1)
                return false;

            return true;
        }

        static string GetField(string xml, string field_name)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            return DomUtil.GetElementText(dom.DocumentElement, field_name);
        }

        static string _FILTERED = "[滤除]";

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
                    // 创建一个书目库
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
                    // 创建一个书目库
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
                    // 创建一个书目库
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

                // *** 创建三条读者记录

                {
                    string path = _globalPatronDbName + "/?";

                    DataModel.SetMessage($"正在创建读者记录 {path}");
                    lRet = channel.SetReaderInfo(null,
                        "new",
                        path,
                        _globalPatronXml,
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
                }

                {
                    string path = _haidianPatronDbName + "/?";

                    DataModel.SetMessage($"正在创建读者记录 {path}");
                    lRet = channel.SetReaderInfo(null,
                        "new",
                        path,
                        _haidianPatronXml,
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
                }

                {
                    string path = _xichengPatronDbName + "/?";

                    DataModel.SetMessage($"正在创建读者记录 {path}");
                    lRet = channel.SetReaderInfo(null,
                        "new",
                        path,
                        _xichengPatronXml,
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
                            Rights = "searchreader,search,getreaderinfo",
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
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 用 SearchReader() + GetSearchResult() API
        public static NormalResult TestSearchReader(
            string search_api_name,
            string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 检索 {search_api_name} ...");
                long lRet = 0;
                if (search_api_name == "SearchReader")
                    lRet = channel.SearchReader(
        null,
        _globalPatronDbName,    // strDatabaseNames,
        "",
        -1,
        "__id",
        "left",
        "zh",
        "default",
        "",
        out strError);
                else
                {
                    string query_xml = "<target list='" + _globalPatronDbName + ":" + "__id'><item><word>"
        + "</word><match>left</match><maxCount>"
        + "-1"
        + "</maxCount></item><lang>zh</lang></target>";

                    lRet = channel.Search(null,
                        query_xml,
                        "default",
                        "",
                        out strError);
                }

                if (lRet == -1)
                {
                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    strError = "检索没有命中任何读者记录";
                    goto ERROR1;
                }

                ResultSetLoader loader = new ResultSetLoader(channel,
null,
"default",
$"id,cols,xml",
"zh");
                List<string> errors = new List<string>();

                foreach (Record record in loader)
                {
                    CheckRecord(
    userName,
    record,
    errors);
                }

                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestSearchReader() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestSearchReader() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static void CheckRecord(
            string userName,
            Record record,
            List<string> errors)
        {
            var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
            string cols = "(null)";
            if (record.Cols != null)
                cols = string.Join(",", record.Cols);
            DataModel.SetMessage($"path={record.Path}");
            DataModel.SetMessage($"cols={cols}");
            DataModel.SetMessage($"xml=\r\n{xml}");

            // 没有 getreaderinfo 权限
            if (userName == "test_cannot")
            {
                // Cols 被滤除
                if (record.Cols != null)
                {
                    if (Array.IndexOf(record.Cols, "本科生") != -1
                        || Array.IndexOf(record.Cols, "数学系") != -1
                        || Array.IndexOf(record.Cols, "张三") != -1)
                    {
                        string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                        errors.Add(error);
                    }
                }

                // 用户 "test_level1" 应无法看到 XML 中的 barcode readerType department 元素才对
                // name 元素能看到，但应该是被 mask 形态
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo 普通权限
            if (userName == "test_normal")
            {
                // Cols 被滤除
                if (Array.IndexOf(record.Cols, "本科生") == -1
                    || Array.IndexOf(record.Cols, "数学系") == -1
                    || Array.IndexOf(record.Cols, "张三") == -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了不正确的浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_normal" 应看到 XML 中的 name barcode readerType department 元素才对
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    XmlElement password = dom.DocumentElement.SelectSingleNode("password") as XmlElement;
                    if (password != null)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 居然获得了读者记录 XML 元素 password。严重问题");

                    var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    if (barcode != "R9999998")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 barcode");

                    if (readerType != "本科生")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 readerType");
                    if (department != "数学系")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 department");

                    if (name != "张三")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 name");

                }
            }


            // getreaderinfo:1
            if (userName == "test_level1")
            {
                // Cols 被滤除
                if (/*Array.IndexOf(record.Cols, "本科生") != -1
                    ||*/ Array.IndexOf(record.Cols, "数学系") != -1
                    || Array.IndexOf(record.Cols, "张三") != -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_level1" 应无法看到 XML 中的 barcode readerType department 元素才对
                // name 元素能看到，但应该是被 mask 形态
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    //var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    //var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    /*
                    if (string.IsNullOrEmpty(barcode) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 barcode");

                    if (string.IsNullOrEmpty(readerType) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 readerType");
                    */
                    if (string.IsNullOrEmpty(department) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 department");

                    if (name == "张三")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 name (没有过滤)");

                }
            }

        }

        // 用 GetBrowseRecords() API
        public static NormalResult TestGetBrowseRecords(string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                List<string> path_list = new List<string>();
                path_list.Add(_globalPatronDbName + "/1");
                path_list.Add(_haidianPatronDbName + "/1");
                path_list.Add(_xichengPatronDbName + "/1");

                DataModel.SetMessage($"正在用路径检索 ({userName}) ...");
                long lRet = channel.GetBrowseRecords(
    null,
    path_list.ToArray(),
    "id,cols,xml",
    out Record[] results,
    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }

                if (results.Length == 0)
                {
                    strError = "路径检索没有命中任何读者记录";
                    goto ERROR1;
                }

                List<string> errors = new List<string>();
                foreach (Record record in results)
                {
                    string dbName = StringUtil.GetDbName(record.Path);
                    CheckCrossRecord(
    userName,
    dbName,
    record,
    errors);
                }


                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestGetBrowseRecords() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestGetBrowseRecords() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        #region 交叉检索

        // 测试交叉检索。也就是一个分馆的用户检索不属于它管辖的另外一个分馆的读者记录
        // 用 Search() + GetSearchResult() API
        public static NormalResult TestCrossSearch(
            string search_api_name,
            string userName,
            string patronDbName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 检索 {search_api_name} {patronDbName}...");
                long lRet = 0;
                if (search_api_name == "SearchReader")
                    lRet = channel.SearchReader(
        null,
        patronDbName,    // strDatabaseNames,
        "",
        -1,
        "__id",
        "left",
        "zh",
        "default",
        "",
        out strError);
                else
                {
                    string query_xml = "<target list='" + patronDbName + ":" + "__id'><item><word>"
        + "</word><match>left</match><maxCount>"
        + "-1"
        + "</maxCount></item><lang>zh</lang></target>";

                    lRet = channel.Search(null,
                        query_xml,
                        "default",
                        "",
                        out strError);
                }

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.AccessDenied)
                    {
                        if (userName.StartsWith("_haidian")
                            && (patronDbName == _xichengPatronDbName || patronDbName == _globalPatronDbName))
                        {
                            // 没办法，不让检索
                            return new NormalResult();
                        }
                        else if (userName.StartsWith("_xicheng")
                            && (patronDbName == _haidianPatronDbName || patronDbName == _globalPatronDbName))
                        {
                            // 没办法，不让检索
                            return new NormalResult();
                        }
                        else
                            goto ERROR1;
                    }

                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    strError = "检索没有命中任何读者记录";
                    goto ERROR1;
                }

                ResultSetLoader loader = new ResultSetLoader(channel,
null,
"default",
$"id,cols,xml",
"zh");
                List<string> errors = new List<string>();

                foreach (Record record in loader)
                {
                    CheckCrossRecord(
    userName,
    patronDbName,
    record,
    errors);
                }

                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestSearchReader() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestSearchReader() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static void CheckCrossRecord(
    string userName,
    string patronDbName,
    Record record,
    List<string> errors)
        {
            var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
            string cols = "(null)";
            if (record.Cols != null)
                cols = string.Join(",", record.Cols);
            DataModel.SetMessage($"path={record.Path}");
            DataModel.SetMessage($"cols={cols}");
            DataModel.SetMessage($"xml=\r\n{xml}");

            #region *** 全局用户

            #region 不交叉(全局用户)

            // 没有 getreaderinfo 权限
            if (userName == "test_cannot" && patronDbName == _globalPatronDbName)
            {
                // Cols 应被滤除
                if (record.Cols != null)
                {
                    if (Array.IndexOf(record.Cols, GetField(_globalPatronXml, "readerType")) != -1
                        || Array.IndexOf(record.Cols, GetField(_globalPatronXml, "department")) != -1
                        || Array.IndexOf(record.Cols, GetField(_globalPatronXml, "name")) != -1)
                    {
                        string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                        errors.Add(error);
                    }
                }

                // 用户 "test_cannot" 应无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo 普通权限
            if (userName == "test_normal" && patronDbName == _globalPatronDbName)
            {
                // Cols 应正常返回
                if (FullContains(record.Cols, _globalPatronXml) == false)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了不正确的浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_normal" 应看到 XML 中的 name barcode readerType department 元素才对
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    XmlElement password = dom.DocumentElement.SelectSingleNode("password") as XmlElement;
                    if (password != null)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 居然获得了读者记录 XML 元素 password。严重问题");

                    var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    if (barcode != GetField(_globalPatronXml, "barcode"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 barcode");

                    if (readerType != GetField(_globalPatronXml, "readerType"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 readerType");
                    if (department != GetField(_globalPatronXml, "department"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 department");

                    if (name != GetField(_globalPatronXml, "name"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 name");

                }
            }

            // getreaderinfo:1
            if (userName == "test_level1" && patronDbName == _globalPatronDbName)
            {
                // Cols 被滤除
                if (/*Array.IndexOf(record.Cols, "本科生") != -1
                    ||*/ Array.IndexOf(record.Cols, "数学系") != -1
                    || Array.IndexOf(record.Cols, "张三") != -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_level1" 应无法看到 XML 中的 barcode readerType department 元素才对
                // name 元素能看到，但应该是被 mask 形态
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    // var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    //var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    /*
                    if (string.IsNullOrEmpty(barcode) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 barcode");
                    if (string.IsNullOrEmpty(readerType) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 readerType");
                    */
                    if (string.IsNullOrEmpty(department) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 department");

                    if (name == "张三")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 name (没有过滤)");

                }
            }


            #endregion

            #region 交叉(全局用户)

            // 没有 getreaderinfo 权限
            if (userName == "test_cannot" && patronDbName == _haidianPatronDbName)
            {
                // Cols 应被滤除
                if (record.Cols != null)
                {
                    if (record.Cols[0] != _FILTERED)
                    {
                        string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                        errors.Add(error);
                    }
                }

                // 用户 "test_cannot" 应无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo 普通权限
            if (userName == "test_normal" && patronDbName == _haidianPatronDbName)
            {
                // Cols 应该正常返回
                if (FullContains(record.Cols, _haidianPatronXml) == false)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了不正确的浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_normal" 应看到 XML 中的 name barcode readerType department 元素才对
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    XmlElement password = dom.DocumentElement.SelectSingleNode("password") as XmlElement;
                    if (password != null)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 居然获得了读者记录 XML 元素 password。严重问题");

                    var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    if (barcode != GetField(_haidianPatronXml, "barcode"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 barcode");

                    if (readerType != GetField(_haidianPatronXml, "readerType"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 readerType");
                    if (department != GetField(_haidianPatronXml, "department"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 department");

                    if (name != GetField(_haidianPatronXml, "name"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 name");
                }
            }

            // getreaderinfo:1
            if (userName == "test_level1" && patronDbName == _haidianPatronDbName)
            {
                // Cols 被滤除
                if (/*Array.IndexOf(record.Cols, GetField(_haidianPatronXml, "readerType")) != -1
                    || */Array.IndexOf(record.Cols, GetField(_haidianPatronXml, "department")) != -1
                    || Array.IndexOf(record.Cols, GetField(_haidianPatronXml, "name")) != -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_level1" 应无法看到 XML 中的 barcode readerType department 元素才对
                // name 元素能看到，但应该是被 mask 形态
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    //var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    //var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    /*
                    if (string.IsNullOrEmpty(barcode) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 barcode");

                    if (string.IsNullOrEmpty(readerType) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 readerType");
                    */
                    if (string.IsNullOrEmpty(department) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 department");

                    if (name == GetField(_haidianPatronXml, "name"))
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 name (没有过滤)");

                }
            }


            #endregion

            #endregion

            #region *** 海淀用户

            #region 不交叉

            // 没有 getreaderinfo 权限
            if (userName == "_haidian_cannot" && patronDbName == _haidianPatronDbName)
            {
                // Cols 被滤除
                if (record.Cols != null)
                {
                    if (Array.IndexOf(record.Cols, "教授") != -1
                        || Array.IndexOf(record.Cols, "物理系") != -1
                        || Array.IndexOf(record.Cols, "海淀读者") != -1)
                    {
                        string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                        errors.Add(error);
                    }
                }

                // 用户 "_haidian_cannot" 应无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo 普通权限
            if (userName == "_haidian_normal" && patronDbName == _haidianPatronDbName)
            {
                // Cols 被滤除
                if (Array.IndexOf(record.Cols, "教授") == -1
                    || Array.IndexOf(record.Cols, "物理系") == -1
                    || Array.IndexOf(record.Cols, "海淀读者") == -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了不正确的浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "_haidian_normal" 应看到 XML 中的 name barcode readerType department 元素才对
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    XmlElement password = dom.DocumentElement.SelectSingleNode("password") as XmlElement;
                    if (password != null)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 居然获得了读者记录 XML 元素 password。严重问题");

                    var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    if (barcode != "R9999997")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 barcode");

                    if (readerType != "教授")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 readerType");
                    if (department != "物理系")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 department");

                    if (name != "海淀读者")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 无法获得读者记录 XML 元素 name");
                }
            }

            // getreaderinfo:1
            if (userName == "_haidian_level1" && patronDbName == _haidianPatronDbName)
            {
                // Cols 被滤除
                if (/*Array.IndexOf(record.Cols, "教授") != -1
                    ||*/ Array.IndexOf(record.Cols, "物理系") != -1
                    || Array.IndexOf(record.Cols, "海淀读者") != -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "_haidian_level1" 应无法看到 XML 中的 barcode readerType department 元素才对
                // name 元素能看到，但应该是被 mask 形态
                if (string.IsNullOrEmpty(xml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    //var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    //var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    /*
                    if (string.IsNullOrEmpty(barcode) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 barcode");

                    if (string.IsNullOrEmpty(readerType) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 readerType");
                    */
                    if (string.IsNullOrEmpty(department) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 department");

                    if (name == "海淀读者")
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 name (没有过滤)");
                }
            }

            #endregion

            #region 交叉: 海淀用户访问西城读者库

            // 没有 getreaderinfo 权限
            if (userName == "_haidian_cannot" && patronDbName == _xichengPatronDbName)
            {
                // Cols 被滤除
                if (record.Cols != null)
                {
                    if (Array.IndexOf(record.Cols, "博士生") != -1
                        || Array.IndexOf(record.Cols, "化学系") != -1
                        || Array.IndexOf(record.Cols, "西城读者") != -1)
                    {
                        string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                        errors.Add(error);
                    }
                }

                // 用户 "_haidian_cannot" 应无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo 普通权限
            if (userName == "_haidian_normal" && patronDbName == _xichengPatronDbName)
            {
                // Cols 被滤除
                if (Array.IndexOf(record.Cols, "博士生") == -1
                    || Array.IndexOf(record.Cols, "化学系") == -1
                    || Array.IndexOf(record.Cols, "西城读者") == -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了不正确的浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "_haidian_normal" 无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            // getreaderinfo:1
            if (userName == "_haidian_level1" && patronDbName == _xichengPatronDbName)
            {
                // Cols 被滤除
                if (/*Array.IndexOf(record.Cols, "博士生") != -1
                    || */Array.IndexOf(record.Cols, "化学系") != -1
                    || Array.IndexOf(record.Cols, "西城读者") != -1)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了没有过滤的原始浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "_haidian_level1" 应无法看到 XML 记录
                if (string.IsNullOrEmpty(xml) == false)
                {
                    errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML");
                }
            }

            #endregion

            #endregion

            #region *** 西城用户


            #endregion
        }

        #endregion

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

                DataModel.SetMessage("结束");
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
            DataModel.SetMessage($"Finish() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }
    }
}
