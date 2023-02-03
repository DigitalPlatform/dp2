using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester
{
    // 测试 SearchBiblio() API 安全性
    // 测试 Search() API 针对书目库的安全性
    public static class TestSearchBiblioSafety
    {
        static string _strBiblioDbName = "_测试用书目";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                // 创建测试所需的书目库

                // 如果测试用的书目库以前就存在，要先删除。
                DataModel.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在创建测试用书目库 ...");
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                int nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    null,
                    _strBiblioDbName,
                    "book",
                    "unimarc",
                    "*",
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // *** 创建书目记录
                string path = _strBiblioDbName + "/?";
                var record = BuildBiblioRecord("测试题名", "");

                string strXml = "";
                nRet = MarcUtil.Marc2XmlEx(
    record.Text,
    "unimarc",
    ref strXml,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
                DataModel.SetMessage("正在创建书目记录");
                lRet = channel.SetBiblioInfo(null,
                    "new",
                    path,
                    "xml", // strBiblioType
                    strXml,
                    null,
                    null,
                    "", // style
                    out string output_biblio_recpath,
                    out byte[] new_timestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                DataModel.SetMessage("正在删除用户 test_rights ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_rights",
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

                DataModel.SetMessage("正在删除用户 test_access2 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_access2",
                    },
                    out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    goto ERROR1;

                // 通过 rights 定义，能正常访问书目记录
                DataModel.SetMessage("正在创建用户 test_rights ...");
                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_rights",
                        Rights = "searchbiblio,search,getbiblioinfo",
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 通过存取代码，造成无法访问书目记录的效果
                DataModel.SetMessage("正在创建用户 test_access1 ...");
                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_access1",
                        Rights = "searchbiblio,search,getbiblioinfo",
                        Access = "中文图书:getbiblioinfo=*;",
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 通过存取代码，定义只能获得 200 字段
                DataModel.SetMessage("正在创建用户 test_access2 ...");
                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_access2",
                        Rights = "searchbiblio,search,getbiblioinfo",
                        Access = $"{_strBiblioDbName}:getbiblioinfo=*(200);",
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 中文图书:getbiblioinfo=*;中文期刊:getbiblioinfo=*|setbiblioinfo=new,change;西文采访:getbiblioinfo=*|setbiblioinfo=*;

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

        // 用 SearchBiblio() + GetSearchResult() API
        public static NormalResult TestSearchBiblio(
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
                if (search_api_name == "SearchBiblio")
                    lRet = channel.SearchBiblio(
        null,
        _strBiblioDbName,    // strDatabaseNames,
        "",
        1000,
        "recid", // "__id",
        "left",
        "zh",
        "default",
        "", // search_style
        "", // output_style
        "", // location_filter
        out string query_xml,
        out strError);
                else
                {
                    string query_xml = "<target list='" + _strBiblioDbName + ":" + "__id'><item><word>"
        + "</word><match>left</match><maxCount>"
        + "1000"
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
                    strError = "检索没有命中任何书目记录";
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
                    var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
                    var cols = string.Join(",", record.Cols);
                    DataModel.SetMessage($"path={record.Path}");
                    DataModel.SetMessage($"cols={cols}");
                    DataModel.SetMessage($"xml=\r\n{xml}");

                    if (userName == "test_access2")
                    {
                        // MARC 中只有 200 字段
                        int nRet = MarcUtil.Xml2Marc(xml,
                            false,
                            "unimarc",
                            out string _,
                            out string marc,
                            out string error);
                        MarcRecord marc_record = new MarcRecord(marc);
                        if (marc_record.select("field[@name='690']").count > 0)
                            errors.Add("xml 中不应该存在 690 字段");
                        if (marc_record.select("field[@name='701']").count > 0)
                            errors.Add("xml 中不应该存在 701 字段");
                    }

                    // access 代码为无法访问的效果
                    if (userName == "test_access1")
                    {
                        // Cols 被滤除
                        if (record.Cols[0].StartsWith("[滤除]") == false)
                        {
                            string error = $"用户 {userName} 通过 GetSearchResult() API 获得了浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                            errors.Add(error);
                        }

                        // 用户 "test_access1" 应无法看到 XML 才对
                        if (string.IsNullOrEmpty(xml) == false)
                        {
                            string error = $"用户 {userName} 通过 GetSearchResult() API 获得了书目记录 XML，违反安全性原则";
                            errors.Add(error);
                        }
                    }
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
                strError = "TestSearchBiblio() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestSearchBiblio() error: {strError}");
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

            LibraryChannel channel = DataModel.NewChannel("test_rights", "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                List<string> path_list = new List<string>();
                path_list.Add(_strBiblioDbName + "/1");

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
                    strError = "路径检索没有命中任何书目记录";
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

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在删除用户 test_rights ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_rights",
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

                DataModel.SetMessage("正在删除用户 test_access2 ...");
                lRet = channel.SetUser(null,
                    "delete",
                    new UserInfo
                    {
                        UserName = "test_access2",
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

        // parameters:
        //      strStyle    create_objects  表示要创建对象文件
        static MarcRecord BuildBiblioRecord(
            string strTitle,
            string strStyle)
        {
            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "200  $a" + strTitle));
            record.add(new MarcField('$', "690  $aI247.5"));
            record.add(new MarcField('$', "701  $a测试著者"));
            return record;
        }

    }
}
