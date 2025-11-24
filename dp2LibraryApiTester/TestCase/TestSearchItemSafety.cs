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
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester
{
    // 测试 SearchItem() API 安全性
    // 测试 Search() API 针对实体库的安全性
    public static class TestSearchItemSafety
    {
        static string _dbType = "item"; // order/issue/comment

        public static NormalResult TestAll(string dbType)
        {
            _dbType = dbType;

            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            string api_name = "";
            if (_dbType == "item")
                api_name = "SearchItem";
            else if (_dbType == "order")
                api_name = "SearchOrder";
            else if (_dbType == "issue")
                api_name = "SearchIssue";
            else if (_dbType == "comment")
                api_name = "SearchComment";
            else
                throw new ArgumentException("_dbType error");

            result = TestSearchItem(api_name, "test_rights");
            if (result.Value == -1) return result;

            result = TestSearchItem(api_name, "test_cannot");
            if (result.Value == -1) return result;

            result = TestGetBrowseRecords("test_rights");
            if (result.Value == -1) return result;
            result = TestGetBrowseRecords("test_cannot");
            if (result.Value == -1) return result;

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }


        static string _strBiblioDbName = "_测试用书目";
        // static string _strItemDbName = "_测试用实体";
        static string _itemRecordPath = "";

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
                    _dbType == "issue" ? "series" : "book",
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


                // 2025/11/3
                // 临时设置 不校验条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?VerifyBarcode",
                    "false",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 创建册记录
                DataModel.SetMessage($"正在创建书目的下级({_dbType})记录");
                EntityInfo[] errorinfos = null;
                var entities = BuildEntityRecords();
                if (_dbType == "item")
                    lRet = channel.SetEntities(null,
                        output_biblio_recpath,
                        entities,
                        out errorinfos,
                        out strError);
                else if (_dbType == "order")
                    lRet = channel.SetOrders(null,
    output_biblio_recpath,
    entities,
    out errorinfos,
    out strError);
                else if (_dbType == "issue")
                    lRet = channel.SetIssues(null,
    output_biblio_recpath,
    entities,
    out errorinfos,
    out strError);
                else if (_dbType == "comment")
                    lRet = channel.SetComments(null,
    output_biblio_recpath,
    entities,
    out errorinfos,
    out strError);
                else
                    throw new ArgumentException("_dbType error");
                if (lRet == -1)
                    goto ERROR1;
                strError = GetError(errorinfos);
                if (string.IsNullOrEmpty(strError) == false)
                    goto ERROR1;
                _itemRecordPath = errorinfos[0].NewRecPath;

                var user_names = new List<string>() { "test_rights", "test_cannot" };
                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                nRet = Utility.DeleteUsers(channel,
                    user_names,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 创建

                // 通过 rights 定义，能正常访问实体记录
                DataModel.SetMessage("正在创建用户 test_rights ...");
                string rights = "";
                if (_dbType == "item")
                    rights = "searchitem,search,getiteminfo";
                else if (_dbType == "order")
                    rights = "searchorder,search,getorderinfo";
                else if (_dbType == "issue")
                    rights = "searchissue,search,getissueinfo";
                else if (_dbType == "comment")
                    rights = "searchcomment,search,getcommentinfo";
                else
                    throw new ArgumentException("_dbType error");

                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_rights",
                        Rights = rights,
                    },
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 不具备 getiteminfo，造成无法访问实体记录的效果
                DataModel.SetMessage("正在创建用户 test_cannot ...");
                rights = "";
                if (_dbType == "item")
                    rights = "searchitem,search";
                else if (_dbType == "order")
                    rights = "searchorder,search";
                else if (_dbType == "issue")
                    rights = "searchissue,search";
                else if (_dbType == "comment")
                    rights = "searchcomment,search";
                else
                    throw new ArgumentException("_dbType error");

                lRet = channel.SetUser(null,
                    "new",
                    new UserInfo
                    {
                        UserName = "test_cannot",
                        Rights = rights,
                        Access = "",
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
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        // 用 SearchItem() + GetSearchResult() API
        public static NormalResult TestSearchItem(
            string search_api_name,
            string userName)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                string strItemDbName = StringUtil.GetDbName(_itemRecordPath);

                DataModel.SetMessage($"正在以用户 {userName} 检索 {search_api_name} ...");
                long lRet = 0;
                if (search_api_name == "SearchItem")
                    lRet = channel.SearchItem(
        null,
        strItemDbName,    // strDatabaseNames,
        "",
        1000,
        "__id",
        "left",
        "zh",
        "default",
        "", // search_style
        "", // output_style
        out strError);
                else if (search_api_name == "SearchOrder")
                    lRet = channel.SearchOrder(
null,
strItemDbName,
"",
1000,
"__id",
"left",
"zh",
"default",
"", // search_style
"", // output_style
out strError);
                else if (search_api_name == "SearchIssue")
                    lRet = channel.SearchIssue(
null,
strItemDbName,
"",
1000,
"__id",
"left",
"zh",
"default",
"", // search_style
"", // output_style
out strError);
                else if (search_api_name == "SearchComment")
                    lRet = channel.SearchComment(
null,
strItemDbName,
"",
1000,
"__id",
"left",
"zh",
"default",
"", // search_style
"", // output_style
out strError);
                else
                {
                    string query_xml = "<target list='" + strItemDbName + ":" + "__id'><item><word>"
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
                    strError = $"检索没有命中任何{_dbType}记录";
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

                    CheckRecord(userName,
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
                strError = "TestSetSubrecords() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestSetSubrecords() error: {strError}", "error");
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

            if (userName == "test_rights")
            {
                errors.AddRange(VerifyCols(record.Cols));

                errors.AddRange(VerifyItemRecord(record.RecordBody.Xml));
            }

            // 权限无法访问的效果
            if (userName == "test_cannot")
            {
                // Cols 被滤除
                if (record.Cols[0] != "[滤除]")
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了浏览列内容 '{string.Join(",", record.Cols)}'，违反安全性原则";
                    errors.Add(error);
                }

                // 用户 "test_cannot" 应无法看到 XML 才对
                if (string.IsNullOrEmpty(xml) == false)
                {
                    string error = $"用户 {userName} 通过 GetSearchResult() API 获得了册记录 XML，违反安全性原则";
                    errors.Add(error);
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
                path_list.Add(_itemRecordPath);

                DataModel.SetMessage($"正在用路径 {StringUtil.MakePathList(path_list, ",")} 检索 ({userName})...");
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
                    strError = "路径检索没有命中任何实体记录";
                    goto ERROR1;
                }

                List<string> errors = new List<string>();
                foreach (Record record in results)
                {
                    var xml = DomUtil.GetIndentXml(record.RecordBody.Xml);
                    var cols = string.Join(",", record.Cols);
                    DataModel.SetMessage($"path={record.Path}");
                    DataModel.SetMessage($"cols={cols}");
                    DataModel.SetMessage($"xml=\r\n{xml}");

                    CheckRecord(userName,
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
            DataModel.SetMessage($"TestGetBrowseRecords() error: {strError}", "error");
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

                var user_names = new List<string>() { "test_rights", "test_cannot" };
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
            DataModel.SetMessage($"Finish() error: {strError}", "error");
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

        static string _itemXml = @"<root>
<barcode>X0000001</barcode>
<location>阅览室</location>
<bookType>普通</bookType>
<price>CNY12.00</price>
</root>";

        static string _issueXml = @"<root>
<publishTime>20150101</publishTime>
<issue>1</issue>
<zong>1</zong>
<volume>1</volume>
</root>";

        static string _orderXml = @"<root>
<seller>qudao</seller>
<source>laiyuan</source>
<copy>1</copy>
<orderTime>Sun, 09 Apr 2017 00:00:00 +0800</orderTime>
<price>CNY12.00</price>
</root>";

        static string _commentXml = @"<root>
<type>书评</type>
<title>title</title>
<creator>R0000001</creator>
<content>comment</content>
<parent>612</parent>
</root>";

        // 验证 Cols 是否正确
        static List<string> VerifyCols(string[] cols)
        {
            List<string> errors = new List<string>();
            XmlDocument dom = new XmlDocument();

            if (_dbType == "item")
            {
                dom.LoadXml(_itemXml);

                string barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (Array.IndexOf(cols, barcode) == -1)
                    errors.Add($"浏览列中未包含册条码号 '{barcode}'");

                string location = DomUtil.GetElementText(dom.DocumentElement, "location");
                if (Array.IndexOf(cols, location) == -1)
                    errors.Add($"浏览列中未包含馆藏地 '{location}'");

                string bookType = DomUtil.GetElementText(dom.DocumentElement, "bookType");
                if (Array.IndexOf(cols, bookType) == -1)
                    errors.Add($"浏览列中未包含图书类型 '{bookType}'");

                string price = DomUtil.GetElementText(dom.DocumentElement, "price");
                if (Array.IndexOf(cols, price) == -1)
                    errors.Add($"浏览列中未包含价格 '{price}'");
            }
            else if (_dbType == "order")
            {
                dom.LoadXml(_orderXml);

                errors.AddRange(VerifyCols(dom, cols, "seller,source,copy,price"));
            }
            else if (_dbType == "issue")
            {
                dom.LoadXml(_issueXml);

                errors.AddRange(VerifyCols(dom, cols, "publishTime,issue,zong,volume"));
            }
            else if (_dbType == "comment")
            {
                dom.LoadXml(ModifyCreator(_commentXml));

                errors.AddRange(VerifyCols(dom, cols, "type,title,creator,content"));
            }
            else 
                throw new ArgumentException("_dbType error");

            return errors;
        }

        static List<string> VerifyCols(XmlDocument dom,
            string [] cols,
            string name_list)
        {
            var names = StringUtil.SplitList(name_list);
            List<string> errors = new List<string>();
            foreach (var name in names)
            {
                string value = DomUtil.GetElementText(dom.DocumentElement, name);
                if (Array.IndexOf(cols, value) == -1)
                    errors.Add($"浏览列中未包含{name}元素值 '{value}'");
            }

            return errors;
        }

        // 验证册记录是否正确
        static List<string> VerifyItemRecord(string xml)
        {
            string list = "";
            if (_dbType == "item")
                list = "barcode,location,bookType,price";
            else if (_dbType == "issue")
                list = "publishTime,issue,zong,volume";
            else if (_dbType == "order")
                list = "seller,source,copy,orderTime,price";
            else if (_dbType == "comment")
                list = "type,title,creator,content";
            else
                throw new ArgumentException("_dbType error");

            return VerifyItemRecord(xml, list);
        }

        static List<string> VerifyItemRecord(string xml,
            string name_list)
        {
            List<string> errors = new List<string>();

            string origin_xml = "";
            if (_dbType == "item")
                origin_xml = _itemXml;
            else if (_dbType == "order")
                origin_xml = _orderXml;
            else if (_dbType == "issue")
                origin_xml = _issueXml;
            else if (_dbType == "comment")
            {
                // 2024/4/19
                // creator 元素需要改为当前通道 dp2libary 用户名
                origin_xml = ModifyCreator(_commentXml);
            }
            else
                throw new ArgumentException("_dbType error");

            XmlDocument origin_dom = new XmlDocument();
            origin_dom.LoadXml(origin_xml);

            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(xml);

            var names = StringUtil.SplitList(name_list);
            foreach (var name in names)
            {
                string error = VerifyElement(name);
                if (error != null)
                    errors.Add(error);
            }

            return errors;

            string VerifyElement(string element_name)
            {
                string origin_value = DomUtil.GetElementText(origin_dom.DocumentElement, element_name);
                string new_value = DomUtil.GetElementText(new_dom.DocumentElement, element_name);
                if (origin_value != new_value)
                    return $"{element_name} 元素值 '{new_value}' 和参考值 '{origin_value}' 不符";
                return null;
            }
        }

        // 修改册 XML 中的 creator 元素文本值
        static string ModifyCreator(string source_xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(source_xml);
            var creators = dom.DocumentElement.SelectNodes("creator");
            foreach (XmlElement creator in creators)
            {
                creator.InnerText = DataModel.dp2libraryUserName;
            }

            return dom.DocumentElement.OuterXml;
        }


        static EntityInfo[] BuildEntityRecords()
        {
            string xml = "";
            if (_dbType == "item")
                xml = _itemXml;
            else if (_dbType == "issue")
                xml = _issueXml;
            else if (_dbType == "order")
                xml = _orderXml;
            else if (_dbType == "comment")
                xml = _commentXml;
            else
                throw new ArgumentException("_dbType 不合法");

            List<EntityInfo> results = new List<EntityInfo>();
            EntityInfo entity = new EntityInfo
            {
                Action = "new",
                NewRecord = xml,
            };
            results.Add(entity);
            return results.ToArray();
        }

        static string GetError(EntityInfo[] errorinfos)
        {
            if (errorinfos != null)
            {
                List<string> errors = new List<string>();
                foreach (var error in errorinfos)
                {
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        errors.Add(error.ErrorInfo);
                    }
                }

                if (errors.Count > 0)
                    return StringUtil.MakePathList(errors, "; ");
            }

            return null;
        }
    }
}
