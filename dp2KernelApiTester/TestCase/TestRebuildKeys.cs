﻿using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2KernelApiTester
{
    // 测试重建检索点有关的功能
    // 中途修改 keys 以后，重建检索点，然后检查新的检索点是否可以检索命中
    public static class TestRebuildKeys
    {
        static string strDatabaseName = "__test";

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
                var result = RefreshNotExist(token);
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

        // 刷新一条并不存在的记录的检索点
        static public NormalResult RefreshNotExist(CancellationToken token)
        {
            var channel = DataModel.GetChannel();

            string path = strDatabaseName + "/100";
            var ret = channel.DoRebuildResKeys(path,
    "forcedeleteoldkeys",
    out string output_path,
    out string strError);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            DataModel.SetMessage($"强制刷新记录 {path} 的检索点成功", "green");

            return new NormalResult();
        }
    }
}
