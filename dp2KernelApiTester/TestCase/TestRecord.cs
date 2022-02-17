using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Xml;

namespace dp2KernelApiTester
{
    // 测试和记录有关的功能。
    // (创建记录，删除记录，元数据记录包含对象的情况)
    public static class TestRecord
    {
        static string strDatabaseName = "__test";

        public static NormalResult TestAll(string style = null)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = BatchCreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "forcedeleteoldkeys");
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = RebuildRecordKeys(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;

                result = DeleteRecords(create_result.CreatedPaths,
    create_result.AccessPoints,
    "");
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

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            List<string[]> logicNames = new List<string[]>();
            {
                logicNames.Add(new string[]
                {
                    strDatabaseName,
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
            var ret = channel.DoDeleteDB(strDatabaseName, out strError);
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

            ret = channel.DoInitialDB(strDatabaseName, out strError);
            if (ret == -1)
            {
                strError = $"初始化数据库时出错: {strError}";
                goto ERROR1;
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

            long ret = channel.DoDeleteDB(strDatabaseName, out strError);
            if (ret == -1)
            {
                strError = $"删除数据库时出错: {strError}";
                goto ERROR1;
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

        public class AccessPoint
        {
            public string Key { get; set; }

            public string From { get; set; }

            public string Path { get; set; }
        }

        public class CreateResult : NormalResult
        {
            public List<string> CreatedPaths { get; set; }
            public List<AccessPoint> AccessPoints { get; set; }
        }

        // 创建若干条数据库记录
        public static CreateResult CreateRecords(int count)
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> created_accesspoints = new List<AccessPoint>();

            for (int i = 0; i < count; i++)
            {
                string path = $"{strDatabaseName}/?";
                string current_barcode = (i + 1).ToString().PadLeft(10, '0');
                string xml = @"<root xmlns:dprms='http://dp2003.com/dprms'>
<barcode>{barcode}</barcode>
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
</root>".Replace("{barcode}", current_barcode);
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
                        AccessPoints = created_accesspoints,
                    };

                created_paths.Add(output_path);
                created_accesspoints.Add(new AccessPoint
                {
                    Key = current_barcode,
                    From = "册条码号",
                    Path = output_path,
                });

                // 上载对象
                for (int j = 0; j < 10; j++)
                {
                    byte[] bytes = new byte[4096];
                    ret = channel.WriteRes($"{output_path}/object/{j + 1}",
                        $"0-{bytes.Length - 1}",
                        bytes.Length,
                        bytes,
                        "",
                        "content,data",
                        null,
                        out string output_object_path,
                        out byte[] output_object_timestamp,
                        out strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            CreatedPaths = created_paths,
                            AccessPoints = created_accesspoints,
                        };
                }

                // 检查检索点是否被成功创建
                foreach (var accesspoint in created_accesspoints)
                {
                    string strQueryXml = $"<target list='{ strDatabaseName}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

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
                }

                DataModel.SetMessage($"创建记录 {output_path} 成功");
            }

            return new CreateResult
            {
                CreatedPaths = created_paths,
                AccessPoints = created_accesspoints,
            };
        }

        // 成批创建记录
        public static CreateResult BatchCreateRecords(int count)
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> created_accesspoints = new List<AccessPoint>();


            List<RecordBody> inputs = new List<RecordBody>();
            for (int i = 0; i < count; i++)
            {
                string path = $"{strDatabaseName}/?";
                string current_barcode = (i + 1).ToString().PadLeft(10, '0');
                string xml = @"<root xmlns:dprms='http://dp2003.com/dprms'>
<barcode>{barcode}</barcode>
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
</root>".Replace("{barcode}", current_barcode);
                // var bytes = Encoding.UTF8.GetBytes(xml);

                inputs.Add(new RecordBody
                {
                    Path = path,
                    Xml = xml,
                    Timestamp = null,
                });
            }

            var ret = channel.DoWriteRecords(null,
inputs.ToArray(), // strMetadata,
"",
out RecordBody[] outputs,
out string strError);
            if (ret == -1)
                return new CreateResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    CreatedPaths = created_paths,
                    AccessPoints = created_accesspoints,
                };

            foreach (var output in outputs)
            {
                created_paths.Add(output.Path);
                string output_xml = output.Xml;
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(output_xml);
                string barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                created_accesspoints.Add(new AccessPoint
                {
                    Key = barcode,
                    From = "册条码号",
                    Path = output.Path,
                });

                DataModel.SetMessage($"创建记录 {output.Path} 成功");

                // 上载对象
                for (int j = 0; j < 10; j++)
                {
                    byte[] bytes = new byte[4096];
                    ret = channel.WriteRes($"{output.Path}/object/{j + 1}",
                        $"0-{bytes.Length - 1}",
                        bytes.Length,
                        bytes,
                        "",
                        "content,data",
                        null,
                        out string output_object_path,
                        out byte[] output_object_timestamp,
                        out strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            CreatedPaths = created_paths,
                            AccessPoints = created_accesspoints,
                        };
                }
            }

            // 检查检索点是否被成功创建
            foreach (var accesspoint in created_accesspoints)
            {
                string strQueryXml = $"<target list='{ strDatabaseName}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

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
            }

            return new CreateResult
            {
                CreatedPaths = created_paths,
                AccessPoints = created_accesspoints,
            };
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
                        ErrorInfo = strError
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

            // 检查检索点是否被成功删除
            foreach (var accesspoint in created_accesspoints)
            {
                var result = VerifyAccessPoint(
                    channel,
                    accesspoint,
                    "nothit");
                if (result.Value == -1)
                    return result;
            }

            return new NormalResult();
        }


        public static NormalResult RebuildRecordKeys(IEnumerable<string> paths,
    IEnumerable<AccessPoint> created_accesspoints,
    string delete_style)
        {
            var channel = DataModel.GetChannel();

            // 刷新前，检查检索点是否存在
            foreach (var accesspoint in created_accesspoints)
            {
                var result = VerifyAccessPoint(
                    channel,
                    accesspoint,
                    "hit");
                if (result.Value == -1)
                    return result;
            }


            foreach (var path in paths)
            {
                // string path = $"{strDatabaseName}/{i+1}";

                var ret = channel.DoRebuildResKeys(path,
                    "", // "forcedeleteoldkeys",
                    out string output_path,
                    out string strError);
                if (ret == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                DataModel.SetMessage($"刷新记录 {path} 检索点成功");
            }

            // 检查检索点是否被成功刷新
            foreach (var accesspoint in created_accesspoints)
            {
                var result = VerifyAccessPoint(
                    channel,
                    accesspoint,
                    "hit");
                if (result.Value == -1)
                    return result;
            }

            return new NormalResult();
        }

        static NormalResult VerifyAccessPoint(
            RmsChannel channel,
            AccessPoint accesspoint,
            string condition)
        {
            string strQueryXml = $"<target list='{ strDatabaseName}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

            var ret = channel.DoSearch(strQueryXml, "default", out string strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DoSearch() 出错: {strError}"
                };
            if (condition == "hit")
            {
                if (ret != 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"检索 '{accesspoint.Key}' 应当命中 1 条。但命中了 {ret} 条",
                    };

                SearchResultLoader loader = new SearchResultLoader(channel,
    null,
    "default",
    "id");
                loader.ElementType = "Record";

                foreach (Record record in loader)
                {
                    if (record.Path != accesspoint.Path)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"应命中记录 {accesspoint.Key}，但命中了记录 {record.Path}",
                        };
                }
            }
            else if (condition == "nothit")
            {
                if (ret != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"检索 '{accesspoint.Key}' 应当不命中。但命中了 {ret} 条",
                    };
            }
            else
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"未知的 condition:'{condition}'"
                };
            }

            return new NormalResult();
        }
    }
}
