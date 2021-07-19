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
        static string _strReaderDbName = "_测试用读者";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                // 创建测试所需的书目库


                // 如果测试用的读者库以前就存在，要先删除。
                DataModel.SetMessage("正在删除测试用读者库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _strReaderDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在创建测试用读者库 ...");
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                int nRet = ManageHelper.CreateReaderDatabase(
                    channel,
                    null,
                    _strReaderDbName,
                    "", // libraryCode
                    true,
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // *** 创建读者记录
                string path = _strReaderDbName + "/?";
                string xml = @"<root>
<barcode>R9999998</barcode>
<name>张三</name>
<readerType>本科生</readerType>
<department>数学系</department>
</root>";

                DataModel.SetMessage("正在创建读者记录");
                lRet = channel.SetReaderInfo(null,
                    "new",
                    path,
                    xml,
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

                var user_names = new List<string>() { "test_cannot", "test_normal", "test_level1", "test_access1" };
                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                nRet = Utility.DeleteUsers(channel,
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
        _strReaderDbName,    // strDatabaseNames,
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
                    string query_xml = "<target list='" + _strReaderDbName + ":" + "__id'><item><word>"
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
                if (Array.IndexOf(record.Cols, "本科生") != -1
                    || Array.IndexOf(record.Cols, "数学系") != -1
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
                    var barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    var name = DomUtil.GetElementText(dom.DocumentElement, "name");
                    var readerType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
                    var department = DomUtil.GetElementText(dom.DocumentElement, "department");

                    if (string.IsNullOrEmpty(barcode) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 barcode");

                    if (string.IsNullOrEmpty(readerType) == false)
                        errors.Add($"用户 {userName} 通过 GetSearchResult() API 获得了读者记录 XML 元素 readerType");
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
                path_list.Add(_strReaderDbName + "/1");

                DataModel.SetMessage("正在用路径检索 ...");
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
    _strReaderDbName,    // strDatabaseNames,
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
