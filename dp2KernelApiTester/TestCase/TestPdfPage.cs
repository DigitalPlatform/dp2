using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

using static dp2KernelApiTester.TestRecord;

using DigitalPlatform;
using DigitalPlatform.rms.Client;

namespace dp2KernelApiTester
{
    public static class TestPdfPage
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
                var create_result = TestUploadAndDownload();
                if (create_result.Value == -1)
                    return create_result;
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


        public static CreateResult TestUploadAndDownload()
        {
            var channel = DataModel.GetChannel();

            List<string> created_paths = new List<string>();
            List<AccessPoint> created_accesspoints = new List<AccessPoint>();

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
                        for (int j = 0; j < 10; j++)
                        {
                            string fileName = Path.Combine(Environment.CurrentDirectory, $"test{j+1}.pdf");
                            File.Delete(fileName);
                            DataModel.SetMessage($"正在创建 PDF 文件 {fileName} ...");
                            CreatePdfFile(fileName);

                            filenames.Add(fileName);

                            string object_path = $"{output_path}/object/{j + 1}";

                            DataModel.SetMessage($"正在上传 PDF 文件 {object_path} ...");
                            var bytes = File.ReadAllBytes(fileName);
                            string strMetadata = RmsChannel.BuildMetadataXml("application/pdf",
    fileName,
    DateTime.UtcNow.ToString("u"));
                            ret = channel.WriteRes(object_path,
    $"0-{bytes.Length - 1}",
    bytes.LongLength,
    bytes,
    strMetadata,
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

                            // DataModel.SetMessage("文件 {object_path} 上传完成");
                        }
                    }

                    // 删除所有文件名为 ~output_page_ 开头的文件(上次运行遗留的文件)
                    {
                        DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
                        var fis = di.GetFiles("~output_page_*.*");
                        foreach (var f in fis)
                        {
                            File.Delete(f.FullName);
                        }
                    }

                    // 下载和显示第一个 PDF 文件的十个 Page
                    for (int j = 0; j < 10; j++)
                    {
                        string object_path = $"{output_path}/object/1/page:{j+1}";
                        string output_fileName = Path.Combine(Environment.CurrentDirectory, $"~output_page_{j + 1}");
                        File.Delete(output_fileName);
                        try
                        {
                            string progress_id = DataModel.NewProgressID();
                            DataModel.ShowProgressMessage(progress_id, $"正在下载页面文件 {object_path} ...");

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

                            // 显示图像文件
                            DataModel.ShowImage(output_fileName);
                        }
                        finally
                        {
                            //if (File.Exists(output_fileName))
                            //    File.Delete(output_fileName);
                        }
                    }

                    DataModel.SetMessage($"上传和下载 PDF 页面 {output_path} 成功");
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

        public static void CreatePdfFile(string fileName)
        {
            PdfDocument document = new PdfDocument();

            for (int i = 0; i < 10; i++)
            {
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Verdana", 40, XFontStyle.Bold);
                gfx.DrawString($"测试 Page {i + 1}", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            }
            document.Save(fileName);
        }
    }
}
