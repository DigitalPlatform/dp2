using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2KernelApiTester
{
    // 测试和记录有关的功能。
    // (创建记录，删除记录，元数据记录包含对象的情况)
    public static class TestRecord
    {
        static string strDatabaseName = "__test";

        public static NormalResult SpecialTest(int count)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            /*
            {
                var create_result = UploadLargObject(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
    create_result.AccessPoints,
    "");
                if (result.Value == -1)
                    return result;
            }
            */

            {
                var create_result = QuickCreateRecords(100);
                if (create_result.Value == -1)
                    return create_result;

                var result = BatchRefreshRecords(create_result.CreatedPaths,
                    create_result.AccessPoints);
                if (result.Value == -1)
                    return result;

                result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(count, false, false);
                if (create_result.Value == -1)
                    return create_result;

                for (int i = 1; i < 10; i++)
                {
                    var result = FragmentReadRecords(create_result.CreatedPaths, i);
                    if (result.Value == -1)
                        return result;
                }

                for (int i = 0; i < 10; i++)
                {
                    var result = FragmentOverwriteRecords(create_result.CreatedPaths, -1);
                    if (result.Value == -1)
                        return result;
                }

                var result1 = DeleteRecords(create_result.CreatedPaths,
    null,
    "");
                if (result1.Value == -1)
                    return result1;
            }

            /*
            {
                var result = Finish();
                if (result.Value == -1)
                    return result;
            }
            */

            return new NormalResult();
        }

        public static NormalResult LargeObjectTest(int count)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = UploadLargObject(count);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
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

        public static NormalResult TestAll(string style = null)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(2, true);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
                if (result.Value == -1)
                    return result;
            }

            // 碎片方式创建记录
            {
                var create_result = FragmentCreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = FragmentOverwriteRecords(create_result.CreatedPaths);
                if (result.Value == -1)
                    return result;

                var result1 = DeleteRecords(create_result.CreatedPaths,
    null,
    "");
                if (result1.Value == -1)
                    return result1;
            }

            // 碎片方式创建记录，overlap 风格
            {
                var create_result = FragmentCreateRecords(1,1,"overlap");
                if (create_result.Value == -1)
                    return create_result;

                var result = FragmentOverwriteRecords(create_result.CreatedPaths, 1, "overlap");
                if (result.Value == -1)
                    return result;

                var result1 = DeleteRecords(create_result.CreatedPaths,
    null,
    "");
                if (result1.Value == -1)
                    return result1;
            }

            {
                var create_result = FragmentCreateRecords(1);
                if (create_result.Value == -1)
                    return create_result;

                for (int i = 1; i < 10; i++)
                {
                    var result = FragmentReadRecords(create_result.CreatedPaths, i);
                    if (result.Value == -1)
                        return result;
                }

                var result1 = DeleteRecords(create_result.CreatedPaths,
    null,
    "");
                if (result1.Value == -1)
                    return result1;
            }

            {
                var create_result = QuickCreateRecords(100);
                if (create_result.Value == -1)
                    return create_result;

                var result = BatchRefreshRecords(create_result.CreatedPaths,
                    create_result.AccessPoints);
                if (result.Value == -1)
                    return result;

                result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "");
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

            //
            {
                var create_result = CreateRecords(1, true);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
                    create_result.AccessPoints,
                    "forcedeleteoldkeys");
                if (result.Value == -1)
                    return result;
            }

            {
                var create_result = CreateRecords(1, true);
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

            // 上传下载大文件
            {
                var create_result = UploadLargObject(1);
                if (create_result.Value == -1)
                    return create_result;

                var result = DeleteRecords(create_result.CreatedPaths,
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
        public static CreateResult CreateRecords(int count,
            bool upload_object = false,
            bool verify_accesspoint = true)
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

                if (upload_object)
                {
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
                }

                if (verify_accesspoint)
                {
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

        // 用 CreateRecords() 刷新检索点
        public static NormalResult BatchRefreshRecords(IEnumerable<string> paths,
            IEnumerable<AccessPoint> created_accesspoints)
        {
            var channel = DataModel.GetChannel();

            List<RecordBody> inputs = new List<RecordBody>();

            foreach (var path in paths)
            {
                inputs.Add(new RecordBody
                {
                    Path = path,
                    // Xml = xml,
                    Timestamp = null,
                });
            }

            var ret = channel.DoWriteRecords(null,
inputs.ToArray(),
"rebuildkeys,deletekeys",
out RecordBody[] outputs,
out string strError);
            if (ret == -1)
                return new CreateResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                };

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

            List<string> refresh_paths = new List<string>();
            foreach (var output in outputs)
            {
                refresh_paths.Add(output.Path);
            }
            DataModel.SetMessage($"刷新(删除后重建)记录检索点 {StringUtil.MakePathList(refresh_paths, ", ")} 成功");

            return new NormalResult();
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

            if (created_accesspoints != null)
            {
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

        // 用 BulkCopy 方式灌入大量记录
        public static CreateResult QuickCreateRecords(int count)
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> created_accesspoints = new List<AccessPoint>();

            List<string> target_dburls = new List<string>();

            List<RecordBody> inputs = new List<RecordBody>();
            for (int i = 0; i < count; i++)
            {
                string path = $"{strDatabaseName}/?";
                string strDbUrl = DataModel.dp2kernelServerUrl + "?" + strDatabaseName;
                // 记载每个数据库的 URL
                if (target_dburls.IndexOf(strDbUrl) == -1)
                {
                    // 每个数据库要进行一次快速模式的准备操作
                    int nRet = ManageKeysIndex(
                        channel,
                        strDbUrl,
                            "beginfastappend",
                            "正在对数据库 " + strDbUrl + " 进行快速导入模式的准备工作 ...",
                            out string error);
                    if (nRet == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    target_dburls.Add(strDbUrl);
                }

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

            DataModel.SetMessage($"正在一次性创建 {inputs.Count} 条记录");

            var ret = channel.DoWriteRecords(null,
    inputs.ToArray(), // strMetadata,
    "fastmode",
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

                // DataModel.SetMessage($"创建记录 {output.Path} 成功");

                /*
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
            */
            }

            DataModel.SetMessage($"创建记录 {StringUtil.MakePathList(created_paths, ", ")} 成功");


            EndFastAppend(channel, target_dburls);

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

        /*
        static string GetDbUrl(string strLongPath)
        {
            ResPath respath = new ResPath(strLongPath);
            respath.MakeDbName();
            return respath.FullPath;
        }
        */

        static void EndFastAppend(RmsChannel channel,
        List<string> target_dburls)
        {
            int nRet = 0;
            foreach (string url in target_dburls)
            {
                DataModel.SetMessage("正在对数据库 " + url + " 进行快速导入模式的最后收尾工作，请耐心等待 ...");
                try
                {
                    nRet = ManageKeysIndex(
                        channel,
                        url,
                        "start_endfastappend",
                        "正在对数据库 " + url + " 启动快速导入模式的收尾工作，请耐心等待 ...",
        out string strQuickModeError);
                    //if (nRet == -1)
                    //    MessageBoxShow(strQuickModeError);
                    if (nRet == -1)
                        throw new Exception(strQuickModeError);
                    if (nRet == 1)
                    {
                        while (true)
                        {
                            //                  detect_endfastappend 探寻任务的状态。返回 0 表示任务尚未结束; 1 表示任务已经结束
                            nRet = ManageKeysIndex(
                                channel,
        url,
        "detect_endfastappend",
        "正在对数据库 " + url + " 进行快速导入模式的收尾工作，请耐心等待 ...",
        out strQuickModeError);
                            if (nRet == -1)
                                throw new Exception(strQuickModeError);
                            if (nRet == 1)
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //LibraryChannelManager.Log?.Debug($"对数据库{url}进行快速导入模式的最后收尾工作阶段出现异常: {ExceptionUtil.GetExceptionText(ex)}\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}");
                    // throw new Exception($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}", ex);
                    throw new Exception($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}。\r\n异常信息{ExceptionUtil.GetExceptionText(ex)}", ex);
                }
            }
        }

        static int ManageKeysIndex(
            RmsChannel channel,
        string strDbUrl,
        string strAction,
        string strMessage,
        out string strError)
        {
            strError = "";

            Debug.Assert(strDbUrl != null);
            ResPath respath = new ResPath(strDbUrl);

            TimeSpan old_timeout = channel.Timeout;
            if (strAction == "endfastappend")
            {
                // 收尾阶段可能要耗费很长的时间
                channel.Timeout = new TimeSpan(3, 0, 0);
            }

            try
            {
                // TODO: 改造为新的查询任务是否完成的用法
                long lRet = channel.DoRefreshDB(
                    strAction,
                    respath.Path,
                    false,
                    out strError);
                if (lRet == -1)
                {
                    strError = "管理数据库 '" + respath.Path + "' 时出错: " + strError;
                    goto ERROR1;
                }

                // 2019/5/13
                return (int)lRet;
            }
            finally
            {
                if (strAction == "endfastappend")
                {
                    channel.Timeout = old_timeout;
                }
            }
        ERROR1:
            return -1;
        }

        // 用片段方式创建记录
        public static CreateResult FragmentCreateRecords(int count,
            int fragment_length = 1,
            string style = "")
        {
            var channel = DataModel.GetChannel();

            if (fragment_length < 1)
                throw new ArgumentException("fragment_length 必须大于等于 1");

            var overlap = StringUtil.IsInList("overlap", style);

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

                {
                    // 增大记录尺寸
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);

                    StringBuilder text = new StringBuilder();
                    for (int k = 0; k < 1024; k++)
                    {
                        text.Append(k.ToString());
                    }
                    DomUtil.SetElementText(dom.DocumentElement, "comment", text.ToString());

                    xml = dom.DocumentElement.OuterXml;
                }

                byte[] bytes = Encoding.UTF8.GetBytes(xml);

                string current_path = path;
                byte[] timestamp = null;
                long start = 0;
                long end = 0;

                string progress_id = DataModel.NewProgressID();
                DataModel.ShowProgressMessage(progress_id, $"正在用 Fragment 方式{style}创建记录 {i}，请耐心等待 ...");

                while (true)
                {
                    int chunk_length = fragment_length;

                    end = start + chunk_length - 1;

                    if (end > bytes.Length - 1)
                    {
                        end = chunk_length - 1;
                        chunk_length = (int)(end - start + 1);
                    }

                    Debug.Assert(end >= start);

                    long delta = 0;  // 调整长度
                    if (overlap)
                    {
                        delta = -10;
                        if (start + delta < 0)
                            delta = -1*start;
                    }

                    byte[] fragment = new byte[end - (start + delta) + 1];
                    Array.Copy(bytes, (start + delta), fragment, 0, fragment.Length);

                    DataModel.ShowProgressMessage(progress_id, $"正在用 Fragment 方式{style}创建记录 {current_path} {start+delta}-{end} {StringUtil.GetPercentText(end+1, bytes.Length)}...");

                    var ret = channel.WriteRes(current_path,
                        $"{start+delta}-{end}",
                        bytes.Length,
                        fragment,
                        "", // strMetadata
                        "", // strStyle,
                        timestamp,
                        out string output_path,
                        out byte[] output_timestamp,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"写入 {current_path} {start}-{end} 时出错: {strError}",
                            CreatedPaths = created_paths,
                            AccessPoints = created_accesspoints,
                        };

                    timestamp = output_timestamp;
                    current_path = output_path;

                    start += chunk_length;
                    if (start > bytes.Length - 1)
                        break;
                }

                DataModel.ShowProgressMessage(progress_id, $"用 Fragment 方式{style}创建记录 {current_path} 完成");

                // TODO: 读出记录检查内容是否和发出的一致
                {
                    var ret = channel.GetRes(current_path,
                        out string strResult,
                        out string _,
                        out byte[] _,
                        out string _,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            CreatedPaths = created_paths,
                            AccessPoints = created_accesspoints,
                        };

                    if (xml != strResult)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = $"读出记录 {current_path} 内容和创建时的不一致",
                            CreatedPaths = created_paths,
                            AccessPoints = created_accesspoints,
                        };
                }

                created_paths.Add(current_path);
                created_accesspoints.Add(new AccessPoint
                {
                    Key = current_barcode,
                    From = "册条码号",
                    Path = current_path,
                });

                // 上载对象
                for (int j = 0; j < 1; j++)
                {
                    int length = 1024 * 1024;
                    byte[] contents = new byte[length];
                    for (int k = 0; k < length; k++)
                    {
                        contents[k] = (byte)k;
                    }

                    string object_path = $"{current_path}/object/{j + 1}";

                    int chunk = 10 * 1024;
                    long start_offs = 0;
                    byte[] object_timestamp = null;
                    while (start_offs < length)
                    {
                        long end_offs = start_offs + chunk - 1;
                        if (end_offs >= length)
                            end_offs = length - 1;

                        long delta = 0;  // 调整长度
                        if (overlap)
                        {
                            delta = -10;
                            if (start_offs + delta < 0)
                                delta = -1 * start_offs;
                        }

                        byte[] chunk_contents = new byte[end_offs - (start_offs + delta)+ 1];
                        Array.Copy(contents, (start_offs + delta), chunk_contents, 0, chunk_contents.Length);
                        var ret = channel.WriteRes(object_path,
                            $"{start_offs+delta}-{end_offs}",
                            length,
                            chunk_contents,
                            "",
                            "content,data",
                            object_timestamp,
                            out string output_object_path,
                            out byte[] output_object_timestamp,
                            out string strError);
                        if (ret == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = strError,
                                CreatedPaths = created_paths,
                                AccessPoints = created_accesspoints,
                            };
                        object_timestamp = output_object_timestamp;

                        start_offs += chunk_contents.Length;
                    }

                    // 读出比较
                    using (var stream = new MemoryStream())
                    {
                        var ret = channel.GetRes(object_path,
                            stream,
                            null,
                            "content,data",
                            null,
                            out string _,
                            out byte[] download_timestamp,
                            out string _,
                            out string strError);
                        if (ret == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = strError,
                                CreatedPaths = created_paths,
                                AccessPoints = created_accesspoints,
                            };
                        byte[] download_bytes = new byte[length];
                        stream.Seek(0, SeekOrigin.Begin);
                        var read_len = stream.Read(download_bytes, 0, length);
                        if (read_len != length)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = "read file error"
                            };

                        if (ByteArray.Compare(contents, download_bytes) != 0)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = $"对象 {object_path} 下载后和原始数据比对发现不一致",
                                CreatedPaths = created_paths,
                                AccessPoints = created_accesspoints,
                            };
                    }
                }

                // 检查检索点是否被成功创建
                foreach (var accesspoint in created_accesspoints)
                {
                    string strQueryXml = $"<target list='{ strDatabaseName}:{accesspoint.From}'><item><word>{accesspoint.Key}</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                    var ret = channel.DoSearch(strQueryXml, "default", out string strError);
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

                DataModel.SetMessage($"Fragment 方式创建记录 {current_path} 成功");
            }

            return new CreateResult
            {
                CreatedPaths = created_paths,
                AccessPoints = created_accesspoints,
            };
        }

        // 用 Fragment 方式覆盖已经创建好的记录
        public static NormalResult FragmentOverwriteRecords(IEnumerable<string> paths,
    int fragment_length = 1,
    string style = "")
        {
            var channel = DataModel.GetChannel();

            int i = 1;
            foreach (var path in paths)
            {
                string origin_xml = "";
                byte[] origin_timestamp = null;
                // 先获得一次原始记录。然后 Fragment 覆盖，在覆盖完以前，每次中途再获取一次记录，应该是看到原始记录
                {
                    var ret = channel.GetRes(path,
                        out origin_xml,
                        out string _,
                        out origin_timestamp,
                        out string _,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                        };
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(origin_xml);
                var old_text = DomUtil.GetElementText(dom.DocumentElement, "changed");
                if (old_text == null)
                    old_text = "";
                DomUtil.SetElementText(dom.DocumentElement, "changed", old_text + CreateString(i++));

                if (i > 2000)
                    i = 0;

                string new_xml = dom.DocumentElement.OuterXml;
                byte[] bytes = Encoding.UTF8.GetBytes(new_xml);

                byte[] timestamp = origin_timestamp;
                long start = 0;
                long end = 0;

                var overlap = StringUtil.IsInList("overlap", style);

                string progress_id = DataModel.NewProgressID();
                DataModel.ShowProgressMessage(progress_id, $"正在用 Fragment 方式{style}覆盖记录 {path}，请耐心等待 ...");

                while (true)
                {
                    int chunk_length = fragment_length;

                    if (chunk_length == -1)
                        chunk_length = bytes.Length;

                    end = start + chunk_length - 1;

                    if (end > bytes.Length - 1)
                    {
                        end = bytes.Length - 1;
                        chunk_length = (int)(end - start + 1);
                    }

                    Debug.Assert(end >= start);

                    long delta = 0;  // 调整长度
                    if (overlap)
                    {
                        delta = -10;
                        if (start + delta < 0)
                            delta = -1 * start;
                    }

                    byte[] fragment = new byte[end - (start + delta) + 1];
                    Array.Copy(bytes, start+delta, fragment, 0, fragment.Length);

                    DataModel.ShowProgressMessage(progress_id, $"正在用 Fragment 方式{style}覆盖记录 {path} {start+delta}-{end} {StringUtil.GetPercentText(end+1, bytes.Length)}...");

                    var ret = channel.WriteRes(path,
                        $"{start+delta}-{end}",
                        bytes.Length,
                        fragment,
                        "", // strMetadata
                        "", // strStyle,
                        timestamp,
                        out string output_path,
                        out byte[] output_timestamp,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                        };

                    // 马上读取检验
                    if (end < bytes.Length - 1)
                    {
                        ret = channel.GetRes(path,
    out string read_xml,
    out string _,
    out byte[] read_timestamp,
    out string _,
    out strError);
                        if (ret == -1)
                            return new CreateResult
                            {
                                Value = -1,
                                ErrorInfo = strError,
                            };
                        if (read_xml != origin_xml)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"记录 {path} {start}-{end}轮次 读取出来和 origin_xml 不一致"
                            };
                        if (ByteArray.Compare(read_timestamp, origin_timestamp) != 0)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"记录 {path} 的时间戳({ByteArray.GetHexTimeStampString(read_timestamp)})读取出来和 origin_timestamp({ByteArray.GetHexTimeStampString(origin_timestamp)}) 不一致"
                            };
                    }

                    timestamp = output_timestamp;

                    start += chunk_length;
                    if (start > bytes.Length - 1)
                        break;
                }

                DataModel.ShowProgressMessage(progress_id, $"用 Fragment 方式{style}覆盖记录 {path} 完成");

                // 覆盖成功后，马上读取检验
                {
                    var ret = channel.GetRes(path,
out string read_xml,
out string _,
out byte[] read_timestamp,
out string _,
out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                        };
                    if (read_xml != new_xml)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"记录 {path} 读取出来和 new_xml 不一致"
                        };
                }

                DataModel.SetMessage($"覆盖记录 {path} 成功");
            }

            return new NormalResult();
        }

        static string CreateString(int length)
        {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char ch = (char)((int)'0' + (i % 10));
                text.Append(ch);
            }

            return text.ToString();
        }

        // 用 Fragment 方式读已经创建好的记录
        public static NormalResult FragmentReadRecords(IEnumerable<string> paths,
    int fragment_length = 1)
        {
            var channel = DataModel.GetChannel();

            int i = 1;
            foreach (var path in paths)
            {
                byte[] origin_bytes = null;
                byte[] origin_timestamp = null;
                string origin_metadata = "";
                // 先获得一次原始记录。然后 Fragment 读
                {
                    var ret = channel.GetRes(path,
                        0,
                        -1,
                        "content,data,metadata,timestamp,outputpath", // strStyle,
                        out origin_bytes,
                        out origin_metadata,
                        out string origin_path,
                        out origin_timestamp,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                        };
                }

                long start = 0;
                long end = 0;

                DataModel.SetMessage($"正在用 Fragment 方式读记录 {path}，请耐心等待 ...");

                while (true)
                {
                    int chunk_length = fragment_length;

                    if (chunk_length == -1)
                        chunk_length = origin_bytes.Length;

                    end = start + chunk_length - 1;

                    if (end > origin_bytes.Length - 1)
                    {
                        end = origin_bytes.Length - 1;
                        chunk_length = (int)(end - start + 1);
                    }

                    Debug.Assert(end >= start);

                    byte[] fragment = new byte[end - start + 1];
                    Array.Copy(origin_bytes, start, fragment, 0, fragment.Length);

                    var ret = channel.GetRes(path,
                        start,
                        fragment.Length,
                        "content,data,metadata,timestamp,outputpath", // strStyle,
                        out byte[] content,
                        out string output_metadata,
                        out string output_path,
                        out byte[] read_timestamp,
                        out string strError);
                    if (ret == -1)
                        return new CreateResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                        };

                    if (ByteArray.Compare(content, fragment) != 0)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"记录 {path} {start}-{end}轮次 读取出来和 origin_xml 不一致"
                        };

                    if (origin_metadata != output_metadata)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"记录 {path} {start}-{end}轮次 读取出来的 metadata 和 origin_metadata 不一致"
                        };

                    if (path != output_path)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"记录 {path} {start}-{end}轮次 读取出来的 output_path 和 origin_path 不一致"
                        };

                    if (ByteArray.Compare(read_timestamp, origin_timestamp) != 0)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"记录 {path} 的时间戳({ByteArray.GetHexTimeStampString(read_timestamp)})读取出来和 origin_timestamp({ByteArray.GetHexTimeStampString(origin_timestamp)}) 不一致"
                        };

                    start += chunk_length;
                    if (start > origin_bytes.Length - 1)
                        break;

                    // TODO: Fragment 读下级对象记录
                }

                DataModel.SetMessage($"Fragment 读取记录 {path} 成功");
            }

            return new NormalResult();
        }

        public static CreateResult UploadLargObject(int count)
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> created_accesspoints = new List<AccessPoint>();

            long object_size_unit = 512 * 1024 * 1024; // 512M

            for (int i = 0; i < 1; i++)
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

                List<string> filenames = new List<string>();

                try
                {
                    {
                        // 上载对象
                        for (int j = 0; j < Math.Min(10, count); j++)
                        {
                            long object_size = object_size_unit * (j + 1);
                            string fileName = Path.Combine(Environment.CurrentDirectory, $"temp_object_{j+1}");
                            File.Delete(fileName);
                            DataModel.SetMessage($"正在创建大文件 {fileName} size={object_size} ...");
                            CreateObjectFile(fileName, object_size);

                            filenames.Add(fileName);

                            string object_path = $"{output_path}/object/{j + 1}";

                            string progress_id = DataModel.NewProgressID();
                            DataModel.ShowProgressMessage(progress_id, $"正在上传大文件 {object_path} ...");

                            long start = 0;
                            using (var file = File.OpenRead(fileName))
                            {
                                byte[] timestamp = null;
                                while (true)
                                {
                                    long rest_length = file.Length - start;
                                    byte[] bytes = new byte[Math.Min((long)(300 * 1024), rest_length)];
                                    int read_length = file.Read(bytes, 0, bytes.Length);
                                    if (read_length == 0)
                                        break;

                                    DataModel.ShowProgressMessage(progress_id, $"正在上传大文件 {object_path} {start}-{start + read_length - 1} {StringUtil.GetPercentText(start+read_length, file.Length)}...");

                                    ret = channel.WriteRes(object_path,
                                        $"{start}-{start + read_length - 1}",
                                        file.Length,
                                        bytes,
                                        "",
                                        "content,data",
                                        timestamp,
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
                                    start += read_length;
                                    if (read_length < bytes.Length
                                        || start >= file.Length)
                                        break;

                                    timestamp = output_object_timestamp;
                                }
                            }

                            DataModel.ShowProgressMessage(progress_id, $"文件 {object_path} 上传完成");
                        }
                    }

                    // 下载，比较
                    for (int j = 0; j < filenames.Count; j++)
                    {
                        string object_path = $"{output_path}/object/{j + 1}";
                        string fileName = filenames[j];
                        string output_fileName = Path.Combine(Environment.CurrentDirectory, $"output_{j+1}");
                        File.Delete(output_fileName);
                        try
                        {
                            string progress_id = DataModel.NewProgressID();
                            DataModel.ShowProgressMessage(progress_id, $"正在下载大文件 {object_path} ...");

                            var stop = new Stop();
                            stop.OnProgressChanged += (o, e) =>
                            {
                                DataModel.ShowProgressMessage(progress_id, e.Message);
                            };
                            stop.BeginLoop();
                            try
                            {
                                var lRet = channel.GetRes(object_path,
                output_fileName,
                stop,
                out string strMetaData,
                out byte[] baOutputTimeStamp,
                out string strOutputPath,
                out strError);
                                if (lRet == -1)
                                    return new CreateResult
                                    {
                                        Value = -1,
                                        ErrorInfo = strError,
                                        CreatedPaths = created_paths,
                                        AccessPoints = created_accesspoints,
                                    };
                            }
                            finally
                            {
                                stop.EndLoop();
                            }
                            DataModel.ShowProgressMessage(progress_id, $"文件 {object_path} 下载完成");

                            // compare
                            progress_id = DataModel.NewProgressID();
                            string compare_error = CompareFiles(fileName,
                                output_fileName,
                                (offset, total_length) =>
                                {
                                    DataModel.ShowProgressMessage(progress_id, $"正在比较文件 {fileName} {output_fileName} {StringUtil.GetPercentText(offset, total_length)}...");
                                });
                            if (compare_error != null)
                                return new CreateResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"原始文件和下载文件比较发现不同: {compare_error}",
                                    CreatedPaths = created_paths,
                                    AccessPoints = created_accesspoints,
                                };

                            DataModel.ShowProgressMessage(progress_id, $"文件 {fileName} {output_fileName} 比较完成");

                            //File.Delete(fileName);
                            //File.Delete(output_fileName);
                        }
                        finally
                        {
                            if (File.Exists(output_fileName))
                                File.Delete(output_fileName);
                        }
                    }

                    DataModel.SetMessage($"上传大对象 {output_path} 成功");
                }
                finally
                {
                    foreach (var fileName in filenames)
                    {
                        if (File.Exists(fileName))
                            File.Delete(fileName);
                    }
                }
            }

            return new CreateResult
            {
                CreatedPaths = created_paths,
                AccessPoints = created_accesspoints,
            };
        }

        delegate void Delegate_showProgress(long current, long total_length);

        static string CompareFiles(string file1,
            string file2,
            Delegate_showProgress proc_showProgress)
        {
            using (var h1 = File.OpenRead(file1))
            using (var h2 = File.OpenRead(file1))
            {
                if (h1.Length != h2.Length)
                    return $"文件 {file1} 和 {file2} 尺寸不同({h1.Length} {h2.Length})";

                long rest_length = h1.Length;
                byte[] buffer1 = new byte[8 * 1024];
                byte[] buffer2 = new byte[8 * 1024];
                long offset = 0;
                while (true)
                {
                    proc_showProgress?.Invoke(offset, h1.Length);

                    int read_length = (int)Math.Min((long)buffer1.Length, h1.Length - offset);
                    var ret1 = h1.Read(buffer1, 0, read_length);
                    Debug.Assert(read_length == ret1);
                    var ret2 = h2.Read(buffer2, 0, read_length);
                    Debug.Assert(read_length == ret2);
                    for (int i = 0; i < read_length; i++)
                    {
                        if (buffer1[i] != buffer2[i])
                            return $"文件 {file1} 和 {file2} 在偏移 {offset + i} 的字节不同({(int)buffer1[i]} {(int)buffer2[i]})";
                    }

                    offset += read_length;
                    if (offset >= h1.Length)
                        break;
                }
            }

            return null;
        }

        // 创建一个指定大小的对象文件
        static NormalResult CreateObjectFile(string fileName, long object_size)
        {
            long rest = object_size;
            using (var file = File.Create(fileName))
            {
                byte[] chunk = new byte[8 * 1024];
                while (true)
                {
                    int write_length = (int)(Math.Min((long)chunk.Length, rest));
                    for (int i = 0; i < write_length; i++)
                    {
                        chunk[i] = (byte)i;
                    }

                    file.Write(chunk, 0, write_length);
                    rest -= write_length;
                    if (rest <= 0)
                        return new NormalResult();
                }
            }

            return new NormalResult();
        }

    }
}
