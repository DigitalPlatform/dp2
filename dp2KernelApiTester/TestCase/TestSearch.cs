﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;

// TODO: 获取记录 prev next 风格
// TODO: XML 记录和检索点中包含 0 字符和非法字符
// TODO: __id 检索返回的结果应该是有序的

namespace dp2KernelApiTester
{
    // 测试检索功能
    public static class TestSearch
    {
        static string _strDatabaseName = "__test";
        static string _strDatabaseName1 = "__test1";
        static string _strDatabaseName2 = "__test2";

        public static NormalResult TestAll(
            CancellationToken token,
            string style = null)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(2, token);
                if (create_result.Value == -1)
                    return create_result;

                var search_result = TestSingleDbLogicSearch(token);
                if (search_result.Value == -1)
                    return search_result;

                search_result = TestSingleDbIdSearch(token);
                if (search_result.Value == -1)
                    return search_result;

                search_result = TestMultiDbLogicSearch(token);
                if (search_result.Value == -1)
                    return search_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;
            }

            {
                var result = UnicodeSearch(token);
                if (result.Value == -1)
                    return result;
            }

            {
                var result = Finish();
                if (result.Value == -1)
                    return result;
            }

            return new NormalResult();
        }

        static string[] database_names = new string[] {
                _strDatabaseName,
                _strDatabaseName1,
                _strDatabaseName2 };

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            foreach (var database_name in database_names)
            {
                List<string[]> logicNames = new List<string[]>();
                {
                    logicNames.Add(new string[]
                    {
                    database_name,
                    "zh",
                    });
                }

                string strKeysDef = @"
<root>
  <key>
    <xpath>*/barcode</xpath>
    <from>barcode</from>
    <table ref='barcode' />
  </key>
  <table name='barcode' id='2'>
    <caption lang='zh-CN'>册条码号</caption>
    <caption lang='zh-CN'>册条码</caption>
    <caption lang='en'>barcode</caption>
  </table>
  <key>
    <xpath>*/batchNo</xpath>
    <from>batchno</from>
    <table ref='batchno' />
  </key>
  <table name='batchno' id='3'>
    <caption lang='zh-CN'>批次号</caption>
    <caption lang='en'>Batch No</caption>
  </table>
  <key>
    <xpath>*/registerNo</xpath>
    <from>registerno</from>
    <table ref='registerno' />
  </key>
  <table name='registerno' id='4'>
    <caption lang='zh-CN'>登录号</caption>
    <caption lang='en'>Register No</caption>
  </table>

  <key>
    <xpath>*/location</xpath>
    <from>location</from>
    <table ref='location' />
  </key>
  <table name='location' id='6'>
    <convert>
      <string style='split,upper'/>
    </convert>
    <convertquery>
      <string style='upper' />
    </convertquery>
    <caption lang='zh-CN'>馆藏地点</caption>
    <caption lang='en'>Location</caption>
  </table>

  <key>
    <xpath>*/refID</xpath>
    <from>refID</from>
    <table ref='refID' />
  </key>
  <table name='refID' id='7'>
    <caption lang='zh-CN'>参考ID</caption>
    <caption lang='en'>Reference ID</caption>
  </table>

  <key>
    <xpath>*/parent</xpath>
    <from>parent</from>
    <table ref='parent' />
  </key>
  <table name='parent' id='1'>
    <caption lang='zh-CN'>父记录</caption>
    <caption lang='en'>Parent</caption>
  </table>

  <key>
    <xpath>*/state</xpath>
    <from>state</from>
    <table ref='state' />
  </key>
  <table name='state' id='9'>
    <convert>
      <string style='split,upper'/>
    </convert>
    <convertquery>
      <string style='upper' />
    </convertquery>
    <caption lang='zh-CN'>状态</caption>
    <caption lang='en'>State</caption>
  </table>

  <key>
    <xpath>*/operations/operation[@name='create']/@time</xpath>
    <from>operTime</from>
    <table ref='createTime' />
  </key>
  <table name='createTime' id='11' type='createTime'>
    <convert>
      <number style='rfc1123time' />
    </convert>
    <convertquery>
      <number style='utime' />
    </convertquery>
    <caption lang='zh-CN'>创建时间</caption>
    <caption lang='en'>CreateTime</caption>
  </table>

  <key>
    <xpath>*/shelfNo</xpath>
    <from>shelfno</from>
    <table ref='shelfno' />
  </key>
  <table name='shelfno' id='12'>
    <caption lang='zh-CN'>架号</caption>
    <caption lang='en'>Shelf No</caption>
  </table>

  <key>
    <xpath>*/uid</xpath>
    <from>uid</from>
    <table ref='uid' />
  </key>
  <table name='uid' id='14'>
    <caption lang='zh-CN'>RFID UID</caption>
    <caption lang='en'>RFID UID</caption>
  </table>

  <!-- Current Location 当前位置 2020/12/20 -->
  <key>
    <xpath>*/currentLocation</xpath>
    <from>curLoc</from>
    <table ref='curLoc' />
  </key>
  <table name='curLoc' id='15' type='currentLocation'>
    <caption lang='zh-CN'>当前位置</caption>
    <caption lang='en'>Current Location</caption>
  </table>

  <!-- ******************配置非用字**************************************** -->
  <stopword>
    <stopwordTable name='title'>
      <separator>
        <t>,</t>
        <t>_</t>
        <t>.</t>
        <t>:</t>
        <t>;</t>
        <t>!</t>
        <t>'</t>
        <t>'</t>
        <t>-</t>
        <t>，</t>
        <t>。</t>
        <t>‘</t>
        <t>’</t>
        <t>“</t>
        <t>”</t>
        <t>—</t>
      </separator>
      <word>
        <t>the</t>
        <t>a</t>
      </word>
    </stopwordTable>
  </stopword>
</root>";
                string strBrowseDef = @"
<root>
  <col title='父记录ID'>
    <xpath>//parent</xpath>
  </col>
  <col title='馆藏地点'>
    <xpath>//location</xpath>
  </col>
  <col title='册条码号'>
    <xpath>//barcode</xpath>
  </col>
</root>";

                // 试探性删除以前残留的数据库
                var ret = channel.DoDeleteDB(database_name, out strError);
                if (ret == -1 && channel.ErrorCode != DigitalPlatform.rms.Client.ChannelErrorCode.NotFound)
                {
                    goto ERROR1;
                }

                ret = channel.DoCreateDB(logicNames,
                    "", // type
                    "", // strSqlDbName,
                    strKeysDef,
                    strBrowseDef,
                    out strError);
                if (ret == -1)
                {
                    strError = $"创建数据库时出错: {strError}";
                    goto ERROR1;
                }

                ret = channel.DoInitialDB(database_name, out strError);
                if (ret == -1)
                {
                    strError = $"初始化数据库时出错: {strError}";
                    goto ERROR1;
                }
            }

            DataModel.SetMessage("创建数据库成功", "green");
            return new NormalResult();
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

            var channel = DataModel.GetChannel();

            foreach (var database_name in database_names)
            {
                long ret = channel.DoDeleteDB(database_name, out strError);
                if (ret == -1)
                {
                    strError = $"删除数据库时出错: {strError}";
                    goto ERROR1;
                }
            }

            DataModel.SetMessage("删除数据库成功", "green");
            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult UnicodeSearch(CancellationToken token)
        {
            var channel = DataModel.GetChannel();

            token.ThrowIfCancellationRequested();

            {
                string path = $"{_strDatabaseName}/?";
                string current_barcode = "0000001";
                // 김:contributor
                string current_location = "김";
                string xml = @"<root xmlns:dprms='http://dp2003.com/dprms'>
<barcode>{barcode}</barcode>
<location>{location}</location>
<dprms:file id='1' />
<dprms:file id='2' />
<dprms:file id='3' />
<dprms:file id='4' />
<dprms:file id='5' />
<dprms:file id='6' />
<dprms:file id='7' />
<dprms:file id='8' />
<dprms:file id='9' />
<dprms:file id='10' />
</root>".Replace("{barcode}", current_barcode).Replace("{location}", current_location);

                token.ThrowIfCancellationRequested();

                var ret = channel.DoSaveTextRes(path,
                    xml, // strMetadata,
                    false,
                    "",
                    null,
                    out byte[] output_timestamp,
                    out string output_path,
                    out string strError);
                if (ret == -1)
                    return new CreateResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                    };

                token.ThrowIfCancellationRequested();

                // 从 dp2kernel 获得检索点
                var get_keys = GetKeys(channel,
    output_path,
    xml);
                if (get_keys.Count != 2)
                    return new CreateResult
                    {
                        Value = -1,
                        ErrorInfo = $"GetKeys({output_path}) 获得的检索点不是 2 个",
                    };

                List<string> querys = new List<string>();

                foreach (var key in get_keys)
                {
                    token.ThrowIfCancellationRequested();

                    if (key.Path != output_path)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"key.Path({key.Path}) != output_path({output_path})",
                        };

                    // 检查检索点
                    if (key.From == "册条码号")
                    {
                        if (key.Key != current_barcode)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"key.Key({key.Key}) != current_barcode({current_barcode})",
                            };
                    }

                    if (key.From == "馆藏地点")
                    {
                        if (key.Key != current_location)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"key.Key({key.Key}) != current_location({current_location})",
                            };
                    }

                    // 检查检索点是否被成功创建
                    {
                        var accesspoint = new AccessPoint
                        {
                            Key = key.Key,
                            From = key.From,
                        };
                        string strQueryXml = $"<target list='{ _strDatabaseName}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                        querys.Add(strQueryXml);

                        token.ThrowIfCancellationRequested();

                        ret = channel.DoSearch(strQueryXml, "default", out strError);
                        if (ret == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };
                        if (ret != 1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"检索 '{accesspoint.Key}' 应当命中 1 条。但命中了 {ret} 条",
                            };

                        List<string> path_list = new List<string>();
                        {
                            path_list.Add(output_path);
                        }
                        var verify_result = VerifyHitRecord(
                            channel, "default", path_list, token);
                        if (verify_result.Value == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = verify_result.ErrorInfo
                            };
                    }
                }

                token.ThrowIfCancellationRequested();

                // 删除记录
                ret = channel.DoDeleteRes(output_path,
    output_timestamp,
    "", // "ignorechecktimestamp",
    out byte[] output_timestamp2,
    out strError);
                if (ret == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"DoDeleteRes() error: {strError}"
                    };

                // 删除记录后，再检查检索点是否被删除干净了
                foreach (string strQueryXml in querys)
                {
                    token.ThrowIfCancellationRequested();

                    ret = channel.DoSearch(strQueryXml, "default", out strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    if (ret != 0)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"检索 '{strQueryXml}' 应当命中 0 条。但命中了 {ret} 条",
                        };
                }

                DataModel.SetMessage($"Unicode 检索 {output_path} 成功");
            }

            return new NormalResult();
        }

        // 创建若干条数据库记录
        public static CreateResult CreateRecords(int count, CancellationToken token)
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> result_accesspoints = new List<AccessPoint>();

            foreach (var database_name in database_names)
            {
                token.ThrowIfCancellationRequested();

                List<AccessPoint> created_accesspoints = new List<AccessPoint>();

                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    string path = $"{database_name}/?";
                    string current_barcode = (i + 1).ToString().PadLeft(10, '0');
                    string current_location = "海淀分馆/阅览室";
                    string xml = @"<root xmlns:dprms='http://dp2003.com/dprms'>
<barcode>{barcode}</barcode>
<location>{location}</location>
<dprms:file id='1' />
<dprms:file id='2' />
<dprms:file id='3' />
<dprms:file id='4' />
<dprms:file id='5' />
<dprms:file id='6' />
<dprms:file id='7' />
<dprms:file id='8' />
<dprms:file id='9' />
<dprms:file id='10' />
</root>".Replace("{barcode}", current_barcode).Replace("{location}", current_location);
                    // var bytes = Encoding.UTF8.GetBytes(xml);

                    var ret = channel.DoSaveTextRes(path,
                        xml, // strMetadata,
                        false,
                        "",
                        null,
                        out byte[] output_timestamp,
                        out string output_path,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            CreatedPaths = created_paths,
                            AccessPoints = result_accesspoints,
                        };

                    created_paths.Add(output_path);

                    // 从 dp2kernel 获得检索点
                    var get_keys = GetKeys(channel,
        output_path,
        xml);
                    if (get_keys.Count != 2)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"GetKeys({output_path}) 获得的检索点不是 2 个",
                        };
                    foreach (var key in get_keys)
                    {
                        if (key.Path != output_path)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"key.Path({key.Path}) != output_path({output_path})",
                            };
                        created_accesspoints.Add(key);

                        // 检查检索点
                        if (key.From == "册条码号")
                        {
                            if (key.Key != current_barcode)
                                return new CreateResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"key.Key({key.Key}) != current_barcode({current_barcode})",
                                };
                        }

                        if (key.From == "馆藏地点")
                        {
                            if (key.Key != current_location)
                                return new CreateResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"key.Key({key.Key}) != current_location({current_location})",
                                };
                        }

                        /*
                        created_accesspoints.Add(new AccessPoint
                        {
                            Key = current_barcode,
                            From = "册条码号",
                            Path = output_path,
                        });
                        */
                    }

                    var groups = created_accesspoints.AsQueryable().GroupBy(p => new { p.Key, p.From }).ToList();

                    // 检查检索点是否被成功创建
                    foreach (var group in groups)
                    {
                        token.ThrowIfCancellationRequested();

                        var accesspoint = new AccessPoint
                        {
                            Key = group.Key.Key,
                            From = group.Key.From,
                        };
                        var c = group.Count();
                        string strQueryXml = $"<target list='{ database_name}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                        ret = channel.DoSearch(strQueryXml, "default", out strError);
                        if (ret == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"DoSearch() 出错: {strError}"
                            };
                        if (ret != c)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"检索 '{accesspoint.Key}' 应当命中 {c} 条。但命中了 {ret} 条",
                            };

                        List<string> path_list = new List<string>();
                        foreach (var r in group)
                        {
                            path_list.Add(r.Path);
                        }
                        var verify_result = VerifyHitRecord(
                            channel, "default", path_list, token);
                        if (verify_result.Value == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = verify_result.ErrorInfo
                            };
                    }

                    DataModel.SetMessage($"创建记录 {output_path} 成功");
                }

                result_accesspoints.AddRange(created_accesspoints);
            }
            return new CreateResult
            {
                CreatedPaths = created_paths,
                AccessPoints = result_accesspoints,
            };
        }

        static List<AccessPoint> GetKeys(RmsChannel channel,
            string path,
            string xml)
        {
            var ret = channel.DoGetKeys(path,
    xml,
    "zh",
    null,
    out List<AccessKeyInfo> keys,
    out string strError);
            if (ret == -1)
                throw new Exception(strError);

            List<AccessPoint> results = new List<AccessPoint>();

            foreach (var key in keys)
            {
                if (Convert.ToInt64(key.ID) != Convert.ToInt64(ResPath.GetRecordId(path)))
                    throw new Exception($"GetKeys() 返回的 ID '{key.ID}' 和 path({path}) 里面的 ID 不同");

                results.Add(new AccessPoint
                {
                    Key = key.KeyNoProcess,
                    From = key.FromName,
                    Path = path
                });
            }

            return results;
        }

        // parameters:
        //      delete_style 如果为 "forcedeleteoldkeys" 表示希望强制删除记录的检索点
        public static NormalResult DeleteRecords(IEnumerable<string> paths,
            IEnumerable<AccessPoint> created_accesspoints,
            string delete_style)
        {
            var channel = DataModel.GetChannel();

            foreach (var path in paths)
            {
                // string path = $"{strDatabaseName}/{i+1}";

                var ret = channel.DoDeleteRes(path,
                    null,
                    "ignorechecktimestamp," + delete_style,
                    out byte[] output_timestamp,
                    out string strError);
                if (ret == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"DoDeleteRes() error: {strError}"
                    };

                // 检查对象是否被删除
                for (int j = 0; j < 9; j++)
                {
                    ret = channel.GetRes($"{path}/object/{j + 1}",
                        0,
                        1,
                        "content,data",
                        out byte[] content,
                        out string metadata,
                        out string output_object_path,
                        out byte[] output_object_timestamp,
                        out strError);
                    if (ret == -1 && channel.ErrorCode == DigitalPlatform.rms.Client.ChannelErrorCode.NotFound)
                    {

                    }
                    else
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "检查删除的对象记录时出错: " + strError
                        };
                }

                DataModel.SetMessage($"删除记录 {path} 成功");
            }

            return new NormalResult();
        }

        public class CreateResult : NormalResult
        {
            public List<string> CreatedPaths { get; set; }
            public List<AccessPoint> AccessPoints { get; set; }
        }

        public class AccessPoint
        {
            public string Key { get; set; }

            public string From { get; set; }

            public string Path { get; set; }
        }

        // 针对单一数据库的 __id 检索
        public static NormalResult TestSingleDbIdSearch(CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";

            foreach (var database_name in database_names)
            {
                token.ThrowIfCancellationRequested();

                // 非逻辑检索
                {
                    string query = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] { database_name + "/1" },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 单一检索验证成功");
                }

                token.ThrowIfCancellationRequested();

                // AND
                {
                    string query1 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:__id'><item><word>1-1000</word><match>exact</match><relation>range</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='AND'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] { database_name + "/1" },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 逻辑检索 AND 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // OR
                {
                    string query1 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:__id'><item><word>2</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='OR'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name + "/1",
            database_name + "/2"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 逻辑检索 OR 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(1)
                {
                    string query1 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:__id'><item><word>2</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name + "/1"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 逻辑检索 SUB(1) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(2)
                {
                    string query1 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 逻辑检索 SUB(2) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(3)
                {
                    string query1 = $"<target list='{ database_name}:__id'><item><word>1-2</word><match>exact</match><relation>range</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:__id'><item><word>1</word><match>exact</match><relation>=</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
                        database_name + "/2"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"__id 逻辑检索 SUB(3) 验证成功");
                }
            }

            return new NormalResult();
        }


        // 针对单一数据库的逻辑检索
        public static NormalResult TestSingleDbLogicSearch(CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";

            foreach (var database_name in database_names)
            {
                token.ThrowIfCancellationRequested();

                // AND
                {
                    string query1 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:馆藏地点'><item><word>海淀分馆/阅览室</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='AND'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] { database_name + "/1" },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"逻辑检索 AND 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // OR
                {
                    string query1 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:册条码号'><item><word>0000000002</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='OR'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name + "/1",
            database_name + "/2"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"逻辑检索 OR 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(1)
                {
                    string query1 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:册条码号'><item><word>0000000002</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name + "/1"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"逻辑检索 SUB(1) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(2)
                {
                    string query1 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"逻辑检索 SUB(2) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(3)
                {
                    string query1 = $"<target list='{ database_name}:馆藏地点'><item><word>海淀分馆/阅览室</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
                        database_name + "/2"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"逻辑检索 SUB(3) 验证成功");
                }
            }

            return new NormalResult();
        }

        // 针对多个数据库的逻辑检索
        public static NormalResult TestMultiDbLogicSearch(CancellationToken token)
        {
            var channel = DataModel.GetChannel();
            string resultset_name = "default";

            var database_name1 = _strDatabaseName1;
            var database_name2 = _strDatabaseName2;

            {
                token.ThrowIfCancellationRequested();

                // AND
                {
                    string query1 = $"<target list='{ database_name1}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:馆藏地点'><item><word>海淀分馆/阅览室</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='AND'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 AND 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // OR
                {
                    string query1 = $"<target list='{ database_name1}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:册条码号'><item><word>0000000002</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='OR'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name1 + "/1",
            database_name2 + "/2"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 OR 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(1)
                {
                    string query1 = $"<target list='{ database_name1}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:册条码号'><item><word>0000000002</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
            database_name1 + "/1"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 SUB(1) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(2)
                {
                    string query1 = $"<target list='{ database_name1}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
                            database_name1 + "/1"
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 SUB(2) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // SUB(3)
                {
                    string query1 = $"<target list='{ database_name1}:馆藏地点'><item><word>海淀分馆/阅览室</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='SUB'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query, resultset_name, out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitRecord(channel,
            resultset_name,
            new string[] {
                        database_name1 + "/1",
                        database_name1 + "/2",
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 SUB(3) 验证成功");
                }

                token.ThrowIfCancellationRequested();

                // OR
                // keycount
                {
                    string query1 = $"<target list='{ database_name1}:册条码号'><item><word>0000000001</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query2 = $"<target list='{ database_name2}:册条码号'><item><word>0000000002</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
                    string query = "<group>" + query1 + "<operator value='OR'/>" + query2 + "</group>";
                    var ret = channel.DoSearch(query,
                        resultset_name,
                        "keycount",
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"DoSearch() 出错: {strError}"
                        };
                    var verify_result = VerifyHitKeys(channel,
            resultset_name,
            new KeyCount[] {
                new KeyCount{ Key="0000000001", Count = "1"},
                new KeyCount{ Key="0000000002", Count = "1"},
            },
            token);
                    if (verify_result.Value == -1)
                        return verify_result;

                    DataModel.SetMessage($"多库逻辑检索 OR 验证成功");
                }
            }

            return new NormalResult();
        }

        class KeyCount // : IEquatable<KeyCount>
        {
            public string Key { get; set; }
            public string Count { get; set; }
            public override string ToString()
            {
                return $"{Key}:{Count}";
            }

            public static string ToString(IEnumerable<KeyCount> items)
            {
                StringBuilder text = new StringBuilder();
                foreach (var item in items)
                {
                    if (text.Length > 0)
                        text.Append(",");
                    text.Append(item.ToString());
                }

                return text.ToString();
            }

            /*
            public bool Equals(KeyCount other)
            {
                return this.Key == other.Key && this.Count == other.Count;
            }
            */
        }

        class KeyCountComparer : IEqualityComparer<KeyCount>
        {
            public bool Equals(KeyCount a, KeyCount b)
            {
                return a.Key == b.Key && a.Count == b.Count;
            }

            public int GetHashCode(KeyCount a)
            {
                return 0;
            }
        }

        // 验证命中记录
        static NormalResult VerifyHitKeys(RmsChannel channel,
            string resultset_name,
            IEnumerable<KeyCount> keys_list,
            CancellationToken token)
        {
            SearchResultLoader loader = new SearchResultLoader(channel,
null,
resultset_name,
"keycount");
            loader.ElementType = "Record";

            List<KeyCount> results = new List<KeyCount>();
            foreach (Record record in loader)
            {
                token.ThrowIfCancellationRequested();

                results.Add(new KeyCount
                {
                    Key = record.Path,
                    Count = record.Cols[0]
                });
            }

            var intersect_count = results.AsQueryable().Intersect<KeyCount>(keys_list, new KeyCountComparer()).Count();
            if (intersect_count != results.Count
                || intersect_count != keys_list.Count())
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"实际命中的集合 '{KeyCount.ToString(results)}' 和期望的集合 '{KeyCount.ToString(keys_list)}' 不吻合"
                };
            }

            return new NormalResult();
        }

        // 验证命中记录
        static NormalResult VerifyHitRecord(RmsChannel channel,
            string resultset_name,
            IEnumerable<string> path_list,
            CancellationToken token)
        {
            SearchResultLoader loader = new SearchResultLoader(channel,
null,
resultset_name,
"id");
            loader.ElementType = "Record";

            List<string> results = new List<string>();
            foreach (Record record in loader)
            {
                token.ThrowIfCancellationRequested();

                results.Add(record.Path);
            }

            var intersect_count = results.AsQueryable().Intersect(path_list).Count();
            if (intersect_count != results.Count
                || intersect_count != path_list.Count())
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"实际命中的集合 '{StringUtil.MakePathList(results)}' 和期望的集合 '{string.Join(",", path_list)}' 不吻合"
                };
            }

            return new NormalResult();
        }

        // 测试各种匹配方式
        public static NormalResult TestMatch()
        {
            return new NormalResult();
        }

        // 测试各种检索途径
        public static NormalResult TestFrom()
        {
            return new NormalResult();
        }

        public static NormalResult TestMultipleDatabaseSearch()
        {
            return new NormalResult();
        }
    }
}
