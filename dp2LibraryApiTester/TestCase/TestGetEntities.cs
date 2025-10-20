using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester.TestCase
{
    /// <summary>
    /// 测试 GetEntities() GetOrders() GetIssues() GetComments() API
    /// </summary>
    public static class TestGetEntities
    {
        static string _dbType = "item"; // order/issue/comment

        public static NormalResult TestAll(string dbType,
            CancellationToken token)
        {
            _dbType = dbType;

            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            {
                result = TestReadRights("");
                if (result.Value == -1) return result;

                result = TestReadRights("file");
                if (result.Value == -1) return result;

                token.ThrowIfCancellationRequested();

                result = TestReadRights("access");
                if (result.Value == -1) return result;

                result = TestReadRights("access,file");
                if (result.Value == -1) return result;
            }

            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

        // 测试“和读取有关的”账户权限
        // parameters:
        //      condition   "normal" 普通权限(缺省)
        //                  "access" 存取定义权限
        //                  "negative" 反向权限
        //                  "positive"  正向权限(缺省)
        //                  "mix" 混合权限
        //                  normal 或 access 可组合 positive 或 negative
        //                  "file" 包含 dprms:file 元素
        public static NormalResult TestReadRights(
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

                var result = PrepareReadingAccounts(super_channel,
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
                    var ret = GetItems(channel,
                        _biblio_recpath,
                        out EntityInfo[] infos,
                        out strError);
                    if (ret == -1)
                        goto ERROR1;

                    strError = Utility.GetError(infos, out ErrorCodeValue error_code);
                    if (strError != null)
                    {
                        goto ERROR1;
                    }

                    // TODO: 验证获得的内容是否正确
                    result = VerifyInfos(condition, infos);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }
                }
                finally
                {
                    DataModel.DeleteChannel(channel);
                }

                DataModel.SetMessage($"TestReadRights({_dbType} {condition}) 测试成功", "green");
                return new NormalResult();
            }
            finally
            {
                super_channel.Timeout = old_timeout;
                DataModel.ReturnChannel(super_channel);
            }

        ERROR1:
            // DataModel.SetMessage($"TestReadRights({condition}) error(_dbType={_dbType}): \r\n{strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = $"TestReadRights({condition}) error(_dbType={_dbType}): \r\n{strError}"
            };
        }

        static long GetItems(LibraryChannel channel,
    string biblio_recpath,
    out EntityInfo[] infos,
    out string strError)
        {
            strError = "";

            if (_dbType == "item")
                return channel.GetEntities(
    null,
    biblio_recpath,
    0,
    -1,
    "",
    "zh",
    out infos,
    out strError);
            else if (_dbType == "order")
                return channel.GetOrders(
    null,
    biblio_recpath,
    0,
    -1,
    "",
    "zh",
    out infos,
    out strError);
            else if (_dbType == "issue")
                return channel.GetIssues(
    null,
    biblio_recpath,
    0,
    -1,
    "",
    "zh",
    out infos,
    out strError);
            else if (_dbType == "comment")
                return channel.GetComments(
    null,
    biblio_recpath,
    0,
    -1,
    "",
    "zh",
    out infos,
    out strError);
            else
            {
                strError = $"暂时不支持 _dbType:{_dbType}";
                infos = null;
                return -1;
            }
        }


        // 准备“读取用途”的账户
        static NormalResult PrepareReadingAccounts(LibraryChannel channel,
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
            var rights = rights_template?.Replace("{dbtype}", _dbType);
            var access = access_template?.Replace("{dbtype}", _dbType);
            DataModel.SetMessage($"正在创建用户 {userName}  普通权限:'{rights}'  存取定义:'{access}' ...");
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
                            var entities = BuildEntityRecords((d) =>
                            {
                                DomUtil.SetElementText(d.DocumentElement, "parent", ResPath.GetRecordId(output_biblio_recpath));
                                if (_dbType == "comment")
                                {
                                    DomUtil.SetElementText(d.DocumentElement, "creator", channel.UserName);
                                }
                            });

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
<parent>612</parent>
<barcode>X0000001</barcode>
<location>阅览室</location>
<bookType>普通</bookType>
<price>CNY12.00</price>
</root>";

        static string _issueXml = @"<root>
<parent>612</parent>
<publishTime>20150101</publishTime>
<issue>1</issue>
<zong>1</zong>
<volume>1</volume>
</root>";

        static string _orderXml = @"<root>
<parent>612</parent>
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

        delegate void delegate_changeXml(XmlDocument dom);

        static EntityInfo[] BuildEntityRecords(delegate_changeXml func_change_xml)
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


            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 先删除已有的 dprms:file 元素
            RemoveDprmsFileElements(dom);
            // 添加 dprms:file 元素
            AddDprmsFileElement(dom, "1");

            func_change_xml(dom);

            xml = DomUtil.GetIndentXml(dom.DocumentElement.OuterXml);
            // 设置回静态变量，便于后面比对
            if (_dbType == "item")
                _itemXml = xml;
            else if (_dbType == "issue")
                _issueXml = xml;
            else if (_dbType == "order")
                _orderXml = xml;
            else if (_dbType == "comment")
                _commentXml = xml;

            List<EntityInfo> results = new List<EntityInfo>();
            EntityInfo entity = new EntityInfo
            {
                Action = "new",
                NewRecord = xml,
            };
            results.Add(entity);
            return results.ToArray();
        }

        static NormalResult VerifyInfos(
            string condition,
            EntityInfo[] infos)
        {
            var file = StringUtil.IsInList("file", condition);

            List<string> errors = new List<string>();
            if (infos.Length != 1)
            {
                errors.Add($"infos.Length 应该为 1。但现在是 {infos.Length}");
                goto END1;
            }

            // 参考用的 XML
            XmlDocument ref_dom = new XmlDocument();
            {
                if (_dbType == "item")
                    ref_dom.LoadXml(_itemXml);
                else if (_dbType == "order")
                    ref_dom.LoadXml(_orderXml);
                else if (_dbType == "issue")
                    ref_dom.LoadXml(_issueXml);
                else if (_dbType == "comment")
                    ref_dom.LoadXml(_commentXml);
                else
                    throw new ArgumentException($"无法识别的 _dbtype '{_dbType}'");
            }

            {
                // 非 file condition 下，把参考的 XML 中的 dprms:file 元素临时去掉，用于比较
                if (!file)
                {
                    RemoveDprmsFileElements(ref_dom);
                }
            }

            {
                XmlDocument data_dom = new XmlDocument();
                data_dom.LoadXml(infos[0].OldRecord);

                var current = ContainsElements(ref_dom, data_dom);
                if (current.Count > 0)
                    errors.AddRange(current);

                {
                    if (!file)
                    {
                        var outer_xmls = GetElementOuterXmls(data_dom.DocumentElement,
                            "DpNs.dprms:file");
                        if (outer_xmls.Count > 0)
                            errors.Add($"非 file condition 情形，从获得的 XML 记录中发现了 dprms:file 元素: \r\n{StringUtil.MakePathList(outer_xmls, "\r\n")}");
                    }
                }
            }
        END1:
            if (errors.Count == 0)
                return new NormalResult();

            return new NormalResult
            {
                Value = -1,
                ErrorInfo = StringUtil.MakePathList(errors, "\r\n")
            };
        }

        static int RemoveDprmsFileElements(XmlDocument dom, string id = null)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            int count = 0;
            var nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach(XmlElement file in nodes)
            {
                if (string.IsNullOrEmpty(id) == false
                    && file.GetAttribute("id") != id)
                    continue;
                file.ParentNode.RemoveChild(file);
                count++;
            }

            return count;
        }

        static void AddDprmsFileElement(XmlDocument dom, string id)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            var element = dom.CreateElement("dprms","file", DpNs.dprms);
            dom.DocumentElement.AppendChild(element);
            element.SetAttribute("id", id);
        }

        // 验证 dom1 中的元素都包含在 dom2 中了
        // 注: 但并不意味着两边完全相同。有可能 dom2 中有一些 dom1 不具备的元素
        static List<string> ContainsElements(XmlDocument dom1,
            XmlDocument dom2)
        {
            var errors = new List<string>();
            foreach (XmlNode node in dom1.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                var name = GetName(node);
                var xmls1 = GetElementOuterXmls(dom1.DocumentElement, name);
                var xmls2 = GetElementOuterXmls(dom2.DocumentElement, name);
                if (xmls1.Count != xmls2.Count)
                    errors.Add($"-- {name} 元素在两侧数量不同:\r\n左侧:{xmls1.Count} {StringUtil.MakePathList(xmls1, "\r\n")}\r\n右侧:{xmls2.Count} {StringUtil.MakePathList(xmls2, "\r\n")}");
                else
                {
                    xmls1.Sort();
                    xmls2.Sort();
                    if (xmls1.SequenceEqual(xmls2) == false)
                        errors.Add($"-- {name} 元素在两侧内容不同:\r\n左侧:{xmls1.Count} {StringUtil.MakePathList(xmls1, "\r\n")}\r\n右侧:{xmls2.Count} {StringUtil.MakePathList(xmls2, "\r\n")}");
                }
            }

            return errors;
        }

        static string GetName(XmlNode node)
        {
            return node.NamespaceURI + ":" + node.Name;
        }

        static List<string> GetElementOuterXmls(XmlElement parent,
            string name)
        {
            var results = new List<string>();
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    var current_name = GetName(node);
                    if (current_name != name)
                        continue;
                    results.Add(node.OuterXml.Replace("\n", "\r").Trim(' ', '\t', '\r'));
                }
            }

            return results;
        }

        static string CompareXmls(string xml1, string xml2)
        {
            xml1 = DomUtil.GetIndentXml(xml1);
            xml2 = DomUtil.GetIndentXml(xml2);
            if (xml1 == xml2)
                return null;
            return $"两个 XML 不相同:\r\nxml1={xml1}\r\nxml2={xml2}";
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

    }
}
