using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2LibraryApiTester
{
    public static class TestSetEntities
    {
        static string _dbType = "item"; // order/issue/comment

        public static NormalResult TestAll(string dbType,
            CancellationToken token)
        {
            _dbType = dbType;

            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            token.ThrowIfCancellationRequested();
            {
                result = TestWriteRights("file");
                if (result.Value == -1) return result;

                result = TestWriteRights("file,negative");
                if (result.Value == -1) return result;

                result = TestWriteRights("file,access");
                if (result.Value == -1) return result;

                result = TestWriteRights("file,access,negative");
                if (result.Value == -1) return result;

                // mix 表示否定的权限是用混合方式表达的，比如有一个不相干的数据库具有权限
                result = TestWriteRights("file,access,negative,mix");
                if (result.Value == -1) return result;

            }

            token.ThrowIfCancellationRequested();

            {
                result = TestWriteRights("");
                if (result.Value == -1) return result;

                result = TestWriteRights("negative");
                if (result.Value == -1) return result;

                result = TestWriteRights("access");
                if (result.Value == -1) return result;

                result = TestWriteRights("access,negative");
                if (result.Value == -1) return result;

                // mix 表示否定的权限是用混合方式表达的，比如有一个不相干的数据库具有权限
                result = TestWriteRights("access,negative,mix");
                if (result.Value == -1) return result;
            }

            token.ThrowIfCancellationRequested();

            {
                result = TestSetSubrecords("test_rights", "notchanged");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_01");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_02");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_03");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_04");
                if (result.Value == -1) return result;
            }

            token.ThrowIfCancellationRequested();

            // 测试 dprms:file 元素的处理
            {
                result = TestSetSubrecords("test_rights", "notchanged,file");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_01,file");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_02,file");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_03,file");
                if (result.Value == -1) return result;

                result = TestSetSubrecords("test_rights", "outofrange_04,file");
                if (result.Value == -1) return result;
            }

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }


        static string _strBiblioDbName = "_测试用书目";
        // static string _strItemDbName = "_测试用实体";
        static string _itemRecordPath = "";
        static string _biblio_recpath = "";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            try
            {
                {
                    // 利用默认账户进行准备操作
                    LibraryChannel channel = DataModel.GetChannel();
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromMinutes(10);

                    try
                    {
                        // 创建测试所需的书目库

                        // 如果测试用的书目库以前就存在，要先删除。
                        DataModel.SetMessage("正在删除测试用书目库 ...");
                        string strOutputInfo = "";
                        lRet = channel.ManageDatabase(
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
                        nRet = ManageHelper.CreateBiblioDatabase(
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

                        // 临时设置 不校验条码号
                        lRet = channel.SetSystemParameter(null,
                            "circulation",
                            "?VerifyBarcode",
                            "false",
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        // 准备 dp2library 账户
                        {
                            DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(_user_names, ",")} ...");
                            nRet = Utility.DeleteUsers(channel,
                                _user_names,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            // 创建

                            // 通过 rights_template 定义，能正常访问实体记录
                            DataModel.SetMessage("正在创建用户 test_rights ...");
                            string rights = "";
                            // setbiblioinfo 权限是因为要用这个账户来创建书目记录
                            if (_dbType == "item")
                                rights = "getbiblioinfo,setbiblioinfo,getiteminfo,setiteminfo,getitemobject,setitemobject";
                            else if (_dbType == "order")
                                rights = "getbiblioinfo,setbiblioinfo,getorderinfo,setorderinfo,getorderobject,setorderobject";
                            else if (_dbType == "issue")
                                rights = "getbiblioinfo,setbiblioinfo,getissueinfo,setissueinfo,getissueobject,setissueobject";
                            else if (_dbType == "comment")
                                rights = "getbiblioinfo,setbiblioinfo,getcommentinfo,setcommentinfo,getcommentobject,setcommentobject";
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
                        }


                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        DataModel.ReturnChannel(channel);
                    }
                }

                {
                    // 利用 test_right 账户进行准备操作。评注库库要求原始创建者才能修改评注记录
                    LibraryChannel channel = DataModel.NewChannel("test_rights", "");
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromMinutes(10);
                    try
                    {
                        // 创建书目记录和下级记录
                        {
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

                            _biblio_recpath = output_biblio_recpath;

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
                            {

                                lRet = channel.SetComments(null,
                output_biblio_recpath,
                entities,
                out errorinfos,
                out strError);

                            }
                            else
                                throw new ArgumentException("_dbType error");
                            if (lRet == -1)
                                goto ERROR1;
                            strError = Utility.GetError(errorinfos, out ErrorCodeValue error_code);
                            if (string.IsNullOrEmpty(strError) == false)
                            {
                                if (_dbType == "comment" && error_code == ErrorCodeValue.PartialDenied)
                                {
                                    // 创建评注记录，XML 中的 creator 元素，会导致返回 PartialDenied 错误码
                                }
                                else
                                    goto ERROR1;
                            }
                            _itemRecordPath = errorinfos[0].NewRecPath;
                        }
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        DataModel.DeleteChannel(channel);
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "PrepareEnvironment() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }

        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static void SetOutDefElement(XmlDocument item_dom,
            string out_def_element_name)
        {
            DomUtil.SetElementText(item_dom.DocumentElement,
    out_def_element_name,
    $"{out_def_element_name}_value");
        }

        static void SetInDefElement(XmlDocument item_dom,
    string in_def_element_name)
        {
            // TODO: 增加关于 SetEntities() 等 API 在 dprms:file/@id 重复时的校验能力
            if (in_def_element_name == "http://dp2003.com/dprms:file")
            {
                // 创建一个 dprms:file 元素
                var nsmgr = new XmlNamespaceManager(item_dom.NameTable);
                nsmgr.AddNamespace("dprms", "http://dp2003.com/dprms");

                var file_element = item_dom.DocumentElement.SelectSingleNode($"//dprms:file[@id='1']", nsmgr) as XmlElement;
                if (file_element == null)
                {
                    file_element = item_dom.CreateElement("dprms:file", "http://dp2003.com/dprms");
                    item_dom.DocumentElement.AppendChild(file_element);
                    file_element.SetAttribute("id", "1");
                }
            }
            else
                DomUtil.SetElementText(item_dom.DocumentElement,
    in_def_element_name,
    $"{in_def_element_name}_value");
        }


        // 用 SetEntities() API，测试对书目子记录的保存，不同的合法、超范围元素组合写入修改
        // parameters:
        //      condition   "notchanged" 保存的册记录没有任何变化
        public static NormalResult TestSetSubrecords(
            string userName,
            string condition)
        {
            string strError = "";

            string in_def_element_name = "comment"; // 定义范围内的元素名
            if (_dbType == "comment")
                in_def_element_name = "summary"; // 评注库没有 comment 元素

            if (StringUtil.IsInList("file", condition))
                in_def_element_name = "http://dp2003.com/dprms:file";

            string out_def_element_name = "ttt";    // 定义范围外的元素名


            LibraryChannel channel = null;

            //if (_dbType == "comment")
            //    channel = DataModel.GetChannel();   // 评注记录需要用原始创建者身份来写入修改
            //else
            channel = DataModel.NewChannel(userName, "");

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                List<string> errors = new List<string>();
                string strItemDbName = StringUtil.GetDbName(_itemRecordPath);
                string item_xml = "";
                string item_recpath = "";
                byte[] item_timestamp = null;

                // *** 获得册记录 XML
                DataModel.SetMessage($"正在以用户 {userName} 身份获得记录 {_itemRecordPath} ...");
                long lRet = 0;
                if (_dbType == "item")
                    lRet = channel.GetItemInfo(
        null,
        $"@path:{_itemRecordPath}",
        "xml",
        out item_xml,
        out item_recpath,
        out item_timestamp,
        "",
        out string _,
        out string _,
        out strError);
                else if (_dbType == "order")
                    lRet = channel.GetOrderInfo(
        null,
        $"@path:{_itemRecordPath}",
        "xml",
        out item_xml,
        out item_recpath,
        out item_timestamp,
        "",
        out string _,
        out string _,
        out strError);
                else if (_dbType == "issue")
                    lRet = channel.GetIssueInfo(
        null,
        $"@path:{_itemRecordPath}",
        "xml",
        out item_xml,
        out item_recpath,
        out item_timestamp,
        "",
        out string _,
        out string _,
        out strError);
                else if (_dbType == "comment")
                    lRet = channel.GetCommentInfo(
        null,
        $"@path:{_itemRecordPath}",
        "xml",
        out item_xml,
        out item_recpath,
        out item_timestamp,
        "",
        out string _,
        out string _,
        out strError);
                else
                {
                    strError = $"暂时不支持 _dbType:{_dbType}";
                    goto ERROR1;
                }

                if (lRet == -1)
                {
                    strError = $"{condition} 条件测试 GetItems() {_dbType} 阶段出错: {strError}";
                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    strError = $"{condition} 条件测试 GetItems() {_dbType} 阶段出错:获得 {_itemRecordPath} 记录没有找到";
                    goto ERROR1;
                }

                // 测试前的 item_xml。用于测试步骤结束时还原记录内容
                var origin_item_xml = item_xml;

                // *** 保存修改
                if (StringUtil.IsInList("notchanged", condition))
                {
                    // notchanged 条件测试
                    // 册记录没有修改的原样的覆盖回去，并且 entity.Style 中不包含 outofrangeAsError

                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "",
                    };
                    results.Add(entity);

                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    if (output_entities.Length != 1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的实体记录数不等于 1，而是 {output_entities.Length}";
                        goto ERROR1;
                    }
                    if (output_entities[0].ErrorCode != ErrorCodeValue.NotChanged)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的错误码不是期望的 ErrorCodeValue.NotChanged, 而是 {output_entities[0].ErrorCode}";
                        goto ERROR1;
                    }
                    else
                    {
                        DataModel.SetMessage($"{condition} 条件测试 {SetItemsApiName()}() 成功", "green");
                    }
                }
                else if (StringUtil.IsInList("outofrange_01", condition))
                {
                    // outofrange_01 条件测试
                    // 册记录中有一个超出系统允许的元素 ttt，并且 entity.Style 中不包含 outofrangeAsError
                    /*
                    SetEntities() API 功能，当 entity.Style 中包含 outofrangeAsError 这个子参数值时，
                    如果提交的 XML 记录比数据库中的记录，修改的某些字段超出了系统允许的元素(见注)时，
                    比方说出现了一个名叫 ttt 的元素，那么就当作错误返回。
                    如果 entity.Style 中的并不包含 outofrangeAsError 这个子参数值时，则不会当作报错，
                    超出系统允许的元素被忽略保存。
                    * */
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(item_xml);
                        SetOutDefElement(item_dom,
                            out_def_element_name);
                        item_xml = item_dom.DocumentElement.OuterXml;
                    }

                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "",
                    };
                    results.Add(entity);

                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    if (output_entities.Length != 1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的实体记录数不等于 1，而是 {output_entities.Length}";
                        goto ERROR1;
                    }

                    // 验证实际保存的册记录
                    {
                        string style = "";
                        if (StringUtil.IsInList("file", condition))
                            style = "dont_has_file";
                        errors.AddRange(
                            VerifySavedItemRecord(output_entities[0].NewRecord,
        style,
        $"{condition} 条件测试 {SetItemsApiName()}() 阶段: ")
                            );
                        if (errors.Count > 0)
                            goto END1;
                    }

                    if (output_entities[0].ErrorCode != ErrorCodeValue.FullDenied)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的错误码不是期望的 ErrorCodeValue.FullDenied, 而是 {output_entities[0].ErrorCode}";
                        goto ERROR1;
                    }
                    else
                    {
                        DataModel.SetMessage($"outofrange_01 条件测试成功", "green");
                    }
                }
                else if (StringUtil.IsInList("outofrange_02", condition))
                {
                    // outofrange_02 条件测试
                    // 册记录中有一个超出系统允许的元素 ttt，并且 entity.Style 中包含 outofrangeAsError
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(item_xml);
                        SetOutDefElement(item_dom,
                            out_def_element_name);
                        item_xml = item_dom.DocumentElement.OuterXml;
                    }

                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "outofrangeAsError",
                    };
                    results.Add(entity);

                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    if (output_entities.Length != 1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的实体记录数不等于 1，而是 {output_entities.Length}";
                        goto ERROR1;
                    }

                    // 验证实际保存的册记录
                    {
                        string style = "";
                        if (StringUtil.IsInList("file", condition))
                            style = "dont_has_file";
                        errors.AddRange(
                            VerifySavedItemRecord(output_entities[0].NewRecord,
        style,
        $"{condition} 条件测试 {SetItemsApiName()}() 阶段: ")
                            );
                        if (errors.Count > 0)
                            goto END1;
                    }

                    if (output_entities[0].ErrorCode != ErrorCodeValue.CommonError)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的错误码不是期望的 ErrorCodeValue.CommonError, 而是 {output_entities[0].ErrorCode}";
                        goto ERROR1;
                    }
                    else
                    {
                        DataModel.SetMessage($"{condition} 条件测试 {SetItemsApiName()}() 成功", "green");
                    }
                }
                else if (StringUtil.IsInList("outofrange_03", condition))
                {
                    // outofrange_03 条件测试
                    // 册记录中有一个超出系统允许的元素 ttt，和一个并不超出系统允许的元素 comment,
                    // 并且 entity.Style 中不包含 outofrangeAsError
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(item_xml);
                        SetOutDefElement(item_dom,
                            out_def_element_name);
                        SetInDefElement(item_dom,
in_def_element_name);
                        item_xml = item_dom.DocumentElement.OuterXml;
                    }

                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "",
                    };
                    results.Add(entity);

                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    if (output_entities.Length != 1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的实体记录数不等于 1，而是 {output_entities.Length}";
                        goto ERROR1;
                    }

                    // 验证实际保存的册记录
                    {
                        string style = "";
                        if (StringUtil.IsInList("file", condition))
                            style = "has_file";

                        errors.AddRange(
                            VerifySavedItemRecord(output_entities[0].NewRecord,
        style,
        $"{condition} 条件测试 {SetItemsApiName()}() 阶段: ")
                            );
                        if (errors.Count > 0)
                            goto END1;
                    }

                    if (output_entities[0].ErrorCode != ErrorCodeValue.PartialDenied)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的错误码不是期望的 ErrorCodeValue.PartialDenied, 而是 {output_entities[0].ErrorCode}\r\n原始错误信息: {output_entities[0].ErrorInfo}";
                        goto ERROR1;
                    }
                    else
                    {
                        DataModel.SetMessage($"{condition} 条件测试 {SetItemsApiName()}() 成功", "green");
                    }
                }
                else if (StringUtil.IsInList("outofrange_04", condition))
                {
                    // outofrange_04 条件测试
                    // 册记录中有一个超出系统允许的元素 ttt，和一个并不超出系统允许的元素 comment,
                    // 并且 entity.Style 中包含 outofrangeAsError
                    {
                        XmlDocument item_dom = new XmlDocument();
                        item_dom.LoadXml(item_xml);
                        SetOutDefElement(item_dom,
                            out_def_element_name);
                        SetInDefElement(item_dom,
in_def_element_name);
                        item_xml = item_dom.DocumentElement.OuterXml;
                    }

                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "outofrangeAsError",
                    };
                    results.Add(entity);

                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    if (output_entities.Length != 1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的实体记录数不等于 1，而是 {output_entities.Length}";
                        goto ERROR1;
                    }

                    // 验证实际保存的册记录
                    {
                        string style = "";
                        if (StringUtil.IsInList("file", condition))
                            style = "has_file";
                        errors.AddRange(
                            VerifySavedItemRecord(output_entities[0].NewRecord,
        style,
        $"{condition} 条件测试 {SetItemsApiName()}() 阶段: ")
                            );
                        if (errors.Count > 0)
                            goto END1;
                    }


                    if (output_entities[0].ErrorCode != ErrorCodeValue.CommonError)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 阶段出错: 返回的错误码不是期望的 ErrorCodeValue.CommonError, 而是 {output_entities[0].ErrorCode}";
                        goto ERROR1;
                    }
                    else
                    {
                        DataModel.SetMessage($"{condition} 条件测试 {SetItemsApiName()}() 成功", "green");
                    }
                }

            END1:
                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                else
                {
                    List<EntityInfo> results = new List<EntityInfo>();
                    EntityInfo entity = new EntityInfo
                    {
                        Action = "change",
                        NewRecPath = item_recpath,
                        NewRecord = origin_item_xml,
                        OldTimestamp = item_timestamp,
                        Style = "outofrangeAsError",
                    };
                    results.Add(entity);
                    lRet = SetItems(
                        channel,
                        _biblio_recpath,
                        results.ToArray(),
                        ref item_timestamp,
                        out EntityInfo[] output_entities,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = $"{condition} 条件测试 {SetItemsApiName()}() 还原记录 XML 阶段出错: {strError}";
                        goto ERROR1;
                    }
                    DataModel.SetMessage($"{condition} 条件测试 {SetItemsApiName()}() 还原记录 XML");
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
                //if (_dbType == "comment")
                //    DataModel.ReturnChannel(channel);
                //else
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

        // 验证实际保存的册记录
        static List<string> VerifySavedItemRecord(string item_xml,
            string style,
            string prefix = "")
        {
            XmlDocument item_dom = new XmlDocument();
            item_dom.LoadXml(item_xml);

            List<string> errors = new List<string>();

            // 要求具备 dprms:file 元素
            if (StringUtil.IsInList("has_file,dont_has_file", style))
            {
                var nsmgr = new XmlNamespaceManager(item_dom.NameTable);
                nsmgr.AddNamespace("dprms", "http://dp2003.com/dprms");
                var file_elements = item_dom.DocumentElement.SelectNodes("dprms:file", nsmgr);
                var found_elements = file_elements.Cast<XmlElement>().Where(x => x.GetAttribute("id") == "1").ToList();
                if (StringUtil.IsInList("has_file", style))
                {
                    if (found_elements.Count == 0)
                    {
                        errors.Add($"{prefix}实际保存的 XML 中没有找到 id 为 \"1\" 的 dprms:file 元素");
                    }
                    else if (found_elements.Count > 1)
                    {
                        var indent_xml = DomUtil.GetIndentXml(item_dom.DocumentElement);
                        errors.Add($"{prefix}实际保存的 XML 中id 为 \"1\" 的 dprms:file 元素数量多于 1 个({found_elements.Count})\r\n{indent_xml}");
                    }
                }

                if (StringUtil.IsInList("dont_has_file", style))
                {
                    if (found_elements.Count > 0)
                    {
                        errors.Add($"实际保存的 XML 中不应该存在 id 为 \"1\" 的 dprms:file 元素(但现在有 {found_elements.Count} 个)");
                    }
                }
            }

            return errors;
        }

        static string SetItemsApiName()
        {
            if (_dbType == "item")
                return "SetEntities";
            else if (_dbType == "order")
                return "SetOrders";
            else if (_dbType == "issue")
                return "SetIssues";
            else if (_dbType == "comment")
                return "SetComments";
            else
                throw new ArgumentException("_dbType error");
        }

        static long GetItem(LibraryChannel channel,
            string item_recpath,
            out string item_xml,
            out byte[] item_timestamp,
            out string strError)
        {
            strError = "";
            item_timestamp = null;
            item_xml = "";

            if (_dbType == "item")
                return channel.GetItemInfo(
    null,
    $"@path:{item_recpath}",
    "xml",
    out item_xml,
    out item_recpath,
    out item_timestamp,
    "",
    out string _,
    out string _,
    out strError);
            else if (_dbType == "order")
                return channel.GetOrderInfo(
    null,
    $"@path:{item_recpath}",
    "xml",
    out item_xml,
    out item_recpath,
    out item_timestamp,
    "",
    out string _,
    out string _,
    out strError);
            else if (_dbType == "issue")
                return channel.GetIssueInfo(
    null,
    $"@path:{item_recpath}",
    "xml",
    out item_xml,
    out item_recpath,
    out item_timestamp,
    "",
    out string _,
    out string _,
    out strError);
            else if (_dbType == "comment")
                return channel.GetCommentInfo(
    null,
    $"@path:{item_recpath}",
    "xml",
    out item_xml,
    out item_recpath,
    out item_timestamp,
    "",
    out string _,
    out string _,
    out strError);
            else
            {
                strError = $"暂时不支持 _dbType:{_dbType}";
                return -1;
            }
        }

        static long SetItems(LibraryChannel channel,
            string biblio_recpath,
            EntityInfo[] items,
            ref byte[] item_timestamp,
            out EntityInfo[] output_items,
            out string strError)
        {
            long ret = -1;
            if (_dbType == "item")
                ret = channel.SetEntities(
        null,
        biblio_recpath,
        items,
        out output_items,
        out strError);
            else if (_dbType == "order")
                ret = channel.SetOrders(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else if (_dbType == "issue")
                ret = channel.SetIssues(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else if (_dbType == "comment")
                ret = channel.SetComments(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else
                throw new ArgumentException($"未知的 _dbType: '{_dbType}'");

            if (output_items != null && output_items.Length > 0
                && output_items[0].NewTimestamp != null)
            {
                item_timestamp = output_items[0].NewTimestamp;
            }

            if (string.IsNullOrEmpty(strError)
                && ret == -1)
                throw new ArgumentException($"SetItems() 中调用 SetXXX() API 的 strError 不该返回空");
            return ret;
        }

        #region

        public delegate void delegate_restore_user_rights();

        // 测试“和修改有关的”账户权限
        // parameters:
        //      condition   "normal" 普通权限(缺省)
        //                  "access" 存取定义权限
        //                  "negative" 反向权限
        //                  "positive"  正向权限(缺省)
        //                  "mix" 混合权限
        //                  normal 或 access 可组合 positive 或 negative
        //                  "file" 包含 dprms:file 元素
        public static NormalResult TestWriteRights(
    string condition)
        {
            string strError = "";

            bool negative = StringUtil.IsInList("negative", condition);
            bool access = StringUtil.IsInList("access", condition);
            bool mix = StringUtil.IsInList("mix", condition);

            bool file = StringUtil.IsInList("file", condition);

            // 用于构造账户的通道
            var super_channel = DataModel.GetChannel();

            TimeSpan old_timeout = super_channel.Timeout;
            super_channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                string strItemDbName = StringUtil.GetDbName(_itemRecordPath);

                // 准备一个账户 test_normal_account
                string rights_template = "";
                string access_string_template = "";
                if (access)
                {
                    rights_template = "";

                    if (file == false)
                    {
                        access_string_template = $"{strItemDbName}:set{{dbtype}}info=new,change,delete|get{{dbtype}}info";

                        if (negative == true)
                        {
                            if (mix == true)
                                access_string_template = $"{strItemDbName}changed:set{{dbtype}}info=new,change,delete|get{{dbtype}}info|{strItemDbName}changed:set{{dbtype}}info=|get{{dbtype}}info";
                            else
                                access_string_template = $"{strItemDbName}:set{{dbtype}}info=|get{{dbtype}}info";
                        }
                    }
                    else // file == true
                    {
                        access_string_template = $"{strItemDbName}:set{{dbtype}}info=new,change,delete|get{{dbtype}}info";

                        if (negative == false)
                            access_string_template += "|set{dbtype}object|get{dbtype}object";
                        else // negative == true
                        {
                            // file + negative 表示 object 部分权限反向测试。不是指普通权限反向测试
                            access_string_template += "|set{dbtype}object=|get{dbtype}object=";
                        }
                    }
                }
                else
                {
                    rights_template = "set{dbtype}info,get{dbtype}info";
                    if (file && negative == false)
                        rights_template += ",set{dbtype}object,get{dbtype}object";
                    access_string_template = "";
                    if (negative == true)
                    {
                        if (file)
                            rights_template = "set{dbtype}info,get{dbtype}info";    // 故意没有 set{dbtype}object 权限
                        else
                            rights_template = "";
                    }
                }

                var result = PrepareWritingAccounts(super_channel,
    "test_normal_account",
    rights_template,
    access_string_template
    );
                if (result.Value == -1)
                    return result;

                // 用于写入的通道。也就是被测试的通道
                var channel = DataModel.NewChannel("test_normal_account", "");
                try
                {
                    // new item record
                    result = WriteNew(channel,
                        condition,
                        out string item_recpath,
                        out byte[] item_timestamp);
                    if (result.Value == -1)
                        return result;
                    // 补充创建一条记录用于后继 negative 状态下测试 change 和 delete
                    if (negative && file == false)
                    {
                        result = WriteNew(super_channel,
    condition.Replace("negative", ""),
    out item_recpath,
    out item_timestamp);
                        if (result.Value == -1)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"辅助创建失败。原始出错信息: {result.ErrorInfo}"
                            };
                        }
                    }

                    // change item record
                    result = WriteChange(ref channel,
                        condition,
                        item_recpath,
                        () =>
                        {
                            var ret = PrepareWritingAccounts(super_channel,
"test_normal_account",
rights_template,
access_string_template
);
                            if (ret.Value == -1)
                                throw new Exception($"恢复原有账户权限时出错: {ret.ErrorInfo}");
                        },
                        ref item_timestamp);
                    if (result.Value == -1)
                        return result;

                    // delete item record
                    result = WriteDelete(channel,
                        condition,
                        item_recpath,
                        item_timestamp);
                    if (result.Value == -1)
                        return result;
                }
                finally
                {
                    DataModel.DeleteChannel(channel);
                }

                return new NormalResult();
            }
            finally
            {
                super_channel.Timeout = old_timeout;
                DataModel.ReturnChannel(super_channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestWriteRights() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static NormalResult PrepareWritingAccounts(LibraryChannel channel,
    string userName,
    string rights_template,
    string access_template)
        {
            var rights = rights_template?.Replace("{dbtype}", _dbType);
            var access = access_template?.Replace("{dbtype}", _dbType);
            return Utility.PrepareAccount(channel,
                new UserInfo
                {
                    UserName = userName,
                    Rights = rights,
                    Access = access
                });
        }

#if REMOVED
        // 准备“写入用途”的账户
        static NormalResult PrepareWritingAccounts(LibraryChannel channel,
            string userName,
            string rights_template,
            string access_template)
        {
            DataModel.SetMessage($"正在删除用户 {userName} ...");
            var nRet = Utility.DeleteUsers(channel,
                new string[] { userName },
                out string strError);
            if (nRet == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            // 创建

            // 通过 rights_template 定义，能正常访问实体记录
            DataModel.SetMessage($"正在创建用户 {userName} ...");
            var rights = rights_template?.Replace("{dbtype}", _dbType);
            var access = access_template?.Replace("{dbtype}", _dbType);
            var lRet = channel.SetUser(null,
                "new",
                new UserInfo
                {
                    UserName = userName,
                    Rights = rights,
                    Access = access,
                },
                out strError);
            if (lRet == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            return new NormalResult();
        }
#endif

        static NormalResult WriteNew(LibraryChannel channel,
            string condition,
            out string item_recpath,
            out byte[] item_timestamp)
        {
            item_timestamp = null;
            item_recpath = "";

            bool negative = StringUtil.IsInList("negative", condition);
            bool file = StringUtil.IsInList("file", condition);

            string item_xml = @"<root>
<barcode></barcode>
<location>阅览室</location>
<bookType>普通</bookType>
<price>CNY12.00</price>
</root>";
            if (_dbType == "issue")
            {
                item_xml = _issueXml;
            }
            if (_dbType == "order")
            {
                item_xml = _orderXml;
            }
            if (_dbType == "comment")
            {
                item_xml = _commentXml;
            }

            // 2025/10/18
            {
                string in_def_element_name = "comment"; // 定义范围内的元素名
                if (_dbType == "comment")
                    in_def_element_name = "summary"; // 评注库没有 comment 元素

                if (StringUtil.IsInList("file", condition))
                    in_def_element_name = "http://dp2003.com/dprms:file";

                string out_def_element_name = "ttt";    // 定义范围外的元素名


                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(item_xml);
                /* 
                SetOutDefElement(item_dom,
        out_def_element_name);
                */
                SetInDefElement(item_dom,
    in_def_element_name);
                item_xml = item_dom.OuterXml;
            }

            // string strItemDbName = StringUtil.GetDbName(_itemRecordPath);

            // TODO: 清空下级库?

            List<EntityInfo> items = new List<EntityInfo>();
            EntityInfo item = new EntityInfo
            {
                Action = "new",
                NewRecPath = "",
                NewRecord = item_xml,
                OldTimestamp = null,
                Style = "",
            };
            items.Add(item);
            item_timestamp = null;
            var ret = SetItems(channel,
    _biblio_recpath,
    items.ToArray(),
    ref item_timestamp,
    out EntityInfo[] output_items,
    out string strError);
            if (ret == -1)
            {
                if (negative && channel.ErrorCode == ErrorCode.AccessDenied)
                    return new NormalResult();

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            // 验证 output_items 中全是成功
            if (output_items.Length != 1)
            {
                strError = $"WriteNew() error: 返回的实体记录数不等于 1，而是 {output_items.Length}";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            if (negative)
            {
                // 2025/10/18
                if (file && output_items[0].ErrorCode == ErrorCodeValue.PartialDenied)
                    goto END1;  // 还需要得到 recpath 和 timestamp

                else if (output_items[0].ErrorCode == ErrorCodeValue.AccessDenied)
                    return new NormalResult();

                if (file)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteNew() {condition} 创建 {_dbType} 记录时出现错误: 期待返回错误码 {ErrorCodeValue.PartialDenied}，但返回了 {output_items[0].ErrorCode} ({output_items[0].ErrorInfo})"
                    };
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteNew() {condition} 创建 {_dbType} 记录时出现错误: 期待返回错误码 {ErrorCodeValue.AccessDenied}，但返回了 {output_items[0].ErrorCode} ({output_items[0].ErrorInfo})"
                    };
            }
            else
            {
                if (output_items[0].ErrorCode != ErrorCodeValue.NoError)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteNew() 创建 {_dbType} 记录时出现错误: {output_items[0].ErrorInfo}"
                    };
            }

        END1:
            item_recpath = output_items[0].NewRecPath;
            DataModel.SetMessage($"{channel.UserName} 身份创建 {_dbType} 记录 {item_recpath} 成功({condition})", "green");
            return new NormalResult();
        }

        // parameters:
        //      channel [ref] 中途可能更换通道。要让调主感知到
        static NormalResult WriteChange(ref LibraryChannel channel,
            string condition,
            string item_recpath,
            delegate_restore_user_rights func_restore,
            ref byte[] item_timestamp)
        {
            bool negative = StringUtil.IsInList("negative", condition);
            bool file = StringUtil.IsInList("file", condition);

            var ret = GetItem(channel,
    item_recpath,
    out string item_xml,
    out item_timestamp,
    out string strError);
            if (ret == -1)
            {
                //if (negative && file && channel.ErrorCode == ErrorCode.PartialDenied)   // dprms:file 元素被拒绝
                //    return new NormalResult();

                if (negative && channel.ErrorCode == ErrorCode.AccessDenied)
                    return new NormalResult();

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            string in_def_element_name = "comment"; // 定义范围内的元素名
            if (_dbType == "comment")
                in_def_element_name = "summary"; // 评注库没有 comment 元素

            if (StringUtil.IsInList("file", condition))
                in_def_element_name = "http://dp2003.com/dprms:file";

            string out_def_element_name = "ttt";    // 定义范围外的元素名


            XmlDocument item_dom = new XmlDocument();
            item_dom.LoadXml(item_xml);

            if (_dbType == "item")
            {
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "comment",
                    $"{DateTime.Now.ToString()} 修改");
            }
            else if (_dbType == "issue")
            {
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "comment",
                    $"{DateTime.Now.ToString()} 修改");
            }
            else if (_dbType == "order")
            {
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "comment",
                    $"{DateTime.Now.ToString()} 修改");
            }
            else if (_dbType == "comment")
            {
                DomUtil.SetElementText(item_dom.DocumentElement,
                    "summary",
                    $"{DateTime.Now.ToString()} 修改");
            }

            if (file)
                SetInDefElement(item_dom,
    in_def_element_name);

            List<EntityInfo> items = new List<EntityInfo>();
            EntityInfo item = new EntityInfo
            {
                Action = "change",
                NewRecPath = item_recpath,
                NewRecord = item_dom.DocumentElement.OuterXml,
                OldTimestamp = item_timestamp,
                Style = "",
            };
            items.Add(item);
            ret = SetItems(channel,
    _biblio_recpath,
    items.ToArray(),
    ref item_timestamp,
    out EntityInfo[] output_items,
    out strError);
            if (ret == -1)
            {
                if (negative && channel.ErrorCode == ErrorCode.AccessDenied)
                    return new NormalResult();
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // 验证 output_items 中全是成功
            if (output_items.Length != 1)
            {
                strError = $"WriteChange() error: 返回的实体记录数不等于 1，而是 {output_items.Length}";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            if (negative)
            {
                if (file && output_items[0].ErrorCode == ErrorCodeValue.PartialDenied)
                {
                    goto CONTINUE;
                    return new NormalResult();
                }
                else if (output_items[0].ErrorCode == ErrorCodeValue.AccessDenied)
                    return new NormalResult();

                if (file)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteChange({condition}) 写入 {_dbType} 记录时出现错误: 期待返回错误码 {ErrorCodeValue.PartialDenied}，但返回了 {output_items[0].ErrorCode} ({output_items[0].ErrorInfo})"
                    };
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteChange({condition}) 写入 {_dbType} 记录时出现错误: 期待返回错误码 {ErrorCodeValue.AccessDenied}，但返回了 {output_items[0].ErrorCode} ({output_items[0].ErrorInfo})"
                    };
            }
            else
            {
                if (output_items[0].ErrorCode != ErrorCodeValue.NoError)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteChange() 写入 {_dbType} 记录时出现错误: {output_items[0].ErrorInfo}"
                    };
            }

        CONTINUE:
            item_recpath = output_items[0].NewRecPath;
            item_xml = output_items[0].NewRecord;

            if (negative && file)
            {
                // 重新写入一条含有 dprms:file 元素的记录。需要临时升级账户的权限，增加 setxxxobject 权限
                var change_ret = ChangeUserRights(channel.UserName,
                    "set{dbtype}object,get{dbtype}object,set{dbtype}info,get{dbtype}info",
                    "");
                if (change_ret.Value == -1)
                    return change_ret;
#if REMOVED
                var super_channel = DataModel.GetChannel();
                try
                {
                    // TODO: 检查，账户权限修改后，dp2library 是否自动切断了这个用户名的所有活跃通道？
                    var result = PrepareWriteAccounts(super_channel,
    channel.UserName,   // "test_normal_account",
    "set{dbtype}object,get{dbtype}object,set{dbtype}info,get{dbtype}info",
    ""
    );
                    if (result.Value == -1)
                        return result;
                }
                finally
                {
                    DataModel.ReturnChannel(super_channel);
                }
#endif
                // 权限改变后，重新登录
                var username = channel.UserName;
                DataModel.DeleteChannel(channel);
                channel = DataModel.NewChannel(username, "");

                items[0].OldTimestamp = item_timestamp;
                ret = SetItems(channel,
_biblio_recpath,
items.ToArray(),
ref item_timestamp,
out output_items,
out strError);
                if (ret == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }

                if (output_items[0].ErrorCode != ErrorCodeValue.NoError)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = output_items[0].ErrorInfo,
                        ErrorCode = output_items[0].ErrorCode.ToString()
                    };

                item_recpath = output_items[0].NewRecPath;
                item_xml = output_items[0].NewRecord;

                // 恢复先前的权限
                {
                    func_restore();

                    DataModel.DeleteChannel(channel);
                    channel = DataModel.NewChannel(username, "");
                }
            }

            // 检查返回的 XML 中是否具备 refID 元素内容
            {
                XmlDocument item_dom2 = new XmlDocument();
                item_dom2.LoadXml(item_xml);
                var refid = DomUtil.GetElementText(item_dom2.DocumentElement,
                    "refID");
                if (string.IsNullOrEmpty(refid))
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteChange({condition}) {_dbType} 返回的 XML 中不应该缺乏 refID 元素内容"
                    };
                }
            }

            DataModel.SetMessage($"{channel.UserName} 身份修改 {_dbType} 记录 {item_recpath} 成功({condition})", "green");
            return new NormalResult();
        }

        static NormalResult ChangeUserRights(string username,
            string rights_template,
            string access_string_template)
        {
            var super_channel = DataModel.GetChannel();
            try
            {
                var result = PrepareWritingAccounts(super_channel,
username,   // "test_normal_account",
rights_template,
access_string_template
);
                if (result.Value == -1)
                    return result;
                return new NormalResult();
            }
            finally
            {
                DataModel.ReturnChannel(super_channel);
            }
        }

        static NormalResult WriteDelete(LibraryChannel channel,
            string condition,
            string item_recpath,
            byte[] item_timestamp)
        {
            bool negative = StringUtil.IsInList("negative", condition);
            bool file = StringUtil.IsInList("negative", condition);

            // TODO: negative + file 应该先写入成功一条含有 dprms:file 元素的记录，
            // 然后再尝试删除，并期待返回错误码 PartialDenied

            List<EntityInfo> items = new List<EntityInfo>();
            EntityInfo item = new EntityInfo
            {
                Action = "delete",
                OldRecPath = item_recpath,
                OldTimestamp = item_timestamp,
                Style = "",
            };
            items.Add(item);
            item_timestamp = null;
            var ret = SetItems(channel,
    _biblio_recpath,
    items.ToArray(),
    ref item_timestamp,
    out EntityInfo[] output_items,
    out string strError);
            if (ret == -1)
            {
                if (negative && channel.ErrorCode == ErrorCode.AccessDenied)
                    return new NormalResult();

                Debug.Assert(string.IsNullOrEmpty(strError) == false);
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // 验证 output_items 中全是成功
            if (output_items.Length != 1)
            {
                strError = $"WriteDelete() error: 返回的实体记录数不等于 1，而是 {output_items.Length}";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            if (negative)
            {
                if (file && output_items[0].ErrorCode == ErrorCodeValue.PartialDenied)
                    return new NormalResult();
                if (output_items[0].ErrorCode != ErrorCodeValue.AccessDenied)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteDelete() 删除 {_dbType} 记录时出现错误: 期待返回错误码 {ErrorCodeValue.AccessDenied}，但返回了 {output_items[0].ErrorCode} ({output_items[0].ErrorInfo})"
                    };
                return new NormalResult();
            }
            else
            {
                if (output_items[0].ErrorCode != ErrorCodeValue.NoError)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"WriteDelete() 删除 {_dbType} 记录时出现错误: {output_items[0].ErrorInfo}"
                    };
            }
            DataModel.SetMessage($"{channel.UserName} 身份删除 {_dbType} 记录 {item_recpath} 成功", "green");
            return new NormalResult();
        }

        #endregion

