using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;

// TODO: 创建一个具有 Access 存取定义的账户，限定读者库访问的那种，然后进行检索验证
namespace dp2LibraryApiTester
{
    // 测试 SearchReader() API 安全性
    public static class TestSearchReaderSafety
    {
        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            // 创建一个读者库
            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                // 创建测试所需的书目库

                string strReaderDbName = "_测试用读者";

                // 如果测试用的读者库以前就存在，要先删除。
                DataModel.SetMessage("正在删除测试用读者库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    strReaderDbName,    // strDatabaseNames,
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
                    strReaderDbName,
                    "", // libraryCode
                    true,
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // *** 创建读者记录
                string path = strReaderDbName + "/?";
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

                DataModel.SetMessage("正在删除用户 test_level1 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_level1",
                    },
                    out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    goto ERROR1;

                DataModel.SetMessage("正在删除用户 test_access1 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_access1",
                    },
                    out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    goto ERROR1;

                // 创建几个测试用工作人员账户
                DataModel.SetMessage("正在创建用户 test_level1 ...");
                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo {
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
        public static NormalResult TestSearchReader(string search_api_name)
        {
            string strError = "";

            // 创建一个读者库
            LibraryChannel channel = DataModel.NewChannel("test_level1", "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                string strReaderDbName = "_测试用读者";

                DataModel.SetMessage($"正在检索 {search_api_name} ...");
                long lRet = 0;
                if (search_api_name == "SearchReader")
                    lRet = channel.SearchReader(
        null,
        strReaderDbName,    // strDatabaseNames,
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
                    string query_xml = "<target list='" + strReaderDbName + ":" + "__id'><item><word>"
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

                foreach (Record record in loader)
                {
                    var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
                    DataModel.SetMessage($"path={record.Path}");
                    DataModel.SetMessage($"xml=\r\n{xml}");
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

        // 用 GetBrowseRecords() API
        public static NormalResult TestGetBrowseRecords()
        {
            string strError = "";

            // 创建一个读者库
            LibraryChannel channel = DataModel.NewChannel("test_level1", "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                string strReaderDbName = "_测试用读者";

                List<string> path_list = new List<string>();
                path_list.Add(strReaderDbName + "/1");

                DataModel.SetMessage("正在用路径检索 ...");
                long lRet = channel.GetBrowseRecords(
    null,
    path_list.ToArray(),
    "id,cols,xml",
    out Record [] results,
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

                foreach (Record record in results)
                {
                    var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
                    DataModel.SetMessage($"path={record.Path}");
                    DataModel.SetMessage($"xml=\r\n{xml}");
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

            // 创建一个读者库
            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                string strReaderDbName = "_测试用读者";

                DataModel.SetMessage("正在删除测试用读者库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    strReaderDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在删除用户 test_level1 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_level1",
                    },
                    out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    goto ERROR1;

                DataModel.SetMessage("正在删除用户 test_access1 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_access1",
                    },
                    out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
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
