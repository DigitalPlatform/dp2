using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using Microsoft.SqlServer.Server;

namespace dp2KernelApiTester
{
    // 测试初始化和删除数据库
    public static class TestCreateDatabase
    {
        static string strDatabaseName = "__test";

        public static NormalResult TestAll(
            CancellationToken token,
            string style = null)
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

            token.ThrowIfCancellationRequested();

            // 试探性删除以前残留的数据库
            var ret = channel.DoDeleteDB(strDatabaseName, out strError);
            if (ret == -1 && channel.ErrorCode != DigitalPlatform.rms.Client.ChannelErrorCode.NotFound)
            {
                goto ERROR1;
            }

            token.ThrowIfCancellationRequested();

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

            token.ThrowIfCancellationRequested();

            ret = channel.DoInitialDB(strDatabaseName, out strError);
            if (ret == -1)
            {
                strError = $"初始化数据库时出错: {strError}";
                goto ERROR1;
            }

            if (StringUtil.IsInList("refresh_database", style))
            {
                token.ThrowIfCancellationRequested();

                var result = RefreshDatabase(true);
                if (result.Value == -1)
                {
                    DataModel.SetMessage($"RefreshDatabase() error: {result.ErrorInfo}", "error");
                    return result;
                }
            }

            if (StringUtil.IsInList("create_records", style))
            {
                token.ThrowIfCancellationRequested();

                var result = CreateRecords(100);
                if (result.Value == -1)
                {
                    DataModel.SetMessage($"CreateRecords() error: {result.ErrorInfo}", "error");
                    return result;
                }
            }

            if (StringUtil.IsInList("buildkeys", style))
            {
                token.ThrowIfCancellationRequested();

                var result = RebuildKeys();
                if (result.Value == -1)
                {
                    DataModel.SetMessage($"RebuildKeys() error: {result.ErrorInfo}", "error");
                    return result;
                }
            }

            token.ThrowIfCancellationRequested();

            // 增加或者减少检索点定义，然后重新创建检索点

            {
                // 少了当前位置和架号
                string strKeysDef_small = @"
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
    <xpath>*/uid</xpath>
    <from>uid</from>
    <table ref='uid' />
  </key>
  <table name='uid' id='14'>
    <caption lang='zh-CN'>RFID UID</caption>
    <caption lang='en'>RFID UID</caption>
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

                var change_result = ChangeConfig(strDatabaseName,
                    "keys",
                    strKeysDef_small);
                if (change_result.Value == -1)
                {
                    DataModel.SetMessage($"ChangeConfig() error: {change_result.ErrorInfo}", "error");
                    return change_result;
                }

                DataModel.SetMessage($"减少 keys 内的检索点定义成功");

                var refresh_result = RefreshDatabase(true);
                if (refresh_result.Value == -1)
                {
                    DataModel.SetMessage($"RefreshDatabase(true) error: {refresh_result.ErrorInfo}", "error");
                    return change_result;
                }

                refresh_result = RefreshDatabase(false);
                if (refresh_result.Value == -1)
                {
                    DataModel.SetMessage($"RefreshDatabase(false) error: {refresh_result.ErrorInfo}", "error");
                    return change_result;
                }
            }

            ret = channel.DoDeleteDB(strDatabaseName, out strError);
            if (ret == -1)
            {
                strError = $"删除数据库时出错: {strError}";
                goto ERROR1;
            }

            DataModel.SetMessage("创建数据库和删除数据库成功", "green");

            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"TestAll() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult RebuildKeys()
        {
            var channel = DataModel.GetChannel();

            var ret = channel.DoRefreshDB("disablekeysindex",
    strDatabaseName,
    false,
    out string strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            ret = channel.DoRefreshDB("rebuildkeysindex",
                strDatabaseName,
                false,
                out strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            DataModel.SetMessage($"刷新数据库定义成功");
            return new NormalResult();
        }

        // 修改一个已存在的配置文件
        public static NormalResult ChangeConfig(string strDatabaseName,
            string strName,
            string strConfigContent)
        {
            var channel = DataModel.GetChannel();

            string strPath = strDatabaseName + "/cfgs/" + strName;

            // 先获得时间戳
            byte[] timestamp = null;
            using (var exist_stream = new MemoryStream())
            {
                string strStyle = "content,data,metadata,timestamp,outputpath";
                long lRet = channel.GetRes(
        strPath,
        exist_stream,
        null,   // stop,
        strStyle,
        null,   // byte [] input_timestamp,
        out string strMetaData,
        out timestamp,
        out string strOutputPath,
        out string strError);
                if (lRet == -1)
                {
                    /*
                    // 配置文件不存在，怎么返回错误码的?
                    if (channel.IsNotFoundOrDamaged())
                    {
                        timestamp = null;
                        goto DO_CREATE;
                    }
                    return -1;
                    */
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }
            }

            XmlDocument temp = new XmlDocument();
            temp.LoadXml(strConfigContent);
            using (Stream new_stream = new MemoryStream())
            { 
                temp.Save(new_stream);

                new_stream.Seek(0, SeekOrigin.Begin);

                // 在服务器端创建对象
                // parameters:
                //      strStyle    风格。当创建目录的时候，为"createdir"，否则为空
                // return:
                //		-1	错误
                //		1	已经存在同名对象
                //		0	正常返回
                int nRet = DatabaseUtility.NewServerSideObject(
                    channel,
                    strPath,
                    "",
                    new_stream,
                    timestamp,
                    out string strError);
                if (nRet == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }
                if (nRet == 1)
                {
                    strError = "NewServerSideObject()发现已经存在同名对象: " + strError;
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }

                return new NormalResult();
            }
        }

        // 刷新数据库定义
        public static NormalResult RefreshDatabase(bool bClearAllKeysTable)
        {
            var channel = DataModel.GetChannel();
            var ret = channel.DoRefreshDB("begin",
                strDatabaseName,
                bClearAllKeysTable,
                out string strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            ret = channel.DoRefreshDB("end",
    strDatabaseName,
    bClearAllKeysTable,
    out strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            DataModel.SetMessage($"刷新数据库定义成功(bClearAllKeysTable={bClearAllKeysTable})");
            return new NormalResult();
        }

        // 创建若干条数据库记录
        public static NormalResult CreateRecords(int count)
        {
            var channel = DataModel.GetChannel();

            for (int i = 0; i < count; i++)
            {
                string path = $"{strDatabaseName}/?";

                string xml = @"<root>
<barcode>{barcode}</barcode>
</root>".Replace("{barcode}", (i + 1).ToString().PadLeft(10, '0'));
                var bytes = Encoding.UTF8.GetBytes(xml);

                var ret = channel.DoSaveTextRes(path,
                    xml, // strMetadata,
                    false,
                    "",
                    null,
                    out byte[] output_timestamp,
                    out string output_path,
                    out string strError);
                if (ret == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                DataModel.SetMessage($"创建记录 {output_path} 成功");

            }

            return new NormalResult();
        }

        /*
        public static NormalResult PrepareEnvironment()
        {

        }

        public static NormalResult Finish()
        {

        }
        */
    }
}