#if TODO_
        #region 尚未完成

        public delegate void delegate_get_user_rights(
            out string rights,
            out string access);
        // 验证新创建、修改、删除的结果
        public delegate void delegate_verify_item_result(
            NormalResult result,
            EntityInfo entity);

        // 针对两阶段弱点进行测试。所谓两阶段，就是 dp2library 在验证权限阶段，第一阶段针对所有数据库验证当前账户是否大致具备存取定义权限，第二阶段才针对具体的数据库精确验证村粗定义权限。测试方法是故意定义错位的权限，然后观察 dp2library 是否的实现过程是否有漏洞
        // 验证回调函数式编程结构

        public static NormalResult TestTwoStageWeak()
        {
            return _test(
                (out string rights, out string access_string) =>
                {
                },
                (ret, entity) =>
                {
                    //new
                },
                (ret, entity) =>
                {
                    // change
                },
                (ret, entity) =>
                {
                    // delete
                }
                );
        }

        static NormalResult _test(delegate_get_user_rights func_get_user_rights,
            delegate_verify_item_result func_verify_new,
            delegate_verify_item_result func_verify_change,
            delegate_verify_item_result func_verify_delete)
        {
            // 用于构造账户的通道
            var super_channel = DataModel.GetChannel();

            TimeSpan old_timeout = super_channel.Timeout;
            super_channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                // *** 获得一个账户的权限
                func_get_user_rights(out string rights_template, out string access_string_template);

                var result = PrepareWritingAccounts(super_channel,
"test_normal_account",
rights_template,
access_string_template
);
                if (result.Value == -1)
                    return result;

                // 用于写入的通道。也就是被测试的通道
                var test_channel = DataModel.NewChannel("test_normal_account", "");
                try
                {
                    // new item record
                    result = WriteNew(test_channel,
                        "",
                        out string item_recpath,
                        out byte[] item_timestamp);
                    if (result.Value == -1)
                        return result;
                }
                finally
                {
                    DataModel.DeleteChannel(test_channel);
                }

                return new NormalResult();
            }
            finally
            {
                super_channel.Timeout = old_timeout;
                DataModel.ReturnChannel(super_channel);
            }
        }

        #endregion

#endif
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

                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(_user_names, ",")} ...");
                int nRet = Utility.DeleteUsers(channel,
                    _user_names,
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

        static List<string> _user_names = new List<string>() {
                    "test_rights",
                    "test_cannot",
                };

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
    }

}
